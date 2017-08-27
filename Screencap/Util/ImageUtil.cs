using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;

namespace Screencap.Util {
    class ImageUtil {
        public static Bitmap CaptureScreenshot(int left, int top, int width, int height) {
            Bitmap bitmap = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(bitmap)) {
                g.CopyFromScreen(
                    new System.Drawing.Point(left, top),
                    new System.Drawing.Point(0, 0),
                    bitmap.Size
                );
            }

            return bitmap;
        }

        public static BitmapSource ConvertBitmapToBitmapSource(Bitmap bitmap) {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                                bitmap.GetHbitmap(),
                                IntPtr.Zero,
                                Int32Rect.Empty,
                                BitmapSizeOptions.FromEmptyOptions()
                            );
        }
    }
}
