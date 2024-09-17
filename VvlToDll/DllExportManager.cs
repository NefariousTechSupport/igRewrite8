using igLibrary.DotNet;
using igLibrary;

namespace VvlToDll
{
	public static class DllExportManager
	{
		public static Dictionary<DotNetLibrary, DllExporter> _libExporterLookup = new Dictionary<DotNetLibrary, DllExporter>();
		public static void ExportAllVvls(string outputDir)
		{
			_libExporterLookup.Clear();
			foreach(KeyValuePair<string, DotNetLibrary> kvp in CDotNetaManager._Instance._libraries)
			{
				_libExporterLookup.Add(kvp.Value, new DllExporter(kvp.Value));
			}
			foreach(KeyValuePair<DotNetLibrary, DllExporter> kvp in _libExporterLookup)
			{
				kvp.Value.CreateTypes();
				kvp.Value.DefineEnums();
			}
			foreach(KeyValuePair<DotNetLibrary, DllExporter> kvp in _libExporterLookup)
			{
				kvp.Value.DefineObjects();
			}
			foreach(KeyValuePair<DotNetLibrary, DllExporter> kvp in _libExporterLookup)
			{
				kvp.Value.DeclareMethods();
			}
			foreach(KeyValuePair<DotNetLibrary, DllExporter> kvp in _libExporterLookup)
			{
				kvp.Value.DefineMethods();
			}
			foreach(KeyValuePair<DotNetLibrary, DllExporter> kvp in _libExporterLookup)
			{
				kvp.Value.Finish(outputDir);
			}
		}
	}
}