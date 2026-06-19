# Technical Baseline Alignment Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Align the initialized repository with the approved C# 14, .NET 10 LTS, WPF, Apache-2.0, clean-room, and 60-minute unattended-run baseline.

**Architecture:** Keep the modular monolith and dependency graph from the repository initialization task book. Move the Windows-specific infrastructure project to its approved name, centralize the target framework and language version, and supersede conflicting historical decisions through a new ADR instead of deleting source records.

**Tech Stack:** C# 14, .NET 10 LTS, WPF, xUnit, GitHub Actions, Apache License 2.0.

---

### Task 1: Align build configuration and module identity

**Files:**
- Modify: `Directory.Build.props`
- Modify: `Directory.Packages.props`
- Modify: `BetterStarRail.sln`
- Modify: `src/BetterStarRail.App/BetterStarRail.App.csproj`
- Move: `src/BetterStarRail.Infrastructure/` to `src/BetterStarRail.Infrastructure.Windows/`
- Modify: all source and test project files

- [x] Set the shared target framework to `net10.0-windows` and language version to `14.0`.
- [x] Remove repeated target framework settings from individual projects.
- [x] Rename the Windows infrastructure project, assembly, namespace, solution entry, and project references.
- [x] Keep the dependency graph exactly as specified by the initialization task book.

### Task 2: Supersede conflicting governance rules

**Files:**
- Modify: `README.md`
- Modify: `AGENTS.md`
- Modify: `docs/PRD.md`
- Modify: `docs/ARCHITECTURE.md`
- Modify: `docs/COMPLIANCE_BOUNDARIES.md`
- Modify: `docs/DEVELOPMENT_RULES.md`
- Modify: `docs/decisions/ADR-0001-technology-stack.md`
- Create: `docs/decisions/ADR-0004-approved-baseline-supersession.md`

- [x] Declare C# 14, .NET 10 LTS, WPF, and Apache-2.0 as the formal baseline.
- [x] Declare a default maximum unattended runtime of 60 minutes.
- [x] Prohibit copying source, assets, templates, wording, coordinates, and thresholds from both named reference projects.
- [x] Record that ADR-0004 supersedes conflicting parts of earlier documents while preserving historical source files.

### Task 3: Align CI and verify the repository

**Files:**
- Modify: `.github/workflows/ci.yml`

- [x] Add `dotnet format --verify-no-changes` before the Release build.
- [ ] Run restore, format, Release build, and test locally.
- [x] Scan tracked files for secrets, build output, forbidden copied assets, and stale active-baseline references.
- [x] Run `git diff --check` and inspect `git status`.
- [ ] Commit, push without force, and verify the resulting GitHub Actions run.
