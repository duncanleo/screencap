using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Screencap.Util {
    class ProcessUtil {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        private struct WINDOWPLACEMENT {
            public int length;
            public int flags;
            public int showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;
        }

        public struct Window {
            public string Name;
            public RECT Rect;
            public Process Process;
        }

        public static List<Window> GetOpenWindows() {
            return Process.GetProcesses()
               .Where(p => p.MainWindowHandle != IntPtr.Zero)
               .Where(p => !String.IsNullOrEmpty(p.MainWindowTitle))
               .Where(p => {
                   WINDOWPLACEMENT wp = new WINDOWPLACEMENT();
                   GetWindowPlacement(p.MainWindowHandle, ref wp);
                   return wp.showCmd != 2; // Minimised
               })
               .Where(p => p.ProcessName != "ShellExperienceHost")
               .Where(p => p.Id != Process.GetCurrentProcess().Id)
               .Select(p => {
                   var rect = new RECT();
                   var success = GetWindowRect(p.MainWindowHandle, out rect);
                   if (!success) {
                       Console.WriteLine("Error getting rect for {0}", p.MainWindowTitle);
                   }
                   return new Window {
                       Name = p.MainWindowTitle,
                       Rect = rect,
                       Process = p
                   };
               })
               .ToList();
        }
    }
}
