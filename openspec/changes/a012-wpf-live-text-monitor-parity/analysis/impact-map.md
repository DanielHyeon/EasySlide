# Impact Map

## CodeGraph Evidence

- CodeGraph status: initialized and healthy for this workspace.
- WinForms baseline path:
  - `FrmMain.btnToLive_Click`
  - `FrmMain.PreviewItemToLive`
  - `FrmMain.CopyPreviewToOutput`
  - `FrmMain.GoLive`
  - `FrmLaunchShow.Start_Presentation`
  - launch-screen item loading formats the current live body into the lyrics/text monitor buffer.
- WPF path:
  - `MainWindow.GoLiveCommand` / `PreviewToLiveCommand`
  - `MainViewModel.PublishSelectedItemAsync`
  - `MainViewModel.PublishOutputItemAsync`
  - `LiveSessionService.GoLive`
  - `LiveSessionService.CreateSnapshot`
  - `OutputWindowHost.OnSessionChanged`
  - `OutputWindowViewModel.ApplyScene`

## Affected Boundary

- Production code changed only in `Easislides.Wpf/Shell/MainViewModel.cs`.
- Tests changed only in `Easislides.Wpf.Tests/Shell/MainViewModelTests.cs`.
- Repo-local analysis/config files added under:
  - `ast-grep/rules/`
  - `tools/logic_map/`
  - `openspec/changes/a012-wpf-live-text-monitor-parity/`

## Behavioral Finding

WinForms reloads or carries display-ready song body text before the live output surface is updated. WPF already carries body text when `LiveQueueItem.Lyrics` is populated, but a DB song item such as `song:{id}` with empty lyrics can reach `LiveSessionService.CreateSnapshot` without the body being hydrated. The output window then receives an empty or stale `BodyText` even though the Live command succeeds.

## Implementation Decision

The smallest safe boundary is the WPF live publish path in `MainViewModel`:

- Prepare the item as before.
- If it is a song, has empty lyrics, and has a resolvable DB song id, load `SongDetail` before creating the live projection.
- Preserve existing Text, Bible, Notice, external text, PowerPoint, and already-hydrated song behavior.

## Non-Impacted Areas

- No WinForms production code changed.
- No DB schema changed.
- No public WPF live session contract changed.
- No PowerPoint COM behavior changed.
