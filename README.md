# Meditation

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![CI](https://github.com/DevToolsNET/meditation/actions/workflows/main.yml/badge.svg)

An application for runtime patching of .NET programs.

-------------------------------------

## Development Setup

### Prerequisites

#### Windows
* .NET 8 SDK
* Visual Studio 2022 with component "Desktop development with C++"
	* See for more information: [Native AOT Prerequisites for Windows](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/?tabs=net8plus%2Cwindows#prerequisites).

#### Linux

* .NET 8 SDK
* clang
* zlib1g-dev
* See for more information: [Native AOT Prerequisites for Linux](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/?tabs=linux-ubuntu%2Cnet8#prerequisites)

### Building & Setup

#### Windows

* Build managed and unmanaged DLLs:
```bash
dotnet build -c Debug src/Meditation.sln
dotnet publish -c Debug src/Meditation.Bootstrap.Native -r win-x64
```
* Install native dependency:
```bash
cp src/Meditation.Bootstrap.Native/bin/Debug/net8.0/win-x64/native/Meditation.Bootstrap.Native.dll \
   src/Meditation.UI.Desktop/bin/Debug/net8.0/Meditation.Bootstrap.Native.dll
```

#### Linux

* Build managed and unmanaged DLLs:
```bash
dotnet build -c Debug src/Meditation.sln
dotnet publish -c Debug src/Meditation.Bootstrap.Native -r linux-x64
```
* Install native dependency:
```bash
cp src/Meditation.Bootstrap.Native/bin/Debug/net8.0/win-x64/native/Meditation.Bootstrap.Native.so \
   src/Meditation.UI.Desktop/bin/Debug/net8.0/Meditation.Bootstrap.Native.so
```

## State of development

The project is under development in the pre-stable release state. Supported .NET versions and platforms can be seen in the table below:

|                         | .NET Framework              | .NET Core / .NET      |
|-------------------------|-----------------------------|-----------------------|
| Windows-x86             | :x:                         | :x:                   |
| Windows-x64             | :construction:              | :white_check_mark:    |
| Linux-x86               | (not applicable)            | :x:                   |
| Linux-x64               | (not applicable)            | :white_check_mark:    |

## Licensing

Project Meditation is licensed under [MIT license](LICENSE).