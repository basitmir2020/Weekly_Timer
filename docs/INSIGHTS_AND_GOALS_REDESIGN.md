# Insights And Goals Redesign

## Purpose

This document explains how to rework the `Insights` and `Goals` tabs so they feel more useful, connected, and action-oriented.

Right now:

- `Insights` shows some good raw information, but it is mostly passive.
- `Goals` is too small in scope and behaves like a simple form instead of a planning system.
- Both tabs work independently, but they should work together.

The redesign should make:

- `Goals` = planning and commitment
- `Insights` = reflection, diagnosis, and coaching
- Both tabs = part of one weekly improvement loop

## Current State Summary

### Insights Tab Today

Current page already has:

- weekly goals snapshot
- current streak
- longest streak
- total completed days
- 12-week heatmap
- 14-day mood trend
- refresh button

Main limitations:

- no clear "what should I do next?"
- no explanation of why progress is good or bad
- no comparisons like this week vs last week
- no breakdown of schedule categories even though the view model has a `CategoryBreakdown` model
- no drill-down from metric to cause
- no habit coaching, suggestions, or pattern detection
- no connection to missed blocks, late blocks, productivity windows, or weekly goal completion quality

### Goals Tab Today

Current page already has:

- three weekly goals only
- DSA goal
- Web Dev goal
- Habit goal
- done toggles
- save button

Main limitations:

- fixed to only 3 goals
- no priorities
- no due dates within the week
- no subtasks
- no progress percent
- no category or tag system
- no carry-forward workflow for unfinished goals
- no reflection on why a goal was missed
- no weekly planning structure
- no visual relationship to actual schedule blocks

## Redesign Direction

## Big Product Idea

These two tabs should support a full cycle:

1. Plan the week in `Goals`
2. Execute through `Schedule`
3. Track check-ins and completion
4. Review outcomes in `Insights`
5. Use insights to create better goals next week

That means:

- `Goals` should answer: "What matters this week?"
- `Insights` should answer: "What actually happened?"
- `Goals` should feed `Insights`
- `Insights` should improve the next set of `Goals`

## Recommended UX Positioning

### Goals Tab Should Feel Like

- a weekly planning dashboard
- a commitment tracker
- a place to define focus, not just type 3 text fields

### Insights Tab Should Feel Like

- a personal review dashboard
- a feedback system
- a coach that points out patterns and next actions

## Insights Tab Redesign

## New Structure For Insights Tab

Recommended section order:

1. Weekly Scorecard
2. Key Wins And Risks
3. Goal Progress Review
4. Time And Category Breakdown
5. Consistency And Streaks
6. Mood / Energy / Check-in Trends
7. Pattern Detection
8. Recommendations For Next Week

## 1. Weekly Scorecard

This should be the hero section at the top.

Show:

- weekly completion score
- number of completed blocks
- missed blocks
- weekly goal completion percent
- consistency score
- this week vs last week delta

Why this matters:

- users should understand their weekly health in under 5 seconds
- the first section should summarize, not force the user to scroll to interpret data

Suggested cards:

- `Execution Score`
- `Goals Completed`
- `Consistency`
- `Recovery / Balance`

Possible formula ideas:

- Execution Score = completed blocks / total planned blocks
- Goal Score = completed goals / total goals
- Consistency Score = count of days above a threshold such as 70 percent completion
- Balance Score = based on study, work, rest, sleep categories

## 2. Key Wins And Risks

This section should auto-generate small insights.

Examples:

- "You completed 5 of 7 days above 70%."
- "Work blocks are strong, but evening study is slipping."
- "Mood improved on days where sleep blocks were completed."
- "You missed 3 late-evening focus blocks."

This section should be concise and visual:

- 2 to 5 insight chips/cards
- green for strengths
- yellow/red for risk areas

## 3. Goal Progress Review

Current goal snapshot in Insights should become stronger.

Show for each weekly goal:

- title
- progress percent
- done / not done
- linked category
- why it succeeded or failed
- carry forward suggestion

Add:

- `Completed`
- `In Progress`
- `Blocked`
- `Dropped`

This is important because goal status is richer than just true/false.

## 4. Time And Category Breakdown

This section is missing today and should be added.

Show:

- total planned minutes by category
- total completed minutes by category
- completion rate by category
- weekly time investment chart

Recommended visual blocks:

- donut chart or segmented bar for categories
- stacked comparison: planned vs completed
- top category by time
- weakest category by completion

Categories to analyze:

