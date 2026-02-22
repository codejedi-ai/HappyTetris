# 快乐俄罗斯方块 - Happy Tetris
## Windows 安装程序制作指南

### 方法一：使用 WiX Toolset (推荐)

#### 1. 安装 WiX Toolset v4
- 访问 https://wixtoolset.org/
- 下载并安装 WiX Toolset v4
- 或者使用 .NET 工具安装:
  ```
  dotnet tool install --global wix
  ```

#### 2. 构建应用程序
```bash
# 运行构建脚本
build.bat
# 或者
.\build.ps1
```

#### 3. 创建 MSI 安装程序
```bash
# 使用 wix 命令创建 MSI
wix build -arch x64 -out HappyTetris.msi .\HappyTetris\Installer\Product.wxs
```

### 方法二：使用 Inno Setup (简单)

#### 1. 下载并安装 Inno Setup
- 访问 https://jrsoftware.org/isdl.php
- 下载并安装 Inno Setup Compiler

#### 2. 使用提供的脚本
- 打开 `Installer\Setup.iss` 文件
- 在 Inno Setup 中打开并编译

### 方法三：直接发布 (最简单)

#### 运行游戏
构建后直接运行:
```
.\publish\HappyTetris.exe
```

#### 创建快捷方式
手动创建快捷方式指向 `HappyTetris.exe`

---

## 系统要求

- Windows 10/11 (64 位)
- .NET 8.0 Runtime (如果发布为自包含则不需要)
- 至少 100MB 可用磁盘空间

## 游戏控制

- ← → : 左右移动
- ↑ : 旋转方块
- ↓ : 加速下落
- 空格 : 直接落下
- P : 暂停游戏
- G : 新手引导
- H 或 F1 : 帮助说明

## 得分规则

- 消除 1 行：100 分
- 消除 2 行：300 分
- 消除 3 行：500 分
- 消除 4 行：800 分

等级每消除 10 行提升一级，游戏速度会加快！

---

## 文件结构

```
HappyTetris/
├── HappyTetris.csproj      # 项目文件
├── App.xaml                # 应用程序定义
├── MainWindow.xaml         # 主窗口 UI
├── Models/                 # 数据模型
│   ├── Piece.cs           # 方块模型
│   ├── Board.cs           # 游戏板模型
│   └── Player.cs          # 玩家模型
├── Engine/                 # 游戏引擎
│   └── GameEngine.cs      # 核心游戏逻辑
├── Audio/                  # 音频控制
│   └── AudioController.cs # 音效控制器
├── Installer/              # 安装程序配置
│   ├── Product.wxs        # WiX 配置
│   └── Setup.iss          # Inno Setup 配置
├── build.bat              # Windows 构建脚本
├── build.ps1              # PowerShell 构建脚本
└── README.md              # 本文件
```

---

## 许可证

本游戏仅供学习和个人娱乐使用。
