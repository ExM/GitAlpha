using System.Text;

namespace GitAlpha.CommandExecuting;

public sealed class Executable
{
	private readonly string _workingDir;
	private readonly string _fileName;

	public Executable(string fileName, string workingDir = "")
	{
		_workingDir = workingDir;
		_fileName = fileName;
	}

	/// <inheritdoc />
	public ProcessWrapper Start(string arguments,
		bool createWindow = false,
		bool redirectInput = false,
		bool redirectOutput = false,
		Encoding? outputEncoding = null,
		bool useShellExecute = false,
		bool throwOnErrorExit = true)
	{
		return new ProcessWrapper(_fileName, arguments, _workingDir, createWindow, redirectInput, redirectOutput,
			outputEncoding, useShellExecute, throwOnErrorExit);
	}
}
