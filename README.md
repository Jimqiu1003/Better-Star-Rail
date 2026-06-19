# Better Star Rail

Better Star Rail is an unofficial C# Windows desktop automation framework for Honkai: Star Rail. The project is in repository-initialization stage only: it currently provides governance documents, a .NET solution skeleton, CI configuration, and minimal tests. It does not provide usable game automation yet.

## Safety and Compliance

This project is not affiliated with, endorsed by, sponsored by, or approved by miHoYo, HoYoverse, or the official Honkai: Star Rail team.

Using third-party automation tools may violate game terms of service and may lead to warnings, restrictions, suspension, or permanent account bans. The project does not promise account safety, long-term availability, or zero risk.

Allowed research directions include screenshots, OCR, image recognition, window detection, local logs, user-configured workflows, controlled keyboard/mouse automation, and stop-anytime task execution.

Forbidden directions include game process injection, DLL injection, hooks, memory reading/writing, game file modification, packet modification, spoofed client/server communication, anti-cheat bypass, hidden execution to evade detection, CAPTCHA or security verification bypass, credential collection, and uploads of screenshots/logs/personal data without explicit user consent.

## Current Stack

- C# and .NET 8 LTS target framework
- Windows 10/11 x64
- WPF application shell
- Modular monolith structure
- xUnit test projects
- GitHub Actions CI

The legacy ADR package also contains .NET 10 proposals. This repository uses .NET 8 for the initial baseline because the current initialization request and PRD require .NET 8 LTS. The conflict is recorded in `docs/decisions/ADR-0001-technology-stack.md`.

## Build

```powershell
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
```

## License

The project is initialized under Apache-2.0 for independently authored code. Third-party material must be reviewed and recorded before use.
