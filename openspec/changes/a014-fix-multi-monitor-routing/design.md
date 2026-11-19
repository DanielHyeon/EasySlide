# Design

## Context

WinForms stores signed desktop coordinates. PowerPoint accepts points. WPF reads monitor bounds in desktop pixels and also supports a custom audience rectangle.

## Decisions

- Restore the operator window within the working area with the largest intersection. Use the primary working area when the saved window is disconnected.
- Route live media using the item or session preference, falling back to the resolved audience placement. Keep the standalone playback menu on the audience output.
- Use one desktop snapshot for audience and stage selection. Keep user preferences separate from temporary choices. Prefer a free secondary display for the stage.
- Apply desktop pixels at the native WPF window boundary so mixed-DPI origins and extents stay in one coordinate space. Keep custom audience bounds independent of stage bounds.

## Verification

Exercise geometry, commands and reconnects with simulated displays, isolated settings and window handles. Compare production registry values before and after verification.
