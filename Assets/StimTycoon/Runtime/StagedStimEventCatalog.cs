using System;
using System.Collections.Generic;
using StimTycoon.Events;

namespace StimTycoon.Runtime
{
    /// <summary>
    /// Domain definitions for staged Yarn batches. These definitions are validated independently
    /// and intentionally remain outside CreateLaunchAlphaCatalog until pacing review approves them.
    /// </summary>
    public static class StagedStimEventCatalog
    {
        private sealed class EventSpec
        {
            public EventSpec(string id, int minAge, int maxAge, string firstChoice, string secondChoice)
            {
                this.id = id;
                this.minAge = minAge;
                this.maxAge = maxAge;
                this.firstChoice = firstChoice;
                this.secondChoice = secondChoice;
            }

            public readonly string id;
            public readonly int minAge;
            public readonly int maxAge;
            public readonly string firstChoice;
            public readonly string secondChoice;
        }

        private static readonly EventSpec[] ChildhoodSpecs =
        {
            new EventSpec("childhood_busy_morning_001", 3, 12, "help_look", "stay_calm"),
            new EventSpec("childhood_playground_turn_002", 4, 12, "join_in", "watch_first"),
            new EventSpec("childhood_story_time_request_003", 3, 10, "ask_for_the_story", "listen_quietly"),
            new EventSpec("childhood_shared_crayon_004", 3, 10, "share_it_fairly", "protect_your_turn"),
            new EventSpec("childhood_family_chore_song_005", 4, 12, "join_the_song", "finish_the_task_quietly"),
            new EventSpec("childhood_lunchbox_surprise_006", 5, 12, "ask_about_it", "enjoy_the_treat"),
            new EventSpec("childhood_first_team_game_007", 5, 12, "play_by_the_rules", "make_new_rules"),
            new EventSpec("childhood_rainy_day_window_008", 3, 12, "watch_the_rain", "make_a_project"),
            new EventSpec("childhood_new_shoes_day_009", 3, 12, "say_thank_you", "test_them_immediately"),
            new EventSpec("childhood_sleepy_after_school_010", 5, 12, "rest_first", "keep_going"),
            new EventSpec("childhood_helper_role_011", 4, 12, "help_happily", "ask_how"),
            new EventSpec("childhood_shy_introduction_012", 5, 12, "say_hello_back", "introduce_your_friend_too"),
            new EventSpec("childhood_missing_toy_013", 3, 10, "search_together", "wait_and_remember"),
            new EventSpec("childhood_holiday_song_014", 3, 12, "sing_too", "watch_and_smile"),
            new EventSpec("childhood_community_fair_015", 4, 12, "explore_the_booths", "stay_close_to_family"),
            new EventSpec("childhood_stuffed_animal_vote_016", 3, 9, "cast_a_vote", "make_peace"),
            new EventSpec("childhood_lost_then_found_017", 4, 12, "celebrate_the_find", "learn_the_lesson"),
            new EventSpec("childhood_sleepover_rules_018", 7, 12, "follow_the_rules", "stay_up_laughing"),
            new EventSpec("childhood_small_brave_moment_019", 3, 12, "be_brave", "ask_for_comfort"),
            new EventSpec("childhood_bedtime_story_swap_020", 3, 10, "pick_a_funny_story", "pick_a_calm_story")
        };

