# Third Party Notices

This repository is initialized as a clean-room project. Do not copy source code, assets, templates, wording, OCR assets, screenshots, UI, icons, configuration, coordinates, thresholds, or documentation from BetterGI, Better-HSR-Currency-Wars, game files, or any repository without explicit compatible permission.

The following direct dependencies are currently used. Versions are centrally managed in `Directory.Packages.props`; license expressions were verified from the restored NuGet package metadata.

| Dependency | Purpose | License |
|---|---|---|
| CommunityToolkit.Mvvm 8.4.0 | MVVM base types | MIT |
| Microsoft.Extensions.Configuration.Json 10.0.1 | JSON configuration | MIT |
| Microsoft.Extensions.DependencyInjection 10.0.1 | DI use in baseline tests | MIT |
| Microsoft.Extensions.Hosting 10.0.1 | Generic Host | MIT |
| Microsoft.Extensions.Options.ConfigurationExtensions 10.0.1 | Options binding | MIT |
| Serilog.Extensions.Hosting 10.0.0 | Host logging integration | Apache-2.0 |
| Serilog.Sinks.File 6.0.0 | Local rolling log files | Apache-2.0 |
| Microsoft.NET.Test.Sdk 17.14.1 | Test host | MIT |
| xunit 2.9.3 | Unit tests | Apache-2.0 |
| xunit.runner.visualstudio 3.1.5 | Test adapter | Apache-2.0 |
| coverlet.collector 6.0.4 | Test coverage | MIT |

GitHub workflows directly use `actions/checkout`, `actions/setup-dotnet`, `actions/upload-artifact`, and `github/codeql-action`. Transitive package notices remain governed by each restored package and must be reviewed again before a binary release.
