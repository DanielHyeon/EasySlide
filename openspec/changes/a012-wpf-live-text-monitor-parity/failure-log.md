# Failure Log

## Red Test

Test:

```powershell
dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj --filter "FullyQualifiedName~GoLiveCommand_WithDatabaseSongMissingLyrics_ReloadsTextMonitorBodyLikeWinForms" --no-restore -v minimal
```

Initial failure:

```text
Expected repo.LastSongId to be 7, but found 0.
```

## Root Cause

The WPF Live publish path prepared and projected the selected item without hydrating a DB song when `LiveQueueItem.Lyrics` was empty. The live session and output window received the projected payload, but the body text was missing before the session snapshot was created.

## Fix

`MainViewModel.PublishSelectedItemAsync` and `MainViewModel.PublishOutputItemAsync` now call `PrepareLiveItemForOutputAsync(...)`, which preserves existing preparation and hydrates missing lyrics for resolvable DB song items before projection.

## Prevention

The regression test asserts both the repository call and the output scene body text, so a future refactor cannot pass by only toggling live state.

## Post-Deploy Monitor Regression

Symptom:

```text
Live Show did not appear on the expected monitor after deployment.
```

Observed runtime state:

- Windows detected two displays.
- `DISPLAY2` was the primary monitor.
- `DISPLAY1` was the non-primary output monitor.
- User settings had `LiveOutput.DisplayAlwaysUseSecondaryMonitor=true`.
- User settings also had `LiveOutput.DefaultOutputMonitorId=\\.\DISPLAY2`.

Root cause:

`MainViewModel.GetPreferredOutputDisplay(...)` applied the stored `DefaultOutputMonitorId` before applying `DisplayAlwaysUseSecondaryMonitor`. A stale or accidental stored primary-monitor id could therefore override the "always use secondary monitor" policy, so Live opened on the primary display instead of the output monitor.

Red test:

```powershell
dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj --filter "FullyQualifiedName~OpenOutputCommand_WhenAlwaysUseSecondaryMonitorIgnoresStoredPrimaryMonitor" --no-restore -v minimal
```

Initial failure:

```text
Expected SelectedOutputDisplay to be display-2/Projector, but found primary/Primary.
```

Fix:

`GetPreferredOutputDisplay(...)` now applies `DisplayAlwaysUseSecondaryMonitor` first. If a stored preferred id points to a non-primary display, it is preserved; if it points to a primary display, the code falls back to the first non-primary display.

Runtime mitigation:

The local deployed-user setting was updated to `DefaultOutputMonitorId=\\.\DISPLAY1` with `DisplayAlwaysUseSecondaryMonitor=true`.

## Actual-Run Output Placement Regression

Symptom:

```text
Live Show still did not visibly cover the output monitor after clicking Live in the deployed app.
```

Observed runtime evidence:

- Actual deployed executable: `C:\EasiSlides\EasislidesNext\EasislidesNext.exe`.
- `DISPLAY1` was the non-primary output monitor at `X=3440, Y=0, Width=2560, Height=1440`.
- User settings had `LiveOutput.DisplayCustomWidth=1`.
- Before the fix, UI Automation reported `EasiSlides Output` at `X=0, Y=0, Width=2, Height=2`.
- After ignoring tiny custom bounds, Live Show appeared but was shifted right: `X=4300, Width=3200`.

Root causes:

1. `WindowPlacementService` trusted legacy custom output bounds even when the custom width was invalidly tiny.
2. `WindowPlacementService` returned physical pixel bounds directly to WPF `Window.Left/Top/Width/Height`, which expect DIP units. On the 125% scaled output monitor, `3440px` became `4300px` at runtime.

Red/verification tests:

```powershell
dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj --filter "FullyQualifiedName~CreateOutputPlacement_WithTinyCustomWidth_IgnoresInvalidCustomBounds" --no-restore -v minimal
```

Initial failure:

```text
Expected placement to be (3440, 0, 2560, 1440), but found (0, 0, 1, 0.75).
```

Fix:

`WindowPlacementService` now ignores custom output widths below 320px and converts physical display/custom bounds to WPF DIP units using `OutputDisplay.DpiScale`.

Actual-run confirmation:

- After the final deployment, UI Automation reported `EasiSlides Output` at `X=3440, Y=0, Width=2560, Height=1440`, matching `DISPLAY1`.
- Screenshot: `evidence/screenshots/2026-06-23/live-show-debug/06-after-live-dpi-fix-monitor-1.png`.

## Apostles Creed Selected Text and Appearance Regression

Symptoms:

```text
When ★★★사도신경★★★.txt was Live Shown, the actual Live Show used a different-looking background/text appearance than the app preview, and copying the selected text from the left Preview area to the right Output area selected the first sentence instead of the current sentence.
```

Observed runtime evidence before the fix:

- Actual deployed executable: `C:\EasiSlides\EasislidesNext\EasislidesNext.exe`.
- `★★★사도신경★★★.txt` was selected.
- Preview was advanced to page `3/10`.
- After `btnToOutput`, the right Output text returned to the first page instead of preserving page `3/10`.
- The app Preview/Output sample panels used the old hardcoded teal/yellow appearance while the actual output monitor used the configured blue cross background with black text.
- Screenshot evidence:
  - `evidence/screenshots/2026-06-23/apostles-live-parity/02-after-select-failed-screen1.png`
  - `evidence/screenshots/2026-06-23/apostles-live-parity/02-after-select-failed-screen0.png`

Root causes:

1. `MainViewModel.PrepareOutputFromItem(...)` prepared a fresh output item without carrying over the currently selected preview lyrics page.
2. The large in-app Preview/Output text panels in `MainWindow.xaml` still used hardcoded sample background/foreground/font values instead of the same effective song/settings formatting used by the live output renderer.

Red test:

```powershell
dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj --filter "FullyQualifiedName~CopyPreviewToOutputCommand_PreparesCurrentPreviewLyricsPage" --no-restore -v minimal
```

Initial failure:

```text
Expected sut.OutputItem!.LyricsPageIndex to be 2, but found 0.
```

Fix:

- `PrepareOutputFromItem(...)` now preserves the selected preview lyrics page when copying Preview to Output.
- The in-app large Preview/Output text panels now bind to computed sample appearance properties that use the same active output settings and per-item `FormatData` values for background, foreground, font size, font family, horizontal alignment, and vertical alignment.

Actual-run confirmation after deployment:

- Selected `★★★사도신경★★★.txt`.
- Advanced Preview to page `3/10`.
- Clicked `btnToOutput`.
- Clicked `LIVE`.
- The right Output preview stayed on page `3/10`.
- The actual Live Show rendered the same selected text and matching blue cross/black centered appearance.
- Screenshot evidence:
  - `evidence/screenshots/2026-06-23/apostles-live-parity/04-final-after-align-screen1.png`
  - `evidence/screenshots/2026-06-23/apostles-live-parity/04-final-after-align-screen0.png`
