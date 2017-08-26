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
using Screencap.Util;

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

        private CaptureType captureType;

        public CaptureWindow(CaptureType captureType) {
            InitializeComponent();

            this.captureType = captureType;

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
            if (this.captureType != CaptureType.REGION) {
                return;
            }

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
            if (this.captureType != CaptureType.WINDOW && (e.LeftButton == MouseButtonState.Released || rect == null))
                return;

            var pos = e.GetPosition(canvas);
            var dpi = GetDPI();

            switch (this.captureType) {
                case CaptureType.REGION:
                    var x = Math.Min(pos.X, startPoint.X);
                    var y = Math.Min(pos.Y, startPoint.Y);

                    var w = Math.Max(pos.X, startPoint.X) - x;
                    var h = Math.Max(pos.Y, startPoint.Y) - y;

                    rect.Width = w;
                    rect.Height = h;

                    Canvas.SetLeft(rect, x);
                    Canvas.SetTop(rect, y);
                    break;
                case CaptureType.WINDOW:
                    var window = ProcessUtil.GetOpenWindows()
                        .Where(win => pos.X >= win.Rect.Left / dpi.X && 
                                        pos.X <= win.Rect.Right / dpi.X && 
                                        pos.Y >= win.Rect.Top / dpi.Y && 
                                        pos.Y <= win.Rect.Bottom / dpi.Y
                        ).FirstOrDefault();
                    Console.WriteLine(
                        "Handling window at X: {0}, Y: {1}. Title: {2}", 
                        window.Rect.Left / dpi.X, 
                        window.Rect.Top / dpi.Y, 
                        window.Name
                    );
                    if (window.Rect.Left != window.Rect.Right && window.Rect.Top != window.Rect.Bottom) {
                        if (rect != null) {
                            canvas.Children.Remove(rect);
                        }
                        rect = new System.Windows.Shapes.Rectangle {
                            Stroke = System.Windows.Media.Brushes.White,
                            StrokeThickness = 2,
                            Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(20, 0, 0, 0))
                        };

                        Canvas.SetLeft(rect, window.Rect.Left / dpi.X);
                        Canvas.SetTop(rect, window.Rect.Top / dpi.Y);
                        rect.Width = (window.Rect.Right - window.Rect.Left + 1) / dpi.X;
                        rect.Height = (window.Rect.Bottom - window.Rect.Top + 1) / dpi.Y;

                        Console.WriteLine(
                            "Drawing rect at X={0} Y={1} w={2} h={3}", 
                            window.Rect.Left / dpi.X, 
                            window.Rect.Top / dpi.Y, 
                            rect.Width, 
                            rect.Height
                         );

                        canvas.Children.Add(rect);

                        if (e.LeftButton == MouseButtonState.Pressed) {
                            captureHandler(sender, null);
                        }
                    }
                    break;
            }
        }

        private void canvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (this.captureType == CaptureType.REGION) {
                captureHandler(sender, e);
            }
        }

        private void captureHandler(object sender, MouseButtonEventArgs e) {
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
