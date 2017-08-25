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

        static List<RECT> GetOpenWindows() {
            return Process.GetProcesses()
               .Where(p => p.MainWindowHandle != IntPtr.Zero)
               .Where(p => !String.IsNullOrEmpty(p.MainWindowTitle))
               .Where(p => {
                   WINDOWPLACEMENT wp = new WINDOWPLACEMENT();
                   GetWindowPlacement(p.MainWindowHandle, ref wp);
                   return wp.showCmd != 2; // Minimised
               })
               .Select(p => {
                   var rect = new RECT();
                   GetWindowRect(p.MainWindowHandle, out rect);
                   return rect;
               })
               .ToList();
        }
    }
}
