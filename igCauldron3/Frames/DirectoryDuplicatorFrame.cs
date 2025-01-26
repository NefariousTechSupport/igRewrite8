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
	/// UI Frame for duplicating an igObjectDirectory
	/// </summary>
	public sealed class DirectoryDuplicatorFrame : DirectoryActionFrame
	{
		private igObjectDirectory _source;


		/// <summary>
		/// Constructor for the frame
		/// </summary>
		/// <param name="wnd">Reference to the main window object</param>
		/// <param name="source">the <c>igObjectDirectory</c> to clone</param>
		public DirectoryDuplicatorFrame(Window wnd, igObjectDirectory source) : base(wnd, "Copy Directory" ,"Copy")
		{
			_source = source;
		}


		/// <summary>
		/// Callback function when the confirmation button is pressed
		/// </summary>
		protected override void OnActionStart()
		{
			// Duplicating igObjectDirectories is a bit hellish by virtue of the fact that they're actually literally
			// just objects when loaded, it's probably best to just save it to a file and then reload that file

			// Ideally this'd be handled with igFileContext's ram storage device but I've not implemented that yet
			string tempPath = Path.ChangeExtension(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()), "igz");
			FileStream tempFs = File.Create(tempPath);

			_source.WriteFile(tempFs, igRegistry.GetRegistry()._platform);

			// Close it so igFileContext can access it
			tempFs.Close();

			igObjectDirectory duplicate = igObjectStreamManager.Singleton.Load(tempPath, new igName(Path.GetFileNameWithoutExtension(_path)))!;
			duplicate._path = _path;

			igFileContext.Singleton.Close(duplicate._fd, igBlockingType.kMayBlock, igFileWorkItem.Priority.kPriorityNormal);

			File.Delete(tempPath);

			DirectoryManagerFrame._instance.AddDirectory(duplicate);

			Close();
		}
	}
}