using System;
using NUnit.Framework;
using StimTycoon.Runtime;
using StimTycoon.Saves;

namespace StimTycoon.Tests.Domain.Runtime
{
    public sealed class NewLifeFactoryTests
    {
        [TestCase(NewLifeFactory.WorkingClassBackground, 0L)]
        [TestCase(NewLifeFactory.MiddleIncomeBackground, 2500L)]
        [TestCase(NewLifeFactory.WealthyBackground, 10000L)]
        public void Create_BuildsValidBirthStateWithTwoCaregivers(string background, long expectedCash)
        {
            var save = NewLifeFactory.Create(
                new NewLifeRequest
                {
                    firstName = "Maya",
                    lastName = "Brown",
                    pronouns = "she/her",
                    country = "Jamaica",
                    backgroundId = background
                },
                "0.1.0",
                DateTimeOffset.Parse("2026-07-13T20:00:00Z"),
                1234);

            Assert.That(SaveValidator.ValidateSave(save).isValid, Is.True);
            Assert.That(save.state.character.firstName, Is.EqualTo("Maya"));
            Assert.That(save.state.character.country, Is.EqualTo("Jamaica"));
            Assert.That(save.state.character.backgroundId, Is.EqualTo(background));
            Assert.That(save.state.character.age, Is.Zero);
            Assert.That(save.state.character.lifeStage, Is.EqualTo("infant"));
            Assert.That(save.state.career.roleTitle, Is.Null.Or.Empty);
            Assert.That(save.state.finances.cashMinorUnits, Is.EqualTo(expectedCash));
            Assert.That(save.state.relationships, Has.Count.EqualTo(2));
            Assert.That(save.state.relationships.TrueForAll(
                relationship => relationship.relationshipType == "parent" && relationship.isGeneticParent), Is.True);
            AssertInheritedRange(save.state.character.health, save.state.relationships, relationship => relationship.geneticHealth, 12);
            AssertInheritedRange(save.state.character.looks, save.state.relationships, relationship => relationship.geneticLooks, 14);
            AssertInheritedRange(save.state.character.smarts, save.state.relationships, relationship => relationship.geneticSmarts, 12);
        }

        [Test]
        public void Create_SameSeedReproducesGeneticsAndDifferentSeedVariesLife()
        {
            var request = new NewLifeRequest
            {
                firstName = "Maya",
                lastName = "Brown",
                country = "Jamaica",
                backgroundId = NewLifeFactory.MiddleIncomeBackground
            };
            var createdAt = DateTimeOffset.Parse("2026-07-13T20:00:00Z");
            var first = NewLifeFactory.Create(request, "0.1.0", createdAt, 1234);
            var repeated = NewLifeFactory.Create(request, "0.1.0", createdAt, 1234);
            var different = NewLifeFactory.Create(request, "0.1.0", createdAt, 9876);

            Assert.That(repeated.state.character.health, Is.EqualTo(first.state.character.health));
            Assert.That(repeated.state.character.looks, Is.EqualTo(first.state.character.looks));
            Assert.That(repeated.state.character.smarts, Is.EqualTo(first.state.character.smarts));
            Assert.That(repeated.state.character.happiness, Is.EqualTo(first.state.character.happiness));
            Assert.That(repeated.state.character.luck, Is.EqualTo(first.state.character.luck));
            Assert.That(repeated.state.relationships[0].geneticHealth, Is.EqualTo(first.state.relationships[0].geneticHealth));

            var everyStatMatches = different.state.character.health == first.state.character.health &&
                                   different.state.character.looks == first.state.character.looks &&
                                   different.state.character.smarts == first.state.character.smarts &&
                                   different.state.character.happiness == first.state.character.happiness &&
                                   different.state.character.luck == first.state.character.luck;
            Assert.IsFalse(everyStatMatches, "Different seeds should produce meaningfully different starting lives.");
        }

        [Test]
        public void Create_EmptyRequestAutoGeneratesCompletePerson()
        {
            var save = NewLifeFactory.Create(
                new NewLifeRequest(),
                "0.1.0",
                DateTimeOffset.Parse("2026-07-13T20:00:00Z"),
                2468);

            Assert.That(save.state.character.firstName, Is.Not.Null.And.Not.Empty);
            Assert.That(save.state.character.lastName, Is.Not.Null.And.Not.Empty);
            CollectionAssert.Contains(new[] { "she/her", "he/him", "they/them" }, save.state.character.pronouns);
            CollectionAssert.Contains(new[] { "USA", "Jamaica" }, save.state.character.country);
            CollectionAssert.Contains(
                new[]
                {
                    NewLifeFactory.WorkingClassBackground,
                    NewLifeFactory.MiddleIncomeBackground,
                    NewLifeFactory.WealthyBackground
                },
                save.state.character.backgroundId);
            Assert.That(save.state.character.avatarId, Does.StartWith("avatar_"));
            Assert.That(save.state.character.appearanceSeed, Is.GreaterThan(0));
            Assert.That(save.state.relationships, Has.Count.EqualTo(2));
            Assert.That(save.state.relationships.TrueForAll(parent => parent.isGeneticParent), Is.True);
            Assert.That(save.state.lifeFeed[0].text, Does.Contain(save.state.character.firstName));
            Assert.That(SaveValidator.ValidateSave(save).isValid, Is.True);
        }

        [Test]
        public void Create_RejectsUnsupportedCountryAndBackground()
        {
            var request = new NewLifeRequest
            {
                firstName = "Ari",
                lastName = "Morgan",
                country = "Canada",
                backgroundId = NewLifeFactory.MiddleIncomeBackground
            };

            Assert.Throws<ArgumentException>(() => NewLifeFactory.Create(
                request,
                "0.1.0",
                DateTimeOffset.UtcNow,
                1));

            request.country = "USA";
            request.backgroundId = "unknown";
            Assert.Throws<ArgumentException>(() => NewLifeFactory.Create(
                request,
                "0.1.0",
                DateTimeOffset.UtcNow,
                1));
        }

        private static void AssertInheritedRange(
            int childValue,
            System.Collections.Generic.List<RelationshipState> parents,
            Func<RelationshipState, int> geneticValue,
            int variance)
        {
            var midpoint = (geneticValue(parents[0]) + geneticValue(parents[1])) / 2;
            Assert.That(childValue, Is.InRange(Math.Max(0, midpoint - variance), Math.Min(100, midpoint + variance)));
        }
    }
}