        private static readonly EventSpec[] SchoolSpecs =
        {
            new EventSpec("school_cafeteria_lunch_swap_001", 6, 17, "trade_fairly", "be_the_broker"),
            new EventSpec("school_group_project_roles_002", 9, 17, "assign_clear_roles", "volunteer_for_too_much"),
            new EventSpec("school_sub_teacher_energy_003", 6, 17, "help_set_the_tone", "test_the_substitute"),
            new EventSpec("school_science_fair_glowup_004", 8, 17, "explain_it_clearly", "add_extra_flair"),
            new EventSpec("school_club_recruitment_005", 8, 17, "try_the_club", "recruit_your_friend_too"),
            new EventSpec("school_exam_study_group_006", 11, 17, "make_a_study_plan", "wing_it_together"),
            new EventSpec("school_friend_group_chat_007", 11, 17, "cool_it_down", "keep_the_joke_going"),
            new EventSpec("school_dress_code_dilemma_008", 11, 17, "adjust_and_move_on", "question_the_policy"),
            new EventSpec("school_art_room_showcase_009", 6, 17, "share_your_work_proudly", "ask_for_feedback"),
            new EventSpec("school_bus_seat_shuffle_010", 6, 17, "sit_and_say_hello", "wait_for_another_seat"),
            new EventSpec("school_fundraiser_cookie_booth_011", 7, 17, "pitch_politely", "overcommit_to_sales"),
            new EventSpec("school_lost_charger_012", 10, 17, "ask_everyone_calmly", "borrow_a_charger"),
            new EventSpec("school_lunch_table_mix_013", 6, 17, "start_a_conversation", "keep_to_yourself"),
            new EventSpec("school_team_tryout_nerves_014", 8, 17, "try_your_best", "make_a_joke_first"),
            new EventSpec("school_parent_teacher_mixup_015", 6, 17, "clarify_the_details", "let_the_adults_handle_it"),
            new EventSpec("school_assembly_shuffle_016", 6, 17, "pay_attention", "find_the_humor"),
            new EventSpec("school_locker_spill_017", 11, 17, "clean_it_up_fast", "ask_for_help"),
            new EventSpec("school_after_school_job_018", 14, 17, "apply_carefully", "apply_everywhere"),
            new EventSpec("school_classmate_boundaries_019", 10, 17, "set_a_boundary", "keep_explaining"),
            new EventSpec("school_last_period_struggle_020", 8, 17, "focus_and_finish", "ask_to_regroup")
        };

        private static readonly EventSpec[] CareerSpecs =
        {
            new EventSpec("career_first_interview_001", 18, 70, "answer_directly", "overprepare"),
            new EventSpec("career_new_shift_002", 18, 70, "set_boundaries", "say_yes"),
            new EventSpec("career_team_meeting_003", 18, 70, "share_your_idea", "stay_quiet"),
            new EventSpec("career_salary_chat_004", 18, 70, "ask_confidently", "postpone_it"),
            new EventSpec("career_mistake_repair_005", 18, 70, "fix_it_quickly", "ask_for_help"),
            new EventSpec("career_late_night_deadline_006", 18, 70, "finish_now", "pause_and_plan"),
            new EventSpec("career_peer_presentation_007", 18, 70, "present_clearly", "wing_it"),
            new EventSpec("career_small_win_008", 18, 70, "celebrate_it", "keep_moving"),
            new EventSpec("career_feedback_note_009", 18, 70, "ask_for_specifics", "take_the_hint"),
            new EventSpec("career_new_tools_day_010", 18, 70, "learn_them_now", "wait_and_watch"),
            new EventSpec("career_customer_request_011", 18, 70, "stay_calm", "escalate_it"),
            new EventSpec("career_learning_curve_012", 18, 70, "ask_questions", "figure_it_out_alone"),
            new EventSpec("career_after_work_plan_013", 18, 70, "protect_your_time", "take_on_more"),
            new EventSpec("career_cover_shift_014", 18, 70, "help_out", "decline_politely"),
            new EventSpec("career_first_promotion_015", 18, 70, "reach_for_it", "wait_a_season"),
            new EventSpec("career_public_mistake_016", 18, 70, "own_it", "deflect"),
            new EventSpec("career_workshop_signup_017", 18, 70, "sign_up", "skip_it"),
            new EventSpec("career_team_lunch_018", 18, 70, "join_the_lunch", "keep_working"),
            new EventSpec("career_exit_review_019", 18, 70, "review_honestly", "ignore_the_week"),
            new EventSpec("career_skill_upgrade_020", 18, 70, "practice_now", "save_it_for_later")
        };

