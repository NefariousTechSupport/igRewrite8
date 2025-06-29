namespace Potion
{
	/// <summary>
	/// Readonly information about a file
	/// </summary>
	public struct FileProps
	{
		public string                   Name => mName;
		public FileAttributes           Attributes => mAttributes;



		private string                  mName;
		private FileAttributes          mAttributes;



		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="fileInfo">Dotnet <c>FileInfo</c> structure</param>
		public FileProps(FileInfo fileInfo)
		{
			mName       = fileInfo.Name;
			mAttributes = fileInfo.Attributes;
		}
	}
}