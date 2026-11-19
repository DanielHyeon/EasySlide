## Context

The two upper lists share ImageCanvas. The lower preview draws a separate image. WinForms builds slide line ranges and layout information before drawing text.

## Decisions

Reserve room for markers and the number before scaling the whole slide proportionally. Keep the scaled raster reusable when selection changes.

Enable paragraph fitting by default for external text files, including installations without a saved preference. The user can disable it in Show options to use their existing overflow preference. Apply accepted changes to loaded text items. Use the shared lyric pagination path for preview and output.

## Trade-offs

Leaving room for the number reduces thumbnail area. A long paragraph can require a small font to fit one screen.
