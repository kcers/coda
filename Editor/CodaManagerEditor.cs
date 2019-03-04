using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;
#if UNITY_2018_1_OR_NEWER
using UnityEditor.PackageManager;
using UnityEngine.Experimental.Rendering;
#endif
#if UNITY_2018_3_OR_NEWER
using UnityEngine.Networking;
#endif
using TapVoxel.Coda.Editor.Toolkit;

namespace TapVoxel.Coda.Editor {
	public class CodaManagerEditor : EditorWindow {
		private Analysis _lastAnalysis;
		private const float UpperPanelHeight = 75.0f;
		private Rect upperPanel;
		private Rect analysisPanel;
		private Vector2 scroll = Vector2.zero;
		private Vector2 scrollTimeline = Vector2.zero;

		private const string AsmPlaceholderText = @"Please paste the enhanced disassembly from the Burst Compiler Inspector...";

		private string analysisText;
		private int cpuSelected = 11;
		private readonly string[] _cpuOptions = new string[]
		{
			"broadwell",
			"btver2",
			"cannonlake",
			"core-avx-i",
			"core-avx2",
			"core2",
			"corei7",
			"corei7-avx",
			"geode",
			"goldmont",
			"goldmont-plus",
			"haswell",
			"icelake-client",
			"icelake-server",
			"ivybridge",
			"knl",
			"knm",
			"nehalem",
			"penryn",
			"sandybridge",
			"silvermont",
			"skx",
			"skylake",
			"skylake-avx512",
			"slm",
			"tremont",
			"westmere",
			"x86-64",
			"yonah",
			"znver1",
		};

		private bool buttonProfileReportEnabled = false;
		private bool buttonDetailedReportEnabled = false;


		private List<Node> nodes = new List<Node>();
		private GUIStyle boxStyle = null;
		private GUIStyle textStyle = null;
		private Texture2D redTex = null;
		private int mouseCycle = 0;
		private int timelineMouseX = 0;

#region Coda Menu Items

		[MenuItem("Window/" + Editor.TVConst.ToolMenu + "/Coda/Show Coda Manager... %#c", false, 40)]
		public static void ShowCodaManager() {
			var manager = EditorWindow.GetWindow<TapVoxel.Coda.Editor.CodaManagerEditor>(false, "Coda Manager");
			manager.Show();
		}

#endregion

		private void OnEnable() {
			textStyle = new GUIStyle {fontSize = 6};
			redTex = MakeTex(1, 1, Color.red);
#if ANALYZE_LLVM_IR
			ParseAnalysis();
#endif
			this.wantsMouseMove = true;
		}

		public Texture2D MakeTex(int width, int height, Color col)
		{
			var pix = new Color[width * height];

			for (int i = 0; i < pix.Length; i++) pix[i] = col;

			var result = new Texture2D(width, height);
			result.SetPixels(pix);
			result.Apply();
			return result;
		}

#region On GUI

		private void OnGUI() {
			if (boxStyle == null) {
				boxStyle = new GUIStyle(GUI.skin.box);
				boxStyle.normal.background = redTex;
			}
			//GUILayout.Label("Base Settings", EditorStyles.boldLabel);
			ProcessEvents(Event.current);

			DrawButtonPanel();

			DrawAnalysisPanel();

			if (GUI.changed) Repaint();
		}

		#endregion

		private void ProcessEvents(Event e) {
			switch (e.rawType) {
				case EventType.MouseMove: {
					//Debug.Log(e.mousePosition.ToString());
					if (e.mousePosition.y >= UpperPanelHeight + 30 && e.mousePosition.y < UpperPanelHeight + 30 + 200) {
						var beginCycle = (int) (scrollTimeline.x / Node.CycleWidth);
						var beginCycleStartX = beginCycle * Node.CycleWidth;
						int cycle = (int) ((e.mousePosition.x + beginCycleStartX) / Node.CycleWidth);
						int ctrlMouseY = (int)(e.mousePosition.y - UpperPanelHeight - 30);
						mouseCycle = Convert.ToInt32(Math.Round((e.mousePosition.x + scrollTimeline.x) / Node.CycleWidth));
						timelineMouseX = (int)(e.mousePosition.x + scrollTimeline.x);
						//Debug.Log($"scrollX: {scrollTimeline.x} scrollBeginCycle: {beginCycle} beginCycleX:{beginCycleStartX}, mouseX: {e.mousePosition.x} mouseCycle:{mouseCycle}");
						//GUI.Label(new Rect(10, 10, 50, 15), $"cycle: {cycle}");
						//Handles.DrawLine(new Vector3(drawX, drawY1), new Vector3(drawX, drawY2));
						//Debug.Log($"cycle: {cycle}, drawX:{drawX}, drawY1:{drawY1}, drawY2:{drawY2}");
					}

					//GUI.Label(new Rect(position.width / 2, 0, 80, 15), $"cycle {cycle}");
					break;
				}
				case EventType.MouseDown: {
					break;
				}
			}
		}

