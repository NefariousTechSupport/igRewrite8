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
			if(igRegistry.GetRegistry()._engineType == EngineType.AlchemyLaboratory)
				_looseArchives.Add(igFileContext.Singleton.LoadArchive("app:/archives/loosefiles.pak"));

			if (igRegistry.GetRegistry()._engineType == EngineType.TfbTool)
			{
				_looseArchives.Add(igFileContext.Singleton.LoadArchive("app:/permanent/bootstrap.bld"));
				_looseArchives.Add(igFileContext.Singleton.LoadArchive("app:/misc/bootstrap_lang.bld"));
				_looseArchives.Add(igFileContext.Singleton.LoadArchive("app:/item/legal.bld"));
				_looseArchives.Add(igFileContext.Singleton.LoadArchive("app:/permanent/global.bld"));
				_looseArchives.Add(igFileContext.Singleton.LoadArchive("app:/character/init_setup.bld"));
			}
		}


		/// <summary>
		/// Render function for the frame
		/// </summary>
		public override void Render()
		{
			ImGui.Begin("igArchive Manager", ImGuiWindowFlags.HorizontalScrollbar);
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
				for(int i = 0; i < _allowedArchiveNames.Length; i++)
				{
					ImGui.PushID(_allowedArchivePaths[i]);
					bool full = ImGui.Button("Full");
					ImGui.SameLine();
					bool loose = false;
					
					if (igRegistry.GetRegistry()._engineType != EngineType.TfbTool)
					{
						loose = ImGui.Button("Loose");
					}
					ImGui.PopID();
					ImGui.SameLine();
					ImGui.Text(_allowedArchiveNames[i]);

					if(full && igRegistry.GetRegistry()._engineType != EngineType.TfbTool)
					{
						_isChoosingArchive = false;

						igArchive loaded = igFileContext.Singleton.LoadArchive(_allowedArchivePaths[i]);
						foreach (var file in loaded._files.Where(file => file._logicalName.EndsWith("_pkg.igz")))
						{
							CPrecacheManager._Instance.PrecachePackage(file._logicalName, EMemoryPoolID.MP_DEFAULT);
						}
					}
					else if(loose || full && igRegistry.GetRegistry()._engineType == EngineType.TfbTool)
					{
						_isChoosingArchive = false;

						_looseArchives.Add(igFileContext.Singleton.LoadArchive(_allowedArchivePaths[i]));
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
			
			var archivesPrefix = igRegistry.GetRegistry()._engineType == EngineType.AlchemyLaboratory ? "archives" : "";
			igFileContext.Singleton.FileList(Path.Combine(igFileContext.Singleton._root, archivesPrefix), out igStringRefList realFiles, igBlockingType.kMayBlock, igFileWorkItem.Priority.kPriorityNormal);
			var virtualFiles = new igStringRefList();
			if (igFileContext.Singleton._archiveManager._patchArchives._count > 0)
			{
				igFileContext.Singleton.FileList(igFileContext.Singleton._archiveManager._patchArchives[0]._path, out igStringRefList addedVirtualFiles, igBlockingType.kMayBlock, igFileWorkItem.Priority.kPriorityNormal);
				foreach (var virtualFile in addedVirtualFiles)
				{
					virtualFiles.Append(virtualFile);
				}
			}

			igStringRefList files = new igStringRefList();
			files.SetCapacity(realFiles._capacity + virtualFiles._count);
			for(int i = 0; i < realFiles._count; i++)
			{
				if (igRegistry.GetRegistry()._engineType == EngineType.AlchemyLaboratory) files.Append("app:/archives/" + Path.GetFileName(realFiles[i]));
				if (igRegistry.GetRegistry()._engineType == EngineType.TfbTool) files.Append("app:/" + Path.GetRelativePath(igFileContext.Singleton._root, realFiles[i]));
			}
			for(int i = 0; i < virtualFiles._count; i++)
			{
				files.Append(virtualFiles[i]);
			}

			List<string> allowedArchivePaths = new List<string>();
			List<string> allowedArchiveNames = new List<string>();
			List<string> acceptedExtensions = new List<string> { ".pak", ".arc", ".bld" };
			
			for(int i = 0; i < files._count; i++)
			{
				if(acceptedExtensions.Contains(Path.GetExtension(files[i])))
				{
					if(!archives.Any(x => Path.GetFileName(x._path).ToLower() == Path.GetFileName(files[i])))
					{
						allowedArchivePaths.Add(files[i]);
						allowedArchiveNames.Add(Path.GetFileName(files[i]));
					}
				}
			}

			//Not proud of this
			_allowedArchivePaths = allowedArchivePaths.OrderBy(x => Path.GetFileName(x)).ToArray();

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
				bool tfbBld = Path.GetExtension(archive._path) == ".bld";

				for(int i = 0; i < fileHeaders.Length; i++)
				{
					string logicalName = fileHeaders[i]._logicalName;
					string extension   = Path.GetExtension(logicalName);
					
					if (extension == ".igz"
					 || extension == ".lng"
					 || tfbBld)
					{
						if(ImGui.Button(logicalName))
						{
							igObjectDirectory? directory = null;
							if (tfbBld && igRegistry.GetRegistry()._engineType == EngineType.TfbTool)
							{
								directory = StreamContext.Singleton.Load(archive._path);
							}
							else
							{
								directory = igObjectStreamManager.Singleton.Load(logicalName)!;
							}

							DirectoryManagerFrame._instance.AddDirectory(directory);
						}
					}

				}
				ImGui.TreePop();
			}
		}
	}
}