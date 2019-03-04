using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace TapVoxel.Coda
{
	public static class Markers {
		public static int Begin() {
#if UNITY_EDITOR
			var handle = AtomicSafetyHandle.GetTempMemoryHandle();
#endif
			return 0;
		}

		public static int End() {
#if UNITY_EDITOR
			var handle = AtomicSafetyHandle.GetTempUnsafePtrSliceHandle();
#endif
			return 0;
		}
	}
}
