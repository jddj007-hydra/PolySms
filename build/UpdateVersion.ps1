#!/usr/bin/env pwsh
<#
.SYNOPSIS
    PolySms版本管理脚本

.DESCRIPTION
    用于管理PolySms项目的混合版本控制系统：
    - 语义版本（NuGet包）：x.y.z
    - 日期版本（程序集）：yyyy.MM.dd.build

.PARAMETER IncrementMajor
    递增主版本号（破坏性变更）

.PARAMETER IncrementMinor
    递增次版本号（新功能）

.PARAMETER IncrementPatch
    递增补丁版本号（问题修复）

.PARAMETER SetVersion
    设置特定版本号（如：1.2.3）

.PARAMETER PreRelease
    设置预发布标签（如：alpha, beta, rc1）

.PARAMETER IncrementBuild
    仅递增构建号

.PARAMETER ShowVersion
    显示当前版本信息

.EXAMPLE
    .\build\UpdateVersion.ps1 -IncrementPatch
    递增补丁版本：1.2.0 -> 1.2.1

.EXAMPLE
    .\build\UpdateVersion.ps1 -SetVersion "2.0.0"
    设置版本为2.0.0

.EXAMPLE
    .\build\UpdateVersion.ps1 -PreRelease "beta"
    设置预发布版本：1.2.0-beta
#>

param(
    [switch]$IncrementMajor,
    [switch]$IncrementMinor,
    [switch]$IncrementPatch,
    [string]$SetVersion,
    [string]$PreRelease,
    [switch]$IncrementBuild,
    [switch]$ShowVersion
)

# 脚本路径
$ScriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootPath = Split-Path -Parent $ScriptPath
$VersionJsonPath = Join-Path $RootPath "version.json"
$DirectoryBuildPropsPath = Join-Path $RootPath "Directory.Build.props"

# 颜色函数
function Write-ColorText($Text, $Color = "White") {
    Write-Host $Text -ForegroundColor $Color
}

function Write-Success($Text) { Write-ColorText $Text "Green" }
function Write-Warning($Text) { Write-ColorText $Text "Yellow" }
function Write-Error($Text) { Write-ColorText $Text "Red" }
function Write-Info($Text) { Write-ColorText $Text "Cyan" }

# 读取版本配置
function Read-VersionConfig {
    if (-not (Test-Path $VersionJsonPath)) {
        Write-Error "版本配置文件不存在：$VersionJsonPath"
        exit 1
    }

    try {
        $json = Get-Content $VersionJsonPath -Raw | ConvertFrom-Json
        return $json
    }
    catch {
        Write-Error "无法解析版本配置文件：$($_.Exception.Message)"
        exit 1
    }
}

# 保存版本配置
function Save-VersionConfig($Config) {
    try {
        $Config.metadata.lastUpdated = (Get-Date).ToString("yyyy-MM-dd")
        $Config.metadata.commitHash = ""
        $Config.metadata.branch = ""

        # 尝试获取Git信息
        try {
            $Config.metadata.commitHash = (git rev-parse --short HEAD 2>$null)
            $Config.metadata.branch = (git branch --show-current 2>$null)
        }
        catch { }

        $json = $Config | ConvertTo-Json -Depth 10
        Set-Content -Path $VersionJsonPath -Value $json -Encoding UTF8
        Write-Success "✅ 版本配置已更新：$VersionJsonPath"
    }
    catch {
        Write-Error "无法保存版本配置：$($_.Exception.Message)"
        exit 1
    }
}

# 更新Directory.Build.props
function Update-DirectoryBuildProps($Config) {
    if (-not (Test-Path $DirectoryBuildPropsPath)) {
        Write-Error "Directory.Build.props文件不存在：$DirectoryBuildPropsPath"
        exit 1
    }

    try {
        $content = Get-Content $DirectoryBuildPropsPath -Raw

        # 更新版本号
        $content = $content -replace '<MajorVersion>\d+</MajorVersion>', "<MajorVersion>$($Config.semantic.major)</MajorVersion>"
        $content = $content -replace '<MinorVersion>\d+</MinorVersion>', "<MinorVersion>$($Config.semantic.minor)</MinorVersion>"
        $content = $content -replace '<PatchVersion>\d+</PatchVersion>', "<PatchVersion>$($Config.semantic.patch)</PatchVersion>"
        $content = $content -replace '<PreReleaseLabel>[^<]*</PreReleaseLabel>', "<PreReleaseLabel>$($Config.semantic.prerelease)</PreReleaseLabel>"
        $content = $content -replace '<BuildNumber>\d+</BuildNumber>', "<BuildNumber>$($Config.build.number)</BuildNumber>"

        Set-Content -Path $DirectoryBuildPropsPath -Value $content -Encoding UTF8
        Write-Success "✅ Directory.Build.props已更新"
    }
    catch {
        Write-Error "无法更新Directory.Build.props：$($_.Exception.Message)"
        exit 1
    }
}

