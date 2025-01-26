/*
	Copyright (c) 2022-2025, The igCauldron Contributors.
	igCauldron and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using igLibrary.Core;
using igLibrary;
#if CAULDRON_FEATURE_EXTERNAL_CONSOLE
using System.Runtime.InteropServices;
#endif // CAULDRON_FEATURE_EXTERNAL_CONSOLE

namespace igCauldron3
{
	/// <summary>
	/// Needs no introduction
	/// </summary>
	public static class Program
	{
		[STAThread] 
		public static void Main(string[] args)
		{
#if DEBUG
			Logging.Mode = Logging.LoggingMode.Console;
#else
			Logging.Mode = Logging.LoggingMode.File;
			DateTime now = DateTime.Now;

			Directory.CreateDirectory(Path.Combine(CauldronConfig.ConfigFolder, "Logs"));
			// Cry about this line length
			Logging.LogFile = File.Create(Path.Combine(CauldronConfig.ConfigFolder, "Logs", $"igCauldron-{now.Year.ToString("0000")}-{now.Month.ToString("00")}-{now.Day.ToString("00")}-{now.Hour.ToString("00")}-{now.Minute.ToString("00")}-{now.Second.ToString("00")}.log"));
#endif // DEBUG

#if CAULDRON_FEATURE_EXTERNAL_CONSOLE
			AllocConsole();
#endif // CAULDRON_FEATURE_EXTERNAL_CONSOLE

			Window wnd = new Window(
				new GameWindowSettings()
				{
					IsMultiThreaded = false,
					RenderFrequency = 60,
					UpdateFrequency = 60,
				},
				new NativeWindowSettings()
				{
					Size = new Vector2i(1280, 720),
					Title = "igCauldron",
					Flags = ContextFlags.ForwardCompatible
				},
				args
			);

#if !DEBUG
			try
			{
				wnd.Run();
			}
			catch(Exception e)
			{
				System.Windows.Forms.MessageBox.Show($"{e.Message}\".\n\n{e.StackTrace}", $"Encountered \"{e.GetType().Name}\"", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
#else
			wnd.Run();
#endif // !DEBUG

#if CAULDRON_FEATURE_EXTERNAL_CONSOLE
			FreeConsole();
#endif // CAULDRON_FEATURE_EXTERNAL_CONSOLE

			Logging.FlushLog();
		}


#if CAULDRON_FEATURE_EXTERNAL_CONSOLE && _WINDOWS
		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool AllocConsole();

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool FreeConsole();
#endif // CAULDRON_FEATURE_EXTERNAL_CONSOLE
	}
}