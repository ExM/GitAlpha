using System.Diagnostics;
using System.Text;

namespace GitAlpha.CommandExecuting;

public sealed class ProcessWrapper: IDisposable
{
	private readonly TaskCompletionSource<int> _exitTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

	private readonly object _syncRoot = new();
	private readonly Process _process;
	//UNDONE
	//private readonly ProcessOperation _logOperation;
	private readonly bool _redirectInput;
	private readonly bool _redirectOutput;
	private readonly bool _throwOnErrorExit;

	private MemoryStream? _emptyStream;
	private StreamReader? _emptyReader;

	private bool _disposed;

	public ProcessWrapper(string fileName,
		string arguments,
		string workDir,
		bool createWindow,
		bool redirectInput,
		bool redirectOutput,
		Encoding? outputEncoding,
		bool useShellExecute,
		bool throwOnErrorExit)
	{
		_redirectInput = redirectInput;
		_redirectOutput = redirectOutput;
		_throwOnErrorExit = throwOnErrorExit;

		Encoding errorEncoding = outputEncoding;
		if (throwOnErrorExit)
		{
			errorEncoding ??= Encoding.Default;
		}

		_process = new Process
		{
			EnableRaisingEvents = true,
			StartInfo =
			{
				UseShellExecute = useShellExecute,
				Verb = useShellExecute ? "open" : string.Empty,
				ErrorDialog = false,
				CreateNoWindow = !createWindow,
				RedirectStandardInput = redirectInput,
				RedirectStandardOutput = redirectOutput,
				RedirectStandardError = redirectOutput || throwOnErrorExit,
				StandardOutputEncoding = outputEncoding,
				StandardErrorEncoding = errorEncoding,
				FileName = fileName,
				Arguments = arguments,
				WorkingDirectory = workDir
			}
		};

		//UNDONE
		//_logOperation = CommandLog.LogProcessStart(fileName, arguments, workDir);

		_process.Exited += OnProcessExit;

		try
		{
			_process.Start();
			try
			{
				//UNDONE
				//_logOperation.SetProcessId(_process.Id);
			}
			catch (InvalidOperationException ex) when (useShellExecute)
			{
				// _process.Start() has succeeded, ignore the failure getting the _process.Id
				//UNDONE
				//_logOperation.LogProcessEnd(ex);
			}
		}
		catch (Exception ex)
		{
			Dispose();
			//UNDONE
			//_logOperation.LogProcessEnd(ex);
			throw new ExternalOperationException(fileName, arguments, workDir, innerException: ex);
		}
	}

	private void OnProcessExit(object sender, EventArgs eventArgs)
	{
		lock (_syncRoot)
		{
			// The Exited event can be raised after the process is disposed, however
			// if the Process is disposed then reading ExitCode will throw.
			if (!_disposed)
			{
				try
				{
					int exitCode = _process.ExitCode;
					//UNDONE _logOperation.LogProcessEnd(exitCode);

					if (_throwOnErrorExit && exitCode != 0)
					{
						string errorOutput = _process.StandardError.ReadToEnd().Trim();
						ExternalOperationException ex
							= new(command: _process.StartInfo.FileName,
								_process.StartInfo.Arguments,
								_process.StartInfo.WorkingDirectory,
								exitCode,
								new Exception(errorOutput));
						//UNDONE _logOperation.LogProcessEnd(ex);
						_exitTaskCompletionSource.TrySetException(ex);
					}

					_exitTaskCompletionSource.TrySetResult(exitCode);
				}
				catch (Exception ex)
				{
					//UNDONE _logOperation.LogProcessEnd(ex);
					_exitTaskCompletionSource.TrySetException(ex);
				}
			}
		}
	}

	public StreamWriter StandardInput
	{
		get
		{
			if (!_redirectInput)
			{
				throw new InvalidOperationException("Process was not created with redirected input.");
			}

			return _process.StandardInput;
		}
	}

	public StreamReader StandardOutput
	{
		get
		{
			if (!_redirectOutput)
			{
				throw new InvalidOperationException("Process was not created with redirected output.");
			}

			return _process.StandardOutput;
		}
	}

	public StreamReader StandardError
	{
		get
		{
			if (!_redirectOutput && !_throwOnErrorExit)
			{
				throw new InvalidOperationException("Process was not created with redirected output.");
			}

			if (!_throwOnErrorExit)
			{
				return _process.StandardError;
			}

			_emptyStream ??= new();
			_emptyReader ??= new(_emptyStream);
			return _emptyReader;
		}
	}

	/// <inheritdoc />
	public void WaitForInputIdle() => _process.WaitForInputIdle();

	/// <inheritdoc />
	public Task<int> WaitForExitAsync() => _exitTaskCompletionSource.Task;

	/// <inheritdoc />
	public Task WaitForProcessExitAsync(CancellationToken token)
	{
		return _process.WaitForExitAsync(token);
	}

	/// <inheritdoc />
	public void Dispose()
	{
		lock (_syncRoot)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;
		}

		_process.Exited -= OnProcessExit;

		_exitTaskCompletionSource.TrySetCanceled();

		_process.Dispose();

		//UNDONE _logOperation.NotifyDisposed();

		_emptyReader?.Dispose();
		_emptyStream?.Dispose();
	}
}