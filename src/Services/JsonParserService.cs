using System.IO;
using System.Text.Json;
using TestPoketLogViewer.Models;

namespace TestPoketLogViewer.Services
{
    /// <summary>
    /// JSON парсер
    /// </summary>
    public class JsonParserService : IJsonParserService
    {
        /// <summary>
        /// Парсер. Пропускает не валидные файлы.
        /// </summary>
        public List<PokerHand> ParseHandsFromFile(string filePath)
        {
            var result = new List<PokerHand>();

            if (!File.Exists(filePath))
            {
                return result;
            }

            try
            {
              
                string json = File.ReadAllText(filePath);
                var data = JsonSerializer.Deserialize<List<PokerHand>>(json);
                
                if (data != null)
                {
                    result.AddRange(data);
                }
            }
            catch (JsonException)
            {
                // ошибка - пропускаю 
                Console.WriteLine($"[Ошибка] JSON-файл не валидный: {filePath}");
            }
            catch (Exception)
            {
                
            }

            return result;
        }
    }
}
