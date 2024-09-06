using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System;
using System.Collections.ObjectModel;

namespace igLibrary.Core
{
	/// <summary>
	/// Contains reflection metadata information. Stands for Application Runtime Kernel.
	/// </summary>
	public static class igArkCore
	{
		/// <summary>
		/// Enum containing every alchemy game.
		/// </summary>
		public enum EGame
		{
			EV_None = -1,
			EV_ZooCube,
			EV_HootersRoadTrip,
			EV_DogsPlayingPoker,
			EV_EnigmaRisingTide,
			EV_CrashNitroKart,
			EV_SpiderMan2,
			EV_LupinSenseiColumbusNoIsanWaAkeNiSomaru,
			EV_YuGiOhTheDawnOfDestiny,
			EV_GraffitiKingdom,
			EV_XMenLegends,
			EV_GradiusV,
			EV_ShamanKingPowerOfSpirit,
			EV_UltimateSpiderMan,
			EV_XMenLegendsIIRiseOfApocalypse,
			EV_TonyHawksAmericanSk8land,
			EV_DigimonWorld4,
			EV_SpiderManBattleForNewYork,
			EV_MarvelUltimateAlliance,
			EV_TonyHawksDownhillJam,
			EV_TransformersAutobots,
			EV_TransformersDecpticons,
			EV_TonyHawksProvingGround,
			EV_ShrekTheThird,
			EV_BeautfilKatamari,
			EV_LupinSenseiLupinNiWaShiOZenigataNiWaKoiO,
			EV_SpiderMan3_DS,
			EV_WanganMidnightMaximumTune3,
			EV_BackyardBasketball2007,
			EV_SpiderMan3_HC,
			EV_OperationDarkness,
			EV_MadagascarTMEscape2AfricaTMTheGameTM,
			EV_SkylandersSpyrosAdventure,
			EV_SkylandersSpyrosAdventure_3DS,
			EV_HatsuneMikuProjectDiva,
			EV_HatsuneMikuProjectDiva2nd,
			EV_HatsuneMikuProjectDivaExtend,
			EV_SkylandersBattlegrounds,
			EV_SkylandersCloudPatrol,
			EV_SkylandersGiants,
			EV_SkylandersGiants_3DS,
			EV_SkylandersLostIslands,
			EV_SkylandersSwapForce,
			EV_SkylandersSwapForce_3DS,
			EV_SkylandersTrapTeam,
			EV_SkylandersTrapTeam_3DS,
			EV_SkylandersSuperchargers,
			EV_SkylandersImaginators,
			EV_CrashNSaneTrilogy,
			EV_CrashTeamRacingNitroFueled,
			EV_Count
		}





		/// <summary>
		/// Namespace for dynamically generated metaobjects
		/// </summary>
		public const string dynamicMetaObjectNS = "igLibrary.Gen.MetaObject";


		/// <summary>
		/// Namespace for dynamically generated metaenums
		/// </summary>
		public const string dynamicMetaEnumNS = "igLibrary.Gen.MetaEnum";


		/// <summary>
		/// Namespace for dynamically generated compound structs
		/// </summary>
		public const string dynamicCompoundFieldNS = "igLibrary.Gen.CompoundField";





		/// <summary>
		/// A readonly list of loaded igMetaObjects
		/// </summary>
		public static IEnumerable<igMetaObject> MetaObjects => _metaObjects.Values;


		/// <summary>
		/// A readonly list of loaded igMetaEnums
		/// </summary>
		public static IEnumerable<igMetaEnum> MetaEnums => _metaEnums.Values;


		/// <summary>
		/// A readonly list of loaded igCompoundMetaFieldInfos
		/// </summary>
		public static IEnumerable<igCompoundMetaFieldInfo> CompoundMetaFieldInfos => _compoundFieldInfos.Values;


		/// <summary>
		/// A readonly list of loaded igMetaFieldPlatformInfo
		/// </summary>
		public static IEnumerable<igMetaFieldPlatformInfo> MetaFieldPlatformInfos => _metaFieldPlatformInfos.Values;


		/// <summary>
		/// The currently loaded game
		/// </summary>
		public static EGame Game => _game;





		/// <summary>
		/// Currently loaded metaenums
		/// </summary>
		private static Dictionary<string, igMetaEnum> _metaEnums = new Dictionary<string, igMetaEnum>();


		/// <summary>
		/// Currently loaded metaobjects
		/// </summary>
		private static Dictionary<string, igMetaObject> _metaObjects = new Dictionary<string, igMetaObject>();


