using System.Collections.Generic;

namespace SpreadsheetApp.Models
{
    public class CellData
    {
        public string Name { get; set; }
        public string Expression { get; set; }
    }

    public class SpreadsheetData
    {
        public int RowCount { get; set; }
        public int ColCount { get; set; }
        public List<CellData> Cells { get; set; } = new List<CellData>();
    }
}