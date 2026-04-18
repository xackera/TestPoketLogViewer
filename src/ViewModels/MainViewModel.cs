using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using TestPoketLogViewer.Models;
using TestPoketLogViewer.Services;


namespace TestPoketLogViewer.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly FolderScannerService _scannerService;
        private readonly object _lockData = new object();
        
        // Сырые данные 
        private List<PokerHand> _allHands = new List<PokerHand>();

        public MainViewModel()
        {
            _scannerService = new FolderScannerService();
            TableNames = new ObservableCollection<string>();
            HandIds = new ObservableCollection<long>();

            // разрешаю обновление из фоновых потоков
            BindingOperations.EnableCollectionSynchronization(TableNames, _lockData);
            BindingOperations.EnableCollectionSynchronization(HandIds, _lockData);

            // фильтрации
            TableNamesView = CollectionViewSource.GetDefaultView(TableNames);
            TableNamesView.Filter = FilterTables;

            HandIdsView = CollectionViewSource.GetDefaultView(HandIds);
            HandIdsView.Filter = FilterHands;

            // Инициализация команд
            StartScanCommand = new RelayCommand(ExecuteStartScan, CanExecuteStartScan);
            SelectFolderCommand = new RelayCommand(ExecuteSelectFolder);
            SwitchLanguageCommand = new RelayCommand(ExecuteSwitchLanguage);

            // Инициализация стартового статуса
            StatusMessage = GetLocalizedString("StatusWait");
        }

        #region UI свойства

        private string _selectedDirectory = string.Empty;
        public string SelectedDirectory
        {
            get => _selectedDirectory;
            set 
            { 
                _selectedDirectory = value; 
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private string _statusMessage = "";
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        private string _currentLanguage = "ru";
        public bool IsRuSelected => _currentLanguage == "ru";
        public bool IsEnSelected => _currentLanguage == "en";

        private bool _isScanning = false;
        public bool IsScanning
        {
            get => _isScanning;
            set
            {
                _isScanning = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotScanning));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsNotScanning => !_isScanning;

        private string _searchQuery = string.Empty;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
                
                // При вводе текста обновляем фильтры в обоих списках
                TableNamesView.Refresh();
                HandIdsView.Refresh();
            }
        }

        // Коллекция левой панели (уникальные названия столов)
        public ObservableCollection<string> TableNames { get; }
        public ICollectionView TableNamesView { get; }
        
        private string? _selectedTableName;
        public string? SelectedTableName
        {
            get => _selectedTableName;
            set
            {
                _selectedTableName = value;
                OnPropertyChanged();
                UpdateHandsList();
            }
        }

        // Коллекция средней панели (ID раздач)
        public ObservableCollection<long> HandIds { get; }
        public ICollectionView HandIdsView { get; }
        
        private long? _selectedHandId;
        public long? SelectedHandId
        {
            get => _selectedHandId;
            set
            {
                _selectedHandId = value;
                OnPropertyChanged();
                UpdateHandDetails();
            }
        }

        // Свойства правой панели (Детали)
        private PokerHand? _selectedHandDetails;
        public PokerHand? SelectedHandDetails
        {
            get => _selectedHandDetails;
            set { _selectedHandDetails = value; OnPropertyChanged(); }
        }

        #endregion

        #region Команды

        public ICommand StartScanCommand { get; }
        public ICommand SelectFolderCommand { get; }
        public ICommand SwitchLanguageCommand { get; }

        /// <summary>
        /// Переключение языка
        /// </summary>
        private void ExecuteSwitchLanguage(object? parameter)
        {
            if (parameter is string lang && _currentLanguage != lang)
            {
                _currentLanguage = lang;
                OnPropertyChanged(nameof(IsRuSelected));
                OnPropertyChanged(nameof(IsEnSelected));

                var dict = new System.Windows.ResourceDictionary { Source = new Uri($"Resources/Strings.{lang}.xaml", UriKind.Relative) };
                System.Windows.Application.Current.Resources.MergedDictionaries.Clear();
                System.Windows.Application.Current.Resources.MergedDictionaries.Add(dict);
                
                // текущий статус
                if (IsScanning)
                    StatusMessage = GetLocalizedString("StatusScan");
                else
                    StatusMessage = GetLocalizedString("StatusWait");
            }
        }

        /// <summary>
        /// Плучение строки с перевдом
        /// </summary>
        private string GetLocalizedString(string key)
        {
            var r = System.Windows.Application.Current.Resources[key];
            return r as string ?? key;
        }

        private bool CanExecuteStartScan(object? parameter)
        {
            return !IsScanning && !string.IsNullOrWhiteSpace(SelectedDirectory);
        }

        private void ExecuteSelectFolder(object? parameter)
        {
#if NET8_0_OR_GREATER
            // используем новый диалог выбора папки из WPF в .NET 8.0+
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Выберите папку с файлами"
            };

            if (dialog.ShowDialog() == true)
            {
                SelectedDirectory = dialog.FolderName;
            }
