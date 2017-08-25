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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;

namespace Screencap {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void TestButton_Click(object sender, RoutedEventArgs e) {
            Application.Current.MainWindow.Hide();

            var res = GetScreenResolution();
            Bitmap bitmap = new Bitmap(
                (int)res.Width,
                (int)res.Height
            );

            using (Graphics g = Graphics.FromImage(bitmap)) {
                g.CopyFromScreen(
                    (int)SystemParameters.VirtualScreenLeft,
                    (int)SystemParameters.VirtualScreenTop,
                    0,
                    0,
                    bitmap.Size
                );

                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                bitmap.Save(System.IO.Path.Combine(path, "test.jpg"));

                Application.Current.MainWindow.Show();
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
    }
}
