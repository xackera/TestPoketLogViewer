cmake_minimum_required(VERSION 3.27)

project(TestPoketLogViewer LANGUAGES CSharp)

# Требется для WPF проектов: указываем, что это WPF и версию Framework'а
set(CMAKE_DOTNET_TARGET_FRAMEWORK_VERSION "net8.0-windows")
set(CMAKE_DOTNET_TARGET_FRAMEWORK "net8.0-windows")

# Включаем WPF
set_property(GLOBAL PROPERTY USE_FOLDERS ON)

# Собираем все C# файлы и XAML
file(GLOB_RECURSE SRC_FILES 
    "src/*.cs" 
    "src/*.xaml"
)

# Добавляем исполняемый файл
add_executable(TestPoketLogViewer ${SRC_FILES})

# Специфичные настройки для WPF свойств (чтобы XAML правильно компилировался)
set_property(TARGET TestPoketLogViewer PROPERTY DOTNET_TARGET_FRAMEWORK_VERSION "net8.0-windows")
set_property(TARGET TestPoketLogViewer PROPERTY VS_DOTNET_REFERENCES
    "Microsoft.CSharp"
    "PresentationCore"
    "PresentationFramework"
    "System"
    "System.Core"
    "System.Xaml"
    "WindowsBase"
)

set_property(TARGET TestPoketLogViewer PROPERTY VS_GLOBAL_UseWPF "true")
set_property(TARGET TestPoketLogViewer PROPERTY VS_GLOBAL_OutputType "WinExe")
set_property(TARGET TestPoketLogViewer PROPERTY VS_GLOBAL_ImplicitUsings "enable")
set_property(TARGET TestPoketLogViewer PROPERTY VS_GLOBAL_Nullable "enable")

# Устанавливаем App.xaml как ApplicationDefinition
set_source_files_properties("src/App.xaml" PROPERTIES VS_XAML_TYPE "ApplicationDefinition")

# Добавляем тестовый проект
add_subdirectory(tests)
