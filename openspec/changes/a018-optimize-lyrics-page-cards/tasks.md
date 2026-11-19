## 1. Operator page rendering

- [x] 1.1 Reuse the prepared page bodies when constructing preview and output cards.
- [x] 1.2 Verify repeated verses, projected content, clamping and selection.

## 2. Verification

- [x] 2.1 Compare release performance on the same 24-page song and run the regression suites.

## Verification results

- On .NET 11 Release, a 12-verse song repeated twice produced 24 cards. Across 15 warmed samples of 50 list builds, median elapsed time per list went from 0.4237 ms to 0.0459 ms. Managed allocations per list went from 396,680 to 41,184 bytes.
- All fields matched on 222 cards across single and dual regions, notation settings, Bible quotations, external text, notices and page selection.
- The full Release suite passed all 2,727 tests.