        private static readonly EventSpec[] HealthSpecs =
        {
            new EventSpec("health_morning_stretch_001", 18, 120, "stretch_gently", "power_through"),
            new EventSpec("health_appointment_reminder_002", 18, 120, "book_the_appointment", "put_it_off"),
            new EventSpec("health_workday_headache_003", 18, 70, "take_a_break", "keep_typing"),
            new EventSpec("health_water_bottle_challenge_004", 18, 120, "hydrate_now", "wait_until_later"),
            new EventSpec("health_sleep_schedule_005", 18, 120, "set_a_bedtime", "scroll_a_bit_more"),
            new EventSpec("health_meal_prep_mixup_006", 18, 120, "eat_what_is_ready", "order_something_else"),
            new EventSpec("health_walk_break_007", 18, 120, "go_outside", "stay_seated"),
            new EventSpec("health_stress_text_008", 18, 120, "respond_calmly", "reply_fast"),
            new EventSpec("health_burnout_warning_009", 18, 70, "slow_down", "push_ahead"),
            new EventSpec("health_community_screening_010", 18, 120, "get_checked", "pass_for_now"),
            new EventSpec("health_family_meal_pressure_011", 18, 120, "keep_your_boundaries", "laugh_it_off"),
            new EventSpec("health_posture_reset_012", 18, 120, "reset_your_posture", "ignore_it"),
            new EventSpec("health_mood_lighten_013", 18, 120, "reach_out", "wait_it_out"),
            new EventSpec("health_pharmacy_run_014", 18, 120, "stay_patient", "leave_and_return_later"),
            new EventSpec("health_rest_day_plan_015", 18, 120, "rest_on_purpose", "fill_it_up"),
            new EventSpec("health_overdue_break_016", 18, 70, "take_the_break", "keep_the_schedule"),
            new EventSpec("health_walk_and_talk_017", 18, 120, "talk_while_walking", "keep_it_light"),
            new EventSpec("health_weekend_stiffness_018", 18, 120, "move_gently", "shrug_it_off"),
            new EventSpec("health_mindful_pause_019", 18, 120, "pause_mindfully", "keep_moving"),
            new EventSpec("health_evening_reset_020", 18, 120, "reset_tonight", "wing_it")
        };

        private static readonly EventSpec[] MoneySpecs =
        {
            new EventSpec("money_side_hustle_pitch_001", 18, 120, "ask_for_numbers", "pass_politely"),
            new EventSpec("money_grocery_budget_002", 18, 120, "shop_with_a_plan", "buy_the_extras"),
            new EventSpec("money_broken_phone_case_003", 18, 120, "replace_it_now", "keep_using_it"),
            new EventSpec("money_shared_lunch_tab_004", 18, 120, "split_it_fairly", "cover_the_table"),
            new EventSpec("money_unexpected_refund_005", 18, 120, "save_it", "use_it_well"),
            new EventSpec("money_commute_cost_006", 18, 120, "adjust_the_route", "pay_and_move_on"),
            new EventSpec("money_family_contribution_007", 18, 120, "contribute_what_you_can", "offer_time_instead"),
            new EventSpec("money_late_fee_reminder_008", 18, 120, "pay_now", "call_and_ask"),
            new EventSpec("money_found_cash_009", 18, 120, "put_it_aside", "treat_yourself"),
            new EventSpec("money_rental_split_010", 18, 120, "split_it_evenly", "split_by_use"),
            new EventSpec("money_small_savings_goal_011", 18, 120, "save_first", "spend_a_little"),
            new EventSpec("money_loan_request_012", 18, 120, "set_terms", "decline_kindly"),
            new EventSpec("money_coupon_confusion_013", 18, 120, "use_it_carefully", "skip_it"),
            new EventSpec("money_fast_choice_014", 18, 120, "wait_and_verify", "put_in_a_little"),
            new EventSpec("money_household_reserve_015", 18, 120, "build_the_reserve", "use_it_now"),
            new EventSpec("money_friend_treat_round_016", 18, 120, "offer_to_split", "let_them_treat"),
            new EventSpec("money_bargain_hunt_017", 18, 120, "compare_prices", "buy_the_nicer_one"),
            new EventSpec("money_backed_out_018", 18, 120, "back_out", "buy_it_anyway"),
            new EventSpec("money_payment_plan_019", 18, 120, "read_the_terms", "sign_now"),
            new EventSpec("money_reset_budget_020", 18, 120, "reset_the_budget", "hope_for_the_best")
        };

