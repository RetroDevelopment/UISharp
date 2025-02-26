# Open UI
This is a new cross-platform project written in C#.
The project has just started so expect a lot of updates to the readme file and to the framework in the future!
The active branch will be "mvp", which will be further merged into main.

The main goal is
1. Allow using OpenGL and svg vector graphics (efficient, high quality, support for 3d rendering)
2. Allow flexible UI (dynamically load xml UI similar to WPF XAML but more simple)
3. Simplicity for medium size projects (easier than WPF)
4. Cross platofrm (Window, mac, linux)
5. Testable (create UI integration tests)

# Open IDE
The OpenUI library comes with a OpenIDE application, which is a UI designer to define your xml definition file visually.
This is still a small mvp, the UI will significantly improve later.
![OpenIDE](doc/Demo.png)

# Guidelines
Branch name: <type>/<issue number>-<description>
Commit name: <type> #<issue number>: <message>
<type> is feature, bug, refactoring
e.g.

feature/100-add-button
refactoring/47-rendering-engine-cleanup
bug/50-fix-issue

and commits

"Feature #100: fixed issue"
"Refactoring #47: it looks better!"
"Bug #50: it now works!"

pull request messages

"Fixes #100" or "Closes #100" or "Resolves #100" so that pull request merging automatically closes issues.
"References #100" to link issue to PR.
DO NOT add these statements in the PR messages or commit messages because they cause confusion. Add them in the description of PRs.

# Build and test
Windows: it works. You can also build self contained
> dotnet publish -c Release -r win-x64 --self-contained
Linux: it works on WSL if you build with
> dotnet publish -c Release -r linux-x64 --self-contained

Note: With wsl it might require some tweaks (ask ChatGPT) to install dotnet environment etc.