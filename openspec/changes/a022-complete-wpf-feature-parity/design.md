## Context

The console already has WPF equivalents for many library, worship-list and output operations. The operator's source selection, prepared Output item and currently visible live state can differ.

## Decisions

Resolve each toolbar or menu action against its WinForms handler. Reuse WPF services for the same operation. Treat live display transformations as transient projection state. Use the WPF settings store for preferences and isolate test data from production.
