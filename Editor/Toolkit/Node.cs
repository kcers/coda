#if UNITY_2018_1_OR_NEWER
using UnityEngine;
#endif

namespace TapVoxel.Coda.Editor.Toolkit
{
	public class Node
	{
		internal const int LineHeight = 11;
		internal const int CycleWidth = 10;
		private readonly int _index;
		private readonly int _beginCycle;
		private readonly int _executionStart;
		private readonly int _executionDone;
		private readonly int _waiting;
		private readonly int _cycleCount;
		private readonly string _asm;

#if UNITY_2018_1_OR_NEWER
		private static GUIStyle _startBg = null;
		private static GUIStyle _greenBg = null;
		private static GUIStyle _blueBg = null;
		private static GUIStyle _orangeBg = null;
#endif

		public int Index => _index;
		public int CycleStart => _beginCycle;
		public int ExecutionStart => _executionStart;
		public int ExecutionDone => _executionDone;
		public int Waiting => _waiting;
		public int CycleCount => _cycleCount;
		public int EndCycle => _beginCycle + _cycleCount;
		public string Asm => _asm;

#if UNITY_2018_1_OR_NEWER
		public static GUIStyle OrangeStyle => _orangeBg;
#endif

		public Node(int index, string asm, int beginCycle, int executionStart, int executionDone, int waiting, int cycleCount)
		{
			_index = index;
			_asm = asm;
			_beginCycle = beginCycle;
			_executionStart = executionStart;
			_executionDone = executionDone;
			_waiting = waiting;
			_cycleCount = cycleCount;
		}

		public void Setup()
		{
		}

#if UNITY_2018_1_OR_NEWER
		private static Texture2D MakeTex(int width, int height, Color col)
		{
			var pix = new Color[width * height];

			for (int i = 0; i < pix.Length; i++) pix[i] = col;

			var result = new Texture2D(width, height);
			result.SetPixels(pix);
			result.Apply();
			return result;
		}

		public static void BeginDrawing()
		{
			if (_startBg == null)
			{
				_startBg = new GUIStyle(GUI.skin.box);
				_startBg.normal.background = MakeTex(1, 1, Color.magenta);
			}

			if (_greenBg == null)
			{
				_greenBg = new GUIStyle(GUI.skin.box);
				_greenBg.normal.background = MakeTex(1, 1, Color.green);
			}

			if (_blueBg == null)
			{
				_blueBg = new GUIStyle(GUI.skin.box);
				_blueBg.normal.background = MakeTex(1, 1, Color.blue);
			}

			if (_orangeBg == null)
			{
				_orangeBg = new GUIStyle(GUI.skin.box);
				_orangeBg.normal.background = MakeTex(1, 1, Color.yellow);
			}
		}

		public void Draw(GUIStyle boxStyle, GUIStyle textStyle)
		{
			var x = _beginCycle * CycleWidth;
			var y = _index * LineHeight;
			var width = _cycleCount * CycleWidth;
			GUI.Box(new Rect(x, y, CycleWidth, 10), "", _startBg);
			var cycle = 1;
			if (_executionStart >= 0)
			{
				if (_executionStart > 1)
				{
					// Draw initial scheduling wait
					GUI.Box(new Rect(x + cycle * CycleWidth, y, (_executionStart - cycle) * CycleWidth, 10), "", _blueBg);
					cycle = _beginCycle + _executionStart;
				}
			}

			if (_executionDone >= 0)
			{
				// Draw execution time
				if (_executionStart < 0)
				{
					// Draw initial scheduling wait
					GUI.Box(new Rect(x + cycle * CycleWidth, y, (_executionDone - cycle) * CycleWidth, 10), "", _blueBg);
					GUI.Box(new Rect(x + _executionDone * CycleWidth, y, CycleWidth, 10), "", _greenBg);
				}
				else
				{
					GUI.Box(new Rect(x + _executionStart * CycleWidth, y, ((_executionDone - _executionStart) + 1) * CycleWidth, 10), "", _greenBg);
				}
				cycle = _beginCycle + _executionDone;
			}

			if (_waiting >= 0)
			{
				// Draw waiting for retire time
				GUI.Box(new Rect(x + _waiting * CycleWidth, y, (_cycleCount - _waiting - 1) * CycleWidth, 10), "", _orangeBg);
			}

			// Draw retired
			GUI.Box(new Rect(x + (_cycleCount - 1) * CycleWidth, y, 10, 10), "", boxStyle);

			x = _beginCycle * CycleWidth + width + 5;
			GUI.Label(new Rect(x, y, 200, 10), _asm, textStyle);
		}
#endif
	}
}
