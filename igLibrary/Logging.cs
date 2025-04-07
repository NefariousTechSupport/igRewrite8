/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Text;

namespace igLibrary
{
	/// <summary>
	/// Functions for Logging debug messages
	/// </summary>
	public static class Logging
	{
		/// <summary>
		/// Various logging modes
		/// </summary>
		public enum LoggingMode
		{
			None,
			Console,
			File
		}

		private const string _infoMessage    = "[ INFO ] ";
		private const string _warningMessage = "[ WARN ] ";
		private const string _errorMessage   = "[  ERR ] ";

		/// <summary>
		/// What output kind should be used
		/// </summary>
		public static LoggingMode Mode
		{
			get => _mode;
			set
			{
				FlushLog();

				_mode = value;
			}
		}


		/// <summary>
		/// Where to output the log if <c>Mode</c> is set to <c>LoggingMode.File</c>
		/// </summary>
		public static FileStream LogFile
		{
			get
			{
				if(Mode != LoggingMode.File)
				{
					throw new InvalidOperationException("Cannot access log file! Logging mode not set to 'File'!");
				}

				if(_logFile == null)
				{
					throw new InvalidOperationException("Cannot access log file! No log file assigned!");
				}

				return _logFile;
			}
			set
			{
				// Don't check the mode, assume they'll set that later and throw the error if they don't 

				if(_logFile != null)
				{
					throw new InvalidOperationException("Override the log file is not permitted!");
				}

				if(value == null)
				{
					throw new ArgumentNullException("Cannot set log file to null!");
				}

				if(!value.CanWrite)
				{
					throw new ArgumentException("Given log file does not support writing!");
				}

				_logFile = value;
			}
		}


		private static LoggingMode _mode;
		private static FileStream? _logFile;	//Use the property LogFile instead, it has actual checks

		/// <summary>
		/// Outputs an informative log message.
		/// </summary>
		/// <param name="fmt">The log message</param>
		/// <param name="args">formatting args for the log message</param>
		public static void Info(string fmt, params object?[] args)
		{
			LogInternal(_infoMessage + fmt, args);
		}


		/// <summary>
		/// Outputs a warning log message.
		/// </summary>
		/// <param name="fmt">The log message</param>
		/// <param name="args">formatting args for the log message</param>
		public static void Warn(string fmt, params object?[] args)
		{
			LogInternal(_warningMessage + fmt, args);
		}


		/// <summary>
		/// Outputs an error log message.
		/// </summary>
		/// <param name="fmt">The log message</param>
		/// <param name="args">formatting args for the log message</param>
		public static void Error(string fmt, params object?[] args)
		{
			LogInternal(_errorMessage + fmt, args);
		}


		/// <summary>
		/// Flushes the log out to the destination.
		/// Call this just before termination.
		/// </summary>
		public static void FlushLog()
		{
			switch(_mode)
			{
				case LoggingMode.None:
					//Nothing to flush
					break;

				case LoggingMode.Console:
					//Gets flushed automatically
					break;

				case LoggingMode.File:
					LogFile.Flush();
					break;
			}
		}


		/// <summary>
		/// Internal logging, outputs to the desired location
		/// </summary>
		/// <param name="fmt">the string with <c>string.Format</c> style formatting.</param>
		/// <param name="args">arguments to pass to <string.Format</c>.</param>
		private static void LogInternal(string fmt, params object?[] args)
		{
			switch(_mode)
			{
				case LoggingMode.None:
					//No logging
					break;

				case LoggingMode.Console:
					Console.WriteLine(fmt, args);
					break;

				case LoggingMode.File:
					LogFile.Write(Encoding.UTF8.GetBytes(string.Format(fmt, args)));
					LogFile.WriteByte(0x0A);	// '\n'
					break;
			}
		}
	}
}