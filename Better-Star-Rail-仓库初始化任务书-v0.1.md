# Better Star Rail 仓库初始化任务书

> 历史任务书提示：本文用于 V0 仓库初始化审计。第 9 节的 `0.1.0-dev` 已被当前 `0.2.0-dev` 开发版本取代；现行阶段/版本术语以 `docs/ROADMAP.md` 和 `docs/project-status/` 为准。技术与许可证基线仍以 ADR-0004 为准。

> 文档版本：v0.1  
> 项目阶段：仓库初始化 / 工程基线建立  
> 适用对象：Codex、Claude Code、GitHub Copilot Agent 或项目开发者  
> 项目性质：Windows 桌面端、C#、公开开源、非官方第三方工具

---

## 1. 任务背景

Better Star Rail 是一个面向《崩坏：星穹铁道》的 Windows 桌面辅助工具项目。

本项目采用**路线 A**：

- 创建全新独立仓库；
- 不 Fork BetterGI；
- 不复制 BetterGI、Better-HSR-Currency-Wars 或其他项目的源码、素材、配置、文案与专有命名；
- BetterGI 仅用于研究产品思路、功能拆分和常见工程问题；
- Better-HSR-Currency-Wars 仅用于研究业务流程；
- 所有正式代码从零编写；
- 仓库使用 Apache License 2.0；
- 项目计划公开开源。

本任务只负责建立可长期维护的仓库和工程基线，不负责实现完整游戏自动化功能。

---

## 2. 总目标

创建一个能够正常克隆、还原、编译、测试和启动的 Better Star Rail 初始仓库，使后续功能开发具备统一的目录、依赖、日志、配置、测试、文档、CI 和协作规范。

完成后必须达到：

1. 在全新 Windows 环境中可通过标准命令还原和编译；
2. WPF 主程序可以启动并显示基础窗口；
3. 依赖注入、日志和配置系统完成最小接入；
4. 核心模块之间的职责和依赖方向明确；
5. 单元测试和 GitHub Actions 可以运行；
6. 开源许可证、第三方许可、风险提示和贡献规范齐全；
7. 仓库中不存在复制的第三方代码、游戏素材或可疑二进制文件；
8. 本阶段不包含真实的挂机、战斗、OCR 点击或反检测逻辑。

---

## 3. 已确定技术栈

| 类别 | 选型 |
|---|---|
| 开发语言 | C# 14 |
| 运行时 | .NET 10 LTS |
| 桌面框架 | WPF |
| UI 架构 | MVVM |
| MVVM 工具 | CommunityToolkit.Mvvm |
| 应用托管 | Microsoft.Extensions.Hosting / Generic Host |
| 配置 | Microsoft.Extensions.Configuration |
| 日志 | Serilog |
| 图像处理 | OpenCvSharp |
| 屏幕采集 | Windows.Graphics.Capture，保留 BitBlt 回退接口 |
| 输入模拟 | Win32 SendInput，暂只定义接口 |
| 测试 | xUnit |
| 架构 | 模块化单体 |
| 目标平台 | Windows 10/11 x64 |
| 开源许可证 | Apache-2.0 |

### 暂不采用

- 跨平台 UI；
- 微服务；
- 独立插件市场；
- 复杂动态插件加载器；
- AI 决策模块；
- 内核驱动；
- DLL 注入；
- 游戏进程内存读写；
- 网络封包修改；
- 反作弊绕过或隐藏；
- 自动更新服务；
- 云端账号系统；
- 遥测和用户行为采集。

---

## 4. 执行原则

执行者必须遵守：

1. 先检查当前目录和 Git 状态，再创建或修改文件；
2. 只在当前项目仓库内操作；
3. 不删除用户已有文件；
4. 不复制参考项目中的实现；
5. 不使用游戏官方图标、角色立绘、截图或其他受版权保护素材作为仓库资源；
6. 不提交密钥、令牌、账号、Cookie、个人路径或本地配置；
7. 不执行 `git push`、发布 Release 或上传包，除非用户另行明确授权；
8. 每完成一个阶段都运行验证命令；
9. 出错时读取完整错误信息，不通过跳过测试或关闭警告掩盖问题；
10. 不为追求“架构完整”建立大量空项目、空接口或无用途抽象；
11. 初始化阶段只建立最小可扩展骨架；
12. 所有文本文件使用 UTF-8；
13. 代码、文件名和提交信息使用英文，面向用户的文档可使用中文；
14. 公共 API 必须有 XML 文档注释；内部简单代码不强制写无意义注释。

