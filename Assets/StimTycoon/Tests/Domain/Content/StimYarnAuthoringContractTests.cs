using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using StimTycoon.Events;
using StimTycoon.Runtime;

namespace StimTycoon.Tests.Domain.Content
{
    [Category("ContentContract")]
    public sealed class StimYarnAuthoringContractTests
    {
        private const string EventsPath = "Assets/StimTycoon/Dialogue/Events";
        private static readonly Regex NodeTitle = new Regex(
            @"(?m)^title:\s*(?<id>\S+)\s*$", RegexOptions.Compiled);
        private static readonly Regex ChoiceTag = new Regex(
            @"#choice:(?<id>[a-z0-9_]+)", RegexOptions.Compiled);
        private static readonly Regex ResolveCommand = new Regex(
            @"<<stim_resolve_choice\s+""(?<event>[a-z0-9_]+)""\s+""(?<choice>[a-z0-9_]+)""\s*>>",
            RegexOptions.Compiled);

        [Test]
        public void EveryYarnNodeAndResolveEventIdIsUnique()
        {
            var nodeOwners = new Dictionary<string, string>(StringComparer.Ordinal);
            var eventOwners = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var node in ReadNodes())
            {
                AssertUnique(nodeOwners, node.nodeId, node.owner, "Yarn node title");
                AssertUnique(eventOwners, node.eventId, node.owner, "resolved event id");
            }
        }

        [Test]
        public void EveryTaggedChoiceResolvesExactlyOnceWithinItsNode()
        {
            foreach (var node in ReadNodes())
            {
                CollectionAssert.AreEquivalent(node.choiceTags, node.resolvedChoices,
                    $"{node.owner} must keep #choice tags and stim_resolve_choice commands in exact parity.");
                Assert.That(node.choiceTags, Has.Count.GreaterThanOrEqualTo(2),
                    $"{node.owner} must offer at least two authored choices.");
                Assert.That(node.choiceTags.Distinct(StringComparer.Ordinal).Count(),
                    Is.EqualTo(node.choiceTags.Count), $"{node.owner} contains a duplicate #choice id.");
            }
        }

        [Test]
        public void EveryNodeResolvesOneLocalizationSafeEventId()
        {
            foreach (var node in ReadNodes())
            {
                Assert.That(node.eventId, Does.Match("^[a-z0-9_]+$"),
                    $"{node.owner} resolves an event id that is not localization-key safe.");
                Assert.That(node.resolvedEventIds, Has.Count.EqualTo(1),
                    $"{node.owner} must not resolve choices against multiple event definitions.");
            }
        }

        [Test]
        public void EveryYarnResolveHasMatchingValidatedCatalogEventAndChoice()
        {
            var catalog = RepresentativeStimEvents.CreateLaunchAlphaCatalog()
                .Concat(StagedStimEventCatalog.CreateChildhoodBatch())
                .Concat(StagedStimEventCatalog.CreateSchoolBatch())
                .Concat(StagedStimEventCatalog.CreateCareerBatch())
                .Concat(StagedStimEventCatalog.CreateHealthBatch())
                .Concat(StagedStimEventCatalog.CreateMoneyBatch())
                .ToDictionary(evt => evt.id, StringComparer.Ordinal);

            foreach (var node in ReadNodes())
            {
                Assert.That(catalog.TryGetValue(node.eventId, out var evt), Is.True,
                    $"{node.owner} has no matching domain event definition.");
                var validation = StimEventValidator.ValidateProductionEvent(evt);
                Assert.That(validation.isValid, Is.True,
                    StimEventValidator.GetValidationSummary(validation, evt.id));
                CollectionAssert.AreEquivalent(node.choiceTags, evt.choices.Select(choice => choice.id),
                    $"{node.owner} must match its catalog choice IDs exactly.");
            }
        }

        [Test]
        public void StagedCatalog_HasExpectedBreadthAndUniqueIds()
        {
            var events = StagedStimEventCatalog.CreateAllStagedEvents();

            Assert.That(events, Has.Count.EqualTo(100));
            Assert.That(events.Select(evt => evt.id).Distinct(StringComparer.Ordinal).Count(),
                Is.EqualTo(100));
            Assert.That(events.Count(evt => evt.category == EventCategory.Childhood), Is.EqualTo(20));
            Assert.That(events.Count(evt => evt.category == EventCategory.School), Is.EqualTo(20));
            Assert.That(events.Count(evt => evt.category == EventCategory.Career), Is.EqualTo(20));
            Assert.That(events.Count(evt => evt.category == EventCategory.Health), Is.EqualTo(20));
            Assert.That(events.Count(evt => evt.category == EventCategory.Money), Is.EqualTo(20));
        }

