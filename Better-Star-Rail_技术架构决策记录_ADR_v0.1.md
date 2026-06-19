# Better Star Rail 技术架构决策记录（ADR）v0.1

> 历史文档提示：其中关于 GPL 复用、基础设施旧命名等冲突内容已被 `docs/decisions/ADR-0004-approved-baseline-supersession.md` 覆盖。现行规则是 Apache-2.0 clean-room 独立实现，并禁止复制 BetterGI 或 Better-HSR-Currency-Wars 的源码、素材、模板、文案、坐标和阈值。

> 文档状态：**已接受（Accepted）**  
> 决策日期：2026-06-19  
> 适用版本：Better Star Rail v0.1～v0.x  
> 决策负责人：项目维护者  
> 复审节点：首次公开测试前、游戏大版本导致识别体系失效时、v1.0 立项时

---

## 1. 文档目的

本文用于冻结 Better Star Rail v0.1 的核心技术路线，避免开发中途频繁更换框架、重复造轮子或把业务逻辑与截图、OCR、键鼠模拟强耦合。

本记录只决定“当前阶段必须统一的架构规则”。任何与本文冲突的实现，必须先新增 ADR，说明变更原因、影响范围、迁移方案和回滚方式。

---

## 2. 项目背景与约束

Better Star Rail 是一个面向 Windows PC 版《崩坏：星穹铁道》的开源桌面辅助工具，计划使用 C# 开发。v0.1 的目标功能包括：

- 自动启动游戏并检测游戏窗口；
- 自动消耗开拓力、刷取指定副本并完成战斗流程；
- 自动完成或领取委托；
- 自动领取邮件、每日训练及活动奖励；
- 基于截图、模板匹配和 OCR 识别界面，再执行键鼠操作；
- 支持键鼠宏录制、回放和任务调度；
- 支持用户主动开启的无人值守流程；
- 全程保留清晰日志、停止入口和故障截图。

### 2.1 硬约束

1. 仅支持 Windows 10/11 x64。
2. 不读取或修改游戏进程内存。
3. 不注入 DLL、不安装驱动、不修改游戏文件、不篡改网络协议。
4. 不实现反检测、进程隐藏、反反作弊或绕过安全机制。
5. 不保存用户账号密码，不自动填写账号凭据。
6. v0.1 优先保证稳定、可维护、可诊断，不追求一次覆盖所有分辨率和全部游戏内容。
7. 开源发布前必须完成第三方依赖许可证审计。

---

## 3. 总体结论

Better Star Rail v0.1 采用以下总体方案：

- **C# + .NET 10 LTS + WPF**；
- **MVVM + .NET Generic Host + 依赖注入**；
- **模块化单体（Modular Monolith）**，不采用微服务；
- **外部视觉自动化**：截图 → 状态识别 → 决策 → 输入 → 结果验证；
- OpenCvSharp 负责传统视觉，ONNX Runtime 负责本地模型推理；
- OCR、截图、输入均通过接口隔离，可独立替换；
- 自动化流程采用可取消、可恢复的状态机，不使用大段固定延时脚本；
- 设置使用 JSON，运行历史和任务状态使用 SQLite；
- 默认本地运行、默认无遥测、默认不上传截图；
- 若复用 BetterGI 的 GPL 代码，整个衍生项目按 GPL-3.0 兼容方式发布；未授权仓库只参考思路，不复制代码。

---

# 4. 架构决策

## ADR-001：运行平台采用 .NET 10 LTS

**状态：已接受**

### 决策

- Target Framework：`net10.0-windows`
- CPU 架构：`win-x64`
- 开启 Nullable：`<Nullable>enable</Nullable>`
- 开启 Implicit Usings，但核心项目禁止依赖隐式全局状态。
- 最低操作系统：Windows 10 22H2；主要测试平台为 Windows 11。

### 原因

- .NET 10 是当前活动状态的 LTS，支持周期覆盖项目早期开发和 v1.0 演进。
- WPF 在 .NET 10 继续维护，并强化了 Fluent UI 相关能力。
- C# 与 Win32、DirectX、ONNX Runtime、OpenCV 的互操作成熟。

