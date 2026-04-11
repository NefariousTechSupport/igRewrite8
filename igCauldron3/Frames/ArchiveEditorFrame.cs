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
using igCauldron3.Utils;
using System.Xml;
using System.Configuration.Provider;

namespace igCauldron3
{
	/// <summary>
	/// UI Frame for viewing and editing archives, a la igArchiveExtractor
	/// </summary>
	public sealed class ArchiveEditorFrame : Frame
	{
		private igArchive _target;
		private FileUiNode _rootNode;
		private FileUiNode? _renamingNode;
		private FileUiNode? _selectedNode;
		private FileUiNode? _deletionNode;
		private string? _loadErrorText;
		private string _renameBuffer;


		/// <summary>
		/// Cached data representing a treenode
		/// </summary>
		private sealed class FileUiNode
		{
			public SortedSet<FileUiNode> _children;
			public string _name;
			public string _displayName;
			public igArchive.FileInfo? _archiveFileInfo;
			public FileUiNode? _parent;

			public bool IsFile => _archiveFileInfo != null;


			/// <summary>
			/// Constructor for a file (not folder) node
			/// </summary>
			/// <param name="fileInfo">the igArchive file information</param>
			/// <param name="parent">The parent node</param>
			public FileUiNode(igArchive.FileInfo fileInfo, FileUiNode? parent)
			{
				_children = new SortedSet<FileUiNode>(new FileUiNodeComparer());
				_name = Path.GetFileName(fileInfo._logicalName);
				_displayName = _name;
				_archiveFileInfo = fileInfo;
				_parent = parent;
			}


			/// <summary>
			/// Constructor for a folder (not file) node
			/// </summary>
			/// <param name="name">The directory name</param>
			/// <param name="parent">The parent node</param>
			public FileUiNode(string name, FileUiNode? parent)
			{
				_children = new SortedSet<FileUiNode>(new FileUiNodeComparer());
				_name = name;
				_displayName = _name  + '/';
				_archiveFileInfo = null;
				_parent = parent;
			}


			/// <summary>
			/// Add a new child node
			/// </summary>
			/// <param name="child"></param>
			/// <returns>The added node</returns>
			public FileUiNode AddChild(FileUiNode child)
			{
				_children.Add(child);
				return child;
			}


			/// <summary>
			/// Attempt to recursively get a node
			/// </summary>
			/// <param name="name">the node to query for</param>
			/// <param name="node">the output node</param>
			/// <returns>Whether the node was found</returns>
			public bool TryGetNode(string name, out FileUiNode? node)
			{
				string[] parts = name.TrimEnd('\\', '/').Split('/', '\\');
				FileUiNode? currentNode = this;
				for (int p = 0; p < parts.Length; p++)
				{
					currentNode = currentNode._children.FirstOrDefault(x => x._name == parts[p]);

					if (currentNode == null)
					{
						break;
					}
				}

				node = currentNode;

				return currentNode != null;
			}


			/// <summary>
			/// Unlinks (deletes) the node and its children from the ui and the igArchive
			/// </summary>
			/// <param name="target">The target igArchive to unlink from</param>
			public void Unlink(igArchive target)
			{
				if (_parent != null)
				{
					_parent._children.Remove(this);
					_parent = null;
				}

				if (_archiveFileInfo != null)
				{
					target.Unlink(_archiveFileInfo);
					_archiveFileInfo = null;
				}

				// copy the children before modifying the set
				FileUiNode[] children = _children.ToArray();
				foreach (FileUiNode child in children)
				{
					child.Unlink(target);
				}
			}


			/// <summary>
			/// Get the absolute path of this node
			/// </summary>
			/// <returns>The absolute path</returns>
			public string GetFullPath()
			{
				return GetFullPathInternal().Replace('\\', '/');
			}


			/// <summary>
			/// Get the absolute path of this node, unsanitised
			/// </summary>
			/// <returns>The unsanitised absolute path of this node</returns>
			private string GetFullPathInternal()
			{
				if (_parent == null)
				{
					return string.Empty;
				}

				return Path.Combine(_parent.GetFullPathInternal(), _name);
			}
		}


