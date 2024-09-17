using igLibrary.Gfx;

namespace igLibrary
{
	public class COtherPackagePrecacher : CResourcePrecacher
	{
		public override void Precache(string filePath)
		{
			//Something with CResourcePrecacherDestinationPoolScope
			CPrecacheManager._Instance.PrecachePackage(filePath.ReplaceEnd("_pkg.igz", ""), mDestMemoryPoolId);
		}
	}
}