project(TestPoketLogViewer.Tests LANGUAGES CSharp)

set(CMAKE_DOTNET_TARGET_FRAMEWORK_VERSION "net8.0")
set(CMAKE_DOTNET_TARGET_FRAMEWORK "net8.0")


file(GLOB_RECURSE TEST_SRC_FILES 
    "*.cs"
    "../src/Services/JsonParserService.cs"
    "../src/Models/PokerHand.cs"
)

add_library(TestPoketLogViewer.Tests SHARED ${TEST_SRC_FILES})

set_property(TARGET TestPoketLogViewer.Tests PROPERTY DOTNET_TARGET_FRAMEWORK_VERSION "net8.0")
set_property(TARGET TestPoketLogViewer.Tests PROPERTY VS_GLOBAL_ImplicitUsings "enable")
set_property(TARGET TestPoketLogViewer.Tests PROPERTY VS_GLOBAL_Nullable "enable")


set_property(TARGET TestPoketLogViewer.Tests PROPERTY VS_PACKAGE_REFERENCES
    "xunit_2.9.2"
    "xunit.runner.visualstudio_2.8.2"
    "Microsoft.NET.Test.Sdk_17.11.1"
)
