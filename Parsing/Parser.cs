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
        private List<Token> _tokens;
        private int _tokenIndex;

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

            _tokens = Tokenize();
            _tokenIndex = 0;

            if (_tokens.Count == 0 || _tokens[0].Type == TokenType.EOF)
                throw new SyntaxException("Вираз порожній", 0);

            var ast = ParseEqv();

            if (CurrentToken().Type != TokenType.EOF)
            {
                _errorPosition = CurrentToken().Position;
                throw new SyntaxException($"Неочікуваний символ: {CurrentToken().Value}", _errorPosition);
            }

            return ast;
        }

        private Token CurrentToken() => _tokenIndex < _tokens.Count ? _tokens[_tokenIndex] : _tokens[_tokens.Count - 1];

        private void Advance() { if (_tokenIndex < _tokens.Count) _tokenIndex++; }

        private void Match(TokenType type)
        {
            if (CurrentToken().Type == type) Advance();
            else
            {
                _errorPosition = CurrentToken().Position;
                throw new SyntaxException($"Очікувався токен {type}", _errorPosition);
            }
        }

        private ASTNode ParseEqv()
        {
            var node = ParseOr();
            while (CurrentToken().Type == TokenType.Eqv)
            {
                var op = CurrentToken().Value;
                Advance();
                var right = ParseOr();
                node = new BinaryOpNode(node, right, op);
            }
            return node;
        }

        private ASTNode ParseOr()
        {
            var node = ParseAnd();
            while (CurrentToken().Type == TokenType.Or)
            {
                var op = CurrentToken().Value;
                Advance();
                var right = ParseAnd();
                node = new BinaryOpNode(node, right, op);
            }
            return node;
        }

        private ASTNode ParseAnd()
        {
            var node = ParseComparison();
            while (CurrentToken().Type == TokenType.And)
            {
                var op = CurrentToken().Value;
                Advance();
                var right = ParseComparison();
                node = new BinaryOpNode(node, right, op);
            }
            return node;
        }

        private ASTNode ParseComparison()
        {
            var node = ParseUnary();
            while (CurrentToken().Type == TokenType.Equal || CurrentToken().Type == TokenType.NotEqual ||
                   CurrentToken().Type == TokenType.Less || CurrentToken().Type == TokenType.LessOrEqual ||
                   CurrentToken().Type == TokenType.Greater || CurrentToken().Type == TokenType.GreaterOrEqual)
            {
                var op = CurrentToken().Value;
                Advance();
                var right = ParseUnary();
                node = new BinaryOpNode(node, right, op);
            }
            return node;
        }

        private ASTNode ParseUnary()
        {
            if (CurrentToken().Type == TokenType.Not)
            {
                var op = CurrentToken().Value;
                Advance();
                var operand = ParseUnary();
                return new UnaryOpNode(operand, op);
            }
            return ParsePrimary();
        }

        private ASTNode ParsePrimary()
        {
            if (CurrentToken().Type == TokenType.Number)
            {
                var val = System.Numerics.BigInteger.Parse(CurrentToken().Value);
                Advance();
                return new NumberNode(val);
            }
            if (CurrentToken().Type == TokenType.CellReference)
            {
                var name = CurrentToken().Value;
                Advance();
                return new CellReferenceNode(name);
            }
            if (CurrentToken().Type == TokenType.LParen)
            {
                Advance();
                var node = ParseEqv();
                Match(TokenType.RParen);
                return node;
            }

            _errorPosition = CurrentToken().Position;
            throw new SyntaxException($"Неочікуваний токен: {CurrentToken().Value}", _errorPosition);
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