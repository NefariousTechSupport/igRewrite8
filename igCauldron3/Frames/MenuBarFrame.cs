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
			DirectoryManagerFrame? dirManager = DirectoryManagerFrame._instance;
			igObjectDirectory? currentDirectory = null;
			if (dirManager != null)
			{
				currentDirectory = dirManager.CurrentDir;
			}

			if(ImGui.BeginMainMenuBar())
			{
				if(ImGui.BeginMenu("File", dirManager != null))
				{
					if(ImGui.MenuItem("Open"))
					{
						_wnd._frames.Add(new DirectoryOpenerFrame(_wnd));
					}
					if(ImGui.BeginMenu("Save", currentDirectory != null))
					{
						igObjectDirectory target = currentDirectory!;
						igStorageDevice device = target._fd._device;
						igArchive? archive = device as igArchive;
						string displayName;
						bool isPatchArchive = false;

						if (archive != null)
						{
							displayName = Path.GetFileName(archive._path);

							// Dumb long line
							if (igFileContext.Singleton._archiveManager._patchArchives.Contains(archive))
							{
								isPatchArchive = true;
							}
						}
						else
						{
							displayName = device.GetType().Name;
						}


						// tracks user's selection
						// 0: nothing selected
						// 1: selected base game pack
						// 2: selected update.pak
						byte state = 0;
						if (ImGui.MenuItem(string.Format("To {0}", displayName)))
						{
							state = 1;
						}
						if (!isPatchArchive && ImGui.MenuItem(string.Format("To update.pak")))
						{
							state = 2;
							archive = igFileContext.Singleton._archiveManager._patchArchives[0];
						}

						if (state != 0 && archive != null)
						{
							// Write to memory
							MemoryStream ms = new MemoryStream();
							target.WriteFile(ms, igRegistry.GetRegistry()._platform);
							ms.Seek(0, SeekOrigin.Begin);

#if DEBUG // Save to a local file for testing
							FileStream fs = File.Create("test.igz");
							ms.CopyTo(fs);
							fs.Close();
							ms.Seek(0, SeekOrigin.Begin);
#endif // DEBUG

							// Output to the archive
							igFilePath fp = new igFilePath();
							fp.Set(target._path);
							archive.GetAddFile(fp._path);
							archive.Compress(fp._path, ms);
							ms.Close();

							// Write out the archive
							if(archive._path[1] == ':')
							{
								archive.Save(archive._path);
							}
							else
							{
								archive.Save($"{igFileContext.Singleton._root}/archives/{Path.GetFileName(archive._path)}");
							}

							// This is bad but will be fixed when the vfs is refactored
							target._fd._device = archive;
						}

						ImGui.EndMenu();
					}
					if(ImGui.MenuItem("New IGZ"))
					{
						_wnd._frames.Add(new DirectoryCreatorFrame(_wnd));
					}
					if(ImGui.MenuItem("Duplicate", currentDirectory != null))
					{
						_wnd._frames.Add(new DirectoryDuplicatorFrame(_wnd, currentDirectory!));
					}
					ImGui.EndMenu();
				}
				if (ImGui.BeginMenu("Edit"))
				{
					if (ImGui.MenuItem("Settings"))
					{
						if (!_wnd._frames.Any(x => x is SettingsFrame))
						{
							_wnd._frames.Add(new SettingsFrame(_wnd));
						}
						else
						{
							ImGui.SetWindowFocus("Settings");
						}
					}
					ImGui.EndMenu();
				}
				if(ImGui.BeginMenu("Developer"))
				{
					if(ImGui.MenuItem("Dump Class", dirManager != null))
					{
						_wnd._frames.Add(new DumpClassFrame(_wnd));
					}
					else if (ImGui.MenuItem("Open ImGui Demo"))
					{
						_wnd._frames.Add(new DemoWindowFrame(_wnd));
					}
					else if (ImGui.MenuItem("Create Skylander"))
					{
						_wnd._frames.Add(new CreateSkylanderFrame(_wnd));
					}
					ImGui.EndMenu();
				}
				ImGui.EndMainMenuBar();
			}
		}
	}
}