### 未采用方案

- **.NET 8**：生态稳定，但支持期明显短于 .NET 10，不适合作为 2026 年新项目的长期基线。
- **WinUI 3**：界面现代，但窗口、覆盖层、成熟控件和桌面自动化生态仍不如 WPF 稳妥。
- **Electron/Tauri**：会增加跨语言桥接、打包和原生截图输入能力的复杂度。
- **Python 主程序**：OCR 原型快，但桌面交付、依赖管理和长期维护不如 C# 统一。

### 复审条件

.NET 10 出现无法接受的原生依赖兼容问题，或目标用户大量停留在不受支持的 Windows 版本。

---

## ADR-002：桌面 UI 采用 WPF + MVVM

**状态：已接受**

### 决策

- UI：WPF。
- 模式：MVVM。
- MVVM 工具：`CommunityToolkit.Mvvm`。
- 现代化视觉层可以使用 WPF-UI 或自研轻量主题，但不得让业务代码依赖具体控件库。
- ViewModel 不直接调用 Win32、OCR、数据库或文件系统。
- View 的 code-behind 仅允许处理纯视图行为，例如窗口拖拽、焦点和动画。

### 规则

- ViewModel 使用 `ObservableObject`、`[ObservableProperty]`、`[RelayCommand]`。
- 异步命令必须支持 `CancellationToken` 或统一任务取消入口。
- 导航、弹窗、通知通过服务接口完成。
- 禁止在 XAML 中放置复杂业务判断。

### 原因

BetterGI 已验证 WPF、CommunityToolkit.Mvvm 和依赖注入适合大型视觉自动化桌面项目；本项目保留这条成熟路线，但采用更小的模块边界，避免早期目录过度膨胀。

---

## ADR-003：采用模块化单体，不采用微服务或单项目“大杂烩”

**状态：已接受**

### 决策

解决方案初始结构：

```text
BetterStarRail.sln
├─ src/
│  ├─ BetterStarRail.App/                 # WPF、View、ViewModel、组合根
│  ├─ BetterStarRail.Core/                # 领域模型、端口接口、通用结果类型
│  ├─ BetterStarRail.Automation/          # 状态机、任务编排、功能模块
│  ├─ BetterStarRail.Vision/              # 图像处理、模板匹配、OCR、模型推理
│  ├─ BetterStarRail.Platform.Windows/    # 截图、窗口、进程、键鼠、热键、权限
│  └─ BetterStarRail.Infrastructure/      # 配置、SQLite、日志、更新、文件系统
├─ tests/
│  ├─ BetterStarRail.Core.Tests/
│  ├─ BetterStarRail.Automation.Tests/
│  ├─ BetterStarRail.Vision.Tests/
│  └─ BetterStarRail.IntegrationTests/
├─ assets/
│  ├─ templates/
│  ├─ models/
│  └─ testdata/
├─ docs/
│  ├─ adr/
│  ├─ architecture/
│  └─ user-guide/
└─ tools/
```

### 依赖方向

```text
App ───────────────┐
                   ├──> Core
Automation ────────┤
Vision ────────────┤
Platform.Windows ──┤
Infrastructure ────┘

App 负责组装各实现；Core 不反向引用任何基础设施项目。
```

### 限制

- v0.1 不为每一个小功能创建独立程序集。
- 功能模块先作为 `Automation/Features/*` 下的文件夹组织。
- 只有当某模块具备独立发布、独立依赖或高风险隔离需求时，才拆为新项目。

### 原因

该结构比单项目更容易测试和替换底层实现，又比“每层十几个项目”更适合小团队。

---

## ADR-004：使用 .NET Generic Host 作为应用生命周期与组合根

**状态：已接受**

### 决策

WPF 启动时创建 `IHost`，统一提供：

- 依赖注入；
- 配置加载；
- 日志；
- 后台服务；
- 应用启动与优雅关闭；
- 全局异常处理；
- 任务取消和资源释放。

`App.xaml.cs` 只负责构建 Host、显示主窗口、停止 Host，不承载业务逻辑。