		private void DrawButtonPanel() {
			upperPanel = new Rect(0, 0, position.width, UpperPanelHeight);

			GUILayout.BeginArea(upperPanel);
			GUILayout.Label("Upper Panel");
			cpuSelected = EditorGUILayout.Popup("CPU", cpuSelected, _cpuOptions);
			if (GUILayout.Button("Paste Assembly Code and Analyze"))
			{
				var start = DateTime.Now;
				PasteAndAnalyze();
				if ((DateTime.Now - start).Milliseconds > 100)
				{
					Debug.Log("PasteAndAnalyze slow...");
				}
			}
			GUILayout.EndArea();
		}

		private void DrawAnalysisPanel() {
			analysisPanel = new Rect(0, UpperPanelHeight, position.width, position.height - UpperPanelHeight);

			GUILayout.BeginArea(analysisPanel);
			GUILayout.Label("Analysis:");
			GUILayout.BeginArea(new Rect(0, 30, position.width, 200));
			//scrollTimeline = EditorGUILayout.BeginScrollView(scrollTimeline, true, true);
			int timelineWidth = 200;
			if (nodes.Count > 0) {
				timelineWidth = Math.Max(timelineWidth, nodes[nodes.Count - 1].EndCycle * 10 + 150);
			}
			scrollTimeline = GUI.BeginScrollView(new Rect(0, 0, position.width, 200), scrollTimeline, new Rect(0, 0, timelineWidth, nodes.Count * Node.LineHeight), true, true);
			var start = DateTime.Now;
			DrawTimeline();
			if ((DateTime.Now - start).Milliseconds > 100)
			{
				Debug.Log("DrawTimeline slow...");
			}
			//EditorGUILayout.EndScrollView();
			GUI.EndScrollView();
			GUILayout.EndArea();
			EditorGUILayout.Space();
			GUILayout.BeginArea(new Rect(0, 40 + 200, position.width, position.height - UpperPanelHeight - 40 - 200));
			GUILayout.Label("Code Analysis Results:");
			/*scroll = EditorGUILayout.BeginScrollView(scroll);
			start = DateTime.Now;
			//var text = EditorGUILayout.TextArea(analysisText, GUILayout.ExpandHeight(true));
			if ((DateTime.Now - start).Milliseconds > 100)
			{
				Debug.Log("TextArea slow...");
			}
			EditorGUILayout.EndScrollView();*/
			//GUI.enabled = nodes.Count > 0;
			GUI.enabled = buttonProfileReportEnabled;
			if (GUILayout.Button("Show profile analysis"))
			{
				if (_lastAnalysis != null)
				{
					var reportPrinter = new ProfileReport(_lastAnalysis);
					if (!reportPrinter.PrintReport(out var err))
					{
						Reporting.ReportError(err);
					}
				}
			}
			GUI.enabled = buttonDetailedReportEnabled;
			if (GUILayout.Button("Show detailed analysis")) {
				if (_lastAnalysis != null)
				{
					var reportPrinter = new DetailedReport(_lastAnalysis);
					if (!reportPrinter.PrintReport(out var err))
					{
						Reporting.ReportError(err);
					}
				}
			}
			GUI.enabled = false;
			GUILayout.EndArea();
			GUILayout.EndArea();
		}

		private void DrawTimeline() {
			Node.BeginDrawing();
			var width = Math.Max(position.width, 40);
			//GUI.Box(new Rect(0, 0, position.width, nodes.Count * Node.LineHeight), "Timeline");
			int firstIndex = (int)(scrollTimeline.y / Node.LineHeight);
			int lastIndex = firstIndex + (200 / Node.LineHeight) + 1;
			int nodesDrawn = 0;
			foreach (var node in nodes) {
				if (node.Index >= firstIndex && node.Index <= lastIndex) {
					node.Draw(boxStyle, textStyle);
					++nodesDrawn;
				}
			}
			//Debug.Log($"Nodes drawn: {nodesDrawn}");

			var beginCycle = (int)(scrollTimeline.x / Node.CycleWidth);
			var beginCycleX = 0;//beginCycle * Node.CycleWidth - scrollTimeline.x;
			//Debug.Log($"scrollX: {scrollTimeline.x} scrollBeginCycle: {beginCycle} beginCycleX:{beginCycleX}, firstX:{(float)beginCycle * Node.CycleWidth + beginCycleX}");
			//GUI.Box(new Rect(0, 0, 1, nodes.Count * Node.LineHeight), "");
			//GUI.Box(new Rect(Node.CycleWidth, 0, 1, nodes.Count * Node.LineHeight), "");
			//Handles.DrawLine(new Vector3((float)(beginCycle + 2) * Node.CycleWidth + beginCycleX, scrollTimeline.y + 10), new Vector3((float)(beginCycle + 2) * Node.CycleWidth + beginCycleX, scrollTimeline.y + 190));
			for (int i = beginCycle; i < beginCycle + (position.width / Node.CycleWidth) + 1; ++i) {
				//GUI.Box(new Rect(i * Node.CycleWidth + beginCycleX, scrollTimeline.y + 150, 1, 50), "");
				Handles.DrawLine(new Vector3((float)i * Node.CycleWidth + beginCycleX, scrollTimeline.y + 180), new Vector3((float)i * Node.CycleWidth + beginCycleX, scrollTimeline.y + 190));
			}

			var drawX = mouseCycle * Node.CycleWidth;
			var drawY1 = scrollTimeline.y;
			var drawY2 = drawY1 + 200;
			Handles.DrawLine(new Vector3(drawX, drawY1), new Vector3(drawX, drawY2));
			GUI.Label(new Rect(drawX, scrollTimeline.y + 160, 80, 25), $"cycle: {mouseCycle}", Node.OrangeStyle);

			//GUI.Box(new Rect(10, 10, 30, 10), "");
		}

