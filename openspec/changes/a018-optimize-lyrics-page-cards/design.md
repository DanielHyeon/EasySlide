## Context

Both operator panels build cards from the raw navigation model. Display text can also depend on transpose, notation visibility, Bible quotation handling and external text file rules.

## Goals / Non-Goals

Reduce work for repeated verses and retain the same page numbering, labels and display text for each item kind.

## Decisions

Keep prepared display pages local to one card build. Share the indexed text selection with single-page output. Refresh from current item and settings on every build.

## Risks / Trade-offs

The navigation count and projected body count can differ for unusual markup or external files. Preserve index clamping and title fallback.
