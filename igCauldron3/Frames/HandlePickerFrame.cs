using igLibrary.Core;
using ImGuiNET;

namespace igCauldron3
{
	public class HandlePickerFrame : Frame
	{
		private List<igHandle> _orderedHandles;
		private List<igHandle> _searchedHandles;
		private Action<igHandle?> _selectedCb;
		private igMetaObject _metaObject;
		private string _searchTerm = "";
		
		public HandlePickerFrame(Window wnd, igMetaObject metaObject, Action<igHandle?> selectedCallback) : base(wnd)
		{
			_selectedCb = selectedCallback;
			_metaObject = metaObject;
			IEnumerable<igHandle> unorderedHnds = igObjectHandleManager.Singleton._objectToHandleTable.Values;
			_orderedHandles = new List<igHandle>(unorderedHnds.Count());
			for(int i = 0; i < unorderedHnds.Count(); i++)
			{
				igObject? alias = unorderedHnds.ElementAt(i).GetObjectAlias<igObject>();
				if(alias == null) continue;
				if(alias.GetMeta().CanBeAssignedTo(metaObject))
				{
					_orderedHandles.Add(unorderedHnds.ElementAt(i));
				}
			}
			_orderedHandles = _orderedHandles.OrderBy(x => x.ToString()).ToList();
			_searchedHandles = _orderedHandles;
		}

		public override void Render()
		{
			ImGui.Begin("Select Handle");

			if(ImGui.Button("Use NullHandle"))
			{
				_selectedCb.Invoke(null);
				Close();
			}

			ImGui.PushID("search box id");
			bool refreshSearch = ImGui.InputTextWithHint("", "Search", ref _searchTerm, 0x100);
			ImGui.PopID();

			if(refreshSearch)
			{
				if(string.IsNullOrWhiteSpace(_searchTerm))
				{
					_searchedHandles = _orderedHandles;
				}
				else
				{
					_searchedHandles = _orderedHandles.Where(x => x.ToString().Contains(_searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
				}
			}

			for(int i = 0; i < _searchedHandles.Count; i++)
			{
				ImGui.PushID(_searchedHandles[i].ToString());
				bool objectSelected = ImGui.Button(_searchedHandles[i].ToString());
				ImGui.PopID();
				if(objectSelected)
				{
					_selectedCb.Invoke(_searchedHandles[i]);
					Close();
				}
			}

			ImGui.End();
		}
	}
}