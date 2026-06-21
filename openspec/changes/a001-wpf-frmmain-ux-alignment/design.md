# Design

## Layout Direction

Use the existing WPF shell and commands, but make the first screen read as an operator console:

- Top menu remains a secondary discovery path.
- `LiveBar` remains the live state signal.
- The fixed live command bar stays visible even when the right inspector is collapsed.
- The central content header separates "Preview ready" from "Output live" so the operator can distinguish the selected item from the item currently being sent.
- The left browser and worship list remain visible together.
- The right inspector remains for detailed appearance edits, not for core live safety actions.

## Code Strategy

This increment intentionally avoids `MainViewModel` changes. CodeGraph impact for `MainViewModel` is broad, so the XAML reuses existing commands and properties:

- `GoLiveCommand`
- `SendToOutputAndNextCommand`
- `StopLiveCommand`
- `BlackScreenCommand`
- `ClearOutputCommand`
- `HideOutputCommand`
- `RestoreOutputCommand`
- `RestartCurrentItemCommand`
- `RefreshOutputCommand`
- `FirstItemCommand`, `PreviousItemCommand`, `NextItemCommand`, `LastItemCommand`
- `PreviousLyricsPageCommand`, `NextLyricsPageCommand`, `JumpToLyricsSectionCommand`
- `PreviousSlideCommand`, `NextSlideCommand`, `GoToSlideCommand`

## Non-goals

- No render parity changes.
- No keyboard handler rewrite.
- No Office COM or PowerPoint thumbnail generation changes.
- No DB access changes.
- No gf/Gf rename work.

## P1 Follow-up

Stop Live is a dangerous live operation and should not require the Output menu. It belongs in the fixed operator bar with a danger visual treatment, separate from Close Output.
