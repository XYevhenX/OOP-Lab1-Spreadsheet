using Microsoft.Maui.Controls;
using SpreadsheetApp.ViewModels;
using System;

namespace SpreadsheetApp
{
    public partial class MainPage : ContentPage
    {
        private MainViewModel _viewModel;

        public MainPage()
        {
            InitializeComponent();
            _viewModel = (MainViewModel)BindingContext;

            _viewModel.OnGridChanged += RenderGrid;

            RenderGrid();
        }

        private void RenderGrid()
        {
            SpreadsheetGrid.Children.Clear();
            SpreadsheetGrid.RowDefinitions.Clear();
            SpreadsheetGrid.ColumnDefinitions.Clear();

            var spreadsheet = _viewModel.Spreadsheet;
            int rows = spreadsheet.RowCount;
            int cols = spreadsheet.ColCount;

            for (int r = 0; r <= rows; r++) SpreadsheetGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (int c = 0; c <= cols; c++) SpreadsheetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = 120 });

            for (int c = 0; c < cols; c++)
            {
                string colName = GetColumnName(c);
                var label = new Label { Text = colName, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center };
                SpreadsheetGrid.Add(label, c + 1, 0);
            }

            for (int r = 0; r < rows; r++)
            {
                var label = new Label { Text = (r + 1).ToString(), FontAttributes = FontAttributes.Bold, VerticalTextAlignment = TextAlignment.Center };
                SpreadsheetGrid.Add(label, 0, r + 1);
            }

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var cell = spreadsheet.GetCellByName($"{GetColumnName(c)}{r + 1}");
                    if (cell == null) continue;

                    View cellView;

                    if (_viewModel.IsExpressionMode)
                    {
                        var entry = new Entry { Text = cell.Expression, BackgroundColor = Colors.White };
                        entry.TextChanged += (s, e) => cell.UpdateExpression(e.NewTextValue);
                        cellView = entry;
                    }
                    else
                    {
                        string displayValue = string.IsNullOrEmpty(cell.ErrorMessage)
                            ? (string.IsNullOrWhiteSpace(cell.Expression) ? "" : cell.GetValue().ToString())
                            : cell.ErrorMessage;

                        var label = new Label
                        {
                            Text = displayValue,
                            TextColor = string.IsNullOrEmpty(cell.ErrorMessage) ? Colors.Black : Colors.Red,
                            BackgroundColor = Colors.LightGray,
                            VerticalTextAlignment = TextAlignment.Center,
                            HorizontalTextAlignment = TextAlignment.Center,
                            HeightRequest = 45
                        };
                        cellView = label;
                    }

                    SpreadsheetGrid.Add(cellView, c + 1, r + 1);
                }
            }
        }

        private string GetColumnName(int col)
        {
            string colName = "";
            int tempCol = col;
            while (tempCol >= 0)
            {
                colName = (char)('A' + (tempCol % 26)) + colName;
                tempCol = (tempCol / 26) - 1;
            }
            return colName;
        }

        private async void OnHelpClicked(object sender, EventArgs e)
        {
            string helpText = "Лабораторна робота 1. Варіант 81.\n\n" +
                              "Дозволені операції: =, <, >, <=, >=, <>, not, or, and, eqv.\n" +
                              "Операнди: цілі числа, імена клітинок.\n" +
                              "Кінцевий результат: логічне значення (True/False).\n\n" +
                              "Режими:\n" +
                              "ВИРАЗ - редагування формул.\n" +
                              "ЗНАЧЕННЯ - перегляд результатів та помилок.";

            await DisplayAlert("Довідка про програму", helpText, "Зрозуміло");
        }
    }
}