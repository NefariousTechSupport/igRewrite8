using igLibrary.DotNet;

namespace igLibrary
{
	public class CDotNetaManager : CManager
	{
		//public igObjectDirectoryLoadCallback _objectDirectoryUnloadedCallback;
		//public igUnsignedIntStringHashTable _functionHashes;
		//public CDotNetLibraryHashTable _libraries;
		public Dictionary<string, DotNetLibrary> _libraries = new Dictionary<string, DotNetLibrary>();
		public DotNetRuntime _runtime = new DotNetRuntime();
		//public igObject _dotNetCommunicator;
		//public bool _debuggerEnabled;
		public static CDotNetaManager _Instance;
		public bool IsLibraryLoaded(string filePath)
		{
			if(filePath == null) return false;
			string libName = FixLibraryFileName(filePath);
			return _libraries.ContainsKey(libName);
		}
		private string FixLibraryFileName(string fileName)
		{
			string fixedFileName = "";
			if(!fileName.Contains('/') && !fileName.Contains('\\'))
			{
				fixedFileName = "scripts\\";
			}
			fixedFileName += fileName.Replace('/', '\\').ToLower();
			fixedFileName.ReplaceBeginning("scripts:", "scripts");
			int extensionIndex = fixedFileName.IndexOf('.');
			if(extensionIndex >= 0)
			{
				fixedFileName = fixedFileName.Substring(0, extensionIndex);
			}
			fixedFileName += ".vvl";
			return fixedFileName;
		}
		public DotNetLibrary LoadScript(string fileName, EMemoryPoolID poolId, bool alwaysLoad)
		{
			return LoadScriptInternal(fileName, alwaysLoad, out bool succeeded);
		}
		private DotNetLibrary LoadScriptInternal(string fileName, bool alwaysLoad, out bool succeeded)
		{
			string libName = FixLibraryFileName(fileName);
			DotNetLibrary? lib = null;
			if(!alwaysLoad)
			{
				if(_libraries.TryGetValue(libName, out lib))
				{
					succeeded = true;
					return lib;
				}
			}
			lib = VvlLoader.Load(libName, _runtime, out succeeded);
			return lib;
		}
	}
}