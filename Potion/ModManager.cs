using igLibrary.Core;

namespace Potion
{
	public class ModManager
	{
		private BlobManager _blobManager;
		private List<InstalledMod> _installedMods = new List<InstalledMod>();

		public BlobManager Blobs => _blobManager;


		private ModManager(BlobManager blobManager)
		{
			_blobManager = blobManager;
		}


		public static ModManager? Construct(igArkCore.EGame game, string modDirectory)
		{
			BlobManager? blobManager = BlobManager.Construct(Path.Combine(modDirectory, "blob"));

			if (blobManager == null)
			{
				return null;
			}

			ModManager modManager = new ModManager(blobManager);

			return modManager;
		}
	}
}