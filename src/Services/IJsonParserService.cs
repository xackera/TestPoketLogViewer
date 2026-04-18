using System.Collections.Generic;
using TestPoketLogViewer.Models;

namespace TestPoketLogViewer.Services
{
    /// <summary>
    /// Контракт для JSON парсера
    /// </summary>
    public interface IJsonParserService
    {
        List<PokerHand> ParseHandsFromFile(string filePath);
    }
}
