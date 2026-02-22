@echo off
setlocal EnableExtensions
chcp 65001 >nul

set "scriptDir=%~dp0"
pushd "%scriptDir%"

set "projectPath=%scriptDir%HappyTetris.csproj"
set "publishPath=%scriptDir%publish"
set "installerPath=%scriptDir%Installer\publish"

echo ======================================
echo   快乐俄罗斯方块 - Build Script
echo   Happy Tetris - Build Script
echo ======================================
echo.

dotnet --version >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo .NET SDK not found. Please install .NET 8 SDK and try again.
    popd
    exit /b 1
)

echo [1/5] Cleaning previous builds...
if exist "%publishPath%" rmdir /s /q "%publishPath%"
if exist "%scriptDir%bin" rmdir /s /q "%scriptDir%bin"
if exist "%scriptDir%obj" rmdir /s /q "%scriptDir%obj"
echo Done!
echo.

echo [2/5] Restoring NuGet packages...
dotnet restore "%projectPath%"
if %ERRORLEVEL% neq 0 (
    echo Failed to restore packages!
    popd
    exit /b 1
)
echo Done!
echo.

echo [3/5] Building application...
dotnet build "%projectPath%" --configuration Release
if %ERRORLEVEL% neq 0 (
    echo Failed to build!
    popd
    exit /b 1
)
echo Done!
echo.

echo [4/5] Publishing application...
dotnet publish "%projectPath%" --configuration Release --runtime win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true --output "%publishPath%"
if %ERRORLEVEL% neq 0 (
    echo Failed to publish!
    popd
    exit /b 1
)
echo Done!
echo.

echo [5/5] Preparing installer files...
if not exist "%installerPath%" mkdir "%installerPath%"
xcopy /E /Y /Q "%publishPath%\*" "%installerPath%\" >nul

echo.
echo ======================================
echo   Build Complete!
echo ======================================
echo.
echo Published files location: %publishPath%
echo Installer files location: %installerPath%
echo.
echo To run the game:
echo   "%publishPath%\HappyTetris.exe"
echo.
echo To create MSI installer (requires WiX Toolset):
echo   1. Install WiX Toolset v4 from https://wixtoolset.org/
echo   2. Run: wix build -arch x64 -out HappyTetris.msi .\Installer\Product.wxs
echo.
popd
if "%1"=="" pause
