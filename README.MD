# UISharp
This is a new cross-platform project written in C#.
The project has just started so expect a lot of updates to the readme file and to the framework in the future!
The active branch will be "mvp", which will be further merged into main.

The main goal is
1. Allow using OpenGL and svg vector graphics (efficient, high quality, support for 3d rendering)
2. Allow flexible UI (dynamically load xml UI similar to WPF XAML but more simple)
3. Simplicity for medium size projects (easier than WPF)
4. Cross platofrm (Window, mac, linux)
5. Testable (create UI integration tests)

# UISharp IDE
The UISharp library comes with a IDE application, which is a UI designer to define your xml definition file visually.
This is still a small mvp, the UI will significantly improve later.
![UISharp IDE](doc/Demo.png)

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

# SDL Windows Hoock
If needed add the following class for Windows (but event filter should suffice)

namespace RetroDev.UISharp.Core.Windowing.SDL;

using System;
using System.Runtime.InteropServices;
using SDL2;

using System;
using System.Runtime.InteropServices;
using SDL2;

public class SdlWindowHook
{
    private nint _window;
    private nint _hwnd;
    private nint _oldWndProc;
    private delegate nint WndProc(nint hWnd, uint msg, nint wParam, nint lParam);
    private WndProc _newWndProc;
    private bool _resizing;

    private const int GWL_WNDPROC = -4;
    private const uint WM_ENTERSIZEMOVE = 0x0231;
    private const uint WM_EXITSIZEMOVE = 0x0232;
    private const uint WM_SIZING = 0x0214;

    [DllImport("user32.dll")]
    private static extern nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong);

    [DllImport("user32.dll")]
    private static extern nint GetWindowLongPtr(nint hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern nint CallWindowProc(nint lpPrevWndFunc, nint hWnd, uint msg, nint wParam, nint lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public POINT pt;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    [DllImport("user32.dll")]
    private static extern int PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

    [DllImport("user32.dll")]
    private static extern int TranslateMessage(ref MSG lpMsg);

    [DllImport("user32.dll")]
    private static extern IntPtr DispatchMessage(ref MSG lpMsg);

    public SdlWindowHook(nint sdlWindow)
    {
        _window = sdlWindow;

        SDL.SDL_SysWMinfo wmInfo = new SDL.SDL_SysWMinfo();
        SDL.SDL_VERSION(out wmInfo.version);
        if (SDL.SDL_GetWindowWMInfo(_window, ref wmInfo) == SDL.SDL_bool.SDL_TRUE)
        {
            _hwnd = wmInfo.info.win.window;
            Console.WriteLine($"HWND obtained successfully: 0x{_hwnd:X}");
        }
        else
        {
            throw new Exception("SDL_GetWindowWMInfo failed.");
        }

        _newWndProc = HookWndProc;
        _oldWndProc = GetWindowLongPtr(_hwnd, GWL_WNDPROC);
        if (_oldWndProc == nint.Zero)
        {
            throw new Exception("Failed to retrieve original WndProc.");
        }

        Console.WriteLine($"Original WndProc: 0x{_oldWndProc:X}");
        SetWindowLongPtr(_hwnd, GWL_WNDPROC, Marshal.GetFunctionPointerForDelegate(_newWndProc));
    }

    private nint HookWndProc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        switch (msg)
        {
            case WM_SIZING:
                Console.WriteLine("Resize move in progres...");
                Console.WriteLine($"Current Thread Name: {Thread.CurrentThread.Name ?? "Unnamed"}");
                // if (_resizing)
                // {
                //     // Process pending messages manually
                //     ProcessPendingMessages();
                // }
                break;
        }

        return CallWindowProc(_oldWndProc, hWnd, msg, wParam, lParam);
    }

    private void ProcessPendingMessages()
    {
        MSG msg;
        while (PeekMessage(out msg, IntPtr.Zero, 0, 0, 1) != 0)
        {
            TranslateMessage(ref msg);
            DispatchMessage(ref msg);
        }
    }

    public void Unhook()
    {
        SetWindowLongPtr(_hwnd, GWL_WNDPROC, _oldWndProc);
    }
}