        [Test]
        public void StagedCatalog_EnforcesAgeCashRepeatAndFollowUpSafety()
        {
            foreach (var evt in StagedStimEventCatalog.CreateAllStagedEvents())
            {
                Assert.That(evt.ageRange, Is.Not.Null, $"{evt.id} requires an age range.");
                Assert.That(evt.ageRange.minAge, Is.GreaterThanOrEqualTo(0));
                Assert.That(evt.ageRange.maxAge, Is.GreaterThanOrEqualTo(evt.ageRange.minAge));
                Assert.That(evt.repeatPolicy, Is.EqualTo(RepeatPolicy.Never),
                    $"{evt.id} must remain once-only during staged review.");
                Assert.That(evt.cooldownYears, Is.GreaterThan(0));

                foreach (var outcome in evt.choices.SelectMany(choice => choice.outcomes))
                {
                    foreach (var followUp in outcome.followUps)
                    {
                        Assert.That(followUp.eventId, Is.Not.Empty);
                        Assert.That(followUp.probability, Is.InRange(0.0001f, 1f));
                        Assert.That(followUp.maxYearsFromNow, Is.GreaterThanOrEqualTo(followUp.minYearsFromNow));
                        Assert.That(followUp.cancellationRule, Is.Not.Empty);
                    }

                    if (outcome.effects.Any(effect =>
                            effect.type == EffectType.CashDelta && effect.value < 0f))
                        Assert.That(evt.ageRange.minAge, Is.GreaterThanOrEqualTo(18),
                            $"{evt.id} cannot charge cash before adult financial agency.");
                }
            }
        }

        [Test]
        public void ChildhoodStagedBatch_HasValidatedCatalogAndExactChoiceParity()
        {
            var nodes = ReadNodes()
                .Where(node => node.owner.Contains("ChildhoodBatch20.yarn"))
                .OrderBy(node => node.eventId, StringComparer.Ordinal)
                .ToList();
            var events = StagedStimEventCatalog.CreateChildhoodBatch()
                .OrderBy(evt => evt.id, StringComparer.Ordinal)
                .ToList();

            Assert.That(nodes, Has.Count.EqualTo(20));
            Assert.That(events, Has.Count.EqualTo(nodes.Count));
            CollectionAssert.AreEqual(nodes.Select(node => node.eventId), events.Select(evt => evt.id));

            for (var index = 0; index < events.Count; index++)
            {
                var evt = events[index];
                var validation = StimEventValidator.ValidateProductionEvent(evt);
                Assert.That(validation.isValid, Is.True,
                    StimEventValidator.GetValidationSummary(validation, evt.id));
                CollectionAssert.AreEquivalent(
                    nodes[index].choiceTags,
                    evt.choices.Select(choice => choice.id),
                    $"{evt.id} must match its Yarn #choice contract before registration.");
            }
        }

        [Test]
        public void ChildhoodStagedBatch_RemainsOutsideRandomLaunchCatalog()
        {
            var launchIds = new HashSet<string>(
                RepresentativeStimEvents.CreateLaunchAlphaCatalog().Select(evt => evt.id),
                StringComparer.Ordinal);

            Assert.That(StagedStimEventCatalog.CreateChildhoodBatch()
                .Any(evt => launchIds.Contains(evt.id)), Is.False,
                "Staged events must remain disabled until pacing and editorial review approves registration.");
        }

        [Test]
        public void SchoolStagedBatch_HasValidatedCatalogAndExactChoiceParity()
        {
            var nodes = ReadNodes()
                .Where(node => node.owner.Contains("SchoolBatch20.yarn"))
                .OrderBy(node => node.eventId, StringComparer.Ordinal)
                .ToList();
            var events = StagedStimEventCatalog.CreateSchoolBatch()
                .OrderBy(evt => evt.id, StringComparer.Ordinal)
                .ToList();

            Assert.That(nodes, Has.Count.EqualTo(20));
            Assert.That(events, Has.Count.EqualTo(nodes.Count));
            CollectionAssert.AreEqual(nodes.Select(node => node.eventId), events.Select(evt => evt.id));

            for (var index = 0; index < events.Count; index++)
            {
                var evt = events[index];
                var validation = StimEventValidator.ValidateProductionEvent(evt);
                Assert.That(validation.isValid, Is.True,
                    StimEventValidator.GetValidationSummary(validation, evt.id));
                CollectionAssert.AreEquivalent(
                    nodes[index].choiceTags,
                    evt.choices.Select(choice => choice.id),
                    $"{evt.id} must match its Yarn #choice contract before registration.");
            }
        }

