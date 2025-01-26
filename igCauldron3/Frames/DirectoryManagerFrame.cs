/*
	Copyright (c) 2022-2025, The igCauldron Contributors.
	igCauldron and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Reflection;
using igLibrary.Core;
using ImGuiNET;

namespace igCauldron3
{
	/// <summary>
	/// UI Frame for managing and editing the currently open <c>igObjectDirectory</c>s
	/// </summary>
	public class DirectoryManagerFrame : Frame
	{
		public static DirectoryManagerFrame _instance { get; private set; }

		private static List<InspectorDrawOverride> _overrides = null!;
		public igObjectDirectoryList _dirs = new igObjectDirectoryList();
		private int _dirIndex = 0;
		public igObjectDirectory CurrentDir => _dirs[_dirIndex];


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="wnd">The window to parent the frame to</param>
		/// <exception cref="InvalidOperationException">This class is effectively a singleton, only one can exist at once</exception>
		public DirectoryManagerFrame(Window wnd) : base(wnd)
		{
			if(_instance != null) throw new InvalidOperationException("DirectoryManagerFrame already created!");
			_instance = this;
			if(_overrides == null)
			{
				_overrides = new List<InspectorDrawOverride>();
				Type[] types = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsAssignableTo(typeof(InspectorDrawOverride))).ToArray();
				for(uint i = 0; i < types.Length; i++)
				{
					if(!types[i].IsAbstract)
					{
						InspectorDrawOverride drawOverride = (InspectorDrawOverride)Activator.CreateInstance(types[i])!;
						_overrides.Add(drawOverride!);
					}
				}
			}
		}


		/// <summary>
		/// Add a directory to the list of uh open directories
		/// </summary>
		/// <param name="dir">The directory</param>
		public void AddDirectory(igObjectDirectory dir)
		{
			int index = -1;
			for (int i = 0; i < _dirs._count; i++)
			{
				if (_dirs[i] == dir)
				{
					index = i;
					break;
				}
			}

			if (index < 0)
			{
				index = _dirs._count;
				_dirs.Append(dir);
			}

			_dirIndex = index;
		}


		/// <summary>
		/// Renders the ui
		/// </summary>
		public override void Render()
		{
			ImGui.Begin("Directory Manager", ImGuiWindowFlags.HorizontalScrollbar);
			if(ImGui.BeginTabBar("directory tabs", ImGuiTabBarFlags.FittingPolicyScroll))
			{
				for(int i = 0; i < _dirs._count; i++)
				{
					bool tabOpen = true;
					ImGui.PushID(i);
					bool tabSelected = ImGui.BeginTabItem(_dirs[i]._name._string, ref tabOpen);
					ImGui.PopID();
					if(tabSelected)
					{
						_dirIndex = i;
						ImGui.BeginChild("$directoryview$", default(System.Numerics.Vector2), false, ImGuiWindowFlags.HorizontalScrollbar);
						RenderDirectory(_dirs[i]);
						ImGui.EndChild();
						ImGui.EndTabItem();
					}
					if(tabOpen == false)
					{
						_dirs.Remove(i);
					}
				}
				ImGui.EndTabBar();
			}
			ImGui.End();
			base.Render();
		}


		/// <summary>
		/// Renders ui for a specific directory
		/// </summary>
		/// <param name="dir">the directory</param>
		private void RenderDirectory(igObjectDirectory dir)
		{
			if(ImGui.TreeNode("Objects"))
			{
				for(int i = 0; i < dir._objectList._count; i++)
				{
					string name;
					if(dir._useNameList) name = dir._nameList![i]._string;
					else                        name = $"Object {i}";

					ImGui.Text(name);
					ImGui.SameLine();
					RenderObject(i + name, dir._objectList[i]);
				}
				if(ImGui.Button("+"))
				{
					igObjectDirectory capturedDir = dir;
					_wnd._frames.Add(new CreateObjectFrame(_wnd, CurrentDir, igArkCore.GetObjectMeta("igObject")!, (obj, name) => {
						if(obj != null)
						{
							capturedDir.AddObject(obj, default(igName), name);
						}
					}));
				}
				ImGui.TreePop();
			}
		}


		/// <summary>
		/// Renders ui for a specific <c>igObject</c>
		/// </summary>
		/// <param name="id">The id to represent the object with in the ui</param>
		/// <param name="obj">The object</param>
		public void RenderObject(string id, igObject? obj)
		{
			if(obj == null)
			{
				ImGui.Text("null");
				return;
			}

			//TODO: add editing for these
			if(obj is igMetaObject mo)
			{
				ImGui.Text("metaobject." + mo._name);
				return;
			}
			else if(obj is igMetaField field)
			{
				ImGui.Text("metafield." + field._parentMeta._name + "::" + field._fieldName);
				return;
			}

			igMetaObject meta = obj.GetMeta();
			if(ImGui.TreeNode(id, meta._name))
			{
				int overrideIndex = _overrides.FindIndex(x => meta._vTablePointer!.IsAssignableTo(x._t));
				if(overrideIndex < 0)
				{
					RenderObjectFields(id, obj, meta);
				}
				else
				{
					_overrides[overrideIndex].Draw2(this, id, obj, meta);
				}
				ImGui.TreePop();
			}
		}


		/// <summary>
		/// Method to render the fields of an object
		/// </summary>
		/// <param name="id">The id to use when rendering the fields</param>
		/// <param name="obj">The object</param>
		/// <param name="meta">The type of the object</param>
		private void RenderObjectFields(string id, igObject obj, igMetaObject meta)
		{
			for(int i = 0; i < meta._metaFields.Count; i++)
			{
				if(meta._metaFields[i] is igStaticMetaField) continue;
				if(meta._metaFields[i] is igPropertyFieldMetaField) continue;
				FieldInfo fi = meta._metaFields[i]._fieldHandle!;
				object? raw = fi.GetValue(obj);
				FieldRenderer.RenderField(id, meta._metaFields[i]._fieldName!, raw, meta._metaFields[i], (value) => fi.SetValue(obj, value));
			}
		}
	}
}