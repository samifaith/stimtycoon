using System;

namespace StimTycoon.Runtime
{
    public enum CoreStatBand
    {
        Critical,
        Low,
        Stable,
        Strong,
        Exceptional,
        Peak
    }

    /// <summary>
    /// Shared implementation contract for the authored progression baseline.
    /// Changes here must remain synchronized with CONTENT_PROGRESSION_STANDARDS.md.
    /// </summary>
    public static class ProgressionStandards
    {
        public const int LowCoreStatStartsAt = 20;
        public const int StableCoreStatStartsAt = 40;
        public const int StrongCoreStatStartsAt = 60;
        public const int ExceptionalCoreStatStartsAt = 80;
        public const int PeakCoreStatStartsAt = 95;
        public const int MaximumMainPathCoreStatRequirement = StrongCoreStatStartsAt;

        public const int CertificateQualificationExperience = 50;
        public const int DiplomaQualificationExperience = 125;
        public const int AdvancedQualificationExperience = 250;

        public const int RoutineSkillExperienceMinimum = 5;
        public const int RoutineSkillExperienceMaximum = 15;
        public const int CommittedSkillExperienceMinimum = 15;
        public const int CommittedSkillExperienceMaximum = 35;
        public const int MajorSkillExperienceMinimum = 25;
        public const int MajorSkillExperienceMaximum = 75;

        public const int IndexInvestmentMinimumAge = 18;
        public const int IndexInvestmentMinimumSmarts = StableCoreStatStartsAt;
        public const long IndexInvestmentMinimumEmergencySavingsMinorUnits = 50000L;

        public const int FirstCareerPromotionProgress = 25;
        public const int SecondCareerPromotionProgress = 50;
        public const int ThirdCareerPromotionProgress = 75;
        public const int BusinessUpgradeProgressPerLevel = 25;

        public const long DailyGoalRewardMinorUnits = 1000L;
        public const long MainGoalRewardMinorUnits = 50000L;
        public const long LifeGoalRewardMinorUnits = 250000L;
        public const long RoutineRewardMinimumMinorUnits = 1000L;
        public const long RoutineRewardMaximumMinorUnits = 5000L;
        public const long MainRewardMinimumMinorUnits = 25000L;
        public const long MainRewardMaximumMinorUnits = 100000L;
        public const long LifeRewardMinimumMinorUnits = 100000L;
        public const long LifeRewardMaximumMinorUnits = 500000L;

        public static CoreStatBand GetCoreStatBand(int value)
        {
            if (value < LowCoreStatStartsAt) return CoreStatBand.Critical;
            if (value < StableCoreStatStartsAt) return CoreStatBand.Low;
            if (value < StrongCoreStatStartsAt) return CoreStatBand.Stable;
            if (value < ExceptionalCoreStatStartsAt) return CoreStatBand.Strong;
            if (value < PeakCoreStatStartsAt) return CoreStatBand.Exceptional;
            return CoreStatBand.Peak;
        }

        public static int GetSkillExperienceForLevel(int level)
        {
            if (level <= 1) return 0;
            return (int)Math.Min(int.MaxValue, 25L * (level - 1) * level);
        }

        public static int GetSkillLevel(int experience)
        {
            experience = Math.Max(0, experience);
            var level = 1;
            while (25L * level * (level + 1) <= experience) level++;
            return level;
        }

        public static int GetBusinessUpgradeProgressRequired(int currentLevel)
        {
            return currentLevel > 0 ? currentLevel * BusinessUpgradeProgressPerLevel : 0;
        }

        public static bool IsGoalRewardWithinDocumentedBand(string category, long rewardMinorUnits)
        {
            if (string.Equals(category, "daily", StringComparison.Ordinal))
                return rewardMinorUnits >= RoutineRewardMinimumMinorUnits &&
                       rewardMinorUnits <= RoutineRewardMaximumMinorUnits;
            if (string.Equals(category, "main", StringComparison.Ordinal))
                return rewardMinorUnits >= MainRewardMinimumMinorUnits &&
                       rewardMinorUnits <= MainRewardMaximumMinorUnits;
            if (string.Equals(category, "life", StringComparison.Ordinal))
                return rewardMinorUnits >= LifeRewardMinimumMinorUnits &&
                       rewardMinorUnits <= LifeRewardMaximumMinorUnits;
            return false;
        }
    }
}