        [Test]
        public void SchoolStagedBatch_RemainsOutsideRandomLaunchCatalog()
        {
            var launchIds = new HashSet<string>(
                RepresentativeStimEvents.CreateLaunchAlphaCatalog().Select(evt => evt.id),
                StringComparer.Ordinal);

            Assert.That(StagedStimEventCatalog.CreateSchoolBatch()
                .Any(evt => launchIds.Contains(evt.id)), Is.False,
                "Staged events must remain disabled until pacing and editorial review approves registration.");
        }

        [Test]
        public void CareerStagedBatch_HasValidatedCatalogAndExactChoiceParity()
        {
            var nodes = ReadNodes()
                .Where(node => node.owner.Contains("CareerBatch20.yarn"))
                .OrderBy(node => node.eventId, StringComparer.Ordinal)
                .ToList();
            var events = StagedStimEventCatalog.CreateCareerBatch()
                .OrderBy(evt => evt.id, StringComparer.Ordinal)
                .ToList();

            Assert.That(nodes, Has.Count.EqualTo(20));
            Assert.That(events, Has.Count.EqualTo(nodes.Count));
            CollectionAssert.AreEqual(nodes.Select(node => node.eventId), events.Select(evt => evt.id));

            for (var index = 0; index < events.Count; index++)
            {
                var evt = events[index];
                var validation = StimEventValidator.ValidateProductionEvent(evt);
                Assert.That(validation.isValid, Is.True,
                    StimEventValidator.GetValidationSummary(validation, evt.id));
                CollectionAssert.AreEquivalent(
                    nodes[index].choiceTags,
                    evt.choices.Select(choice => choice.id),
                    $"{evt.id} must match its Yarn #choice contract before registration.");
            }
        }

        [Test]
        public void CareerStagedBatch_RemainsOutsideRandomLaunchCatalog()
        {
            var launchIds = new HashSet<string>(
                RepresentativeStimEvents.CreateLaunchAlphaCatalog().Select(evt => evt.id),
                StringComparer.Ordinal);

            Assert.That(StagedStimEventCatalog.CreateCareerBatch()
                .Any(evt => launchIds.Contains(evt.id)), Is.False,
                "Staged events must remain disabled until pacing and editorial review approves registration.");
        }

        [Test]
        public void HealthStagedBatch_HasValidatedCatalogAndExactChoiceParity()
        {
            var nodes = ReadNodes()
                .Where(node => node.owner.Contains("HealthBatch20.yarn"))
                .OrderBy(node => node.eventId, StringComparer.Ordinal)
                .ToList();
            var events = StagedStimEventCatalog.CreateHealthBatch()
                .OrderBy(evt => evt.id, StringComparer.Ordinal)
                .ToList();

            Assert.That(nodes, Has.Count.EqualTo(20));
            Assert.That(events, Has.Count.EqualTo(nodes.Count));
            CollectionAssert.AreEqual(nodes.Select(node => node.eventId), events.Select(evt => evt.id));

            for (var index = 0; index < events.Count; index++)
            {
                var evt = events[index];
                var validation = StimEventValidator.ValidateProductionEvent(evt);
                Assert.That(validation.isValid, Is.True,
                    StimEventValidator.GetValidationSummary(validation, evt.id));
                CollectionAssert.AreEquivalent(
                    nodes[index].choiceTags,
                    evt.choices.Select(choice => choice.id),
                    $"{evt.id} must match its Yarn #choice contract before registration.");
            }
        }

        [Test]
        public void HealthStagedBatch_RemainsOutsideRandomLaunchCatalog()
        {
            var launchIds = new HashSet<string>(
                RepresentativeStimEvents.CreateLaunchAlphaCatalog().Select(evt => evt.id),
                StringComparer.Ordinal);

            Assert.That(StagedStimEventCatalog.CreateHealthBatch()
                .Any(evt => launchIds.Contains(evt.id)), Is.False,
                "Staged events must remain disabled until pacing and editorial review approves registration.");
        }

