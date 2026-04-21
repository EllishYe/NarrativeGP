# reCAPTCHA Narrative Framework

## Recommended Scene Setup

- Use **one main scene** for the workday loop.
- Put `Attendance`, `Desk`, `Emails`, `News`, `Log` on separate root panels under one UI Canvas.
- Keep the bottom-center icons always visible and switch panels on/off instead of switching scenes.

This fits your project better than multi-scene because:

- daily state stays in memory without extra scene-loading plumbing
- UI layout and transitions stay consistent
- narrative iteration is much faster for a course project

## Core Runtime Objects

Create these objects in the scene:

1. `GameState`
2. `GameFlowController`
3. `SectionScreenController`

Then create one `SectionIconPresenter` per bottom icon.

## Day Rules Implemented

- **Day 1**: only the first incomplete section is interactable
- `Attendance -> Desk -> Emails -> News -> Log`
- **Day 2+**: `Attendance`, `Desk`, `Emails`, `News` are always interactable
- **Day 2+**: `Log` unlocks only after the other four sections are completed for the current day

## Icon States Implemented

- `Locked`: grey
- `Available`: black
- `New`: red

`Available` covers both:

- unlocked sections with no new content yet
- sections whose required content for today has already been cleared

If a section gets new content later in the day, call:

```csharp
gameState.SetUnreadContent(SectionId.Emails, true);
```

Opening a section only marks it as visited. New-content state stays red until the section's own logic decides that all required interactions are complete and calls:

```csharp
gameState.MarkSectionCompleted(SectionId.Emails);
```

## Condition-Driven Component States

Use `ConditionalStatePresenter` for things like:

- different attendance messages by day
- a strange email appearing only after a flag is set
- news articles changing after the player finishes a task

Each presenter can own multiple visual states, and each state has a `ConditionSet`.

Example ideas:

- show normal inbox state when `CurrentDayEquals = 1`
- show suspicious inbox state when `FlagIsSet = saw_weird_attachment`
- show underground email when `SectionCompletedToday = Desk`

## Suggested Content Authoring Pattern

For each section, split logic into:

1. `Section progress`
2. `Narrative flags`
3. `UI state presenter`

Example flags you may use later:

- `noticed_dead_employee_news`
- `opened_union_spam_mail`
- `failed_satisfaction_survey`
- `saw_glitch_in_attendance`

This lets you write narrative beats as combinations of:

- current day
- whether a section was opened/completed
- whether a story flag has been triggered
