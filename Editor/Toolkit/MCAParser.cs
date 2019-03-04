using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TapVoxel.Coda.Editor.Toolkit {
	public class MCAResult {
		public List<Node> Nodes;
		public CostingResult CostingResult;
		public string Error;
		public Dictionary<string, string> LabelFunctionName;
		public Dictionary<int, string> FilePaths;
	}

	internal class MCAParser {
		private ParseResult _parseResult;
		private string _mcaStdout;
		private MCAResult _result;
		private Dictionary<string, string> _labelFunctionName = new Dictionary<string, string>();
		private Dictionary<int, string> _filePath = new Dictionary<int, string>();

		internal MCAParser(ParseResult parseResult, string stdout) {
			_parseResult = parseResult;
			_mcaStdout = stdout;
			_result = new MCAResult();
		}

		internal MCAResult Analyze() {
			var nodes = new List<Node>();
			try {
				var timelineRegex = new Regex("^Timeline view\\:", RegexOptions.Compiled | RegexOptions.Multiline);
				var timelineItemRegex = new Regex("^\\[\\d+,(\\d+)\\]", RegexOptions.Compiled | RegexOptions.Multiline);
				var digit = new Regex("\\d+", RegexOptions.Compiled);
				var match = timelineRegex.Match(_mcaStdout);
				if (match.Success) {
					// Calculate max cycles - TODO: Can we get the cycle count from the header and use a simpler heuristic instead?
					var matchIndex = match.Index;
					var nl = _mcaStdout.IndexOf('\n', matchIndex);
					var n2 = _mcaStdout.IndexOf('\n', nl + 1);
					var line = _mcaStdout.Substring(nl + 1, n2 - nl - 1);
					var maxLen = 0;
					var firstDigit = 80;
					if (!line.StartsWith("Index")) {
						maxLen = line.Length - 1;
						var digitMatch1 = digit.Match(line);
						if (digitMatch1.Success) {
							firstDigit = Math.Min(firstDigit, digitMatch1.Index);
						}

						nl = n2;
						n2 = _mcaStdout.IndexOf('\n', nl + 1);
						line = _mcaStdout.Substring(nl + 1, n2 - nl - 1);
					}

					maxLen = Math.Max(maxLen, line.Length - 1);
					var digitMatch = digit.Match(line);
					if (digitMatch.Success) {
						firstDigit = Math.Min(firstDigit, digitMatch.Index);
					}

					var costing = new Costing(maxLen - firstDigit);

					// Parse each instruction timeline
					var itemMatch = timelineItemRegex.Match(_mcaStdout, matchIndex);
					var spaceNormalizeRegex = new Regex(@"(\s\s+|\t+)", RegexOptions.Compiled);
					var instructionIndex = _parseResult.ReorderedInstructionsList != null ? -1 : (_parseResult.BeginMarker != null ? _parseResult.BeginMarker.InstructionIndex : 0);
					var instructions = _parseResult.Instructions;
					var debugInfos = _parseResult.DebugInfo;
					var tokens = _parseResult.Tokens;
					var firstInstructionIndex = _parseResult.ReorderedInstructionsList != null ? 0 : instructionIndex;
					var lastInstructionIndex = _parseResult.ReorderedInstructionsList?.Count - 1 ?? (_parseResult.EndMarker != null ? _parseResult.EndMarker.InstructionIndex : instructions.Count - 1);
					while (itemMatch.Success && instructionIndex < lastInstructionIndex) {
						var eolPos = _mcaStdout.IndexOf('\n', itemMatch.Index);
						var itemLine = _mcaStdout.Substring(itemMatch.Index + firstDigit, maxLen - firstDigit);
						var asmLine = _mcaStdout.Substring(itemMatch.Index + maxLen + 3, eolPos - itemMatch.Index - maxLen - 4);
						var instIndexStr = itemMatch.Groups[1].Captures[0].Value;
						var instIndex = int.Parse(instIndexStr);
						var startCycle = itemLine.IndexOf("D");
						var executionStart = itemLine.IndexOf("e") - startCycle;
						var executionDone = itemLine.IndexOf("E") - startCycle;
						var waiting = itemLine.IndexOf("-") - startCycle;
						var endCycle = itemLine.IndexOf("R");

						var normalizedTimelAsm = spaceNormalizeRegex.Replace(asmLine, " ");

						string normalizedInstrAsm = "";
						InstructionInfo instruction = null;
						int beginIndex = _parseResult.ReorderedInstructionsList != null ? 0 : (_parseResult.BeginMarker != null ? _parseResult.BeginMarker.InstructionIndex : 0);
						if (instructionIndex != beginIndex || _parseResult.ReorderedInstructionsList != null) {
							++instructionIndex;
							instruction = instructions[_parseResult.ReorderedInstructionsList?[instructionIndex] ?? instructionIndex];
							normalizedInstrAsm = spaceNormalizeRegex.Replace(tokens[instruction.TokenIndex].Contents, " ");
							normalizedInstrAsm = LLVMInterop.SanitizeLine(normalizedInstrAsm);
						} else {
							while (normalizedInstrAsm != normalizedTimelAsm && instructionIndex < instructions.Count - 1) {
								++instructionIndex;
								instruction = instructions[_parseResult.ReorderedInstructionsList?[instructionIndex] ?? instructionIndex];
								normalizedInstrAsm = spaceNormalizeRegex.Replace(tokens[instruction.TokenIndex].Contents, " ");
								normalizedInstrAsm = LLVMInterop.SanitizeLine(normalizedInstrAsm);
							}

							firstInstructionIndex = instructionIndex;
						}

						if (instruction != null) {
							var dbgInfo = _parseResult.DebugInfo[instruction.DebugInfoIndex];
							for (var i = startCycle; i < endCycle; i++) {
								costing.AddInstruction(i, instructionIndex - firstInstructionIndex, endCycle - startCycle + 1);
							}
						} else {
							Reporting.ReportError("Ran out of instructions.");
						}

						nodes.Add(new Node(instIndex, asmLine, startCycle, executionStart, executionDone, waiting, endCycle != -1 ? endCycle - startCycle + 1 : 1));
						itemMatch = timelineItemRegex.Match(_mcaStdout, eolPos + 1);
					}

					// Load filename debug info
					var sectionDbgStrRegex = new Regex(@"^\s*\.section\s+\.debug_str", RegexOptions.Multiline);
					var foundSectionDbgStr = false;
					for (var i = 0; i < _parseResult.Tokens.Count; i++) {
						var token = _parseResult.Tokens[i];
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
						_result.Error = "Could not find debug_str section";
						Reporting.ReportError(_result.Error);
					}

					var foundSectionDebugInfo = false;
					var sectionDbgInfoRegex = new Regex(@"^\s*\.section\s+\.debug_info", RegexOptions.Multiline);
					for (var i = 0; i < _parseResult.Tokens.Count; i++) {
						var token = _parseResult.Tokens[i];
						if (token.IsDirective()) {
							var sectionDbgInfo = sectionDbgInfoRegex.Match(token.Contents);
							if (sectionDbgInfo.Success) {
								foundSectionDebugInfo = true;
								ParseSectionDbgInfo(i + 1);
								break;
							}
						}
					}

					if (!foundSectionDebugInfo) {
						_result.Error = "Could not find debug_info section";
						Reporting.ReportError(_result.Error);
					}

					_result.LabelFunctionName = _labelFunctionName;
					_result.FilePaths = _filePath;
					_result.Nodes = nodes;
					_result.CostingResult = costing.Analyze(instructions, _parseResult.ReorderedInstructionsList, firstInstructionIndex, _parseResult.ReorderedInstructionsList?.Count ?? ((_parseResult.EndMarker?.InstructionIndex ?? lastInstructionIndex) - firstInstructionIndex), debugInfos,
						_filePath);
				} else {
					_result.Error = "Could not find timeline information";
					Reporting.ReportError(_result.Error);
				}
			} catch (Exception e) {
				_result.Error = e.Message;
				Reporting.ReportError(_result.Error);
			}

			return _result;
		}

		private void ParseSectionDbgStr(int startIndex) {
			var sectionRegex = new Regex(@"^\.section");
			var labelRegex = new Regex(@"^\.[A-Za-z_]+(\d+)");
			var dataRegex = new Regex(@"^.asciz\s+\""([^\""]+)\""");
			string currentLabel = "";

			for (var i = startIndex; i < _parseResult.Tokens.Count; i++) {
				var token = _parseResult.Tokens[i];
				if (token.IsDirective()) {
					var match = dataRegex.Match(token.Contents);
					if (match.Success) {
						var str = match.Groups[1].Value;
						_labelFunctionName[currentLabel] = str;
					} else if (sectionRegex.IsMatch(token.Contents)) {
						break;
					}
				} else if (token.IsLabel()) {
					var labelMatch = labelRegex.Match(token.Contents);
					if (labelMatch.Success) {
						currentLabel = token.Contents.Substring(0, token.Contents.Length);
					}
				}
			}
		}

		private void ParseSectionDbgInfo(int startIndex) {
			var byteMarkerRegex = new Regex(@"^\.byte\s+(\d+)", RegexOptions.Compiled);
			var sectionRegex = new Regex(@"^\.section", RegexOptions.Compiled);
			var nameLabelRegex = new Regex(@".long\s+(.*)");
			for (var i = startIndex; i < _parseResult.Tokens.Count; i++) {
				var token = _parseResult.Tokens[i];
				if (token.IsDirective()) {
					var byteMarkerMatch = byteMarkerRegex.Match(token.Contents);
					if (byteMarkerMatch.Success) {
						int val = int.Parse(byteMarkerMatch.Groups[1].Value);
						if (val == 1) {
							var byteMarker2Match = byteMarkerRegex.Match(_parseResult.Tokens[i + 1].Contents);
							if (byteMarker2Match.Success) {
								int val2 = int.Parse(byteMarker2Match.Groups[1].Value);
								if (val2 == 87) {
									// Got a match
									var nameMatch = nameLabelRegex.Match(_parseResult.Tokens[i + 2].Contents);
									if (nameMatch.Success) {
										var nameLabel = nameMatch.Groups[1].Value;
										var fileIndexMatch = byteMarkerRegex.Match(_parseResult.Tokens[i + 3].Contents);
										var lineNumberMatch = byteMarkerRegex.Match(_parseResult.Tokens[i + 4].Contents);
										if (fileIndexMatch.Success && lineNumberMatch.Success) {
											var name = _labelFunctionName[nameLabel];
											var fileIndex = int.Parse(fileIndexMatch.Groups[1].Value);
											var lineNumber = int.Parse(lineNumberMatch.Groups[1].Value);
											_filePath[fileIndex] = name;
										}
									}
								}
							}
						}
					} else if (sectionRegex.IsMatch(token.Contents)) {
						break;
					}
				}
			}
		}
	}
}