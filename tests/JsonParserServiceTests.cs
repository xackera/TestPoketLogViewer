using System.IO;
using TestPoketLogViewer.Services;
using Xunit;

namespace TestPoketLogViewer.Tests
{
    public class JsonParserServiceTests
    {
        private readonly JsonParserService _parser;

        public JsonParserServiceTests()
        {
            _parser = new JsonParserService();
        }

        [Fact]
        public void ParseHandsFromFile_ValidJson_ReturnsHands()
        {
            // Подготовка
            string validJson = @"[
                { ""TableName"": ""Test Table"", ""HandId"": 123, ""Players"": [""Bob"", ""Alice""], ""Winners"": [""Alice""], ""WinAmount"": 50 }
            ]";
            string tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, validJson);

            try
            {
                // Действие
                var result = _parser.ParseHandsFromFile(tempFile);

                // Проверка
                Assert.NotNull(result);
                Assert.Single(result);
                Assert.Equal("Test Table", result[0].TableName);
                Assert.Equal(123, result[0].HandId);
                Assert.Equal(50, result[0].WinAmount);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void ParseHandsFromFile_InvalidJson_ReturnsEmptyList()
        {
            // Подготовка битого файла
            string invalidJson = @"[
                { ""TableName"": ""Broken JSON"", ""HandId"": 
            ";
            string tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, invalidJson);

            try
            {
                // Действие: парсер не должен упасть
                var result = _parser.ParseHandsFromFile(tempFile);

                // Должен вернуть пустой список
                Assert.NotNull(result);
                Assert.Empty(result);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void ParseHandsFromFile_FileNotFound_ReturnsEmptyList()
        {
            // Подготовка
            string nonExistentFile = "this_file_does_not_exist_at_all.json";

            // Действие
            var result = _parser.ParseHandsFromFile(nonExistentFile);

            // Проверка
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
