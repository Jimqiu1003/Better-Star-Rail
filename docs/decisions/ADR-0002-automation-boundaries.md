# ADR-0002: Automation Boundaries

Status: Accepted
Date: 2026-06-19

Better Star Rail may only use external, user-visible automation techniques: screenshots, OCR, image recognition, window detection, local logs, user-configured workflows, and controlled input simulation.

The project must not implement injection, DLL injection, game-memory access, hooks, game-file modification, packet modification, anti-cheat bypass, hidden execution to evade detection, security-verification bypass, credential collection, or non-consensual upload of screenshots/logs/personal data.

When uncertain, stop and ask for a project-owner decision. Safer and stricter rules override more permissive text.
