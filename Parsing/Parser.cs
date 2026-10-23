using System;

namespace SpreadsheetApp.Parsing
{
    public class Parser
    {
        private int _errorPosition = -1;

        public int GetErrorPosition()
        {
            return _errorPosition;
        }

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
                throw new ArgumentException("Вираз не може бути порожнім");
            }

            return null; 
        }
    }
}