using System;
using System.Collections.Generic;
using StimTycoon.Saves;

namespace StimTycoon.Runtime
{
    public sealed class StimNewLifeRequest
    {
        public string firstName;
        public string lastName;
        public string pronouns;
        public string country;
        public string backgroundId;
    }

    public static class StimNewLifeFactory
    {
        public const string WorkingClassBackground = "working_class";
        public const string MiddleIncomeBackground = "middle_income";
        public const string WealthyBackground = "wealthy";

        public static StimSaveEnvelope Create(
            StimNewLifeRequest request,
            string gameBuildVersion,
            DateTimeOffset createdAt,
            int seed)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var now = createdAt.ToUniversalTime().ToString("O");
            var random = new Random(seed);
            var country = string.IsNullOrWhiteSpace(request.country)
                ? GenerateCountry(random)
                : NormalizeCountry(request.country);
            var background = string.IsNullOrWhiteSpace(request.backgroundId)
                ? GenerateBackground(random)
                : NormalizeBackground(request.backgroundId);
            var firstName = string.IsNullOrWhiteSpace(request.firstName)
                ? GenerateFirstName(country, random)
                : request.firstName.Trim();
            var lastName = string.IsNullOrWhiteSpace(request.lastName)
                ? GenerateLastName(country, random)
                : request.lastName.Trim();
            var pronouns = string.IsNullOrWhiteSpace(request.pronouns)
                ? GeneratePronouns(random)
                : request.pronouns.Trim();
            var avatarId = $"avatar_{random.Next(1, 13):00}";
            var appearanceSeed = random.Next(1, int.MaxValue);
            var parents = CreateParents(country, background, random);
            var startingStats = CreateChildStats(background, parents, random);

            return new StimSaveEnvelope
            {
                gameBuildVersion = string.IsNullOrWhiteSpace(gameBuildVersion) ? "0.1.0" : gameBuildVersion,
                contentVersion = "1",
                saveId = Guid.NewGuid().ToString("N"),
                playerAccountId = "local-player",
                lifeId = Guid.NewGuid().ToString("N"),
                createdAtUtc = now,
                updatedAtUtc = now,
                revision = 1,
                deviceIdHash = "local-device",
                rng = new StimRngState { seed = seed, step = 0 },
                integrity = new StimSaveIntegrity { payloadHash = "pending" },
                state = new StimGameState
                {
                    character = new StimCharacterState
                    {
                        firstName = firstName,
                        lastName = lastName,
                        pronouns = pronouns,
                        country = country,
                        backgroundId = background,
                        avatarId = avatarId,
                        appearanceSeed = appearanceSeed,
                        lifeStage = "infant",
                        age = 0,
                        health = startingStats.health,
                        happiness = startingStats.happiness,
                        smarts = startingStats.smarts,
                        looks = startingStats.looks,
                        luck = startingStats.luck
                    },
                    calendar = new StimCalendarState { monthOfYear = 1 },
                    finances = new StimFinancesState { cashMinorUnits = startingStats.cashMinorUnits },
                    career = new StimCareerState(),
                    education = new StimEducationState { stage = "not_started" },
                    relationships = new List<StimRelationshipState> { parents[0].relationship, parents[1].relationship },
                    lifeFeed = new List<StimLifeFeedEntry>
                    {
                        new StimLifeFeedEntry
                        {
                            entryId = "birth",
                            category = "milestone",
                            text = $"{firstName} {lastName} was born in {country} with two parents.",
                            age = 0,
                            monthOfYear = 1,
                            revision = 1,
                            timestampUtc = now
                        }
                    },
                    eventHistory = new List<StimEventHistoryEntry>(),
                    scheduledEvents = new List<StimScheduledEventRecord>()
                }
            };
        }

        private static ParentProfile[] CreateParents(string country, string background, Random random)
        {
            var startingValue = background == WealthyBackground ? 58 : background == MiddleIncomeBackground ? 62 : 65;
            var names = country == "Jamaica"
                ? new[] { "Alicia", "Marlon", "Simone", "Andre", "Nadine", "Dwayne" }
                : new[] { "Avery", "Jordan", "Morgan", "Cameron", "Taylor", "Riley" };
            var firstNameIndex = random.Next(names.Length);
            var secondNameIndex = random.Next(names.Length - 1);
            if (secondNameIndex >= firstNameIndex) secondNameIndex++;

            return new[]
            {
                CreateParent("parent_1", names[firstNameIndex], startingValue, random),
                CreateParent("parent_2", names[secondNameIndex], startingValue, random)
            };
        }

