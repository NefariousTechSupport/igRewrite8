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
	/// UI Frame for creating a new igObjectDirectory
	/// </summary>
	public sealed class DirectoryCreatorFrame : DirectoryActionFrame
	{
		/// <summary>
		/// Constructor for the frame
		/// </summary>
		/// <param name="wnd">Reference to the main window object</param>
		public DirectoryCreatorFrame(Window wnd) : base(wnd, "New Directory", "Create"){}


		/// <summary>
		/// Callback function when the confirmation button is pressed
		/// </summary>
		protected override void OnActionStart()
		{
			igObjectDirectory newDir = new igObjectDirectory(_path, new igName(Path.GetFileNameWithoutExtension(_path)));
			newDir._nameList = new igNameList();
			newDir._useNameList = true;
			newDir._type = igObjectDirectory.FileType.kIGZ;

			igObjectStreamManager.Singleton.AddObjectDirectory(newDir, _path);

			DirectoryManagerFrame._instance.AddDirectory(newDir);

			Close();
		}
	}
}