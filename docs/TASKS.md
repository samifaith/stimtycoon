# Stim Tycoon Task List

This is the active implementation queue. The master README remains the product definition; this file is the short operational backlog.

## P0 — Finish the playable Life foundation

- [x] Hide the built-in ScrollView scrollbar while retaining touch scrolling.
- [x] Rebuild the playable scene around the approved cozy-corporate Life dashboard.
- [x] Move event decisions and outcomes into an overlay above the dashboard.
- [x] Implement transactional Study and Workout actions with autosave, monthly cooldown, skill XP, and signed stat feedback.
- [x] Run and verify the expanded activity/UI suite; 95 EditMode tests pass as of July 13, 2026.
- [x] Add visible signed change feedback to every monthly action and activity result.
- [x] Consolidate the duplicate Life UI paths; the playable UXML now consumes the reusable header and navigation templates.
- [x] Add structural UI Toolkit tests for required controller bindings, the shared four-tab shell, and default event-sheet state.
- [x] Clamp every visible stat, age, and career-progress fill to 0–100% and cover boundary values.
- [ ] Add controller interaction tests for event presentation, activity feedback, and the persistent Advance Month action.
- [ ] Validate 320, 390, 430, and 768 widths at 100% and 130% font scale.
- [ ] Replace placeholder glyphs with the approved logo, avatar treatment, and licensed icon set.
- [ ] Add the rounded production font and verify fallback/localization coverage.

## P1 — Complete the offline life loop

- [x] Add one-button randomized new-life creation with three starting backgrounds and USA/Jamaica generation.
- [x] Start new lives at birth with two generated genetic parents.
- [x] Add basic life-stage and school-stage progression through monthly aging.
- [x] Add age-appropriate Play, Rest, Study, and Workout focus actions.
- [x] Add Luck-weighted random gains/losses and a dedicated Luck event pool.
- [x] Replace the latest-only feed label with a chronological persisted Life Feed UI.
- [ ] Build complete childhood and education actions around the existing stage state.
- [ ] Add practice actions, skill levels, and visible unlocks.
- [ ] Add parent/relationship list-detail UI and transactional age-appropriate interactions.
- [ ] Add career applications, interviews, job ladders, promotion, quitting, and retirement rules.
- [ ] Add complete-life simulation through health decline, death, and final summary.
- [ ] Run full seeded lives from birth to ending without developer intervention.

## P2 — Money and Business vertical expansion

- [ ] Implement the Money destination with cash flow, debt, net worth, and transaction history.
- [ ] Implement active earning with cooldown/fatigue balance.
- [ ] Implement one complete business type before expanding to three.
- [ ] Add shorter business turns and annual settlement.
- [ ] Add stock/index investing and basic property ownership after business cash flow is stable.

## Release validation

- [ ] Produce the first iOS development build.
- [ ] Validate safe areas and touch targets on physical supported iPhones.
- [ ] Add accessibility settings for font scale and reduced motion.
- [ ] Add Authentication, Game Center, and Cloud Save after offline-life stability.
- [ ] Add LevelPlay only after placement, consent, and child-directed treatment are validated.
