# Better Star Rail 开发环境验证脚本
# 只读取环境；不会安装软件、修改配置或删除文件。
# 用法：
#   powershell -ExecutionPolicy Bypass -File .\verify-better-star-rail-env.ps1
#   powershell -ExecutionPolicy Bypass -File .\verify-better-star-rail-env.ps1 -ProjectPath "D:\Projects\BetterStarRail" -RunBuild

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$ProjectPath = (Get-Location).Path,

    [Parameter(Mandatory = $false)]
    [switch]$RunBuild
)

$ErrorActionPreference = "Continue"
$results = New-Object System.Collections.Generic.List[object]

function Add-CheckResult {
    param(
        [string]$Name,
        [ValidateSet("必需", "推荐", "可选")]
        [string]$Level,
        [bool]$Passed,
        [string]$Details,
        [string]$Fix = ""
    )

    $results.Add([PSCustomObject]@{
        项目 = $Name
        级别 = $Level
        状态 = if ($Passed) { "通过" } else { "未通过" }
        详情 = $Details
        修复建议 = $Fix
    })
}

function Test-CommandExists {
    param([string]$Command)
    return $null -ne (Get-Command $Command -ErrorAction SilentlyContinue)
}

function Get-FirstLine {
    param([string[]]$Text)
    if ($null -eq $Text -or $Text.Count -eq 0) { return "" }
    return ($Text | Select-Object -First 1).ToString().Trim()
}

Write-Host ""
Write-Host "=== Better Star Rail 开发环境检查 ===" -ForegroundColor Cyan
Write-Host "项目路径：$ProjectPath"
Write-Host "仅检查，不安装、不修改。"
Write-Host ""

# 1. Windows / CPU 架构
$isWindows = $env:OS -eq "Windows_NT"
Add-CheckResult `
    -Name "Windows 系统" `
    -Level "必需" `
    -Passed $isWindows `
    -Details $(if ($isWindows) { "Windows 已识别" } else { "当前不是 Windows" }) `
    -Fix "本项目采用 WPF 与 Windows API，应在 Windows 10/11 x64 上开发。"

$is64 = [Environment]::Is64BitOperatingSystem
Add-CheckResult `
    -Name "64 位操作系统" `
    -Level "必需" `
    -Passed $is64 `
    -Details "Is64BitOperatingSystem=$is64" `
    -Fix "请使用 Windows x64。"

try {
    $os = Get-CimInstance Win32_OperatingSystem -ErrorAction Stop
    Add-CheckResult `
        -Name "Windows 版本信息" `
        -Level "推荐" `
        -Passed $true `
        -Details "$($os.Caption)；版本 $($os.Version)；Build $($os.BuildNumber)"
} catch {
    Add-CheckResult -Name "Windows 版本信息" -Level "推荐" -Passed $false -Details $_.Exception.Message
}

# 2. winget
$hasWinget = Test-CommandExists "winget"
$wingetVersion = if ($hasWinget) { Get-FirstLine (& winget --version 2>&1) } else { "未找到" }
Add-CheckResult `
    -Name "Windows Package Manager (winget)" `
    -Level "推荐" `
    -Passed $hasWinget `
    -Details $wingetVersion `
    -Fix "更新 Windows 的“应用安装程序”，或安装 Windows Package Manager。"

# 3. .NET SDK / Desktop Runtime
$hasDotnet = Test-CommandExists "dotnet"
Add-CheckResult `
    -Name ".NET CLI" `
    -Level "必需" `
    -Passed $hasDotnet `
    -Details $(if ($hasDotnet) { Get-FirstLine (& dotnet --version 2>&1) } else { "dotnet 不在 PATH" }) `
    -Fix "安装 .NET 10 SDK x64，并重启终端。"

if ($hasDotnet) {
    $sdks = @(& dotnet --list-sdks 2>&1)
    $hasNet10Sdk = $sdks | Where-Object { $_ -match '^\s*10\.' }
    Add-CheckResult `
        -Name ".NET 10 SDK" `
        -Level "必需" `
        -Passed ([bool]$hasNet10Sdk) `
        -Details $(if ($hasNet10Sdk) { ($hasNet10Sdk -join "；") } else { "已安装 SDK：$($sdks -join '；')" }) `
        -Fix "安装 .NET 10 SDK x64。"

    $runtimes = @(& dotnet --list-runtimes 2>&1)
    $desktop10 = $runtimes | Where-Object { $_ -match '^Microsoft\.WindowsDesktop\.App\s+10\.' }
    Add-CheckResult `
        -Name ".NET 10 Windows Desktop Runtime" `
        -Level "必需" `
        -Passed ([bool]$desktop10) `
        -Details $(if ($desktop10) { ($desktop10 -join "；") } else { "未发现 Microsoft.WindowsDesktop.App 10.x" }) `
        -Fix "重新安装 .NET 10 SDK 或 Windows Desktop Runtime。"
}

