# AGENTS.md

This file is mandatory reading for Codex, Claude Code, GitHub Copilot Agent, and any other automated contributor before changing this repository.

## Project Goal

Better Star Rail is an unofficial Windows desktop automation framework for Honkai: Star Rail. The project focuses on external screen vision, OCR, window detection, and user-controlled input automation. It must remain a clean-room implementation.

## Current Phase

Repository initialization and engineering baseline. Do not implement real game automation in this phase.

## Allowed Work

Documentation, governance, tests, CI, repository structure, deterministic placeholder logic, local-only configuration/logging foundations, and abstractions for screen/OCR/input work.

## Forbidden Work

Game process injection, DLL injection, hooks, memory reading/writing, game file modification, packet modification, spoofed client/server communication, anti-cheat bypass, hidden execution to evade detection, CAPTCHA/security-verification bypass, credential collection, and uploads without explicit user consent.

Do not claim absolute safety, zero ban risk, official approval, or long-term guaranteed availability. Do not copy code, templates, OCR assets, screenshots, icons, documents, coordinates, thresholds, or configuration from reference projects or game files.

## Required Checks

Before work, read `README.md`, `docs/COMPLIANCE_BOUNDARIES.md`, `docs/DEVELOPMENT_RULES.md`, `docs/RISK_REGISTER.md`, `docs/ARCHITECTURE.md`, and `docs/decisions/*.md`.

After relevant changes, run `dotnet restore`, `dotnet build --configuration Release`, `dotnet test --configuration Release`, `git diff --check`, and `git status --short` when available.

Use Conventional Commits. If code and docs conflict, record the conflict and follow the safer rule until the project owner decides.
