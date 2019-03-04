using System;
using System.Collections.Generic;
using System.Linq;

namespace TapVoxel.Coda.Editor.Toolkit {
	public class CostingResult {
		public List<CycleCosting> CyclesCosting;
		public Dictionary<int, int> InstructionsCosting;
		public int TotalCost;
		public int LongestInstruction;
		public List<KeyValuePair<int, int>> Percentiles;
		public int MostExpensiveInstruction;
		public Dictionary<int, Dictionary<int, int>> LineCosting;
		public Dictionary<int, Dictionary<int, string>> LineCode;
		public List<KeyValuePair<int, int>> LinePercentiles;
	}

	public class CycleCosting {
		public int CycleNumber;
		public List<int> InstructionIndices;
	}

	internal class Costing {
		private List<CycleCosting> _cyclesCosting;
		private Dictionary<int, int> _instructionsCosting = new Dictionary<int, int>();
		private Dictionary<int, int> _histogram = new Dictionary<int, int>();
		private int _totalCost;
		private int _totalCostLine;
		private int _longestCycle;

		internal Costing(int cycleCount) {
			_cyclesCosting = new List<CycleCosting>(cycleCount);
			for (var i = 0; i < cycleCount; ++i) {
				_cyclesCosting.Add(new CycleCosting {CycleNumber = i, InstructionIndices = new List<int>()});
			}
		}

		internal void AddInstruction(int cycle, int instructionIndex, int cycleCount) {
			try {
				_cyclesCosting[cycle].InstructionIndices.Add(instructionIndex);
				_longestCycle = Math.Max(cycleCount, _longestCycle);
			} catch (Exception e) {
				Reporting.ReportError(e.Message);
				throw;
			}
		}

		internal CostingResult Analyze(List<InstructionInfo> instructions, List<int> reorderedInstructionsList, int firstIndex, int instructionCount, List<DebugInfo> debugInfo, Dictionary<int, string> filePaths) {
			_totalCost = 0;
			_instructionsCosting = new Dictionary<int, int>();
			_histogram = new Dictionary<int, int>();
			for (var i = 0; i < _cyclesCosting.Count; ++i) {
				var cost = _cyclesCosting[i];
				foreach (var instructionIndex in cost.InstructionIndices) {
					if (!_instructionsCosting.ContainsKey(instructionIndex)) {
						_instructionsCosting.Add(instructionIndex, 0);
					}

					_instructionsCosting[instructionIndex]++;
					++_totalCost;
				}
			}

			// Cost the instructions in percentile tenths
			var sortedInstructions = _instructionsCosting.OrderBy(kv => kv.Value).ThenBy(kv => kv.Key).ToList();
			var totalCount = _instructionsCosting.Count;
			var percentiles = new List<KeyValuePair<int, int>>(totalCount);
			var mostExpensive = 0;
			var mostExpensiveIndex = -1;
			for (var index = 0; index < totalCount; ++index) {
				var percentile = index * 10 / totalCount; // 0 - 9
				var instrPair = sortedInstructions[index];
				percentiles.Add(new KeyValuePair<int, int>(instrPair.Key, percentile));
				if (instrPair.Value > mostExpensive) {
					mostExpensive = instrPair.Value;
					mostExpensiveIndex = instrPair.Key;
				}
			}

			// Cost the original source lines
			var lineCosting = new Dictionary<int, Dictionary<int, int>>();
			var lineCode = new Dictionary<int, Dictionary<int, string>>();
			_totalCostLine = 0;
			for (var index = firstIndex; index < firstIndex + instructionCount; ++index) {
				var dbgIndex = instructions[reorderedInstructionsList?[index] ?? index].DebugInfoIndex;
				var dbgInfo = debugInfo[dbgIndex];
				int fileIndex = debugInfo[dbgIndex].FileIndex;
				string filePath = filePaths[fileIndex];

				// TODO: Match only user code, but still cost calls into Unity to the user's code which ended up calling it
				ulong hash = (ulong)(dbgInfo.LineNumber + 1) + ((ulong) fileIndex << 32);
				var lineNumber = dbgInfo.LineNumber + 1;
				if (!lineCosting.ContainsKey(fileIndex)) {
					lineCosting[fileIndex] = new Dictionary<int, int>();
				}

				if (!lineCode.ContainsKey(fileIndex)) {
					lineCode[fileIndex] = new Dictionary<int, string>();
				}

				if (!lineCosting[fileIndex].ContainsKey(lineNumber)) {
					lineCosting[fileIndex][lineNumber] = 0;
				}

				if (!lineCode[fileIndex].ContainsKey(lineNumber)) {
					lineCode[fileIndex][lineNumber] = dbgInfo.Code;
				}

				var instructionCost = _instructionsCosting[index - firstIndex];
				lineCosting[fileIndex][lineNumber] += instructionCost;
				_totalCostLine += instructionCost;
			}

			// Cost the lines in percentile tenths
			var totalLineCount = 0;
			foreach (var kd in lineCosting)
			{
				var fileIndex = kd.Key;
				totalLineCount += kd.Value.Keys.Count;
			}
			var linePercentiles = new List<KeyValuePair<int, int>>(totalCount);
			foreach (var kd in lineCosting) {
				var fileIndex = kd.Key;
				// TODO: Handle multiple files
				var sortedLines = kd.Value.OrderBy(kv => kv.Value).ThenBy(kv => kv.Key).ToList();
				for (var dcIndex = 0; dcIndex < sortedLines.Count; ++dcIndex) {
					var dc = sortedLines[dcIndex];
					linePercentiles.Add(new KeyValuePair<int, int>(dc.Key, dcIndex * 10 / totalLineCount));
				}
			}

			return new CostingResult {
				CyclesCosting = _cyclesCosting, InstructionsCosting = _instructionsCosting, TotalCost = _totalCost, LongestInstruction = _longestCycle, Percentiles = percentiles,
				MostExpensiveInstruction = mostExpensiveIndex, LineCosting = lineCosting, LineCode = lineCode, LinePercentiles = linePercentiles
			};
		}
	}
}