@echo off
cd /d "D:\DDRIVE-Code\Code-C#\HappyTetris"
echo === Listing all branches ===
git branch -a
echo.
echo === Current status ===
git status
echo.
echo === Checking out main branch ===
git checkout main
echo.
echo === Merging copilot/vscode-mlxzy6i3-3hvd ===
git merge copilot/vscode-mlxzy6i3-3hvd
echo.
echo === Checking status for conflicts ===
git status
echo.
echo === Listing conflicted files ===
git diff --name-only --diff-filter=U