        public static IReadOnlyList<StimEvent> CreateChildhoodBatch()
        {
            var events = new List<StimEvent>(ChildhoodSpecs.Length);
            for (var index = 0; index < ChildhoodSpecs.Length; index++)
                events.Add(CreateChildhoodEvent(ChildhoodSpecs[index], index));
            return events;
        }

        public static IReadOnlyList<StimEvent> CreateSchoolBatch()
        {
            var events = new List<StimEvent>(SchoolSpecs.Length);
            for (var index = 0; index < SchoolSpecs.Length; index++)
                events.Add(CreateSchoolEvent(SchoolSpecs[index], index));
            return events;
        }

        public static IReadOnlyList<StimEvent> CreateCareerBatch()
        {
            var events = new List<StimEvent>(CareerSpecs.Length);
            for (var index = 0; index < CareerSpecs.Length; index++)
                events.Add(CreateCareerEvent(CareerSpecs[index], index));
            return events;
        }

        public static IReadOnlyList<StimEvent> CreateHealthBatch()
        {
            var events = new List<StimEvent>(HealthSpecs.Length);
            for (var index = 0; index < HealthSpecs.Length; index++)
                events.Add(CreateHealthEvent(HealthSpecs[index]));
            return events;
        }

        public static IReadOnlyList<StimEvent> CreateMoneyBatch()
        {
            var events = new List<StimEvent>(MoneySpecs.Length);
            for (var index = 0; index < MoneySpecs.Length; index++)
                events.Add(CreateMoneyEvent(MoneySpecs[index], index));
            return events;
        }

        public static IReadOnlyList<StimEvent> CreateAllStagedEvents()
        {
            var events = new List<StimEvent>(100);
            events.AddRange(CreateChildhoodBatch());
            events.AddRange(CreateSchoolBatch());
            events.AddRange(CreateCareerBatch());
            events.AddRange(CreateHealthBatch());
            events.AddRange(CreateMoneyBatch());
            return events;
        }

        private static StimEvent CreateChildhoodEvent(EventSpec spec, int index)
        {
            return new StimEvent
            {
                id = spec.id,
                category = EventCategory.Childhood,
                titleKey = HumanizeEventId(spec.id),
                bodyKey = "A small childhood moment asks you to choose how you respond.",
                toneTags = new List<string> { "grounded", "warm", "age_appropriate", "staged" },
                ageRange = new AgeRange { minAge = spec.minAge, maxAge = spec.maxAge },
                locations = new List<string> { "USA", "Jamaica" },
                requirementsJson = "{}",
                exclusionsJson = "{}",
                cooldownYears = 100,
                repeatPolicy = RepeatPolicy.Never,
                timingPolicy = EventTimingPolicy.AnyMonth,
                monthlyTriggerChance = 0.05f,
                analyticsTags = new List<string> { "childhood", "staged", spec.id },
                choices = new List<Choice>
                {
                    CreateChoice(spec, spec.firstChoice, index % 2 == 0 ? "happiness" : "smarts"),
                    CreateChoice(spec, spec.secondChoice, index % 2 == 0 ? "smarts" : "happiness")
                }
            };
        }

        private static StimEvent CreateSchoolEvent(EventSpec spec, int index)
        {
            return new StimEvent
            {
                id = spec.id,
                category = EventCategory.School,
                titleKey = HumanizeEventId(spec.id),
                bodyKey = "A school-day moment asks you to choose how you respond.",
                toneTags = new List<string> { "grounded", "school", "age_appropriate", "staged" },
                ageRange = new AgeRange { minAge = spec.minAge, maxAge = spec.maxAge },
                locations = new List<string> { "USA", "Jamaica" },
                requirementsJson = "{}",
                exclusionsJson = "{}",
                cooldownYears = 100,
                repeatPolicy = RepeatPolicy.Never,
                timingPolicy = EventTimingPolicy.AnyMonth,
                monthlyTriggerChance = 0.05f,
                analyticsTags = new List<string> { "school", "staged", spec.id },
                choices = new List<Choice>
                {
                    CreateChoice(spec, spec.firstChoice, index % 2 == 0 ? "learning" : "professional", EffectType.SkillXp),
                    CreateChoice(spec, spec.secondChoice, index % 2 == 0 ? "professional" : "learning", EffectType.SkillXp)
                }
            };
        }

