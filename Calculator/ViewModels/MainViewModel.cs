﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Calculator.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {

        public MainViewModel()
        {
            Expression = "";
            Result = "0";
            _entries = new Stack<string>();

            ChangeToDegreeUnit = new RelayCommand(obj => {
                UsedAngleUnit = ExpressionParser.AngleUnit.Degrees;
                HasExpressionChanged = true;
            });
            ChangeToRadiansUnit = new RelayCommand(obj => {
                UsedAngleUnit = ExpressionParser.AngleUnit.Radians;
                HasExpressionChanged = true;
            });
            ChangeToGradUnit = new RelayCommand(obj => {
                UsedAngleUnit = ExpressionParser.AngleUnit.Grad;
                HasExpressionChanged = true;
            });

            ToggleShortCutVisibility = new RelayCommand(obj => AreShortCutsVisible = !AreShortCutsVisible);

            InsertEntry = new RelayCommand(obj => {
                string objStr = obj?.ToString() ?? "";
                Expression += objStr;
                _entries.Push(objStr);
            });
            RemoveLastEntry = new RelayCommand(obj => Expression = Expression[..^_entries.Pop().Length], () => _entries.Any());
            Clear = new RelayCommand(obj => {
                Expression = "";
                Result = "0";
                _entries.Clear();
            });
            Evaluate = new RelayCommand(obj => Result = ExpressionParser.Evaluate(Expression).ToString());

        }


        // Properties
        public ExpressionParser.AngleUnit UsedAngleUnit {
            get => ExpressionParser.UsedAngleUnit;
            set {
                ExpressionParser.UsedAngleUnit = value;
                OnPropertyChanged(nameof(IsUnitDegreesUsed));
                OnPropertyChanged(nameof(IsUnitRadiansUsed));
                OnPropertyChanged(nameof(IsUnitGradUsed));
            }
        }

        public bool IsUnitDegreesUsed => UsedAngleUnit == ExpressionParser.AngleUnit.Degrees;
        public bool IsUnitRadiansUsed => UsedAngleUnit == ExpressionParser.AngleUnit.Radians;
        public bool IsUnitGradUsed => UsedAngleUnit == ExpressionParser.AngleUnit.Grad;

        private bool _areShortcutsVisible;
        public bool AreShortCutsVisible {
            get => _areShortcutsVisible;
            set {
                _areShortcutsVisible = value;
                OnPropertyChanged();
            }
        }


        private bool _hasExpressionChanged;
        public bool HasExpressionChanged {
            get => _hasExpressionChanged;
            set {
                _hasExpressionChanged = value;
                OnPropertyChanged();
            }
        }

        private string _expression = "";
        public string Expression {
            get => _expression;
            set {
                _expression = value;
                HasExpressionChanged = true;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RemoveLastEntry));
                OnPropertyChanged(nameof(Clear));
            }
        }

        private string _result = "";
        public string Result {
            get => _result;
            set {
                _result = value;
                HasExpressionChanged = false;
                OnPropertyChanged();
            }
        }

        private int _pointerIndex;
        public int PointerIndex {
            get => _pointerIndex;
            set {
                _pointerIndex = value;
                OnPropertyChanged();
            }
        }


        //Commands 

        //public ICommand ChangeToUnaryPage { get; private set; }
        //public ICommand ChangeToPolyadicPage { get; private set; }

        public ICommand ChangeToDegreeUnit { get; private set; }
        public ICommand ChangeToRadiansUnit { get; private set; }
        public ICommand ChangeToGradUnit { get; private set; }

        public ICommand ToggleShortCutVisibility { get; private set; }

        private readonly Stack<string> _entries;
        public ICommand InsertEntry { get; private set; }
        public ICommand RemoveLastEntry { get; private set; }
        public ICommand Clear { get; private set; }
        public ICommand Evaluate { get; private set; }




        // INotifyPropertyChanged support

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