		/// <summary>
		/// Currently loaded compound field information, aka igCompoundMetaField type structs
		/// </summary>
		private static Dictionary<string, igCompoundMetaFieldInfo> _compoundFieldInfos = new Dictionary<string, igCompoundMetaFieldInfo>();


		/// <summary>
		/// Currently loaded metafield platform information
		/// </summary>
		private static Dictionary<string, igMetaFieldPlatformInfo> _metaFieldPlatformInfos = new Dictionary<string, igMetaFieldPlatformInfo>();





		/// <summary>
		/// Cached class Types for igMetaObjects
		/// </summary>
		private static Dictionary<string, Type>? _vTableCache = null;


		/// <summary>
		/// Cached struct Types for igCompoundMetaFieldInfos
		/// </summary>
		private static Dictionary<string, Type>? _compoundStructCache = null;


		/// <summary>
		/// Cached enums for igMetaEnums
		/// </summary>
		private static Dictionary<string, Type>? _enumCache = null;





		/// <summary>
		/// The DotNet assembly where dynamically generated types are outputted
		/// </summary>
		private static AssemblyBuilder? _dynamicTypeAssembly;


		/// <summary>
		/// The DotNet module where dynamically generated types are outputted
		/// </summary>
		private static ModuleBuilder? _dynamicTypeModule;





		/// <summary>
		/// Dynamically generated types for igMetaObjects
		/// </summary>
		private static Dictionary<string, TypeBuilder>? _dynamicTypes = new Dictionary<string, TypeBuilder>();


		/// <summary>
		/// Dynamically generated types for igCompoundFieldInfo
		/// </summary>
		private static Dictionary<string, Type>? _dynamicStructs = new Dictionary<string, Type>();


		/// <summary>
		/// Types to generate dynamic classes for
		/// </summary>
		public static List<igBaseMeta> _pendingTypes = new List<igBaseMeta>();


		/// <summary>
		/// The currently loade game
		/// </summary>
		private static EGame _game;





		/// <summary>
		/// Deals with differing metadata on different platforms.
		/// </summary>
		private static void FixupClasses(EGame game)
		{
			string funcName = game.ToString();
			MethodInfo? func = typeof(igArkCoreFixups).GetMethod(funcName.ReplaceBeginning("EV_", ""));
			func?.Invoke(null, null);
		}



		/// <summary>
		/// Output stored reflection metadata to an igArkCoreFile.
		/// </summary>
		public static void WriteToFile(EGame game)
		{
			//FixupClasses(game);
			igArkCoreFile saver = new igArkCoreFile();
			saver.BeginSave($"{igArkCoreFile.ArkCoreFolder}/{game.ToString()}.ark");
			foreach(igMetaEnum metaEnum in MetaEnums)
			{
				saver.SaveMetaEnum(metaEnum);
			}
			foreach(igMetaObject metaObject in MetaObjects)
			{
				saver.SaveMetaObject(metaObject);
			}
			foreach(igCompoundMetaFieldInfo compoundFieldInfo in CompoundMetaFieldInfos)
			{
				saver.SaveCompoundInfo(compoundFieldInfo);
			}
			foreach(igMetaFieldPlatformInfo metaFieldPlatformInfo in MetaFieldPlatformInfos)
			{
				saver.SaveMetaFieldPlatformInfo(metaFieldPlatformInfo);
			}
			saver.FinishSave();
			saver.Dispose();
		}



		/// <summary>
		/// Debug function for dumping the dynamically generated assembly to a dotnet dll.
		/// </summary>
		public static void DebugDumpDynamicTypes()
		{
			if(_dynamicTypeAssembly != null)
			{
				throw new NotImplementedException("Building assemblies isn't yet a thing.");
			}
		}


		/// <summary>
		/// Resets the state of igArkCore (not fully implemented)
		/// </summary>
		public static void Reset()
		{
			_metaObjects.Clear();
			_metaEnums.Clear();
			_metaFieldPlatformInfos.Clear();
			_compoundFieldInfos.Clear();
			_game = EGame.EV_None;
		}


		/// <summary>
		/// Reads metadata from an igArkCoreFile.
		/// </summary>
		public static void ReadFromFile(EGame game)
		{
			_game = game;

			System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

			stopwatch.Start();

			igArkCoreFile loader = new igArkCoreFile();
			loader.ReadFile($"{igArkCoreFile.ArkCoreFolder}/{game.ToString()}.ark");
			loader.Dispose();

			stopwatch.Stop();

			Logging.Info("Loading and generating all types took {0} seconds", stopwatch.Elapsed.TotalSeconds);
		}