        private static StimEvent CreateCareerEvent(EventSpec spec, int index)
        {
            return new StimEvent
            {
                id = spec.id,
                category = EventCategory.Career,
                titleKey = HumanizeEventId(spec.id),
                bodyKey = "A workplace moment asks you to choose how you respond.",
                toneTags = new List<string> { "grounded", "career", "adult", "staged" },
                ageRange = new AgeRange { minAge = spec.minAge, maxAge = spec.maxAge },
                locations = new List<string> { "USA", "Jamaica" },
                requirementsJson = "{}",
                exclusionsJson = "{}",
                cooldownYears = 100,
                repeatPolicy = RepeatPolicy.Never,
                timingPolicy = EventTimingPolicy.AnyMonth,
                monthlyTriggerChance = 0.04f,
                analyticsTags = new List<string> { "career", "staged", spec.id },
                choices = new List<Choice>
                {
                    CreateChoice(spec, spec.firstChoice,
                        index % 2 == 0 ? "performance" : "professional",
                        index % 2 == 0 ? EffectType.CareerProgressDelta : EffectType.SkillXp),
                    CreateChoice(spec, spec.secondChoice,
                        index % 2 == 0 ? "professional" : "performance",
                        index % 2 == 0 ? EffectType.SkillXp : EffectType.CareerProgressDelta)
                }
            };
        }

        private static StimEvent CreateHealthEvent(EventSpec spec)
        {
            return new StimEvent
            {
                id = spec.id,
                category = EventCategory.Health,
                titleKey = HumanizeEventId(spec.id),
                bodyKey = "A health and wellbeing moment asks you to choose how you respond.",
                toneTags = new List<string> { "grounded", "health", "recovery", "staged" },
                ageRange = new AgeRange { minAge = spec.minAge, maxAge = spec.maxAge },
                locations = new List<string> { "USA", "Jamaica" },
                requirementsJson = "{}",
                exclusionsJson = "{}",
                cooldownYears = 100,
                repeatPolicy = RepeatPolicy.Never,
                timingPolicy = EventTimingPolicy.AnyMonth,
                monthlyTriggerChance = 0.04f,
                analyticsTags = new List<string> { "health", "recovery", "staged", spec.id },
                choices = new List<Choice>
                {
                    CreateChoice(spec, spec.firstChoice, "health", EffectType.StatDelta, 1f,
                        OutcomeClassification.Positive),
                    CreateChoice(spec, spec.secondChoice, "health", EffectType.StatDelta, -1f,
                        OutcomeClassification.Negative)
                }
            };
        }

        private static StimEvent CreateMoneyEvent(EventSpec spec, int index)
        {
            return new StimEvent
            {
                id = spec.id,
                category = EventCategory.Money,
                titleKey = HumanizeEventId(spec.id),
                bodyKey = "A practical money moment asks you to choose how you respond.",
                toneTags = new List<string> { "grounded", "money", "adult", "staged" },
                ageRange = new AgeRange { minAge = spec.minAge, maxAge = spec.maxAge },
                locations = new List<string> { "USA", "Jamaica" },
                requirementsJson = "{}",
                exclusionsJson = "{}",
                cooldownYears = 100,
                repeatPolicy = RepeatPolicy.Never,
                timingPolicy = EventTimingPolicy.AnyMonth,
                monthlyTriggerChance = 0.04f,
                analyticsTags = new List<string> { "money", "adult_financial_agency", "staged", spec.id },
                choices = new List<Choice>
                {
                    CreateMoneyChoice(spec, index, true),
                    CreateMoneyChoice(spec, index, false)
                }
            };
        }

