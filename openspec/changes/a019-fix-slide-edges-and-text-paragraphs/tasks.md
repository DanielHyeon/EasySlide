## 1. Thumbnail rendering

- [x] 1.1 Reproduce covered edges and verify proportional content and marker placement on both lists.

## 2. Text file display

- [x] 2.1 Add the Show option with persistence, paragraph fitting and refresh on acceptance.
- [x] 2.2 Verify blank-line boundaries and legacy overflow behaviour against regression cases and the Creed file.

## 3. Verification

- [x] 3.1 Build and run the solution tests and validate the change.

## Verification

- The pre-fix pixel tests lost the top edge at both slide ratios. The Creed fixture split into five slides instead of two. Both regressions pass after the changes.
- With the original Creed text at 960 × 540 and 60 pt, both WinForms pipelines produced 21 slides with ordinary overflow and 10 with paragraph fitting. All ten paragraphs matched the source. Each expected line was present in the rendered output under both largest-font settings.
- The Options dialog saved and reloaded both states under a redirected HKCU. The upper thumbnail controls rendered the first-slide preview embedded in the reported PowerPoint file.
- Debug build succeeded. All 2,730 Release solution tests passed with UI tests run serially. OpenSpec strict validation passed.
