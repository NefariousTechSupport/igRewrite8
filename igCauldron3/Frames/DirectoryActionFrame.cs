using System.Runtime.Serialization;
using igLibrary.Core;
using ImGuiNET;

namespace igCauldron3
{
	/// <summary>
	/// Base UI Frame for manipulating igObjectDirectories
	/// </summary>
	public abstract class DirectoryActionFrame : Frame
	{
		protected string _path = "";
		protected string? _errorMsg = null;
		private readonly string _title;
		private readonly string _action;


		/// <summary>
		/// Constructor for the frame
		/// </summary>
		/// <param name="wnd">Reference to the main window object</param>
		/// <param name="title">title of the window</param>
		/// <param name="action">string of the action button</param>
		public DirectoryActionFrame(Window wnd, string title, string action) : base(wnd)
		{
			_title = title;
			_action = action;
		}


		/// <summary>
		/// Render function for the frame
		/// </summary>
		public override void Render()
		{
			ImGui.Begin(_title, ImGuiWindowFlags.NoDocking);

			if(_errorMsg != null)
			{
				ImGui.TextColored(Styles._errorTxt, _errorMsg);
			}

			//Render path field and check for errors
			ImGui.Text("Path");
			ImGui.SameLine();
			bool pathErrored = string.IsNullOrWhiteSpace(_path) || _path.Contains(' ');
			if(pathErrored) ImGui.PushStyleColor(ImGuiCol.FrameBg, Styles._errorBg);
			ImGui.InputText(string.Empty, ref _path, 0x100);
			if(pathErrored) ImGui.PopStyleColor();

			if(ImGui.Button(_action) && !pathErrored)
			{
				OnActionStart();
			}
			if(ImGui.Button("Close")) Close();
			ImGui.End();
		}

		protected abstract void OnActionStart();
	}
}