		/// <summary>
		/// Alphabetical comparer of this node
		/// </summary>
		private class FileUiNodeComparer : IComparer<FileUiNode>
		{
			public int Compare(FileUiNode? x, FileUiNode? y)
			{
				if (x != null && y != null)
				{
					return x._name.CompareTo(y._name);
				}
				if (y != null)
				{
					return -1;
				}
				if (x != null)
				{
					return 1;
				}
				return 0;
			}
		}


		/// <summary>
		/// Constructor for the frame
		/// </summary>
		/// <param name="wnd">Reference to the main window object</param>
		/// <param name="target">The igArchive to experiment on</param>
		public ArchiveEditorFrame(Window wnd, igArchive target) : base(wnd)
		{
			_target = target;

			_rootNode = new FileUiNode("$root$", null);
			_rootNode._displayName = "/";

			for (int f = 0; f < _target._files.Count; f++)
			{
				AddNode(_target._files[f]);
			}

			_renameBuffer = string.Empty;
			_renamingNode = null;
		}


		/// <summary>
		/// Adds a node
		/// </summary>
		/// <param name="file">The file to construct a node for</param>
		private FileUiNode AddNode(igArchive.FileInfo file)
		{
			string[] parts = file._logicalName.Split('/', '\\');
			FileUiNode node = _rootNode;
			for (int p = 0; p < parts.Length - 1; p++)
			{
				if (!node.TryGetNode(parts[p], out FileUiNode? child))
				{
					child = new FileUiNode(parts[p], node);
					node.AddChild(child);
				}

				node = child!;
			}

			return node.AddChild(new FileUiNode(file, node));
		}


		/// <summary>
		/// Adds a node
		/// </summary>
		/// <param name="dirName">The directory to construct a node for</param>
		private FileUiNode AddNode(string dirName)
		{
			string[] parts = dirName.TrimEnd('\\', '/').Split('/', '\\');
			FileUiNode node = _rootNode;
			for (int p = 0; p < parts.Length; p++)
			{
				if (!node.TryGetNode(parts[p], out FileUiNode? child))
				{
					child = new FileUiNode(parts[p], node);
					node.AddChild(child);
				}

				node = child!;
			}

			return node;
		}