### 原因

桌面程序同样需要可测试的依赖关系、统一日志和生命周期管理。Generic Host 可避免大量静态单例和 Service Locator。

---

## ADR-005：自动化只采用外部视觉与系统输入

**状态：已接受，属于不可轻易变更的红线决策**

### 决策

系统只通过下列方式感知和操作游戏：

1. 获取游戏窗口、进程是否存在及窗口几何信息；
2. 捕获用户屏幕上可见的游戏画面；
3. 使用模板匹配、颜色、轮廓、OCR 或本地视觉模型识别 UI；
4. 使用 Windows 标准输入 API 模拟键鼠操作；
5. 操作后重新截图并验证结果。

### 明确禁止

- 读写游戏进程内存；
- DLL 注入、Hook 游戏内部函数；
- 修改客户端文件、资源包或配置以取得自动化优势；
- 构造、重放或篡改游戏网络请求；
- 内核驱动或虚拟 HID 驱动；
- 绕过、禁用或对抗反作弊；
- 隐藏进程、窗口、模块或行为；
- 任何以“更难被检测”为主要目的的实现。

### 原因

该路线技术风险、账号风险和维护风险仍然存在，但边界清晰，且不与游戏内部实现直接耦合。

---

## ADR-006：截图采用多后端抽象

**状态：已接受**

### 决策

定义统一接口：

```csharp
public interface IFrameSource : IAsyncDisposable
{
    ValueTask<FrameLease> CaptureAsync(
        CaptureRegion region,
        CancellationToken cancellationToken);
}
```

后端优先级：

1. **Windows.Graphics.Capture**：窗口化/无边框窗口默认后端；
2. **DXGI Desktop Duplication**：全屏或 WGC 不稳定时的回退后端；
3. **GDI BitBlt**：仅作为兼容性和诊断回退，不作为高频默认方案。

### 规则

- 截图实现不得泄露 Direct3D 对象到 Automation 层。
- Frame 使用池化内存或租约对象，避免每帧大对象分配。
- 仅截取任务所需 ROI，不持续处理整屏 60 FPS。
- 默认识别频率 2～10 FPS，由任务状态动态调整。
- 捕获失败时应切换后端或停止，不得无限空转。

### 未采用方案

直接绑定 BetterGI 的截图实现。原因是许可证、耦合和后续独立维护问题；即使合法复用，也必须包在本项目接口后。

---

## ADR-007：传统视觉采用 OpenCvSharp，优先模板与规则，模型只解决必要问题

**状态：已接受**

### 决策

- 使用 OpenCvSharp 进行裁剪、缩放、颜色转换、阈值、模板匹配、特征和轮廓处理。
- v0.1 优先采用可解释、易调试的传统视觉算法。
- 只有模板/规则在多个场景下明显不足时，才引入 ONNX 模型。
- 每个识别器返回置信度、命中区域和诊断信息，而不是只返回 `bool`。

建议统一结果：

```csharp
public sealed record DetectionResult(
    bool IsMatch,
    float Confidence,
    NormalizedRect Region,
    string Detector,
    IReadOnlyDictionary<string, object?> Diagnostics);
```

### 原因

游戏 UI 大量元素相对固定，模板和局部规则成本低、行为可解释。盲目使用大模型会增加包体、资源占用和误判排查难度。

---

## ADR-008：OCR 采用可替换接口，生产版本不依赖用户安装 Python

**状态：已接受**

### 决策

定义 `IOcrEngine`，Automation 层只接收文本块、置信度和坐标。

```csharp
public interface IOcrEngine
{
    ValueTask<OcrResult> RecognizeAsync(
        ImageRegion image,
        OcrRequest request,
        CancellationToken cancellationToken);
}
```

v0.1 技术方向：

- OCR 模型在本地运行；
- 优先 C# 直接调用 ONNX Runtime；
- CPU 为必选执行后端；
- GPU/DirectML 为可选加速，不作为正确性依赖；
- 模型、字典、预处理参数作为版本化资产；
- 不要求用户单独安装 Python、CUDA 或系统级 OCR 环境。

