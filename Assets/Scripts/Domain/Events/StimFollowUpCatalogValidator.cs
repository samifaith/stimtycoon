using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StimTycoon.Events
{
    public sealed class StimFollowUpCatalogValidationResult
    {
        public bool isValid = true;
        public int followUpCount;
        public readonly List<string> errors = new List<string>();
    }

    public static class StimFollowUpCatalogValidator
    {
        private static readonly Regex CancellationRule = new Regex(
            "^[a-z][a-z0-9_]*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static StimFollowUpCatalogValidationResult Validate(IReadOnlyList<StimEvent> events)
        {
            var result = new StimFollowUpCatalogValidationResult();
            if (events == null) return Fail(result, "Authored event catalog is null.");
            var byId = new Dictionary<string, StimEvent>(StringComparer.Ordinal);
            foreach (var evt in events)
            {
                if (evt == null || string.IsNullOrWhiteSpace(evt.id))
                {
                    AddError(result, "Authored event catalog contains a null event or empty ID.");
                    continue;
                }
                if (byId.ContainsKey(evt.id)) AddError(result, $"Duplicate event ID {evt.id}.");
                else byId.Add(evt.id, evt);
            }

            foreach (var source in events)
            {
                if (source?.choices == null) continue;
                foreach (var choice in source.choices)
                foreach (var outcome in choice?.outcomes ?? new List<Outcome>())
                foreach (var followUp in outcome?.followUps ?? new List<ScheduledEventRef>())
                {
                    result.followUpCount++;
                    var owner = $"{source.id}/{choice?.id}/{outcome?.id}";
                    if (followUp == null)
                    {
                        AddError(result, $"{owner} contains a null follow-up.");
                        continue;
                    }
                    if (!byId.TryGetValue(followUp.eventId ?? string.Empty, out var target))
                    {
                        AddError(result, $"{owner} references missing follow-up {followUp.eventId}.");
                        continue;
                    }
                    if (followUp.probability <= 0f || followUp.probability > 1f ||
                        float.IsNaN(followUp.probability) || float.IsInfinity(followUp.probability))
                        AddError(result, $"{owner} has invalid follow-up probability {followUp.probability}.");
                    if (followUp.minYearsFromNow < 0 || followUp.maxYearsFromNow < followUp.minYearsFromNow)
                        AddError(result, $"{owner} has invalid follow-up delay window.");
                    if (string.IsNullOrWhiteSpace(followUp.cancellationRule) ||
                        !CancellationRule.IsMatch(followUp.cancellationRule))
                        AddError(result, $"{owner} has invalid cancellation rule {followUp.cancellationRule}.");
                    if (!CanAgeWindowsIntersect(source, target, followUp))
                        AddError(result,
                            $"{owner} cannot reach {target.id}: source age {Range(source)}, delay " +
                            $"{followUp.minYearsFromNow}–{followUp.maxYearsFromNow}, target age {Range(target)}.");
                }
            }
            return result;
        }

        private static bool CanAgeWindowsIntersect(
            StimEvent source, StimEvent target, ScheduledEventRef followUp)
        {
            if (source.ageRange == null || target.ageRange == null ||
                followUp.minYearsFromNow < 0 || followUp.maxYearsFromNow < followUp.minYearsFromNow)
                return false;
            var earliestPossible = (long)source.ageRange.minAge + followUp.minYearsFromNow;
            var latestPossible = (long)source.ageRange.maxAge + followUp.maxYearsFromNow;
            return earliestPossible <= target.ageRange.maxAge && latestPossible >= target.ageRange.minAge;
        }

        private static string Range(StimEvent evt) => evt.ageRange == null
            ? "missing"
            : $"{evt.ageRange.minAge}–{evt.ageRange.maxAge}";

        private static StimFollowUpCatalogValidationResult Fail(
            StimFollowUpCatalogValidationResult result, string error)
        {
            AddError(result, error);
            return result;
        }

        private static void AddError(StimFollowUpCatalogValidationResult result, string error)
        {
            result.isValid = false;
            result.errors.Add(error);
        }
    }
}
