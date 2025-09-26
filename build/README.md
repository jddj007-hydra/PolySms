# PolySms 版本管理系统

## 概述

PolySms 采用混合版本控制模式：
- **NuGet包版本**：遵循语义化版本规范 (SemVer)
- **程序集版本**：使用日期格式便于追踪构建时间

## 版本格式

| 版本类型 | 格式 | 示例 | 用途 |
|----------|------|------|------|
| **PackageVersion** | `x.y.z[-预发布]` | `1.2.0` | NuGet包发布 |
| **AssemblyVersion** | `yyyy.MM.dd.build` | `2024.12.26.0` | 程序集标识 |
| **InformationalVersion** | `x.y.z+build.yyyyMMdd.build.hash` | `1.2.0+build.20241226.0.abc1234` | 完整构建信息 |

## 使用方法

### 1. 手动更新版本
```bash
# 递增补丁版本（修复Bug）
pwsh ./build/UpdateVersion.ps1 -IncrementPatch

# 递增次版本（新功能）
pwsh ./build/UpdateVersion.ps1 -IncrementMinor

# 递增主版本（破坏性变更）
pwsh ./build/UpdateVersion.ps1 -IncrementMajor

# 设置特定版本
pwsh ./build/UpdateVersion.ps1 -SetVersion "2.0.0"

# 设置预发布版本
pwsh ./build/UpdateVersion.ps1 -PreRelease "beta"
```

### 2. 查看当前版本
```bash
pwsh ./build/UpdateVersion.ps1 -ShowVersion
```

### 3. CI/CD 集成
```yaml
- name: 更新版本
  run: |
    pwsh ./build/UpdateVersion.ps1 -IncrementPatch
    git add Directory.Build.props version.json
```

## 文件说明

- **`Directory.Build.props`** - MSBuild全局属性配置
- **`version.json`** - 版本配置和变更日志
- **`build/UpdateVersion.ps1`** - 版本管理脚本

## 优势

✅ **NuGet兼容** - 完全符合NuGet生态标准
✅ **时间追踪** - 程序集版本包含构建日期
✅ **自动化友好** - 支持CI/CD流水线集成
✅ **语义清晰** - 通过语义版本判断兼容性
✅ **构建信息** - 包含Git提交哈希和分支信息

## 最佳实践

1. **补丁版本**: 问题修复、性能优化
2. **次版本**: 新功能、向后兼容的改进
3. **主版本**: 破坏性API变更
4. **预发布**: 测试版本使用 `-alpha`、`-beta`、`-rc1` 等标签