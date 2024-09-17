using igCauldron3.Utils;
using igLibrary.Core;
using ImGuiNET;

namespace igCauldron3
{
	public class ObjectPickerFrame : Frame
	{
		private class FilteredDir
		{
			public igObjectDirectory _dir;
			public List<igObject> _objects = new List<igObject>();
			public List<igName> _names = new List<igName>();
			public FilteredDir(igObjectDirectory dir, igMetaObject filter)
			{
				if(filter == null) throw new ArgumentNullException("Filter cannot be null!");

				_dir = dir;
				for(int i = 0; i < dir._objectList._count; i++)
				{
					//Use metaobject stuff for this cos _vTablePointer isn't guaranteed to be assigned
					if(dir._objectList[i].GetMeta().CanBeAssignedTo(filter))
					{
						_objects.Add(dir._objectList[i]);
						_names.Add(dir._nameList![i]);
					}
				}
			}
		}
		private List<FilteredDir> _orderedDirs;
		private List<igObject> _curDirObjects;
		private List<string> _curDirNames;
		private Action<igObject?> _selectedCb;
		private igMetaObject _metaObject;
		private igObjectDirectory _dir;
		
		public ObjectPickerFrame(Window wnd, igObjectDirectory dir, igMetaObject metaObject, Action<igObject?> selectedCallback) : base(wnd)
		{
			_selectedCb = selectedCallback;
			_metaObject = metaObject;
			_dir = dir;
			IEnumerable<igObjectDirectory> unorderedDirs = igObjectStreamManager.Singleton._directoriesByPath.Values;
			IEnumerable<igObjectDirectory> orderedUnfilteredDirs = unorderedDirs.OrderBy(x => x._name._string);
			_orderedDirs = new List<FilteredDir>(orderedUnfilteredDirs.Count());
			for(int i = 0; i < orderedUnfilteredDirs.Count(); i++)
			{
				if(orderedUnfilteredDirs.ElementAt(i)._nameList == null) continue;
				FilteredDir filteredDir = new FilteredDir(unorderedDirs.ElementAt(i), metaObject);
				if(filteredDir._objects.Count == 0) continue;
				_orderedDirs.Add(filteredDir);
			}
			FieldTraversal.TraverseObjectDir(dir, metaObject, out _curDirObjects, out _curDirNames);
		}
		public override void Render()
		{
			ImGui.Begin("Select Object");

			if(ImGui.Button("Use null"))
			{
				_selectedCb.Invoke(null);
				Close();
			}

			string searchTerm = "";
			ImGui.PushID("search box id");
			bool refreshSearch = ImGui.InputTextWithHint("", "Search box would go here if it worked...", ref searchTerm, 0x100);
			ImGui.PopID();

			if(ImGui.TreeNode("this directory"))
			{
				for(int o = 0; o < _curDirObjects.Count; o++)
				{
					ImGui.PushID(o);
					bool objectSelected = ImGui.Button(_curDirNames[o]);
					ImGui.PopID();
					if(objectSelected)
					{
						_selectedCb.Invoke(_curDirObjects[o]);
						Close();
					}
				}
				ImGui.TreePop();
			}

			for(int i = 0; i < _orderedDirs.Count; i++)
			{
				FilteredDir filteredDir = _orderedDirs[i];
				igObjectDirectory dir = filteredDir._dir;

				ImGui.PushID(i);
				bool openDir = ImGui.TreeNode(dir._name._string);
				ImGui.PopID();
				if(openDir)
				{
					for(int o = 0; o < filteredDir._objects.Count; o++)
					{
						ImGui.PushID(dir._name._string + filteredDir._names[o]._string);
						bool objectSelected = ImGui.Button(filteredDir._names[o]._string);
						ImGui.PopID();
						if(objectSelected)
						{
							_selectedCb.Invoke(filteredDir._objects[o]);
							Close();
						}
					}
					ImGui.TreePop();
				}
			}

			ImGui.End();
		}
	}
}