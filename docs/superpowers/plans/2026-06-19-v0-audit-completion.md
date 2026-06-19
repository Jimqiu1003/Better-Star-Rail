# V0 Audit and Completion Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Turn the current repository skeleton into a verified V0 engineering baseline with frozen governance, a real WPF Generic Host startup path, safe abstractions, meaningful tests, and complete GitHub automation.

**Architecture:** Preserve the task-book modular monolith and keep all production dependencies within the approved graph. The WPF app remains the composition root, Core owns stable contracts, Infrastructure.Windows owns LocalAppData paths, Automation contains deterministic orchestration only, and Vision contains interface/model boundaries without real capture or input.

**Tech Stack:** C# 14, .NET 10 LTS, WPF, Generic Host, CommunityToolkit.Mvvm, Serilog, xUnit, GitHub Actions, CodeQL, Dependabot.

---

### Task 1: Freeze the V0 governance baseline

**Files:**
- Create: `docs/project-status/V0-BASELINE.md`
- Create: `PRIVACY.md`
- Create: `LEGAL_NOTICE.md`
- Modify: `AGENTS.md`
- Modify: `README.md`
- Modify: `THIRD_PARTY_NOTICES.md`
- Modify: `docs/COMPLIANCE_BOUNDARIES.md`
- Modify: `docs/decisions/ADR-0004-approved-baseline-supersession.md`

- [x] Record the active stack, license, scope, red lines, repository structure, Definition of Done, superseded decisions, and future change rule in `V0-BASELINE.md`.
- [x] Add root privacy and legal notices that point to the detailed policies and repeat the non-official/account-risk boundary.
- [x] Require future agents to read the baseline, PRD, ADRs, risk register, absolute prohibitions, and initialization task book.
- [x] Freeze unattended limits at 60 minutes, 3 consecutive failures, 5 repeated same-page actions, emergency stop, and watchdog.
- [x] Replace the placeholder third-party notice with the actual directly used NuGet and GitHub Actions inventory.

### Task 2: Add deterministic orchestration behavior with TDD

**Files:**
- Create: `src/BetterStarRail.Automation/Abstractions/IAutomationWorkflow.cs`
- Modify: `src/BetterStarRail.Automation/Services/AutomationCoordinator.cs`
- Modify: `tests/BetterStarRail.Automation.Tests/AutomationCoordinatorTests.cs`

- [x] Add tests proving cancellation is observed, failed steps are returned unchanged, and an empty workflow completes without executing a step.
- [x] Run the Automation test project and verify the new workflow test fails because the API does not exist.
- [x] Implement only deterministic workflow iteration and stop on the first failed step.
- [x] Re-run the Automation tests and verify all pass.

### Task 3: Add LocalAppData paths and WPF service composition with TDD

**Files:**
- Create: `src/BetterStarRail.Core/Abstractions/IAppPathProvider.cs`
- Create: `src/BetterStarRail.Core/Diagnostics/AppStatus.cs`
- Modify: `src/BetterStarRail.Infrastructure.Windows/Storage/LocalAppDataPathProvider.cs`
- Create: `src/BetterStarRail.App/Configuration/ApplicationOptions.cs`
- Create: `src/BetterStarRail.App/Hosting/ApplicationHost.cs`
- Create: `src/BetterStarRail.App/appsettings.json`
- Modify: `src/BetterStarRail.App/App.xaml`
- Modify: `src/BetterStarRail.App/App.xaml.cs`
- Modify: `src/BetterStarRail.App/MainWindow.xaml`
- Modify: `src/BetterStarRail.App/MainWindow.xaml.cs`
- Modify: `src/BetterStarRail.App/ViewModels/MainWindowViewModel.cs`
- Modify: `src/BetterStarRail.App/BetterStarRail.App.csproj`
- Modify: `tests/BetterStarRail.Core.Tests/BetterStarRail.Core.Tests.csproj`
- Create: `tests/BetterStarRail.Core.Tests/ApplicationBaselineTests.cs`

- [x] Add failing tests for `%LocalAppData%/BetterStarRail/logs`, `%LocalAppData%/BetterStarRail/config`, directory creation cleanup, and DI resolution of `MainWindowViewModel`.
- [x] Run the Core test project and verify failures are caused by missing contracts/composition.
- [x] Implement the path provider, options binding, Generic Host, Serilog file logging, MVVM registration, injected window/ViewModel, and global exception logging.
- [x] Re-run the Core tests and verify all pass without accessing the game, network, mouse, or keyboard.

### Task 4: Complete safety-oriented interfaces and tests

**Files:**
- Create: `src/BetterStarRail.Vision/Abstractions/IScreenCaptureService.cs`
- Modify: `tests/BetterStarRail.Vision.Tests/CaptureRegionTests.cs`

- [x] Add the missing negative-height test and verify existing validation passes it.
- [x] Add an interface-only, cancellable screen-capture boundary with no implementation and no Win32 call.
- [x] Scan production code for `SendInput`, process launch, injection, hooks, memory APIs, network calls, and administrator manifests.

### Task 5: Complete GitHub and repository governance

**Files:**
- Rename: `.github/workflows/ci.yml` to `.github/workflows/build.yml`
- Create: `.github/workflows/codeql.yml`
- Create: `.github/dependabot.yml`
- Rename: `.github/PULL_REQUEST_TEMPLATE.md` to `.github/pull_request_template.md`
- Modify: `.github/workflows/build.yml`
- Modify: `.github/pull_request_template.md`
- Modify: `CHANGELOG.md`

- [x] Make build CI run restore, format, Release build, tests with XPlat coverage, and upload only test results.
- [x] Add least-privilege C# CodeQL for push, PR, schedule, and manual dispatch.
- [x] Add weekly NuGet and GitHub Actions Dependabot with bounded open PRs.
- [x] Strengthen PR provenance and V0 safety declarations.

### Task 6: Verify WPF runtime and repository safety

**Files:**
- Modify only if verification finds a reproducible defect.

- [x] Run restore, format, Release build, and all tests.
- [ ] Launch the WPF app, locate its window, close it normally, and confirm process exit.
- [x] Confirm logs are written under LocalAppData and no logs/config are written in the repository.
- [x] Confirm no game installation, network, administrator rights, or real input is required.
- [x] Scan secrets, privacy paths, binaries, game assets, build output, and forbidden implementation terms.
- [x] Run `git diff --check` and review the complete diff.

### Task 7: Commit, push, and verify GitHub

**Files:**
- No new production files; commit verified changes only.

- [ ] Create scoped Conventional Commits for docs, engineering/tests, and CI as warranted by the final diff.
- [ ] Push normally to the existing `origin/main` without rewriting history.
- [ ] Verify build and CodeQL runs for the pushed head commit.
- [ ] Record the final clean Git status, commit hashes, remote, CI URLs, and weighted V0 score.
