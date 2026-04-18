using System.IO;
using TestPoketLogViewer.Models;

namespace TestPoketLogViewer.Services
{
    /// <summary>
    /// Серквис для сканирования папки на наличие JSON файлов и парсинга их в модели PokerHand.
    /// </summary>
    public class FolderScannerService : IFolderScannerService
    {
        private readonly IJsonParserService _parser;
        private FileSystemWatcher? _watcher;
        private Action<List<PokerHand>>? _onDataParsedCallback;
        private volatile bool _isStopped;

        public FolderScannerService(IJsonParserService parser)
        {
            _parser = parser;
        }

        /// <summary>
        /// Запуск сканирования и наблюдателя за изменениями в папке
        /// </summary>
        /// <param name="path">Путь к папке</param>
        /// <param name="onDataParsed">Callback при успешном парсинге файла (возвращает список моделей)</param>
        /// <param name="onComplete">Callback по завершении сканирования (передает количество обработанных файлов)</param>
        public void StartScanning(string path, Action<List<PokerHand>> onDataParsed, Action<int, string?> onComplete)
        {
            _onDataParsedCallback = onDataParsed;
            _isStopped = false;

            Thread sThread = new Thread(() =>
            {
                int processedFiles = 0;
                try
                {
                    if (Directory.Exists(path))
                    {
                        // Подключаем слежение за папкой
                        StartWatching(path);

                        // папка существует, начинаю сканирование
                        var files = Directory.EnumerateFiles(path, "*.json", SearchOption.AllDirectories);
                        
                        foreach (var file in files)
                        {
                            if (_isStopped) break;

                            var hands = _parser.ParseHandsFromFile(file);
                            if (hands != null && hands.Count > 0)
                            {
                                onDataParsed?.Invoke(hands);
                            }
                            processedFiles++;
                        }
                    }
                    else
                    {
                        onComplete?.Invoke(0, "Директория не найдена");
                        return;
                    }

                    onComplete?.Invoke(processedFiles, null); // Успех
                }
                catch (ThreadAbortException)
                {
                    // нас прервали, сообщаем об этом
                    onComplete?.Invoke(processedFiles, "Сканирование прервано");
                }
                catch (ThreadInterruptedException)
                {
                    // нас прервали, сообщаем об этом
                    onComplete?.Invoke(processedFiles, "Сканирование прервано");
                }
                catch (Exception ex)
                {
                    // произошла ошибка, сообщаем об этом
                    onComplete?.Invoke(processedFiles, $"Ошибка: {ex.Message}");
                }
            });
           
            sThread.IsBackground = true; 
            sThread.Start();
        }

        private void StartWatching(string path)
        {
            // Очищаем старый watcher
            _watcher?.Dispose();

            _watcher = new FileSystemWatcher(path, "*.json")
            {
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            // События watcher
            _watcher.Created += OnFileChanged;
            _watcher.Changed += OnFileChanged;
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            // Задержака для ОС. чтобы блокировка с файла была снята
            Thread.Sleep(200);

            // Обрабатываю файл в отдельном (background) потоке
            var hands = _parser.ParseHandsFromFile(e.FullPath);
            if (hands != null && hands.Count > 0 && _onDataParsedCallback != null && !_isStopped)
            {
                _onDataParsedCallback.Invoke(hands);
            }
        }

        /// <summary>
        /// Остановка сканирования
        /// </summary>
        public void StopScanning()
        {
            _isStopped = true;
            if (_watcher != null)
            {
                try
                {
                    _watcher.EnableRaisingEvents = false;
                    _watcher.Dispose();
                    _watcher = null;
                } catch(Exception) { 
                
                }
                
            }
        }
    }
}
