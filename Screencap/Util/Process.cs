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

        [DllImport("dwmapi.dll")]
        static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out RECT pvAttribute, int cbAttribute);

        [Flags]
        public enum DwmWindowAttribute : uint {
            DWMWA_NCRENDERING_ENABLED = 1,
            DWMWA_NCRENDERING_POLICY,
            DWMWA_TRANSITIONS_FORCEDISABLED,
            DWMWA_ALLOW_NCPAINT,
            DWMWA_CAPTION_BUTTON_BOUNDS,
            DWMWA_NONCLIENT_RTL_LAYOUT,
            DWMWA_FORCE_ICONIC_REPRESENTATION,
            DWMWA_FLIP3D_POLICY,
            DWMWA_EXTENDED_FRAME_BOUNDS,
            DWMWA_HAS_ICONIC_BITMAP,
            DWMWA_DISALLOW_PEEK,
            DWMWA_EXCLUDED_FROM_PEEK,
            DWMWA_CLOAK,
            DWMWA_CLOAKED,
            DWMWA_FREEZE_REPRESENTATION,
            DWMWA_LAST
        }

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
                   int size = Marshal.SizeOf(typeof(RECT));
                   DwmGetWindowAttribute(p.MainWindowHandle, (int)DwmWindowAttribute.DWMWA_EXTENDED_FRAME_BOUNDS, out rect, size);
                   //var success = GetWindowRect(p.MainWindowHandle, out rect);
                   //if (!success) {
                   //    Console.WriteLine("Error getting rect for {0}", p.MainWindowTitle);
                   //}
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
