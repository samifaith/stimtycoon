using System;
using System.Collections.Generic;
using StimTycoon.Saves;

namespace StimTycoon.Runtime
{
    public sealed class StimCareerRoleDefinition
    {
        public string title;
        public long annualSalaryMinorUnits;
        public int promotionProgressRequired;
    }

    public sealed class StimCareerIndustryDefinition
    {
        public string industryId;
        public string displayName;
        public string employerId;
        public string requiredStudyTrack;
        public int requiredQualificationExperience;
        public string requiredSkillId;
        public int requiredSkillLevel;
        public IReadOnlyList<StimCareerRoleDefinition> roles;
    }

    public static class StimCareerCatalog
    {
        public const string FinanceIndustryId = "finance";
        public const string HealthcareIndustryId = "healthcare";
        public const string SkilledTradesIndustryId = "skilled_trades";

        private static readonly IReadOnlyList<StimCareerIndustryDefinition> Industries =
            new List<StimCareerIndustryDefinition>
            {
                Create(FinanceIndustryId, "Finance", "stim_financial_group", string.Empty, 0,
                    "professional", 1,
                    Role("Junior Associate", 4000000, StimProgressionStandards.FirstCareerPromotionProgress),
                    Role("Associate", 5500000, StimProgressionStandards.SecondCareerPromotionProgress),
                    Role("Senior Associate", 7500000, StimProgressionStandards.ThirdCareerPromotionProgress),
                    Role("Manager", 10000000, 0)),
                Create(HealthcareIndustryId, "Healthcare", "harbor_health_network", "academic", 125,
                    "learning", 2,
                    Role("Care Assistant", 3800000, 30), Role("Clinical Coordinator", 5800000, 55),
                    Role("Senior Coordinator", 7800000, 80), Role("Health Services Manager", 10500000, 0)),
                Create(SkilledTradesIndustryId, "Skilled Trades", "community_build_works", "vocational", 50,
                    "fitness", 1,
                    Role("Apprentice Technician", 3600000, 20), Role("Technician", 5200000, 45),
                    Role("Senior Technician", 7000000, 70), Role("Operations Foreperson", 9000000, 0))
            };

        public static IReadOnlyList<StimCareerIndustryDefinition> GetIndustries() => Industries;

        public static bool TryGetIndustry(string industryId, out StimCareerIndustryDefinition industry)
        {
            industry = null;
            if (string.IsNullOrEmpty(industryId)) return false;
            foreach (var candidate in Industries)
            {
                if (!string.Equals(candidate.industryId, industryId, StringComparison.Ordinal)) continue;
                industry = candidate;
                return true;
            }
            return false;
        }

        public static bool TryGetNextRole(
            string industryId, string roleTitle, out StimCareerRoleDefinition nextRole, out int progressRequired)
        {
            nextRole = null;
            progressRequired = 0;
            if (!TryGetIndustry(industryId, out var industry)) return false;
            for (var index = 0; index < industry.roles.Count - 1; index++)
            {
                if (industry.roles[index].title != roleTitle) continue;
                nextRole = industry.roles[index + 1];
                progressRequired = industry.roles[index].promotionProgressRequired;
                return true;
            }
            return false;
        }

        public static bool TryGetApplicationRequirement(
            StimGameState state, string industryId, out string requirement)
        {
            if (!TryGetIndustry(industryId, out var industry))
            {
                requirement = $"Career industry {industryId} is not available.";
                return false;
            }
            var education = state?.education;
            if (!string.IsNullOrEmpty(industry.requiredStudyTrack) &&
                education?.studyTrack != industry.requiredStudyTrack)
            {
                requirement = $"{industry.displayName} requires the {ToDisplay(industry.requiredStudyTrack)} study track.";
                return false;
            }
            if ((education?.qualificationExperience ?? 0) < industry.requiredQualificationExperience)
            {
                requirement = $"{industry.displayName} requires {industry.requiredQualificationExperience} qualification XP.";
                return false;
            }
            var skillLevel = StimGameSessionService.GetSkillLevel(
                StimGameSessionService.GetSkillExperience(state?.skills, industry.requiredSkillId));
            if (skillLevel < industry.requiredSkillLevel)
            {
                requirement = $"{industry.displayName} requires {ToDisplay(industry.requiredSkillId)} Level {industry.requiredSkillLevel}.";
                return false;
            }
            requirement = string.Empty;
            return true;
        }

        private static StimCareerIndustryDefinition Create(
            string id, string name, string employer, string track, int xp, string skill, int level,
            params StimCareerRoleDefinition[] roles) => new StimCareerIndustryDefinition
        {
            industryId = id, displayName = name, employerId = employer, requiredStudyTrack = track,
            requiredQualificationExperience = xp, requiredSkillId = skill, requiredSkillLevel = level,
            roles = roles
        };

        private static StimCareerRoleDefinition Role(string title, long salary, int progress) =>
            new StimCareerRoleDefinition
                { title = title, annualSalaryMinorUnits = salary, promotionProgressRequired = progress };

        private static string ToDisplay(string value) =>
            string.IsNullOrEmpty(value) ? string.Empty : value.Replace('_', ' ');
    }
}
