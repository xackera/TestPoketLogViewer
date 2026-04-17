using System.Windows.Input;

namespace TestPoketLogViewer.ViewModels
{
    /// <summary>
    /// Команда для выполнения действий (биндится к кнопкам в интерфейсе).
    /// </summary>
    public class RelayCommand : ICommand
    {
        // Делегат действия, которое нужно выполнить
        private readonly Action<object?> _execute;
        
        // Делегат проверки, можно ли в данный момент нажимать кнопку
        private readonly Func<object?, bool>? _canExecute;

        /// <summary>
        /// Конструктор команды.
        /// </summary>
        /// <param name="execute">Метод, который сработает при нажатии.</param>
        /// <param name="canExecute">Необязательный метод, определяющий доступность кнопки.</param>
        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Событие, которое заставляет WPF перепроверить статус кнопки.
        /// Привязывается к глобальному диспетчеру (CommandManager).
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Определяет, может ли команда выполниться в ее текущем состоянии.
        /// </summary>
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        /// <summary>
        /// Выполнение команды при клике.
        /// </summary>
        public void Execute(object? parameter)
        {
            _execute(parameter);
        }
    }
}
