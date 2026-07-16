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
            Invoke("ConfigureDestinationContent");
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
        public void FirstLifeOrientation_UsesOneFocusedScreenAndPersistsContinue()
        {
            session.ActiveSave.state.character.age = 0;
            session.ActiveSave.state.orientation = new StimOrientationState();

            Invoke("PresentFirstLifeOrientation");

            Assert.That(root.Q<Label>("event-category").text, Is.EqualTo("WELCOME TO STIM TYCOON"));
            Assert.That(root.Q<Label>("event-body").text,
                Does.Contain("Life Feed").And.Contain("Advance Month").And.Contain("Locked actions").And.Contain("autosaves"));
            Assert.IsFalse(root.Q("event-sheet").ClassListContains("hidden"));

            Invoke("CloseEventSheet");

            Assert.That(session.ActiveSave.state.orientation.status, Is.EqualTo("completed"));
            Assert.IsTrue(root.Q("event-sheet").ClassListContains("hidden"));
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
            Assert.That(groups[0].Query<Label>(className: "st-feed-title").ToList()[0].text,
                Is.EqualTo("Met a friend"));
            Assert.That(groups[1].Q<Label>(className: "st-feed-month-header").text,
                Is.EqualTo("AGE 17  ·  DECEMBER"));
            Assert.That(groups[0].Query<VisualElement>(className: "st-feed-dot").ToList(), Is.Not.Empty);
            Assert.That(groups[0].Q<VisualElement>("feed-item-1").tooltip,
                Does.Contain("Item 1 of 3").And.Contain("Event"));
        }

        [Test]
        public void LifeFeed_UsesDeterministicSortWithoutMutatingSavedOrder()
        {
            session.ActiveSave.state.lifeFeed.Clear();
            var storedFirst = new StimLifeFeedEntry
            {
                entryId = "older", age = 18, monthOfYear = 1, revision = 4,
                category = "activity", text = "Stored first", timestampUtc = "2026-01-01T00:00:00Z"
            };
            session.ActiveSave.state.lifeFeed.Add(storedFirst);
            session.ActiveSave.state.lifeFeed.Add(new StimLifeFeedEntry
            {
                entryId = "event", age = 18, monthOfYear = 1, revision = 5,
                category = "event", text = "Event outcome", timestampUtc = "2026-01-02T00:00:00Z"
            });
            session.ActiveSave.state.lifeFeed.Add(new StimLifeFeedEntry
            {
                entryId = "milestone", age = 18, monthOfYear = 1, revision = 5,
                category = "milestone", text = "Major transition", timestampUtc = "2026-01-02T00:00:00Z"
            });

            Invoke("RefreshFeed");

            var rendered = root.Q("life-feed-list").Query<Label>(className: "st-feed-title").ToList();
            Assert.That(rendered[0].text, Is.EqualTo("Major transition"));
            Assert.That(rendered[1].text, Is.EqualTo("Event outcome"));
            Assert.That(rendered[2].text, Is.EqualTo("Stored first"));
            Assert.That(session.ActiveSave.state.lifeFeed[0], Is.SameAs(storedFirst));
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
        public void AdvanceYear_ShowsBatchSummaryAfterTwelveCommittedMonths()
        {
            var ageBefore = session.ActiveSave.state.character.age;

            Invoke("AdvanceYear");

            Assert.That(session.ActiveSave.state.character.age, Is.EqualTo(ageBefore + 1));
            Assert.That(root.Q<Label>("event-category").text, Is.EqualTo("YEAR SUMMARY"));
            Assert.That(root.Q<Label>("result-text").text, Does.Contain("Advanced 12 months"));
            Assert.That(root.Q<Label>("result-effects").text,
                Is.EqualTo("12 monthly transactions committed"));
            Assert.That(root.Q("life-feed-list").Query<VisualElement>(className: "category-year").ToList(),
                Has.Count.EqualTo(1));
            Assert.That(root.Q("life-feed-list").Q<VisualElement>(className: "category-year")
                .Q<Label>(className: "st-feed-title").text,
                Does.Contain("Advanced 12 months"));
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
        public void SixDestinationNavigation_ActivatesOneViewAndPreservesSocialDetail()
        {
            Invoke("ShowEducationDestination");
            Assert.IsFalse(root.Q("education-view").ClassListContains("hidden"));
            Assert.IsTrue(root.Q<Button>("nav-education").ClassListContains("active"));
            Assert.That(root.Q("education-destination-content").Q("education-card"), Is.Not.Null);

            Invoke("ShowCareerDestination");
            Assert.IsFalse(root.Q("career-view").ClassListContains("hidden"));
            Assert.IsTrue(root.Q<Button>("nav-career").ClassListContains("active"));
            Assert.That(root.Q("career-destination-content").Q("career-card"), Is.Not.Null);

            Invoke("ShowGoalsDestination");
            Assert.IsFalse(root.Q("goals-view").ClassListContains("hidden"));
            Assert.IsTrue(root.Q<Button>("nav-goals").ClassListContains("active"));
            Assert.That(root.Q("goals-destination-content").Q("achievements-card"), Is.Not.Null);

            Invoke("ShowSocialDestination");
            var relationshipId = session.ActiveSave.state.relationships[0].relationshipId;
            Invoke("ShowRelationshipDetail", relationshipId);
            Invoke("ShowMoneyDestination");
            Invoke("ShowSocialDestination");

            Assert.IsFalse(root.Q("relationship-detail-view").ClassListContains("hidden"));
            Assert.That(root.Q<Label>("relationship-name").text,
                Is.EqualTo(session.ActiveSave.state.relationships[0].displayName));
        }

        [Test]
        public void LifeSummary_OpensFromHeaderAndReturnsToPreviousDestination()
        {
            Invoke("ShowMoneyDestination");

            Invoke("ShowLifeSummary");

            Assert.IsFalse(root.Q("life-summary-view").ClassListContains("hidden"));
            Assert.IsTrue(root.Q("money-view").ClassListContains("hidden"));
            Assert.IsTrue(root.Q("time-dock").ClassListContains("hidden"));
            Assert.IsTrue(root.Q<Button>("nav-money").ClassListContains("active"));
            Assert.That(root.Q<Label>("summary-career-detail").text, Is.Not.Empty);

            Invoke("CloseLifeSummary");

            Assert.IsTrue(root.Q("life-summary-view").ClassListContains("hidden"));
            Assert.IsFalse(root.Q("money-view").ClassListContains("hidden"));
            Assert.IsTrue(root.Q<Button>("nav-money").ClassListContains("active"));
            Assert.IsTrue(root.Q("time-dock").ClassListContains("hidden"));
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
        public void PartnerDetail_ShowsConsentGatedFamilyPlanningActions()
        {
            var partner = session.ActiveSave.state.relationships[0];
            partner.relationshipType = "partner";
            partner.relationshipStage = "partnered";
            partner.value = 80;
            partner.warmth = 70;
            Invoke("RefreshSocial");
            Invoke("ShowRelationshipDetail", partner.relationshipId);

            Assert.That(root.Q<Button>("family-action-discuss"), Is.Not.Null);
            Assert.IsTrue(root.Q<Button>("family-action-discuss").enabledSelf);
            Assert.IsFalse(root.Q<Button>("family-action-tryforchild").enabledSelf);
            Assert.IsFalse(root.Q<Button>("family-action-pursueadoption").enabledSelf);
            Assert.IsTrue(root.Q<Button>("family-action-optout").enabledSelf);

            Invoke("PerformFamilyPlanning", StimFamilyPlanningAction.Discuss);
            Assert.IsTrue(session.ActiveSave.state.family.partnerConsent);
            Assert.That(root.Q<Label>("result-text").text, Does.Contain("Both partners agreed"));
        }

        [Test]
        public void ChildDetail_ShowsParentingActionsAndRefreshesDevelopment()
        {
            session.ActiveSave.state.family.children.Add(new StimChildState
            {
                childId = "child_ui", displayName = "Ari", path = "adoption",
                parentRelationshipId = "parent_1", joinedAtParentAge = 18,
                birthMonth = 1, age = 8, wellbeing = 60, custodyStatus = "household"
            });
            session.ActiveSave.state.relationships.Add(new StimRelationshipState
            {
                relationshipId = "child_ui", displayName = "Ari", relationshipType = "child",
                relationshipStage = "dependent_child", value = 70, warmth = 70
            });
            Invoke("RefreshSocial");
            Invoke("ShowRelationshipDetail", "child_ui");

            Assert.That(root.Q<Button>("parenting-action-qualitytime"), Is.Not.Null);
            Assert.That(root.Q<Button>("parenting-action-supportneeds").text, Does.Contain("$25"));
            Invoke("PerformParentingAction", StimParentingAction.Teach);

            Assert.That(session.ActiveSave.state.family.children[0].learning, Is.EqualTo(7));
            Assert.That(root.Q<Label>("result-text").text, Does.Contain("Learning improved"));
            Assert.IsFalse(root.Q<Button>("parenting-action-qualitytime").enabledSelf);
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
        public void EducationCatalog_AppearsAtTrackAgeAndMarksPersistentSelection()
        {
            session.ActiveSave.state.character.age = 13;
            session.ActiveSave.state.education.stage = "middle_school";
            Invoke("RefreshEducation");
            Assert.IsTrue(root.Q("education-catalog").ClassListContains("hidden"));

            session.ActiveSave.state.character.age = 15;
            session.ActiveSave.state.education.stage = "high_school";
            session.ActiveSave.state.education.studyTrack = null;
            session.ActiveSave.state.finances.cashMinorUnits = 10000;
            Invoke("RefreshEducation");

            Assert.IsFalse(root.Q("education-catalog").ClassListContains("hidden"));
            Assert.That(root.Q("education-catalog-list").childCount, Is.EqualTo(3));
            Assert.That(root.Q<Button>("path-row-study-general"), Is.Not.Null);
            Assert.That(root.Q<Button>("path-row-study-academic"), Is.Not.Null);
            Assert.That(root.Q<Button>("path-row-study-vocational"), Is.Not.Null);
            Assert.That(root.Q<Button>("path-row-study-general").Q<Label>(className: "st-path-title").text,
                Is.EqualTo("Applied Finance"));
            Assert.That(root.Q<Button>("path-row-study-academic").Q<Label>(className: "st-path-title").text,
                Is.EqualTo("Community Health"));
            Assert.That(root.Q<Button>("path-row-study-vocational").Q<Label>(className: "st-path-title").text,
                Is.EqualTo("Sustainable Trades"));

            Invoke("PerformStudyTrackChoice", StimStudyTrack.Academic);

            Assert.IsTrue(root.Q("path-row-study-academic").ClassListContains("selected"));
            Assert.That(root.Q<Label>("education-catalog-status").text,
                Does.Contain("Current: Academic").And.Contain("0 XP"));
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
        public void StudySessionCard_OpensFocusedPreviewBeforeCommit()
        {
            session.ActiveSave.state.character.age = 15;
            session.ActiveSave.state.character.smarts = 60;
            session.ActiveSave.state.education.stage = "high_school";
            session.ActiveSave.state.education.studyTrack = "academic";
            session.ActiveSave.state.education.qualificationExperience = 45;
            var definition = session.GetStudySessionDefinitions()[1];

            Invoke("ShowStudySessionSheet", StimStudyDifficulty.Medium, definition);

            Assert.IsFalse(root.Q("study-session-sheet").ClassListContains("hidden"));
            Assert.That(root.Q<Label>("study-session-title").text, Is.EqualTo("Medium Study Session"));
            Assert.That(root.Q<Label>("study-session-effects").text,
                Does.Contain("Qualification XP +20").And.Contain("Happiness −1"));
            Assert.That(root.Q<Label>("study-session-timing").text, Does.Contain("this month's school action"));
            Assert.IsTrue(root.Q<Button>("study-session-confirm").enabledSelf);

            Invoke("ConfirmSelectedStudySession");

            Assert.IsTrue(root.Q("study-session-sheet").ClassListContains("hidden"));
            Assert.That(session.ActiveSave.state.education.qualificationExperience, Is.EqualTo(65));
            Assert.That(root.Q<Label>("result-text").text, Does.Contain("Certificate Qualification"));
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
        public void CareerCard_ShowsQualificationRequirementForSelectedStudyTrack()
        {
            session.ActiveSave.state.career = new StimCareerState();
            session.ActiveSave.state.education.studyTrack = "general";
            session.ActiveSave.state.education.qualificationExperience = 124;

            Invoke("RefreshHeader");

            var apply = root.Q<Button>("career-action-apply");
            Assert.That(apply.enabledSelf, Is.False);
            Assert.That(apply.text, Does.Contain("Diploma qualification (125 XP)"));

            session.ActiveSave.state.education.qualificationExperience = 125;
            Invoke("RefreshHeader");
            Assert.That(root.Q<Button>("career-action-apply").enabledSelf, Is.True);
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
        public void SkillsCard_ShowsFitnessAndProfessionalLevelProgress()
        {
            session.ActiveSave.state.skills.Add(new StimSkillState
                { skillId = "fitness", experience = 50 });
            session.ActiveSave.state.skills.Add(new StimSkillState
                { skillId = "professional", experience = 25 });

            Invoke("RefreshHeader");

            Assert.That(root.Q<Label>("skill-fitness-level").text, Is.EqualTo("Level 2"));
            Assert.That(root.Q<Label>("skill-fitness-progress").text,
                Is.EqualTo("0 / 100 XP to Level 3"));
            Assert.That(root.Q<Label>("skill-professional-level").text, Is.EqualTo("Level 1"));
            Assert.That(root.Q("skill-fitness").tooltip, Does.Contain("overtime strain"));
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
        public void SocialDiscovery_CreatesPersonAndOpensPersistentDetail()
        {
            Invoke("RefreshSocial");
            Assert.IsTrue(root.Q<Button>("discover-compatible-person").enabledSelf);

            Invoke("DiscoverCompatiblePerson");

            Assert.That(session.ActiveSave.state.relationships, Has.Count.EqualTo(3));
            var discovered = session.ActiveSave.state.relationships[^1];
            Assert.That(discovered.origin, Is.EqualTo("compatible_discovery"));
            Assert.That(root.Q<Label>("relationship-name").text, Is.EqualTo(discovered.displayName));
            Assert.That(root.Q<Label>("relationship-strength").text, Does.Contain("Warmth 50"));
            Assert.That(root.Q<Label>("relationship-genetics").text, Does.Contain("Met through"));
            Assert.IsFalse(root.Q<Button>("discover-compatible-person").enabledSelf);
        }

        [Test]
        public void AgeGatedOptions_DoNotAppearBeforeTheyAreAgeAppropriate()
        {
            session.ActiveSave.state.character.age = 5;
            session.ActiveSave.state.character.lifeStage = StimGameSessionService.GetLifeStage(5);

            Invoke("RefreshEducation");
            Invoke("RefreshCareer");
            Invoke("RefreshMoney");
            Invoke("RefreshSocial");

            Assert.That(root.Q("education-empty-state").Q(className: "st-path-row"), Is.Null);
            Assert.IsTrue(root.Q("career-empty-state").ClassListContains("hidden"));
            Assert.That(root.Q("career-path-preview").childCount, Is.EqualTo(0));
            Assert.IsTrue(root.Q("index-investment-card").ClassListContains("hidden"));
            Assert.IsTrue(root.Q("manual-work-card").ClassListContains("hidden"));
            Assert.IsTrue(root.Q<Button>("bank-tab-credit").ClassListContains("hidden"));
            Assert.IsTrue(root.Q<Button>("bank-tab-investing").ClassListContains("hidden"));
            var setBankTab = typeof(StimVerticalSliceController).GetMethod(
                "SetBankTab", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(setBankTab, Is.Not.Null);
            var investingTab = Enum.Parse(setBankTab.GetParameters()[0].ParameterType, "Investing");
            setBankTab.Invoke(controller, new[] { investingTab });
            Assert.IsTrue(root.Q<Button>("bank-tab-savings").ClassListContains("active"));
            Assert.IsTrue(root.Q("bank-panel-investing").ClassListContains("hidden"));
            Assert.That(root.Q("money-accounts-list").Q("account-row-index-fund"), Is.Null);
            Assert.IsTrue(root.Q<Button>("discover-compatible-person").ClassListContains("hidden"));
            Assert.That(root.Q<Button>("career-action-retire"), Is.Null);

            session.ActiveSave.state.character.age = 18;
            Invoke("RefreshCareer");
            Invoke("RefreshMoney");
            Invoke("RefreshSocial");

            Assert.IsFalse(root.Q("career-empty-state").ClassListContains("hidden"));
            Assert.That(root.Q("career-path-preview").childCount, Is.GreaterThan(0));
            Assert.IsFalse(root.Q("index-investment-card").ClassListContains("hidden"));
            Assert.IsFalse(root.Q<Button>("discover-compatible-person").ClassListContains("hidden"));
            Assert.IsFalse(root.Q<Button>("bank-tab-credit").ClassListContains("hidden"));
            Assert.IsFalse(root.Q<Button>("bank-tab-investing").ClassListContains("hidden"));
            Assert.That(root.Q<Button>("career-action-retire"), Is.Null);

            session.ActiveSave.state.character.age = 65;
            Invoke("RefreshCareer");
            Assert.That(root.Q<Button>("career-action-retire"), Is.Not.Null);
        }

        [Test]
        public void BankTabs_AreExclusiveAndPersistAcrossDestinationNavigation()
        {
            Invoke("ShowMoneyDestination");
            Assert.IsTrue(root.Q<Button>("bank-tab-savings").ClassListContains("active"));
            Assert.IsFalse(root.Q("bank-panel-savings").ClassListContains("hidden"));

            var setBankTab = typeof(StimVerticalSliceController).GetMethod(
                "SetBankTab", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(setBankTab, Is.Not.Null);
            var investing = Enum.Parse(setBankTab.GetParameters()[0].ParameterType, "Investing");
            setBankTab.Invoke(controller, new[] { investing });

            Assert.IsTrue(root.Q<Button>("bank-tab-investing").ClassListContains("active"));
            Assert.IsFalse(root.Q("bank-panel-investing").ClassListContains("hidden"));
            Assert.IsTrue(root.Q("bank-panel-savings").ClassListContains("hidden"));
            Assert.IsTrue(root.Q("bank-panel-credit").ClassListContains("hidden"));

            Invoke("ShowLifeDestination");
            Invoke("ShowMoneyDestination");

            Assert.IsTrue(root.Q<Button>("bank-tab-investing").ClassListContains("active"));
            Assert.IsFalse(root.Q("bank-panel-investing").ClassListContains("hidden"));
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

        [Test]
        public void HomeCard_PreviewsAndRefreshesPersistedActionState()
        {
            var cashBefore = session.ActiveSave.state.finances.cashMinorUnits;
            Invoke("RefreshHeader");

            var read = root.Q<Button>("home-action-read");
            Assert.That(root.Q<Label>("home-condition").text, Does.Contain("Condition 80 / 100"));
            Assert.That(read.text, Does.Contain("$5"));
            Assert.That(read.text, Does.Contain("Learning XP"));

            Invoke("PerformHomeAction", StimHomeActionType.Read);

            Assert.That(session.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(cashBefore - 500));
            Assert.That(root.Q<Label>("home-condition").text, Does.Contain("Condition 79 / 100"));
            Assert.IsFalse(root.Q<Button>("home-action-read").enabledSelf);
            Assert.That(root.Q<Button>("home-action-read").text, Does.Contain("Available next month"));
            Assert.That(root.Q<Label>("home-upgrade-feedback").text, Does.Contain("Read at home"));
        }

        [Test]
        public void MoneyDestination_SavingsTransferRefreshesBalancesAndHistory()
        {
            var cashBefore = session.ActiveSave.state.finances.cashMinorUnits;
            Invoke("ShowMoneyDestination");
            Invoke("PerformSavingsTransfer", 1000L);

            Assert.That(session.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(cashBefore - 1000));
            Assert.That(session.ActiveSave.state.finances.savingsMinorUnits, Is.EqualTo(1000));
            Assert.That(root.Q<Label>("savings-balance-value").text, Is.EqualTo("$10"));
            Assert.That(root.Q<Label>("savings-transfer-feedback").text, Does.Contain("Deposited $10"));
            Assert.That(root.Q("money-transaction-history")
                .Query<VisualElement>(className: "st-money-history-row").ToList(), Has.Count.EqualTo(1));
        }

        [Test]
        public void MoneyDestination_CreditRepaymentRefreshesDebtAndHistory()
        {
            session.ActiveSave.state.finances.debtMinorUnits = 1000;
            session.ActiveSave.state.finances.householdCreditBalanceMinorUnits = 1000;
            session.ActiveSave.state.finances.householdCreditAprBasisPoints = 1800;
            var cashBefore = session.ActiveSave.state.finances.cashMinorUnits;
            Invoke("ShowMoneyDestination");
            Invoke("PerformCreditRepayment", 500L);

            Assert.That(session.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(cashBefore - 500));
            Assert.That(session.ActiveSave.state.finances.debtMinorUnits, Is.EqualTo(500));
            Assert.That(root.Q<Label>("credit-balance-value").text, Is.EqualTo("Balance: $5"));
            Assert.That(root.Q<Label>("credit-repayment-feedback").text, Does.Contain("Repaid $5"));
            Assert.That(root.Q("money-transaction-history")
                .Q<Label>(className: "st-money-history-title").text, Does.Contain("Credit repayment"));
        }

        private void BindControllerFields()
        {
            SetField("gameSession", session);
            var bindings = new Dictionary<string, string>
            {
                { "cashValue", "cash-value" }, { "lifeSummary", "life-summary" },
                { "calendarSummary", "calendar-summary" },
                { "headerNetWorthValue", "header-net-worth-value" },
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
                { "studySessionSheet", "study-session-sheet" },
                { "studySessionTitle", "study-session-title" },
                { "studySessionDescription", "study-session-description" },
                { "studySessionEffects", "study-session-effects" },
                { "studySessionTiming", "study-session-timing" },
                { "studySessionRequirement", "study-session-requirement" },
                { "studySessionCancel", "study-session-cancel" },
                { "studySessionConfirm", "study-session-confirm" },
                { "healthFill", "health-fill" }, { "happinessFill", "happiness-fill" },
                { "smartsFill", "smarts-fill" }, { "looksFill", "looks-fill" },
                { "luckFill", "luck-fill" }, { "advanceMonth", "advance-month" },
                { "advanceYear", "advance-year" },
                { "eventContinue", "event-continue" }, { "focusStudy", "focus-study" },
                { "focusWorkout", "focus-workout" }, { "focusStudyTitle", "focus-study-title" },
                { "focusStudyEffect", "focus-study-effect" }, { "focusWorkoutTitle", "focus-workout-title" },
                { "focusWorkoutEffect", "focus-workout-effect" },
                { "contextActivities", "context-activities" },
                { "homeCondition", "home-condition" }, { "homeProgress", "home-progress" },
                { "homeActions", "home-actions" }, { "homeUpgradeFeedback", "home-upgrade-feedback" },
                { "lifeScroll", "life-scroll" }, { "lifeSummaryView", "life-summary-view" },
                { "openLifeSummary", "open-life-summary" }, { "closeLifeSummary", "close-life-summary" },
                { "addCash", "add-cash" },
                { "summaryStageDetail", "summary-stage-detail" },
                { "summaryCalendarDetail", "summary-calendar-detail" },
                { "summaryCareerDetail", "summary-career-detail" },
                { "summaryHealthValue", "summary-health-value" },
                { "summaryHappinessValue", "summary-happiness-value" },
                { "summarySmartsValue", "summary-smarts-value" },
                { "summaryLooksValue", "summary-looks-value" },
                { "summaryLuckValue", "summary-luck-value" },
                { "summaryHealthFill", "summary-health-fill" },
                { "summaryHappinessFill", "summary-happiness-fill" },
                { "summarySmartsFill", "summary-smarts-fill" },
                { "summaryLooksFill", "summary-looks-fill" },
                { "summaryLuckFill", "summary-luck-fill" },
                { "socialView", "social-view" },
                { "timeDock", "time-dock" }, { "navLife", "nav-life" },
                { "navEducation", "nav-education" }, { "navCareer", "nav-career" },
                { "navSocial", "nav-social" }, { "navGoals", "nav-goals" },
                { "educationView", "education-view" }, { "careerView", "career-view" },
                { "goalsView", "goals-view" },
                { "educationDestinationContent", "education-destination-content" },
                { "careerDestinationContent", "career-destination-content" },
                { "goalsDestinationContent", "goals-destination-content" },
                { "educationEmptyState", "education-empty-state" },
                { "educationUnavailableCopy", "education-unavailable-copy" },
                { "educationCatalog", "education-catalog" },
                { "educationCatalogStatus", "education-catalog-status" },
                { "educationCatalogList", "education-catalog-list" },
                { "careerEmptyState", "career-empty-state" },
                { "careerContextCopy", "career-context-copy" },
                { "careerPathPreview", "career-path-preview" },
                { "relationshipListView", "relationship-list-view" }, { "relationshipList", "relationship-list" },
                { "discoverCompatiblePerson", "discover-compatible-person" },
                { "relationshipDiscoveryFeedback", "relationship-discovery-feedback" },
                { "relationshipDetailView", "relationship-detail-view" },
                { "relationshipBack", "relationship-back" }, { "relationshipAvatar", "relationship-avatar" },
                { "relationshipName", "relationship-name" }, { "relationshipType", "relationship-type" },
                { "relationshipStrength", "relationship-strength" }, { "relationshipFill", "relationship-fill" },
                { "relationshipGenetics", "relationship-genetics" }, { "relationshipActions", "relationship-actions" },
                { "educationCard", "education-card" }, { "educationStage", "education-stage" },
                { "learningLevel", "learning-level" }, { "learningFill", "learning-fill" },
                { "learningProgress", "learning-progress" }, { "educationActions", "education-actions" },
                { "skillsList", "skills-list" },
                { "careerCard", "career-card" }, { "careerRole", "career-role" },
                { "careerSalary", "career-salary" }, { "careerNextStep", "career-next-step" },
                { "careerActionFill", "career-action-fill" },
                { "careerActionProgress", "career-action-progress" }, { "careerActions", "career-actions" },
                { "careerActionsCard", "career-actions-card" },
                { "finalLifeSummary", "final-life-summary" }, { "endingName", "ending-name" },
                { "endingStatus", "ending-status" }, { "endingSummary", "ending-summary" },
                { "endingNewLife", "ending-new-life" }, { "achievementsCount", "achievements-count" },
                { "achievementsList", "achievements-list" }, { "navMoney", "nav-money" },
                { "moneyView", "money-view" }, { "manualWorkRole", "manual-work-role" },
                { "manualWorkRate", "manual-work-rate" }, { "moneyCashValue", "money-cash-value" },
                { "manualWorkTap", "manual-work-tap" }, { "manualWorkFeedback", "manual-work-feedback" },
                { "savingsBalanceValue", "savings-balance-value" },
                { "savingsAvailableValue", "savings-available-value" },
                { "savingsDepositMode", "savings-deposit-mode" },
                { "savingsWithdrawMode", "savings-withdraw-mode" },
                { "savingsAmountInput", "savings-amount-input" },
                { "savingsTransferFeedback", "savings-transfer-feedback" },
                { "moneyTransactionHistory", "money-transaction-history" },
                { "moneyAccountsList", "money-accounts-list" },
                { "cashFlowGross", "cash-flow-gross" }, { "cashFlowTaxes", "cash-flow-taxes" },
                { "cashFlowExpenses", "cash-flow-expenses" },
                { "cashFlowCreditInterest", "cash-flow-credit-interest" },
                { "cashFlowSavingsInterest", "cash-flow-savings-interest" },
                { "cashFlowNet", "cash-flow-net" }, { "savingsProjection", "savings-projection" },
                { "creditBalanceValue", "credit-balance-value" },
                { "creditDetailValue", "credit-detail-value" },
                { "availableCreditValue", "available-credit-value" },
                { "creditRepaymentInput", "credit-repayment-input" },
                { "creditRepaymentFeedback", "credit-repayment-feedback" },
                { "indexFundValue", "index-fund-value" },
                { "indexInvestmentRequirement", "index-investment-requirement" },
                { "indexInvestmentInput", "index-investment-input" },
                { "indexInvestmentFeedback", "index-investment-feedback" },
                { "bankTabSavings", "bank-tab-savings" },
                { "bankTabCredit", "bank-tab-credit" },
                { "bankTabInvesting", "bank-tab-investing" },
                { "bankPanelSavings", "bank-panel-savings" },
                { "bankPanelCredit", "bank-panel-credit" },
                { "bankPanelInvesting", "bank-panel-investing" }
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