### 原因

参考 HSR 仓库使用 Python RapidOCR 桥接，适合原型验证，但会增加进程通信、运行环境和打包故障。正式产品应将 OCR 隔离并最终统一到可自包含分发的实现。

### 过渡方案

若 C# OCR 技术验证未按期达到准确率，可在开发版使用随包携带的独立 OCR Worker，但协议必须保持稳定，且不得依赖用户现有 Python 环境。

---

## ADR-009：自动化流程采用“感知—判断—动作—验证”状态机

**状态：已接受**

### 决策

每个功能实现为 `IAutomationTask`，内部由有限状态机或显式工作流步骤组成：

```text
Capture → Detect → Decide → Act → Verify
                      │          │
                      └─Recover←─┘
```

统一接口示意：

```csharp
public interface IAutomationTask
{
    string Id { get; }
    Task<TaskRunResult> RunAsync(
        TaskContext context,
        CancellationToken cancellationToken);
}
```

### 强制规则

- 禁止用连续坐标点击和固定 `Sleep` 拼出完整流程。
- 每次关键操作前确认当前页面状态。
- 每次关键操作后验证预期状态变化。
- 超时、重试次数和恢复路径必须显式定义。
- 所有循环必须响应取消。
- 状态无法确认时停止并保存故障现场，不猜测点击。

### 标准状态

`NotStarted / Preparing / Running / Paused / Recovering / Completed / Cancelled / Failed`

### 原因

游戏更新、网络波动、动画时间和设备性能会让固定延时脚本极易失效。状态机能把失败定位到具体状态，并支持恢复。

---

## ADR-010：输入模拟使用 Win32 SendInput，不使用驱动级方案

**状态：已接受**

### 决策

- 使用 Win32 `SendInput` 封装键盘、鼠标移动、点击、滚轮和组合键。
- 所有输入通过 `IInputController`，业务层不得直接 P/Invoke。
- 输入前检查目标窗口、前台状态、客户区位置和 DPI。
- 游戏以更高完整性级别运行时，提示用户以相同权限重新启动工具。
- 不通过驱动、虚拟设备或游戏内部接口发送输入。

### 安全措施

- 全局停止热键拥有最高优先级；
- 用户操作鼠标或键盘时，可配置自动暂停；
- 任务结束或异常退出时释放所有按键；
- 不实现以规避检测为目的的随机化；延时抖动仅用于等待 UI 稳定。

### 原因

SendInput 是 Windows 标准用户态输入方式，便于审计和维护。其受 UIPI 完整性级别限制，因此权限检测必须成为平台层能力。

---

## ADR-011：坐标统一使用归一化坐标与锚点，不使用散落的绝对像素

**状态：已接受**

### 决策

- 内部使用 `[0,1]` 归一化坐标和区域。
- 模板资源记录设计分辨率、ROI、缩放范围和阈值。
- 点击位置优先来自检测结果中心或相对锚点，而不是硬编码屏幕坐标。
- v0.1 正式支持 16:9：1280×720、1600×900、1920×1080、2560×1440。
- 非 16:9、HDR、显卡滤镜、非默认 UI 缩放在 v0.1 中提示不支持并阻止无人值守任务。

### 原因

先冻结有限测试矩阵，远比“声称支持全部分辨率但大量误点”可靠。归一化和锚点设计保留后续扩展能力。

---

## ADR-012：调度器与任务运行时统一管理暂停、取消、超时和互斥

**状态：已接受**

### 决策

- 同一游戏实例同一时间只允许一个高层自动化任务控制输入。
- 实时检测可以与任务并存，但必须通过只读帧总线共享截图，避免重复捕获。
- 使用 `CancellationToken` 贯穿任务、步骤、OCR、截图和等待。
- 暂停采用可感知的异步门，不通过阻塞线程实现。
- 调度任务记录：计划时间、开始时间、结束时间、结果、失败步骤、版本和资产版本。

### v0.1 调度范围

- 手动立即执行；
- 每日固定时间；
- 启动应用后执行；
- 前置任务成功后执行；
- 失败后有限次数重试。

