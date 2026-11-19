## 1. WinForms rendering

- [x] 1.1 Reuse file backgrounds and verify tile edges and alpha.
- [x] 1.2 Reuse composite surfaces and verify active frames and failure recovery.
- [x] 1.3 Cache scaled thumbnail pixels and verify marker changes and replacement.

## 2. WPF thumbnails

- [x] 2.1 Share identical thumbnail loads and verify invalidation, cancellation and retry.

## 3. Verification

- [x] 3.1 Run regression suites, verify the OpenSpec change and compare rendering measurements.

## Verification

- Release solution tests passed on Windows: 205 WinForms, 2,476 WPF and 20 analyzer tests.
- A 3840 × 2160 Release benchmark used two warmups and the median of nine repetitions. Repeating the same JPEG took 78.41 ms before and 0.03 ms after. Drawing a fresh 16 × 16 tile background took 219.27 ms before and 20.63 ms after. Instant frame composition took 38.93 ms before and 28.83 ms after.
- The 160 × 90 example retained a 532 × 300 content raster instead of the original 3840 × 2160 image. Rendered thumbnails matched the original pixel for pixel for a JPEG and a translucent PNG in all three layouts.
- The WPF shell probe held thumbnail 2 pending while repeating a five-slide deck selection. The repeated selection requested the main slide once, with zero thumbnail cancellations.
- The 32-bit and 64-bit views of the EasiSlides production registry had identical fingerprints before and after verification.
