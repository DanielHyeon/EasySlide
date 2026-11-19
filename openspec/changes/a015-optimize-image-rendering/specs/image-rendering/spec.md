## ADDED Requirements

### Requirement: File backgrounds and transitions
A screen SHALL reuse its rendered file background for matching file versions, dimensions and modes, preserve tile pixels and compose layers into reusable private buffers.

#### Scenario: Repeated or changed backgrounds
- **WHEN** a screen requests the same background twice and then changes its file, mode or geometry
- **THEN** it reuses the first raster on the second request and renders the changed background

#### Scenario: Transition restarts
- **WHEN** a new composite is drawn during a transition
- **THEN** frames in use remain valid, transparent pixels replace old pixels and failed composition preserves the published frames

### Requirement: Thumbnail reuse
WinForms SHALL retain thumbnail-sized content for marker updates. Each WPF preview or output strip SHALL coalesce identical thumbnail work and replace obsolete loads.

#### Scenario: Marker redraw
- **WHEN** a selection or next-slide border changes
- **THEN** the thumbnail redraws from scaled pixels with its original aspect ratio and slide numbering

#### Scenario: Deck changes during loading
- **WHEN** identical requests overlap or a different deck version or render size is requested
- **THEN** identical requests share a load, only current images publish and an incomplete strip can retry
