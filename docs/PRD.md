# Product Requirements Baseline

The source PRD is preserved in `Better-Star-Rail-v0.1-PRD.md`. This normalized document summarizes the accepted baseline.

The formal technical baseline is C# 14, .NET 10 LTS, and WPF. The repository is licensed under Apache License 2.0 and uses clean-room independent implementation.

Planned product features include game launch, trailblaze-power spending, routine battles, assignments, mail, daily rewards, activity rewards, OCR-assisted clicking, macro recording/replay, and explicitly configured unattended runs. Unattended runs are disabled by default and have a default maximum runtime of 60 minutes when enabled.

These are product plans, not initialized functionality. The repository initialization stage must not implement real game interaction.

The product must stop on uncertain UI states, low confidence, login/security verification, payment/resource-spending pages, unknown popups, or target-window loss. It must not promise zero bans, absolute safety, or official approval.