        private static ParentProfile CreateParent(string id, string name, int relationshipValue, Random random)
        {
            var profile = new ParentProfile
            {
                health = random.Next(38, 91),
                looks = random.Next(30, 91),
                smarts = random.Next(35, 91)
            };
            profile.relationship = new StimRelationshipState
            {
                relationshipId = id,
                displayName = name,
                relationshipType = "parent",
                isGeneticParent = true,
                geneticHealth = profile.health,
                geneticLooks = profile.looks,
                geneticSmarts = profile.smarts,
                value = relationshipValue
            };
            return profile;
        }

        private static (int health, int happiness, int smarts, int looks, int luck, long cashMinorUnits) CreateChildStats(
            string background,
            ParentProfile[] parents,
            Random random)
        {
            var health = Inherit(parents[0].health, parents[1].health, random, 12);
            var looks = Inherit(parents[0].looks, parents[1].looks, random, 14);
            var smarts = Inherit(parents[0].smarts, parents[1].smarts, random, 12);
            var happinessCenter = background == WorkingClassBackground ? 62 : background == WealthyBackground ? 68 : 66;
            var happiness = Clamp(happinessCenter + random.Next(-12, 13));
            var luck = random.Next(25, 76);
            return (health, happiness, smarts, looks, luck, GetStartingCash(background));
        }

        private static int Inherit(int firstParent, int secondParent, Random random, int variance)
        {
            var midpoint = (firstParent + secondParent) / 2;
            return Clamp(midpoint + random.Next(-variance, variance + 1));
        }

        private static int Clamp(int value)
        {
            return Math.Max(StimSaveSchema.MinCoreStatValue, Math.Min(StimSaveSchema.MaxCoreStatValue, value));
        }

        private static string GenerateFirstName(string country, Random random)
        {
            var names = country == "Jamaica"
                ? new[] { "Amari", "Zuri", "Malik", "Nia", "Kemar", "Imani", "Jelani", "Talia" }
                : new[] { "Alex", "Maya", "Noah", "Ari", "Jordan", "Sofia", "Eli", "Riley" };
            return names[random.Next(names.Length)];
        }

        private static string GenerateLastName(string country, Random random)
        {
            var names = country == "Jamaica"
                ? new[] { "Brown", "Campbell", "Williams", "Grant", "Reid", "Morgan", "Clarke", "Robinson" }
                : new[] { "Morgan", "Bennett", "Reed", "Carter", "Brooks", "Hayes", "Parker", "Rivera" };
            return names[random.Next(names.Length)];
        }

        private static string GeneratePronouns(Random random)
        {
            var values = new[] { "she/her", "he/him", "they/them" };
            return values[random.Next(values.Length)];
        }

        private static string NormalizeCountry(string country)
        {
            if (string.Equals(country, "USA", StringComparison.OrdinalIgnoreCase)) return "USA";
            if (string.Equals(country, "Jamaica", StringComparison.OrdinalIgnoreCase)) return "Jamaica";
            throw new ArgumentException("Country must be USA or Jamaica.", nameof(country));
        }

        private static string GenerateCountry(Random random)
        {
            return random.Next(2) == 0 ? "USA" : "Jamaica";
        }

        private static string GenerateBackground(Random random)
        {
            var roll = random.Next(10);
            if (roll < 4) return WorkingClassBackground;
            if (roll < 8) return MiddleIncomeBackground;
            return WealthyBackground;
        }

        private static string NormalizeBackground(string background)
        {
            if (background == WorkingClassBackground || background == MiddleIncomeBackground || background == WealthyBackground)
            {
                return background;
            }
            throw new ArgumentException("Background must be working_class, middle_income, or wealthy.", nameof(background));
        }

        private static long GetStartingCash(string background)
        {
            switch (background)
            {
                case WorkingClassBackground: return 0;
                case MiddleIncomeBackground: return 2500;
                default: return 10000;
            }
        }

        private sealed class ParentProfile
        {
            public int health;
            public int looks;
            public int smarts;
            public StimRelationshipState relationship;
        }
    }
}
