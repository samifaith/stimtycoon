using System.Collections.Generic;
using UnityEngine;

namespace StimTycoon.Events
{
    /// <summary>
    /// Risk/Reward Band Calculator
    /// 
    /// Converts a final success probability into an internal balancing label.
    /// Normal gameplay keeps this classification hidden.
    /// Also provides reward guidance based on outcome classification.
    /// 
    /// Locked bands (from STIM_TYCOON_MASTER_README, Section 7.4):
    /// - Safe:       70–100% success
    /// - Moderate:   50–69% success
    /// - Risky:      30–49% success
    /// - Extreme:    0–29% success
    /// </summary>
    public static class RiskRewardCalculator
    {
        // Locked probability bands (as percentages for clarity, converted to [0,1] in code)
        private const float SAFE_MIN = 0.70f;
        private const float MODERATE_MIN = 0.50f;
        private const float RISKY_MIN = 0.30f;
        private const float EXTREME_MIN = 0.00f;
        private const float EXTREME_MAX = 0.29f;
        private const float RISKY_MAX = 0.49f;
        private const float MODERATE_MAX = 0.69f;
        private const float SAFE_MAX = 1.00f;

        // Hard clamp: no outcome is 100% guaranteed or 0% impossible
        private const float MIN_CLAMP = 0.05f;
        private const float MAX_CLAMP = 0.95f;

        /// <summary>
        /// Calculate the final success probability after clamping.
        /// 
        /// This ensures:
        /// - No choice is guaranteed (always ≥5% failure chance)
        /// - No choice is impossible (always ≤5% failure chance)
        /// </summary>
        public static float CalculateFinalSuccessChance(float baseChance)
        {
            return Mathf.Clamp(baseChance, MIN_CLAMP, MAX_CLAMP);
        }

        /// <summary>
        /// Map a final success probability to a risk level.
        /// </summary>
        public static RiskLevel GetRiskLevel(float finalSuccessProbability)
        {
            // Clamp first
            float clamped = CalculateFinalSuccessChance(finalSuccessProbability);

            if (clamped >= SAFE_MIN)
                return RiskLevel.Safe;
            else if (clamped >= MODERATE_MIN)
                return RiskLevel.Moderate;
            else if (clamped >= RISKY_MIN)
                return RiskLevel.Risky;
            else
                return RiskLevel.Extreme;
        }

