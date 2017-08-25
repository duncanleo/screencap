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
        private System.Windows.Point startPoint;
        private System.Windows.Shapes.Rectangle rect;

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

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e) {
            startPoint = e.GetPosition(canvas);

            rect = new System.Windows.Shapes.Rectangle {
                Stroke = System.Windows.Media.Brushes.White,
                StrokeThickness = 2,
                Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(10, 255, 255, 255))
            };

            Canvas.SetLeft(rect, startPoint.X);
            Canvas.SetTop(rect, startPoint.Y);
            canvas.Children.Add(rect);
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Released || rect == null)
                return;

            var pos = e.GetPosition(canvas);

            var x = Math.Min(pos.X, startPoint.X);
            var y = Math.Min(pos.Y, startPoint.Y);

            var w = Math.Max(pos.X, startPoint.X) - x;
            var h = Math.Max(pos.Y, startPoint.Y) - y;

            rect.Width = w;
            rect.Height = h;

            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e) {
            canvas.Children.Remove(rect);
            rect = null;
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
                dateTime.ToString("H.mm.ss tt")
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
            }

            return bitmap;
        }

        
    }
}
