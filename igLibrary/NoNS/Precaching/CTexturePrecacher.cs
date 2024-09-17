using igLibrary.Gfx;

namespace igLibrary
{
	public class CTexturePrecacher : CResourcePrecacher
	{
		public static Action<igImage2>? _precacheTexture;
		public override void Precache(string filePath)
		{
			igFilePath fp = new igFilePath();
			fp.Set(filePath);
			fp._extension = "igz";
			fp.GeneratePath("");
			igObjectDirectory dir = igObjectStreamManager.Singleton.Load(fp.getNativePath());
			igImage2 image = (igImage2)dir._objectList[0];
			CPrecacheManager._Instance.mObjectDirectoryLists[(int)mDestMemoryPoolId].Append(dir);
			if(_precacheTexture != null)
			{
				_precacheTexture.Invoke(image);
			}
		}
	}
}