## 1. Database commands

- [x] 1.1 Bind values in search, import and saved-song lookups and quote dynamic identifiers.
- [x] 1.2 Reduce repeated reads for listing and merge, and batch usage deletion.
- [x] 1.3 Verify query semantics, resource ownership and rollback with isolated SQLite tests and the Release suite.

## Verification

- Release solution tests passed on Windows: 213 WinForms, 2,477 WPF and 20 analyzer tests.
- In-memory SQLite tests exercised quoted titles, replayed date and folder filters, whole-word verse matches, repeated merge pairs and transaction rollback. The SQLite trace recorded one SELECT for the merge.
- Temporary files covered punctuation in file and table names, missing read-only inputs, usage batches and the active-database export guard. Both EasiSlides registry views matched their pre-test fingerprints.
