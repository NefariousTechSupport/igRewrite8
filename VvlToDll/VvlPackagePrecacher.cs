using System.Text;
using igLibrary.Core;
using igLibrary.Gfx;

namespace igLibrary
{
	public class VvlPackagePrecacher : CManager
	{
		public igVector<string> _packages;
		public igVector<igObjectDirectoryList> mObjectDirectoryLists;
		public static VvlPackagePrecacher _Instance = new VvlPackagePrecacher();

		public override void Intialize()
		{
			_packages = new igVector<string>();
			_packages.SetCapacity((int)EMemoryPoolID.MP_MAX_POOL);
			mObjectDirectoryLists = new igVector<igObjectDirectoryList>();
			mObjectDirectoryLists.SetCapacity((int)EMemoryPoolID.MP_MAX_POOL);

		}
		public bool IsPackageCached(string packageName)
		{
			string packagePathToCheck = packageName.ToLower();
			return _packages.Contains(packagePathToCheck);
		}
		public bool PrecachePackage(string packageName)
		{
			if(IsPackageCached(packageName)) return true;

			string packagePath = packageName.ToLower();

			if(!packagePath.StartsWith("packages"))
			{
				packagePath = "packages/" + packagePath;
			}
			if(!packagePath.EndsWith("_pkg.igz"))
			{
				packagePath += "_pkg.igz";
			}

			CArchive.Open(Path.GetFileNameWithoutExtension(packagePath.ReplaceEnd("_pkg.igz", "")), out igArchive? arc, EMemoryPoolID.MP_TEMPORARY, 0);

			igObjectDirectory? pkgDir = igObjectStreamManager.Singleton.Load(packagePath);
			if(pkgDir == null)
			{
				CArchive.Close(arc);
				return false;
			}
			igStringRefList list = (igStringRefList)pkgDir._objectList[0];
			CleanupDeadRules();
			for(int i = 0; i < list._count; i += 2)
			{
				string type = list[i];
				string file = list[i+1];
				if(type == "pkg")
				{
					PrecachePackage(file);
				}
				else if(type == "script")
				{
					igObjectStreamManager.Singleton.Load(file);
				}
			}
			return true;
		}
		public void CleanupDeadRules()
		{

		}
	}
}