复杂条件编排和可视化流程编辑器延后。

---

## ADR-013：设置使用 JSON，结构化运行数据使用 SQLite

**状态：已接受**

### 决策

- `settings.json`：用户可编辑设置、功能开关和路径。
- `profiles/*.json`：任务方案和队伍/副本配置。
- SQLite：任务运行、调度记录、迁移版本、资产索引和错误摘要。
- 模板、模型和截图作为文件保存，数据库只记录路径和元数据。
- 通过 Repository/Store 接口访问持久化，ViewModel 不直接使用 SQLite。

### 原因

纯 JSON 易于查看，但不适合查询大量历史记录；全部使用数据库又会降低可移植性。两者分工最稳妥。

---

## ADR-014：日志采用结构化日志，并保留视觉诊断证据

**状态：已接受**

### 决策

- 业务代码依赖 `Microsoft.Extensions.Logging`。
- 文件日志可使用 Serilog Provider 实现滚动和结构化输出。
- 每次任务拥有 `RunId`、`TaskId`、`StepId`。
- 失败时保存：失败前后截图、命中框、OCR 文本、阈值、当前状态和应用版本。
- 正常运行默认不持续保存全量截图。
- 日志按天滚动并设置总容量上限。

### 隐私规则

- 默认不上报日志和截图；
- 日志不得记录账号、密码、Cookie、Token；
- 对 Windows 用户名和完整用户目录进行脱敏；
- 用户导出诊断包前显示包含内容，并允许排除截图。

---

## ADR-015：错误处理采用分类、有限重试与安全停止

**状态：已接受**

### 错误分类

1. `Transient`：动画未结束、网络短暂延迟，可有限重试；
2. `Recoverable`：进入错误页面，可返回首页或重新定位；
3. `Configuration`：路径、分辨率或资源错误，停止并提示；
4. `UnsupportedState`：未知弹窗、游戏更新、识别不确定，立即停止；
5. `Fatal`：截图、输入或进程层不可用，终止本次运行。

### 规则

- 重试必须有上限和退避。
- 不允许 `catch (Exception) { continue; }`。
- 发生不确定状态时宁可停止，不盲点。
- 故障后生成可复现的诊断摘要。

---

## ADR-016：宏录制与回放采用语义事件格式，原始事件仅作底层数据

**状态：已接受**

### 决策

宏数据包含：

- 相对时间；
- 键鼠事件；
- 归一化坐标；
- 目标窗口信息；
- 可选的页面前置条件；
- 可选的操作后验证条件；
- 宏格式版本。

宏回放必须：

- 检查游戏窗口存在；
- 支持暂停、取消和紧急停止；
- 对分辨率和客户区做坐标转换；
- 禁止在非目标窗口继续发送输入。

### 延后

v0.1 不提供任意 C#、PowerShell、Python 或 JavaScript 脚本执行能力。

### 原因

任意脚本会引入远程代码执行、插件信任和兼容性问题。先把安全、可校验的宏格式做稳。

---

## ADR-017：启动器只负责启动、定位和状态检测，不托管账号凭据

**状态：已接受**

### 决策

`IGameLauncher` 支持：

- 用户配置官方启动器或游戏可执行文件路径；
- 启动进程；
- 等待游戏进程和主窗口；
- 检测客户端类型、窗口标题、客户区和权限级别；
- 超时后给出明确错误。

不实现：

- 保存或填写账号密码；
- 绕过启动器；
- 修改启动参数以禁用安全组件；
- 多开绕过或客户端限制绕过。

---

## ADR-018：功能模块按统一生命周期实现

**状态：已接受**

每个功能目录至少包含：

```text
Features/<FeatureName>/
├─ <FeatureName>Task.cs
├─ <FeatureName>Options.cs
├─ States/
├─ Detectors/
├─ Steps/
├─ Assets/
└─ README.md
```

### 首批模块

