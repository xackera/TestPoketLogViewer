using System;
using System.Collections.Generic;
using TestPoketLogViewer.Models;

namespace TestPoketLogViewer.Services
{
    /// <summary>
    /// Контракт для сканера директорий и наблюдателя
    /// </summary>
    public interface IFolderScannerService
    {
        void StartScanning(string path, Action<List<PokerHand>> onDataParsed, Action<int, string?> onComplete);
        void StopScanning();
    }
}
