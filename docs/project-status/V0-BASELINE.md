# Better Star Rail V0 现行基线

状态：Accepted（已接受）

生效日期：2026-06-19

本文档是 V0 阶段唯一的现行汇总基线。历史 PRD、立项包或 ADR 与本文冲突时，以本文和 `docs/decisions/ADR-0004-approved-baseline-supersession.md` 为准；历史文件保留用于审计，不代表当前可执行结论。

## 技术栈

- C# 14；
- .NET 10 LTS，目标框架 `net10.0-windows`；
- WPF、MVVM、CommunityToolkit.Mvvm；
- .NET Generic Host、Microsoft.Extensions.Configuration；
- Serilog 本地结构化日志；
- xUnit；
- Windows 10/11 x64；
- 模块化单体；
- 外部视觉自动化路线；
- Apache License 2.0 与 clean-room 独立实现。

## V0 范围

V0 冻结项目章程、PRD、范围、风险、合规、技术架构和仓库规则，并交付可克隆、还原、编译、测试、启动和持续开发的工程基线。

V0 只包含桌面壳、Host/DI、配置、日志、LocalAppData 路径、安全接口、确定性占位逻辑、测试和 GitHub 治理。V0 不提供可用的游戏自动化功能。

## 绝对红线

禁止真实游戏自动化、游戏窗口控制、OCR 自动点击、实际键鼠输入、游戏启动、无人值守挂机、注入、Hook、内存读写、驱动、封包修改、反作弊绕过、反检测、凭据读取，以及任意脚本或插件执行。

禁止复制 BetterGI、Better-HSR-Currency-Wars 或其他参考项目的源码、素材、模板、文案、坐标、阈值和配置，也禁止基于其实现进行翻译、改写或生成式重写。

## 无人值守冻结规则

该能力不属于 V0 实现范围。未来若获准实现，必须同时满足：

- 默认关闭，由用户主动开启；
- 默认单次最大运行 60 分钟；
- 连续失败 3 次停止；
- 同一页面重复动作 5 次停止；
- 必须有独立紧急停止和看门狗；
- 未知页面、验证页面、目标窗口丢失或低置信度时安全停止。

## 目录与依赖

```text
src/
  BetterStarRail.App/
  BetterStarRail.Core/
  BetterStarRail.Automation/
  BetterStarRail.Vision/
  BetterStarRail.Infrastructure.Windows/

tests/
  BetterStarRail.Core.Tests/
  BetterStarRail.Automation.Tests/
  BetterStarRail.Vision.Tests/
```

允许的生产项目引用：

```text
App -> Core, Automation, Vision, Infrastructure.Windows
Automation -> Core
Vision -> Core
Infrastructure.Windows -> Core, Automation, Vision
Core -> 无生产项目依赖
```

## V0 Definition of Done

- 立项、产品范围、风险和合规结论已冻结；
- Apache-2.0 与 clean-room 规则已落地；
- .NET 10、C# 14、WPF、x64 和集中包管理已落地；
- WPF 使用 Generic Host、MVVM、配置和 Serilog，并能正常启动关闭；
- 日志写入 `%LocalAppData%\BetterStarRail\logs\`，用户配置位于 `%LocalAppData%\BetterStarRail\config\`；
- 测试不依赖游戏、网络、管理员权限或真实输入；
- Restore、Format、Release Build、Test 和 GitHub Actions 通过；
- CodeQL、Dependabot、Issue/PR 模板和治理文件齐全；
- 仓库不包含密钥、个人数据、游戏素材、来源不明二进制、日志或构建垃圾；
- 完成 WPF 运行验收并保留真实执行报告。

## 已替代决策

- `docs/decisions/ADR-0001-technology-stack.md`：Superseded，被 ADR-0004 替代；
- 历史 PRD 中的 .NET 8 和 90 分钟限制：不再适用；
- 历史立项包中的 GPL-3.0-only 建议和参考项目代码复用路线：不再适用；
- 旧基础设施模块名 `BetterStarRail.Infrastructure`：已替代为 `BetterStarRail.Infrastructure.Windows`。

## 修改规则

任何改变技术版本、许可证、红线、无人值守限制、模块命名或依赖方向的变更，必须先取得项目负责人确认并新增 ADR。Agent 不得通过代码、测试豁免或文档重写静默绕过本基线。

## 依据文件

- 产品：`Better-Star-Rail-v0.1-PRD.md`、`docs/PRD.md`；
- 架构：`Better-Star-Rail_技术架构决策记录_ADR_v0.1.md`、`docs/ARCHITECTURE.md`、`docs/decisions/`；
- 风险：`Better-Star-Rail-v0.1-风险清单.md`、`docs/RISK_REGISTER.md`；
- 红线：`Better-Star-Rail-v0.1-视觉自动化立项包/docs/05-绝对禁止项.md`；
- 范围：`Better-Star-Rail-v0.1-视觉自动化立项包/docs/11-范围矩阵.md`；
- 发布门禁：`Better-Star-Rail-v0.1-视觉自动化立项包/docs/09-发布门禁.md`；
- 故障安全：`Better-Star-Rail-v0.1-视觉自动化立项包/docs/07-故障安全设计.md`；
- 仓库执行：`Better-Star-Rail-仓库初始化任务书-v0.1.md`。
