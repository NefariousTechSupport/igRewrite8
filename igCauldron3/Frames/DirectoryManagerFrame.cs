using System.Reflection;
using igLibrary.Core;
using ImGuiNET;

namespace igCauldron3
{
	public class DirectoryManagerFrame : Frame
	{
		public static DirectoryManagerFrame _instance { get; private set; }

		private static List<InspectorDrawOverride> _overrides = null!;
		public igObjectDirectoryList _dirs = new igObjectDirectoryList();
		private int _dirIndex = 0;
		public igObjectDirectory CurrentDir => _dirs[_dirIndex];

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

			_dirs.Append(igObjectStreamManager.Singleton.Load("LooseData/GuiSystemData.igz")!);
		}
		public void AddDirectory(igObjectDirectory dir)
		{
			_dirIndex = _dirs._count;
			_dirs.Append(dir);
		}
		public override void Render()
		{
			ImGui.Begin("Directory Manager");
			if(ImGui.BeginTabBar("directory tabs"))
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
						RenderDirectory(_dirs[i]);
						ImGui.EndTabItem();
					}
					if(tabOpen == false)
					{
						Console.WriteLine($"Remove {i}");
					}
				}
				ImGui.EndTabBar();
			}
			ImGui.End();
			base.Render();
		}
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
					RenderObject(name, dir._objectList[i]);
				}
				ImGui.TreePop();
			}
		}
		public void RenderObject(string id, igObject? obj)
		{
			if(obj == null)
			{
				ImGui.Text("null");
				return;
			}

			//TODO: ADD METAFIELD AND METAOBJECT CASES

			igMetaObject meta = obj.GetMeta();
			if(ImGui.TreeNode(id, meta._name))
			{
				int overrideIndex = _overrides.FindIndex(x => meta._vTablePointer.IsAssignableTo(x._t));
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
		private void RenderObjectFields(string id, igObject obj, igMetaObject meta)
		{
			for(int i = 0; i < meta._metaFields.Count; i++)
			{
				FieldInfo fi = meta._metaFields[i]._fieldHandle!;
				object? raw = fi.GetValue(obj);
				FieldRenderer.RenderField(id, meta._metaFields[i]._fieldName!, raw, meta._metaFields[i], (value) => fi.SetValue(obj, value));
			}
		}
	}
}