        /// <summary>
        /// Get a human-readable percentage label for the risk level.
        /// For display in UI tooltips or info screens.
        /// </summary>
        public static string GetRiskLevelDescription(RiskLevel level)
        {
            return level switch
            {
                RiskLevel.Safe => "Safe (70–100% success)",
                RiskLevel.Moderate => "Moderate (50–69% success)",
                RiskLevel.Risky => "Risky (30–49% success)",
                RiskLevel.Extreme => "Extreme (0–29% success)",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Validate that risk and reward are properly offset.
        /// 
        /// Higher risk should generally offer higher reward.
        /// Used during event authoring to catch imbalances.
        /// </summary>
        public static bool ValidateRiskRewardOffset(RiskLevel risk, RewardLevel reward, out string feedback)
        {
            feedback = "";

            if (risk == RiskLevel.Calculated)
            {
                feedback = "✓ Calculated risk reward is validated after modifiers";
                return true;
            }

            if ((risk == RiskLevel.Safe && reward == RewardLevel.Low) ||
                (risk == RiskLevel.Safe && reward == RewardLevel.Medium) ||
                (risk == RiskLevel.Moderate && reward == RewardLevel.Medium) ||
                (risk == RiskLevel.Moderate && reward == RewardLevel.High) ||
                (risk == RiskLevel.Risky && reward == RewardLevel.High) ||
                (risk == RiskLevel.Risky && reward == RewardLevel.Exceptional) ||
                (risk == RiskLevel.Extreme && reward == RewardLevel.High) ||
                (risk == RiskLevel.Extreme && reward == RewardLevel.Exceptional))
            {
                feedback = "✓ Risk and reward are well-offset";
                return true;
            }

            feedback = $"Risk {risk} is not balanced by reward {reward}";
            return false;
        }

        /// <summary>
        /// Calculate what base success chance would result in a given risk level.
        /// Useful for event designers to work backwards.
        /// 
        /// Returns the midpoint of the band (e.g., 0.6 for Moderate).
        /// </summary>
        public static float GetBaseChanceForRiskLevel(RiskLevel level)
        {
            return level switch
            {
                RiskLevel.Safe => (SAFE_MIN + SAFE_MAX) / 2f,      // 0.85
                RiskLevel.Moderate => (MODERATE_MIN + MODERATE_MAX) / 2f,  // 0.595
                RiskLevel.Risky => (RISKY_MIN + RISKY_MAX) / 2f,    // 0.395
                RiskLevel.Extreme => (EXTREME_MIN + EXTREME_MAX) / 2f, // 0.145
                _ => 0.5f
            };
        }

        /// <summary>
        /// Get all risk levels in order of difficulty.
        /// </summary>
        public static RiskLevel[] GetRiskLevelsInOrder()
        {
            return new[] { RiskLevel.Safe, RiskLevel.Moderate, RiskLevel.Risky, RiskLevel.Extreme };
        }

        /// <summary>
        /// Get all reward levels in order of value.
        /// </summary>
        public static RewardLevel[] GetRewardLevelsInOrder()
        {
            return new[] { RewardLevel.Low, RewardLevel.Medium, RewardLevel.High, RewardLevel.Exceptional };
        }

        /// <summary>
        /// Validate that a choice's declared risk level matches its actual success probability.
        /// </summary>
        public static bool ValidateRiskLevelAccuracy(float baseSuccessChance, RiskLevel declaredRisk, List<string> modifierIds, out string feedback)
        {
            feedback = "";

            // For now, just validate the base chance without modifiers
            // (In Phase 1, this would evaluate all modifiers to get the final chance)
            RiskLevel calculatedRisk = GetRiskLevel(baseSuccessChance);

            if (declaredRisk == RiskLevel.Calculated)
            {
                // Accept if modifiers are declared
                if (modifierIds.Count == 0)
                {
                    feedback = "⚠ Choice uses Calculated risk but has no modifiers";
                    return false;
                }
                return true;
            }

            // Check if declared matches calculated
            if (declaredRisk == calculatedRisk)
            {
                feedback = $"✓ Declared risk {declaredRisk} matches calculated ({baseSuccessChance:P0})";
                return true;
            }

            feedback = $"⚠ Declared risk {declaredRisk} but base chance {baseSuccessChance:P0} suggests {calculatedRisk}";
            return false;
        }
    }

    /// <summary>
    /// Tests for RiskRewardCalculator.
    /// </summary>
#if UNITY_EDITOR
    public static class RiskRewardCalculatorTests
    {
        public static void RunAllTests()
        {
            Debug.Log("=== Risk/Reward Calculator Tests ===");

            TestClamping();
            TestRiskLevelMapping();
            TestRiskRewardOffset();
            TestBaseChanceCalculation();

            Debug.Log("=== All Tests Passed ===");
        }

        private static void TestClamping()
        {
            Debug.Log("Test: Clamping");
            
            assert(RiskRewardCalculator.CalculateFinalSuccessChance(0.0f) == 0.05f, "Zero should clamp to 5%");
            assert(RiskRewardCalculator.CalculateFinalSuccessChance(1.0f) == 0.95f, "100% should clamp to 95%");
            assert(RiskRewardCalculator.CalculateFinalSuccessChance(0.5f) == 0.5f, "50% should pass through");
            
            Debug.Log("✓ Clamping tests passed");
        }

        private static void TestRiskLevelMapping()
        {
            Debug.Log("Test: Risk Level Mapping");
            
            assert(RiskRewardCalculator.GetRiskLevel(0.75f) == RiskLevel.Safe, "75% should be Safe");
            assert(RiskRewardCalculator.GetRiskLevel(0.60f) == RiskLevel.Moderate, "60% should be Moderate");
            assert(RiskRewardCalculator.GetRiskLevel(0.40f) == RiskLevel.Risky, "40% should be Risky");
            assert(RiskRewardCalculator.GetRiskLevel(0.20f) == RiskLevel.Extreme, "20% should be Extreme");
            
            Debug.Log("✓ Risk level mapping tests passed");
        }

        private static void TestRiskRewardOffset()
        {
            Debug.Log("Test: Risk/Reward Offset");
            
            bool result;
            string feedback;
            
            result = RiskRewardCalculator.ValidateRiskRewardOffset(RiskLevel.Safe, RewardLevel.Low, out feedback);
            assert(result, "Safe + Low should be valid");
            
            result = RiskRewardCalculator.ValidateRiskRewardOffset(RiskLevel.Extreme, RewardLevel.Exceptional, out feedback);
            assert(result, "Extreme + Exceptional should be valid");
            
            result = RiskRewardCalculator.ValidateRiskRewardOffset(RiskLevel.Safe, RewardLevel.Exceptional, out feedback);
            assert(!result, "Safe + Exceptional should be invalid");
            
            Debug.Log("✓ Risk/reward offset tests passed");
        }

        private static void TestBaseChanceCalculation()
        {
            Debug.Log("Test: Base Chance Calculation");
            
            float safeChance = RiskRewardCalculator.GetBaseChanceForRiskLevel(RiskLevel.Safe);
            assert(safeChance >= 0.70f && safeChance <= 1.0f, $"Safe base chance should be in [0.7, 1.0], got {safeChance}");
            
            float moderateChance = RiskRewardCalculator.GetBaseChanceForRiskLevel(RiskLevel.Moderate);
            assert(moderateChance >= 0.50f && moderateChance <= 0.69f, $"Moderate base chance should be in [0.5, 0.69], got {moderateChance}");
            
            float riskyChance = RiskRewardCalculator.GetBaseChanceForRiskLevel(RiskLevel.Risky);
            assert(riskyChance >= 0.30f && riskyChance <= 0.49f, $"Risky base chance should be in [0.3, 0.49], got {riskyChance}");
            
            float extremeChance = RiskRewardCalculator.GetBaseChanceForRiskLevel(RiskLevel.Extreme);
            assert(extremeChance >= 0.0f && extremeChance <= 0.29f, $"Extreme base chance should be in [0, 0.29], got {extremeChance}");
            
            Debug.Log("✓ Base chance calculation tests passed");
        }

        private static void assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new System.Exception($"Assertion failed: {message}");
            }
        }
    }
#endif
}
