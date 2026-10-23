using System;
using System.Numerics;

namespace SpreadsheetApp.Parsing
{
    public class Evaluator
    {
        public bool Evaluate(ASTNode ast)
        {
            if (ast == null) return false;

            object result = EvaluateNode(ast);

            if (result is bool boolResult)
            {
                return boolResult;
            }

            throw new Exception("Кінцевий результат виразу не є логічним значенням.");
        }

        private object EvaluateNode(ASTNode node)
        {
            return false;
        }
    }
}