---

## 5. 目标仓库结构

```text
BetterStarRail/
├─ .github/
│  ├─ ISSUE_TEMPLATE/
│  │  ├─ bug_report.yml
│  │  ├─ feature_request.yml
│  │  └─ config.yml
│  ├─ workflows/
│  │  ├─ build.yml
│  │  └─ codeql.yml
│  ├─ dependabot.yml
│  └─ pull_request_template.md
│
├─ assets/
│  ├─ branding/
│  └─ README.md
│
├─ docs/
│  ├─ architecture/
│  │  ├─ README.md
│  │  └─ adr/
│  │     └─ 0001-initial-architecture.md
│  ├─ development/
│  │  ├─ getting-started.md
│  │  ├─ coding-style.md
│  │  └─ git-workflow.md
│  ├─ legal/
│  │  ├─ disclaimer.md
│  │  └─ third-party-policy.md
│  └─ product/
│     └─ scope-v0.1.md
│
├─ src/
│  ├─ BetterStarRail.App/
│  ├─ BetterStarRail.Core/
│  ├─ BetterStarRail.Automation/
│  ├─ BetterStarRail.Vision/
│  └─ BetterStarRail.Infrastructure.Windows/
│
├─ tests/
│  ├─ BetterStarRail.Core.Tests/
│  ├─ BetterStarRail.Automation.Tests/
│  └─ BetterStarRail.Vision.Tests/
│
├─ Directory.Build.props
├─ Directory.Packages.props
├─ BetterStarRail.sln
├─ global.json
├─ .editorconfig
├─ .gitattributes
├─ .gitignore
├─ CHANGELOG.md
├─ CODE_OF_CONDUCT.md
├─ CONTRIBUTING.md
├─ LICENSE
├─ NOTICE
├─ README.md
├─ SECURITY.md
└─ THIRD_PARTY_NOTICES.md
```

### 目录约束

- `src/BetterStarRail.App`：WPF 程序入口、窗口、视图、ViewModel、Host 启动和模块装配；
- `src/BetterStarRail.Core`：领域模型、公共结果类型、状态定义、基础接口，不引用 WPF、OpenCV 或 Win32；
- `src/BetterStarRail.Automation`：任务流程、状态机、步骤编排和取消控制，不直接调用具体 Win32 API；
- `src/BetterStarRail.Vision`：图像匹配、区域定义、识别结果模型和图像算法接口；
- `src/BetterStarRail.Infrastructure.Windows`：窗口查找、进程启动、屏幕采集、输入模拟、文件系统等 Windows 实现；
- `tests`：只存放自动化测试；
- `assets`：只允许自制或明确可再分发的项目素材；
- `docs`：架构、开发、产品和法律文档。

---

## 6. 项目依赖方向

必须保持以下依赖关系：

```text
BetterStarRail.App
 ├─> BetterStarRail.Core
 ├─> BetterStarRail.Automation
 ├─> BetterStarRail.Vision
 └─> BetterStarRail.Infrastructure.Windows

BetterStarRail.Automation
 └─> BetterStarRail.Core

BetterStarRail.Vision
 └─> BetterStarRail.Core

BetterStarRail.Infrastructure.Windows
 ├─> BetterStarRail.Core
 ├─> BetterStarRail.Automation
 └─> BetterStarRail.Vision
```

禁止：

- `Core` 引用 WPF、Win32、OpenCV 或具体基础设施；
- `Automation` 直接使用 `SendInput`；
- ViewModel 直接调用 Win32 API；
- `Vision` 直接控制鼠标键盘；
- 项目之间形成循环引用；
- 把所有实现堆进 WPF 主项目。

---

