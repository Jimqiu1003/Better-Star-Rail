# Risk Register

| Risk | Probability | Impact | Level | Mitigation | Accepted | Owner Module |
|---|---|---|---|---|---|---|
| Game account penalty | High | Critical | Extreme | Clear warnings, no safety promises, no evasion features, user-controlled stop | Yes, residual risk remains | Product |
| Terms of service violation | High | Critical | Extreme | Disclose risk, avoid official branding, stop unsafe scope | Yes | Product |
| Open-source misuse | Medium | High | High | Code of conduct, issue/PR rules, reject unsafe features | Partially | Governance |
| OCR misoperation | High | High | Extreme | ROI, confidence thresholds, multi-signal validation, stop on uncertainty | Not for release until tested | Vision |
| Resolution/UI changes | High | High | Extreme | Compatibility matrix, versioned assets, fail closed | Not for release until tested | Vision |
| Unattended abnormal operation | Medium | Critical | Extreme | Default off, time limits, emergency stop, watchdog, reports | Only with controls | Automation |
| Privacy and screenshot leakage | Medium | Critical | Extreme | Local-first, no upload by default, redaction, export review | Only with controls | Infrastructure |
| Third-party dependency supply chain | Medium | High | High | Central package management, license review, Dependabot, notices | Yes with monitoring | Repository |
| Administrator permission risk | Medium | High | High | Default normal permissions, explain any elevation, avoid system changes | Only by exception | Infrastructure |
| Automatic update risk | Medium | Critical | Extreme | No auto-update in v0.1; future signing and hash verification required | No for v0.1 | Release |
| Logs leaking personal data | Medium | High | High | Avoid credentials, redact local paths, bounded diagnostics | Only with controls | Infrastructure |
