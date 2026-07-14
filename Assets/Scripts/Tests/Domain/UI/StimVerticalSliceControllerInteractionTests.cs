using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using StimTycoon.Abstractions;
using StimTycoon.Events;
using StimTycoon.Runtime;
using StimTycoon.Saves;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace StimTycoon.Tests.Domain.UI
{
    public sealed class StimVerticalSliceControllerInteractionTests
    {
        private const string PlayableLifePath = "Assets/UI/StimVerticalSlice.uxml";
        private GameObject host;
        private StimVerticalSliceController controller;
        private TemplateContainer root;
        private StimGameSessionService session;

        [SetUp]
        public void SetUp()
        {
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PlayableLifePath);
            Assert.That(asset, Is.Not.Null);
            root = asset.CloneTree();

            host = new GameObject("Stim controller interaction test");
            host.SetActive(false);
            host.AddComponent<UIDocument>();
            controller = host.AddComponent<StimVerticalSliceController>();

            session = new StimGameSessionService(
                new InMemoryStimEventCatalog(),
                new MemorySaveRepository(),
                utcNow: () => DateTimeOffset.Parse("2026-07-13T20:00:00Z"));
            var save = StimNewLifeFactory.Create(
                new StimNewLifeRequest
                {
                    firstName = "Avery",
                    lastName = "Grant",
                    country = "USA",
                    backgroundId = StimNewLifeFactory.MiddleIncomeBackground
                },
                "0.1.0",
                DateTimeOffset.Parse("2026-07-13T19:00:00Z"),
                1234);
            save.state.character.age = 18;
            save.state.character.lifeStage = StimGameSessionService.GetLifeStage(18);
            save.state.education.stage = StimGameSessionService.GetEducationStage(18);
            save.state.character.smarts = 60;
            save.state.character.happiness = 70;
            session.Start(save);

            BindControllerFields();
            Invoke("RefreshHeader");
            Invoke("RefreshFeed");
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(host);
        }

        [Test]
        public void PresentEvent_OpensSheetAndBuildsEveryChoice()
        {
            var evt = RepresentativeStimEvents.CreateSchoolGroupProject();

            Invoke("PresentEvent", evt);

            Assert.IsFalse(root.Q("event-sheet").ClassListContains("hidden"));
            Assert.IsFalse(root.Q("choices").ClassListContains("hidden"));
            Assert.That(root.Q("choices").Query<Button>().ToList(), Has.Count.EqualTo(evt.choices.Count));
            Assert.That(root.Q<Label>("event-title").text, Is.EqualTo(evt.titleKey));
        }

        [Test]
        public void LifeFeed_GroupsEntriesByMonthWithNewestMonthFirst()
        {
            session.ActiveSave.state.lifeFeed.Clear();
            session.ActiveSave.state.lifeFeed.Add(new StimLifeFeedEntry
                { age = 17, monthOfYear = 12, category = "milestone", text = "Older month" });
            session.ActiveSave.state.lifeFeed.Add(new StimLifeFeedEntry
                { age = 18, monthOfYear = 1, category = "activity", text = "Worked out" });
            session.ActiveSave.state.lifeFeed.Add(new StimLifeFeedEntry
                { age = 18, monthOfYear = 1, category = "event", text = "Met a friend" });

            Invoke("RefreshFeed");

            var groups = root.Q("life-feed-list")
                .Query<VisualElement>(className: "st-feed-month-group").ToList();
            Assert.That(groups, Has.Count.EqualTo(2));
            Assert.That(groups[0].Q<Label>(className: "st-feed-month-header").text,
                Is.EqualTo("AGE 18  ·  JANUARY"));
            Assert.That(groups[0].Query<VisualElement>(className: "st-feed-entry").ToList(),
                Has.Count.EqualTo(2));
            Assert.That(groups[0].Query<Label>(className: "st-feed-text").ToList()[0].text,
                Is.EqualTo("Met a friend"));
            Assert.That(groups[1].Q<Label>(className: "st-feed-month-header").text,
                Is.EqualTo("AGE 17  ·  DECEMBER"));
        }

        [Test]
        public void PerformActivity_ShowsSignedFeedbackAndRefreshesVisibleState()
        {
            var smartsBefore = session.ActiveSave.state.character.smarts;

            Invoke("PerformActivity", StimActivityType.Study);

            Assert.That(session.ActiveSave.state.character.smarts, Is.EqualTo(smartsBefore + 2));
            Assert.That(root.Q<Label>("event-category").text, Is.EqualTo("FOCUS COMPLETE"));
            Assert.That(root.Q<Label>("result-text").text, Does.Contain("Smarts +2"));
            Assert.That(root.Q<Label>("smarts-value").text, Is.EqualTo($"{smartsBefore + 2} / 100"));
            Assert.IsFalse(root.Q("event-sheet").ClassListContains("hidden"));
        }

        [Test]
        public void AdvanceMonth_RemainsAvailableAndShowsMonthlyOutcome()
        {
            var advanceButton = root.Q<Button>("advance-month");
            var monthBefore = session.ActiveSave.state.calendar.monthOfYear;

            Invoke("AdvanceMonth");

            Assert.That(session.ActiveSave.state.calendar.monthOfYear, Is.EqualTo(monthBefore + 1));
            Assert.IsFalse(advanceButton.ClassListContains("hidden"));
            Assert.That(root.Q<Label>("event-category").text, Is.EqualTo("MONTHLY SUMMARY"));
            Assert.That(root.Q<Label>("result-effects").text, Does.Contain("Cash"));
            Assert.IsFalse(root.Q("event-sheet").ClassListContains("hidden"));
        }

        [Test]
        public void SocialDestination_RendersParentsAndAgeAppropriateDetailActions()
        {
            Invoke("ShowSocialDestination");

            Assert.IsTrue(root.Q("life-scroll").ClassListContains("hidden"));
            Assert.IsFalse(root.Q("social-view").ClassListContains("hidden"));
            Assert.IsTrue(root.Q<Button>("nav-social").ClassListContains("active"));
            Assert.That(root.Q("relationship-list").Query<Button>().ToList(), Has.Count.EqualTo(2));

            var parent = session.ActiveSave.state.relationships[0];
            Invoke("ShowRelationshipDetail", parent.relationshipId);

            Assert.That(root.Q<Label>("relationship-name").text, Is.EqualTo(parent.displayName));
            Assert.IsFalse(root.Q("relationship-detail-view").ClassListContains("hidden"));
            Assert.That(root.Q("relationship-actions").Query<Button>().ToList(), Has.Count.EqualTo(3));
            Assert.That(root.Q<Button>("relationship-action-talk"), Is.Not.Null);
            Assert.That(root.Q<Button>("relationship-action-spendtime"), Is.Not.Null);
            Assert.That(root.Q<Button>("relationship-action-argue"), Is.Not.Null);
        }

        [Test]
        public void SocialInteraction_UpdatesProfileAndShowsOutcomeOverlay()
        {
            var parent = session.ActiveSave.state.relationships[0];
            var relationshipBefore = parent.value;
            Invoke("RefreshSocial");
            Invoke("ShowRelationshipDetail", parent.relationshipId);

            Invoke("PerformRelationshipInteraction", StimRelationshipInteractionType.Talk);

            Assert.That(session.ActiveSave.state.relationships[0].value, Is.EqualTo(relationshipBefore + 2));
            Assert.That(root.Q<Label>("event-category").text, Is.EqualTo("SOCIAL MOMENT"));
            Assert.That(root.Q<Label>("result-text").text, Does.Contain("Relationship +2"));
            Assert.That(root.Q<Label>("relationship-strength").text, Does.Contain((relationshipBefore + 2).ToString()));
            Assert.IsFalse(root.Q("event-sheet").ClassListContains("hidden"));
        }

        [Test]
        public void EducationCard_ShowsLevelsLocksAndTransactionalProgress()
        {
            session.ActiveSave.state.character.age = 10;
            session.ActiveSave.state.character.lifeStage = StimGameSessionService.GetLifeStage(10);
            session.ActiveSave.state.education.stage = StimGameSessionService.GetEducationStage(10);
            session.ActiveSave.state.skills.Clear();
            Invoke("RefreshHeader");

            Assert.IsFalse(root.Q("education-card").ClassListContains("hidden"));
            Assert.That(root.Q<Label>("learning-level").text, Is.EqualTo("Learning Level 1"));
            Assert.IsTrue(root.Q<Button>("education-action-read").enabledSelf);
            Assert.IsFalse(root.Q<Button>("education-action-studygroup").enabledSelf);
            Assert.That(root.Q<Button>("education-action-studygroup").text, Does.Contain("Learning Level 2"));
            Assert.That(root.Query<VisualElement>(className: "st-action-card").ToList(), Has.Count.EqualTo(4));
            Assert.That(root.Query<Label>(className: "st-action-card-preview").ToList(), Has.Count.EqualTo(4));
            Assert.That(root.Query<Label>(className: "st-action-requirement-chip").ToList(), Is.Not.Empty);

            Invoke("PerformEducationAction", StimEducationActionType.Read);

            Assert.That(StimGameSessionService.GetSkillExperience(
                session.ActiveSave.state.skills, "learning"), Is.EqualTo(12));
            Assert.That(root.Q<Label>("result-text").text, Does.Contain("Learning XP +12"));
            Assert.IsFalse(root.Q("event-sheet").ClassListContains("hidden"));
            Assert.IsFalse(root.Q<Button>("education-action-homework").enabledSelf);
        }

        [Test]
        public void EducationCard_PresentsAndCommitsRequiredSchoolPath()
        {
            session.ActiveSave.state.character.age = 15;
            session.ActiveSave.state.character.lifeStage = StimGameSessionService.GetLifeStage(15);
            session.ActiveSave.state.education.stage = "high_school";
            session.ActiveSave.state.education.awaitingDecisionId = "education_high_transition";

            Invoke("RefreshHeader");

            Assert.That(root.Q<Label>("education-stage").text, Is.EqualTo("School path decision required"));
            Assert.That(root.Q<Button>("school-path-academictrack"), Is.Not.Null);
            Assert.That(root.Q<Button>("school-path-vocationaltrack"), Is.Not.Null);
            Assert.That(root.Q<Button>("school-path-leaveschool"), Is.Not.Null);

            Invoke("PerformSchoolPathChoice", StimSchoolPathChoice.VocationalTrack);

            Assert.That(session.ActiveSave.state.education.schoolPath, Is.EqualTo("vocational_track"));
            Assert.That(session.ActiveSave.state.lifeDecisions, Has.Count.EqualTo(1));
            Assert.That(StimGameSessionService.GetSkillExperience(
                session.ActiveSave.state.skills, "practical"), Is.EqualTo(15));
            Assert.That(root.Q<Label>("result-text").text, Does.Contain("Practical XP +15"));
        }

        [Test]
        public void EducationCard_PreviewsAndCommitsAffordableStudyTrack()
        {
            session.ActiveSave.state.character.age = 15;
            session.ActiveSave.state.character.lifeStage = StimGameSessionService.GetLifeStage(15);
            session.ActiveSave.state.education.stage = "high_school";
            session.ActiveSave.state.education.awaitingDecisionId = null;
            session.ActiveSave.state.education.studyTrack = null;
            session.ActiveSave.state.finances.cashMinorUnits = 10000;

            Invoke("RefreshHeader");

            Assert.That(root.Q<Label>("education-stage").text, Is.EqualTo("Choose a study track"));
            Assert.That(root.Q<Button>("study-track-general").enabledSelf, Is.True);
            Assert.That(root.Q<Button>("study-track-academic").enabledSelf, Is.True);
            Assert.That(root.Q<Button>("study-track-vocational").enabledSelf, Is.True);
            Assert.That(root.Q("study-track-card-academic")
                .Q<Label>(className: "st-action-card-preview").text, Is.EqualTo("Materials: −$50.00"));

            Invoke("PerformStudyTrackChoice", StimStudyTrack.Academic);

            Assert.That(session.ActiveSave.state.education.studyTrack, Is.EqualTo("academic"));
            Assert.That(session.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(5000));
            Assert.That(root.Q<Label>("result-text").text, Does.Contain("Academic study track selected"));
            Assert.That(root.Q<Button>("study-track-academic"), Is.Null);
        }

        [Test]
        public void EducationCard_DisablesUnaffordableStudyTracks()
        {
            session.ActiveSave.state.character.age = 15;
            session.ActiveSave.state.education.stage = "high_school";
            session.ActiveSave.state.education.studyTrack = null;
            session.ActiveSave.state.finances.cashMinorUnits = 0;

            Invoke("RefreshHeader");

            Assert.That(root.Q<Button>("study-track-general").enabledSelf, Is.True);
            Assert.That(root.Q<Button>("study-track-academic").enabledSelf, Is.False);
            Assert.That(root.Q<Button>("study-track-vocational").enabledSelf, Is.False);
            Assert.That(root.Q("study-track-card-vocational")
                .Q<Label>(className: "st-action-requirement-chip").text,
                Is.EqualTo("Requires $75.00 cash"));
        }

        [Test]
        public void EducationCard_ShowsQualificationProgressAndCommitsStudyDifficulty()
        {
            session.ActiveSave.state.character.age = 15;
            session.ActiveSave.state.character.smarts = 60;
            session.ActiveSave.state.education.stage = "high_school";
            session.ActiveSave.state.education.studyTrack = "academic";
            session.ActiveSave.state.education.qualificationExperience = 45;

            Invoke("RefreshHeader");

            Assert.That(root.Q<Label>("qualification-progress").text,
                Is.EqualTo("45 / 50 Qualification XP"));
            Assert.That(root.Q<Button>("education-action-study-easy").enabledSelf, Is.True);
            Assert.That(root.Q<Button>("education-action-study-medium").enabledSelf, Is.True);
            Assert.That(root.Q<Button>("education-action-study-hard").enabledSelf, Is.True);

            Invoke("PerformStudySession", StimStudyDifficulty.Medium);

            Assert.That(session.ActiveSave.state.education.qualificationExperience, Is.EqualTo(65));
            Assert.That(root.Q<Label>("qualification-progress").text,
                Is.EqualTo("65 / 125 Qualification XP"));
            Assert.That(root.Q<Label>("result-text").text,
                Does.Contain("Certificate Qualification"));
            Assert.That(root.Q<Button>("education-action-study-easy").enabledSelf, Is.False);
        }

        [Test]
        public void CareerCard_DrivesApplicationInterviewAndHiringFlow()
        {
            session.ActiveSave.state.career = new StimCareerState();
            Invoke("RefreshHeader");

            Assert.IsFalse(root.Q("career-card").ClassListContains("hidden"));
            Assert.That(root.Q<Label>("career-role").text, Is.EqualTo("Unemployed"));
            Assert.IsTrue(root.Q<Button>("career-action-apply").enabledSelf);
            Assert.IsFalse(root.Q<Button>("career-action-interview").enabledSelf);

            Invoke("PerformCareerAction", StimCareerActionType.Apply);
            Assert.That(root.Q<Label>("result-text").text, Does.Contain("Interview unlocked next month"));
            Invoke("AdvanceMonth");
            Invoke("CloseEventSheet");
            Assert.IsTrue(root.Q<Button>("career-action-interview").enabledSelf);

            Invoke("PerformCareerAction", StimCareerActionType.Interview);
            Assert.That(session.ActiveSave.state.career.roleTitle, Is.EqualTo("Junior Associate"));
            Assert.That(root.Q<Label>("career-role").text, Is.EqualTo("Junior Associate"));
            Assert.That(root.Q<Label>("result-text").text, Does.Contain("Hired"));
        }

        [Test]
        public void ContextActivityDeck_ChangesForEmployedAdult()
        {
            session.ActiveSave.state.character.age = 30;
            session.ActiveSave.state.career.roleTitle = "Junior Associate";
            session.ActiveSave.state.career.annualSalaryMinorUnits = 4000000;

            Invoke("RefreshHeader");

            Assert.That(root.Q<Button>("context-activity-workshift"), Is.Not.Null);
            Assert.That(root.Q<Button>("context-activity-overtime"), Is.Not.Null);
            Assert.That(root.Q<Button>("context-activity-training"), Is.Not.Null);
            Assert.That(root.Q<Button>("context-activity-socialize"), Is.Not.Null);
            Assert.That(root.Q<Button>("context-activity-attendschool"), Is.Null);
        }

        [Test]
        public void FinalLifeSummary_AppearsWhenAnnualHealthDeclineEndsLife()
        {
            session.ActiveSave.state.character.age = 79;
            session.ActiveSave.state.character.health = 4;
            session.ActiveSave.state.character.lifeStage = StimGameSessionService.GetLifeStage(79);
            session.ActiveSave.state.calendar.monthOfYear = 12;
            session.ActiveSave.state.career = new StimCareerState();
            Invoke("RefreshHeader");

            Invoke("AdvanceMonth");

            Assert.That(session.ActiveSave.state.character.lifeStatus, Is.EqualTo("deceased"));
            Assert.IsFalse(root.Q("final-life-summary").ClassListContains("hidden"));
            Assert.That(root.Q<Label>("ending-status").text, Does.Contain("age 80"));
            Assert.That(root.Q<Label>("ending-summary").text, Does.Contain("death at age 80"));
            Assert.IsFalse(root.Q<Button>("advance-month").enabledSelf);
        }

        [Test]
        public void AchievementCard_RendersPersistedUnlocks()
        {
            session.ActiveSave.state.achievements.Add(new StimAchievementState
            {
                achievementId = "first_job",
                unlockedAtAge = 18,
                revision = session.ActiveSave.revision,
                timestampUtc = session.ActiveSave.updatedAtUtc
            });

            Invoke("RefreshHeader");

            Assert.That(root.Q<Label>("achievements-count").text, Is.EqualTo("1 unlocked"));
            Assert.That(root.Q("achievements-list").Query<VisualElement>(className: "st-achievement-row").ToList(), Has.Count.EqualTo(1));
            Assert.That(root.Q("achievements-list").Q<Label>(className: "st-achievement-name").text, Is.EqualTo("Hired"));
        }

        [Test]
        public void MoneyDestination_ManualTapPaysCurrentJobsHourlyRate()
        {
            session.ActiveSave.state.career = new StimCareerState
            {
                roleTitle = "Junior Associate",
                annualSalaryMinorUnits = 4000000
            };
            var cashBefore = session.ActiveSave.state.finances.cashMinorUnits;
            Invoke("ShowMoneyDestination");

            Assert.IsFalse(root.Q("money-view").ClassListContains("hidden"));
            Assert.IsTrue(root.Q<Button>("nav-money").ClassListContains("active"));
            Assert.That(root.Q<Label>("manual-work-rate").text, Is.EqualTo("$19.23 per hour"));
            Assert.That(root.Q<Button>("manual-work-tap").text, Does.Contain("+$19.23"));

            Invoke("PerformManualWorkTap");

            Assert.That(session.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(cashBefore + 1923));
            Assert.That(root.Q<Label>("manual-work-feedback").text, Does.Contain("Cash +$19.23"));
        }

        private void BindControllerFields()
        {
            SetField("gameSession", session);
            var bindings = new Dictionary<string, string>
            {
                { "cashValue", "cash-value" }, { "lifeSummary", "life-summary" },
                { "eventCategory", "event-category" }, { "eventTitle", "event-title" },
                { "eventBody", "event-body" }, { "resultText", "result-text" },
                { "resultEffects", "result-effects" }, { "lifeFeedList", "life-feed-list" },
                { "overviewCareer", "overview-career" }, { "overviewCalendar", "overview-calendar" },
                { "healthValue", "health-value" }, { "happinessValue", "happiness-value" },
                { "smartsValue", "smarts-value" }, { "looksValue", "looks-value" },
                { "luckValue", "luck-value" }, { "careerProgressValue", "career-progress-value" },
                { "monthlyPaycheckValue", "monthly-paycheck-value" },
                { "annualSalaryValue", "annual-salary-value" }, { "netWorthValue", "net-worth-value" },
                { "avatarGlyph", "avatar-glyph" }, { "choices", "choices" },
                { "resultCard", "result-card" }, { "playerOverview", "player-overview" },
                { "careerProgressFill", "career-progress-fill" }, { "eventSheet", "event-sheet" },
                { "healthFill", "health-fill" }, { "happinessFill", "happiness-fill" },
                { "smartsFill", "smarts-fill" }, { "looksFill", "looks-fill" },
                { "luckFill", "luck-fill" }, { "advanceMonth", "advance-month" },
                { "eventContinue", "event-continue" }, { "focusStudy", "focus-study" },
                { "focusWorkout", "focus-workout" }, { "focusStudyTitle", "focus-study-title" },
                { "focusStudyEffect", "focus-study-effect" }, { "focusWorkoutTitle", "focus-workout-title" },
                { "focusWorkoutEffect", "focus-workout-effect" },
                { "contextActivities", "context-activities" },
                { "lifeScroll", "life-scroll" }, { "socialView", "social-view" },
                { "timeDock", "time-dock" }, { "navLife", "nav-life" }, { "navSocial", "nav-social" },
                { "relationshipListView", "relationship-list-view" }, { "relationshipList", "relationship-list" },
                { "relationshipDetailView", "relationship-detail-view" },
                { "relationshipBack", "relationship-back" }, { "relationshipAvatar", "relationship-avatar" },
                { "relationshipName", "relationship-name" }, { "relationshipType", "relationship-type" },
                { "relationshipStrength", "relationship-strength" }, { "relationshipFill", "relationship-fill" },
                { "relationshipGenetics", "relationship-genetics" }, { "relationshipActions", "relationship-actions" },
                { "educationCard", "education-card" }, { "educationStage", "education-stage" },
                { "learningLevel", "learning-level" }, { "learningFill", "learning-fill" },
                { "learningProgress", "learning-progress" }, { "educationActions", "education-actions" },
                { "careerCard", "career-card" }, { "careerRole", "career-role" },
                { "careerSalary", "career-salary" }, { "careerNextStep", "career-next-step" },
                { "careerActionFill", "career-action-fill" },
                { "careerActionProgress", "career-action-progress" }, { "careerActions", "career-actions" },
                { "finalLifeSummary", "final-life-summary" }, { "endingName", "ending-name" },
                { "endingStatus", "ending-status" }, { "endingSummary", "ending-summary" },
                { "endingNewLife", "ending-new-life" }, { "achievementsCount", "achievements-count" },
                { "achievementsList", "achievements-list" }, { "navMoney", "nav-money" },
                { "moneyView", "money-view" }, { "manualWorkRole", "manual-work-role" },
                { "manualWorkRate", "manual-work-rate" }, { "moneyCashValue", "money-cash-value" },
                { "manualWorkTap", "manual-work-tap" }, { "manualWorkFeedback", "manual-work-feedback" }
            };

            foreach (var binding in bindings)
            {
                SetField(binding.Key, root.Q(binding.Value));
            }
        }

        private void SetField(string name, object value)
        {
            var field = typeof(StimVerticalSliceController).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Controller field '{name}' was not found.");
            field.SetValue(controller, value);
        }

        private void Invoke(string name, params object[] arguments)
        {
            var method = typeof(StimVerticalSliceController).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, $"Controller method '{name}' was not found.");
            method.Invoke(controller, arguments);
        }

        private sealed class MemorySaveRepository : IStimSaveRepository
        {
            private string latest;

            public bool TryCommitAutosave(string serializedSave, out string persistenceSummary)
            {
                latest = serializedSave;
                persistenceSummary = "saved";
                return true;
            }

            public bool TryLoadLatestSave(out string serializedSave)
            {
                serializedSave = latest;
                return !string.IsNullOrEmpty(latest);
            }

            public bool TryValidateSave(string serializedSave, out string validationSummary)
            {
                validationSummary = "valid";
                return !string.IsNullOrEmpty(serializedSave);
            }
        }
    }
}
