using System.Collections.Generic;
using StimTycoon.Events;

namespace StimTycoon.Runtime
{
    public static class RepresentativeStimEvents
    {
        public const string SalaryNegotiationId = "career_salary_negotiation_001";
        public const string HealthBurnoutId = "health_body_asking_for_pause_001";
        public const string MoneyFastReturnId = "money_fast_return_pitch_001";
        public const string SchoolGroupProjectId = "school_group_project_politics_001";
        public const string ChildhoodGrownFolksTableId = "childhood_grown_folks_table_001";
        public const string RandomGainId = "world_random_gain_001";
        public const string RandomLossId = "world_random_loss_001";
        public const string RandomGainRefundId = "world_random_gain_refund_001";
        public const string RandomLossRepairId = "world_random_loss_repair_001";
        public const string LuckCrossroadsId = "world_luck_crossroads_001";
        public const string ChildhoodDiscoveryId = "childhood_small_discovery_001";
        public const string ChildhoodComfortId = "childhood_need_comfort_001";
        public const string PeerTrustConflictId = "relationship_peer_trust_conflict_001";
        public const string PeerTrustAftermathId = "relationship_peer_trust_aftermath_001";
        public const string PeerJealousyId = "relationship_peer_jealousy_001";
        public const string ComingOfAgeGenderId = "coming_of_age_gender_identity_001";
        public const string ComingOfAgeOrientationId = "coming_of_age_orientation_001";
        public const string PromInvitationId = "romance_prom_invitation_001";
        public const string FirstKissId = "romance_first_kiss_001";
        public const string ProposalId = "romance_proposal_001";
        public const string WeddingId = "romance_wedding_001";
        public const string MarriageCrossroadsId = "romance_marriage_crossroads_001";
        public const string YearInReviewId = "time_year_in_review_001";
        public const string HomeDeferredMaintenanceId = "home_deferred_maintenance_001";

        public static IReadOnlyList<StimEvent> CreateLaunchAlphaCatalog()
        {
            return new List<StimEvent>
            {
                CreateHomeDeferredMaintenance(), CreateYearInReview(), CreateSalaryNegotiation(),
                CreateHealthBurnout(), CreateMoneyFastReturn(), CreateSchoolGroupProject(),
                CreateChildhoodGrownFolksTable(), CreateRandomGain(), CreateRandomLoss(),
                CreateRandomGainRefund(), CreateRandomLossRepair(), CreateLuckCrossroads(),
                CreateChildhoodDiscovery(), CreateChildhoodComfort(), CreatePeerTrustConflict(),
                CreatePeerTrustAftermath(), CreatePeerJealousy(), CreateComingOfAgeGender(),
                CreateComingOfAgeOrientation(), CreatePromInvitation(), CreateFirstKiss(),
                CreateProposal(), CreateWedding(), CreateMarriageCrossroads()
            };
        }

