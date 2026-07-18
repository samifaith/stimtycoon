using NUnit.Framework;
using StimTycoon.Runtime;
using StimTycoon.Saves;

namespace StimTycoon.Tests.Domain.Runtime
{
    public sealed class StimNonEventAgeBoundaryAuditTests
    {
        [TestCase(2, "infant", "not_started")]
        [TestCase(3, "early_childhood", "not_started")]
        [TestCase(5, "early_childhood", "not_started")]
        [TestCase(6, "primary_school", "primary_school")]
        [TestCase(11, "primary_school", "primary_school")]
        [TestCase(12, "secondary_school", "middle_school")]
        [TestCase(14, "secondary_school", "middle_school")]
        [TestCase(15, "secondary_school", "high_school")]
        [TestCase(17, "secondary_school", "high_school")]
        [TestCase(18, "adult", "completed_secondary")]
        [TestCase(64, "adult", "completed_secondary")]
        [TestCase(65, "retirement", "completed_secondary")]
        public void LifeAndEducationStages_UseExactAdjacentBoundaries(
            int age, string lifeStage, string educationStage)
        {
            Assert.That(StimGameSessionService.GetLifeStage(age), Is.EqualTo(lifeStage));
            Assert.That(StimGameSessionService.GetEducationStage(age), Is.EqualTo(educationStage));
        }

        [TestCase(StimActivityType.Play, 12, true)]
        [TestCase(StimActivityType.Play, 13, false)]
        [TestCase(StimActivityType.Study, 4, false)]
        [TestCase(StimActivityType.Study, 5, true)]
        [TestCase(StimActivityType.Workout, 12, false)]
        [TestCase(StimActivityType.Workout, 13, true)]
        [TestCase(StimActivityType.FamilyMovieCredit, 17, false)]
        [TestCase(StimActivityType.FamilyMovieCredit, 18, true)]
        [TestCase(StimActivityType.Explore, 17, true)]
        [TestCase(StimActivityType.Explore, 18, false)]
        [TestCase(StimActivityType.AttendSchool, 5, false)]
        [TestCase(StimActivityType.AttendSchool, 6, true)]
        [TestCase(StimActivityType.AttendSchool, 17, true)]
        [TestCase(StimActivityType.AttendSchool, 18, false)]
        [TestCase(StimActivityType.JoinClub, 9, false)]
        [TestCase(StimActivityType.JoinClub, 10, true)]
        [TestCase(StimActivityType.JoinClub, 17, true)]
        [TestCase(StimActivityType.JoinClub, 18, false)]
        [TestCase(StimActivityType.WorkShift, 17, false)]
        [TestCase(StimActivityType.WorkShift, 18, true)]
        [TestCase(StimActivityType.WorkShift, 64, true)]
        [TestCase(StimActivityType.WorkShift, 65, false)]
        [TestCase(StimActivityType.Overtime, 17, false)]
        [TestCase(StimActivityType.Overtime, 18, true)]
        [TestCase(StimActivityType.Training, 64, true)]
        [TestCase(StimActivityType.Training, 65, false)]
        [TestCase(StimActivityType.Socialize, 9, false)]
        [TestCase(StimActivityType.Socialize, 10, true)]
        [TestCase(StimActivityType.Hobby, 64, false)]
        [TestCase(StimActivityType.Hobby, 65, true)]
        [TestCase(StimActivityType.Checkup, 49, false)]
        [TestCase(StimActivityType.Checkup, 50, true)]
        public void Activities_UseExactAndAdjacentAgeBoundaries(
            StimActivityType activity, int age, bool expected)
        {
            Assert.That(StimGameSessionService.IsActivityAgeAppropriate(activity, age), Is.EqualTo(expected));
        }

        [TestCase(StimRelationshipInteractionType.Argue, 7, false)]
        [TestCase(StimRelationshipInteractionType.Argue, 8, true)]
        [TestCase(StimRelationshipInteractionType.Compete, 7, false)]
        [TestCase(StimRelationshipInteractionType.Compete, 8, true)]
        [TestCase(StimRelationshipInteractionType.Reconcile, 9, false)]
        [TestCase(StimRelationshipInteractionType.Reconcile, 10, true)]
        [TestCase(StimRelationshipInteractionType.DeepenFriendship, 9, false)]
        [TestCase(StimRelationshipInteractionType.DeepenFriendship, 10, true)]
        [TestCase(StimRelationshipInteractionType.Recover, 17, false)]
        [TestCase(StimRelationshipInteractionType.Recover, 18, true)]
        [TestCase(StimRelationshipInteractionType.Commit, 20, false)]
        [TestCase(StimRelationshipInteractionType.Commit, 21, true)]
        public void RelationshipActions_UseExactAndAdjacentAgeBoundaries(
            StimRelationshipInteractionType interaction, int age, bool expected)
        {
            Assert.That(StimGameSessionService.IsRelationshipInteractionAgeAppropriate(interaction, age),
                Is.EqualTo(expected));
        }

        [TestCase(13, false)]
        [TestCase(14, true)]
        [TestCase(17, true)]
        [TestCase(18, false)]
        public void FocusedStudy_UsesExactEnrollmentAgeBoundaries(int age, bool expected)
        {
            var state = CreateState(age);
            state.education.studyTrack = "general";

            Assert.That(StimEducationActionService.TryGetStudySessionRequirement(
                state, StimStudyDifficulty.Easy, out _), Is.EqualTo(expected));
        }

        [Test]
        public void CareerAndRetirement_UseExactAdultBoundaries()
        {
            var state = CreateState(17);
            Assert.That(StimGameSessionService.TryGetCareerActionRequirement(
                state, StimCareerActionType.Retrain, out _), Is.False);

            state.character.age = 18;
            Assert.That(StimGameSessionService.TryGetCareerActionRequirement(
                state, StimCareerActionType.Retrain, out _), Is.True);

            state.career.roleTitle = "Associate";
            state.character.age = 64;
            Assert.That(StimGameSessionService.TryGetCareerActionRequirement(
                state, StimCareerActionType.Retire, out _), Is.False);

            state.character.age = 65;
            Assert.That(StimGameSessionService.TryGetCareerActionRequirement(
                state, StimCareerActionType.Retire, out _), Is.True);
        }

        private static StimGameState CreateState(int age)
        {
            return new StimGameState
            {
                character = new StimCharacterState { age = age, lifeStatus = "active" },
                education = new StimEducationState(),
                career = new StimCareerState(),
                finances = new StimFinancesState()
            };
        }
    }
}
