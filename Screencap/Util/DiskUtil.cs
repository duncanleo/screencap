using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screencap.Util {
    class DiskUtil {
        public static string GenerateDiskFilePath() {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            return System.IO.Path.Combine(path, GenerateFileName());
        }

        private static string GenerateFileName() {
            var dateTime = DateTime.Now;
            return String.Format(
                "Screen Shot {0} at {1}.png",
                dateTime.ToString("yyyy-MM-dd"),
                dateTime.ToString("H.mm.ss tt")
            );
        }
    }
}
