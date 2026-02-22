@echo off
echo ======================================
echo   快乐俄罗斯方块 - Build Script
echo   Happy Tetris - Build Script
echo ======================================
echo.

set projectPath=.\HappyTetris
set publishPath=.\publish
set installerPath=.\installer

echo [1/5] Cleaning previous builds...
if exist %publishPath% rmdir /s /q %publishPath%
if exist .\HappyTetris\bin rmdir /s /q .\HappyTetris\bin
if exist .\HappyTetris\obj rmdir /s /q .\HappyTetris\obj
echo Done!
echo.

echo [2/5] Restoring NuGet packages...
dotnet restore %projectPath%
if %ERRORLEVEL% neq 0 (
    echo Failed to restore packages!
    exit /b 1
)
echo Done!
echo.

echo [3/5] Building application...
dotnet build %projectPath% --configuration Release
if %ERRORLEVEL% neq 0 (
    echo Failed to build!
    exit /b 1
)
echo Done!
echo.

echo [4/5] Publishing application...
dotnet publish %projectPath% ^
    --configuration Release ^
    --runtime win-x64 ^
    --self-contained true ^
    --publish-single-file true ^
    --publish-ready-to-run true ^
    --output %publishPath%
if %ERRORLEVEL% neq 0 (
    echo Failed to publish!
    exit /b 1
)
echo Done!
echo.

echo [5/5] Preparing installer files...
if not exist %installerPath% mkdir %installerPath%
xcopy /E /Y /Q %publishPath%\* %installerPath%\

echo.
echo ======================================
echo   Build Complete!
echo ======================================
echo.
echo Published files location: %publishPath%
echo Installer files location: %installerPath%
echo.
echo To run the game:
echo   %publishPath%\HappyTetris.exe
echo.
echo To create MSI installer ^(requires WiX Toolset^):
echo   1. Install WiX Toolset v4 from https://wixtoolset.org/
echo   2. Run: wix build -arch x64 -out HappyTetris.msi .\HappyTetris\Installer\Product.wxs
echo.
pause
