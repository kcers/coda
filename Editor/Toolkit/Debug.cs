namespace TapVoxel.Coda.Editor.Toolkit {
	public class DebugInfo {
		public int TokenIndex { get; }
		public int FileIndex { get; }
		public int LineNumber { get; }
		public string Code { get; }

		public DebugInfo(int tokenIndex, int fileIndex, int lineNumber, string code) {
			TokenIndex = tokenIndex;
			FileIndex = fileIndex;
			LineNumber = lineNumber;
			Code = code;
		}
	}
}