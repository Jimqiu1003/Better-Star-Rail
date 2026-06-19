# Development Rules

- Read `AGENTS.md` before work.
- Keep changes scoped.
- Do not change product boundaries without a new decision record.
- Do not bypass safety limits to make tests pass.
- Use C# 14, .NET 10 LTS, and WPF; do not introduce a .NET 8 target.
- Keep module names and dependency direction aligned with `Better-Star-Rail-仓库初始化任务书-v0.1.md`.
- Do not copy BetterGI or Better-HSR-Currency-Wars source code, assets, templates, wording, coordinates, thresholds, UI, or docs.
- Keep unattended operation disabled by default with a default maximum runtime of 60 minutes when enabled.
- Do not commit secrets, tokens, cookies, keys, local user paths, logs, or build output.
- Use Conventional Commits.
- Prefer tests that do not require the game, network, real keyboard, or real mouse.
- If code and docs conflict, update docs or record an ADR before merging behavior changes.
