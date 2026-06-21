# Better Star Rail V1 主线集成状态

状态：Completed and integrated into main

开发版本：`0.2.0-dev`

V1 已通过 Pull Request #11 完成人工审查并合并到 `main`。V0 已完成，V2 仍为 Planned，尚未开始。`v0.1.0` 是未来产品发布版本；当前没有 Release 或 tag。

## V1 范围

- 项目自建原生测试窗口；
- 窗口身份、前台、最小化、客户区、DPI 和用户输入变化检测；
- 归一化客户区坐标；
- GDI 客户区与 ROI 截图；
- Welcome、Confirm、Progress、Complete、Blocked 五种页面与 UnknownState；
- SafetyGuard、动作白名单和输入前双重窗口状态复核；
- Welcome → Confirm → Complete 两跳闭环；
- 动作后重新截图验证；
- CancellationToken、有限轮询、急停和派发失败释放；
- LocalAppData 本地诊断；
- 可复现的原生窗口验证脚本。

## 验证结果

- 自动化测试：59/59；
- 确定性闭环：100/100；
- 原生测试窗口闭环：100/100；
- 错误窗口输入调用：0；
- 失焦、最小化、窗口位置/尺寸/DPI 变化、用户输入、低置信度和 UnknownState：均拒绝输入并安全停止；
- 急停：阻止新输入，并使用不可取消的释放路径释放允许的按键和鼠标状态；
- 诊断：仅写入 `%LocalAppData%\BetterStarRail\diagnostics\`，无上传路径。

## 明确不包含

V1 不包含真实游戏业务、自动刷本、开拓力、委托和奖励、宏录制、无人值守、游戏启动、内存读写、注入、Hook、私有协议、反检测、任意脚本执行或远程控制。

GDI `BitBlt` 仅为 V1 自建测试窗口实现。真实游戏相关阶段仍需单独验证 Windows Graphics Capture；该工作不属于 V1，也不得在主线集成收口中开始。

## 许可证与来源

项目当前主许可证为 Apache License 2.0，采用 clean-room 独立实现。本次 V1 未新增 NuGet 包、原生 DLL、模型、字体、图片或第三方资产。文本 PPM 是项目自制的 2×2 测试夹具，不含游戏素材。BetterGI 的 GPL-3.0 仅作为第三方参考项目许可证记录；无许可证仓库只允许观察公开功能，不得复制代码、素材或文案。
