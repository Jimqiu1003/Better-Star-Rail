# Better Star Rail

Better Star Rail 是一个非官方的 C# Windows 桌面视觉自动化框架，面向《崩坏：星穹铁道》相关的工程学习与安全研究。当前开发版本为 `0.2.0-dev`，尚未提供真实游戏自动化、自动刷本或完整产品能力。

## 当前状态

| 开发阶段 | 状态 | 范围 |
|---|---|---|
| V0 | Completed | 仓库治理、WPF/Host/MVVM 工程基线、配置、日志、测试与 CI |
| V1 | Completed and integrated into main | 仅在项目自建测试窗口上完成截图、识别、安全守卫、定向输入、动作后验证与急停闭环 |
| V2 | Planned | 尚未开始，范围需由项目负责人另行确认 |

V0、V1、V2 表示开发阶段；M0、M1、M2 表示里程碑；`0.2.0-dev` 是当前开发版本；`v0.1.0` 是未来产品发布版本，不代表当前已有 Release。

V1 使用 GDI `BitBlt` 作为自建测试窗口的验证实现。进入真实游戏相关阶段前，仍需单独验证 Windows Graphics Capture 的兼容性、性能与安全边界。当前代码不会查找、启动或操作真实游戏窗口。

## 安全与合规

本项目与米哈游、HoYoverse 或《崩坏：星穹铁道》官方不存在隶属、赞助、认可或批准关系。第三方自动化可能违反游戏服务条款，并可能导致警告、限制、暂停或永久封禁；项目不承诺账号安全、长期可用、零风险或官方认可。

永久禁止游戏进程注入、DLL 注入、Hook、内存读写、游戏文件或封包修改、私有协议或未公开 API、反作弊绕过、反检测、隐藏执行、验证码绕过、凭据采集、任意脚本执行、远程控制，以及未经用户明确同意上传截图、日志或个人数据。

无人值守不属于 V1。未来若经批准实现，必须默认关闭、由用户显式启用、默认单次最长运行 60 分钟，并保持可停止和不确定状态下 fail-closed（故障即安全停止）。

## 技术基线

- C# 14、.NET 10 LTS、`net10.0-windows`
- WPF、MVVM、CommunityToolkit.Mvvm
- Windows 10/11、win-x64
- Generic Host、依赖注入、Serilog
- 模块化单体与 xUnit

`docs/decisions/ADR-0004-approved-baseline-supersession.md` 已取代历史材料中的 .NET 8、GPL 主许可证和 90 分钟无人值守建议。

## 本地构建

```powershell
dotnet restore
dotnet format --verify-no-changes
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build
```

完成 Release 构建后，可在交互式 Windows 桌面运行项目自建窗口的原生闭环验证：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\verify-v1-native.ps1 -Iterations 100
```

运行日志、配置和 V1 失败诊断仅写入 `%LocalAppData%\BetterStarRail\` 下的对应目录，不写入仓库，也不会自动上传。

## 许可证与来源

项目 clean-room 独立实现的代码使用 Apache License 2.0，完整正文见 `LICENSE`。

BetterGI 的 GPL-3.0 仅作为第三方参考项目许可证记录；本项目不得复制其代码、素材、模板、文案、坐标、阈值或配置，除非未来重新完成许可证决策。对无许可证仓库只允许观察公开功能，不得复制代码、素材或文案。第三方依赖见 `THIRD_PARTY_NOTICES.md`。
