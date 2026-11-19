## Why

Repeated slide backgrounds, full-size composite allocations and thumbnail rebuilds consume UI time and native bitmap memory during projection.

## What Changes

Reuse rendered file backgrounds and composite buffers, fill tiles in one operation, retain scaled thumbnail pixels, and share identical in-flight PowerPoint thumbnail requests.

## Capabilities

### New Capabilities
- `image-rendering` covers image reuse and thumbnail invalidation.

## Impact

WinForms background, transition and thumbnail drawing, and WPF preview and output thumbnail loading.