## 7. 初始化工作项

## 7.1 P0：Git 与基础文件

执行：

- 初始化 Git 仓库；
- 默认分支设为 `main`；
- 创建适用于 Visual Studio、Rider、VS Code 和 .NET 的 `.gitignore`；
- 创建 `.gitattributes`，统一文本换行规则；
- 创建 `.editorconfig`；
- 创建 `LICENSE`，内容为 Apache License 2.0；
- 创建 `NOTICE`；
- 创建 `THIRD_PARTY_NOTICES.md`；
- 创建 `README.md`；
- 创建 `CHANGELOG.md`，采用 Keep a Changelog 风格；
- 创建 `SECURITY.md`；
- 创建 `CONTRIBUTING.md`；
- 创建 `CODE_OF_CONDUCT.md`。

### 验收

- `git status` 中不存在 IDE 缓存、`bin/`、`obj/`、用户密钥和本地配置；
- `LICENSE` 是完整 Apache-2.0 正文；
- README 明确项目为非官方第三方项目；
- README 不出现“绝对安全”“不会封号”等保证。

---

## 7.2 P1：解决方案与项目骨架

创建 `BetterStarRail.sln`，并加入以下项目：

```text
src/BetterStarRail.App
src/BetterStarRail.Core
src/BetterStarRail.Automation
src/BetterStarRail.Vision
src/BetterStarRail.Infrastructure.Windows
tests/BetterStarRail.Core.Tests
tests/BetterStarRail.Automation.Tests
tests/BetterStarRail.Vision.Tests
```

### 项目类型

- `BetterStarRail.App`：WPF Application；
- 其他 `src` 项目：Class Library；
- `tests` 项目：xUnit Test Project。

### 统一编译设置

在 `Directory.Build.props` 中设置：

- `TargetFramework`：`net10.0-windows`；
- `PlatformTarget`：`x64`；
- `Nullable`：启用；
- `ImplicitUsings`：启用；
- `TreatWarningsAsErrors`：CI 中启用；
- `Deterministic`：启用；
- `EnableNETAnalyzers`：启用；
- 生成 XML 文档；
- Release 构建启用优化；
- WPF 项目启用 Windows Desktop SDK。

不允许在每个 `.csproj` 中重复相同配置。

### 集中依赖管理

创建 `Directory.Packages.props`，集中维护 NuGet 版本。初始仅引入实际需要的包：

- CommunityToolkit.Mvvm；
- Microsoft.Extensions.Hosting；
- Microsoft.Extensions.Options.ConfigurationExtensions；
- Serilog；
- Serilog.Extensions.Hosting；
- Serilog.Sinks.File；
- OpenCvSharp4；
- OpenCvSharp4.runtime.win；
- xunit；
- xunit.runner.visualstudio；
- Microsoft.NET.Test.Sdk；
- coverlet.collector。

不得添加暂时未使用的大型依赖。

---

## 7.3 P2：应用最小启动链路

实现最小可启动 WPF 程序。

### 必须完成

1. 使用 Generic Host 创建应用容器；
2. 注册配置、日志、主窗口和主 ViewModel；
3. 应用启动时初始化 Host；
4. 应用退出时正确停止并释放 Host；
5. 主窗口显示：
   - 项目名称；
   - 当前版本；
   - “工程初始化完成”状态；
   - 不包含任何可执行自动化按钮；
6. 使用 CommunityToolkit.Mvvm 构建 ViewModel；
7. 日志默认写入：
   - `%LocalAppData%\BetterStarRail\logs\`
8. 用户配置默认写入：
   - `%LocalAppData%\BetterStarRail\config\`
9. 不向仓库目录写运行日志或用户配置；
10. 全局异常处理至少记录：
    - UI 线程未处理异常；
    - AppDomain 未处理异常；
    - 未观察到的 Task 异常。

### 配置要求

初始 `appsettings.json` 只允许包含非敏感默认值，例如：

```json
{
  "Application": {
    "Name": "Better Star Rail",
    "Environment": "Development"
  },
  "Logging": {
    "MinimumLevel": "Information"
  }
}
```

不得包含：

- 游戏账号；
- 用户名；
- 本机绝对路径；
- Access Token；
- Cookie；
- API Key；
- 游戏安装目录。

---

## 7.4 P3：核心抽象与占位实现

只创建后续开发确实需要的最小抽象，禁止实现真实自动化行为。

建议建立：

```text
Core/
├─ Abstractions/
│  ├─ IClock.cs
│  ├─ IAppPathProvider.cs
│  └─ IApplicationVersionProvider.cs
├─ Models/
│  └─ OperationResult.cs
└─ Diagnostics/
   └─ AppStatus.cs