#else
            // Вариант для .NET 6
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = GetLocalizedString("DialogTitle");
                dialog.UseDescriptionForTitle = true;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    SelectedDirectory = dialog.SelectedPath;
                }
            }
#endif
        }

        private void ExecuteStartScan(object? parameter)
        {
            IsScanning = true;
            StatusMessage = GetLocalizedString("StatusScan");
            
            lock (_lockData)
            {
                _allHands.Clear();
                TableNames.Clear();
                HandIds.Clear();
                SelectedHandDetails = null;
            }

            _scannerService.StartScanning(SelectedDirectory, OnDataParsed, OnScanComplete);
        }

        #endregion

        #region Callback'и от потока

        private void OnDataParsed(List<PokerHand> parsedHands)
        {
           
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                lock (_lockData)
                {
                    bool currentTableAffected = false;

                    foreach (var hand in parsedHands)
                    {
                        // Избегаем дублей 
                        if (!_allHands.Any(h => h.HandId == hand.HandId))
                        {
                            _allHands.Add(hand);
                        }
                    }

                    // Обновляем уникальные имена таблиц
                    var newTables = parsedHands
                        .Where(h => !string.IsNullOrEmpty(h.TableName))
                        .Select(h => h.TableName!)
                        .Distinct()
                        .ToList();

                    foreach (var tableName in newTables)
                    {
                        if (!TableNames.Contains(tableName))
                        {
                            // Сортировка по алфавиту "на лету" 
                            InsertSorted(TableNames, tableName);
                        }
                        
                        if (tableName == SelectedTableName)
                        {
                            currentTableAffected = true;
                        }
                    }

                    // Если новые данные принадлежат к той же таблице, которую сейчас смотрит пользователь, обновим колонку раздач (HandID)
                    if (currentTableAffected)
                    {
                        UpdateHandsList();
                    }
                }
            }));
        }

        private void OnScanComplete(int processedCount, string? error)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                IsScanning = false;
                if (error == null)
                {
                    StatusMessage = $"{GetLocalizedString("StatusDone")} {processedCount}";
                }
                else
                {
                    StatusMessage = $"{GetLocalizedString("StatusError")} {error}";
                }
            }));
        }

        #endregion

        #region Вспомогательные методы
        
        private void UpdateHandsList()
        {
            lock (_lockData)
            {
                HandIds.Clear();
                if (SelectedTableName != null)
                {
                    var ids = _allHands
                        .Where(h => h.TableName == SelectedTableName)
                        .Select(h => h.HandId)
                        .Distinct();
                        
                    foreach (var id in ids)
                    {
                        HandIds.Add(id);
                    }
                }
            }
        }

        private void UpdateHandDetails()
        {
            lock (_lockData)
            {
                if (SelectedHandId != null)
                {
                    SelectedHandDetails = _allHands.FirstOrDefault(h => h.HandId == SelectedHandId);
                }
                else
                {
                    SelectedHandDetails = null;
                }
            }
        }

        private void InsertSorted(ObservableCollection<string> collection, string item)
        {
            int index = 0;
            while (index < collection.Count && string.Compare(collection[index], item, StringComparison.OrdinalIgnoreCase) < 0)
            {
                index++;
            }
            collection.Insert(index, item);
        }

        private bool FilterTables(object item)
        {
            if (string.IsNullOrWhiteSpace(SearchQuery)) return true;
            
            if (item is string tableName)
            {
                // Фильтр по названию стола
                if (tableName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)) return true;
                
                // Или если внутри стола есть HandID, который ищет пользователь
                lock (_lockData)
                {
                    return _allHands.Any(h => h.TableName == tableName && h.HandId.ToString().Contains(SearchQuery));
                }
            }
            return false;
        }

        private bool FilterHands(object item)
        {
            if (string.IsNullOrWhiteSpace(SearchQuery)) return true;
            
            if (item is long handId)
            {
                // Фильтр по номеру раздачи
                if (handId.ToString().Contains(SearchQuery)) return true;
                
                // Если пользователь ввел название стола и он совпадает с текущим, показываю его раздачи
                if (SelectedTableName != null && SelectedTableName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
