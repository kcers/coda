using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace TapVoxel.Coda.Editor.Toolkit {
	public class ParseResult {
		public List<Token> Tokens;
		public List<string> Labels;
		public List<InstructionInfo> Instructions;
		public List<DebugInfo> DebugInfo;
		public Marker BeginMarker;
		public Marker EndMarker;
		public List<int> ReorderedInstructionsList;
	}
	public class Parser {
		private List<Token> _tokens;
		private List<string> _labels = new List<string>();
		private int _currentLabelIndex = -1;
		private List<InstructionInfo> _instructions = new List<InstructionInfo>();
		private List<DebugInfo> _debugInfos = new List<DebugInfo>();
		private HashSet<int> _fileIndexSet = new HashSet<int>();
		private Dictionary<int, string> _filenames = new Dictionary<int, string>();
		private int _currentDebugInfoIndex = -1;
		private Regex _jmpRegex = new Regex(@"^\s*j\S+\s+(.*)", RegexOptions.Compiled);
		private Regex _dbgInfoRegex = new Regex(@"^<color[^>]+>\((\d+),?(?<line>\d+)? : ([^\)]+)\)(.*)</color>\s*$", RegexOptions.Compiled);
		private Regex _markerRegex = new Regex(@".*TapVoxel\.Coda\.Markers\.(Begin|End)\(\)", RegexOptions.Compiled);
		private Marker _beginMarker;
		private Marker _endMarker;
		private Dictionary<int, int> _labelInstructionIndex = new Dictionary<int, int>();
		private Dictionary<int, int> _labelLastInstructionIndex = new Dictionary<int, int>();
		private List<int> _reorderedInstructionsList;

		public List<Token> Tokens => _tokens;
		public List<string> Labels => _labels;
		public List<InstructionInfo> Instructions => _instructions;
		public List<DebugInfo> DebugInfo => _debugInfos;
		public Marker BeginMarker => _beginMarker;
		public Marker EndMarker => _endMarker;
		public List<int> ReorderedInstructionsList => _reorderedInstructionsList;

		public Parser(List<Token> tokens) {
			_tokens = tokens;
		}

		public override string ToString() {
			var sb = new StringBuilder();
			if (_beginMarker != null) {
				sb.AppendLine($"; Begin marker at label {_labels[_beginMarker.LabelIndex]}, index {_beginMarker.TokenIndex}");
			}

			if (_endMarker != null) {
				sb.AppendLine($"; End marker at label {_labels[_endMarker.LabelIndex]}, index {_endMarker.TokenIndex}");
			}

			if (_debugInfos.Count > 0 && _labels.Count > 0) {
				var dbgInfoIndex = 0;
				var prevDbgInfoIndex = -1;
				for (var i = 0; i < _instructions.Count; i++) {
					var instruction = _instructions[i];
					var token = _tokens[instruction.TokenIndex];
					var tokenIndex = instruction.TokenIndex;
					if (token.Type == Token.TokenType.Instruction) {
						if (SeekDbgInfo(tokenIndex, ref dbgInfoIndex)) {
							if (prevDbgInfoIndex != dbgInfoIndex) {
								var dbgInfo = _debugInfos[dbgInfoIndex];
								sb.AppendLine($"; {_filenames[dbgInfo.FileIndex]}:{dbgInfo.LineNumber} : {dbgInfo.Code}");
							}

							prevDbgInfoIndex = dbgInfoIndex;
						}

						if (instruction.LabelIndex >= 0) {
							sb.Append($"[{_labels[instruction.LabelIndex]}] : ");
						}
					}

					sb.AppendLine(token.Contents);
				}
			}

			return sb.ToString();
		}

		public string GetRawAsm(out string displayErr, ref ParseResult parseResult) {
			var sb = new StringBuilder();
			sb.AppendLine(@"        .text");
			sb.AppendLine(@"        .intel_syntax noprefix");

			int startInstruction = 0;
			int endInstruction = _instructions.Count;
			if (_beginMarker == null || _endMarker == null)
			{
				// Could not find markers... Analyze everything
				displayErr = "Could not find Coda markers!  Analyzing the entire loop statically (this is probably NOT what you want)";
			}
			else
			{
				if (_beginMarker.LabelIndex == _endMarker.LabelIndex && _beginMarker.TokenIndex < _endMarker.TokenIndex)
				{
					// Easy case
					for (var i = 0; i < _instructions.Count; i++) {
						var instruction = _instructions[i];

						if (_beginMarker.TokenIndex + 5 > instruction.TokenIndex) {
							startInstruction = i;
						}

						if (_endMarker.TokenIndex - 2 > instruction.TokenIndex) {
							endInstruction = i + 3;
						}
					}
					displayErr = "";
				}
				else {
					//displayErr = "Complex control flow is not yet handled. If you are using optimized assembly, try using unoptimized assembly as input instead. Analyzing the entire loop statically (this is probably NOT what you want)";
					displayErr = "";
					ReorderInstructions(ref sb, ref parseResult);
					return sb.ToString();
				}
			}

			int lastLabelIndex = -1;
			string lastLabel = "";
			for (var i = startInstruction; i < endInstruction; i++) {
				var instruction = _instructions[i];
				var token = _tokens[instruction.TokenIndex];
				if (instruction.LabelIndex >= 0) {
					if (lastLabelIndex != instruction.LabelIndex || _labels[instruction.LabelIndex] != lastLabel) {
						sb.AppendLine($"{_labels[instruction.LabelIndex]}:");
						lastLabelIndex = instruction.LabelIndex;
						lastLabel = _labels[instruction.LabelIndex];
					}
				}

				sb.AppendLine($"        {token.Contents}");
			}

			return sb.ToString();
		}

		private void AddInstructionsFromLabel(int labelIndex, int lastLabelIndex, ref StringBuilder sb, ref ParseResult parseResult) {
			var labelName = _labels[labelIndex];
			sb.AppendLine($"{labelName}:");
			bool foundJump = false;
			for (var i = _labelInstructionIndex[labelIndex]; i <= _labelLastInstructionIndex[labelIndex]; ++i) {
				var token = _tokens[_instructions[i].TokenIndex];
				if (token.IsInstruction() && labelIndex != lastLabelIndex && token.Contents.StartsWith("jmp")) {
					// Follow jump
					var m = _jmpRegex.Match(token.Contents);
					if (m.Success) {
						var label = m.Groups[1].Captures[0].Value;
						AddInstructionsFromLabel(_labels.FindIndex(n => n == label), lastLabelIndex, ref sb, ref parseResult);
					}
					foundJump = true;
				} else {
					parseResult.ReorderedInstructionsList.Add(i);
					sb.AppendLine($"        {token.Contents}");
				}
			}

			if (!foundJump && labelIndex != lastLabelIndex) {
				// Fall through to next label
				if (_labelInstructionIndex.ContainsKey(labelIndex + 1)) {
					AddInstructionsFromLabel(labelIndex + 1, lastLabelIndex, ref sb, ref parseResult);
				}
			}
		}

		private void ReorderInstructions(ref StringBuilder sb, ref ParseResult parseResult) {
			parseResult.ReorderedInstructionsList = new List<int>();
			int prevLabelIndex = -1;
			for (var i = 0; i < _instructions.Count; ++i) {
				var labelIndex = _instructions[i].LabelIndex;
				if (!_labelInstructionIndex.ContainsKey(labelIndex)) {
					_labelInstructionIndex[labelIndex] = i;
				}

				if (prevLabelIndex != -1 && prevLabelIndex != labelIndex) {
					_labelLastInstructionIndex[prevLabelIndex] = i - 1;
				}
				prevLabelIndex = labelIndex;
			}
			var beginLabel = BeginMarker.LabelIndex;
			AddInstructionsFromLabel(beginLabel, EndMarker.LabelIndex, ref sb, ref parseResult);
		}

		private bool SeekDbgInfo(int tokenIndex, ref int dbgInfoIndex) {
			while (dbgInfoIndex < _debugInfos.Count - 1 && _debugInfos[dbgInfoIndex + 1].TokenIndex < tokenIndex) {
				dbgInfoIndex++;
			}

			if (dbgInfoIndex < _debugInfos.Count) {
				return _debugInfos[dbgInfoIndex].TokenIndex <= tokenIndex;
			}

			return false;
		}

		internal ParseResult ParseSimple() {
			for (var i = 0; i < _tokens.Count; i++) {
				var token = _tokens[i];
				switch (token.Type) {
					case Token.TokenType.Label:
						_currentLabelIndex = _labels.Count;
						_labels.Add(token.Contents);
						break;
					case Token.TokenType.Directive:
						// Ignore
						break;
					case Token.TokenType.Instruction:
						ParseAssembly(i, token);
						break;
					case Token.TokenType.DebugInfo:
						ParseDebugInfo(i, token);
						break;
					default:
						throw new ApplicationException("Unhandled token type " + token.Type);
				}
			}

			return new ParseResult {
				Tokens = _tokens,
				Labels = _labels,
				Instructions = _instructions,
				DebugInfo = _debugInfos,
				BeginMarker = _beginMarker,
				EndMarker = _endMarker,
				ReorderedInstructionsList = _reorderedInstructionsList,
			};
		}

		private void ParseAssembly(int index, Token token) {
			var jmpMatch = _jmpRegex.Match(token.Contents);
			if (jmpMatch.Success) {
				// Build CFG
				_instructions.Add(new InstructionInfo(index, _currentLabelIndex, _currentDebugInfoIndex));
			} else {
				_instructions.Add(new InstructionInfo(index, _currentLabelIndex, _currentDebugInfoIndex));
			}
		}

		private void ParseDebugInfo(int index, Token token) {
			var dbgInfoMatch = _dbgInfoRegex.Match(token.Contents);
			if (dbgInfoMatch.Success) {
				// Parse debug info
				int lineNumber = -1;
				var fileIndex = int.Parse(dbgInfoMatch.Groups[1].Value);
				var fileName = dbgInfoMatch.Groups[2].Value;
				var code = dbgInfoMatch.Groups[3].Value;
				if (dbgInfoMatch.Groups[4].Length > 0) {
					// Full match with line number
					lineNumber = int.Parse(dbgInfoMatch.Groups[4].Value);
				}

				if (!_fileIndexSet.Contains(fileIndex)) {
					_filenames[fileIndex] = fileName;
					_fileIndexSet.Add(fileIndex);
				}

				_currentDebugInfoIndex = _debugInfos.Count;
				_debugInfos.Add(new DebugInfo(index, fileIndex, lineNumber, code));
				var markerMatch = _markerRegex.Match(code);
				if (markerMatch.Success) {
					string which = markerMatch.Groups[1].Value;
					int instructionIndex = 0;
					while (instructionIndex < _instructions.Count && _instructions[instructionIndex].TokenIndex < index) {
						instructionIndex++;
					}
					if (which == "Begin") {
						_beginMarker = new Marker(Marker.MarkerKind.Begin, _currentLabelIndex, index, instructionIndex);
					} else {
						_endMarker = new Marker(Marker.MarkerKind.End, _currentLabelIndex, index, instructionIndex);
					}
				}
			} else {
				throw new ApplicationException("Unable to parse debug info: " + token.Contents);
			}
		}
	}
}