```

```text
Automation/
├─ Abstractions/
│  ├─ IAutomationWorkflow.cs
│  └─ IAutomationStep.cs
├─ Models/
│  ├─ AutomationContext.cs
│  └─ AutomationStepResult.cs
└─ Services/
   └─ AutomationCoordinator.cs
```

```text
Vision/
├─ Abstractions/
│  ├─ IScreenCaptureService.cs
│  └─ IImageMatcher.cs
└─ Models/
   ├─ CaptureRegion.cs
   └─ MatchResult.cs
```

```text
Infrastructure.Windows/
├─ Capture/
├─ Input/
├─ Processes/
├─ Windows/
└─ Storage/
```

### 初始化阶段限制

- `IScreenCaptureService` 可以只有接口或安全的空实现；
- `IImageMatcher` 可以只有接口或确定性测试实现；
- 输入模拟模块只能定义接口和能力边界；
- 不得在初始化阶段调用游戏进程；
- 不得读取游戏窗口；
- 不得移动鼠标或发送按键；
- 不得实现无人值守挂机；
- 不得实现 OCR 自动点击；
- 不得要求管理员权限。

---

## 7.5 P4：测试基线

每个测试项目至少创建一个有意义的测试。

最低测试范围：

1. `OperationResult` 成功和失败状态正确；
2. `AutomationCoordinator` 支持取消令牌；
3. 自动化步骤失败后返回明确结果，不吞异常；
4. `CaptureRegion` 拒绝负数宽高；
5. DI 容器可以解析主 ViewModel；
6. 配置路径不指向仓库目录；
7. 空工作流不会触发输入或采集实现。

### 测试要求

- 测试不能依赖已安装的《崩坏：星穹铁道》；
- 测试不能操作真实鼠标键盘；
- 测试不能访问网络；
- 测试不能依赖用户机器上的固定路径；
- 测试运行结束后不留下临时文件；
- `dotnet test` 必须可重复通过。

---

## 7.6 P5：GitHub 协作配置

创建以下 GitHub 配置：

### Issue 模板

- Bug Report；
- Feature Request；
- 禁用空白 Issue，或明确要求先阅读贡献指南。

### Pull Request 模板

至少包含：

- 变更目的；
- 变更类型；
- 测试方式；
- 是否涉及用户数据；
- 是否涉及游戏输入或屏幕采集；
- 是否引入第三方代码或素材；
- 风险和回滚方式；
- 自查清单。

### Dependabot

- 每周检查 NuGet；
- 每周检查 GitHub Actions；
- 限制同时打开的更新 PR 数量；
- 不自动合并重大版本更新。

### 分支规范

- `main`：始终保持可编译；
- `feature/<name>`：功能开发；
- `fix/<name>`：缺陷修复；
- `docs/<name>`：文档变更；
- `refactor/<name>`：重构；
- `chore/<name>`：工程维护。

提交信息采用 Conventional Commits，例如：

```text
feat: add application host bootstrap
fix: handle host shutdown correctly
docs: add repository contribution guide
test: add automation cancellation tests
chore: configure central package management
```

---

## 7.7 P6：CI 基线

创建 `.github/workflows/build.yml`。

### 触发条件

- 推送到 `main`；
- 对 `main` 创建或更新 Pull Request；
- 手动触发。

### CI 步骤

1. Checkout；
2. 安装 `global.json` 指定的 .NET SDK；
3. `dotnet restore`；
4. `dotnet format --verify-no-changes`；
5. `dotnet build --configuration Release --no-restore`；
6. `dotnet test --configuration Release --no-build --collect:"XPlat Code Coverage"`；
7. 上传测试结果和覆盖率文件；
8. 不在初始化阶段自动发布程序。

### CodeQL

创建 `.github/workflows/codeql.yml`：

- 分析 C#；
- 对 `main` push、PR 和定期任务运行；
- 不影响本地开发；
- 失败时不得静默忽略。

---

## 7.8 P7：文档基线

### README 必须包含

1. 项目简介；
2. 当前开发状态；
3. 支持平台；
4. 当前尚未提供可用自动化功能；
5. 从源码构建方式；
6. 仓库结构；
7. 风险提示；
8. 隐私说明；
9. 贡献方式；
10. 许可证；
11. 与 HoYoverse / 米哈游不存在隶属、授权或合作关系的声明；
12. 不保证使用该工具不会违反游戏服务条款；
13. 不保证账号安全；
14. 禁止将项目用于破坏游戏公平性、绕过安全机制或其他违法用途。

### `docs/legal/disclaimer.md`

必须明确：

- 项目是非官方社区项目；
- 用户应自行阅读并遵守游戏协议和当地法律；
- 使用风险由用户自行判断；
- 项目不承诺账号安全；
- 项目不提供反作弊绕过；
- 项目不收集账号密码；
- 项目不会要求用户关闭或规避安全软件；
- 出现风险时应优先停止使用。

### `docs/legal/third-party-policy.md`

必须明确：

- 引入代码前检查许可证；
- 不接受来源不明代码；
- 不复制其他仓库实现后简单改名；
- 引入第三方代码必须保留版权和许可证；
- 引入素材必须有明确授权；
- 游戏官方素材默认不进入仓库；
- Pull Request 必须声明第三方来源。

### ADR 0001

记录：

- 为什么选择 C# + WPF；
- 为什么选择 .NET 10；
- 为什么采用模块化单体；
- 为什么不 Fork 参考项目；
- 为什么不做注入、内存读写和反作弊绕过；
- 为什么 v0.1 不做插件系统和 AI。

---

## 8. 安全与合规红线

本仓库初始化以及后续默认开发不得包含：

1. DLL 注入；
2. 代码注入；
3. 远程线程创建；
4. 游戏进程内存扫描、读取或写入；
5. 驱动级输入模拟；
6. Hook 游戏内部函数；
7. 修改网络封包；
8. 中间人代理游戏通信；
9. 反作弊检测、规避、对抗或隐藏；
10. 自动关闭反作弊、杀毒软件或系统安全功能；
11. 账号密码、Cookie、Token 的采集和存储；
12. 未经用户同意上传日志、截图或配置；
13. 来源不明的二进制依赖；
14. 混淆恶意行为；
15. 冒充官方程序；
16. 使用官方商标作为应用图标或造成官方授权误解；
17. “零封号”“绝对安全”等宣传；
18. 默认管理员权限；
19. 静默自启动；
20. 静默修改系统设置。

涉及屏幕采集和输入模拟的功能，后续必须满足：

- 用户主动开启；
- UI 中显示当前状态；
- 支持立即停止；
- 支持取消令牌；
- 发生异常时停止输入；
- 不在后台隐藏运行；
- 日志不得记录敏感画面内容；
- 提供明显风险说明。

---

## 9. 版本和发布基线

初始化阶段版本设为：

```text
0.1.0-dev
```

规则：

- 使用 Semantic Versioning；
- 初始化完成后不立即创建正式 Release；
- 首个可供测试的构建版本再打 `v0.1.0-alpha.1`；
- `CHANGELOG.md` 的未发布内容写入 `Unreleased`；
- 构建产物不提交进 Git；
- 不提交 `bin/`、`obj/`、日志、用户配置或本地数据库。

---

## 10. 完成验收命令

执行者必须从仓库根目录运行：

```powershell
dotnet --info
dotnet restore
dotnet format --verify-no-changes
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build
git status --short
```

然后手动验证：

1. 启动 WPF 应用；
2. 主窗口正常显示；
3. 关闭窗口后进程正确退出；
4. `%LocalAppData%\BetterStarRail\logs\` 生成日志；
5. 仓库根目录未生成运行日志；
6. 不安装游戏也可以完成编译和测试；
7. 不连接网络也可以启动基础程序；
8. 主程序未产生鼠标键盘操作；
9. 没有请求管理员权限；
10. 没有未说明的第三方二进制文件。

---

## 11. Definition of Done

只有同时满足以下条件，仓库初始化任务才算完成：

- [ ] Git 仓库和 `main` 分支已建立；
- [ ] Apache-2.0、NOTICE 和第三方许可文件齐全；
- [ ] 解决方案和所有目标项目已创建；
- [ ] 项目依赖方向符合约束；
- [ ] WPF 基础程序可以启动；
- [ ] Generic Host、MVVM、日志和配置已接入；
- [ ] 用户数据写入 LocalAppData；
- [ ] 至少存在基础单元测试；
- [ ] Release 构建无错误；
- [ ] 所有测试通过；
- [ ] 格式检查通过；
- [ ] GitHub Actions 已配置；
- [ ] Issue、PR、Dependabot 和 CodeQL 已配置；
- [ ] README、贡献指南、安全政策和 ADR 已完成；
- [ ] 没有复制参考项目代码或素材；
- [ ] 没有注入、内存读写、反作弊绕过等实现；
- [ ] 没有真实游戏自动化行为；
- [ ] `git status` 中无应被忽略的构建垃圾；
- [ ] 执行结果报告已生成。

---

## 12. 执行结果报告格式

执行完成后，必须按以下格式汇报：

```markdown
# 仓库初始化执行报告

