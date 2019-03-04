using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
#if UNITY_2018_1_OR_NEWER
using UnityEditor;
#else
using System.Windows.Forms;
#endif

namespace TapVoxel.Coda.Editor.Toolkit {
	public interface IReporting {
		bool PrintReport(out string err);
	}

	public class DetailedReport : IReporting {
		private readonly Analysis _analysis;

		public DetailedReport(Analysis analysis) {
			_analysis = analysis;
		}

		public bool PrintReport(out string err) {
			Reporting.ExportDetailedAnalysis(_analysis.DetailedText);
			err = "";
			return true;
		}
	}

	public class ProfileReport : IReporting {
		private readonly Analysis _analysis;
		private Dictionary<string, string> _labelFunctionName = new Dictionary<string, string>();

		public ProfileReport(Analysis analysis) {
			_analysis = analysis;
		}

		public bool PrintReport(out string err) {
			var sectionDbgStrRegex = new Regex(@"^\s*\.section\s+\.debug_str", RegexOptions.Multiline);

			var foundSectionDbgStr = false;
			for (var i = 0; i < _analysis.Parser.Tokens.Count; i++) {
				var token = _analysis.Parser.Tokens[i];
				if (token.IsDirective()) {
					var sectionDbgStr = sectionDbgStrRegex.Match(token.Contents);
					if (sectionDbgStr.Success) {
						foundSectionDbgStr = true;
						ParseSectionDbgStr(i + 1);
						break;
					}
				}
			}

			if (!foundSectionDbgStr) {
				err = "Could not find debug_str section";
				return false;
			}

			string displayErr = "";

			var tmpPath = Reporting.GetTempFilePathWithExtension(".html");
			using (FileStream fs = new FileStream(tmpPath, FileMode.CreateNew)) {
				using (TextWriter writer = new StreamWriter(fs)) {
					writer.Write("<html><head><style>");
					writer.Write(Reporting.GetTextResource("prism.css"));
					writer.Write("</style></head><body><h1>Profiling Analysis (CPU Resource Pressure):</h1>");
					if (displayErr != "") {
						writer.Write("<div class=\"warning\" style=\"background-color:#FFEEEE;\">" + displayErr + "</div>");
					}
					var costing = _analysis.Result.CostingResult;
					var fileIndices = costing.LineCosting.Keys.ToArray();
					Array.Sort(fileIndices);
					var sb = new StringBuilder();
					var dataLine = new List<string>();
					foreach (var fileIndex in fileIndices) {
						writer.Write($"<h2>{_analysis.Result.FilePaths[fileIndex]}</h2>");
						var costFile = costing.LineCosting[fileIndex];
						var lineNumbers = costFile.Keys.ToArray();
						Array.Sort(lineNumbers);
						var pIdx = 0;
						var percentileIndex = new Dictionary<int, int>();
						foreach (var kd in costing.LinePercentiles) {
							percentileIndex[kd.Key] = pIdx++;
						}

						var lineIndex = 1;
						var lastLine = lineNumbers[0];
						var offset = 0;
						foreach (var lineNumber in lineNumbers) {
							if (lineNumber != lastLine + 1) {
								sb.AppendLine($"{lastLine + 1} : // ...");
								//for (var ins = lastLine + 1; ins < lineNumber; ++ins) {
								//	sb.AppendLine($"{ins,4}");
									//offset++;
								//}
								offset += 1;
							}
							dataLine.Add($"{percentileIndex[lineNumber] * 100 / costing.LinePercentiles.Count}:{lineIndex + offset}");
							//sb.AppendLine($"{lineNumber, 4} : {percentileIndex[lineNumber]} : {percentileIndex[lineNumber] * 100 / costing.LinePercentiles.Count} : {costFile[lineNumber]} : {costing.LineCode[fileIndex][lineNumber]}");
							sb.AppendLine($"{lineNumber,4} : {costing.LineCode[fileIndex][lineNumber]}");
							lineIndex++;
							lastLine = lineNumber;
						}
					}
					writer.Write("<pre class=\"line-numbers\" data-line=\"" + string.Join(",", dataLine) + "\"><code class=\"language-csharp\">");
					writer.Write(sb.ToString());
#if OLD
					writer.Write(@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BurstParserTest.Toolkit {
	public class Marker {
		public enum MarkerKind {
			Begin,
			End,
		}

		public int LabelIndex { get; }
		public int TokenIndex { get; }
		public MarkerKind Kind { get; }

		public Marker(MarkerKind kind, int labelIndex, int tokenIndex) {
			this.LabelIndex = labelIndex;
			this.TokenIndex = tokenIndex;
			this.Kind = kind;
		}
	}
}");
#endif
					writer.Write("</code></pre><script>");
					writer.Write(Reporting.GetTextResource("prism.js"));
					writer.Write("</script><script>");
					writer.Write(Reporting.GetTextResource("line-heatmap.js"));
					writer.Write("</script></body></html>");
				}
			}

			System.Diagnostics.Process.Start(tmpPath);

			err = "";
			return true;
		}

		private void ParseSectionDbgStr(int startIndex) {
			var sectionRegex = new Regex(@"^\s+\.section");
			var labelRegex = new Regex(@"^\.[A-Za-z_.]+:");
			var dataRegex = new Regex(@"^\s+.asciz\s+\""([^\""]+)\""");
			string currentLabel = "";

			for (var i = startIndex; i < _analysis.Parser.Tokens.Count; i++) {
				var token = _analysis.Parser.Tokens[i];
				if (token.IsDirective()) {
					if (labelRegex.IsMatch(token.Contents)) {
						currentLabel = token.Contents.Substring(0, token.Contents.Length - 1);
					} else if (sectionRegex.IsMatch(token.Contents)) {
						break;
					} else {
						var match = dataRegex.Match(token.Contents);
						if (match.Success) {
							var str = match.Groups[1].Value;
							_labelFunctionName[currentLabel] = str;
						}
					}
				}
			}
		}
	}

	static class Reporting {
		internal static string GetTextResource(string resourceFilename) {
			/*Assembly assembly = Assembly.GetExecutingAssembly();
			var resourceName = "TapVoxel.Coda.Editor.Toolkit." + resourceFilename;
			using (Stream stream = assembly.GetManifestResourceStream(resourceName)) {
				using (StreamReader reader = new StreamReader(stream)) {
					string result = reader.ReadToEnd();
					return result;
				}
			}*/
			var filename = LLVMInterop.ApplicationExePath() + resourceFilename;
			return File.ReadAllText(filename);
		}

		internal static void ExportDetailedAnalysis(string analysisText) {
			var filename = GetTempFilePathWithExtension(".txt");
			File.WriteAllText(filename, analysisText);
			System.Diagnostics.Process.Start(filename);
		}

		internal static string GetTempFilePathWithExtension(string extension) {
			var path = Path.GetTempPath();
			var fileName = Guid.NewGuid().ToString() + extension;
			return Path.Combine(path, fileName);
		}

		internal static void ReportError(string err) {
#if UNITY_2018_1_OR_NEWER
			EditorUtility.DisplayDialog("Error", "Could not find timeline information", "OK");
#else
			MessageBox.Show(err, "Report Generation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
#endif
		}
	}
}