		private void PasteAndAnalyze() {
			string input = EditorGUIUtility.systemCopyBuffer;
			try
			{
				_lastAnalysis = Analysis.Analyze(_cpuOptions[cpuSelected], input);
				if (_lastAnalysis.Successful) {
					analysisText = _lastAnalysis.DetailedText;
					ParseAnalysis();
					buttonProfileReportEnabled = true;
					buttonDetailedReportEnabled = true;
				}
				else {
					buttonProfileReportEnabled = false;
					buttonDetailedReportEnabled = false;
					Reporting.ReportError(_lastAnalysis.DetailedText);
				}
			}
			catch (Exception ex)
			{
				buttonProfileReportEnabled = false;
				buttonDetailedReportEnabled = false;
				Reporting.ReportError(ex.Message);
				Debug.LogException(ex);
			}
		}

		private void ParseAnalysis()
		{
			nodes = new List<Node>();
			try
			{
				var timelineRegex = new Regex("^Timeline view\\:", RegexOptions.Compiled | RegexOptions.Multiline);
				var timelineItemRegex = new Regex("^\\[\\d+,(\\d+)\\]", RegexOptions.Compiled | RegexOptions.Multiline);
				var digit = new Regex("\\d+", RegexOptions.Compiled);
				var match = timelineRegex.Match(analysisText);
				if (match.Success)
				{
					var matchIndex = match.Index;
					var nl = analysisText.IndexOf('\n', matchIndex);
					var n2 = analysisText.IndexOf('\n', nl + 1);
					var line = analysisText.Substring(nl + 1, n2 - nl - 1);
					var maxLen = 0;
					var firstDigit = 80;
					if (!line.StartsWith("Index"))
					{
						maxLen = line.Length - 1;
						var digitMatch1 = digit.Match(line);
						if (digitMatch1.Success)
						{
							firstDigit = Math.Min(firstDigit, digitMatch1.Index);
						}
						nl = n2;
						n2 = analysisText.IndexOf('\n', nl + 1);
						line = analysisText.Substring(nl + 1, n2 - nl - 1);
					}
					maxLen = Math.Max(maxLen, line.Length - 1);
					var digitMatch = digit.Match(line);
					if (digitMatch.Success)
					{
						firstDigit = Math.Min(firstDigit, digitMatch.Index);
					}
					var itemMatch = timelineItemRegex.Match(analysisText, matchIndex);
					while (itemMatch.Success)
					{
						var eolPos = analysisText.IndexOf('\n', itemMatch.Index);
						var itemLine = analysisText.Substring(itemMatch.Index + firstDigit, maxLen - firstDigit);
						var asmLine = analysisText.Substring(itemMatch.Index + maxLen + 1, eolPos - itemMatch.Index - maxLen - 2);
						var instIndexStr = itemMatch.Groups[1].Captures[0].Value;
						var instIndex = int.Parse(instIndexStr);
						var startCycle = itemLine.IndexOf("D");
						var executionStart = itemLine.IndexOf("e") - startCycle;
						var executionDone = itemLine.IndexOf("E") - startCycle;
						var waiting = itemLine.IndexOf("-") - startCycle;
						var endCycle = itemLine.IndexOf("R");
						nodes.Add(new Node(instIndex, asmLine, startCycle, executionStart, executionDone, waiting, endCycle != -1 ? endCycle - startCycle + 1 : 1));
						itemMatch = timelineItemRegex.Match(analysisText, eolPos + 1);
					}
				}
				else
				{
					EditorUtility.DisplayDialog("Error", "Could not find timeline information", "OK");
				}
			}
			catch (Exception e)
			{
				EditorUtility.DisplayDialog("Error", e.Message, "OK");
				Debug.LogException(e);
			}
		}
	}

	public static class StringExtensions {
		public static string Replace(this string s, int index, int length, string replacement) {
			var builder = new StringBuilder();
			builder.Append(s.Substring(0, index));
			builder.Append(replacement);
			builder.Append(s.Substring(index + length));
			return builder.ToString();
		}
	}
}
