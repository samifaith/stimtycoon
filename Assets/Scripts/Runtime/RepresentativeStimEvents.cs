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
                cooldownYears = 0,
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
    }
}
