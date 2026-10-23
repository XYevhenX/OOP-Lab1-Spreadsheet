using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using SpreadsheetApp.Models;

namespace SpreadsheetApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private Spreadsheet _spreadsheet;
        private bool _isExpressionMode;

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

        public event Action OnGridChanged;

        public MainViewModel()
        {
            Spreadsheet = new Spreadsheet(5, 5);
            IsExpressionMode = true;

            AddRowCommand = new Command(() => { Spreadsheet.AddRow(); NotifyGridRedraw(); });
            AddColumnCommand = new Command(() => { Spreadsheet.AddColumn(); NotifyGridRedraw(); });
            RemoveRowCommand = new Command(() => { Spreadsheet.RemoveRow(); NotifyGridRedraw(); });
            RemoveColumnCommand = new Command(() => { Spreadsheet.RemoveColumn(); NotifyGridRedraw(); });

            EvaluateCommand = new Command(() =>
            {
                Spreadsheet.EvaluateAll();
                IsExpressionMode = false;
                NotifyGridRedraw();
            });

            ToggleModeCommand = new Command(() =>
            {
                IsExpressionMode = !IsExpressionMode;
                NotifyGridRedraw();
            });
        }

        private void NotifyGridRedraw()
        {
            OnGridChanged?.Invoke();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}