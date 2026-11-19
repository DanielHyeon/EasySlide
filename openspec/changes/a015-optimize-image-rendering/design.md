## Context

Preview and output have independent lifetimes. GDI+ bitmaps may be referenced by several picture fields during a transition. PowerPoint thumbnails arrive asynchronously.

## Decisions

Keep one file-background identity on its owning control. Keep composite buffers private until all layers draw successfully. A texture brush repeats tiles from the origin. Rasterize thumbnail content at the existing display-cache resolution with a 1,024-pixel limit on the longer edge. Normalize translucent source pixels before scaling to preserve legacy colors. Keep the original dimensions for layout and redraw markers from the scaled pixels. Key each WPF strip by deck version, count and normalized render size. Resume incomplete strips at their missing or undersized slides. Preview and output keep independent selection markers. Publish only while that request is current.

## Risks / Trade-offs

File identity uses modification time and length. Replacement with identical metadata needs explicit invalidation. Compare tile pixels with a reference draw and interrupt asynchronous loads in tests.
