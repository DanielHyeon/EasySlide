# WPF FrmMain visual parity correction

## Why

User visual review showed that the current WPF shell still does not look like legacy `FrmMain`. It exposes many commands, but the first screen remains a modern dashboard instead of the dense operator console used during worship.

The reference `FrmMain` screen has a stable structure:

- top menu and compact toolbar
- left source browser above Worship List
- middle Preview text/list area above Preview slide surface
- right Output text/list area above Output slide surface
- thin splitters and dense command strips directly attached to Preview/Output panes

## What Changes

- Rework the WPF first-screen shell toward the `FrmMain` 3-column, 2-row console geometry.
- Keep Preview and Output visible at the same time instead of treating Output as only a status header or right inspector.
- Move the first-screen emphasis away from the right settings inspector and toward the live Output pane.
- Add layout drift tests that lock the named Preview/Output panes and command strips.

## Scope

- XAML layout and XAML drift tests only.
- No database, Office Interop, rendering pipeline, output coordinate, or public API changes.
- Existing VM commands are reused.

## Acceptance Criteria

- The main shell has stable `ClassicSourcePane`, `ClassicPreviewPane`, and `ClassicOutputPane` regions.
- Preview and Output each have a top text/list area and a bottom slide surface visible on the first screen.
- Preview and Output command strips include the same core live controls operators expect from `FrmMain`.
- WPF tests, main project build, and OpenSpec validation pass.