        private static Choice CreateMoneyChoice(EventSpec spec, int index, bool first)
        {
            var choiceId = first ? spec.firstChoice : spec.secondChoice;
            if (index == 4 || index == 8)
                return CreateChoice(spec, choiceId, "cash", EffectType.CashDelta,
                    first ? 1000f : 500f, OutcomeClassification.Positive);

            var secondChoiceCost = index == 1 || index == 3 || index == 5 || index == 10 ||
                                   index == 13 || index == 14 || index == 16 || index == 17 || index == 18;
            if (!first && secondChoiceCost)
                return CreateChoice(spec, choiceId, "cash", EffectType.CashDelta, -500f,
                    OutcomeClassification.Negative);

            var firstChoiceCost = index == 2 || index == 6 || index == 7 || index == 15;
            if (first && firstChoiceCost)
                return CreateChoice(spec, choiceId, "cash", EffectType.CashDelta, -500f,
                    OutcomeClassification.Neutral);

            return CreateChoice(spec, choiceId, first ? "smarts" : "happiness");
        }

        private static Choice CreateChoice(
            EventSpec spec,
            string choiceId,
            string targetId,
            EffectType effectType = EffectType.StatDelta,
            float value = 1f,
            OutcomeClassification classification = OutcomeClassification.Positive)
        {
            var label = Humanize(choiceId);
            return new Choice
            {
                id = choiceId,
                labelKey = label,
                requirements = "{}",
                riskPreview = RiskLevel.Safe,
                rewardPreview = RewardLevel.Low,
                baseSuccessChance = 1f,
                outcomes = new List<Outcome>
                {
                    new Outcome
                    {
                        id = choiceId + "_result",
                        classification = classification,
                        resultTextKey = value >= 0f
                            ? label + " helped you through the moment."
                            : label + " left you needing a little more recovery.",
                        feedEntryKey = label + " during a " + GetMomentLabel(spec.id) + ".",
                        weightWithinResultGroup = 1f,
                        telemetryCode = spec.id + "_" + choiceId,
                        effects = new List<Effect>
                        {
                            new Effect
                            {
                                type = effectType,
                                targetId = targetId,
                                value = value,
                                valueRuleId = GetValueRule(effectType, value)
                            }
                        }
                    }
                }
            };
        }

        private static string HumanizeEventId(string eventId)
        {
            var finalSeparator = eventId.LastIndexOf('_');
            var withoutSequence = finalSeparator > 0 ? eventId.Substring(0, finalSeparator) : eventId;
            var prefixSeparator = withoutSequence.IndexOf('_');
            if (prefixSeparator >= 0 && prefixSeparator + 1 < withoutSequence.Length)
                withoutSequence = withoutSequence.Substring(prefixSeparator + 1);
            return Humanize(withoutSequence);
        }

        private static string GetMomentLabel(string eventId)
        {
            if (eventId.StartsWith("childhood_", StringComparison.Ordinal)) return "childhood moment";
            if (eventId.StartsWith("school_", StringComparison.Ordinal)) return "school moment";
            if (eventId.StartsWith("career_", StringComparison.Ordinal)) return "workplace moment";
            if (eventId.StartsWith("health_", StringComparison.Ordinal)) return "health moment";
            if (eventId.StartsWith("money_", StringComparison.Ordinal)) return "money moment";
            return "life moment";
        }

        private static string GetValueRule(EffectType effectType, float value)
        {
            switch (effectType)
            {
                case EffectType.StatDelta:
                    return value < 0f ? StimEffectValueRules.StagedStatLoss :
                        StimEffectValueRules.StagedStatGain;
                case EffectType.SkillXp:
                    return StimEffectValueRules.StagedSkillXpGain;
                case EffectType.CareerProgressDelta:
                    return StimEffectValueRules.StagedCareerProgressGain;
                case EffectType.CashDelta:
                    if (value < 0f) return StimEffectValueRules.StagedCashSmallCost;
                    return value >= 1000f ? StimEffectValueRules.StagedCashMediumGain :
                        StimEffectValueRules.StagedCashSmallGain;
                default:
                    throw new ArgumentOutOfRangeException(nameof(effectType), effectType,
                        "Staged reward effects require a registered balance rule.");
            }
        }

        private static string Humanize(string value)
        {
            var words = value.Replace('_', ' ');
            return words.Length == 0 ? words : char.ToUpperInvariant(words[0]) + words.Substring(1);
        }
    }
}