# 4. Visual Studio / MSBuild workload
$vswhereCandidates = @(
    "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe",
    "$env:ProgramFiles\Microsoft Visual Studio\Installer\vswhere.exe"
)
$vswhere = $vswhereCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1

if ($vswhere) {
    $vsPath = (& $vswhere -latest -products * -requires Microsoft.VisualStudio.Workload.ManagedDesktop -property installationPath 2>$null | Select-Object -First 1)
    $hasManagedDesktop = -not [string]::IsNullOrWhiteSpace($vsPath)
    Add-CheckResult `
        -Name "Visual Studio .NET 桌面开发工作负载" `
        -Level "推荐" `
        -Passed $hasManagedDesktop `
        -Details $(if ($hasManagedDesktop) { $vsPath } else { "未检测到 Microsoft.VisualStudio.Workload.ManagedDesktop" }) `
        -Fix "打开 Visual Studio Installer，勾选“.NET 桌面开发”。"

    if ($hasManagedDesktop) {
        $msbuildPath = Join-Path $vsPath "MSBuild\Current\Bin\MSBuild.exe"
        Add-CheckResult `
            -Name "MSBuild" `
            -Level "推荐" `
            -Passed (Test-Path $msbuildPath) `
            -Details $msbuildPath `
            -Fix "修复或重新安装 Visual Studio / Build Tools。"
    }
} else {
    Add-CheckResult `
        -Name "Visual Studio / vswhere" `
        -Level "推荐" `
        -Passed $false `
        -Details "未发现 vswhere.exe" `
        -Fix "使用 Visual Studio 时安装“.NET 桌面开发”工作负载；仅使用 Rider/Codex CLI 可不装完整 Visual Studio。"
}

# 5. Windows SDK
$windowsSdkLib = "${env:ProgramFiles(x86)}\Windows Kits\10\Lib"
$sdkVersions = @()
if (Test-Path $windowsSdkLib) {
    $sdkVersions = Get-ChildItem $windowsSdkLib -Directory -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -match '^\d+\.\d+\.\d+\.\d+$' } |
        ForEach-Object {
            try { [version]$_.Name } catch { $null }
        } |
        Where-Object { $null -ne $_ } |
        Sort-Object -Descending
}
$latestSdk = $sdkVersions | Select-Object -First 1
$minSdk = [version]"10.0.22621.0"
$windowsSdkOk = $null -ne $latestSdk -and $latestSdk -ge $minSdk
Add-CheckResult `
    -Name "Windows SDK >= 10.0.22621.0" `
    -Level "必需" `
    -Passed $windowsSdkOk `
    -Details $(if ($latestSdk) { "最高版本：$latestSdk" } else { "未检测到 Windows 10/11 SDK" }) `
    -Fix "在 Visual Studio Installer 的“单个组件”中安装较新的 Windows 11 SDK。"

# 6. Git / LFS / GitHub CLI
$hasGit = Test-CommandExists "git"
Add-CheckResult `
    -Name "Git" `
    -Level "必需" `
    -Passed $hasGit `
    -Details $(if ($hasGit) { Get-FirstLine (& git --version 2>&1) } else { "git 不在 PATH" }) `
    -Fix "安装 Git for Windows，并重启终端。"

$hasGitLfs = $false
$gitLfsDetails = "未找到"
if ($hasGit) {
    $lfsOutput = @(& git lfs version 2>&1)
    $hasGitLfs = $LASTEXITCODE -eq 0
    if ($hasGitLfs) { $gitLfsDetails = Get-FirstLine $lfsOutput }
}
Add-CheckResult `
    -Name "Git LFS" `
    -Level "推荐" `
    -Passed $hasGitLfs `
    -Details $gitLfsDetails `
    -Fix "安装 Git LFS 后执行：git lfs install"

$hasGh = Test-CommandExists "gh"
Add-CheckResult `
    -Name "GitHub CLI" `
    -Level "推荐" `
    -Passed $hasGh `
    -Details $(if ($hasGh) { Get-FirstLine (& gh --version 2>&1) } else { "gh 不在 PATH" }) `
    -Fix "安装 GitHub CLI；用于 Codex 创建仓库、Issue、PR 和 Release。"

if ($hasGh) {
    $ghAuthOutput = @(& gh auth status 2>&1)
    $ghAuthOk = $LASTEXITCODE -eq 0
    Add-CheckResult `
        -Name "GitHub CLI 登录状态" `
        -Level "推荐" `
        -Passed $ghAuthOk `
        -Details (($ghAuthOutput | Select-Object -First 4) -join "；") `
        -Fix "执行：gh auth login"
}

