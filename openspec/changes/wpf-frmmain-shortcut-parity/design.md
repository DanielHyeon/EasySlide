## Context

The WPF shell already routes shortcuts through `CommandCatalog`, `ShortcutRegistry`, `MainViewModel.BindShortcuts`, and `MainWindow.OnPreviewKeyDown`. Existing tests cover uniqueness, core F-key shortcuts, global/local registry behavior, and menu hint existence. The remaining risk is drift: menu hints, command ids, and FrmMain parity documentation can diverge while layout work continues.

## Goals / Non-Goals

**Goals:**

- Keep the FrmMain live-operation shortcut set explicit and testable.
- Strengthen anti-drift tests for command id to gesture mapping, including Space and Shift+Space navigation.
- Document the current WPF shortcut parity map and manual QA gaps.

**Non-Goals:**

- Do not redesign the shortcut customization system.
- Do not change global hook behavior.
- Do not change text input focus rules in this increment; record any remaining focus-sensitive QA separately.

## Decisions

- Use `CommandCatalog` as the source of truth for actual shortcut definitions.
  - Alternative: parse XAML menu hints as source of truth. Rejected because menu hints are display-only strings and can lie.
- Keep menu hint tests in `MainMenuBarTests`.
  - Alternative: move all shortcut tests to `CommandCatalogTests`. Rejected because XAML drift must be caught where the strings live.
- Add documentation under `docs/wpf-migration/inventory`.
  - Alternative: only update roadmap prose. Rejected because operators and reviewers need a tabular parity map.

## Risks / Trade-offs

- [Risk] A test that only checks display text can miss wrong command binding.
  - Mitigation: assert both command id to `DisplayText` in `CommandCatalog` and matching XAML menu hint.
- [Risk] Text input focus behavior can be more nuanced than static tests show.
  - Mitigation: keep it as manual QA/backlog unless a specific regression is found.
