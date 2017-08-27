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
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Screencap.Util;
using System.Windows.Media.Effects;

namespace Screencap {
    public enum CaptureType {
        FULLSCREEN,
        REGION,
        WINDOW
    }

    public enum SaveType {
        DISK,
        CLIPBOARD
    }

    /// <summary>
    /// Interaction logic for CaptureWindow.xaml
    /// </summary>
    public partial class CaptureWindow : Window {
        private System.Windows.Point startPoint;
        private System.Windows.Shapes.Rectangle rect;
        private TextBlock textBlock;

        private List<ProcessUtil.Window> windows = ProcessUtil.GetOpenWindows();

        private CaptureType captureType;
        private SaveType saveType;

        public CaptureWindow(CaptureType captureType, SaveType saveType = SaveType.DISK) {
            InitializeComponent();

            this.captureType = captureType;
            this.saveType = saveType;

            switch (captureType) {
                case CaptureType.FULLSCREEN:
                    var screenRes = ScreenUtil.GetScreenResolution();

                    Hide();
                    var cap = ImageUtil.CaptureScreenshot(0, 0, (int)screenRes.Width, (int)screenRes.Height);
                    Show();

                    switch (saveType) {
                        case SaveType.DISK:
                            cap.Save(DiskUtil.GenerateDiskFilePath(), ImageFormat.Png);
                            break;
                        case SaveType.CLIPBOARD:
                            Clipboard.SetImage(ImageUtil.ConvertBitmapToBitmapSource(cap));
                            break;
                    }
                    
                    Close();
                    break;
                case CaptureType.REGION:
                    Cursor = Cursors.Cross;
                    break;
                case CaptureType.WINDOW:
                    Cursor cameraCursor = new Cursor(
                        Application.GetResourceStream(new Uri("Images/camera.cur", UriKind.Relative)).Stream
                    );
                    Cursor = cameraCursor;
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
            var dpi = ScreenUtil.GetDPI();

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
                    var window = windows
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
                            Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(90, 235, 248, 254))
                        };

                        Canvas.SetLeft(rect, window.Rect.Left / dpi.X - Left);
                        Canvas.SetTop(rect, window.Rect.Top / dpi.Y - Top);
                        rect.Width = Math.Floor((window.Rect.Right - window.Rect.Left) / dpi.X);
                        rect.Height = Math.Floor((window.Rect.Bottom - window.Rect.Top) / dpi.Y);

                        Console.WriteLine(
                            "Drawing rect at X={0} Y={1} w={2} h={3}", 
                            window.Rect.Left / dpi.X, 
                            window.Rect.Top / dpi.Y, 
                            rect.Width, 
                            rect.Height
                         );

                        canvas.Children.Add(rect);

                        drawTargetWindowName(window.Name);

                        if (e.LeftButton == MouseButtonState.Pressed) {
                            ProcessUtil.SetForegroundWindow(window.Process.MainWindowHandle);
                            captureByCanvasRect();
                            canvas.Children.Remove(textBlock);
                        }
                    }
                    break;
            }
        }

        private void canvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (this.captureType == CaptureType.REGION) {
                captureByCanvasRect();
            }
        }

        private void drawTargetWindowName(string name) {
            if (textBlock != null) {
                canvas.Children.Remove(textBlock);
            }
            textBlock = new TextBlock();
            textBlock.Text = name;
            textBlock.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0,0,0));
            textBlock.TextAlignment = TextAlignment.Center;
            textBlock.FontSize = 17;
            textBlock.FontWeight = FontWeights.Medium;
            textBlock.Effect = new DropShadowEffect {
                Color = new System.Windows.Media.Color { A = 255, R = 255, B = 255, G = 255},
                Opacity = 1,
                ShadowDepth = 0,
                BlurRadius = 35
            };
            textBlock.Width = rect.Width;
            Canvas.SetLeft(textBlock, Canvas.GetLeft(rect));
            Canvas.SetTop(textBlock, Canvas.GetTop(rect) + rect.Height / 2.0 + rect.Height / 2.5);
            canvas.Children.Add(textBlock);
        }

        private void captureByCanvasRect() {
            canvas.Children.Remove(rect);

            var dpi = ScreenUtil.GetDPI();
            var res = ScreenUtil.GetScreenResolution();

            // Capture
            Hide();
            var cap = ImageUtil.CaptureScreenshot(
                (int)(Canvas.GetLeft(rect) * dpi.X + Left),
                (int)(Canvas.GetTop(rect) * dpi.Y + Top),
                (int)(rect.Width * dpi.X + Left / dpi.X),
                (int)(rect.Height * dpi.Y + Top/ dpi.Y)
            );
            Show();

            switch(this.saveType) {
                case SaveType.DISK:
                    cap.Save(DiskUtil.GenerateDiskFilePath(), ImageFormat.Png);
                    break;
                case SaveType.CLIPBOARD:
                    Clipboard.SetImage(ImageUtil.ConvertBitmapToBitmapSource(cap));
                    break;
            }

            rect = null;
            Close();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                Close();
            }
        }
    }
}
