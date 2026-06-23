# Branch Test Map

## Branches

| Branch | Evidence | Expected result |
| --- | --- | --- |
| WPF Live with already-populated lyrics | `GoLiveCommand_WhenOutputClosed_UpdatesOutputWindowTextMonitorBody` | Output surface scene body equals the live item lyrics. |
| WPF Live with DB song id and empty lyrics | `GoLiveCommand_WithDatabaseSongMissingLyrics_ReloadsTextMonitorBodyLikeWinForms` | Song detail repository is called and output surface scene body equals the loaded verse. |
| Non-song item | Guarded by `PrepareLiveItemForOutputAsync`: non-song kinds return the existing prepared item. | Existing Text/Bible/Notice/external text fallback behavior is preserved. |
| Song with existing lyrics | Guarded by `PrepareLiveItemForOutputAsync`: non-empty lyrics return without DB load. | No unnecessary repository call, current body remains authoritative. |
| Song detail missing or empty | Guarded by `PrepareLiveItemForOutputAsync`: null/empty detail returns existing prepared item. | Live command remains resilient. |
| Song detail load exception | Guarded by catch block. | Status text records failure, live command continues with existing payload. |

## TDD Notes

- Red: DB song with empty lyrics did not call the detail repository before publishing live output.
- Green: async preparation hydrates missing song lyrics before live projection.
- Refactor: no additional abstraction beyond the local async preparation helper.