		/// <summary>
		/// Render function for the frame
		/// </summary>
		public override void Render()
		{
			ImGui.Begin("igArchive editor", ImGuiWindowFlags.MenuBar);
			if (ImGui.BeginMenuBar())
			{
				if (ImGui.BeginMenu("File"))
				{
					if (ImGui.MenuItem("Open..."))
					{
						string input = CrossFileDialog.OpenFile("Open Archive", ".pak;.bld;.arc");
						if (!string.IsNullOrEmpty(input))
						{
							bool success = false;
							try
							{
								igArchive archive = igFileContext.Singleton.LoadArchive(input);
								_target = archive;
								success = true;
							}
							catch (Exception e)
							{
								_loadErrorText = "Failed to load igArchive with the following error:\n\n" + e.Message;
							}

							if (success)
							{
								// repopulate ui
								ClearNodes();
								for (int f = 0; f < _target._files.Count; f++)
								{
									AddNode(_target._files[f]);
								}
							}
						}
					}
					if (ImGui.MenuItem("Save as..."))
					{
						SaveAs("Ratchet.pak");
					}
					if (ImGui.MenuItem("Save"))
					{
						
					}

					ImGui.EndMenu();
				}

				ImGui.EndMenuBar();
			}

			const float maxFileWidth = 300;

			ImGuiStylePtr style = ImGui.GetStyle();
			System.Numerics.Vector2 availableX = System.Numerics.Vector2.UnitX * ImGui.GetContentRegionAvail().X;
			System.Numerics.Vector2 borderWidth = System.Numerics.Vector2.UnitX * style.ChildBorderSize * 2;
			System.Numerics.Vector2 padding = System.Numerics.Vector2.UnitX * style.FramePadding.X;

			System.Numerics.Vector2 mainSize = availableX * 0.60f - borderWidth - padding;
			// only needs to be an estimate
			float fileWidthEstimate = availableX.X - mainSize.X;
			if (fileWidthEstimate > maxFileWidth)
			{
				mainSize.X = ImGui.GetContentRegionAvail().X - maxFileWidth;
			}

			ImGui.BeginChild("Archive", mainSize, true, ImGuiWindowFlags.HorizontalScrollbar);

			RenderNodes(_rootNode);

			ImGui.EndChild();

			ImGui.SameLine();

			ImGui.BeginChild("Selected", System.Numerics.Vector2.Zero, true, ImGuiWindowFlags.HorizontalScrollbar);
			if (_selectedNode != null && _selectedNode._archiveFileInfo != null)
			{
				igArchive.FileInfo fileInfo = _selectedNode._archiveFileInfo;
				ImGui.Text("Name:");
				ImGui.Text(_selectedNode._displayName);
				ImGui.NewLine();
				ImGui.Text("Size:");
				ImGui.Text(fileInfo._length.ToString());
				ImGui.NewLine();
				ImGui.Text("Hash:");
				ImGui.Text(fileInfo._hash.ToString("X08"));
				ImGui.NewLine();
				ImGui.Text("Ordinal:");
				ImGui.Text(fileInfo._ordinal.ToString());
				ImGui.NewLine();
				ImGui.Text("Last Modified:");
				ImGui.Text(fileInfo.HasModTime ? fileInfo.FriendlyModTime.ToLocalTime().ToString() : "N/A");
				ImGui.NewLine();

				ImGui.SetCursorPosY(ImGui.GetContentRegionMax().Y - ImGui.GetFontSize() - style.FramePadding.Y * 2);
				bool extract = ImGui.Button("Extract", System.Numerics.Vector2.UnitX * ImGui.GetContentRegionAvail().X * 0.5f - padding);
				ImGui.SameLine();
				bool replace = ImGui.Button("Replace", System.Numerics.Vector2.UnitX * ImGui.GetContentRegionAvail().X);
				if (extract)
				{
					string output = CrossFileDialog.SaveFile("Save To", string.Empty, Path.GetFileName(fileInfo._logicalName));
					if (!string.IsNullOrEmpty(output))
					{
						FileStream ofs = File.Create(output, 0x8000);
						_target.Decompress(fileInfo, ofs);
						ofs.Close();
					}
				}
				if (replace)
				{
					string input = CrossFileDialog.OpenFile("Choose File", string.Empty, Path.GetFileName(fileInfo._logicalName));
					if (!string.IsNullOrEmpty(input))
					{
						FileStream ifs = File.OpenRead(input);
						_target.Compress(fileInfo, ifs);
						ifs.Close();
						fileInfo.FriendlyModTime = DateTime.UtcNow;
					}
				}
			}
			ImGui.EndChild();

			if (_deletionNode != null)
			{
				ImGui.OpenPopup("Delete?");

				System.Numerics.Vector2 centre = ImGui.GetMainViewport().GetCenter();
				ImGui.SetNextWindowPos(centre, ImGuiCond.Appearing, System.Numerics.Vector2.One * 0.5f);
				bool temp = true;
				if (ImGui.BeginPopupModal("Delete?", ref temp, ImGuiWindowFlags.AlwaysAutoResize))
				{
					ImGui.Text($"Are you sure you want to delete this {(_deletionNode.IsFile ? "file" : "folder")}?");
					if (!_deletionNode.IsFile)
					{
						ImGui.Text("Doing so would also delete all its children");
					}

					if (ImGui.Button("Yes"))
					{
						// rip bozo
						_deletionNode.Unlink(_target);
						_deletionNode = null;

						ImGui.CloseCurrentPopup();
					}
					ImGui.SameLine();
					if (ImGui.Button("No"))
					{
						_deletionNode = null;
						ImGui.CloseCurrentPopup();
					}

					ImGui.EndPopup();
				}
			}

			if (_loadErrorText != null)
			{
				ImGui.OpenPopup("Load Error");

				System.Numerics.Vector2 centre = ImGui.GetMainViewport().GetCenter();
				ImGui.SetNextWindowPos(centre, ImGuiCond.Appearing, System.Numerics.Vector2.One * 0.5f);
				bool temp = true;
				if (ImGui.BeginPopupModal("Load Error", ref temp, ImGuiWindowFlags.AlwaysAutoResize))
				{
					ImGui.Text(_loadErrorText);

					if (ImGui.Button("Copy to Clipboard"))
					{
						ImGui.SetClipboardText(_loadErrorText);
					}
					ImGui.SameLine();
					if (ImGui.Button("OK"))
					{
						_loadErrorText = null;
						ImGui.CloseCurrentPopup();
					}

					ImGui.EndPopup();
				}
			}

			ImGui.DockSpaceOverViewport();
		}