        public static StimEvent CreateHomeDeferredMaintenance()
        {
            return new StimEvent
            {
                id = HomeDeferredMaintenanceId,
                category = EventCategory.Money,
                titleKey = "The home needs attention",
                bodyKey = "Small problems have accumulated. Repairing them now costs money, but leaving them unresolved strains the household.",
                toneTags = new List<string> { "grounded", "household", "maintenance" },
                ageRange = new AgeRange { minAge = 0, maxAge = 120 },
                locations = new List<string> { "USA", "Jamaica" },
                requirementsJson = "{\"maximumHomeCondition\":24}",
                cooldownYears = 1,
                repeatPolicy = RepeatPolicy.Repeatable,
                timingPolicy = EventTimingPolicy.AnyMonth,
                monthlyTriggerChance = 1f,
                analyticsTags = new List<string> { "home", "condition", "expense" },
                choices = new List<Choice>
                {
                    new Choice
                    {
                        id = "repair_now", labelKey = "Repair the most urgent problems · $75",
                        riskPreview = RiskLevel.Safe, rewardPreview = RewardLevel.Medium, baseSuccessChance = 1f,
                        outcomes = new List<Outcome>
                        {
                            new Outcome
                            {
                                id = "urgent_repairs_completed", classification = OutcomeClassification.Positive,
                                resultTextKey = "The urgent repairs are complete and the home feels steadier.",
                                feedEntryKey = "Completed urgent home repairs.", telemetryCode = "home_repair_paid",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.CashDelta, targetId = "cash", value = -7500 },
                                    new Effect { type = EffectType.StatDelta, targetId = "home_condition", value = 30 }
                                }
                            }
                        }
                    },
                    new Choice
                    {
                        id = "defer_repairs", labelKey = "Defer the repairs · Happiness −2",
                        riskPreview = RiskLevel.Safe, rewardPreview = RewardLevel.Low, baseSuccessChance = 1f,
                        outcomes = new List<Outcome>
                        {
                            new Outcome
                            {
                                id = "repairs_deferred", classification = OutcomeClassification.Negative,
                                resultTextKey = "The problems remain, and the unresolved strain follows the household.",
                                feedEntryKey = "Deferred urgent home repairs.", telemetryCode = "home_repair_deferred",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = -2 }
                                }
                            }
                        }
                    }
                }
            };
        }

        public static StimEvent CreateYearInReview()
        {
            return new StimEvent
            {
                id = YearInReviewId,
                category = EventCategory.World,
                titleKey = "Year in Review",
                bodyKey = "A full year has passed. Looking at what changed, what will you carry into the year ahead?",
                toneTags = new List<string> { "reflective", "milestone" },
                ageRange = new AgeRange { minAge = 0, maxAge = 120 },
                locations = new List<string> { "USA", "Jamaica" },
                requirementsJson = "{}",
                cooldownYears = 1,
                repeatPolicy = RepeatPolicy.Repeatable,
                timingPolicy = EventTimingPolicy.AnnualRollover,
                monthlyTriggerChance = 1f,
                analyticsTags = new List<string> { "time", "annual_review", "annual_reward" },
                choices = new List<Choice>
                {
                    AnnualFocus("build_security", "Build security · guaranteed $500 reward", "You set aside a modest cushion for the year ahead.", "year_review_security", RewardLevel.Medium),
                    AnnualFocus("invest_in_growth", "Invest in growth · Qualification XP +12 and Learning XP +12", "You commit the year's lesson to your own growth.", "year_review_growth", RewardLevel.Medium),
                    AnnualFocus("nurture_connections", "Nurture connections · closest relationship +6 and household happiness +4", "You choose to make more room for the people around you.", "year_review_connections", RewardLevel.Medium)
                }
            };
        }

        private static Choice AnnualFocus(string id, string label, string result, string telemetry, RewardLevel reward)
        {
            var effects = new List<Effect>();
            switch (id)
            {
                case "build_security":
                    effects.Add(new Effect { type = EffectType.CashDelta, targetId = "cash", value = 50000 });
                    break;
                case "invest_in_growth":
                    effects.Add(new Effect { type = EffectType.SkillXp, targetId = "learning", value = 12 });
                    break;
                case "nurture_connections":
                    effects.Add(new Effect { type = EffectType.RelationshipDelta, targetId = "closest_relationship", value = 6 });
                    break;
            }
            return new Choice
            {
                id = id,
                labelKey = label,
                riskPreview = RiskLevel.Safe,
                rewardPreview = reward,
                baseSuccessChance = 1f,
                modifierRuleIds = new List<string>(),
                outcomes = new List<Outcome>
                {
                    new Outcome
                    {
                        id = id + "_chosen",
                        classification = OutcomeClassification.Positive,
                        resultTextKey = result,
                        feedEntryKey = "Completed the Year in Review.",
                        telemetryCode = telemetry,
                        weightWithinResultGroup = 1f,
                        effects = effects
                    }
                }
            };
        }

        public static StimEvent CreateSalaryNegotiation()
        {
            return new StimEvent
            {
                id = SalaryNegotiationId,
                category = EventCategory.Career,
                titleKey = "The annual review",
                bodyKey = "Your annual review is wrapping up. Your manager asks whether there is anything else you want to discuss.",
                toneTags = new List<string> { "grounded", "direct" },
                ageRange = new AgeRange { minAge = 18, maxAge = 75 },
                locations = new List<string> { "USA", "Jamaica" },
                requirementsJson = "{}",
                cooldownYears = 2,
                repeatPolicy = RepeatPolicy.Repeatable,
                timingPolicy = EventTimingPolicy.AnnualRollover,
                monthlyTriggerChance = 1f,
                analyticsTags = new List<string> { "career", "negotiation" },
                choices = new List<Choice>
                {
                    new Choice
                    {
                        id = "make_the_case",
                        labelKey = "Make the case for a raise",
                        riskPreview = RiskLevel.Moderate,
                        rewardPreview = RewardLevel.High,
                        baseSuccessChance = 0.55f,
                        modifierRuleIds = new List<string> { "skill_negotiation_2" },
                        outcomes = new List<Outcome>
                        {
                            new Outcome
                            {
                                id = "raise_approved",
                                classification = OutcomeClassification.Positive,
                                resultTextKey = "Your manager approves a meaningful raise.",
                                feedEntryKey = "Negotiated a salary increase.",
                                telemetryCode = "salary_raise_approved",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.SalaryDelta, targetId = "annual_salary", value = 500000 },
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 5 }
                                }
                            },
                            new Outcome
                            {
                                id = "raise_declined",
                                classification = OutcomeClassification.Negative,
                                resultTextKey = "Your manager declines and asks you to revisit it next year.",
                                feedEntryKey = "A salary request was declined.",
                                telemetryCode = "salary_raise_declined",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = -4 }
                                }
                            }
                        }
                    },
                    new Choice
                    {
                        id = "let_it_pass",
                        labelKey = "Let it pass for now",
                        riskPreview = RiskLevel.Safe,
                        rewardPreview = RewardLevel.Low,
                        baseSuccessChance = 0.9f,
                        modifierRuleIds = new List<string>(),
                        outcomes = new List<Outcome>
                        {
                            new Outcome
                            {
                                id = "status_quo",
                                classification = OutcomeClassification.Positive,
                                resultTextKey = "You leave the review with your current salary unchanged.",
                                feedEntryKey = "Kept the current salary.",
                                telemetryCode = "salary_status_quo",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 1 }
                                }
                            }
                        }
                    }
                }
            };
        }

        public static StimEvent CreateHealthBurnout()
        {
            return new StimEvent
            {
                id = HealthBurnoutId,
                category = EventCategory.Health,
                titleKey = "Your body is asking for a pause",
                bodyKey = "You have been waking up exhausted, losing focus, and carrying work stress into every evening.",
                toneTags = new List<string> { "grounded", "reflective" },
                ageRange = new AgeRange { minAge = 18, maxAge = 80 },
                locations = new List<string> { "USA", "Jamaica" },
                requirementsJson = "{}",
                cooldownYears = 3,
                repeatPolicy = RepeatPolicy.Repeatable,
                analyticsTags = new List<string> { "health", "burnout" },
                choices = new List<Choice>
                {
                    new Choice
                    {
                        id = "take_a_break",
                        labelKey = "Take a few days to recover",
                        riskPreview = RiskLevel.Safe,
                        rewardPreview = RewardLevel.Medium,
                        baseSuccessChance = 0.85f,
                        outcomes = new List<Outcome>
                        {
                            new Outcome
                            {
                                id = "restored_energy",
                                classification = OutcomeClassification.Positive,
                                resultTextKey = "The break gives your body and mind room to recover.",
                                feedEntryKey = "Took time to recover from burnout.",
                                telemetryCode = "health_burnout_restored",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.StatDelta, targetId = "health", value = 8 },
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 5 }
                                }
                            },
                            new Outcome
                            {
                                id = "rest_was_not_enough",
                                classification = OutcomeClassification.Negative,
                                resultTextKey = "A short break helps, but the exhaustion returns quickly.",
                                feedEntryKey = "Burnout symptoms returned after a short break.",
                                telemetryCode = "health_burnout_persisted",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.StatDelta, targetId = "health", value = -2 }
                                }
                            }
                        }
                    },
                    new Choice
                    {
                        id = "push_through",
                        labelKey = "Push through the exhaustion",
                        riskPreview = RiskLevel.Risky,
                        rewardPreview = RewardLevel.High,
                        baseSuccessChance = 0.4f,
                        outcomes = new List<Outcome>
                        {
                            new Outcome
                            {
                                id = "deadline_met",
                                classification = OutcomeClassification.Positive,
                                resultTextKey = "You meet the deadline, but the pace is not sustainable.",
                                feedEntryKey = "Pushed through burnout to meet a deadline.",
                                telemetryCode = "health_burnout_deadline_met",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.CareerProgressDelta, targetId = "career", value = 10 },
                                    new Effect { type = EffectType.StatDelta, targetId = "health", value = -2 }
                                }
                            },
                            new Outcome
                            {
                                id = "burnout_crash",
                                classification = OutcomeClassification.Negative,
                                resultTextKey = "Your body forces the pause you would not take voluntarily.",
                                feedEntryKey = "Burnout caused a serious crash.",
                                telemetryCode = "health_burnout_crash",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.CareerProgressDelta, targetId = "career", value = -5 },
                                    new Effect { type = EffectType.StatDelta, targetId = "health", value = -12 },
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = -8 }
                                }
                            }
                        }
                    }
                }
            };
        }

        public static StimEvent CreateMoneyFastReturn()
        {
            return new StimEvent
            {
                id = MoneyFastReturnId,
                category = EventCategory.Money,
                titleKey = "The fast return",
                bodyKey = "Someone you know presents an investment that is supposed to pay back quickly. The details are thin, but the confidence is loud.",
                toneTags = new List<string> { "grounded", "tense", "direct" },
                ageRange = new AgeRange { minAge = 18, maxAge = 90 },
                locations = new List<string> { "USA", "Jamaica" },
                requirementsJson = "{}",
                cooldownYears = 4,
                repeatPolicy = RepeatPolicy.Repeatable,
                analyticsTags = new List<string> { "money", "investment", "risk" },
                choices = new List<Choice>
                {
                    new Choice
                    {
                        id = "verify_the_numbers",
                        labelKey = "Ask for documents and verify the numbers",
                        riskPreview = RiskLevel.Safe,
                        rewardPreview = RewardLevel.Medium,
                        baseSuccessChance = 0.85f,
                        modifierRuleIds = new List<string> { "stat_smarts_standard" },
                        outcomes = new List<Outcome>
                        {
                            new Outcome
                            {
                                id = "bad_deal_exposed",
                                classification = OutcomeClassification.Positive,
                                resultTextKey = "The numbers do not survive a closer look. You keep your money and learn what to check next time.",
                                feedEntryKey = "Verified an investment pitch and avoided a bad deal.",
                                telemetryCode = "money_fast_return_verified",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.StatDelta, targetId = "smarts", value = 3 },
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 1 }
                                }
                            },
                            new Outcome
                            {
                                id = "relationship_cools",
                                classification = OutcomeClassification.Negative,
                                resultTextKey = "The person takes your questions personally and the conversation ends cold.",
                                feedEntryKey = "Questioned an investment pitch and created tension.",
                                telemetryCode = "money_fast_return_questions_rejected",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.StatDelta, targetId = "smarts", value = 1 },
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = -2 }
                                }
                            }
                        }
                    },
                    new Choice
                    {
                        id = "invest_small",
                        labelKey = "Invest a small amount",
                        riskPreview = RiskLevel.Risky,
                        rewardPreview = RewardLevel.High,
                        baseSuccessChance = 0.42f,
                        modifierRuleIds = new List<string> { "stat_smarts_standard" },
                        outcomes = new List<Outcome>
                        {
                            new Outcome
                            {
                                id = "small_stake_wins",
                                classification = OutcomeClassification.Positive,
                                resultTextKey = "The small stake pays off before the opportunity disappears.",
                                feedEntryKey = "Made a profitable small speculative investment.",
                                telemetryCode = "money_fast_return_small_win",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.CashDelta, targetId = "cash", value = 50000 },
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 3 }
                                }
                            },
                            new Outcome
                            {
                                id = "small_stake_lost",
                                classification = OutcomeClassification.Negative,
                                resultTextKey = "The promised return never arrives. The loss is contained, but real.",
                                feedEntryKey = "Lost money on a speculative investment.",
                                telemetryCode = "money_fast_return_small_loss",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.CashDelta, targetId = "cash", value = -25000 },
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = -3 },
                                    new Effect { type = EffectType.StatDelta, targetId = "smarts", value = 1 }
                                }
                            }
                        }
                    },
                    new Choice
                    {
                        id = "go_all_in",
                        labelKey = "Go all in",
                        riskPreview = RiskLevel.Extreme,
                        rewardPreview = RewardLevel.Exceptional,
                        baseSuccessChance = 0.18f,
                        modifierRuleIds = new List<string> { "stat_luck_standard" },
                        outcomes = new List<Outcome>
                        {
                            new Outcome
                            {
                                id = "all_in_windfall",
                                classification = OutcomeClassification.Positive,
                                resultTextKey = "The long shot lands and changes your financial position overnight.",
                                feedEntryKey = "Won big on an extreme investment risk.",
                                telemetryCode = "money_fast_return_all_in_win",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.CashDelta, targetId = "cash", value = 200000 },
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 8 }
                                }
                            },
                            new Outcome
                            {
                                id = "all_in_collapse",
                                classification = OutcomeClassification.Negative,
                                resultTextKey = "The money is gone, and covering the loss creates a new debt problem.",
                                feedEntryKey = "Suffered a major loss on an extreme investment risk.",
                                telemetryCode = "money_fast_return_all_in_loss",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.CashDelta, targetId = "cash", value = -100000 },
                                    new Effect { type = EffectType.DebtDelta, targetId = "debt", value = 100000 },
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = -10 },
                                    new Effect { type = EffectType.StatDelta, targetId = "smarts", value = 2 }
                                }
                            }
                        }
                    },
                    new Choice
                    {
                        id = "walk_away",
                        labelKey = "Walk away",
                        riskPreview = RiskLevel.Safe,
                        rewardPreview = RewardLevel.Low,
                        baseSuccessChance = 0.95f,
                        modifierRuleIds = new List<string>(),
                        outcomes = new List<Outcome>
                        {
                            new Outcome
                            {
                                id = "loss_avoided",
                                classification = OutcomeClassification.Positive,
                                resultTextKey = "You leave your money where it is and the pressure passes.",
                                feedEntryKey = "Walked away from a high-pressure investment pitch.",
                                telemetryCode = "money_fast_return_walked_away",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.StatDelta, targetId = "smarts", value = 2 },
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 1 }
                                }
                            },
                            new Outcome
                            {
                                id = "temporary_regret",
                                classification = OutcomeClassification.Negative,
                                resultTextKey = "The deal appears to work for someone else, and doubt follows you home.",
                                feedEntryKey = "Felt temporary regret after declining an investment.",
                                telemetryCode = "money_fast_return_regret",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = -1 },
                                    new Effect { type = EffectType.StatDelta, targetId = "smarts", value = 1 }
                                }
                            }
                        }
                    }
                }
            };
        }

        public static StimEvent CreateSchoolGroupProject()
        {
            return new StimEvent
            {
                id = SchoolGroupProjectId,
                category = EventCategory.School,
                titleKey = "Group project politics",
                bodyKey = "Your group project is due soon. One classmate has gone quiet, another is trying to take over, and the work still needs to get done.",
                toneTags = new List<string> { "grounded", "social", "tense" },
                ageRange = new AgeRange { minAge = 12, maxAge = 18 },
                locations = new List<string> { "USA", "Jamaica" },
                requirementsJson = "{}",
                cooldownYears = 2,
                repeatPolicy = RepeatPolicy.OncePerLifeStage,
                analyticsTags = new List<string> { "school", "teamwork", "accountability" },
                choices = new List<Choice>
                {
                    new Choice
                    {
                        id = "reassign_the_work",
                        labelKey = "Reassign the work and set a deadline",
                        riskPreview = RiskLevel.Moderate,
                        rewardPreview = RewardLevel.High,
                        baseSuccessChance = 0.65f,
                        modifierRuleIds = new List<string> { "stat_smarts_standard" },
                        outcomes = new List<Outcome>
                        {
                            new Outcome
                            {
                                id = "group_pulls_together",
                                classification = OutcomeClassification.Positive,
                                resultTextKey = "The group accepts the plan, finishes the work, and earns a strong grade.",
                                feedEntryKey = "Organized a struggling school project team.",
                                telemetryCode = "school_group_project_led_team",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.StatDelta, targetId = "smarts", value = 4 },
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 2 }
                                }
                            },
                            new Outcome
                            {
                                id = "group_rejects_plan",
                                classification = OutcomeClassification.Negative,
                                resultTextKey = "The deadline becomes another argument, and the project barely comes together.",
                                feedEntryKey = "Struggled to organize a divided project team.",
                                telemetryCode = "school_group_project_plan_rejected",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.StatDelta, targetId = "smarts", value = 1 },
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = -4 }
                                }
                            }
                        }
                    },
                    new Choice
                    {
                        id = "do_it_yourself",
                        labelKey = "Do the missing work yourself",
                        riskPreview = RiskLevel.Safe,
                        rewardPreview = RewardLevel.Medium,
                        baseSuccessChance = 0.78f,
                        modifierRuleIds = new List<string> { "stat_smarts_standard" },
                        outcomes = new List<Outcome>
                        {
                            new Outcome
                            {
                                id = "strong_grade_extra_load",
                                classification = OutcomeClassification.Positive,
                                resultTextKey = "The grade is strong, but carrying the project alone takes something out of you.",
                                feedEntryKey = "Carried a group project to a strong grade.",
                                telemetryCode = "school_group_project_carried",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.StatDelta, targetId = "smarts", value = 3 },
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = -1 }
                                }
                            },
                            new Outcome
                            {
                                id = "work_overwhelms_you",
                                classification = OutcomeClassification.Negative,
                                resultTextKey = "There is too much for one person to fix, and the final result shows it.",
                                feedEntryKey = "Took on too much of a group project alone.",
                                telemetryCode = "school_group_project_overloaded",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.StatDelta, targetId = "smarts", value = 1 },
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = -5 }
                                }
                            }
                        }
                    },
                    new Choice
                    {
                        id = "tell_the_teacher",
                        labelKey = "Tell the teacher exactly what happened",
                        riskPreview = RiskLevel.Moderate,
                        rewardPreview = RewardLevel.Medium,
                        baseSuccessChance = 0.58f,
                        modifierRuleIds = new List<string>(),
                        outcomes = new List<Outcome>
                        {
                            new Outcome
                            {
                                id = "individual_grading",
                                classification = OutcomeClassification.Positive,
                                resultTextKey = "The teacher listens and grades everyone on the work they actually completed.",
                                feedEntryKey = "Asked a teacher for individual project grading.",
                                telemetryCode = "school_group_project_individual_grading",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.StatDelta, targetId = "smarts", value = 2 },
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 2 }
                                }
                            },
                            new Outcome
                            {
                                id = "classmates_resent_report",
                                classification = OutcomeClassification.Negative,
                                resultTextKey = "The teacher gives one final warning, while your classmates decide you broke ranks.",
                                feedEntryKey = "Created tension by reporting a group project problem.",
                                telemetryCode = "school_group_project_report_backfired",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.StatDelta, targetId = "smarts", value = 1 },
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = -3 }
                                }
                            }
                        }
                    }
                }
            };
        }

        public static StimEvent CreateChildhoodGrownFolksTable()
        {
            return new StimEvent
            {
                id = ChildhoodGrownFolksTableId,
                category = EventCategory.Childhood,
                titleKey = "The grown-folks table",
                bodyKey = "Two adults in your household are discussing money at the table. You catch enough to know things are tight.",
                toneTags = new List<string> { "grounded", "warm", "tense" },
                ageRange = new AgeRange { minAge = 7, maxAge = 11 },
                locations = new List<string> { "USA", "Jamaica" },
                requirementsJson = "{}",
                cooldownYears = 2,
                repeatPolicy = RepeatPolicy.OncePerLifeStage,
                analyticsTags = new List<string> { "childhood", "family", "money" },
                choices = new List<Choice>
                {
                    new Choice
                    {
                        id = "ask_whats_going_on",
                        labelKey = "Ask what is going on",
                        riskPreview = RiskLevel.Safe,
                        rewardPreview = RewardLevel.Medium,
                        baseSuccessChance = 0.8f,
                        modifierRuleIds = new List<string> { "stat_smarts_standard" },
                        outcomes = new List<Outcome>
                        {
                            new Outcome
                            {
                                id = "budgeting_explained",
                                classification = OutcomeClassification.Positive,
                                resultTextKey = "One of the adults explains the basics without making the problem yours to carry.",
                                feedEntryKey = "Learned why the household was watching its money.",
                                telemetryCode = "childhood_table_budget_explained",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.StatDelta, targetId = "smarts", value = 3 },
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 1 }
                                }
                            },
                            new Outcome
                            {
                                id = "conversation_gets_tense",
                                classification = OutcomeClassification.Negative,
                                resultTextKey = "The conversation tightens, and you leave feeling responsible for a problem that is not yours.",
                                feedEntryKey = "Felt caught in a tense household money conversation.",
                                telemetryCode = "childhood_table_tension",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.StatDelta, targetId = "smarts", value = 1 },
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = -3 }
                                }
                            }
                        }
                    },
                    new Choice
                    {
                        id = "offer_saved_money",
                        labelKey = "Offer your saved money",
                        riskPreview = RiskLevel.Moderate,
                        rewardPreview = RewardLevel.Medium,
                        baseSuccessChance = 0.6f,
                        modifierRuleIds = new List<string>(),
                        outcomes = new List<Outcome>
                        {
                            new Outcome
                            {
                                id = "savings_protected",
                                classification = OutcomeClassification.Positive,
                                resultTextKey = "They thank you, refuse the money, and remind you that the household is their responsibility.",
                                feedEntryKey = "Offered savings to help the household and was reassured.",
                                telemetryCode = "childhood_table_savings_protected",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 3 },
                                    new Effect { type = EffectType.StatDelta, targetId = "smarts", value = 1 }
                                }
                            },
                            new Outcome
                            {
                                id = "savings_used",
                                classification = OutcomeClassification.Negative,
                                resultTextKey = "A small part of your savings goes into the household budget, and the promise to replace it stays vague.",
                                feedEntryKey = "Used a little saved money to help the household.",
                                telemetryCode = "childhood_table_savings_used",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.CashDelta, targetId = "cash", value = -500 },
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = -4 },
                                    new Effect { type = EffectType.StatDelta, targetId = "smarts", value = 1 }
                                }
                            }
                        }
                    },
                    new Choice
                    {
                        id = "stay_quiet_and_listen",
                        labelKey = "Stay quiet and listen",
                        riskPreview = RiskLevel.Safe,
                        rewardPreview = RewardLevel.Low,
                        baseSuccessChance = 0.75f,
                        modifierRuleIds = new List<string>(),
                        outcomes = new List<Outcome>
                        {
                            new Outcome
                            {
                                id = "learn_from_listening",
                                classification = OutcomeClassification.Positive,
                                resultTextKey = "You understand a little more without being pulled into the pressure.",
                                feedEntryKey = "Listened quietly to a household money conversation.",
                                telemetryCode = "childhood_table_listened",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.StatDelta, targetId = "smarts", value = 2 },
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 1 }
                                }
                            },
                            new Outcome
                            {
                                id = "misunderstood_the_problem",
                                classification = OutcomeClassification.Negative,
                                resultTextKey = "You fill in what you did not understand with something worse than the truth.",
                                feedEntryKey = "Worried after overhearing a household money conversation.",
                                telemetryCode = "childhood_table_worried",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = -2 },
                                    new Effect { type = EffectType.StatDelta, targetId = "smarts", value = 1 }
                                }
                            }
                        }
                    }
                }
            };
        }

        public static StimEvent CreateRandomGain()
        {
            return new StimEvent
            {
                id = RandomGainId,
                category = EventCategory.World,
                titleKey = "A little luck finds you",
                bodyKey = "Something unexpectedly goes your way. It is not life-changing, but it changes the shape of this month.",
                toneTags = new List<string> { "warm", "surprising" },
                ageRange = new AgeRange { minAge = 13, maxAge = 100 },
                locations = new List<string> { "USA", "Jamaica" },
                requirementsJson = "{}",
                cooldownYears = 1,
                repeatPolicy = RepeatPolicy.Repeatable,
                timingPolicy = EventTimingPolicy.AnyMonth,
                monthlyTriggerChance = 0.16f,
                analyticsTags = new List<string> { "world", "random_gain", "luck" },
                choices = new List<Choice>
                {
                    CreateRandomChoice("enjoy_the_gain", "Enjoy the good turn", 0.78f,
                        "The surprise leaves you with a little more money and a lighter mood.", "Enjoyed an unexpected gain.", "random_gain_enjoyed",
                        new Effect { type = EffectType.CashDelta, targetId = "cash", value = 5000 },
                        new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 3 }),
                    CreateRandomChoice("share_the_gain", "Share some of it", 0.72f,
                        "Sharing the moment makes the gain feel bigger than the amount.", "Shared an unexpected gain.", "random_gain_shared",
                        new Effect { type = EffectType.CashDelta, targetId = "cash", value = 2500 },
                        new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 4 })
                }
            };
        }

        public static StimEvent CreateRandomLoss()
        {
            return new StimEvent
            {
                id = RandomLossId,
                category = EventCategory.World,
                titleKey = "An expense you did not plan for",
                bodyKey = "A small but unavoidable cost lands at the wrong time. How you handle it matters more than the surprise itself.",
                toneTags = new List<string> { "grounded", "tense" },
                ageRange = new AgeRange { minAge = 13, maxAge = 100 },
                locations = new List<string> { "USA", "Jamaica" },
                requirementsJson = "{}",
                cooldownYears = 1,
                repeatPolicy = RepeatPolicy.Repeatable,
                timingPolicy = EventTimingPolicy.AnyMonth,
                monthlyTriggerChance = 0.14f,
                analyticsTags = new List<string> { "world", "random_loss", "luck" },
                choices = new List<Choice>
                {
                    CreateRandomChoice("handle_it_now", "Handle it now", 0.7f,
                        "You absorb the cost and keep the problem from growing.", "Covered an unexpected expense.", "random_loss_handled",
                        new Effect { type = EffectType.CashDelta, targetId = "cash", value = -4000 },
                        new Effect { type = EffectType.StatDelta, targetId = "happiness", value = -2 }),
                    CreateRandomChoice("ask_for_help", "Ask for help", 0.62f,
                        "The cost still stings, but support keeps it manageable.", "Asked for help with an unexpected expense.", "random_loss_help",
                        new Effect { type = EffectType.CashDelta, targetId = "cash", value = -2000 },
                        new Effect { type = EffectType.StatDelta, targetId = "smarts", value = 1 },
                        new Effect { type = EffectType.StatDelta, targetId = "happiness", value = -1 }),
                    CreateCertainChoice("accept_the_setback", "Accept the setback",
                        "You cannot cover the expense and live with the consequences for now.", "Could not afford an unexpected expense.", "random_loss_unaffordable",
                        new Effect { type = EffectType.StatDelta, targetId = "happiness", value = -4 })
                }
            };
        }

        public static StimEvent CreateRandomGainRefund()
        {
            return new StimEvent
            {
                id = RandomGainRefundId,
                category = EventCategory.Money,
                titleKey = "Money comes back to you",
                bodyKey = "A corrected charge, forgotten deposit, or small refund puts money back where you did not expect it.",
                toneTags = new List<string> { "grounded", "pleasant" },
                ageRange = new AgeRange { minAge = 13, maxAge = 100 },
                locations = new List<string> { "USA", "Jamaica" },
                requirementsJson = "{}",
                cooldownYears = 1,
                repeatPolicy = RepeatPolicy.Repeatable,
                timingPolicy = EventTimingPolicy.AnyMonth,
                monthlyTriggerChance = 0.12f,
                analyticsTags = new List<string> { "money", "random_gain", "luck" },
                choices = new List<Choice>
                {
                    CreateRandomChoice("save_the_refund", "Put it aside", 0.86f,
                        "You save the returned money and feel slightly more prepared.", "Saved an unexpected refund.", "random_refund_saved",
                        new Effect { type = EffectType.CashDelta, targetId = "cash", value = 7500 },
                        new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 2 }),
                    CreateRandomChoice("use_the_refund", "Use it for something useful", 0.74f,
                        "The refund covers something you had been putting off.", "Used an unexpected refund well.", "random_refund_used",
                        new Effect { type = EffectType.CashDelta, targetId = "cash", value = 3500 },
                        new Effect { type = EffectType.StatDelta, targetId = "health", value = 1 },
                        new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 3 })
                }
            };
        }

        public static StimEvent CreateRandomLossRepair()
        {
            return new StimEvent
            {
                id = RandomLossRepairId,
                category = EventCategory.Money,
                titleKey = "Something important stops working",
                bodyKey = "An everyday item fails without warning. Ignoring it is possible, but inconvenient.",
                toneTags = new List<string> { "grounded", "annoying" },
                ageRange = new AgeRange { minAge = 13, maxAge = 100 },
                locations = new List<string> { "USA", "Jamaica" },
                requirementsJson = "{}",
                cooldownYears = 1,
                repeatPolicy = RepeatPolicy.Repeatable,
                timingPolicy = EventTimingPolicy.AnyMonth,
                monthlyTriggerChance = 0.12f,
                analyticsTags = new List<string> { "money", "random_loss", "luck" },
                choices = new List<Choice>
                {
                    CreateRandomChoice("repair_it", "Pay for the repair", 0.8f,
                        "The repair hurts your cash, but the problem is finished.", "Paid for an unexpected repair.", "random_repair_paid",
                        new Effect { type = EffectType.CashDelta, targetId = "cash", value = -6500 },
                        new Effect { type = EffectType.StatDelta, targetId = "happiness", value = -2 }),
                    CreateRandomChoice("work_around_it", "Work around it for now", 0.68f,
                        "You keep going without spending as much, but the inconvenience follows you.", "Worked around a broken item.", "random_repair_delayed",
                        new Effect { type = EffectType.CashDelta, targetId = "cash", value = -1500 },
                        new Effect { type = EffectType.StatDelta, targetId = "smarts", value = 1 },
                        new Effect { type = EffectType.StatDelta, targetId = "happiness", value = -2 }),
                    CreateCertainChoice("go_without_it", "Go without it",
                        "You cannot afford a repair, so you make do without the item for now.", "Went without something that broke.", "random_repair_unaffordable",
                        new Effect { type = EffectType.StatDelta, targetId = "happiness", value = -4 })
                }
            };
        }

        public static StimEvent CreateLuckCrossroads()
        {
            return new StimEvent
            {
                id = LuckCrossroadsId,
                category = EventCategory.World,
                titleKey = "A strange run of timing",
                bodyKey = "Small coincidences keep lining up around you. You can follow the feeling or let the moment pass.",
                toneTags = new List<string> { "curious", "playful" },
                ageRange = new AgeRange { minAge = 7, maxAge = 100 },
                locations = new List<string> { "USA", "Jamaica" },
                requirementsJson = "{}",
                cooldownYears = 2,
                repeatPolicy = RepeatPolicy.Repeatable,
                timingPolicy = EventTimingPolicy.AnyMonth,
                monthlyTriggerChance = 0.1f,
                analyticsTags = new List<string> { "world", "luck_event" },
                choices = new List<Choice>
                {
                    new Choice
                    {
                        id = "follow_the_hunch",
                        labelKey = "Follow the hunch",
                        riskPreview = RiskLevel.Moderate,
                        rewardPreview = RewardLevel.Medium,
                        baseSuccessChance = 0.6f,
                        modifierRuleIds = new List<string> { "stat_luck_standard" },
                        outcomes = new List<Outcome>
                        {
                            new Outcome
                            {
                                id = "timing_clicks",
                                classification = OutcomeClassification.Positive,
                                resultTextKey = "The timing works, and trusting your instincts feels easier afterward.",
                                feedEntryKey = "Followed a hunch that worked out.", telemetryCode = "luck_hunch_worked", weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.StatDelta, targetId = "luck", value = 4 },
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 2 }
                                }
                            },
                            new Outcome
                            {
                                id = "timing_misses",
                                classification = OutcomeClassification.Negative,
                                resultTextKey = "The pattern was mostly noise, and your confidence in the moment fades.",
                                feedEntryKey = "Followed a hunch that went nowhere.", telemetryCode = "luck_hunch_missed", weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.StatDelta, targetId = "luck", value = -2 },
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = -1 }
                                }
                            }
                        }
                    },
                    CreateRandomChoice("let_it_pass", "Let the moment pass", 0.82f,
                        "You stay grounded and notice the next opportunity more clearly.", "Let a lucky feeling pass.", "luck_moment_passed",
                        new Effect { type = EffectType.StatDelta, targetId = "luck", value = 1 },
                        new Effect { type = EffectType.StatDelta, targetId = "smarts", value = 1 })
                }
            };
        }

        public static StimEvent CreateChildhoodDiscovery()
        {
            return new StimEvent
            {
                id = ChildhoodDiscoveryId,
                category = EventCategory.Childhood,
                titleKey = "Something new catches your attention",
                bodyKey = "A color, sound, object, or tiny detail feels more interesting than everything else nearby.",
                toneTags = new List<string> { "warm", "curious", "age_appropriate" },
                ageRange = new AgeRange { minAge = 0, maxAge = 12 },
                locations = new List<string> { "USA", "Jamaica" },
                requirementsJson = "{}",
                cooldownYears = 1,
                repeatPolicy = RepeatPolicy.Repeatable,
                timingPolicy = EventTimingPolicy.AnyMonth,
                monthlyTriggerChance = 0.18f,
                analyticsTags = new List<string> { "childhood", "random_gain" },
                choices = new List<Choice>
                {
                    CreateRandomChoice("explore_it", "Explore it", 0.82f,
                        "You stay with the discovery and learn something small but real.", "Explored a small new discovery.", "childhood_discovery_explored",
                        new Effect { type = EffectType.StatDelta, targetId = "smarts", value = 2 },
                        new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 2 }),
                    CreateRandomChoice("watch_first", "Watch for a while", 0.88f,
                        "Taking your time helps the unfamiliar thing feel safe.", "Watched something new with curiosity.", "childhood_discovery_watched",
                        new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 2 },
                        new Effect { type = EffectType.StatDelta, targetId = "smarts", value = 1 })
                }
            };
        }

        public static StimEvent CreateChildhoodComfort()
        {
            return new StimEvent
            {
                id = ChildhoodComfortId,
                category = EventCategory.Childhood,
                titleKey = "Today feels harder than usual",
                bodyKey = "You are tired, overwhelmed, or simply out of sorts. A trusted adult notices.",
                toneTags = new List<string> { "gentle", "grounded", "age_appropriate" },
                ageRange = new AgeRange { minAge = 0, maxAge = 12 },
                locations = new List<string> { "USA", "Jamaica" },
                requirementsJson = "{}",
                cooldownYears = 1,
                repeatPolicy = RepeatPolicy.Repeatable,
                timingPolicy = EventTimingPolicy.AnyMonth,
                monthlyTriggerChance = 0.16f,
                analyticsTags = new List<string> { "childhood", "random_loss" },
                choices = new List<Choice>
                {
                    CreateRandomChoice("reach_for_comfort", "Stay close to someone you trust", 0.8f,
                        "The difficult feeling eases, though the day still takes something out of you.", "Found comfort during a hard day.", "childhood_comfort_received",
                        new Effect { type = EffectType.StatDelta, targetId = "happiness", value = -1 },
                        new Effect { type = EffectType.StatDelta, targetId = "health", value = 1 }),
                    CreateRandomChoice("take_quiet_time", "Take some quiet time", 0.74f,
                        "The quiet helps you settle and understand what you needed.", "Took quiet time during a hard day.", "childhood_comfort_quiet",
                        new Effect { type = EffectType.StatDelta, targetId = "happiness", value = -1 },
                        new Effect { type = EffectType.StatDelta, targetId = "smarts", value = 1 })
                }
            };
        }

        public static StimEvent CreatePeerTrustConflict()
        {
            return new StimEvent
            {
                id = PeerTrustConflictId,
                category = EventCategory.Relationship,
                titleKey = "A confidence under pressure",
                bodyKey = "A school friend trusts you with something private. Another student pushes you to share it.",
                toneTags = new List<string> { "friendship", "drama", "trust", "age_appropriate" },
                ageRange = new AgeRange { minAge = 10, maxAge = 17 },
                locations = new List<string> { "USA", "Jamaica" },
                requirementsJson = "{\"relationshipId\":\"school_peer_primary\"}",
                cooldownYears = 0,
                repeatPolicy = RepeatPolicy.Never,
                timingPolicy = EventTimingPolicy.AnyMonth,
                monthlyTriggerChance = 0.28f,
                analyticsTags = new List<string> { "relationship", "friendship_drama" },
                choices = new List<Choice>
                {
                    CreateDramaChoice("keep_confidence", "Keep your friend's confidence",
                        "You protect the confidence even when the social pressure gets uncomfortable.",
                        "Protected a friend's trust under pressure.", "peer_trust_protected", 8, 2),
                    CreateDramaChoice("stay_silent", "Stay out of it",
                        "Your silence avoids the confrontation, but your friend notices you did not stand beside them.",
                        "Stayed silent during a friend's difficult moment.", "peer_trust_silent", -5, -2),
                    CreateDramaChoice("share_secret", "Share it to gain attention",
                        "The secret spreads quickly, and your friend realizes where it started.",
                        "Betrayed a friend's confidence for attention.", "peer_trust_betrayed", -18, 1)
                }
            };
        }

        public static StimEvent CreatePeerTrustAftermath()
        {
            return new StimEvent
            {
                id = PeerTrustAftermathId,
                category = EventCategory.Relationship,
                titleKey = "The trust conversation returns",
                bodyKey = "Time has passed, but the unresolved moment between you and your school friend comes up again.",
                toneTags = new List<string> { "friendship", "follow_up", "reconciliation" },
                ageRange = new AgeRange { minAge = 11, maxAge = 21 },
                locations = new List<string> { "USA", "Jamaica" },
                requirementsJson = "{\"relationshipId\":\"school_peer_primary\"}",
                cooldownYears = 0,
                repeatPolicy = RepeatPolicy.Never,
                timingPolicy = EventTimingPolicy.AnyMonth,
                monthlyTriggerChance = 0.01f,
                analyticsTags = new List<string> { "relationship", "friendship_follow_up" },
                choices = new List<Choice>
                {
                    CreateCertainChoice("talk_honestly", "Talk honestly and repair things",
                        "An honest conversation gives the friendship room to recover.",
                        "Had an honest conversation about broken trust.", "peer_aftermath_repair",
                        new Effect { type = EffectType.RelationshipDelta, targetId = "school_peer_primary", value = 10 }),
                    CreateCertainChoice("compete_instead", "Turn the tension into competition",
                        "The tension becomes a rivalry that pushes both of you harder.",
                        "Turned unresolved tension into competition.", "peer_aftermath_compete",
                        new Effect { type = EffectType.RelationshipDelta, targetId = "school_peer_primary", value = -6 },
                        new Effect { type = EffectType.StatDelta, targetId = "smarts", value = 2 }),
                    CreateCertainChoice("walk_away", "Walk away from the friendship",
                        "You decide the friendship no longer belongs in your life.",
                        "Walked away from a damaged friendship.", "peer_aftermath_walked_away",
                        new Effect { type = EffectType.RelationshipDelta, targetId = "school_peer_primary", value = -15 },
                        new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 1 })
                }
            };
        }

        public static StimEvent CreatePeerJealousy()
        {
            return new StimEvent
            {
                id = PeerJealousyId,
                category = EventCategory.Relationship,
                titleKey = "Pulled between two friends",
                bodyKey = "Your longtime friend feels replaced by someone you met more recently. Both are watching how you handle it.",
                toneTags = new List<string> { "friendship", "jealousy", "social_group", "age_appropriate" },
                ageRange = new AgeRange { minAge = 13, maxAge = 17 },
                locations = new List<string> { "USA", "Jamaica" },
                requirementsJson = "{\"relationshipId\":\"school_peer_primary\",\"secondaryRelationshipId\":\"school_peer_middle\"}",
                cooldownYears = 0,
                repeatPolicy = RepeatPolicy.Never,
                timingPolicy = EventTimingPolicy.AnyMonth,
                monthlyTriggerChance = 0.22f,
                analyticsTags = new List<string> { "relationship", "jealousy", "friendship_drama" },
                choices = new List<Choice>
                {
                    CreateCertainChoice("bring_them_together", "Bring both friends together",
                        "You acknowledge the jealousy without choosing sides, giving the group a chance to reset.",
                        "Addressed jealousy without abandoning either friend.", "peer_jealousy_inclusive",
                        new Effect { type = EffectType.RelationshipDelta, targetId = "school_peer_primary", value = 5 },
                        new Effect { type = EffectType.RelationshipDelta, targetId = "school_peer_middle", value = 5 },
                        new Effect { type = EffectType.SkillXp, targetId = "social", value = 8 }),
                    CreateCertainChoice("choose_old_friend", "Reassure your longtime friend",
                        "Your longtime friend feels secure again, but the newer friendship takes the rejection personally.",
                        "Chose a longtime friend during a jealous conflict.", "peer_jealousy_old_friend",
                        new Effect { type = EffectType.RelationshipDelta, targetId = "school_peer_primary", value = 10 },
                        new Effect { type = EffectType.RelationshipDelta, targetId = "school_peer_middle", value = -10 }),
                    CreateCertainChoice("choose_new_friend", "Stand with your newer friend",
                        "The newer bond grows stronger while your longtime friend feels pushed aside.",
                        "Chose a newer friend during a jealous conflict.", "peer_jealousy_new_friend",
                        new Effect { type = EffectType.RelationshipDelta, targetId = "school_peer_primary", value = -12 },
                        new Effect { type = EffectType.RelationshipDelta, targetId = "school_peer_middle", value = 10 }),
                    CreateCertainChoice("enjoy_attention", "Keep them competing for your attention",
                        "The attention feels exciting briefly, but both friends lose trust in you.",
                        "Encouraged two friends to compete for attention.", "peer_jealousy_manipulated",
                        new Effect { type = EffectType.RelationshipDelta, targetId = "school_peer_primary", value = -10 },
                        new Effect { type = EffectType.RelationshipDelta, targetId = "school_peer_middle", value = -10 },
                        new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 2 })
                }
            };
        }

        public static StimEvent CreateComingOfAgeGender()
        {
            var evt = new StimEvent
            {
                id = ComingOfAgeGenderId,
                category = EventCategory.Relationship,
                titleKey = "Becoming more yourself",
                bodyKey = "As you grow into yourself, you have space to describe your gender identity. There is no wrong answer, and it can evolve later.",
                toneTags = new List<string> { "reflective", "supportive", "identity" },
                ageRange = new AgeRange { minAge = 16, maxAge = 16 },
                locations = new List<string> { "USA", "Jamaica" },
                requirementsJson = "{}",
                cooldownYears = 100,
                repeatPolicy = RepeatPolicy.Never,
                timingPolicy = EventTimingPolicy.AnnualRollover,
                monthlyTriggerChance = 1f,
                analyticsTags = new List<string> { "coming_of_age", "identity" },
                choices = new List<Choice>
                {
                    CreateIdentityChoice("identify_woman", "I am a woman", "You chose to describe yourself as a woman."),
                    CreateIdentityChoice("identify_man", "I am a man", "You chose to describe yourself as a man."),
                    CreateIdentityChoice("identify_nonbinary", "I am nonbinary", "You chose to describe yourself as nonbinary."),
                    CreateIdentityChoice("still_questioning_gender", "I am still figuring it out", "You gave yourself time to keep exploring your gender identity.")
                }
            };
            foreach (var choice in evt.choices)
            {
                choice.outcomes[0].followUps.Add(new ScheduledEventRef
                {
                    eventId = ComingOfAgeOrientationId,
                    minYearsFromNow = 1,
                    maxYearsFromNow = 1,
                    probability = 1f,
                    cancellationRule = "life_ended"
                });
            }
            return evt;
        }

        public static StimEvent CreateComingOfAgeOrientation()
        {
            var evt = new StimEvent
            {
                id = ComingOfAgeOrientationId,
                category = EventCategory.Relationship,
                titleKey = "Understanding attraction",
                bodyKey = "You are learning more about who you are drawn to. Choose the description that feels closest right now; identity can evolve later.",
                toneTags = new List<string> { "reflective", "supportive", "identity" },
                ageRange = new AgeRange { minAge = 17, maxAge = 17 },
                locations = new List<string> { "USA", "Jamaica" },
                requirementsJson = "{}",
                cooldownYears = 100,
                repeatPolicy = RepeatPolicy.Never,
                timingPolicy = EventTimingPolicy.AnnualRollover,
                monthlyTriggerChance = 1f,
                analyticsTags = new List<string> { "coming_of_age", "orientation" },
                choices = new List<Choice>
                {
                    CreateIdentityChoice("orientation_straight", "Straight", "You chose straight as the description that fits you."),
                    CreateIdentityChoice("orientation_gay_lesbian", "Gay or lesbian", "You chose gay or lesbian as the description that fits you."),
                    CreateIdentityChoice("orientation_bisexual", "Bisexual", "You chose bisexual as the description that fits you."),
                    CreateIdentityChoice("orientation_pansexual", "Pansexual", "You chose pansexual as the description that fits you."),
                    CreateIdentityChoice("orientation_asexual", "Asexual", "You chose asexual as the description that fits you."),
                    CreateIdentityChoice("still_questioning_orientation", "Still questioning", "You gave yourself time to keep understanding your orientation.")
                }
            };
            foreach (var choice in evt.choices)
            {
                choice.outcomes[0].followUps.Add(new ScheduledEventRef
                {
                    eventId = PromInvitationId,
                    minYearsFromNow = 1,
                    maxYearsFromNow = 1,
                    probability = 1f,
                    cancellationRule = "school_peer_primary_missing_or_not_close"
                });
            }
            return evt;
        }

        public static StimEvent CreatePromInvitation()
        {
            var attendTogether = CreateCertainChoice(
                "attend_prom_together", "Go to prom as a date",
                "You and a close friend went to prom together and began dating.",
                "Went to prom with a close friend and started dating.", "prom_date",
                new Effect { type = EffectType.RelationshipDelta, targetId = "school_peer_primary", value = 15 },
                new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 5 });
            attendTogether.outcomes[0].followUps.Add(new ScheduledEventRef
            {
                eventId = FirstKissId,
                minYearsFromNow = 1,
                maxYearsFromNow = 1,
                probability = 1f,
                cancellationRule = "school_peer_primary_missing"
            });
            return new StimEvent
            {
                id = PromInvitationId,
                category = EventCategory.Relationship,
                titleKey = "A prom invitation",
                bodyKey = "A close friend asks how you would feel about going to prom together. You can make it romantic, keep it friendly, or decline.",
                toneTags = new List<string> { "romance", "coming_of_age", "choice" },
                ageRange = new AgeRange { minAge = 18, maxAge = 18 },
                locations = new List<string> { "USA", "Jamaica" },
                requirementsJson = "{\"relationshipId\":\"school_peer_primary\",\"minimumRelationshipValue\":60}",
                cooldownYears = 100,
                repeatPolicy = RepeatPolicy.Never,
                timingPolicy = EventTimingPolicy.AnnualRollover,
                monthlyTriggerChance = 1f,
                analyticsTags = new List<string> { "romance", "prom" },
                choices = new List<Choice>
                {
                    attendTogether,
                    CreateCertainChoice("attend_prom_as_friends", "Go as friends",
                        "You and your friend shared a fun prom night without making it romantic.",
                        "Went to prom with a close friend as friends.", "prom_friends",
                        new Effect { type = EffectType.RelationshipDelta, targetId = "school_peer_primary", value = 8 },
                        new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 3 }),
                    CreateCertainChoice("decline_prom", "Politely decline",
                        "You politely declined and spent prom night your own way.",
                        "Declined a prom invitation.", "prom_declined",
                        new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 1 })
                }
            };
        }

        public static StimEvent CreateFirstKiss()
        {
            return new StimEvent
            {
                id = FirstKissId,
                category = EventCategory.Relationship,
                titleKey = "A quiet moment together",
                bodyKey = "After another meaningful date, you and your partner pause close together. You decide what feels comfortable.",
                toneTags = new List<string> { "romance", "consent", "milestone" },
                ageRange = new AgeRange { minAge = 19, maxAge = 19 },
                locations = new List<string> { "USA", "Jamaica" },
                requirementsJson = "{\"relationshipId\":\"school_peer_primary\",\"relationshipType\":\"dating\"}",
                cooldownYears = 100,
                repeatPolicy = RepeatPolicy.Never,
                timingPolicy = EventTimingPolicy.AnnualRollover,
                monthlyTriggerChance = 1f,
                analyticsTags = new List<string> { "romance", "first_kiss", "consent" },
                choices = new List<Choice>
                {
                    CreateCertainChoice("share_first_kiss", "Kiss Taylor",
                        "You both leaned in and shared your first kiss.",
                        "Shared a first kiss with a partner.", "first_kiss_shared",
                        new Effect { type = EffectType.RelationshipDelta, targetId = "school_peer_primary", value = 10 },
                        new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 5 }),
                    CreateCertainChoice("wait_to_kiss", "Say you want to wait",
                        "Your partner respected your boundary, and the honest moment brought you closer.",
                        "Chose to wait before a first kiss.", "first_kiss_waited",
                        new Effect { type = EffectType.RelationshipDelta, targetId = "school_peer_primary", value = 5 },
                        new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 2 })
                }
            };
        }

        public static StimEvent CreateProposal()
        {
            var propose = CreateCertainChoice("propose_marriage", "Propose marriage",
                "Your partner said yes, and you became engaged.",
                "Proposed marriage and became engaged.", "proposal_accepted",
                new Effect { type = EffectType.RelationshipDelta, targetId = "school_peer_primary", value = 5 },
                new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 6 });
            propose.outcomes[0].followUps.Add(new ScheduledEventRef
            {
                eventId = WeddingId,
                minYearsFromNow = 1,
                maxYearsFromNow = 1,
                probability = 1f,
                cancellationRule = "engagement_ended"
            });
            return new StimEvent
            {
                id = ProposalId,
                category = EventCategory.Relationship,
                titleKey = "A shared future",
                bodyKey = "Your partnership feels strong enough to consider marriage. You decide whether this is the right time.",
                toneTags = new List<string> { "romance", "commitment", "adult" },
                ageRange = new AgeRange { minAge = 24, maxAge = 65 },
                locations = new List<string> { "USA", "Jamaica" },
                requirementsJson = "{\"relationshipId\":\"school_peer_primary\",\"relationshipType\":\"partner\",\"minimumRelationshipValue\":80}",
                cooldownYears = 100,
                repeatPolicy = RepeatPolicy.Never,
                timingPolicy = EventTimingPolicy.AnnualRollover,
                monthlyTriggerChance = 1f,
                analyticsTags = new List<string> { "romance", "proposal" },
                choices = new List<Choice>
                {
                    propose,
                    CreateCertainChoice("wait_on_proposal", "Wait until it feels right",
                        "You chose not to rush marriage, and your partner respected the conversation.",
                        "Chose to wait before proposing.", "proposal_waited",
                        new Effect { type = EffectType.RelationshipDelta, targetId = "school_peer_primary", value = 2 },
                        new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 1 }),
                    CreateCertainChoice("end_partnership", "Admit the relationship should end",
                        "You ended the partnership instead of promising a future you did not want.",
                        "Ended a long-term partnership.", "proposal_breakup",
                        new Effect { type = EffectType.RelationshipDelta, targetId = "school_peer_primary", value = -20 },
                        new Effect { type = EffectType.StatDelta, targetId = "happiness", value = -6 })
                }
            };
        }

        public static StimEvent CreateWedding()
        {
            var evt = new StimEvent
            {
                id = WeddingId,
                category = EventCategory.Relationship,
                titleKey = "The wedding day",
                bodyKey = "The wedding day arrives. You can marry, postpone together, or call it off before making the commitment.",
                toneTags = new List<string> { "romance", "marriage", "choice" },
                ageRange = new AgeRange { minAge = 25, maxAge = 66 },
                locations = new List<string> { "USA", "Jamaica" },
                requirementsJson = "{\"relationshipId\":\"school_peer_primary\",\"relationshipType\":\"engaged\"}",
                cooldownYears = 1,
                repeatPolicy = RepeatPolicy.Repeatable,
                timingPolicy = EventTimingPolicy.AnnualRollover,
                monthlyTriggerChance = 1f,
                analyticsTags = new List<string> { "romance", "wedding", "marriage" },
                choices = new List<Choice>
                {
                    CreateCertainChoice("get_married", "Get married",
                        "You exchanged vows and began married life together.",
                        "Got married.", "wedding_married",
                        new Effect { type = EffectType.RelationshipDelta, targetId = "school_peer_primary", value = 5 },
                        new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 8 }),
                    CreateCertainChoice("postpone_wedding", "Postpone together",
                        "You mutually postponed the wedding while remaining engaged.",
                        "Postponed the wedding together.", "wedding_postponed",
                        new Effect { type = EffectType.RelationshipDelta, targetId = "school_peer_primary", value = 1 },
                        new Effect { type = EffectType.StatDelta, targetId = "happiness", value = -1 }),
                    CreateCertainChoice("call_off_wedding", "Call off the wedding",
                        "You called off the wedding and ended the relationship.",
                        "Called off a wedding.", "wedding_cancelled",
                        new Effect { type = EffectType.RelationshipDelta, targetId = "school_peer_primary", value = -25 },
                        new Effect { type = EffectType.StatDelta, targetId = "happiness", value = -8 })
                }
            };
            evt.choices[1].outcomes[0].followUps.Add(new ScheduledEventRef
            {
                eventId = WeddingId,
                minYearsFromNow = 1,
                maxYearsFromNow = 1,
                probability = 1f,
                cancellationRule = "engagement_ended"
            });
            return evt;
        }

        public static StimEvent CreateMarriageCrossroads()
        {
            return new StimEvent
            {
                id = MarriageCrossroadsId,
                category = EventCategory.Relationship,
                titleKey = "A marriage at a crossroads",
                bodyKey = "Distance has built between you and your spouse. You decide whether to repair the marriage, separate, or divorce.",
                toneTags = new List<string> { "marriage", "conflict", "adult" },
                ageRange = new AgeRange { minAge = 25, maxAge = 85 },
                locations = new List<string> { "USA", "Jamaica" },
                requirementsJson = "{\"relationshipId\":\"school_peer_primary\",\"relationshipType\":\"married\",\"maximumRelationshipValue\":55}",
                cooldownYears = 2,
                repeatPolicy = RepeatPolicy.Repeatable,
                timingPolicy = EventTimingPolicy.AnnualRollover,
                monthlyTriggerChance = 1f,
                analyticsTags = new List<string> { "romance", "marriage", "conflict" },
                choices = new List<Choice>
                {
                    CreateCertainChoice("seek_counseling", "Seek counseling",
                        "You invested in counseling and began rebuilding trust together.",
                        "Started marriage counseling.", "marriage_counseling",
                        new Effect { type = EffectType.RelationshipDelta, targetId = "school_peer_primary", value = 12 },
                        new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 2 }),
                    CreateCertainChoice("recommit", "Recommit to each other",
                        "You had an honest conversation and recommitted to the marriage.",
                        "Recommitted to a strained marriage.", "marriage_recommit",
                        new Effect { type = EffectType.RelationshipDelta, targetId = "school_peer_primary", value = 8 },
                        new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 1 }),
                    CreateCertainChoice("separate", "Separate for now",
                        "You separated and ended the active partnership without filing for divorce.",
                        "Separated from a spouse.", "marriage_separated",
                        new Effect { type = EffectType.RelationshipDelta, targetId = "school_peer_primary", value = -10 },
                        new Effect { type = EffectType.StatDelta, targetId = "happiness", value = -5 }),
                    CreateCertainChoice("divorce", "File for divorce",
                        "The marriage ended in divorce, including legal fees and an asset settlement.",
                        "Divorced a spouse.", "marriage_divorced",
                        new Effect { type = EffectType.RelationshipDelta, targetId = "school_peer_primary", value = -20 },
                        new Effect { type = EffectType.StatDelta, targetId = "happiness", value = -8 })
                }
            };
        }

        private static Choice CreateIdentityChoice(string id, string label, string result)
        {
            return CreateCertainChoice(id, label, result, result, id,
                new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 1 });
        }

        private static Choice CreateDramaChoice(
            string id, string label, string result, string feed, string telemetry,
            int relationshipDelta, int happinessDelta)
        {
            var choice = CreateCertainChoice(id, label, result, feed, telemetry,
                new Effect { type = EffectType.RelationshipDelta, targetId = "school_peer_primary", value = relationshipDelta },
                new Effect { type = EffectType.StatDelta, targetId = "happiness", value = happinessDelta });
            choice.outcomes[0].followUps.Add(new ScheduledEventRef
            {
                eventId = PeerTrustAftermathId,
                minYearsFromNow = 1,
                maxYearsFromNow = 2,
                probability = 1f,
                cancellationRule = "school_peer_primary_missing"
            });
            return choice;
        }

        private static Choice CreateCertainChoice(
            string id, string label, string result, string feed, string telemetry,
            params Effect[] effects)
        {
            return new Choice
            {
                id = id,
                labelKey = label,
                riskPreview = RiskLevel.Safe,
                rewardPreview = RewardLevel.Medium,
                baseSuccessChance = 1f,
                modifierRuleIds = new List<string>(),
                outcomes = new List<Outcome>
                {
                    new Outcome
                    {
                        id = id + "_result",
                        classification = OutcomeClassification.Neutral,
                        resultTextKey = result,
                        feedEntryKey = feed,
                        telemetryCode = telemetry,
                        weightWithinResultGroup = 1f,
                        effects = new List<Effect>(effects)
                    }
                }
            };
        }

        private static Choice CreateRandomChoice(
            string id,
            string label,
            float successChance,
            string result,
            string feed,
            string telemetry,
            params Effect[] effects)
        {
            return new Choice
            {
                id = id,
                labelKey = label,
                riskPreview = successChance >= 0.7f ? RiskLevel.Safe : RiskLevel.Moderate,
                rewardPreview = successChance >= 0.7f ? RewardLevel.Low : RewardLevel.Medium,
                baseSuccessChance = successChance,
                modifierRuleIds = new List<string> { "stat_luck_standard" },
                outcomes = new List<Outcome>
                {
                    new Outcome
                    {
                        id = id + "_result",
                        classification = effects.Length > 0 && effects[0].value < 0
                            ? OutcomeClassification.Negative
                            : OutcomeClassification.Positive,
                        resultTextKey = result,
                        feedEntryKey = feed,
                        telemetryCode = telemetry,
                        weightWithinResultGroup = 1f,
                        effects = new List<Effect>(effects)
                    }
                }
            };
        }
    }
}