1. `GameLaunch`：启动与窗口检测；
2. `DailyTraining`：每日训练奖励；
3. `Assignments`：委托领取和重新派遣；
4. `MailRewards`：邮件奖励；
5. `ActivityRewards`：活动页可确认奖励；
6. `TrailblazePower`：进入目标副本、战斗、结算和循环；
7. `MacroReplay`：录制与回放；
8. `DailyRoutine`：组合前述任务。

模块之间不得通过 ViewModel 相互调用，只能通过任务运行时和明确的结果对象编排。

---

## ADR-019：测试以离线截图回放为核心，减少必须启动游戏的测试

**状态：已接受**

### 测试层级

- **单元测试**：状态机、文本匹配、坐标换算、重试和配置迁移；
- **Golden Image 测试**：固定截图、ROI、预期检测结果；
- **OCR 数据集测试**：文本、置信度、坐标和不同缩放；
- **流程回放测试**：按时间序列输入截图，验证状态转移与动作请求；
- **平台集成测试**：窗口枚举、截图、SendInput、权限检测；
- **人工冒烟测试**：真实游戏环境，只验证关键主路径。

### 合并门槛

- 核心状态机和坐标模块必须有单元测试；
- 新增检测器必须带至少一组正样本和负样本；
- 修复识别 Bug 时必须加入回归样本；
- CI 不依赖游戏安装即可完成主要测试。

---

## ADR-020：发布采用自包含 x64 文件夹包，不采用单文件压缩

**状态：已接受**

### 决策

- 发布：`dotnet publish -c Release -r win-x64 --self-contained true`。
- 产物为 ZIP/安装器形式的目录包。
- 不强制用户预装 .NET Runtime。
- v0.1 不启用 SingleFile 和 NativeAOT，以避免 OpenCV、ONNX、模型和原生 DLL 装载问题。
- 每个 Release 提供 SHA-256 校验值和第三方许可证清单。

### 更新策略

- v0.1 仅检查新版本并打开发布页，不静默替换程序。
- 自动更新必须等到包签名、哈希校验、回滚和断电恢复全部完成后再启用。

---

## ADR-021：默认本地优先、无遥测

**状态：已接受**

### 决策

- 默认不收集使用数据；
- 不上传截图、识别文本、游戏信息或运行记录；
- 联网功能仅用于用户主动执行的版本检查、资源更新或打开文档；
- 未来如引入遥测，必须单独 ADR、默认关闭、明确列出字段并允许查看和删除。

### 原因

辅助工具会接触屏幕画面和本地路径，隐私风险高。默认本地优先更符合用户信任和开源审计要求。

---

## ADR-022：开源许可证与参考项目使用规则

**状态：已接受，发布前必须复核**

### 决策

1. BetterGI 仓库标注为 GPL-3.0。若直接复制、修改或形成其衍生代码，本项目必须采用与其兼容的 GPL 发布方式，保留版权和许可证声明，并提供相应源代码。
2. `Better-HSR-Currency-Wars` 仓库当前未见 LICENSE 文件。没有明确许可证不代表可以复制、修改或再分发；本项目只允许观察公开行为、目录和技术思路，不复制其源码、资源或 UI。
3. 若希望项目使用 MIT/Apache-2.0，必须执行 clean-room：不复制 GPL 或无许可证代码，只基于公开需求和独立设计重新实现。
4. v0.1 若计划实质复用 BetterGI 代码，项目主许可证暂定 **GPL-3.0-only**；是否可使用 “or later” 必须以原项目明确授权为准，不能自行扩大。
5. 每个第三方依赖必须记录：名称、版本、用途、许可证、源地址、是否包含原生库、是否需要 NOTICE。

### 推荐落地

在仓库加入：

```text
LICENSE
NOTICE.md
THIRD-PARTY-NOTICES.md
docs/legal/dependency-license-audit.csv
```

### 原因

“参考开源项目”最容易在复制代码、资源和界面时产生许可证风险。许可证决策必须早于代码大规模迁移。

---

# 5. 核心运行链路

