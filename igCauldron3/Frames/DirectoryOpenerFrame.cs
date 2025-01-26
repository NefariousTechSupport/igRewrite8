/*
	Copyright (c) 2022-2025, The igCauldron Contributors.
	igCauldron and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Runtime.Serialization;
using igLibrary.Core;
using ImGuiNET;

namespace igCauldron3
{
	/// <summary>
	/// UI Frame for opening an igObjectDirectory by path
	/// </summary>
	public sealed class DirectoryOpenerFrame : DirectoryActionFrame
	{
		/// <summary>
		/// Constructor for the frame
		/// </summary>
		/// <param name="wnd">Reference to the main window object</param>
		public DirectoryOpenerFrame(Window wnd) : base(wnd, "Open Directory", "Open"){}


		/// <summary>
		/// Callback function when the confirmation button is pressed
		/// </summary>
		protected override void OnActionStart()
		{
			try
			{
				igObjectDirectory directory = igObjectStreamManager.Singleton.Load(_path)!;
				DirectoryManagerFrame._instance.AddDirectory(directory);

				Close();
			}
			catch(KeyNotFoundException)
			{
				_errorMsg = $"Path \"{_path}\" doesn't end in a supported file extension, try \".igz\" or \".lng\".";
			}
			catch(FileNotFoundException)
			{
				_errorMsg = $"File \"{_path}\" does not exist in any mounted archive.";
			}
		}
	}
}