		/// <summary>
		/// Lookup an igMetaObject by name.
		/// </summary>
		public static igMetaObject? GetObjectMeta(string name)
		{
			if(name == null) return null;

			_metaObjects.TryGetValue(name, out igMetaObject? metaObject);

			return metaObject;
		}


		/// <summary>
		/// Lookup an igMetaFieldPlatformInfo by name.
		/// </summary>
		public static igMetaFieldPlatformInfo? GetMetaFieldPlatformInfo(string name)
		{
			if(name == null) return null;

			_metaFieldPlatformInfos.TryGetValue(name, out igMetaFieldPlatformInfo? platformInfo);

			return platformInfo;
		}


		/// <summary>
		/// Get the DotNet type of an igCompoundMetaField struct.
		/// </summary>
		public static Type? GetStructDotNetType(string name)
		{
			CheckAndInitializeCaches();

			if(_compoundStructCache.ContainsKey(name))
			{
				return _compoundStructCache[name];
			}
			else if(_dynamicStructs.ContainsKey(name))
			{
				return _compoundStructCache[name];
			}
			else return null;
		}


		/// <summary>
		/// Get the DotNet type of an igMetaObject.
		/// </summary>
		public static Type? GetObjectDotNetType(string name)
		{
			CheckAndInitializeCaches();

			if(_vTableCache.ContainsKey(name))
			{
				return _vTableCache[name];
			}
			else if(_dynamicTypes.ContainsKey(name))
			{
				return _dynamicTypes[name];
			}
			else return null;
		}


		/// <summary>
		/// Lookup an igMetaEnum by name.
		/// </summary>
		public static igMetaEnum? GetMetaEnum(string name)
		{
			if(name == null) return null;

			_metaEnums.TryGetValue(name, out igMetaEnum? metaEnum);

			return metaEnum;
		}


		/// <summary>
		/// Lookup an igCompoundMetaFieldInfo by name.
		/// </summary>
		public static igCompoundMetaFieldInfo? GetCompoundFieldInfo(string name)
		{
			if(_compoundFieldInfos == null) return null;

			_compoundFieldInfos.TryGetValue(name, out igCompoundMetaFieldInfo? compoundFieldInfo);

			return compoundFieldInfo;
		}


		/// <summary>
		/// Initialize the dotnet type caches if they've not already been initialized.
		/// </summary>
		private static void CheckAndInitializeCaches()
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

			if(_vTableCache == null)
			{
				Type[] types = assemblies.SelectMany(x => x.GetTypes()).Where(x => x.IsAssignableTo(typeof(__internalObjectBase))).ToArray();
				_vTableCache = new Dictionary<string, Type>();
				for(uint i = 0; i < types.Length; i++)
				{
					_vTableCache.Add(types[i].Name, types[i]);
				}
			}

			if(_compoundStructCache == null)
			{
				Type[] types = assemblies.SelectMany(x => x.GetTypes()).Where(x => x.IsValueType && !x.IsEnum && x.GetCustomAttribute<igStruct>() != null).ToArray();
				_compoundStructCache = new Dictionary<string, Type>();
				for(uint i = 0; i < types.Length; i++)
				{
					string typeName = types[i].Name;
					int backtickIndex = typeName.IndexOf('`');
					if(backtickIndex >= 0) typeName = typeName.Substring(0, backtickIndex);
					_compoundStructCache.Add(typeName, types[i]);
				}
			}

			if(_enumCache == null)
			{
				Type[] types = assemblies.SelectMany(x => x.GetTypes()).Where(x => x.IsEnum && x.Namespace != null && x.Namespace.StartsWith("ig")).ToArray();
				_enumCache = new Dictionary<string, Type>();
				for(uint i = 0; i < types.Length; i++)
				{
					_enumCache.Add(types[i].Name, types[i]);
				}
			}
		}


		/// <summary>
		/// Get the DotNet type of an igMetaEnum.
		/// </summary>
		public static Type? GetEnumDotNetType(string name)
		{
			CheckAndInitializeCaches();

			if(_enumCache.ContainsKey(name))
			{
				return _enumCache[name];
			}
			else
			{
				return null;
			}
		}


