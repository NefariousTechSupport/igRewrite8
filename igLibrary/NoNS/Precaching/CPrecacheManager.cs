/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Text;
using igLibrary.Gfx;

namespace igLibrary
{
	public class CPrecacheManager : CManager
	{
		public igVector<igVector<string>> _packagesPerPool;
		[Obsolete("This exists for the reflection system, do not use.")] public object? _asyncPackageRecords;               //CAsyncPackageRecordList
		[Obsolete("This exists for the reflection system, do not use.")] public object? _currentlyLoadingZone;              //CZone
		[Obsolete("This exists for the reflection system, do not use.")] public object? _asynchronousLoadingModelRecords;   //igVector<CAsynchronousLoadingModelRecord>
		public igVector<igObjectDirectoryList> mObjectDirectoryLists;
		[Obsolete("This exists for the reflection system, do not use.")] public object? mAssetLists;                        //igVector<CAssetList>
		public CResourcePrecacherList _resourcePrecachers;
		public CStringResourcePrecacherHashTable _resourcePrecacherLookup;
		public string _packageName;
		[Obsolete("This exists for the reflection system, do not use.")] public object? mpPrecacheMemoryTracker;            //CPrecacheMemoryTracker
		public bool mReportHeroPrecacheMemory;
		public bool mReportVehiclePrecacheMemory;
		public bool mReportMapPrecacheMemory;
		public bool mReportAllPrecacheMemory;
		public bool _loadTextures = true;
		public EMemoryPoolID _currentUncachingPool;
		[Obsolete("This exists for the reflection system, do not use.")] public object? _exclusionRuleSetList;              //CPrecacheManagerExcludeRuleSetHandleList
		public static CPrecacheManager _Instance;

		public override void Intialize()
		{
			_resourcePrecachers = new CResourcePrecacherList();
			_resourcePrecachers.SetCapacity(0x24);
			_resourcePrecacherLookup = new CStringResourcePrecacherHashTable();
			_resourcePrecacherLookup.Activate(0x24);
			RegisterResourcePrecacher(               "pkg", new COtherPackagePrecacher());
			RegisterResourcePrecacher(    "character_data", new CCharacterDataPrecacher());
			RegisterResourcePrecacher(         "actorskin", new CSkinPrecacher());
			RegisterResourcePrecacher(       "havokanimdb", new CHavokAnimDBPrecacher());
			RegisterResourcePrecacher(    "havokrigidbody", new CHavokPhysicsSystemPrecacher());
			RegisterResourcePrecacher("havokphysicssystem", new CHavokPhysicsSystemPrecacher());
			RegisterResourcePrecacher(           "texture", new CTexturePrecacher());
			RegisterResourcePrecacher(            "effect", new CVfxPrecacher());
			RegisterResourcePrecacher(            "shader", new CShaderPrecacher());
			RegisterResourcePrecacher(        "motionpath", new CMotionPathPrecacher());
			RegisterResourcePrecacher(          "igx_file", new CIgFilePrecacher());
			RegisterResourcePrecacher("material_instances", new CMaterialPrecacher());
			RegisterResourcePrecacher(      "igx_entities", new CEntityPrecacher());
			RegisterResourcePrecacher(       "gui_project", new CGuiProjectPrecacher());
			RegisterResourcePrecacher(              "font", new CFontPrecacher());
			RegisterResourcePrecacher(         "lang_file", new CLanguageFilePrecacher());
			RegisterResourcePrecacher(         "spawnmesh", new CIgFilePrecacher());
			RegisterResourcePrecacher(             "model", new CModelPrecacher());
			RegisterResourcePrecacher(         "sky_model", new CModelPrecacher());
			RegisterResourcePrecacher(          "behavior", new CBehaviorPrecacher());
			RegisterResourcePrecacher("graphdata_behavior", new CBehaviorGraphDataPrecacher());
			RegisterResourcePrecacher(   "events_behavior", new CBehaviorEventPrecacher());
			RegisterResourcePrecacher(    "asset_behavior", new CBehaviorAssetPrecacher());
			RegisterResourcePrecacher(      "hkb_behavior", new CBehaviorAssetPrecacher());
			RegisterResourcePrecacher(     "hkc_character", new CBehaviorAssetPrecacher());
			RegisterResourcePrecacher(           "navmesh", new CNavMeshPrecacher());
			RegisterResourcePrecacher(            "script", new CScriptPrecacher());
			//...

			_packagesPerPool = new igVector<igVector<string>>();
			_packagesPerPool.SetCapacity((int)EMemoryPoolID.MP_MAX_POOL);
			mObjectDirectoryLists = new igVector<igObjectDirectoryList>();
			mObjectDirectoryLists.SetCapacity((int)EMemoryPoolID.MP_MAX_POOL);
			for(int i = 0; i < (int)EMemoryPoolID.MP_MAX_POOL; i++)
			{
				_packagesPerPool.Append(new igVector<string>());
				mObjectDirectoryLists.Append(new igObjectDirectoryList());
			}

		}
		private void RegisterResourcePrecacher(string name, CResourcePrecacher precacher)
		{
			_resourcePrecachers.Append(precacher);
			_resourcePrecacherLookup.Add(name, precacher);
		}
		public bool IsPackageCached(string packageName, EMemoryPoolID poolId)
		{
			string packagePathToCheck = packageName.ToLower();
			igVector<string> packages = _packagesPerPool[(int)poolId];
			return packages.Contains(packagePathToCheck);
		}
		public bool PrecachePackage(string packageName, EMemoryPoolID poolID)
		{
			string packagePath = packageName.ToLower();

			//CEntityPrecacher._currentlyLoadingZone = _currentlyLoadingZone;
			CResourcePrecacher.mDestMemoryPoolId = poolID;

			if(!packagePath.StartsWith("packages"))
			{
				packagePath = "packages/" + packagePath;
			}
			if(!packagePath.EndsWith("_pkg.igz"))
			{
				packagePath += "_pkg.igz";
			}

			if(IsPackageCached(packagePath, poolID)) return true;

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
				_packageName = packagePath;
				string type = list[i];
				string file = list[i+1];
				if(_resourcePrecacherLookup.TryGetValue(type, out CResourcePrecacher precacher))
				{
					precacher.Precache(file);
				}
				else
				{
					throw new NotImplementedException($"file type {type} has no registered loader");
				}
			}

			_packagesPerPool[(int)poolID].Append(packagePath);

			return true;
		}
		public void CleanupDeadRules()
		{

		}
	}
}