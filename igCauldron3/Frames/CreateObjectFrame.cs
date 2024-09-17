using igLibrary.Core;
using ImGuiNET;

namespace igCauldron3
{
	public class CreateObjectFrame : Frame
	{
		private igObjectDirectory _dir;
		private string _name = "";
		private igMetaObject _meta = null;
		private igMetaObject _parentType;
		private string _memoryPoolName = "Default";
		private List<igMetaObject> _alphabeticalMetas;
		private Action<igObject?, igName>? _cbAddRoot;
		private Action<igObject?>? _cbAddNormal;

		public CreateObjectFrame(Window wnd, igObjectDirectory dir, igMetaObject parentType, Action<igObject?, igName> cb) : base(wnd)
		{
			if(wnd == null) throw new ArgumentNullException("wnd must not be null!");
			if(parentType == null) throw new ArgumentNullException("parentType must not be null!");
			if(dir == null) throw new ArgumentNullException("dir must not be null!");
			if(cb == null) throw new ArgumentNullException("Callback must not be null!");
			_parentType = parentType;
			_alphabeticalMetas = igArkCore.MetaObjects.Where(x => x.CanBeAssignedTo(parentType)).OrderBy(x => x._name).ToList();
			_dir = dir;
			_cbAddRoot = cb;
		}

		public CreateObjectFrame(Window wnd, igObjectDirectory dir, igMetaObject parentType, Action<igObject?> cb) : base(wnd)
		{
			if(wnd == null) throw new ArgumentNullException("wnd must not be null!");
			if(parentType == null) throw new ArgumentNullException("parentType must not be null!");
			if(dir == null) throw new ArgumentNullException("dir must not be null!");
			if(cb == null) throw new ArgumentNullException("Callback must not be null!");
			_parentType = parentType;
			_alphabeticalMetas = igArkCore.MetaObjects.Where(x => x.CanBeAssignedTo(parentType)).OrderBy(x => x._name).ToList();
			_dir = dir;
			_cbAddNormal = cb;
		}

		public override void Render()
		{
			ImGui.Begin("New Object", ImGuiWindowFlags.NoDocking);

			bool nameErrored = false;
			if(_cbAddRoot != null)
			{
				ImGui.Text("Name");
				ImGui.SameLine();
				nameErrored = string.IsNullOrWhiteSpace(_name);
				if(nameErrored) ImGui.PushStyleColor(ImGuiCol.FrameBg, Styles._errorBg);
				ImGui.InputText(string.Empty, ref _name, 0x100);
				if(nameErrored) ImGui.PopStyleColor();
			}

			ImGui.Text("Type");
			ImGui.SameLine();
			ImGui.PushID("Type");
			bool metaErrored = _meta == null;
			if(metaErrored) ImGui.PushStyleColor(ImGuiCol.FrameBg, Styles._errorBg);
			bool comboing = ImGui.BeginCombo(string.Empty, _meta == null ? "Select a type" : _meta._name);
			if(metaErrored) ImGui.PopStyleColor();
			if(comboing)
			{
				for(int i = 0; i < _alphabeticalMetas.Count; i++)
				{
					ImGui.PushID(i);
					if(ImGui.Selectable(_alphabeticalMetas[i]._name, _meta == _alphabeticalMetas[i]))
					{
						_meta = _alphabeticalMetas[i];
					}
					if(_meta == _alphabeticalMetas[i])
					{
						ImGui.SetItemDefaultFocus();
					}
					ImGui.PopID();
				}
				ImGui.EndCombo();
			}
			ImGui.PopID();

			ImGui.Text("Memory Pool");
			ImGui.SameLine();
			ImGui.PushID("Pool");
			if(ImGui.BeginCombo(string.Empty, _memoryPoolName))
			{
				foreach(KeyValuePair<string, igMemoryPool> pool in igMemoryContext.Singleton._pools)
				{
					ImGui.PushID(pool.Key);
					if(ImGui.Selectable(pool.Key))
					{
						_memoryPoolName = pool.Key;
					}
					if(_memoryPoolName == pool.Key)
					{
						ImGui.SetItemDefaultFocus();
					}
					ImGui.PopID();
				}
				ImGui.EndCombo();
			}
			ImGui.PopID();
			bool pressed = ImGui.Button("Create");
			if(pressed && !metaErrored && !nameErrored)
			{
				if(_cbAddRoot != null)
				{
					_cbAddRoot.Invoke(_meta!.ConstructInstance(igMemoryContext.Singleton._pools[_memoryPoolName]), new igName(_name));
				}
				else if(_cbAddNormal != null)
				{
					_cbAddNormal.Invoke(_meta!.ConstructInstance(igMemoryContext.Singleton._pools[_memoryPoolName]));
				}
				Close();
			}
			if(ImGui.Button("Close")) Close();
			ImGui.End();
		}
	}
}