# 7. Codex
$hasCodex = Test-CommandExists "codex"
Add-CheckResult `
    -Name "Codex CLI" `
    -Level "必需" `
    -Passed $hasCodex `
    -Details $(if ($hasCodex) { Get-FirstLine (& codex --version 2>&1) } else { "codex 不在 PATH" }) `
    -Fix "确认 Codex CLI 已安装，并重启 Windows Terminal。"

if ($hasCodex) {
    $mcpHelp = @(& codex mcp --help 2>&1)
    Add-CheckResult `
        -Name "Codex MCP 子命令" `
        -Level "推荐" `
        -Passed ($LASTEXITCODE -eq 0) `
        -Details (Get-FirstLine $mcpHelp) `
        -Fix "升级 Codex CLI。"

    $pluginHelp = @(& codex plugin --help 2>&1)
    Add-CheckResult `
        -Name "Codex Plugin 子命令" `
        -Level "可选" `
        -Passed ($LASTEXITCODE -eq 0) `
        -Details (Get-FirstLine $pluginHelp) `
        -Fix "升级 Codex CLI；也可在 Codex 中输入 /plugins 检查。"
}

# 8. Node.js：只为基于 npx 的 MCP 使用
$hasNode = Test-CommandExists "node"
$hasNpx = Test-CommandExists "npx"
Add-CheckResult `
    -Name "Node.js / npx" `
    -Level "可选" `
    -Passed ($hasNode -and $hasNpx) `
    -Details $(if ($hasNode -and $hasNpx) { "node $(& node --version 2>&1)；npx 可用" } else { "仅在使用 Context7 等 npx MCP 时需要" }) `
    -Fix "仅在需要基于 npx 的 MCP 时安装 Node.js LTS。"