- work
- study
- exercise
- meal
- break
- routine
- sleep
- relax

Useful insights:

- "Study had the lowest completion rate this week."
- "Routine blocks are your strongest category."
- "Sleep is planned well but not consistently completed."

## 5. Consistency And Streaks

Keep the current streak cards and heatmap, but make them more valuable.

Add:

- current streak
- best streak
- completed days this month
- consecutive days above 70%
- day-of-week reliability

Day-of-week reliability should answer:

- which weekday is strongest
- which weekday is weakest
- whether weekends are collapsing the weekly momentum

This helps the user see whether the problem is systemic or isolated.

## 6. Mood / Energy / Check-in Trends

Current mood trend is too thin.

It should include:

- morning energy trend
- evening mood trend
- correlation with completion
- best-performing mood days
- lowest-performing mood days

Possible questions the system can answer:

- "Do high-energy mornings lead to higher completion?"
- "Does poor evening mood follow overloaded days?"
- "Which days had good mood and good completion together?"

This turns check-ins into useful insight instead of decorative history.

## 7. Pattern Detection

This should be one of the most valuable sections.

The app should surface patterns such as:

- most missed time window
- strongest completion window
- blocks usually completed on time
- blocks frequently skipped
- goals that tend to fail when sleep is poor
- weekends reducing Monday performance

Suggested examples:

- "You are strongest between 7 AM and 1 PM."
- "Blocks after 8:30 PM have the highest skip rate."
- "Your habit goal is usually completed when exercise is also completed."

## 8. Recommendations For Next Week

Insights should not stop at reporting.

Add a final section:

- recommended weekly adjustments
- schedule tuning suggestions
- goal sizing suggestions
- habit simplification ideas

Examples:

- "Reduce evening focus blocks from 90 min to 60 min."
- "Move DSA revision to the morning window."
- "Set 2 primary goals and 1 stretch goal."
- "Break habit goals into daily checkmarks instead of one weekly yes/no."

## Features To Add In Insights Tab

- weekly execution score
- weekly goal completion score
- this week vs last week comparison
- planned vs completed time by category
- strongest category and weakest category
- strongest day and weakest day
- strongest time window and weakest time window
- missed blocks summary
- skipped-late-night pattern detection
- day-of-week reliability chart
- energy vs completion comparison
- mood vs completion comparison
- insight cards with plain-language explanations
- recommendations for next week
- drill-down into a day when a metric is tapped
- goal review section with richer statuses
- monthly trend section
- export/share weekly review summary

## Goals Tab Redesign

## New Structure For Goals Tab

Recommended section order:

1. Week Header
2. Weekly Focus Summary
3. Goal Board
4. Habit Commitments
5. Progress Tracker
6. Reflection And Carry Forward

## 1. Week Header

Top section should show:

- current week range
- week theme or focus
- remaining days in week
- overall goal progress

Example:

- `Week of Mar 9 - Mar 15`
- `Theme: Consistency over intensity`
- `2 of 5 goals completed`

This makes the tab feel like a weekly planning workspace.

## 2. Weekly Focus Summary

Before individual goals, add a short summary area:

- primary focus
- secondary focus
- non-negotiable habit
- one thing to avoid this week

This is useful because not all priorities should have equal importance.

Example:

- Primary focus: Finish DSA recursion set
- Secondary focus: Ship auth UI
- Non-negotiable: Sleep before 10:30 PM
- Avoid: Overloading weekends

## 3. Goal Board

Replace the current 3 fixed text fields with a flexible goal board.

Each goal card should support:

- title
- category
- description
- target outcome
- priority
- status
- progress percent
- due day
- linked schedule category
- notes

Recommended statuses:

- Not Started
- In Progress
- Completed
- Blocked
- Deferred

Recommended priorities:

- High
- Medium
- Low

Recommended goal types:

- Outcome goal
- Project goal
- Learning goal
- Habit goal
- Stretch goal

## 4. Habit Commitments

Habit goals should not be mixed with project goals.

Create a separate habit area for:

- sleep target
- exercise frequency
- study consistency
- hydration
- screen time boundary

Habit tracking can be:

- daily checkboxes
- streak counter
- target frequency such as `5/7`

This is much better than a single text field called `Habit Focus`.

## 5. Progress Tracker

Goals tab should show progress while the week is running.

Add:

- progress bar per goal
- completed subtasks count
- days left
- urgency indicator
- linked completed schedule blocks

Examples:

- "DSA Topic: 60% complete"
- "2 subtasks pending"
- "Due by Friday"
- "No progress in the last 3 days"

This turns the tab from planning-only into planning + execution tracking.

## 6. Reflection And Carry Forward

At the bottom of the weekly goals tab, add a closing workflow.

When the week ends:

- mark goal as completed, partial, or missed
- write why it was missed
- carry forward into next week
- archive it
- split it into smaller next-week goals

This is critical because unfinished goals should not disappear without explanation.

## Recommended Goal Data Model Changes

Current model is too rigid.

Instead of only:

- DSA topic
- Web dev feature
- habit focus
- done flags

Move toward:

- `WeeklyPlan`
- `WeeklyGoalItem`
- `HabitCommitment`
- `GoalSubtask`
- `WeeklyReflection`

Suggested `WeeklyGoalItem` fields:

- `Id`
- `WeekStartDate`
- `Title`
- `Description`
- `Category`
- `Priority`
- `Status`
- `ProgressPercent`
- `TargetType`
- `DueDay`
- `LinkedScheduleCategory`
- `Notes`
- `CreatedAt`
- `UpdatedAt`
- `CompletedAt`

Suggested `HabitCommitment` fields:

- `Id`
- `WeekStartDate`
- `Title`
- `TargetFrequency`
- `CompletedCount`
- `DailyChecks`
- `Status`

Suggested `WeeklyReflection` fields:

- `WeekStartDate`
- `BiggestWin`
- `BiggestMiss`
- `WhyGoalsFailed`
- `WhatToImproveNextWeek`

## Features To Add In Goals Tab

- dynamic number of weekly goals
- priorities for each goal
- goal status beyond done/not done
- progress percent
- subtasks/checklist under each goal
- due day inside the week
- weekly focus/theme
- separate habit commitments section
- target frequency for habits
- auto carry-forward for incomplete goals
- archive completed or dropped goals
- notes or blockers field
- goal templates
- stretch goals
- link goals to schedule categories
- link goals to actual completed blocks
- weekly review and reflection form
- quick duplicate last week goals
- weekly planning checklist on Sunday
- auto-highlight stale goals with no progress

## How Insights And Goals Should Connect

This is the most important part of the redesign.

The two tabs should share data and workflows.

### Goals -> Insights

Insights should use goal data to show:

- goal completion rate
- which goal types succeed most
- which priorities are realistic
- which categories are under-supported by the schedule

### Insights -> Goals

Goals should use analytics to suggest:

- fewer goals if last week was overloaded
- more realistic habit targets
- moving focus to better-performing time windows
- carrying forward only the most important unfinished goals

### Good Product Loop

1. User creates 3 to 5 goals in `Goals`
2. App tracks schedule completion and check-ins
3. `Insights` explains execution quality
4. App suggests better goals for next week
5. User starts next week with smarter planning

## Recommended UI Improvements

## Insights UI

- use a summary hero at the top
- use compact metric cards
- use colored insight chips
- add expandable sections for details
- add tap-to-drill-down cards
- reduce passive scrolling by grouping related data

## Goals UI

- use a board or stacked card layout
- show each goal as its own object, not just entry fields
- add visual priority tags
- add status pills
- add progress bars
- add habit mini-tracker rows
- add a weekly review section at the bottom

## Recommended Phased Implementation

## Phase 1: Improve Existing Structure

- redesign layout without changing too much backend logic
- improve weekly goals snapshot in Insights
- add weekly scorecard
- add category breakdown in Insights
- improve Goals UI with priority and status
- add subtasks and progress bars

## Phase 2: Expand Data Model

- replace rigid weekly goal schema with reusable goal items
- add habit commitments
- add goal reflection data
- add carry-forward support

## Phase 3: Smart Recommendations

- add pattern detection
- add week-over-week comparisons
- add suggestions based on missed blocks and mood trends
- add auto-generated next-week planning recommendations

## Recommended Priority Order

If you want the highest-value build order, do this:

1. Redesign Goals into a flexible goal board
2. Add weekly scorecard to Insights
3. Add category/time breakdown in Insights
4. Add goal progress review in Insights
5. Add habit commitments with daily checks
6. Add carry-forward and weekly reflection
7. Add smart recommendations and pattern detection

## Final Recommendation

The biggest mistake would be treating:

- `Insights` as only charts
- `Goals` as only form inputs

The better design is:

- `Goals` = planning system
- `Insights` = decision system

If implemented well, these two tabs can become the part of the app that makes users improve week after week instead of only tracking completion.
