# FrmMain Shortcut Parity Map

작성일: 2026-06-04
대상: `Easislides.Wpf/Input/CommandCatalog.cs`, `Easislides.Wpf/MainWindow.xaml`, `Easislides.Wpf/Shell/MainViewModel.cs`

## 목적

FrmMain 기반 운영자가 예배 송출 중 기대하는 키보드 조작을 WPF에서도 같은 의미로 찾을 수 있게 유지한다. 이 표는 단축키가 실제 명령 카탈로그에 있고, 메뉴 힌트가 거짓 표시로 drift 되지 않도록 검증 범위를 고정한다.

## Live Operation Shortcuts

| Shortcut | FrmMain meaning | WPF command id | WPF binding/menu path | Scope | Automated guard |
|---|---|---|---|---|---|
| `F12` | Go Live / Start Show | `MainCommandIds.LiveGo` | `GoLiveCommand`, Output menu | Local | `CommandCatalogTests`, `MainMenuBarTests` |
| `F11` | 송출 후 다음 항목 | `MainCommandIds.LiveGoAndNext` | `SendToOutputAndNextCommand`, Output menu | Local | `CommandCatalogTests`, `MainMenuBarTests` |
| `Space` | 다음 항목 | `MainCommandIds.LiveNext` | `NextItemCommand`, Output menu | Local | `CommandCatalogTests`, `MainMenuBarTests` |
| `Shift+Space` | 이전 항목 | `MainCommandIds.LivePrevious` | `PreviousItemCommand`, Output menu | Local | `CommandCatalogTests`, `MainMenuBarTests` |
| `F5` | 다음 항목 global 운용키 | `MainCommandIds.LiveNext` | `ShortcutRegistry` global route | Global | `CommandCatalogTests`, `ShortcutRegistryTests`, `GlobalInputServiceTests` |
| `F4` | 이전 항목 global 운용키 | `MainCommandIds.LivePrevious` | `ShortcutRegistry` global route | Global | `CommandCatalogTests`, `ShortcutRegistryTests` |
| `F9` | Black screen | `MainCommandIds.LiveBlack` | `BlackScreenCommand`, Output menu | Local | `CommandCatalogTests`, `MainMenuBarTests` |
| `F3` | Clear screen | `MainCommandIds.LiveClear` | `ClearOutputCommand`, Output menu | Local | `CommandCatalogTests`, `MainMenuBarTests` |
| `Ctrl+R` | 현재 항목 처음으로 | `MainCommandIds.LiveRestart` | `RestartCurrentItemCommand`, Output menu | Local | `CommandCatalogTests`, `MainMenuBarTests` |
| `Ctrl+F5` | 출력 새로고침 | `MainCommandIds.LiveRefresh` | `RefreshOutputCommand`, Output menu | Local | `CommandCatalogTests`, `MainMenuBarTests` |
| `F1` | 도움말 | `MainCommandIds.WindowHelp` | `OpenHelp_Click`, Help menu | Local | `CommandCatalogTests`, `MainMenuBarTests` |

## Verification Notes

- `CommandCatalog` is the source of truth for actual shortcuts.
- `InputGestureText` in XAML is display-only and must be tested against the catalog.
- `F5` and `Ctrl+F5` intentionally coexist: the modifier makes refresh distinct from next item.
- `Space` can also be a media playback key when live media is active; `MainWindow.OnPreviewKeyDown` routes media keys before generic shortcuts.

## Phase 6 Evidence (2026-06-21)

- CodeGraph impact refreshed for `ShortcutRegistry`, `CommandCatalog`, `VerseJumpKeyMap`, `MediaPlayerKeyMap`, and `WorshipListKeyMap`.
- `MainWindow.OnPreviewKeyDown` routes focused Preview/Output surfaces before global shortcut fallback.
- `MainWindow.IsTextInputFocused()` blocks final live shortcut routing while the operator is typing in QuickFind, Bible lookup, live message, or editable setting fields.
- `VerseJumpKeyMap` covers digit and Shift verse mappings (`Shift+B/W/P/Q/T`) without allowing Ctrl/Alt/Win fallthrough.
- Focused test command passed: `dotnet test Easislides.Wpf.Tests --filter "FullyQualifiedName~CommandCatalogTests|FullyQualifiedName~ShortcutRegistryTests|FullyQualifiedName~GlobalInputServiceTests|FullyQualifiedName~VerseJumpKeyMapTests|FullyQualifiedName~MediaPlayerKeyMapTests|FullyQualifiedName~WorshipListKeyMapTests|FullyQualifiedName~BindShortcuts_|FullyQualifiedName~MainWindow_UsesFocusedPreviewOutputKeyboardBeforeGlobalShortcuts|FullyQualifiedName~MainWindow_BlocksGlobalShortcutsWhenTextInputFocused|FullyQualifiedName~MainWindow_PassesShiftToVerseJumpKeyMap"`; result: 108 passed, 0 failed, 0 skipped.

## Remaining Manual QA

- Confirm `Space` advances worship items when no media playback key route is active.
- Confirm `Space` controls media playback when a live media item is active and the media router can execute.
- Confirm `F5`/`F4` global live navigation works while the output window has focus.
- Confirm text input fields do not cause accidental verse jump on the running WPF shell; automated guards already cover the text-input block path.
