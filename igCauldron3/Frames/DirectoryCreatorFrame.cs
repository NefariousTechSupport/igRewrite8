using System.Runtime.Serialization;
using igLibrary.Core;
using ImGuiNET;

namespace igCauldron3
{
	/// <summary>
	/// UI Frame for creating a new igObjectDirectory
	/// </summary>
	public sealed class DirectoryCreatorFrame : DirectoryActionFrame
	{
		private string _path = "";


		/// <summary>
		/// Constructor for the frame
		/// </summary>
		/// <param name="wnd">Reference to the main window object</param>
		public DirectoryCreatorFrame(Window wnd) : base(wnd, "New Directory", "Create"){}


		/// <summary>
		/// Callback function when the confirmation button is pressed
		/// </summary>
		protected override void OnActionStart()
		{
			igObjectDirectory newDir = new igObjectDirectory(_path, new igName(Path.GetFileNameWithoutExtension(_path)));
			newDir._nameList = new igNameList();
			newDir._useNameList = true;

			igObjectStreamManager.Singleton.AddObjectDirectory(newDir, _path);

			DirectoryManagerFrame._instance.AddDirectory(newDir);
		}
	}
}