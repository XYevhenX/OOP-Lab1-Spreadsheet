using System;
using System.Numerics;

namespace SpreadsheetApp.Parsing
{
    public class Evaluator
    {
        private readonly Func<string, object> _getCellValue;

        public Evaluator(Func<string, object> getCellValue = null)
        {
            _getCellValue = getCellValue;
        }

        public bool Evaluate(ASTNode ast)
        {
            if (ast == null) throw new ArgumentNullException(nameof(ast));

            object result = EvaluateNode(ast);

            if (result is bool boolResult)
                return boolResult;

            if (result is BigInteger numResult)
                return numResult != 0;

            throw new Exception("Кінцевий результат виразу не вдалося звести до логічного значення.");
        }

        private object EvaluateNode(ASTNode node)
        {
            if (node is NumberNode numberNode)
                return numberNode.Value;

            if (node is BooleanNode boolNode)
                return boolNode.Value;

            if (node is CellReferenceNode cellNode)
            {
                if (_getCellValue == null)
                    throw new Exception($"Немає доступу до клітинки {cellNode.CellName}");

                return _getCellValue(cellNode.CellName);
            }

            if (node is UnaryOpNode unaryNode)
            {
                object operand = EvaluateNode(unaryNode.Operand);

                if (unaryNode.Operator.ToLower() == "not")
                {
                    return !ToBool(operand);
                }
                throw new Exception($"Невідомий унарний оператор: {unaryNode.Operator}");
            }

            if (node is BinaryOpNode binaryNode)
            {
                object left = EvaluateNode(binaryNode.Left);
                object right = EvaluateNode(binaryNode.Right);
                string op = binaryNode.Operator.ToLower();

                if (op == "and" || op == "or" || op == "eqv")
                {
                    bool lb = ToBool(left);
                    bool rb = ToBool(right);
                    return op switch
                    {
                        "and" => lb && rb,
                        "or" => lb || rb,
                        "eqv" => lb == rb,
                        _ => throw new Exception($"Невідомий логічний оператор: {op}")
                    };
                }

                if (left is BigInteger li && right is BigInteger ri)
                {
                    return op switch
                    {
                        "=" => li == ri,
                        "<>" => li != ri,
                        "<" => li < ri,
                        ">" => li > ri,
                        "<=" => li <= ri,
                        ">=" => li >= ri,
                        _ => throw new Exception($"Невідомий оператор порівняння: {op}")
                    };
                }

                if (op == "=" || op == "<>")
                {
                    bool lb = ToBool(left);
                    bool rb = ToBool(right);
                    return op == "=" ? lb == rb : lb != rb;
                }

                throw new Exception("Несумісні типи операндів для виконання операції.");
            }

            throw new Exception("Невідомий тип вузла синтаксичного дерева.");
        }

        private bool ToBool(object value)
        {
            if (value is bool b) return b;
            if (value is BigInteger num) return num != 0;
            throw new Exception("Неможливо звести операнд до логічного значення.");
        }
    }
}