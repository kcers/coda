namespace TapVoxel.Coda.Editor.Toolkit {
	public class Marker {
		public enum MarkerKind {
			Begin,
			End,
		}

		public int LabelIndex { get; }
		public int TokenIndex { get; }
		public int InstructionIndex { get; }
		public MarkerKind Kind { get; }

		public Marker(MarkerKind kind, int labelIndex, int tokenIndex, int instructionIndex) {
			Kind = kind;
			LabelIndex = labelIndex;
			TokenIndex = tokenIndex;
			InstructionIndex = instructionIndex;
		}
	}
}