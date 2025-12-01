#!/bin/bash

# PolySms NuGet 包构建脚本
# 用途：清理缓存、重新编译并打包 NuGet 包

set -e  # 遇到错误立即退出

echo "======================================"
echo "📦 PolySms NuGet 包构建脚本"
echo "======================================"
echo ""

# 项目路径
PROJECT_PATH="PolySms/PolySms.csproj"
OUTPUT_DIR="PolySms/bin/Release"

# 步骤 1：清理构建输出
echo "🧹 步骤 1/6: 清理构建输出..."
dotnet clean -c Release

# 步骤 2：删除 bin 和 obj 目录
echo "🗑️  步骤 2/6: 删除缓存目录..."
rm -rf PolySms/bin PolySms/obj

# 步骤 3：恢复依赖
echo "📥 步骤 3/6: 恢复 NuGet 依赖..."
dotnet restore $PROJECT_PATH

# 步骤 4：编译
echo "🔨 步骤 4/6: 编译项目..."
dotnet build $PROJECT_PATH -c Release --no-restore

# 步骤 5：打包
echo "📦 步骤 5/6: 打包 NuGet..."
dotnet pack $PROJECT_PATH -c Release --no-build

# 步骤 6：显示打包结果
echo ""
echo "✅ 步骤 6/6: 打包完成！"
echo ""
echo "======================================"
echo "📋 打包结果信息"
echo "======================================"

# 查找生成的 .nupkg 文件
NUPKG_FILE=$(find $OUTPUT_DIR -name "*.nupkg" -type f | head -1)

if [ -n "$NUPKG_FILE" ]; then
    echo "📦 NuGet 包路径: $NUPKG_FILE"
    echo "📊 包大小: $(du -h "$NUPKG_FILE" | cut -f1)"
    echo ""

    # 显示包内 DLL 信息
    echo "🔍 包内 DLL 文件信息:"
    unzip -l "$NUPKG_FILE" | grep "\.dll" | awk '{print "   " $1 " bytes - " $2 " " $3 " - " $4}'
    echo ""

    # 显示本地 DLL 信息（用于对比）
    DLL_FILE="$OUTPUT_DIR/net9.0/PolySms.dll"
    if [ -f "$DLL_FILE" ]; then
        echo "📂 本地 DLL 文件信息:"
        ls -lh "$DLL_FILE" | awk '{print "   " $5 " - " $6 " " $7 " " $8 " - " $9}'
        echo ""

        # 计算文件哈希（用于验证）
        LOCAL_HASH=$(sha256sum "$DLL_FILE" | cut -d' ' -f1)
        echo "🔐 本地 DLL SHA256: ${LOCAL_HASH:0:16}..."
    fi

    echo ""
    echo "======================================"
    echo "✨ 构建成功完成！"
    echo "======================================"
else
    echo "❌ 错误: 未找到生成的 NuGet 包"
    exit 1
fi
