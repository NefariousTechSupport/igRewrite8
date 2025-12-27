/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using igLibrary.Sg;
using igLibrary.Tfb.Attrs;

namespace igLibrary.Tfb.Game
{
	/// <summary>
	/// The main class for loading/unloading tfb archives. This differs from
	/// alchemy laboratory due to many differences in the way the engines
	/// are structured
	/// </summary>
	public class StreamContext
	{
		private static Lazy<StreamContext> lazy = new Lazy<StreamContext>(() => new StreamContext());
		/// <summary>
		/// Singleton
		/// </summary>
		public static StreamContext Singleton => lazy.Value;



		/// <summary>
		/// Structure representing a loaded file
		/// </summary>
		public class Streamable
		{
			public igArchive _bundleArchive;
			public igArchive? _streamArchive;
			public igObjectDirectory _levelBundle;
			public igObjectDirectory _languagePak;

			// hacky way of tying the igObjectDirectorys to the igArchive
			public igFileDescriptor _ignoreBundleFd;
			public igFileDescriptor _ignoreStreamedFd;
		}



		/// <summary>
		/// All loaded files
		/// </summary>
		private Dictionary<string, Streamable> _streamables = new Dictionary<string, Streamable>();



		/// <summary>
		/// Counter for the global.bld igHandle namespace
		/// </summary>
		private uint _globalBldCounter = 0;



		/// <summary>
		/// public accessor for the streamable items
		/// </summary>
		public IReadOnlyDictionary<string, Streamable> Streamables => _streamables;



		/// <summary>
		/// Constructor
		/// </summary>
		private StreamContext()
		{
			igObjectHandleManager.Singleton.AddSystemNamespace("global.bld");
		}



		/// <summary>
		/// Loads a tfb path
		/// </summary>
		/// <param name="path">the path</param>
		/// <returns>the level.bld's igObjectDirectory</returns>
		public igObjectDirectory Load(string path)
		{
			// Fixup the path
			if (!path.StartsWith("app:/"))
			{
				path = "app:/" + path;
			}
			if (!path.EndsWith(".bld"))
			{
				path = path + ".bld";
			}

			// Mount the archive

			Streamable streamable = new Streamable();
			streamable._bundleArchive = igFileContext.Singleton.LoadArchive(path);
			streamable._ignoreBundleFd = new igFileDescriptor();
			streamable._ignoreBundleFd._device = streamable._bundleArchive;

			string streamPath = Path.ChangeExtension(path, ".arc");
			if (igFileContext.Singleton.Exists(streamPath))
			{
				streamable._streamArchive = igFileContext.Singleton.LoadArchive(streamPath);
				streamable._ignoreStreamedFd = new igFileDescriptor();
				streamable._ignoreStreamedFd._device = streamable._bundleArchive;
			}

			// Load the language pak if it exists

			bool hasLanguagePak = streamable._bundleArchive.HasFile("ENGLISH.pak");

			if (hasLanguagePak)
			{
				streamable._languagePak = new igObjectDirectory(path, new igName(path + " (language pak)"));
				streamable._languagePak._type = igObjectDirectory.FileType.kIGZ;
				streamable._languagePak._fd = streamable._ignoreBundleFd;

				// bypass vfs and load it from the archive directly
				MemoryStream languagePakStream = new MemoryStream();
				streamable._bundleArchive.Decompress("ENGLISH.pak", languagePakStream);

				igIGZLoader languageLoader = new igIGZLoader(streamable._languagePak, languagePakStream, false);
				languageLoader.Read(streamable._languagePak, false);

				// ...then set up the handles...
				BindLanguagePackHandles(streamable);
			}

			// Then load the level.bld

			streamable._levelBundle = new igObjectDirectory(path, new igName(path + " (level bld)"));
			streamable._levelBundle._type = igObjectDirectory.FileType.kIGZ;
			streamable._levelBundle._fd = streamable._ignoreBundleFd;

			// bypass vfs and load it from the archive directly
			MemoryStream levelBundleStream = new MemoryStream();
			streamable._bundleArchive.Decompress("level.bld", levelBundleStream);

			// ...finally load the level.bld

			igIGZLoader levelLoader = new igIGZLoader(streamable._levelBundle, levelBundleStream, false);
			levelLoader.Read(streamable._levelBundle, false);

			// clean up the handles
			UnbindLanguagePackHandles(streamable);

			PostLoadTasks(streamable._levelBundle);

			_streamables.Add(path, streamable);

			return streamable._levelBundle;
		}


