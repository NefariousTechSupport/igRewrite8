/*
	Copyright (c) 2022-2025, The igCauldron Contributors.
	igCauldron and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using ImGuiNET;
using igLibrary.Core;

namespace igCauldron3
{
	/// <summary>
	/// UI frame for the top menu bar
	/// </summary>
	public class MenuBarFrame : Frame
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="wnd">The window to parent the frame to</param>
		public MenuBarFrame(Window wnd) : base(wnd){}


		/// <summary>
		/// Renders the ui
		/// </summary>
		public override void Render()
		{
			if(ImGui.BeginMainMenuBar())
			{
				if(ImGui.BeginMenu("File"))
				{
					if(ImGui.MenuItem("Open"))
					{
						_wnd._frames.Add(new DirectoryOpenerFrame(_wnd));
					}
					else if(ImGui.MenuItem("Save"))
					{
						MemoryStream ms = new MemoryStream();
						igObjectDirectory target = DirectoryManagerFrame._instance.CurrentDir;
						target.WriteFile(ms, igRegistry.GetRegistry()._platform);
						ms.Seek(0, SeekOrigin.Begin);
#if DEBUG
						FileStream fs = File.Create("test.igz");
						ms.CopyTo(fs);
						fs.Close();
						ms.Seek(0, SeekOrigin.Begin);
#endif
						igFilePath fp = new igFilePath();
						fp.Set(target._path);
						igArchive arc = igFileContext.Singleton._archiveManager._patchArchives._count > 0 ? igFileContext.Singleton._archiveManager._patchArchives[0] : (igArchive)target._fd._device;
						arc.GetAddFile(fp._path);
						arc.Compress(fp._path, ms);
						ms.Close();
						if(arc._path[1] == ':') arc.Save(arc._path);
						else arc.Save($"{igFileContext.Singleton._root}/archives/{Path.GetFileName(arc._path)}");
					}
					else if(ImGui.MenuItem("New IGZ"))
					{
						_wnd._frames.Add(new DirectoryCreatorFrame(_wnd));
					}
					else if(ImGui.MenuItem("Duplicate"))
					{
						_wnd._frames.Add(new DirectoryDuplicatorFrame(_wnd, DirectoryManagerFrame._instance.CurrentDir));
					}
					ImGui.EndMenu();
				}
				if(ImGui.BeginMenu("Developer"))
				{
					if(ImGui.MenuItem("Dump Class"))
					{
						_wnd._frames.Add(new DumpClassFrame(_wnd));
					}
					else if (ImGui.MenuItem("Open ImGui Demo"))
					{
						_wnd._frames.Add(new DemoWindowFrame(_wnd));
					}
					ImGui.EndMenu();
				}
				ImGui.EndMainMenuBar();
			}
		}
	}
}