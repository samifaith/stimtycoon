using System;
using System.Collections.Generic;
using UnityEngine;

namespace StimTycoon.Saves
{
    public sealed class StimSaveMigrationReport
    {
        public int sourceVersion;
        public int targetVersion;
        public bool changed;
        public readonly List<string> changes = new List<string>();
    }

    /// <summary>
    /// Forward-only entry point for save upgrades. Version 1 currently performs
    /// additive normalization so saves created before newer v1 fields still load.
    /// </summary>
    public static class StimSaveMigrator
    {
        public static bool TryMigrate(
            string serializedSave,
            out StimSaveEnvelope save,
            out StimSaveMigrationReport report,
            out string error)
        {
            save = null;
            report = new StimSaveMigrationReport();
            if (string.IsNullOrWhiteSpace(serializedSave))
            {
                error = "Serialized save is required.";
                return false;
            }

            try
            {
                save = JsonUtility.FromJson<StimSaveEnvelope>(serializedSave);
            }
            catch (Exception exception)
            {
                error = $"Serialized save is not valid JSON: {exception.Message}";
                return false;
            }

            if (save == null)
            {
                error = "Serialized save produced a null envelope.";
                return false;
            }

            report.sourceVersion = save.saveFormatVersion;
            report.targetVersion = StimSaveSchema.SupportedSaveFormatVersion;
            if (save.saveFormatVersion != StimSaveSchema.SupportedSaveFormatVersion)
            {
                error = $"No migration path exists from save format {save.saveFormatVersion}.";
                return false;
            }

            NormalizeV1(save, serializedSave, report);
            if (report.changed && save.integrity != null)
            {
                save.integrity.payloadHash = string.Empty;
            }

            error = string.Empty;
            return true;
        }

        private static void NormalizeV1(
            StimSaveEnvelope save,
            string serializedSave,
            StimSaveMigrationReport report)
        {
            if (save.state == null)
            {
                return;
            }

            if (save.state.character != null)
            {
                if (!serializedSave.Contains("\"looks\""))
                {
                    save.state.character.looks = 50;
                    Record(report, "state.character.looks=50");
                }
                if (!serializedSave.Contains("\"luck\""))
                {
                    save.state.character.luck = 50;
                    Record(report, "state.character.luck=50");
                }
            }

            if (save.state.calendar == null)
            {
                save.state.calendar = new StimCalendarState();
                Record(report, "state.calendar created");
            }
            if (save.state.finances == null)
            {
                save.state.finances = new StimFinancesState();
                Record(report, "state.finances created");
            }
            if (save.state.career == null)
            {
                save.state.career = new StimCareerState();
                Record(report, "state.career created");
            }
            if (save.state.education == null)
            {
                save.state.education = new StimEducationState();
                Record(report, "state.education created");
            }
            if (save.state.skills == null)
            {
                save.state.skills = new List<StimSkillState>();
                Record(report, "state.skills created");
            }
            if (save.state.relationships == null)
            {
                save.state.relationships = new List<StimRelationshipState>();
                Record(report, "state.relationships created");
            }
            if (save.state.statuses == null)
            {
                save.state.statuses = new List<StimStatusState>();
                Record(report, "state.statuses created");
            }
            if (save.state.lifeFeed == null)
            {
                save.state.lifeFeed = new List<StimLifeFeedEntry>();
                Record(report, "state.lifeFeed created");
            }
            if (save.state.eventHistory == null)
            {
                save.state.eventHistory = new List<StimEventHistoryEntry>();
                Record(report, "state.eventHistory created");
            }
            if (save.state.scheduledEvents == null)
            {
                save.state.scheduledEvents = new List<StimScheduledEventRecord>();
                Record(report, "state.scheduledEvents created");
            }
        }

        private static void Record(StimSaveMigrationReport report, string change)
        {
            report.changed = true;
            report.changes.Add(change);
        }
    }
}
