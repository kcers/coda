using System;
using System.Collections.Generic;
using System.Text;

namespace TapVoxel.Coda.Editor.Toolkit {
	public class Token {
		public enum TokenType {
			Label,
			Directive,
			Instruction,
			Symbol,
			Identifier,
			Comma,
			Opcode,
			TypeQualifier,
			ConstantLiteral,
			StringLiteral,
			NumberLiteral,
			Offset,
			DebugInfo,
			Parenthetical,
			Minus,
		}

		private TokenType _tokenType;
		private string _contents;
		private int _line;
		private int _column;

		public TokenType Type => _tokenType;
		public string Contents => _contents;
		public int Line => _line;
		public int Column => _column;

		public Token(TokenType tokenType) {
			_tokenType = tokenType;
		}

		public void SetContents(string contents) {
			_contents = contents;
		}

		public void SetPosition(int line, int column) {
			_line = line;
			_column = column;
		}

		public bool IsLabel() {
			return _tokenType == TokenType.Label;
		}

		public bool IsDirective() {
			return _tokenType == TokenType.Directive;
		}

		public bool IsInstruction() {
			return _tokenType == TokenType.Instruction;
		}

		public bool IsSymbol() {
			return _tokenType == TokenType.Symbol;
		}
	}

	public class Lexer {
		private List<Token> _tokens = new List<Token>();
		private string _input;
		private int _inputLength;
		private int _pos;
		private int _column;
		private int _line;
		private StringBuilder _buffer = new StringBuilder();
		private int _spanLine;
		private int _spanColumn;

		public List<Token> Tokens => _tokens;

		public Lexer(string input) {
			_input = input;
			_inputLength = input.Length;
			//lexImpl();
		}

		private void Err(string msg) {
			throw new ApplicationException(msg + " at " + PositionAsString());
		}

		private void Consume() {
			_buffer.Append(_input[_pos++]);
			_column++;
		}

		private void Discard() {
			_pos++;
			_column++;
		}

		private void PushToken(Token.TokenType tokenType) {
			var token = new Token(tokenType);
			token.SetContents(_buffer.ToString());
			token.SetPosition(_spanLine + 1, _spanColumn + 1);
			ClearBuffer();
			_tokens.Add(token);
		}

		private void ClearBuffer() {
			_buffer.Clear();
		}

		private string PositionAsString() {
			return $"{_line + 1}:{_column + 1}";
		}

		private void Expect(char expected) {
			if (At() != expected) {
				Err($"Expected {expected} but found character {At()} at {PositionAsString()}");
			}
		}

		private void CheckUnexpectedEof() {
			if (IsEof()) {
				Err("Unexpected EOF");
			}
		}

		private void StartSpan() {
			_spanLine = _line;
			_spanColumn = _column;
		}

		internal void LexSimple() {
			while (_pos < _inputLength) {
				var ch = _input[_pos];
				switch (ch) {
					case '.': {
						if (_column == 0) {
							RecognizeLabel();
						} else {
							// It's a directive
							Consume();
							ch = At();
							while (!IsEol(ch)) {
								Consume();
								ch = At();
							}

							PushToken(Token.TokenType.Directive);
						}

						break;
					}
					case ' ':
					case '\t': {
						Discard();
						break;
					}
					case '"': {
						if (_column == 0) {
							Consume();
							RecognizeStringLabel();
							break;
						}

						Err("Unexpected string literal");
						break;
					}
					case '\r':
						Discard();
						break;
					case '\n': {
						Discard();
						_line++;
						_column = 0;
						break;
					}
					case '<': {
						if (_column == 0) {
							RecognizeDebugInfo();
							break;
						}

						Err("Unexpected character <");
						break;
					}
					default: {
						if (IsIdentifier(ch) && _column == 0) {
							RecognizeLabel();
						} else if (IsIdentifier(ch)) {
							// It's a line of assembly code
							Consume();
							ch = At();
							while (!IsEol(ch)) {
								Consume();
								ch = At();
							}

							PushToken(Token.TokenType.Instruction);
						} else {
							Err("Unexpected character " + ch);
						}

						break;
					}
				}
			}

#if OLD
				if (ch == '.') {
					consume();
					while (ch != ':')
					{
						consume();
						ch = at();
					}

					discard();
					pushToken(new Token(Token.TokenType.Label));
				}
				else if (isIdentifier(ch)) {
					// Must be a label
					while (ch != ':') {
						consume();
					}

					discard();
					pushToken(new Token(Token.TokenType.Label));
				} else if (isWhitespace(ch)) {
					while (isWhitespace(at())) {
						discard();
					}

					if (at() == '.') {
						// It's a directive
						consume();
						ch = at();
						while (!isEOL(ch)) {
							consume();
							ch = at();
						}

						pushToken(new Token(Token.TokenType.Directive));
					} else {
						// It's a line of assembly code
						consume();
						ch = at();
						while (!isEOL(ch)) {
							consume();
							ch = at();
						}

						pushToken(new Token(Token.TokenType.Assembly));
					}
				} else if (ch == '"' && column == 0) {
					// It's a label
					discard();
					ch = at();
					while (ch != '"')
					{
						consume();
						ch = at();
					}

					discard(); // "
					expect(':');
					discard();

					pushToken(new Token(Token.TokenType.Label));
				}
				else if (ch == '\r') {
					discard();
				} else if (isEOL(ch)) {
					discard();
					line++;
					column = 0;
				} else {
					err("Unexpected character " + ch);
				}
			}
#endif
		}

