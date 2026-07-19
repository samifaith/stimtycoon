using NUnit.Framework;
using StimTycoon.Runtime;
using StimTycoon.Saves;

namespace StimTycoon.Tests.Domain.Runtime
{
    public sealed class NonEventAgeBoundaryAuditTests
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
            Assert.That(GameSessionService.GetLifeStage(age), Is.EqualTo(lifeStage));
            Assert.That(GameSessionService.GetEducationStage(age), Is.EqualTo(educationStage));
        }

        [TestCase(ActivityType.Play, 12, true)]
        [TestCase(ActivityType.Play, 13, false)]
        [TestCase(ActivityType.Study, 4, false)]
        [TestCase(ActivityType.Study, 5, true)]
        [TestCase(ActivityType.Workout, 12, false)]
        [TestCase(ActivityType.Workout, 13, true)]
        [TestCase(ActivityType.FamilyMovieCredit, 17, false)]
        [TestCase(ActivityType.FamilyMovieCredit, 18, true)]
        [TestCase(ActivityType.Explore, 17, true)]
        [TestCase(ActivityType.Explore, 18, false)]
        [TestCase(ActivityType.AttendSchool, 5, false)]
        [TestCase(ActivityType.AttendSchool, 6, true)]
        [TestCase(ActivityType.AttendSchool, 17, true)]
        [TestCase(ActivityType.AttendSchool, 18, false)]
        [TestCase(ActivityType.JoinClub, 9, false)]
        [TestCase(ActivityType.JoinClub, 10, true)]
        [TestCase(ActivityType.JoinClub, 17, true)]
        [TestCase(ActivityType.JoinClub, 18, false)]
        [TestCase(ActivityType.WorkShift, 17, false)]
        [TestCase(ActivityType.WorkShift, 18, true)]
        [TestCase(ActivityType.WorkShift, 64, true)]
        [TestCase(ActivityType.WorkShift, 65, false)]
        [TestCase(ActivityType.Overtime, 17, false)]
        [TestCase(ActivityType.Overtime, 18, true)]
        [TestCase(ActivityType.Training, 64, true)]
        [TestCase(ActivityType.Training, 65, false)]
        [TestCase(ActivityType.Socialize, 9, false)]
        [TestCase(ActivityType.Socialize, 10, true)]
        [TestCase(ActivityType.Hobby, 64, false)]
        [TestCase(ActivityType.Hobby, 65, true)]
        [TestCase(ActivityType.Checkup, 49, false)]
        [TestCase(ActivityType.Checkup, 50, true)]
        public void Activities_UseExactAndAdjacentAgeBoundaries(
            ActivityType activity, int age, bool expected)
        {
            Assert.That(GameSessionService.IsActivityAgeAppropriate(activity, age), Is.EqualTo(expected));
        }

        [TestCase(RelationshipInteractionType.Argue, 7, false)]
        [TestCase(RelationshipInteractionType.Argue, 8, true)]
        [TestCase(RelationshipInteractionType.Compete, 7, false)]
        [TestCase(RelationshipInteractionType.Compete, 8, true)]
        [TestCase(RelationshipInteractionType.Reconcile, 9, false)]
        [TestCase(RelationshipInteractionType.Reconcile, 10, true)]
        [TestCase(RelationshipInteractionType.DeepenFriendship, 9, false)]
        [TestCase(RelationshipInteractionType.DeepenFriendship, 10, true)]
        [TestCase(RelationshipInteractionType.Recover, 17, false)]
        [TestCase(RelationshipInteractionType.Recover, 18, true)]
        [TestCase(RelationshipInteractionType.Commit, 20, false)]
        [TestCase(RelationshipInteractionType.Commit, 21, true)]
        public void RelationshipActions_UseExactAndAdjacentAgeBoundaries(
            RelationshipInteractionType interaction, int age, bool expected)
        {
            Assert.That(GameSessionService.IsRelationshipInteractionAgeAppropriate(interaction, age),
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

            Assert.That(EducationActionService.TryGetStudySessionRequirement(
                state, StudyDifficulty.Easy, out _), Is.EqualTo(expected));
        }

        [Test]
        public void CareerAndRetirement_UseExactAdultBoundaries()
        {
            var state = CreateState(17);
            Assert.That(GameSessionService.TryGetCareerActionRequirement(
                state, CareerActionType.Retrain, out _), Is.False);

            state.character.age = 18;
            Assert.That(GameSessionService.TryGetCareerActionRequirement(
                state, CareerActionType.Retrain, out _), Is.True);

            state.career.roleTitle = "Associate";
            state.character.age = 64;
            Assert.That(GameSessionService.TryGetCareerActionRequirement(
                state, CareerActionType.Retire, out _), Is.False);

            state.character.age = 65;
            Assert.That(GameSessionService.TryGetCareerActionRequirement(
                state, CareerActionType.Retire, out _), Is.True);
        }

        private static GameState CreateState(int age)
        {
            return new GameState
            {
                character = new CharacterState { age = age, lifeStatus = "active" },
                education = new EducationState(),
                career = new CareerState(),
                finances = new FinancesState()
            };
        }
    }
}
