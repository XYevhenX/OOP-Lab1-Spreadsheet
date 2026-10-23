using System;
using System.Collections.Generic;
using SpreadsheetApp.Parsing.Exceptions;

namespace SpreadsheetApp.Parsing
{
    public class Parser
    {
        private int _errorPosition = -1;
        private string _text;
        private int _pos;

        public int GetErrorPosition() => _errorPosition;

        public bool CheckSyntax(string expr)
        {
            try
            {
                ParseToAST(expr);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public ASTNode ParseToAST(string expr)
        {
            _errorPosition = -1;
            if (string.IsNullOrWhiteSpace(expr))
            {
                _errorPosition = 0;
                throw new SyntaxException("Вираз не може бути порожнім", 0);
            }

            _text = expr;
            _pos = 0;

            List<Token> tokens = Tokenize();

            return null;
        }

        private List<Token> Tokenize()
        {
            var tokens = new List<Token>();
            while (_pos < _text.Length)
            {
                char current = _text[_pos];

                if (char.IsWhiteSpace(current))
                {
                    _pos++;
                    continue;
                }

                if (char.IsDigit(current))
                {
                    tokens.Add(ReadNumber());
                    continue;
                }

                if (char.IsLetter(current))
                {
                    tokens.Add(ReadIdentifierOrKeyword());
                    continue;
                }

                switch (current)
                {
                    case '(': tokens.Add(new Token(TokenType.LParen, "(", _pos++)); break;
                    case ')': tokens.Add(new Token(TokenType.RParen, ")", _pos++)); break;
                    case '=': tokens.Add(new Token(TokenType.Equal, "=", _pos++)); break;
                    case '<':
                        if (_pos + 1 < _text.Length && _text[_pos + 1] == '>')
                        {
                            tokens.Add(new Token(TokenType.NotEqual, "<>", _pos));
                            _pos += 2;
                        }
                        else if (_pos + 1 < _text.Length && _text[_pos + 1] == '=')
                        {
                            tokens.Add(new Token(TokenType.LessOrEqual, "<=", _pos));
                            _pos += 2;
                        }
                        else
                        {
                            tokens.Add(new Token(TokenType.Less, "<", _pos++));
                        }
                        break;
                    case '>':
                        if (_pos + 1 < _text.Length && _text[_pos + 1] == '=')
                        {
                            tokens.Add(new Token(TokenType.GreaterOrEqual, ">=", _pos));
                            _pos += 2;
                        }
                        else
                        {
                            tokens.Add(new Token(TokenType.Greater, ">", _pos++));
                        }
                        break;
                    default:
                        _errorPosition = _pos;
                        throw new SyntaxException($"Невідомий символ '{current}'", _pos);
                }
            }
            tokens.Add(new Token(TokenType.EOF, "", _pos));
            return tokens;
        }

        private Token ReadNumber()
        {
            int start = _pos;
            while (_pos < _text.Length && char.IsDigit(_text[_pos])) _pos++;
            return new Token(TokenType.Number, _text.Substring(start, _pos - start), start);
        }

        private Token ReadIdentifierOrKeyword()
        {
            int start = _pos;
            while (_pos < _text.Length && char.IsLetterOrDigit(_text[_pos])) _pos++;
            string value = _text.Substring(start, _pos - start);

            return value.ToLower() switch
            {
                "not" => new Token(TokenType.Not, value, start),
                "and" => new Token(TokenType.And, value, start),
                "or" => new Token(TokenType.Or, value, start),
                "eqv" => new Token(TokenType.Eqv, value, start),
                _ => new Token(TokenType.CellReference, value.ToUpper(), start)
            };
        }
    }
}