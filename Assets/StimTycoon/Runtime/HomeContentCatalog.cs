using System;
using System.Collections.Generic;

namespace StimTycoon.Runtime
{
    public enum HomeCapacityType
    {
        None,
        ReadingMaterials,
        EquipmentCondition
    }

    [Serializable]
    public sealed class HomeActionDefinition
    {
        public string actionId;
        public string roomObjectId;
        public HomeActionType actionType;
        public string displayName;
        public string benefitPreview;
        public long costMinorUnits;
        public HomeCapacityType capacityType;
        public int capacityConsumed;
        public int conditionDelta;
        public int improvementProgressDelta;
        public int benefitPerUpgradeLevel;
    }

    [Serializable]
    public sealed class HomeDefinition
    {
        public string homeId;
        public string displayName;
        public int startingCondition;
        public int maxUpgradeLevel;
        public List<HomeActionDefinition> actions = new List<HomeActionDefinition>();
    }

    public sealed class HomeContentValidationResult
    {
        public bool isValid = true;
        public readonly List<string> errors = new List<string>();
    }

    public static class HomeContentCatalog
    {
        private static readonly HomeDefinition StarterHome = new HomeDefinition
        {
            homeId = "starter_home",
            displayName = "Starter Home",
            startingCondition = 80,
            maxUpgradeLevel = 3,
            actions = new List<HomeActionDefinition>
            {
                Action("starter_read", "bookshelf", HomeActionType.Read, "Read", "Smarts +1 · Learning XP", 500, HomeCapacityType.ReadingMaterials, 1, -1, 2, 2),
                Action("starter_train", "training_corner", HomeActionType.Train, "Train", "Health +1 · Fitness XP", 1000, HomeCapacityType.EquipmentCondition, 10, -2, 2, 2),
                Action("starter_rest", "bed", HomeActionType.Rest, "Rest", "Health and Happiness recovery", 0, HomeCapacityType.None, 0, 0, 0, 1),
                Action("starter_maintain", "toolbox", HomeActionType.Maintain, "Maintain", "Condition +20 · Restock and repair", 5000, HomeCapacityType.None, 0, 20, 5, 0),
                Action("starter_household_time", "living_room", HomeActionType.HouseholdTime, "Household time", "Happiness, cohesion, relationships", 2000, HomeCapacityType.None, 0, 0, 0, 1)
            }
        };

        public static HomeDefinition Get(string homeId)
        {
            return string.Equals(homeId, StarterHome.homeId, StringComparison.Ordinal)
                ? StarterHome
                : null;
        }

        public static HomeActionDefinition GetAction(string homeId, HomeActionType actionType)
        {
            return Get(homeId)?.actions.Find(action => action.actionType == actionType);
        }

        public static HomeContentValidationResult Validate(HomeDefinition definition)
        {
            var result = new HomeContentValidationResult();
            if (definition == null || string.IsNullOrWhiteSpace(definition.homeId) ||
                string.IsNullOrWhiteSpace(definition.displayName))
            {
                result.isValid = false;
                result.errors.Add("Home ID and display name are required.");
                return result;
            }
            if (definition.startingCondition < 0 || definition.startingCondition > 100 ||
                definition.maxUpgradeLevel < 0 || definition.maxUpgradeLevel > 10)
            {
                result.isValid = false;
                result.errors.Add("Home condition or upgrade bounds are invalid.");
            }
            if (definition.actions == null || definition.actions.Count == 0)
            {
                result.isValid = false;
                result.errors.Add("At least one room-object action is required.");
                return result;
            }
            var ids = new HashSet<string>(StringComparer.Ordinal);
            var types = new HashSet<HomeActionType>();
            foreach (var action in definition.actions)
            {
                if (action == null || string.IsNullOrWhiteSpace(action.actionId) ||
                    string.IsNullOrWhiteSpace(action.roomObjectId) || string.IsNullOrWhiteSpace(action.displayName) ||
                    string.IsNullOrWhiteSpace(action.benefitPreview) || !ids.Add(action.actionId) ||
                    !types.Add(action.actionType) || action.costMinorUnits < 0 || action.capacityConsumed < 0 ||
                    action.conditionDelta < -100 || action.conditionDelta > 100 ||
                    action.improvementProgressDelta < 0 || action.improvementProgressDelta > 100)
                {
                    result.isValid = false;
                    result.errors.Add("Home actions require unique IDs/types and valid room, preview, cost, capacity, condition, and progress values.");
                }
            }
            return result;
        }

        private static HomeActionDefinition Action(
            string id, string room, HomeActionType type, string name, string preview, long cost,
            HomeCapacityType capacityType, int capacityConsumed, int conditionDelta,
            int progressDelta, int upgradeBenefit)
        {
            return new HomeActionDefinition
            {
                actionId = id,
                roomObjectId = room,
                actionType = type,
                displayName = name,
                benefitPreview = preview,
                costMinorUnits = cost,
                capacityType = capacityType,
                capacityConsumed = capacityConsumed,
                conditionDelta = conditionDelta,
                improvementProgressDelta = progressDelta,
                benefitPerUpgradeLevel = upgradeBenefit
            };
        }
    }
}