		/// <summary>
		/// Render a node
		/// </summary>
		/// <param name="node">The node to render</param>
		private void RenderNodes(FileUiNode node)
		{
			ImGuiTreeNodeFlags flags = node.IsFile ? ImGuiTreeNodeFlags.Leaf : ImGuiTreeNodeFlags.NavLeftJumpsBackHere;

			bool beingRenamed = _renamingNode == node;

			// select the node if it's not being renamed and it is selected
			flags |= (_selectedNode == node && !beingRenamed) ? ImGuiTreeNodeFlags.Selected : ImGuiTreeNodeFlags.None;

			float startCursorX = ImGui.GetCursorPosX();

			// draw the treenode

			bool emptyFolder = !node.IsFile && node._children.Count == 0;
			// colour it red if it's empty and not being renamed
			if (emptyFolder && !beingRenamed && node != _rootNode)
			{
				ImGui.PushStyleColor(ImGuiCol.Text, Styles._errorTxt);
			}
			// no text if it's being renamed currently
			bool expanded = ImGui.TreeNodeEx(node._name, flags, beingRenamed ? string.Empty : node._displayName);
			// colour it red if it's empty and not being renamed
			if (emptyFolder && !beingRenamed && node != _rootNode)
			{
				ImGui.PopStyleColor();
			}
			// render tooltip
			if (emptyFolder && node != _rootNode && ImGui.IsItemHovered())
			{
				ImGui.SetTooltip("Empty folders are not allowed, either add files or delete this");
			}


			// selection logic

			if (node.IsFile && ImGui.IsItemClicked())
			{
				_selectedNode = node;
			}


			// renaming logic

			if (beingRenamed)
			{
				// position textbox correctly
				ImGui.SameLine();
				ImGui.SetCursorPosX(startCursorX + ImGui.GetTreeNodeToLabelSpacing());

				bool valid = true;

				// prevent invalid characters
				char[] invalidChars = Path.GetInvalidFileNameChars();
				for (int c = 0; c < invalidChars.Length; c++)
				{
					valid &= !_renameBuffer.Contains(invalidChars[c]);
				}

				// prevent 0 length name
				valid &= _renameBuffer.Length > 0;

				// prevent duplicate files/folders
				valid &= !node._parent!.TryGetNode(_renameBuffer, out FileUiNode? result) || result == node;

				// render red
				if (!valid)
				{
					ImGui.PushStyleColor(ImGuiCol.FrameBg, Styles._errorBg);
				}

				// draw textbox
				UIUtil.RenderTextField("", "$rename_field$", ref _renameBuffer);

				// render red
				if (!valid)
				{
					ImGui.PopStyleColor();
				}

				// submission logic
				if (valid && (ImGui.IsKeyPressed(ImGuiKey.Enter) || ImGui.IsKeyPressed(ImGuiKey.KeypadEnter)))
				{
					node._name = _renameBuffer;
					node._displayName = node._name + (node.IsFile ? string.Empty : "/");
					_renamingNode = null;

					RepopulateArchive(_target, _rootNode, string.Empty);
				}
				// escape logic
				if (ImGui.IsKeyPressed(ImGuiKey.Escape))
				{
					_renamingNode = null;
				}
			}


			// right-click menu

			if (ImGui.BeginPopupContextItem())
			{
				// renaming, disallow root node
				if (node != _rootNode && ImGui.Selectable("Rename"))
				{
					_renamingNode = node;
					_renameBuffer = node._name;

					// there's always going to be another option
					ImGui.Separator();
				}

				// folder only options
				if (!node.IsFile)
				{
					// create a file
					if (ImGui.Selectable("Create File"))
					{
						string input = CrossFileDialog.OpenFile("Choose File");
						if (!string.IsNullOrEmpty(input))
						{
							FileUiNode newFileNode = CreateUniqueNode(Path.Combine(node.GetFullPath(), Path.GetFileName(input)).Replace('\\', '/'));

							// This will add a new file
							igArchive.FileInfo fileInfo = _target.GetAddFile(newFileNode.GetFullPath());

							FileStream ifs = File.OpenRead(input);
							_target.Compress(fileInfo, ifs);
							ifs.Close();

							fileInfo.FriendlyModTime = DateTime.UtcNow;
							newFileNode._archiveFileInfo = fileInfo;
							newFileNode._displayName = newFileNode._name;
						}
					}
					if (ImGui.Selectable("Create Folder"))
					{
						FileUiNode dirNode = CreateUniqueNode(Path.Combine(node.GetFullPath(), "New Folder").Replace('\\', '/'));
						// prompt the user to rename it
						_renamingNode = dirNode;
						_renameBuffer = dirNode._name;
					}

					ImGui.Separator();
				}

				// never delete the root node
				if (ImGui.Selectable(node == _rootNode ? "Delete Children" : "Delete"))
				{
					_deletionNode = node;
				}

				ImGui.EndPopup();
			}

			// render children
			if (expanded)
			{
				foreach (FileUiNode child in node._children)
				{
					RenderNodes(child);
				}

				ImGui.TreePop();
			}
		}


