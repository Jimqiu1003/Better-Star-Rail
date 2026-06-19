# ADR-0003: Repository Structure

Status: Accepted
Date: 2026-06-19

The repository uses `src/` for application code, `tests/` for xUnit tests, `docs/` for governance and architecture, `assets/` for project-owned assets only, `scripts/` for helper scripts, and `.github/` for collaboration and CI configuration.

The root preserves historical project documents. New normalized documents live in `docs/` and must not delete or rewrite the original project package.
