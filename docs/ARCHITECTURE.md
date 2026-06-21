# Architecture

The initial repository uses a modular monolith:

- `BetterStarRail.App`: WPF application shell and composition root.
- `BetterStarRail.Core`: domain types, result models, and stable abstractions.
- `BetterStarRail.Infrastructure.Windows`: Windows-specific configuration, logging, files, capture, input, and system-service implementations.
- `BetterStarRail.Automation`: controlled workflow abstractions and orchestration.
- `BetterStarRail.Vision`: screenshot, OCR, template, and coordinate abstractions.

Dependency direction flows toward `Core`. Automation may not call Win32 input APIs directly. Vision may not control input. App composes implementations but should not contain business logic.

The allowed project references are:

```text
BetterStarRail.App -> Core, Automation, Vision, Infrastructure.Windows
BetterStarRail.Automation -> Core
BetterStarRail.Vision -> Core
BetterStarRail.Infrastructure.Windows -> Core, Automation, Vision
```

No other production-project dependency or circular reference is allowed without an accepted ADR.

The approved automation model is external vision: capture visible screen state, detect UI state, decide, perform controlled input, then verify the result.

V1 implements this sequence only for the project-owned test window. GDI `BitBlt` is the V1 test capture backend, solid-color anchors are deterministic test recognition rather than OCR, and targeted window messages are restricted to the exact saved test-window identity. Real-game capture and input are not implemented. Windows Graphics Capture remains the preferred candidate for a future real-window phase and must be separately validated before use.
