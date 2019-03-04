using UnityEngine;

namespace TapVoxel.Coda.Editor {
	internal class Node {
		internal const int LineHeight = 11;
		internal const int CycleWidth = 10;
		private readonly int index;
		private readonly int beginCycle;
		private readonly int executionStart;
		private readonly int executionDone;
		private readonly int waiting;
		private readonly int cycleCount;
		private readonly string asm;

		private static GUIStyle startBG = null;
		private static GUIStyle greenBG = null;
		private static GUIStyle blueBG = null;
		private static GUIStyle orangeBG = null;

		public int Index => index;
		public int CycleStart => beginCycle;
		public int ExecutionStart => executionStart;
		public int ExecutionDone => executionDone;
		public int Waiting => waiting;
		public int CycleCount => cycleCount;
		public int EndCycle => beginCycle + cycleCount;
		public string Asm => asm;

		public static GUIStyle OrangeStyle => orangeBG;

		public Node(int index, string asm, int beginCycle, int executionStart, int executionDone, int waiting, int cycleCount) {
			this.index = index;
			this.asm = asm;
			this.beginCycle = beginCycle;
			this.executionStart = executionStart;
			this.executionDone = executionDone;
			this.waiting = waiting;
			this.cycleCount = cycleCount;
		}

		public void Setup() {
		}

		private static Texture2D MakeTex(int width, int height, Color col)
		{
			var pix = new Color[width * height];

			for (int i = 0; i < pix.Length; i++) pix[i] = col;

			var result = new Texture2D(width, height);
			result.SetPixels(pix);
			result.Apply();
			return result;
		}

		public static void BeginDrawing() {
			if (startBG == null) {
				startBG = new GUIStyle(GUI.skin.box);
				startBG.normal.background = MakeTex(1, 1, Color.magenta);
			}

			if (greenBG == null) {
				greenBG = new GUIStyle(GUI.skin.box);
				greenBG.normal.background = MakeTex(1, 1, Color.green);
			}

			if (blueBG == null) {
				blueBG = new GUIStyle(GUI.skin.box);
				blueBG.normal.background = MakeTex(1, 1, Color.blue);
			}

			if (orangeBG == null) {
				orangeBG = new GUIStyle(GUI.skin.box);
				orangeBG.normal.background = MakeTex(1, 1, Color.yellow);
			}
		}

		public void Draw(GUIStyle boxStyle, GUIStyle textStyle) {
			var x = beginCycle * CycleWidth;
			var y = index * LineHeight;
			var width = cycleCount * CycleWidth;
			GUI.Box(new Rect(x, y, CycleWidth, 10), "", startBG);
			var cycle = 1;
			if (executionStart >= 0) {
				if (executionStart > 1) {
					// Draw initial scheduling wait
					GUI.Box(new Rect(x + cycle * CycleWidth, y, (executionStart - cycle) * CycleWidth, 10), "", blueBG);
					cycle = beginCycle + executionStart;
				}
			}

			if (executionDone >= 0) {
				// Draw execution time
				if (executionStart < 0) {
					// Draw initial scheduling wait
					GUI.Box(new Rect(x + cycle * CycleWidth, y, (executionDone - cycle) * CycleWidth, 10), "", blueBG);
					GUI.Box(new Rect(x + executionDone * CycleWidth, y, CycleWidth, 10), "", greenBG);
				} else {
					GUI.Box(new Rect(x + executionStart * CycleWidth, y, ((executionDone - executionStart) + 1) * CycleWidth, 10), "", greenBG);
				}
				cycle = beginCycle + executionDone;
			}

			if (waiting >= 0) {
				// Draw waiting for retire time
				GUI.Box(new Rect(x + waiting * CycleWidth, y, (cycleCount - waiting - 1) * CycleWidth, 10), "", orangeBG);
			}

			// Draw retired
			GUI.Box(new Rect(x + (cycleCount - 1) * CycleWidth, y, 10, 10), "", boxStyle);

			x = beginCycle * CycleWidth + width + 5;
			GUI.Label(new Rect(x, y, 200, 10), asm, textStyle);
		}
	}
}