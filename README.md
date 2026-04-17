# TestPoketLogViewer

Просмотрщик покерных логов, разработанный в рамках тестового задания (C# WPF). 

## Требования
- ОС: Windows 10/11 (x64)
- .NET SDK: 8.0
- Visual Studio 2019-2022

## Сборка и запуск через CMake
В корневой папке репозитория выполните следующие команды:

```bash
# 1. Генерация решения (.sln и .csproj)
cmake -B build -G "Visual Studio 17 2022" 

# 2. Сборка проекта
cmake --build build --config Release
```

После этого сгенерированный проект можно запустить вручную через терминал или открыть файл `build/TestPoketLogViewer.sln` в Visual Studio.

## Архитектура
- **Язык**: C#
- **Паттерн**: MVVM
- **Многопоточность**: Чистые `System.Threading.Thread` без `Task`/`async`/`await`
- **JSON парсер**: `System.Text.Json`


