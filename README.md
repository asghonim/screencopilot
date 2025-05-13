# ScreenCopilot
 
## Overview
ScreenCopilot is a Windows application that captures screenshots and provides a system tray interface for managing the application. It includes features like specifying a folder path for saving screenshots, starting and pausing screenshot capture, and displaying notifications.

## Prerequisites

1. **Operating System**: Windows 10 or later.
2. **.NET SDK**: Install the .NET 9.0 SDK. You can download it from the [official .NET website](https://dotnet.microsoft.com/download).

## Building the Project

1. Clone the repository or download the project files.
2. Open a terminal and navigate to the project directory.
3. Run the following command to build the project:
   ```
   dotnet build ScreenCopilot.csproj
   ```

## Running the Project

1. After building the project, navigate to the output directory:
   ```
   cd bin\Debug\net9.0-windows
   ```
2. Run the executable:
   ```
   ScreenCopilot.exe
   ```

## Debugging the Project in Visual Studio Code

To debug the project in Visual Studio Code, follow these steps:

### Prerequisites

1. Install Visual Studio Code from the [official website](https://code.visualstudio.com/).
2. Install the following extensions in Visual Studio Code:
   - **C#**: Provides support for .NET development. Install it from the [Visual Studio Code Marketplace](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp).

### Steps to Debug

1. Open the project folder in Visual Studio Code.
2. Ensure the .NET SDK is installed and added to your system's PATH.
3. Press `Ctrl+Shift+D` or click on the Debug icon in the Activity Bar to open the Debug view.
4. Click on "Run and Debug" and select ".NET Core Launch (web)" or ".NET Core Attach" if prompted.
5. Set breakpoints in the code where you want to pause execution.
6. Press `F5` to start debugging. The application will launch, and you can debug it step by step.

### Notes

- Ensure that the `launch.json` file is properly configured for .NET debugging. Visual Studio Code may generate this file automatically when you first set up debugging.
- If you encounter any issues, check the Debug Console for error messages.

## Features

- **System Tray Interface**: Access the application from the system tray.
- **Screenshot Capture**: Capture screenshots at regular intervals.
- **Custom Save Path**: Specify a folder path for saving screenshots.
- **Notifications**: Receive notifications when screenshots are captured.

## Notes

- Ensure that the .NET 9.0 SDK is installed and added to your system's PATH.
- If you encounter any issues, check the logs or console output for error messages.