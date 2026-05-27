# Form Designer Split Plan

## Goal

Refactor active WinForms `Frm*.cs` files so designer-generated UI code is separated from hand-written form logic, following the same core pattern used by `FrmMain`:

- `FrmX.cs`: constructor, event handlers, workflow logic, non-designer state.
- `FrmX.Designer.cs`: `components`, `Dispose`, `InitializeComponent`, and designer control fields.
- All form class declarations use `partial`.

`FrmMain` already has additional `Fields`, `Events`, `Layout`, and `Logic` files. This pass focuses on the common WinForms split that directly removes mixed designer/logic code from every form. Additional semantic splitting can be done later per large form after this low-risk structural baseline is in place.

## Refactoring Scope

Active forms under `Easislides/Easislides` are in scope. Existing generated/auxiliary fragments such as `.Designer.cs`, `.Events.cs`, `.Fields.cs`, `.Layout.cs`, and `.Logic.cs` are not treated as source form roots.

Project-excluded legacy forms remain out of functional scope, but the mechanical split is safe for source hygiene when those files are still present in the tree.

## TDD / Verification Plan

1. Add a static verification script before refactoring.
2. Run it and confirm it fails on the current mixed files.
3. Refactor forms into `FrmX.cs` + `FrmX.Designer.cs`.
4. Re-run the static verification until it passes.
5. Run `dotnet build Easislides/Easislides.csproj`.
6. If available, run external QA tools:
   - `gstack /qa`
   - `GSD verify-work`
7. If those tools are unavailable, record that and use static verification plus build as the local fallback.

## Completion Criteria

- Every `Frm*.cs` form root has a `partial class` declaration.
- No form root contains `private void InitializeComponent()`.
- No form root contains `protected override void Dispose(bool disposing)`.
- Every form root has a matching `FrmX.Designer.cs`.
- Every designer file contains the matching partial class and `InitializeComponent`.
- The Easislides project builds successfully.

## Status

- [x] CodeGraph context reviewed.
- [x] Static verification script added.
- [x] Initial failing TDD verification captured.
- [x] Forms refactored.
- [x] Static verification passing.
- [x] Build passing.
- [x] External QA tool availability checked. `gstack` and `GSD` are not available in the current PATH.
- [ ] Commit created.
- [ ] Push attempted.