		/// <summary>
		/// Grabs an igMetaField by its handle.
		/// </summary>
		public static igMetaField? GetFieldMetaForObject(string handle)
		{
			string[] args = handle.Split("::");
			igMetaObject? targetMeta = GetObjectMeta(args[0]);
			if(targetMeta == null) return null;
			return targetMeta.GetFieldByName(args[1]);
		}


		/// <summary>
		/// Create the dynamic type module for type generation
		/// </summary>
		private static void CreateDynamicModule()
		{
			AssemblyName dynamicAssemblyName = new AssemblyName("ArkGeneratedTypes");

			_dynamicTypeAssembly = AssemblyBuilder.DefineDynamicAssembly(dynamicAssemblyName, AssemblyBuilderAccess.RunAndCollect);
			_dynamicTypeModule = _dynamicTypeAssembly.DefineDynamicModule(dynamicAssemblyName.Name);
		}


		/// <summary>
		/// Factory constructor for a new EnumBuilder for igMetaEnum.
		/// </summary>
		public static EnumBuilder GetNewEnumBuilder(string name)
		{
			if(_dynamicTypeModule == null)
			{
				CreateDynamicModule();
			}

			return _dynamicTypeModule.DefineEnum($"{dynamicMetaEnumNS}.{name}", TypeAttributes.Public, typeof(int));
		}


		/// <summary>
		/// Factory constructor for a new TypeBuilder for igCompoundMetaFieldInfo.
		/// </summary>
		public static TypeBuilder GetNewStructTypeBuilder(string name)
		{
			if(_dynamicTypeModule == null)
			{
				CreateDynamicModule();
			}

			TypeBuilder tb = _dynamicTypeModule.DefineType($"{dynamicCompoundFieldNS}.{name}", TypeAttributes.Public, typeof(ValueType));

			_dynamicStructs.Add(name, tb);

			return tb;
		}


		/// <summary>
		/// Factory constructor for a new TypeBuilder for igMetaObjects.
		/// </summary>
		public static TypeBuilder GetNewTypeBuilder(string name)
		{
			if(_dynamicTypeModule == null)
			{
				CreateDynamicModule();
			}

			TypeBuilder tb = _dynamicTypeModule.DefineType($"{dynamicMetaObjectNS}.{name}", TypeAttributes.AutoClass | TypeAttributes.Public);

			_dynamicTypes.Add(name, tb);

			return tb;
		}


		/// <summary>
		/// Add a dynamic type to the type caches
		/// </summary>
		public static void AddDynamicTypeToCache(Type type)
		{
			CheckAndInitializeCaches();


			if(_dynamicTypes.ContainsKey(type.Name))
			{
				_dynamicTypes.Remove(type.Name);
			}

			if(!_vTableCache.ContainsKey(type.Name))
			{
				_vTableCache.Add(type.Name, type);
			}
		}


		/// <summary>
		/// Checks whether a type was created dynamically
		/// </summary>
		/// <param name="meta">The type to check</param>
		/// <returns>whether the type was created at runtime</returns>
		public static bool IsDynamicType(igMetaObject meta) => meta._vTablePointer == null || meta._vTablePointer.Assembly == _dynamicTypeAssembly;


		/// <summary>
		/// Generate dynamics types for pending types.
		/// </summary>
		public static void FlushPendingTypes()
		{
			for(int i = 0; i < _pendingTypes.Count; i++)
			{
				_pendingTypes[i].DefineType2();
			}
			for(int i = 0; i < _pendingTypes.Count; i++)
			{
				_pendingTypes[i].CreateType2();
			}
			_pendingTypes.Clear();
		}


		/// <summary>
		/// Add igMetaObject to metaobject list
		/// </summary>
		public static void AddObjectMeta(igMetaObject meta) => _metaObjects.Add(meta._name!, meta);


		/// <summary>
		/// Add igMetaEnum to metaenum list
		/// </summary>
		public static void AddEnumMeta(igMetaEnum meta) => _metaEnums.Add(meta._name!, meta);


		/// <summary>
		/// Add igCompoundMetaFieldInfo to compound field info list
		/// </summary>
		public static void AddCompoundMeta(igCompoundMetaFieldInfo meta) => _compoundFieldInfos.Add(meta._name!, meta);


		/// <summary>
		/// Add igMetaFieldPlatformInfo to metafield platform info list
		/// </summary>
		public static void AddPlatformMeta(igMetaFieldPlatformInfo meta) => _metaFieldPlatformInfos.Add(meta._name!, meta);
	}
}