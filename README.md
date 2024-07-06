# HiEnds - TigerRoar
TigerRoar is a powerful tool designed to assist in automating and managing command execution tasks in development and deployment environments. This tool aims to make it easier for developers and system administrators to execute complex commands and scripts, track results, and manage outputs effectively.
![image](https://github.com/sugia279/HiEnds/assets/10128207/04f4d254-fce4-4422-ad37-3efbf60a83aa)


**App Version**:
https://github.com/sugia279/HiEnds/blob/main/AppVersions/TigerRoar_V1.7z

**Requirements**
- Install **.NET 8 or later**: https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-8.0.301-windows-x64-installer
- **Download the TigerRoar_V1.7z => Decompress => Open TigerRoar.exe**
   
## Use Case: NUnit 3 Test Runner GUI for Tests

TigerRoar addresses the lack of an official **GUI for NUnit 3.x** by providing a user-friendly interface for running and managing NUnit tests. This is particularly useful for developers and QA engineers who need to automate and streamline their testing processes using NUnit 3.17.0.

https://github.com/sugia279/HiEnds/assets/10128207/487ee507-de1a-45d7-8ff4-ba0b7cab4fe4

- **Test Discovery**: Browse and discover all tests within a specified DLL, ensuring no test is overlooked. 
- **Search Test**: Search tests following the criteria of case name or the test method attributes.
- **Selective Test Execution**: Easily select and run specific tests from the discovered list using the integrated grid.
- **Compatibility with NUnit 3.17.0**: Built to work flawlessly with NUnit 3.17.0, leveraging its powerful test execution capabilities.
- **Integration with `nunitconsole.exe`**: Executes selected tests using `nunitconsole.exe`, providing detailed results and logs.
- **Can run multithread**: Set RunThreadNumber = x then click the Save button, click Refresh and select multi-tests to enjoy it.

https://github.com/sugia279/HiEnds/assets/10128207/a537202e-d098-47a2-988c-d605a80d450a

## Getting Started

1. **Download and Install**: Download HiEndsRunner from the [release page](#) and follow the installation instructions.
2. **Load DLL**: Open TigerRoar and browse to the DLL containing your NUnit tests.
3. **Run Tests**: Select the tests you want to run from the displayed list and click the "Run" button.
4. **View Results**: Analyze the test results directly within the tool.

## Key Features of TigerRoar
![image](https://github.com/sugia279/HiEnds/assets/10128207/5a041f10-caf5-4420-a7d7-02952f59b3a3)

### Data Extraction:
Extract various kinds of data from DLL, CSV, and XML files for comprehensive testing and reporting. 
- **Template file**:
  ![image](https://github.com/sugia279/HiEnds/assets/10128207/1cccad0a-f277-4196-b8cb-b36d0e5cd39b)

### Command and Script Execution:
TigerRoar allows users to run commands and scripts from a user interface or through scheduled tasks. The tool supports various command and script types, including batch, PowerShell, and other scripting languages.
### Logging and Result Tracking
For every command executed, TigerRoar logs the entire output and result, including the exit code. This helps users easily check and analyze errors that occur during execution.
### Automated Input
TigerRoar supports automated input for commands that require user input. When it detects input prompts, the tool can automatically provide pre-defined values, reducing the need for user intervention and increasing automation.

## Future Improvements
We are continuously working to enhance HiEnds's capabilities. One of our upcoming features includes improving templates to extract and transform data into DataTables from additional sources such as JSON, SQL queries, and API services. This will provide even greater flexibility and utility for comprehensive testing and reporting.


