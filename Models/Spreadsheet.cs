namespace SpreadsheetApp.Models
{
    public class Spreadsheet
    {
        public int RowCount { get; private set; }
        public int ColCount { get; private set; }

        private Cell[,] _cells;

        public Spreadsheet(int rows, int cols)
        {
            RowCount = rows;
            ColCount = cols;
            _cells = new Cell[rows, cols];
            InitializeCells();
        }

        private void InitializeCells()
        {
            for (int r = 0; r < RowCount; r++)
            {
                for (int c = 0; c < ColCount; c++)
                {               
                    string name = $"{(char)('A' + c)}{r + 1}";
                    _cells[r, c] = new Cell(name);
                }
            }
        }
        public void EvaluateAll() { }
        public void AddRow() { }
        public void AddColumn() { }
        public void RemoveRow() { }
        public void RemoveColumn() { }
    }
}