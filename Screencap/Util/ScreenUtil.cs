using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Text;
using System.Threading.Tasks;

namespace Screencap.Util {
    class ScreenUtil {
        public static dynamic GetScreenResolution() {
            var dpi = GetDPI();
            double ScreenHeight = SystemParameters.PrimaryScreenHeight * dpi.Y;
            double ScreenWidth = SystemParameters.PrimaryScreenWidth * dpi.X;

            return new {
                Height = ScreenHeight,
                Width = ScreenWidth
            };
        }

        public static dynamic GetDPI() {
            Window MainWindow = Application.Current.MainWindow;
            PresentationSource MainWindowPresentationSource = PresentationSource.FromVisual(MainWindow);
            Matrix m = MainWindowPresentationSource.CompositionTarget.TransformToDevice;
            var DpiWidthFactor = m.M11;
            var DpiHeightFactor = m.M22;

            return new {
                X = m.M11,
                Y = m.M22
            };
        }
    }
}