        [Test]
        public void MoneyStagedBatch_HasValidatedCatalogAndExactChoiceParity()
        {
            var nodes = ReadNodes()
                .Where(node => node.owner.Contains("MoneyBatch20.yarn"))
                .OrderBy(node => node.eventId, StringComparer.Ordinal)
                .ToList();
            var events = StagedStimEventCatalog.CreateMoneyBatch()
                .OrderBy(evt => evt.id, StringComparer.Ordinal)
                .ToList();

            Assert.That(nodes, Has.Count.EqualTo(20));
            Assert.That(events, Has.Count.EqualTo(nodes.Count));
            CollectionAssert.AreEqual(nodes.Select(node => node.eventId), events.Select(evt => evt.id));

            for (var index = 0; index < events.Count; index++)
            {
                var evt = events[index];
                var validation = StimEventValidator.ValidateProductionEvent(evt);
                Assert.That(validation.isValid, Is.True,
                    StimEventValidator.GetValidationSummary(validation, evt.id));
                CollectionAssert.AreEquivalent(
                    nodes[index].choiceTags,
                    evt.choices.Select(choice => choice.id),
                    $"{evt.id} must match its Yarn #choice contract before registration.");

                var chargesCash = evt.choices.SelectMany(choice => choice.outcomes)
                    .SelectMany(outcome => outcome.effects)
                    .Any(effect => effect.type == EffectType.CashDelta && effect.value < 0f);
                if (chargesCash)
                    Assert.That(evt.ageRange.minAge, Is.GreaterThanOrEqualTo(18),
                        $"{evt.id} cannot charge cash before adult financial agency.");
            }
        }

        [Test]
        public void MoneyStagedBatch_RemainsOutsideRandomLaunchCatalog()
        {
            var launchIds = new HashSet<string>(
                RepresentativeStimEvents.CreateLaunchAlphaCatalog().Select(evt => evt.id),
                StringComparer.Ordinal);

            Assert.That(StagedStimEventCatalog.CreateMoneyBatch()
                .Any(evt => launchIds.Contains(evt.id)), Is.False,
                "Staged events must remain disabled until pacing and editorial review approves registration.");
        }

        private static IReadOnlyList<YarnNodeContract> ReadNodes()
        {
            Assert.That(Directory.Exists(EventsPath), Is.True,
                $"Yarn event directory was not found at {EventsPath}.");

            var nodes = new List<YarnNodeContract>();
            foreach (var path in Directory.GetFiles(EventsPath, "*.yarn", SearchOption.AllDirectories)
                         .OrderBy(value => value, StringComparer.Ordinal))
            {
                var source = File.ReadAllText(path);
                var blocks = Regex.Split(source, @"(?m)^===\s*$");
                for (var index = 0; index < blocks.Length; index++)
                {
                    var block = blocks[index];
                    if (string.IsNullOrWhiteSpace(block)) continue;
                    var title = NodeTitle.Match(block);
                    Assert.That(title.Success, Is.True, $"{path} block {index + 1} has no title.");

                    var commands = ResolveCommand.Matches(block).Cast<Match>().ToList();
                    Assert.That(commands, Is.Not.Empty,
                        $"{path} node {title.Groups["id"].Value} has no stim_resolve_choice command.");
                    var eventIds = commands.Select(match => match.Groups["event"].Value)
                        .Distinct(StringComparer.Ordinal).ToList();
                    nodes.Add(new YarnNodeContract(
                        $"{path}::{title.Groups["id"].Value}",
                        title.Groups["id"].Value,
                        eventIds,
                        ChoiceTag.Matches(block).Cast<Match>()
                            .Select(match => match.Groups["id"].Value).ToList(),
                        commands.Select(match => match.Groups["choice"].Value).ToList()));
                }
            }

            Assert.That(nodes, Is.Not.Empty, "No authored Yarn event nodes were discovered.");
            return nodes;
        }

        private static void AssertUnique(
            IDictionary<string, string> owners,
            string id,
            string owner,
            string label)
        {
            Assert.That(owners.TryGetValue(id, out var existingOwner), Is.False,
                $"{label} '{id}' is owned by both {existingOwner} and {owner}.");
            owners.Add(id, owner);
        }

        private sealed class YarnNodeContract
        {
            public YarnNodeContract(
                string owner,
                string nodeId,
                IReadOnlyList<string> resolvedEventIds,
                IReadOnlyList<string> choiceTags,
                IReadOnlyList<string> resolvedChoices)
            {
                this.owner = owner;
                this.nodeId = nodeId;
                this.resolvedEventIds = resolvedEventIds;
                this.choiceTags = choiceTags;
                this.resolvedChoices = resolvedChoices;
            }

            public string owner { get; }
            public string nodeId { get; }
            public string eventId => resolvedEventIds[0];
            public IReadOnlyList<string> resolvedEventIds { get; }
            public IReadOnlyList<string> choiceTags { get; }
            public IReadOnlyList<string> resolvedChoices { get; }
        }
    }
}
