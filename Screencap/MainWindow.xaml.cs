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
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Screencap {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void FullscreenButton_Click(object sender, RoutedEventArgs e) {
            new CaptureWindow(CaptureType.FULLSCREEN);
        }

        private void RegionButton_Click(object sender, RoutedEventArgs e) {
            var window = new CaptureWindow(CaptureType.REGION);
            window.Show();
        }

        private void WindowButton_Click(object sender, RoutedEventArgs e) {
            var window = new CaptureWindow(CaptureType.WINDOW);
            window.Show();
        }
    }
}
