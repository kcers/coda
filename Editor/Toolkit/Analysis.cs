using System.Linq;

namespace TapVoxel.Coda.Editor.Toolkit {
	public class Analysis {
		public static Analysis Analyze(string cpuOption, string input) {
			var lexer = new Lexer(input);
			lexer.LexSimple();

			var parser = new Parser(lexer.Tokens);
			var parseResult = parser.ParseSimple();

			var asm = parser.GetRawAsm(out var asmErr, ref parseResult);
			if (asmErr != "") {
				return new Analysis(false, lexer, parser, "Error during asm parsing: " + asmErr + "\r\n\r\nTruncated assembly below:\r\n" + string.Join("\r\n", asm.Split('\n').Take(10)), asmErr, null);
			}

			asm = LLVMInterop.Sanitize(asm, out var err);
			if (!string.IsNullOrEmpty(err)) {
				return new Analysis(false, lexer, parser, "Error during asm sanitization: " + err + "\r\n\r\nTruncated Assembly below:\r\n" + string.Join("\r\n", asm.Split('\n').Take(10)), err, null);
			}

			if (!LLVMInterop.ExecuteCommand("llvm-mca.exe", $"-output-asm-variant=1 -all-views -mtriple=x86_64-linux-unknown -mcpu={cpuOption} -iterations=1 -timeline-max-cycles=3000", asm,
				out var stdout, out var stderr)) {
				return new Analysis(false, lexer, parser, "Error during asm analysis: " + stderr, stderr, null);
			}

			var mcaParser = new MCAParser(parseResult, stdout);
			var mcaResult = mcaParser.Analyze();

			return new Analysis(true, lexer, parser, stderr + "\r\n\r\n" + stdout, stderr, mcaResult);
			//return string.Join(" <<< ", lexer.Tokens.Select(token => token.Contents));
			//return parser.ToString();
		}

		public bool Successful { get; }
		public string DetailedText { get; }
		public Lexer Lexer { get; }
		public Parser Parser { get; }
		public string DisplayErr { get; }
		public MCAResult Result { get; }

		public Analysis(bool successful, Lexer lexer, Parser parser, string detailedText, string displayErr, MCAResult result) {
			Successful = successful;
			DetailedText = detailedText;
			Lexer = lexer;
			Parser = parser;
			DisplayErr = displayErr;
			Result = result;
		}
	}
}