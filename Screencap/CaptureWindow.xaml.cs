using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Screencap {
    public enum CaptureType {
        FULLSCREEN,
        REGION,
        WINDOW
    }

    /// <summary>
    /// Interaction logic for CaptureWindow.xaml
    /// </summary>
    public partial class CaptureWindow : Window {
        

        public CaptureWindow(CaptureType captureType) {
            InitializeComponent();

            switch (captureType) {
                case CaptureType.FULLSCREEN:
                    var screenRes = GetScreenResolution();
                    var cap = Capture((int)screenRes.Width, (int)screenRes.Height);
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    cap.Save(System.IO.Path.Combine(path, GenerateFileName()));
                    Close();
                    break;
            }
        }

        private dynamic GetScreenResolution() {
            Window MainWindow = Application.Current.MainWindow;
            PresentationSource MainWindowPresentationSource = PresentationSource.FromVisual(MainWindow);
            Matrix m = MainWindowPresentationSource.CompositionTarget.TransformToDevice;
            var DpiWidthFactor = m.M11;
            var DpiHeightFactor = m.M22;
            double ScreenHeight = SystemParameters.PrimaryScreenHeight * DpiHeightFactor;
            double ScreenWidth = SystemParameters.PrimaryScreenWidth * DpiWidthFactor;

            return new {
                Height = ScreenHeight,
                Width = ScreenWidth
            };
        }

        private string GenerateFileName() {
            var dateTime = DateTime.Now;
            return String.Format(
                "Screen Shot {0} at {1}.png", 
                dateTime.ToString("yyyy-MM-dd"),
                dateTime.ToString("H.mm.ss aa")
            );
        }

        private Bitmap Capture(int width, int height) {
            Bitmap bitmap = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(bitmap)) {
                g.CopyFromScreen(
                    (int)SystemParameters.VirtualScreenLeft,
                    (int)SystemParameters.VirtualScreenTop,
                    0,
                    0,
                    bitmap.Size
                );
                

                //Application.Current.MainWindow.Show();
            }

            return bitmap;
        }
    }
}
