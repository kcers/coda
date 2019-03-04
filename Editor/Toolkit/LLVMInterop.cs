using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
#if UNITY_2018_1_OR_NEWER
using UnityEngine;
#else
using System.Windows.Forms;
#endif

namespace TapVoxel.Coda.Editor.Toolkit {
	public static class StringExtensions {
		public static string Replace(this string s, int index, int length, string replacement) {
			var builder = new StringBuilder();
			builder.Append(s.Substring(0, index)).Append(replacement).Append(s.Substring(index + length));
			return builder.ToString();
		}
	}

	internal class LLVMInterop {
		static readonly Regex timelineRegex = new Regex(@", offset (\.\S+|\"".+\""|burst_abort)", RegexOptions.Compiled);
		internal static string ApplicationExePath() {
#if UNITY_2018_1_OR_NEWER
			return Application.dataPath + @"\TapVoxel\Coda\";
#else
			// Assume running in test harness
			return Path.GetDirectoryName(Application.ExecutablePath) + @"\";
#endif
		}

		internal static bool ExecuteCommand(string filename, string arguments, string stdin, out string stdout, out string stderr) {
			var stdoutSB = new StringBuilder();
			var stderrSB = new StringBuilder();
			ProcessStartInfo startInfo = new ProcessStartInfo {
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				FileName = $"{ApplicationExePath()}{filename}",
				WindowStyle = ProcessWindowStyle.Hidden,
				Arguments = arguments
			};
			try {
				using (Process exeProcess = new Process()) {
					exeProcess.StartInfo = startInfo;
					exeProcess.OutputDataReceived += (sender, e) => { stdoutSB.AppendLine(e.Data); };
					exeProcess.ErrorDataReceived += (sender, e) => { stderrSB.AppendLine(e.Data); };
					exeProcess.Start();
					exeProcess.BeginOutputReadLine();
					exeProcess.BeginErrorReadLine();
					if (!string.IsNullOrEmpty(stdin)) {
						exeProcess.StandardInput.Write(stdin + "\r\n");
					}

					exeProcess.StandardInput.Close();
					exeProcess.WaitForExit(20000);
					stderr = stderrSB.ToString();
					stdout = stdoutSB.ToString();
				}

				return true;
			} catch (Exception e) {
				stderr = e.Message;
				stdout = "";
				return false;
			}
		}

		internal static string Sanitize(string inAsm, out string err) {
			err = "";
			StringBuilder sb = new StringBuilder();
			//var timelineRegex = new Regex(@", offset (\.\S+|\"".+\""|burst_abort)", RegexOptions.Compiled);

			using (StringReader reader = new StringReader(inAsm)) {
				string line;
				while ((line = reader.ReadLine()) != null) {
					// The only thing necessary is for us to replace offset label operand arguments with zeroes for MCA to understand it
					var match = timelineRegex.Match(line);
					if (match.Success) {
						line = line.Replace(match.Index, match.Length, ", 0");
					}

					sb.AppendLine(line);
				}
			}

			return sb.ToString();
		}

		internal static string SanitizeLine(string inAsm) {
			var match = timelineRegex.Match(inAsm);
			if (match.Success)
			{
				inAsm = inAsm.Replace(match.Index, match.Length, ", 0");
			}

			return inAsm;
		}
	}
}
