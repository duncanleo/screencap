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
                    var cap = Capture(0, 0, (int)screenRes.Width, (int)screenRes.Height);
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
                Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(20, 0, 0, 0))
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

            var dpi = GetDPI();

            // Capture
            var cap = Capture(
                (int)(Canvas.GetLeft(rect) * dpi.X), 
                (int)(Canvas.GetTop(rect) * dpi.Y),
                (int)(rect.Width * dpi.X), 
                (int)(rect.Height * dpi.Y)
            );
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            cap.Save(System.IO.Path.Combine(path, GenerateFileName()));

            rect = null;
            Close();
        }

        private dynamic GetDPI() {
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

        private dynamic GetScreenResolution() {
            var dpi = GetDPI();
            double ScreenHeight = SystemParameters.PrimaryScreenHeight * dpi.Y;
            double ScreenWidth = SystemParameters.PrimaryScreenWidth * dpi.X;

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

        private Bitmap Capture(int left, int top, int width, int height) {
            Hide();
            Bitmap bitmap = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(bitmap)) {
                g.CopyFromScreen(
                    new System.Drawing.Point(left, top),
                    new System.Drawing.Point(0, 0),
                    bitmap.Size
                );
            }

            Show();

            return bitmap;
        }

        
    }
}
