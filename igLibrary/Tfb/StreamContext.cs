/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


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
		private struct Streamable
		{
			public igArchive _archive;
			public igObjectDirectory _levelBundle;
		}



		/// <summary>
		/// All loaded files
		/// </summary>
		private Dictionary<string, Streamable> _streamables = new Dictionary<string, Streamable>();



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
			streamable._archive = igFileContext.Singleton.LoadArchive(path);

			// Manually load the level.bld

			streamable._levelBundle = new igObjectDirectory(path, new igName(path));

			// interface with the archive directly and bypass the vfs
			MemoryStream memoryStream = new MemoryStream();
			streamable._archive.Decompress("level.bld", memoryStream);

			igIGZLoader loader = new igIGZLoader(streamable._levelBundle, memoryStream, false);
			loader.Read(streamable._levelBundle, false);

			_streamables.Add(path, streamable);

			return streamable._levelBundle;
		}
	}
}