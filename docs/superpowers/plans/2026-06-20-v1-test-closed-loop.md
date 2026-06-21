# V1 Test Closed Loop Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 在项目自建测试窗口上实现可取消、可验证、默认停止的 V1 安全自动化闭环。

**Architecture:** 平台无关模型和策略位于 Core/Vision/Automation，Win32 只位于 Infrastructure.Windows，WPF 测试目标独立成工具项目。所有平台能力通过接口注入，以便 CI 使用确定性内存替身。

**Tech Stack:** C# 14、.NET 10、WPF、Win32 P/Invoke、xUnit。

---

### Task 1: 窗口与归一化坐标

**Files:** `src/BetterStarRail.Core/Windows/*`、`tests/BetterStarRail.Core.Tests/WindowModelTests.cs`

- [ ] 先写坐标边界、客户区转换、句柄/尺寸/DPI 变化测试并确认失败。
- [ ] 实现最小模型与变化检测，运行 Core 测试确认通过。

### Task 2: 捕获帧与页面识别

**Files:** `src/BetterStarRail.Vision/*`、`tests/BetterStarRail.Vision.Tests/*`、`tests/BetterStarRail.Vision.Tests/GoldenImages/*.ppm`

- [ ] 先写五页、负样本、低置信度、Unknown 和 ROI 测试并确认失败。
- [ ] 实现 BGRA 帧、统一结果和纯色锚点识别器，运行 Vision 测试确认通过。

### Task 3: SafetyGuard 与急停

**Files:** `src/BetterStarRail.Automation/Safety/*`、`tests/BetterStarRail.Automation.Tests/SafetyGuardTests.cs`

- [ ] 先写白名单、错误窗口、失焦、页面、变化、用户输入和急停拒绝测试并确认失败。
- [ ] 实现 fail-closed 守卫与急停控制器，运行 Automation 测试确认通过。

### Task 4: 两跳闭环状态机

**Files:** `src/BetterStarRail.Automation/Workflows/*`、`tests/BetterStarRail.Automation.Tests/TestClosedLoopWorkflowTests.cs`

- [ ] 先写成功、Unknown、低置信度、取消、超时、错误窗口和 100 次运行测试并确认失败。
- [ ] 实现每步重新验证、有限重试和动作后验证，运行 Automation 测试确认通过。

### Task 5: Windows 适配器与自建测试窗口

**Files:** `src/BetterStarRail.Infrastructure.Windows/*`、`tools/BetterStarRail.TestWindow/*`、解决方案文件。

- [ ] 添加测试窗口项目和原生适配器契约测试。
- [ ] 实现窗口查询、GDI 捕获、定向输入、资源释放与本地诊断。
- [ ] 构建并运行所有测试，确认不依赖游戏、网络或管理员权限。

### Task 6: 最终验收与报告

**Files:** `docs/project-status/V1-EXECUTION-REPORT.md`、`CHANGELOG.md`、应用版本配置。

- [ ] 更新版本与 V1 报告，逐项核对 Definition of Done。
- [ ] 运行 restore、format、Release build、test、diff check 和 status，记录实际结果。
