# Changelog

## Unreleased

- 完成 V1 自建测试窗口安全闭环（仅限项目自建窗口，不含真实游戏业务）：
  - 自建原生测试窗口，以及 Welcome、Confirm、Progress、Complete、Blocked 五种页面与 UnknownState；
  - 窗口身份、前台、最小化、客户区、DPI 和用户输入变化检测；
  - 归一化客户区坐标、GDI 客户区/ROI 截图和项目自制纯色锚点识别；
  - SafetyGuard、动作白名单、输入前双重窗口复核和派发失败兜底释放；
  - Welcome → Confirm → Complete 两跳闭环，以及每次动作后的重新截图验证；
  - 可取消的有限状态机、急停、新输入阻止和按键/鼠标释放；
  - `%LocalAppData%\BetterStarRail\diagnostics\` 本地失败诊断；
  - 确定性 100 次闭环与原生测试窗口 100 次闭环验证工具。
- V1 收口验证结果：59/59 自动化测试通过，确定性闭环 100/100，原生测试窗口闭环 100/100，错误窗口输入调用 0。
- GDI `BitBlt` 仅作为 V1 测试实现；真实游戏相关阶段仍需验证 Windows Graphics Capture。

- Initialize repository governance documents.
- Add .NET solution skeleton for Better Star Rail.
- Add WPF Generic Host, MVVM, LocalAppData configuration, and Serilog logging foundations.
- Add deterministic workflow orchestration and V0 safety tests.
- Add build, coverage, CodeQL, Dependabot, issue templates, and project rules.
