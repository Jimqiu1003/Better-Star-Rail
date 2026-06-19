# ADR-0001: Technology Stack

Status: Superseded by ADR-0004
Date: 2026-06-19

## Context

Project documents contain conflicts: the PRD and current initialization request require C# with .NET 8 LTS, while the earlier architecture ADR and initialization task book proposed .NET 10 LTS. The early charter also mentioned GPL-3.0-only as a possible license, while the risk register and task book select Apache-2.0 for clean-room code.

## Decision

Use C# with `net8.0-windows` for the initial repository baseline, WPF for the desktop shell, modular monolith project structure, x64 as the primary architecture, nullable enabled, and xUnit for tests. Use Apache-2.0 for independently authored code while preserving third-party notices.

## Reason

The current explicit request and PRD are treated as higher priority for runtime. The risk register and repository task book are more specific for clean-room licensing. Any copied GPL or unlicensed third-party code remains forbidden.

## Consequences

The existing .NET 10 and GPL notes are preserved in source documents for history. ADR-0004 replaces this runtime decision with the approved C# 14 and .NET 10 LTS baseline.
