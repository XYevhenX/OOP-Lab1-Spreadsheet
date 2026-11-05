using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using SpreadsheetApp.Models;

namespace SpreadsheetApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private Spreadsheet _spreadsheet;
        private bool _isExpressionMode;
        private bool _isModified;

        private readonly string _filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SpreadsheetLab81.json");

        public Spreadsheet Spreadsheet
        {
            get => _spreadsheet;
            set { _spreadsheet = value; OnPropertyChanged(); }
        }

        public bool IsExpressionMode
        {
            get => _isExpressionMode;
            set
            {
                _isExpressionMode = value;
                OnPropertyChanged();
                ModeText = _isExpressionMode ? "Режим: ВИРАЗ" : "Режим: ЗНАЧЕННЯ";
            }
        }

        public bool IsModified
        {
            get => _isModified;
            set { _isModified = value; OnPropertyChanged(); }
        }

        private string _modeText;
        public string ModeText
        {
            get => _modeText;
            set { _modeText = value; OnPropertyChanged(); }
        }

        public ICommand AddRowCommand { get; }
        public ICommand AddColumnCommand { get; }
        public ICommand RemoveRowCommand { get; }
        public ICommand RemoveColumnCommand { get; }
        public ICommand EvaluateCommand { get; }
        public ICommand ToggleModeCommand { get; }

        public ICommand NewTableCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand ExitCommand { get; }

        public event Action OnGridChanged;

        public MainViewModel()
        {
            Spreadsheet = new Spreadsheet(5, 5);
            IsExpressionMode = true;
            IsModified = false;

            AddRowCommand = new Command(() => { Spreadsheet.AddRow(); IsModified = true; NotifyGridRedraw(); });
            AddColumnCommand = new Command(() => { Spreadsheet.AddColumn(); IsModified = true; NotifyGridRedraw(); });
            RemoveRowCommand = new Command(() => { Spreadsheet.RemoveRow(); IsModified = true; NotifyGridRedraw(); });
            RemoveColumnCommand = new Command(() => { Spreadsheet.RemoveColumn(); IsModified = true; NotifyGridRedraw(); });

            EvaluateCommand = new Command(() => { Spreadsheet.EvaluateAll(); IsExpressionMode = false; NotifyGridRedraw(); });
            ToggleModeCommand = new Command(() => { IsExpressionMode = !IsExpressionMode; NotifyGridRedraw(); });

            NewTableCommand = new Command(async () =>
            {
                if (IsModified)
                {
                    bool answer = await Application.Current.MainPage.DisplayAlert("Увага", "Є незбережені зміни. Створити нову таблицю і втратити їх?", "Так", "Ні");
                    if (!answer) return;
                }
                Spreadsheet = new Spreadsheet(5, 5);
                IsModified = false;
                NotifyGridRedraw();
            });

            SaveCommand = new Command(SaveToFile);
            LoadCommand = new Command(LoadFromFile);

            ExitCommand = new Command(async () =>
            {
                if (IsModified)
                {
                    string action = await Application.Current.MainPage.DisplayActionSheet(
                        "Увага! Є незбережені зміни.\n Що зробити?",
                        "Скасувати",
                        null,
                        "Зберегти та вийти",
                        "Вийти без збереження");

                    if (action == "Скасувати" || string.IsNullOrEmpty(action))
                    {
                        return;
                    }

                    if (action == "Зберегти та вийти")
                    {
                        SaveToFile();
                        await System.Threading.Tasks.Task.Delay(500);
                        Application.Current.Quit();
                    }
                    else if (action == "Вийти без збереження")
                    {
                        Application.Current.Quit();
                    }
                }
                else
                {
                    Application.Current.Quit();
                }
            });
        }

        private async void SaveToFile()
        {
            try
            {
                var data = new SpreadsheetData { RowCount = Spreadsheet.RowCount, ColCount = Spreadsheet.ColCount };
                for (int r = 0; r < Spreadsheet.RowCount; r++)
                {
                    for (int c = 0; c < Spreadsheet.ColCount; c++)
                    {
                        var cell = Spreadsheet.GetCell(r, c);
                        if (cell != null && !string.IsNullOrWhiteSpace(cell.Expression))
                        {
                            data.Cells.Add(new CellData { Name = cell.Name, Expression = cell.Expression });
                        }
                    }
                }

                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
                IsModified = false;

                await Application.Current.MainPage.DisplayAlert("Успіх", "Таблицю збережено!", "ОК");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Помилка", $"Не вдалося зберегти: {ex.Message}", "ОК");
            }
        }

        private async void LoadFromFile()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    await Application.Current.MainPage.DisplayAlert("Увага", "Файл збереження ще не існує.", "ОК");
                    return;
                }

                string json = File.ReadAllText(_filePath);
                var data = JsonSerializer.Deserialize<SpreadsheetData>(json);

                if (data != null)
                {
                    Spreadsheet = new Spreadsheet(data.RowCount, data.ColCount);
                    foreach (var cellData in data.Cells)
                    {
                        var cell = Spreadsheet.GetCellByName(cellData.Name);
                        if (cell != null) cell.UpdateExpression(cellData.Expression);
                    }

                    Spreadsheet.EvaluateAll();
                    IsExpressionMode = false;
                    IsModified = false;
                    NotifyGridRedraw();

                    await Application.Current.MainPage.DisplayAlert("Успіх", "Таблицю завантажено!", "ОК");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Помилка", $"Не вдалося завантажити: {ex.Message}", "ОК");
            }
        }

        private void NotifyGridRedraw() => OnGridChanged?.Invoke();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}