		private void LexImpl() {
			while (_pos < _inputLength) {
				var ch = _input[_pos];
				switch (ch) {
					case '.': {
						if (_column == 0) {
							StartSpan();
							Consume();
							RecognizeLabel();
						} else {
							StartSpan();
							Consume();
							RecognizeSymbol();
						}

						break;
					}
					case '\r': {
						Discard();
						break;
					}
					case '\n': {
						Consume();
						ClearBuffer();
						_line++;
						_column = 0;
						break;
					}
					case ' ': {
						// Whitespace outside token
						Discard();
						break;
					}
					case '"': {
						StartSpan();
						if (_column == 0) {
							Consume();
							RecognizeStringLabel();
							continue;
						}

						Consume();
						RecognizeStringLiteral();
						break;
					}
					case ',': {
						StartSpan();
						Consume();
						RecognizeComma();
						break;
					}
					case '@': {
						StartSpan();
						Consume();
						RecognizeTypeQualifier();
						break;
					}
					case '<': {
						if (_column == 0) {
							StartSpan();
							Consume();
							RecognizeDebugInfo();
							continue;
						}

						Err("Unexpected <");
						break;
					}
					case '[': {
						StartSpan();
						Consume();
						RecognizeOffset();
						break;
					}
					case '-': {
						StartSpan();
						Consume();
						RecognizeMinus();
						break;
					}
					case '(': {
						StartSpan();
						Consume();
						RecognizeParenthetical();
						break;
					}
					default: {
						if (IsIdentifier(ch)) {
							StartSpan();
							var isLabel = _column == 0;
							Consume();
							if (!isLabel) {
								RecognizeIdentifier();
							} else {
								RecognizeLabel();
							}

							continue;
						}

						Err($"Unexpected character {ch}");
						break;
					}
				}
			}
		}

		private void RecognizeLabel() {
			var ch = At();
			while (IsIdentifier(ch) || ch == '.') {
				CheckUnexpectedEof();
				Consume();
				ch = At();
			}

			Expect(':');
			Discard();

			PushToken(Token.TokenType.Label);
		}

		private void RecognizeStringLabel() {
			LexStringLiteral();
			Expect(':');
			Discard();

			PushToken(Token.TokenType.Label);
		}

		private void RecognizeSymbol() {
			while (IsIdentifier(At())) {
				CheckUnexpectedEof();
				Consume();
			}

			PushToken(Token.TokenType.Symbol);
		}

		private void RecognizeIdentifier() {
			while (IsIdentifier(At())) {
				CheckUnexpectedEof();
				Consume();
			}

			PushToken(Token.TokenType.Identifier);
		}

		private void RecognizeTypeQualifier() {
			while (IsIdentifier(At())) {
				CheckUnexpectedEof();
				Consume();
			}

			PushToken(Token.TokenType.TypeQualifier);
		}

		private void RecognizeStringLiteral() {
			LexStringLiteral();

			PushToken(Token.TokenType.StringLiteral);
		}

		private void RecognizeDebugInfo() {
			var ch = At();
			while (!IsEol(ch) && !IsEof()) {
				if (ch != '\r') {
					Consume();
				} else {
					Discard();
				}

				ch = At();
			}

			PushToken(Token.TokenType.DebugInfo);
		}

		private void RecognizeOffset() {
			while (At() != ']' && !IsEof()) {
				Consume();
			}

			Expect(']');
			Discard();

			PushToken(Token.TokenType.Offset);
		}

		private void RecognizeParenthetical() {
			while (At() != ')' && !IsEof()) {
				Consume();
			}

			Expect(')');
			Discard();

			PushToken(Token.TokenType.Parenthetical);
		}

		private void RecognizeComma() {
			PushToken(Token.TokenType.Comma);
		}

		private void RecognizeMinus() {
			PushToken(Token.TokenType.Minus);
		}

		private void LexStringLiteral() {
			while (!IsQuote(At())) {
				CheckUnexpectedEof();
				Consume();
			}

			Discard(); // Trailing quote
		}

		private static bool IsQuote(char ch) {
			return ch == '"';
		}

		private static bool IsIdentifier(char ch) {
			return char.IsLetterOrDigit(ch) || ch == '_';
		}

		private bool IsEof() {
			return _pos >= _inputLength;
		}

		private static bool IsEol(char ch) {
			return ch == '\n' || ch == '\r';
		}

		private bool IsWhitespace(char ch) {
			return ch == ' ' || ch == '\t';
		}

		private char At() {
			return _input[_pos];
		}
	}
}