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
            {
                return boolResult;
            }

            throw new Exception("Кінцевий результат виразу не є логічним значенням.");
        }

        private object EvaluateNode(ASTNode node)
        {
            if (node is NumberNode numberNode)
                return numberNode.Value;

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
                    if (operand is bool b) return !b;
                    throw new Exception("Оператор NOT застосовується тільки до логічних значень.");
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
                    if (left is bool lb && right is bool rb)
                    {
                        return op switch
                        {
                            "and" => lb && rb,
                            "or" => lb || rb,
                            "eqv" => lb == rb,
                            _ => throw new Exception($"Невідомий логічний оператор: {op}")
                        };
                    }
                    throw new Exception($"Оператор '{op}' очікує логічні операнди.");
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

                if (left is bool lbool && right is bool rbool)
                {
                    return op switch
                    {
                        "=" => lbool == rbool,
                        "<>" => lbool != rbool,
                        _ => throw new Exception($"Оператор '{op}' не підтримується для порівняння логічних значень.")
                    };
                }

                throw new Exception("Несумісні типи операндів для виконання операції.");
            }

            throw new Exception("Невідомий тип вузла синтаксичного дерева.");
        }
    }
}