		public igObjectDirectory? StreamDirectory(Streamable streamable, uint tocHash)
		{
			igArchive? archive = streamable._streamArchive;
			if (archive == null)
			{
				return null;
			}

			if (!archive.HasFile(tocHash))
			{
				return null;
			}

			MemoryStream ms = new MemoryStream();
			archive.Decompress(tocHash, ms);

			string directoryName = $"{archive._path} ({tocHash.ToString("X08")})";
			igObjectDirectory directory = new igObjectDirectory(directoryName, new igName(directoryName));
			directory._type = igObjectDirectory.FileType.kIGZ;
			directory._fd = streamable._ignoreStreamedFd;

			igIGZLoader loader = new igIGZLoader(directory, ms, false);
			loader.Read(directory, false);

			return directory;
		}



		/// <summary>
		/// Add "level.bld" (language pack) handles to the igObjectHandleManager
		/// </summary>
		/// <param name="streamable"></param>
		private void BindLanguagePackHandles(Streamable streamable)
		{
			if (streamable._languagePak == null)
			{
				return;
			}

			// Set up handles
			igHandleName levelBldName = new igHandleName();
			levelBldName._ns = new igName("level.bld");
			for (int l = 0; l < streamable._languagePak._objectList._count; l++)
			{
				igObject obj = streamable._languagePak._objectList[l];

				levelBldName._name._hash = (uint)(l + 1);

				igObjectHandleManager.Singleton.AddObject(obj, levelBldName);
			}
			igObjectHandleManager.Singleton.AddSystemNamespace("level.bld");
		}



		/// <summary>
		/// Add "level.bld" (language pack) handles to the igObjectHandleManager
		/// </summary>
		/// <param name="streamable"></param>
		private void UnbindLanguagePackHandles(Streamable streamable)
		{
			if (streamable._languagePak == null)
			{
				return;
			}

			igObjectHandleManager.Singleton.RemoveSystemNamespace("level.bld");

			igHandleName levelBldName = new igHandleName();
			levelBldName._ns = new igName("level.bld");
			for (int l = 0; l < streamable._languagePak._objectList._count; l++)
			{
				levelBldName._name._hash = (uint)(l + 1);

				igObjectHandleManager.Singleton.RemoveHandle(levelBldName);
			}
		}


		/// <summary>
		/// Tfb like to do additional things to the objects after loading a file
		/// </summary>
		/// <param name="dir">the directory to fix up</param>
		private void PostLoadTasks(igObjectDirectory dir)
		{
			for (int o = 0; o < dir._objectList._count; o++)
			{
				igObject obj = dir._objectList[o];

				if (obj is tfbEffectInfo effectInfo)
				{
					PostLoadTfbEffectInfo(effectInfo);
				}
			}
		}


		/// <summary>
		/// handle tfbEffectInfo post load conditions
		/// </summary>
		/// <param name="effectInfo">the tfbEffectInfo</param>
		private void PostLoadTfbEffectInfo(tfbEffectInfo effectInfo)
		{
			string effectPlatform = "effect" + igAlchemyCore.GetPlatformString(igRegistry.GetRegistry()._platform);

			for (int e = 0; e < effectInfo._effectList._count; e++)
			{
				igEffect effect = effectInfo._effectList[e];

				// handle would've already been created so let's patch it up
				// instead of creating a new one
				igHandle handle = igObjectHandleManager.Singleton.LookupHandle(new igName(Path.GetFileNameWithoutExtension(effect._name)), new igName(effectPlatform));
				handle._object = effect;
			}
		}



		/// <summary>
		/// handles global.bld EXID stuff
		/// </summary>
		/// <param name="globalBld">the global.bld level.bld directory</param>
		public void HandleGlobalBld(igObjectDirectory globalBld)
		{
			igHandleName name = new igHandleName();
			name._ns = new igName("global.bld");

			for (int g = 0; g < globalBld._objectList._count; g++)
			{
				igObject obj = globalBld._objectList[g];
				name._name._hash = ++_globalBldCounter;

				igObjectHandleManager.Singleton.AddObject(obj, name);
			}
		}
	}
}