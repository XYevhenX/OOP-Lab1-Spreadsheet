namespace SpreadsheetApp.Models
{
    public class Cell
    {
        public string Name { get; set; }
        public string Expression { get; set; }
        public bool Value { get; set; }
        public string ErrorMessage { get; set; }

        public Cell(string name)
        {
            Name = name;
            Expression = "";
            Value = false;
            ErrorMessage = "";
        }

        public void UpdateExpression(string expr)
        {
            Expression = expr;
        }

        public bool GetValue()
        {
            return Value;
        }
    }
}