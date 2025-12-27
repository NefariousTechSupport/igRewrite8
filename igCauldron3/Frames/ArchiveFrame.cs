/*
	Copyright (c) 2022-2025, The igCauldron Contributors.
	igCauldron and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using ImGuiNET;
using igLibrary.Core;
using igLibrary;
using igLibrary.Tfb.Game;

namespace igCauldron3
{
	/// <summary>
	/// UI Frame for viewing files to load
	/// </summary>
	public sealed class ArchiveFrame : Frame
	{
		private bool _isChoosingArchive = false;
		private string[]? _allowedArchivePaths = null;
		private string[]? _allowedArchiveNames = null;
		private string searchQuery = "";

		private List<igArchive> _looseArchives = new List<igArchive>();
		private Dictionary<string, igArchive.FileInfo[]> _sortedFileHeaders = new Dictionary<string, igArchive.FileInfo[]>();
		private Dictionary<string, PackageUiData> _packageUiData = new Dictionary<string, PackageUiData>();


		/// <summary>
		/// Cached data representing a package
		/// </summary>
		private struct PackageUiData
		{
			public igObjectDirectory _directory;
			public Dictionary<string, igStringRefList> _files;
		}


		/// <summary>
		/// Constructor for the frame
		/// </summary>
		/// <param name="wnd">Reference to the main window object</param>
		public ArchiveFrame(Window wnd) : base(wnd)
		{
			if (igRegistry.GetRegistry()._engineType == EngineType.AlchemyLaboratory)
			{
				_looseArchives.Add(igFileContext.Singleton.LoadArchive("app:/archives/loosefiles.pak"));
			}
		}


		/// <summary>
		/// Render function for the frame
		/// </summary>
		public override void Render()
		{
			ImGui.Begin("igArchive Manager", ImGuiWindowFlags.HorizontalScrollbar);

			EngineType engineType = igRegistry.GetRegistry()._engineType;

			if(!_isChoosingArchive)
			{
				igArchiveList archives = igFileContext.Singleton._archiveManager._archiveList;
				if(ImGui.Button("Add Archive"))
				{
					_isChoosingArchive = true;
					PopulateArchiveList();
				}
				for(int i = 0; i < _looseArchives.Count; i++)
				{
					RenderLooseArchive(_looseArchives[i]);
				}
				igVector<string> packages = CPrecacheManager._Instance._packagesPerPool[(int)EMemoryPoolID.MP_DEFAULT];
				for(int i = 0; i < packages._count; i++)
				{
					RenderPackage(packages[i]);
				}
				IReadOnlyDictionary<string, StreamContext.Streamable> streamables = StreamContext.Singleton.Streamables;
				foreach (var streamable in streamables)
				{
					RenderTfbStreamable(streamable.Key, streamable.Value);
				}
				searchQuery = "";
			}
			else
			{
				if(_allowedArchiveNames == null || _allowedArchivePaths == null)
				{
					return;
				}

				if(ImGui.Button("Cancel"))
				{
					_isChoosingArchive = false;
				}
				ImGui.SameLine();
				ImGui.InputText(string.Empty, ref searchQuery, 0x100);
				for (int i = 0; i < _allowedArchiveNames.Length; i++)
				{
					if (_allowedArchivePaths[i].ToLower().Contains(searchQuery.ToLower()))
					{
						ImGui.PushID(_allowedArchivePaths[i]);

						bool full = false;
						bool loose = false;
						if (engineType == EngineType.TfbTool)
						{
							full = ImGui.Button("Open");
						}
						else
						{
							full = ImGui.Button("Full");
							ImGui.SameLine();
							loose = ImGui.Button("Loose");
						}

						ImGui.PopID();

						ImGui.SameLine();
						ImGui.Text(_allowedArchiveNames[i]);

						if (full)
						{
							_isChoosingArchive = false;

							if (engineType == EngineType.TfbTool)
							{
								StreamContext.Singleton.Load(_allowedArchiveNames[i]);
							}
							else
							{
								igArchive loaded = igFileContext.Singleton.LoadArchive(_allowedArchivePaths[i]);
								for (int j = 0; j < loaded._files.Count; j++)
								{
									if (loaded._files[j]._logicalName.EndsWith("_pkg.igz"))
									{
										CPrecacheManager._Instance.PrecachePackage(loaded._files[j]._logicalName, EMemoryPoolID.MP_DEFAULT);
									}
								}
							}
						}
						else if (loose)
						{
							_isChoosingArchive = false;

							_looseArchives.Add(igFileContext.Singleton.LoadArchive(_allowedArchivePaths[i]));
						}
					}
				}
			}
			ImGui.End();
		}


		/// <summary>
		/// Renders the contents of a package file
		/// </summary>
		/// <param name="packageName">path to a package file</param>
		private void RenderPackage(string packageName)
		{
			bool expand = ImGui.TreeNode(packageName);
			if(ImGui.BeginPopupContextItem())
			{
				if(ImGui.Selectable("Edit"))
				{
					DirectoryManagerFrame._instance.AddDirectory(igObjectStreamManager.Singleton.Load(packageName)!);
				}
				ImGui.EndPopup();
			}
			if(expand)
			{
				PackageUiData uiData = GetOrCreatePackageUi(packageName);

				foreach(KeyValuePair<string, igStringRefList> kvp in uiData._files)
				{
					if(ImGui.TreeNode(kvp.Key))
					{
						for(int i = 0; i < kvp.Value._count; i++)
						{
							if(ImGui.Button(kvp.Value[i]))
							{
								DirectoryManagerFrame._instance.AddDirectory(igObjectStreamManager.Singleton.Load(kvp.Value[i])!);
							}
						}

						ImGui.TreePop();
					}
				}

				ImGui.TreePop();
			}
			else
			{
				if(_packageUiData.ContainsKey(packageName))
				{
					_packageUiData.Remove(packageName);
				}
			}
		}


		/// <summary>
		/// Gets or creates the ui data for a package
		/// </summary>
		/// <param name="packageName">Patch to a package file</param>
		private PackageUiData GetOrCreatePackageUi(string packageName)
		{
			PackageUiData uiData;

			if(_packageUiData.TryGetValue(packageName, out uiData))
			{
				return uiData;
			}

			uiData._directory = igObjectStreamManager.Singleton.Load(packageName)!;
			uiData._files = new Dictionary<string, igStringRefList>();

			//Only show igObjectDirectories, the rest are hidden
			uiData._files.Add(               "pkg", new igStringRefList());
			uiData._files.Add(    "character_data", new igStringRefList());
			uiData._files.Add(         "actorskin", new igStringRefList());
			//uiData._files.Add(       "havokanimdb", new igStringRefList());
			//uiData._files.Add(    "havokrigidbody", new igStringRefList());
			//uiData._files.Add("havokphysicssystem", new igStringRefList());
			uiData._files.Add(           "texture", new igStringRefList());
			uiData._files.Add(            "effect", new igStringRefList());
			uiData._files.Add(            "shader", new igStringRefList());
			uiData._files.Add(        "motionpath", new igStringRefList());
			uiData._files.Add(          "igx_file", new igStringRefList());
			uiData._files.Add("material_instances", new igStringRefList());
			uiData._files.Add(      "igx_entities", new igStringRefList());
			uiData._files.Add(       "gui_project", new igStringRefList());
			uiData._files.Add(              "font", new igStringRefList());
			uiData._files.Add(         "lang_file", new igStringRefList());
			uiData._files.Add(         "spawnmesh", new igStringRefList());
			uiData._files.Add(             "model", new igStringRefList());
			uiData._files.Add(         "sky_model", new igStringRefList());
			//uiData._files.Add(          "behavior", new igStringRefList());
			uiData._files.Add("graphdata_behavior", new igStringRefList());
			uiData._files.Add(   "events_behavior", new igStringRefList());
			//uiData._files.Add(    "asset_behavior", new igStringRefList());
			//uiData._files.Add(      "hkb_behavior", new igStringRefList());
			//uiData._files.Add(     "hkc_character", new igStringRefList());
			//uiData._files.Add(           "navmesh", new igStringRefList());
			//uiData._files.Add(            "script", new igStringRefList());

			igStringRefList packageContent = (igStringRefList)uiData._directory._objectList[0];

			for(int i = 0; i < packageContent._count; i += 2)
			{
				string type = packageContent[i];
				string file = packageContent[i+1];

				if(!uiData._files.TryGetValue(type, out igStringRefList? output))
				{
					continue;
				}

				output.Append(file);
			}

			igStringRefList emptyPackages = new igStringRefList();
			foreach(KeyValuePair<string, igStringRefList> kvp in uiData._files)
			{
				if(kvp.Value._count == 0)
				{
					emptyPackages.Append(kvp.Key);
				}
			}

			for(int i = 0; i < emptyPackages._count; i++)
			{
				uiData._files.Remove(emptyPackages[i]);
			}

			_packageUiData.Add(packageName, uiData);

			return uiData;
		}


		/// <summary>
		/// Populates UI data for archives
		/// </summary>
		private void PopulateArchiveList()
		{
			igArchiveList archives = igFileContext.Singleton._archiveManager._archiveList;

			igStringRefList files = new igStringRefList();

			bool tfbTool = igRegistry.GetRegistry()._engineType == EngineType.TfbTool;
			string dirName = tfbTool ? "" : "archives";
			string rootName = Path.Combine(igFileContext.Singleton._root, dirName);

			igFileContext.Singleton.FileList(rootName, out igStringRefList realFiles, tfbTool, igBlockingType.kMayBlock, igFileWorkItem.Priority.kPriorityNormal);

			files.SetCapacity(realFiles._capacity);
			for(int i = 0; i < realFiles._count; i++)
			{
				files.Append(Path.Combine("app:/", dirName, Path.GetRelativePath(rootName, realFiles[i])).Replace('\\', '/'));
			}

			if (igFileContext.Singleton._archiveManager._patchArchives._count > 0)
			{
				igFileContext.Singleton.FileList(igFileContext.Singleton._archiveManager._patchArchives[0]._path, out igStringRefList virtualFiles, tfbTool, igBlockingType.kMayBlock, igFileWorkItem.Priority.kPriorityNormal);

				for(int i = 0; i < virtualFiles._count; i++)
				{
					files.Append(virtualFiles[i]);
				}
			}

			List<string> allowedArchivePaths = new List<string>();
			List<string> allowedArchiveNames = new List<string>();
			List<string> allowedExtensions = new List<string>();
			if (tfbTool)
			{
				allowedExtensions.Add(".bld");
			}
			else
			{
				allowedExtensions.Add(".pak");
			}

			for(int i = 0; i < files._count; i++)
			{
				string extension = Path.GetExtension(files[i]);
				if (allowedExtensions.Contains(extension))
				{
					if(!archives.Any(x => Path.GetFileName(x._path).ToLower() == Path.GetFileName(files[i])))
					{
						allowedArchivePaths.Add(files[i]);
						allowedArchiveNames.Add(Path.ChangeExtension(files[i], null));
					}
				} 
			}

			//Not proud of the fact these are being sorted twice
			allowedArchivePaths.Sort();
			_allowedArchivePaths = allowedArchivePaths.ToArray();

			allowedArchiveNames.Sort();
			_allowedArchiveNames = allowedArchiveNames.ToArray();
		}


		/// <summary>
		/// Function that renders the contents of an <c>igArchive</c>
		/// </summary>
		/// <param name="archive">The archive to render</param>
		private void RenderLooseArchive(igArchive archive)
		{
			igArchive.FileInfo[]? fileHeaders;
			if(!_sortedFileHeaders.TryGetValue(archive._path, out fileHeaders))
			{
				fileHeaders = archive._files.OrderBy(x => x._logicalName).ToArray();
				_sortedFileHeaders.Add(archive._path, fileHeaders);
			}
			if(ImGui.TreeNode(archive._path))
			{
				for(int i = 0; i < fileHeaders.Length; i++)
				{
					string logicalName = fileHeaders[i]._logicalName;
					string extension   = Path.GetExtension(logicalName);
					
					if (extension == ".igz"
					 || extension == ".lng")
					{
						if(ImGui.Button(logicalName))
						{
							igObjectDirectory? directory = igObjectStreamManager.Singleton.Load(logicalName)!;
							DirectoryManagerFrame._instance.AddDirectory(directory);
						}
					}

				}
				ImGui.TreePop();
			}
		}



		/// <summary>
		/// Renders a loose archive but tfb
		/// </summary>
		/// <param name="streamable">streamable asset</param>
		private void RenderTfbLooseArchive(StreamContext.Streamable streamable)
		{
			igArchive? archive = streamable._streamArchive;
			if (archive == null)
			{
				return;
			}

			igArchive.FileInfo[]? fileHeaders;
			if(!_sortedFileHeaders.TryGetValue(archive._path, out fileHeaders))
			{
				fileHeaders = archive._files.OrderBy(x => x._name).ToArray();
				_sortedFileHeaders.Add(archive._path, fileHeaders);
			}
			if(ImGui.TreeNode("streamable assets"))
			{
				for(int i = 0; i < fileHeaders.Length; i++)
				{
					string displayName = fileHeaders[i]._name;
					string extension   = Path.GetExtension(displayName);
					
					if (extension == ".igz")
					{
						if(ImGui.Button(displayName))
						{
							igObjectDirectory? directory = StreamContext.Singleton.StreamDirectory(streamable, fileHeaders[i]._hash);
							if (directory != null)
							{
								DirectoryManagerFrame._instance.AddDirectory(directory);
							}
						}
					}

				}
				ImGui.TreePop();
			}
		}



		/// <summary>
		/// Renders a tfb streamable asset
		/// </summary>
		/// <param name="name">name of the streamable</param>
		/// <param name="streamable">the streamable asset</param>
		private void RenderTfbStreamable(string name, StreamContext.Streamable streamable)
		{
			if (ImGui.TreeNode(name))
			{
				RenderTfbLooseArchive(streamable);

				if (ImGui.Button("level.bld"))
				{
					DirectoryManagerFrame._instance.AddDirectory(streamable._levelBundle);
				}

				ImGui.TreePop();
			}
		}
	}
}