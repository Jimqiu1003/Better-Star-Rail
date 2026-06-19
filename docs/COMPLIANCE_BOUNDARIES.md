# Compliance Boundaries

## Allowed

Screenshots, OCR, image recognition, window detection, keyboard and mouse input simulation through reviewed stoppable interfaces, user-configured workflows, local logs, operation recording/replay, and game-external launcher/configuration/task-management UI.

## Forbidden

Game process injection, DLL injection, hooks, modifying game memory, reading non-public game memory data, modifying game files, modifying network packets, spoofing client/server communication, bypassing anti-cheat systems, hiding the program to evade detection, avoiding bans or detection mechanisms, bypassing CAPTCHA or security verification, collecting account passwords, uploading screenshots/logs/personal data without explicit consent, and any feature intended to damage game fairness or bypass safety mechanisms.

When documents conflict, the safer and more restrictive rule wins until a new accepted ADR says otherwise.

## Unattended Runtime

Unattended operation is disabled by default. A user must explicitly enable it, the default maximum runtime is 60 minutes, and stop controls and fail-closed behavior must remain available throughout the run.

## Clean-room Rule

This repository is independently implemented under Apache License 2.0. Do not copy BetterGI or Better-HSR-Currency-Wars source code, assets, templates, wording, coordinates, thresholds, configuration, or derived rewrites. Public behavior may inform requirements only; implementation must originate from this project's own specifications and tests.
