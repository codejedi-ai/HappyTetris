# Happy Tetris - Build and Installer Script
# PowerShell Script for Windows

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "  快乐俄罗斯方块 - Build Script" -ForegroundColor Cyan
Write-Host "  Happy Tetris - Build Script" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

$projectPath = ".\HappyTetris"
$publishPath = ".\publish"
$installerPath = ".\installer"

# Step 1: Clean previous builds
Write-Host "[1/5] Cleaning previous builds..." -ForegroundColor Yellow
if (Test-Path $publishPath) {
    Remove-Item -Recurse -Force $publishPath
}
if (Test-Path ".\HappyTetris\bin") {
    Remove-Item -Recurse -Force ".\HappyTetris\bin"
}
if (Test-Path ".\HappyTetris\obj") {
    Remove-Item -Recurse -Force ".\HappyTetris\obj"
}
Write-Host "Done!" -ForegroundColor Green

# Step 2: Restore NuGet packages
Write-Host "[2/5] Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore $projectPath
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to restore packages!" -ForegroundColor Red
    exit 1
}
Write-Host "Done!" -ForegroundColor Green

# Step 3: Build the application
Write-Host "[3/5] Building application..." -ForegroundColor Yellow
dotnet build $projectPath --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to build!" -ForegroundColor Red
    exit 1
}
Write-Host "Done!" -ForegroundColor Green

# Step 4: Publish the application
Write-Host "[4/5] Publishing application..." -ForegroundColor Yellow
dotnet publish $projectPath `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --publish-single-file true `
    --publish-ready-to-run true `
    --output $publishPath
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to publish!" -ForegroundColor Red
    exit 1
}
Write-Host "Done!" -ForegroundColor Green

# Step 5: Create installer directory
Write-Host "[5/5] Preparing installer files..." -ForegroundColor Yellow
if (!(Test-Path $installerPath)) {
    New-Item -ItemType Directory -Path $installerPath | Out-Null
}

# Copy published files to installer directory
Copy-Item -Path "$publishPath\*" -Destination $installerPath -Recurse -Force

Write-Host ""
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "  Build Complete!" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Published files location: $publishPath" -ForegroundColor Cyan
Write-Host "Installer files location: $installerPath" -ForegroundColor Cyan
Write-Host ""
Write-Host "To run the game:" -ForegroundColor Yellow
Write-Host "  .\$publishPath\HappyTetris.exe" -ForegroundColor White
Write-Host ""
Write-Host "To create MSI installer (requires WiX Toolset):" -ForegroundColor Yellow
Write-Host "  1. Install WiX Toolset v4 from https://wixtoolset.org/" -ForegroundColor White
Write-Host "  2. Run: wix build -arch x64 -out HappyTetris.msi .\HappyTetris\Installer\Product.wxs" -ForegroundColor White
Write-Host ""