## 1. 完成状态
- 总体状态：成功 / 部分成功 / 失败
- 当前版本：
- 当前分支：
- 最后提交：

## 2. 已创建内容
- 解决方案：
- 项目：
- 文档：
- GitHub 配置：
- 测试：

## 3. 关键设计
- 项目依赖关系：
- 配置目录：
- 日志目录：
- 异常处理：
- CI：

## 4. 验证结果
- dotnet restore：
- dotnet format：
- dotnet build：
- dotnet test：
- WPF 启动：
- Git 状态：

## 5. 未完成内容
- 项目：
- 原因：
- 建议处理方式：

## 6. 风险与注意事项
- 风险：
- 后续开发约束：

## 7. 文件变更清单
- 新增：
- 修改：
- 删除：

## 8. 下一步建议
仅列出最优先的 3 项，不得直接开始实现。
```

---

## 13. 可直接交给 Agent 的执行提示词

```text
你现在负责初始化 Better Star Rail 的全新 Git 仓库。

严格按照仓库根目录中的《仓库初始化任务书》执行。开始前先检查当前目录、已有文件、Git 状态和已安装的 .NET SDK，然后再操作。

项目已定案：
- C# 14
- .NET 10 LTS
- WPF
- MVVM
- CommunityToolkit.Mvvm
- Generic Host
- Serilog
- OpenCvSharp
- xUnit
- Windows 10/11 x64
- 模块化单体
- Apache-2.0

强制约束：
1. 创建全新仓库，不 Fork 任何参考项目；
2. 不复制 BetterGI、Better-HSR-Currency-Wars 或其他项目的源码、素材和文案；
3. 本阶段只建立仓库与工程基线，不实现真实游戏自动化；
4. 不实现 DLL 注入、内存读写、Hook、驱动、封包修改或反作弊绕过；
5. 不操作真实鼠标键盘；
6. 不读取或启动游戏；
7. 不要求管理员权限；
8. 不执行 git push，不创建 Release；
9. 不删除任何已有用户文件；
10. 所有改动完成后运行 restore、format、Release build 和 test；
11. 发现错误必须修复根因，禁止通过关闭警告、跳过测试或删除失败测试来通过；
12. 最后按任务书中的“仓库初始化执行报告格式”汇报。

请直接执行，不要只给方案。完成每个阶段后进行验证，最终列出全部新增/修改文件、命令结果、遗留问题和下一步建议。
```
