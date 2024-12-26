using igLibrary;

namespace Potion
{
	public sealed class AlchemyLaboratoryMod : Mod
	{
		struct ModifiedFile
		{
			public Blob _blob;
			public string _path;
			public string _archive;
		}

		List<ModifiedFile> _files = new List<ModifiedFile>();


		public override void Apply(ModManager manager)
		{
			for (int i = 0; i < _files.Count; i++)
			{
				ModifiedFile file = _files[i];
				FileInfo? fi = manager.Blobs.BlobToFile(file._blob);

				if (fi == null)
				{
					Logging.Error("Failed to find file {0} in blobs ({1})", file._path, file._blob);
					return;
				}
			} 
		}
	}
}