```text
用户/调度器
    │
    ▼
Automation Runtime
    │ 创建任务上下文、取消令牌、RunId
    ▼
Feature Task / State Machine
    │
    ├── IGameProcessService ──> 进程与窗口状态
    ├── IFrameSource ─────────> 游戏截图
    ├── IVisionService ───────> 模板/颜色/轮廓识别
    ├── IOcrEngine ───────────> OCR 文本与坐标
    ├── IInputController ─────> SendInput
    ├── ITaskStore ───────────> SQLite 运行状态
    └── ILogger ──────────────> 日志与故障证据

每次关键动作：
识别当前状态 → 验证置信度 → 执行动作 → 等待可取消条件 → 验证新状态
```

---

# 6. 关键接口清单

v0.1 在功能开发前应先冻结下列接口的最小版本：

```text
IFrameSource              截图
IGameWindowService        游戏窗口与客户区
IGameProcessService       进程状态
IGameLauncher             启动游戏
IInputController          键鼠输入
IHotkeyService            全局停止/暂停热键
IVisionMatcher            模板及规则识别
IOcrEngine                OCR
IAssetProvider            模板和模型资产
IAutomationTask           高层任务
IWorkflowStep             可复用步骤
IAutomationRuntime        任务生命周期
ITaskScheduler            调度
ITaskStore                运行记录
ISettingsStore            设置
IDiagnosticCapture        故障截图与诊断包
IUpdateService            更新检查
```

接口参数不得直接暴露 `Bitmap`、`Mat`、Direct3D Texture 或具体数据库连接，防止底层库扩散到全项目。

---

# 7. 性能预算

在 1920×1080、中画质 60 FPS 游戏运行环境下：

| 指标 | v0.1 目标 |
|---|---:|
| 空闲 CPU | < 1% |
| 常规视觉任务平均 CPU | < 12% |
| 峰值 CPU | < 30%，不持续超过 5 秒 |
| 工具内存 | < 500 MB，不含可选大模型 |
| 常规状态识别延迟 | < 300 ms |
| OCR 单次 ROI 延迟 | CPU < 800 ms |
| 停止热键响应 | < 200 ms |
| 截图/帧内存 | 池化，不持续增长 |

性能目标不是通过减少验证步骤换取；安全停止和状态确认优先于速度。

---

# 8. v0.1 不做的技术能力

为避免首版失控，明确延后：

- 微服务、远程控制服务器；
- 手机远程控制；
- 插件市场；
- 任意脚本执行；
- 云端 OCR 或云端视觉模型；
- 多账号凭据管理；
- 多游戏支持；
- 超宽屏和所有 UI 缩放组合；
- 自动学习用户操作；
- 自修改、自下载并执行未知脚本；
- 任何反检测或绕过安全机制的能力。

---

# 9. 进入编码前的技术验证门槛

以下 Spike 全部通过后，才开始大量编写具体业务流程：

| 编号 | 技术验证 | 通过标准 |
|---|---|---|
| SP-01 | 截图 | 连续捕获游戏窗口 30 分钟，无黑屏、泄漏和明显卡顿 |
| SP-02 | DPI/坐标 | 100%、125%、150% DPI 下点击误差不超过 3 px 或目标控件安全区 |
| SP-03 | 输入 | SendInput 可稳定操作同权限游戏窗口，异常时能检测并提示 |
| SP-04 | 模板识别 | 4 个支持分辨率下核心按钮正样本召回率 ≥ 99%，负样本误报率可接受 |
| SP-05 | OCR | 中文核心 UI 文本测试集字符准确率达到业务阈值，低置信度会停止而非误点 |
| SP-06 | 状态机 | 使用截图序列可完整回放一个领取任务，支持取消、超时和恢复 |
| SP-07 | 打包 | 干净 Windows 机器无需安装开发环境即可启动 |
| SP-08 | 许可证 | 所有依赖和复用代码来源可追溯，许可证无冲突 |

若 SP-04 或 SP-05 未通过，应先调整识别方案，而不是继续堆业务功能。

---

# 10. 风险与缓解

