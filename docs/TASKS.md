# Stim Tycoon — Next Task List

This is the active queue after the user-verified 140-test Phase 1 run. The master README remains the product definition. Completed implementation history is summarized below instead of mixed into the active queue.

## P0 — Keep the verified loop fast and shippable

- [ ] Mark the birth-to-ending harness as slow simulation coverage and make it separately selectable in the Unity Test Runner.
- [ ] Measure the harness duration, simulated months, transaction count, largest serialized save, and final Life Feed size.
- [ ] Reduce redundant serialization in simulation-only coverage without weakening focused atomic-save, rollback, migration, corruption, and recovery tests.
- [ ] Keep the native atomic JSON repository through the first physical-device profiling pass.
- [ ] Validate Life, Social, education, career, achievements, event overlays, new-life setup, and ending summary at 320, 390, 430, and 768 widths.
- [ ] Repeat the layout pass at 100% and 130% text scale; fix clipping, wrapping, scroll reachability, and persistent-action overlap.
- [ ] Produce the first iOS development build.
- [ ] Profile save/load latency, save size, memory, safe areas, and touch targets on a physical supported iPhone.
- [ ] Evaluate MessagePack behind `IStimSaveRepository` only if device profiling shows unacceptable JSON latency or file size.

## P1 — Expand Phase 2 life depth

- [ ] Add explicit enrollment decisions and stage-specific school choices for primary, middle, and high school.
- [ ] Add at least two skill paths beyond Learning, with XP thresholds and visible action/event unlocks.
- [ ] Add peers and friendship relationship records, list/detail presentation, age-appropriate actions, and events.
- [ ] Add longer relationship arcs and consequences before implementing romance.
- [ ] Expand health gameplay with checkups, treatment, recovery, chronic statuses, and player choices before late-life decline.
- [ ] Add more career industries and authored interview uncertainty while preserving the transactional ladder boundary.
- [ ] Expand achievement definitions across education, relationships, careers, wealth, health, and alternate endings.

## P2 — Money destination

- [ ] Implement the Money destination using the existing four-tab shell.
- [ ] Show monthly gross income, taxes, expenses, debt pressure, net cash flow, net worth, and persisted transaction history.
- [ ] Add active earning with cooldown/fatigue balance and signed feedback.
- [ ] Add debt repayment and emergency-expense decisions before investing.
- [ ] Add stock/index investing only after cash-flow and transaction-history behavior is stable.

## P3 — First business slice

- [ ] Implement one complete business type before expanding to three.
- [ ] Add the business state model, shorter operating turns, action budget, revenue, expenses, and annual settlement.
- [ ] Add staffing, pricing, marketing, upgrades, business events, valuation, failure, and sale incrementally.
- [ ] Add property ownership only after the first business and investing loops are stable.

## Production and service gates

- [ ] Replace placeholder glyphs with the approved logo, avatar treatment, and licensed icon set.
- [ ] Add the rounded production font and verify fallback/localization coverage.
- [ ] Add accessibility settings for text scale and reduced motion.
- [ ] Freeze beta save semantics before account-enabled distribution.
- [ ] Add Unity Authentication, Game Center, Cloud Save, and conflict fixtures after the offline save contract is frozen.
- [ ] Add LevelPlay only after placement, consent, and child-directed treatment are validated.

## Verified baseline

- [x] Phase 0 offline architecture and representative content
- [x] 140 passing EditMode tests
- [x] Deterministic seeded birth-to-ending simulation
- [x] Transactional local saves, migration, integrity checks, backup recovery, and rollback safety
- [x] Playable Life, Social, education, career, achievement, retirement/death, and final-summary flows