# 显示版本信息
function Show-VersionInfo($Config) {
    $semanticVersion = "$($Config.semantic.major).$($Config.semantic.minor).$($Config.semantic.patch)"
    if ($Config.semantic.prerelease) {
        $semanticVersion += "-$($Config.semantic.prerelease)"
    }

    $today = Get-Date
    $assemblyVersion = "$($today.Year).$($today.Month.ToString('00')).$($today.Day.ToString('00')).$($Config.build.number)"
    $informationalVersion = "$semanticVersion+build.$($today.Year)$($today.Month.ToString('00'))$($today.Day.ToString('00')).$($Config.build.number)"

    Write-Info "📦 PolySms 版本信息"
    Write-Info "├── NuGet版本（语义版本）: $semanticVersion"
    Write-Info "├── 程序集版本（日期版本）: $assemblyVersion"
    Write-Info "├── 完整版本信息: $informationalVersion"
    Write-Info "├── 构建号: $($Config.build.number)"
    Write-Info "├── 最后更新: $($Config.metadata.lastUpdated)"
    Write-Info "└── Git分支: $($Config.metadata.branch)"
}

# 主逻辑
function Main {
    Write-Info "🚀 PolySms版本管理工具"
    Write-Info "─────────────────────────"

    $config = Read-VersionConfig

    if ($ShowVersion) {
        Show-VersionInfo $config
        return
    }

    $updated = $false

    # 处理版本更新
    if ($IncrementMajor) {
        $config.semantic.major++
        $config.semantic.minor = 0
        $config.semantic.patch = 0
        $updated = $true
        Write-Success "🔼 主版本递增：$($config.semantic.major).0.0"
    }
    elseif ($IncrementMinor) {
        $config.semantic.minor++
        $config.semantic.patch = 0
        $updated = $true
        Write-Success "🔼 次版本递增：$($config.semantic.major).$($config.semantic.minor).0"
    }
    elseif ($IncrementPatch) {
        $config.semantic.patch++
        $updated = $true
        Write-Success "🔼 补丁版本递增：$($config.semantic.major).$($config.semantic.minor).$($config.semantic.patch)"
    }
    elseif ($SetVersion) {
        if ($SetVersion -match '^(\d+)\.(\d+)\.(\d+)$') {
            $config.semantic.major = [int]$Matches[1]
            $config.semantic.minor = [int]$Matches[2]
            $config.semantic.patch = [int]$Matches[3]
            $updated = $true
            Write-Success "🎯 版本设置为：$SetVersion"
        }
        else {
            Write-Error "无效的版本格式：$SetVersion（应为：x.y.z）"
            exit 1
        }
    }

    # 处理预发布标签
    if ($PSBoundParameters.ContainsKey('PreRelease')) {
        $config.semantic.prerelease = $PreRelease
        $updated = $true
        if ($PreRelease) {
            Write-Success "🏷️  预发布标签：$PreRelease"
        }
        else {
            Write-Success "🏷️  已清除预发布标签"
        }
    }

    # 处理构建号
    if ($IncrementBuild -or $updated) {
        $config.build.number++
        $updated = $true
        Write-Success "🔨 构建号递增：$($config.build.number)"
    }

    if (-not $updated) {
        Write-Warning "⚠️  未指定任何操作"
        Write-Info "使用 -ShowVersion 查看当前版本，或使用其他参数更新版本"
        Write-Info "运行 Get-Help .\build\UpdateVersion.ps1 获取帮助"
        return
    }

    # 保存配置并更新文件
    Save-VersionConfig $config
    Update-DirectoryBuildProps $config

    Write-Info ""
    Show-VersionInfo $config

    Write-Info ""
    Write-Success "✨ 版本更新完成！"
    Write-Info "💡 建议：运行 dotnet build 验证配置正确性"
}

# 错误处理
trap {
    Write-Error "脚本执行出错：$($_.Exception.Message)"
    Write-Error "位置：$($_.InvocationInfo.PositionMessage)"
    exit 1
}

# 执行主函数
Main