# 9. Python：只供原型或迁移参考
$pythonCommand = if (Test-CommandExists "python") { "python" } elseif (Test-CommandExists "py") { "py" } else { $null }
Add-CheckResult `
    -Name "Python" `
    -Level "可选" `
    -Passed ($null -ne $pythonCommand) `
    -Details $(if ($pythonCommand) { Get-FirstLine (& $pythonCommand --version 2>&1) } else { "C# 主项目不要求；仅供旧 OCR 原型或数据脚本使用" }) `
    -Fix "主项目无需为了 Python 参考仓库强制安装。"

# 10. 项目结构
$projectExists = Test-Path $ProjectPath
Add-CheckResult `
    -Name "项目目录" `
    -Level "必需" `
    -Passed $projectExists `
    -Details $ProjectPath `
    -Fix "传入正确路径：-ProjectPath `"D:\Projects\BetterStarRail`""

$buildTarget = $null
if ($projectExists) {
    $solution = Get-ChildItem $ProjectPath -File -ErrorAction SilentlyContinue |
        Where-Object { $_.Extension -in @(".sln", ".slnx") } |
        Select-Object -First 1
    $project = Get-ChildItem $ProjectPath -Filter *.csproj -File -Recurse -ErrorAction SilentlyContinue |
        Select-Object -First 1

    if ($solution) { $buildTarget = $solution.FullName }
    elseif ($project) { $buildTarget = $project.FullName }

    Add-CheckResult `
        -Name "解决方案或 C# 项目文件" `
        -Level "必需" `
        -Passed ($null -ne $buildTarget) `
        -Details $(if ($buildTarget) { $buildTarget } else { "未发现 .sln、.slnx 或 .csproj" }) `
        -Fix "先初始化 WPF 解决方案与项目。"

    $recommendedFiles = @(
        @{ Name = "AGENTS.md"; Path = (Join-Path $ProjectPath "AGENTS.md"); Fix = "写入 Codex 的项目规则、禁区和验证命令。" },
        @{ Name = ".editorconfig"; Path = (Join-Path $ProjectPath ".editorconfig"); Fix = "统一 C#、XAML 与文本格式。" },
        @{ Name = "global.json"; Path = (Join-Path $ProjectPath "global.json"); Fix = "固定 .NET 10 SDK 主版本，减少不同机器构建差异。" },
        @{ Name = "Directory.Build.props"; Path = (Join-Path $ProjectPath "Directory.Build.props"); Fix = "集中管理 Nullable、WarningsAsErrors、平台与分析器规则。" },
        @{ Name = ".gitignore"; Path = (Join-Path $ProjectPath ".gitignore"); Fix = "忽略 bin、obj、.vs、日志、截图缓存和本地模型缓存。" },
        @{ Name = "README.md"; Path = (Join-Path $ProjectPath "README.md"); Fix = "记录安装、构建、风险和免责声明。" },
        @{ Name = "LICENSE"; Path = (Join-Path $ProjectPath "LICENSE"); Fix = "公开开源前必须明确许可证。" }
    )

    foreach ($item in $recommendedFiles) {
        Add-CheckResult `
            -Name "项目文件：$($item.Name)" `
            -Level "推荐" `
            -Passed (Test-Path $item.Path) `
            -Details $item.Path `
            -Fix $item.Fix
    }

    $skillsRoot = Join-Path $ProjectPath ".agents\skills"
    $skillFiles = @()
    if (Test-Path $skillsRoot) {
        $skillFiles = Get-ChildItem $skillsRoot -Filter SKILL.md -File -Recurse -ErrorAction SilentlyContinue
    }
    Add-CheckResult `
        -Name "仓库级 Codex Skills" `
        -Level "推荐" `
        -Passed ($skillFiles.Count -gt 0) `
        -Details $(if ($skillFiles.Count -gt 0) { "$($skillFiles.Count) 个：$((($skillFiles | ForEach-Object { $_.Directory.Name }) -join '、'))" } else { "未发现 .agents\skills\*\SKILL.md" }) `
        -Fix "在 .agents\skills 下建立项目专用 Skills。"

    $projectCodexConfig = Join-Path $ProjectPath ".codex\config.toml"
    $userCodexConfig = Join-Path $HOME ".codex\config.toml"
    Add-CheckResult `
        -Name "Codex 配置文件" `
        -Level "可选" `
        -Passed ((Test-Path $projectCodexConfig) -or (Test-Path $userCodexConfig)) `
        -Details "项目：$projectCodexConfig；用户：$userCodexConfig" `
        -Fix "需要 MCP 或项目级权限配置时再创建，不必为了好看空建。"
}

# 11. 真正构建验证
if ($RunBuild) {
    if (-not $hasDotnet) {
        Add-CheckResult -Name "dotnet restore" -Level "必需" -Passed $false -Details "dotnet 不可用"
        Add-CheckResult -Name "dotnet build" -Level "必需" -Passed $false -Details "dotnet 不可用"
    } elseif (-not $buildTarget) {
        Add-CheckResult -Name "dotnet restore" -Level "必需" -Passed $false -Details "未找到构建目标"
        Add-CheckResult -Name "dotnet build" -Level "必需" -Passed $false -Details "未找到构建目标"
    } else {
        Write-Host ""
        Write-Host "正在验证 NuGet 还原：$buildTarget" -ForegroundColor Yellow
        & dotnet restore $buildTarget
        $restoreOk = $LASTEXITCODE -eq 0
        Add-CheckResult `
            -Name "dotnet restore" `
            -Level "必需" `
            -Passed $restoreOk `
            -Details $(if ($restoreOk) { "NuGet 依赖还原成功" } else { "NuGet 依赖还原失败，退出码 $LASTEXITCODE" }) `
            -Fix "检查网络、NuGet.Config、包源和项目目标框架。"

        if ($restoreOk) {
            Write-Host ""
            Write-Host "正在执行 Debug 构建……" -ForegroundColor Yellow
            & dotnet build $buildTarget -c Debug --no-restore
            $buildOk = $LASTEXITCODE -eq 0
            Add-CheckResult `
                -Name "dotnet build -c Debug" `
                -Level "必需" `
                -Passed $buildOk `
                -Details $(if ($buildOk) { "项目编译成功" } else { "项目编译失败，退出码 $LASTEXITCODE" }) `
                -Fix "从第一条 error 开始修复，不要先处理后续连锁错误。"
        }
    }
}

# 输出汇总
Write-Host ""
Write-Host "=== 检查结果 ===" -ForegroundColor Cyan
$results | Format-Table -AutoSize -Wrap

$requiredFailures = @($results | Where-Object { $_.级别 -eq "必需" -and $_.状态 -ne "通过" })
$recommendedFailures = @($results | Where-Object { $_.级别 -eq "推荐" -and $_.状态 -ne "通过" })

Write-Host ""
Write-Host "必需项失败：$($requiredFailures.Count)" -ForegroundColor $(if ($requiredFailures.Count -eq 0) { "Green" } else { "Red" })
Write-Host "推荐项缺失：$($recommendedFailures.Count)" -ForegroundColor $(if ($recommendedFailures.Count -eq 0) { "Green" } else { "Yellow" })

if ($requiredFailures.Count -eq 0) {
    Write-Host "结论：核心开发环境通过。" -ForegroundColor Green
    exit 0
} else {
    Write-Host "结论：核心开发环境尚未通过，请先修复必需项。" -ForegroundColor Red
    exit 1
}
