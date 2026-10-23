using System.Numerics;

namespace SpreadsheetApp.Parsing
{
    public abstract class ASTNode { }

    public class NumberNode : ASTNode
    {
        public BigInteger Value { get; }
        public NumberNode(BigInteger value) { Value = value; }
    }

    public class CellReferenceNode : ASTNode
    {
        public string CellName { get; }
        public CellReferenceNode(string cellName) { CellName = cellName; }
    }

    public class BinaryOpNode : ASTNode
    {
        public ASTNode Left { get; }
        public ASTNode Right { get; }
        public string Operator { get; }

        public BinaryOpNode(ASTNode left, ASTNode right, string op)
        {
            Left = left;
            Right = right;
            Operator = op;
        }
    }

    public class UnaryOpNode : ASTNode
    {
        public ASTNode Operand { get; }
        public string Operator { get; }

        public UnaryOpNode(ASTNode operand, string op)
        {
            Operand = operand;
            Operator = op;
        }
    }
}