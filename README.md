# Better Star Rail

Better Star Rail is an unofficial C# Windows desktop automation framework for Honkai: Star Rail. The project is in V0 engineering-baseline stage: it provides governance documents, a WPF Generic Host shell, local configuration/logging foundations, CI, security analysis, and offline tests. It does not provide usable game automation.

## Safety and Compliance

This project is not affiliated with, endorsed by, sponsored by, or approved by miHoYo, HoYoverse, or the official Honkai: Star Rail team.

Using third-party automation tools may violate game terms of service and may lead to warnings, restrictions, suspension, or permanent account bans. The project does not promise account safety, long-term availability, or zero risk.

Allowed research directions include screenshots, OCR, image recognition, window detection, local logs, user-configured workflows, controlled keyboard/mouse automation, and stop-anytime task execution.

Forbidden directions include game process injection, DLL injection, hooks, memory reading/writing, game file modification, packet modification, spoofed client/server communication, anti-cheat bypass, hidden execution to evade detection, CAPTCHA or security verification bypass, credential collection, and uploads of screenshots/logs/personal data without explicit user consent.

## Current Stack

- C# 14 and .NET 10 LTS target framework
- Windows 10/11 x64
- WPF application shell
- Generic Host, dependency injection, CommunityToolkit.Mvvm, and Serilog
- Modular monolith structure
- xUnit test projects
- GitHub Actions CI

The approved baseline is C# 14, .NET 10 LTS, and WPF. `docs/decisions/ADR-0004-approved-baseline-supersession.md` supersedes conflicting .NET 8 and GPL proposals in historical source documents.

User-enabled unattended runs must default to a maximum duration of 60 minutes and remain stoppable at any time.

## Build

```powershell
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
```

Runtime logs are stored under `%LocalAppData%\BetterStarRail\logs\`. Optional user configuration belongs under `%LocalAppData%\BetterStarRail\config\`; neither location is inside the repository.

The active V0 decisions are summarized in `docs/project-status/V0-BASELINE.md`.

## License

The project uses Apache License 2.0 for clean-room, independently authored code. BetterGI and Better-HSR-Currency-Wars source code, assets, templates, wording, coordinates, and thresholds must not be copied.