| 风险 | 影响 | 缓解措施 |
|---|---|---|
| 游戏更新改变 UI | 大量检测失效 | 资产版本化、Golden Image、置信度阈值、未知状态停止 |
| OCR 误识别导致误点 | 错误领取或进入错误页面 | ROI、白名单、上下文验证、操作后验证、危险动作二次确认 |
| 截图后端黑屏 | 无法识别 | 多后端、启动自检、故障截图、兼容模式 |
| DPI/分辨率误差 | 坐标偏移 | 客户区坐标、归一化、锚点、有限支持矩阵 |
| 管理员权限不一致 | SendInput 失败 | 启动时检测完整性级别并明确提示 |
| 原生 DLL 打包失败 | 用户无法启动 | 自包含目录包、CI 干净机验证、禁止过早 SingleFile |
| 长期挂机失控 | 重复点击或资源占用 | 超时、重试上限、全局停止、看门狗、未知状态停止 |
| 开源许可证冲突 | 无法发布或被要求下架 | 早期许可证审计、不复制无许可证代码、保留 NOTICE |
| 杀毒软件误报 | 用户信任受损 | 不混淆、不注入、不驱动、公开源码、可复现构建、代码签名规划 |

---

# 11. 决策优先级

发生冲突时按以下顺序取舍：

1. 用户设备和账号风险控制；
2. 明确停止和可恢复；
3. 不违反许可证和项目红线；
4. 识别正确性；
5. 可诊断性；
6. 可维护性；
7. 性能；
8. 开发速度；
9. UI 动画和视觉细节。

---

# 12. ADR 变更流程

任何人提出技术路线变更时，新 ADR 至少包含：

```text
标题
状态：Proposed / Accepted / Superseded / Rejected
日期
背景
决策
候选方案
优点
代价与风险
迁移方案
回滚方案
影响的项目和接口
验证方式
```

以下变更必须写 ADR，不允许直接合并：

- 更换 UI 框架或 .NET 主版本；
- 引入游戏内存访问、注入、驱动或网络协议操作；
- 更换主 OCR/视觉引擎；
- 引入脚本或插件执行；
- 改变主许可证；
- 引入云服务或遥测；
- 改变持久化方案；
- 自动更新具备下载和执行能力。

---

# 13. 参考依据（调研快照：2026-06-19）

1. Microsoft .NET Support Policy：.NET 10 为 LTS，支持至 2028-11-14。  
   https://dotnet.microsoft.com/en-us/platform/support/policy
2. Microsoft：What's new in WPF for .NET 10。  
   https://learn.microsoft.com/en-us/dotnet/desktop/wpf/whats-new/net100
3. Microsoft：Use the .NET Generic Host in a WPF app。  
   https://learn.microsoft.com/en-us/dotnet/desktop/wpf/app-development/how-to-use-host-builder
4. Microsoft：CommunityToolkit.Mvvm。  
   https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/
5. Microsoft：Windows.Graphics.Capture。  
   https://learn.microsoft.com/en-us/windows/apps/develop/media-authoring-processing/screen-capture
6. Microsoft：Desktop Duplication API。  
   https://learn.microsoft.com/en-us/windows/win32/direct3ddxgi/desktop-dup-api
7. Microsoft：SendInput。  
   https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-sendinput
8. ONNX Runtime C# API。  
   https://onnxruntime.ai/docs/get-started/with-csharp.html
9. OpenCvSharp。  
   https://github.com/shimat/opencvsharp
10. BetterGI：公开仓库、项目结构、MVVM 规范、视觉自动化路线和 GPL-3.0 标识。  
    https://github.com/babalae/better-genshin-impact
11. Better HSR-Currency-Wars：公开目录显示 WPF、Core/Services、OCR Bridge 等原型结构；仓库根目录当前未见 LICENSE。  
    https://github.com/439awsl-hue/Better-HSR-Currency-Wars

---

## 14. 最终批准结论

Better Star Rail v0.1 以 **.NET 10 + WPF + MVVM + 模块化单体 + 外部视觉状态机** 为正式技术基线。

开发必须优先完成截图、坐标、输入、OCR、状态机和打包六项技术验证；业务功能不得绕过抽象接口直接操作底层库。任何涉及内存、注入、驱动、反检测、账号凭据或无许可证代码复制的方案，均不属于本项目已批准架构。
