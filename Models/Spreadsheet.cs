using System;
using System.Collections.Generic;
using SpreadsheetApp.Parsing;

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
                    if (_cells[r, c] == null)
                    {
                        _cells[r, c] = new Cell(GetCellName(r, c));
                    }
                }
            }
        }

        private string GetCellName(int row, int col)
        {
            string colName = "";
            int tempCol = col;
            while (tempCol >= 0)
            {
                colName = (char)('A' + (tempCol % 26)) + colName;
                tempCol = (tempCol / 26) - 1;
            }
            return $"{colName}{row + 1}";
        }

        public Cell GetCellByName(string name)
        {
            for (int r = 0; r < RowCount; r++)
            {
                for (int c = 0; c < ColCount; c++)
                {
                    if (_cells[r, c].Name.ToUpper() == name.ToUpper())
                        return _cells[r, c];
                }
            }
            return null;
        }

        public void AddRow()
        {
            var newCells = new Cell[RowCount + 1, ColCount];
            for (int r = 0; r < RowCount; r++)
                for (int c = 0; c < ColCount; c++)
                    newCells[r, c] = _cells[r, c];

            _cells = newCells;
            RowCount++;
            InitializeCells();
        }

        public void AddColumn()
        {
            var newCells = new Cell[RowCount, ColCount + 1];
            for (int r = 0; r < RowCount; r++)
                for (int c = 0; c < ColCount; c++)
                    newCells[r, c] = _cells[r, c];

            _cells = newCells;
            ColCount++;
            InitializeCells();
        }

        public void RemoveRow()
        {
            if (RowCount <= 1) return;
            var newCells = new Cell[RowCount - 1, ColCount];
            for (int r = 0; r < RowCount - 1; r++)
                for (int c = 0; c < ColCount; c++)
                    newCells[r, c] = _cells[r, c];

            _cells = newCells;
            RowCount--;
        }

        public void RemoveColumn()
        {
            if (ColCount <= 1) return;
            var newCells = new Cell[RowCount, ColCount - 1];
            for (int r = 0; r < RowCount; r++)
                for (int c = 0; c < ColCount - 1; c++)
                    newCells[r, c] = _cells[r, c];

            _cells = newCells;
            ColCount--;
        }
        private HashSet<string> _visitedCells;

        public void EvaluateAll()
        {
            for (int r = 0; r < RowCount; r++)
            {
                for (int c = 0; c < ColCount; c++)
                {
                    var cell = _cells[r, c];
                    cell.ErrorMessage = "";

                    if (string.IsNullOrWhiteSpace(cell.Expression))
                    {
                        cell.Value = false;
                        continue;
                    }

                    try
                    {
                        _visitedCells = new HashSet<string>();
                        cell.Value = EvaluateCellRecursively(cell.Name);
                    }
                    catch (Exception ex)
                    {
                        cell.ErrorMessage = ex.Message;
                        cell.Value = false;
                    }
                }
            }
        }

        private bool EvaluateCellRecursively(string cellName)
        {
            if (_visitedCells.Contains(cellName))
                throw new Exception($"Циклічне посилання: {cellName}");

            var cell = GetCellByName(cellName);
            if (cell == null)
                throw new Exception($"Клітинка {cellName} не існує");

            _visitedCells.Add(cellName);

            if (string.IsNullOrWhiteSpace(cell.Expression))
                return cell.Value;

            var parser = new Parser();
            var evaluator = new Evaluator(GetCellValueForEvaluator);
            var ast = parser.ParseToAST(cell.Expression);

            bool result = evaluator.Evaluate(ast);
            cell.Value = result;
            return result;
        }

        private object GetCellValueForEvaluator(string cellName)
        {
            return EvaluateCellRecursively(cellName);
        }
    }
}