		/// <summary>
		/// Save the node at the specified filepath
		/// </summary>
		/// <param name="filePath">The file path to save to</param>
		private void SaveAs(string filePath)
		{
			RepopulateArchive(_target, _rootNode, string.Empty);

			_target.Save(filePath);
		}


		/// <summary>
		/// Repopulate the archive's file listing
		/// </summary>
		/// <param name="target">The target igArchive</param>
		/// <param name="node">The node we're currently processing</param>
		/// <param name="nodepath">The current path of the node</param>
		private static void RepopulateArchive(igArchive target, FileUiNode node, string nodepath)
		{
			if (node._archiveFileInfo != null)
			{
				node._archiveFileInfo._logicalName = nodepath.Replace('\\', '/');
				node._archiveFileInfo._name = $"Temporary/BuildServer/{igAlchemyCore.GetPlatformString(igRegistry.GetRegistry()._platform)}/Output/{node._archiveFileInfo._logicalName}";

				node._archiveFileInfo._hash = igHash.Hash((target._archiveHeader._flags & 1) != 0 ? node._archiveFileInfo._logicalName.ToLower() : node._archiveFileInfo._logicalName);
			}

			foreach (FileUiNode child in node._children)
			{
				RepopulateArchive(target, child, Path.Combine(nodepath, child._name));
			}
		}


		/// <summary>
		/// Create a unique file node based on this (possibly existing) path
		/// </summary>
		/// <param name="thisFilePath">The (possibly existing) file path</param>
		/// <returns>The newly created file node</returns>
		private FileUiNode CreateUniqueNode(string thisFilePath)
		{
			_rootNode.TryGetNode(thisFilePath, out FileUiNode? existing);
			if (existing != null && existing._parent != null)
			{
				int i = 0;
				string fileName = Path.GetFileNameWithoutExtension(thisFilePath)!;
				string ext = Path.GetExtension(thisFilePath)!;
				bool preexisting;
				do
				{
					i++;
					preexisting = existing._parent.TryGetNode($"{fileName} ({i}){ext}", out FileUiNode? temp);
				} while (preexisting);

				existing = existing._parent.AddChild(new FileUiNode($"{fileName} ({i}){ext}", existing._parent));
			}
			else
			{
				existing = AddNode(thisFilePath);
			}

			return existing;
		}


		private void ClearNodes()
		{
			ClearNodesInternal(_rootNode);
		}


		private void ClearNodesInternal(FileUiNode node)
		{
			node._parent = null;

			foreach (FileUiNode child in node._children)
			{
				ClearNodesInternal(child);
			}

			node._children.Clear();
		}
	}
}