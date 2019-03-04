using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TapVoxel.Coda.Editor {
	static class Util {
		public static void ExportDetailedAnalysis(string analysisText) {
			var filename = GetTempFilePathWithExtension(".txt");
			File.WriteAllText(filename, analysisText);
			System.Diagnostics.Process.Start(filename);
		}

		private static string GetTempFilePathWithExtension(string extension)
		{
			var path = Path.GetTempPath();
			var fileName = Guid.NewGuid().ToString() + extension;
			return Path.Combine(path, fileName);
		}
	}
}