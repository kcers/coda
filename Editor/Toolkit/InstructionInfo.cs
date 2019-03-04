namespace TapVoxel.Coda.Editor.Toolkit {
	public class InstructionInfo {
		public int TokenIndex { get; }

		public int LabelIndex { get; }

		public int DebugInfoIndex { get; }

		public InstructionInfo(int tokenIndex, int labelIndex, int debugInfoIndex) {
			TokenIndex = tokenIndex;
			LabelIndex = labelIndex;
			DebugInfoIndex = debugInfoIndex;
		}
	}
}