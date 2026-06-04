## CodeGraph Impact

- `CommandCatalog` impact radius is broad: command palette, settings shortcut editor, shortcut registry tests, platform diagnostics, and `MainViewModel.BindShortcuts` all depend on its descriptors/default shortcuts.
- This increment avoids production `CommandCatalog` changes unless a concrete missing shortcut is found.
- Planned code edits are scoped to:
  - `Easislides.Wpf.Tests/Input/CommandCatalogTests.cs`
  - `Easislides.Wpf.Tests/Shell/MainMenuBarTests.cs`
  - `docs/wpf-migration/inventory/*`
- Verification focus:
  - catalog shortcut definitions remain unique and command-backed
  - menu `InputGestureText` strings match catalog shortcut display text for the same command
  - local/global F-key distinctions remain collision-free
