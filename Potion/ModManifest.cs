/*
	Copyright (c) 2022-2025, The Potion Contributors.
	Potion and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using igLibrary.Core;


namespace Potion
{
	public class ModManifest
	{
		/// <summary>
		/// Xml data for a mod manifest, do not reference this class
		/// </summary>
		[XmlType(TypeName = "mod_manifest")]
		public class ModManifestXml
		{
			public ulong               manifest_version;
			public string              identifier;
			public ulong               mod_version;
			public string              name;
			public string              author;
			public string              description;
			public igArkCore.EGame     game;
			public IG_CORE_PLATFORM    platform;



			public ModManifestXml()
			{
				manifest_version = kCurrentManifestVersion;
				identifier       = string.Empty;
				mod_version      = 0;
				name             = string.Empty;
				author           = string.Empty;
				description      = string.Empty;
				game             = igArkCore.EGame.EV_None;
				platform         = IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT;
			}
		}



		public ulong                   ManifestVersion { get => mManifestVersion; set => mManifestVersion = value; }
		public string                  Identifier      { get => mIdentifier;                                       }
		public ulong                   ModVersion      { get => mModVersion;      set => mModVersion      = value; }
		public string                  Name            { get => mName;            set => mName            = value; }
		public string                  Author          { get => mAuthor;          set => mAuthor          = value; }
		public string                  Desc            { get => mDesc;            set => mDesc            = value; }
		public igArkCore.EGame         Game            { get => mGame;            set => mGame            = value; }
		public IG_CORE_PLATFORM        Platform        { get => mPlatform;        set => mPlatform        = value; }



		/// <summary>
		/// The current, and hence latest manifest version
		/// </summary>
		public const ulong             kCurrentManifestVersion = 0;



		private ulong                  mManifestVersion;
		private string                 mIdentifier;
		private ulong                  mModVersion;
		private string                 mName;
		private string                 mAuthor;
		private string                 mDesc;
		private igArkCore.EGame        mGame;
		private IG_CORE_PLATFORM       mPlatform;



		/// <summary>
		/// ModManifest constructor
		/// </summary>
		/// <param name="game">The game the mod applies to</param>
		/// <param name="platform">The platform the mod applies to</param>
		public ModManifest(igArkCore.EGame game, IG_CORE_PLATFORM platform)
		{
			mManifestVersion = kCurrentManifestVersion;
			mIdentifier      = string.Empty;
			mModVersion      = 0;
			mName            = string.Empty;
			mAuthor          = string.Empty;
			mDesc            = string.Empty;
			mGame            = game;
			mPlatform        = platform;
		}



		/// <summary>
		/// private ModManifest constructor
		/// </summary>
		private ModManifest()
		{
			mManifestVersion = kCurrentManifestVersion;
			mIdentifier      = string.Empty;
			mModVersion      = 0;
			mName            = string.Empty;
			mAuthor          = string.Empty;
			mDesc            = string.Empty;
			mGame            = igArkCore.EGame.EV_None;
			mPlatform        = IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT;
		}



		/// <summary>
		/// Attempts to read a mod manifest
		/// </summary>
		/// <param name="connection">The file connection</param>
		/// <param name="folder">The folder to read from</param>
		/// <returns>The manifest, null if the manifest is invalid</returns>
		public static async Task<ModManifest?> Read(Connection connection, FileProps folder)
		{
			ModManifest? manifest = null;

			bool validMod = IsValidIdentifier(folder.Name);
			validMod &= folder.Attributes.HasFlag(FileAttributes.Directory);

			bool? exists = await connection.Exists($"{folder.Name}/manifest.xml");

			validMod &= exists.HasValue && exists.Value;

			if (validMod)
			{
				Stream? data = await connection.Pull($"{folder.Name}/manifest.xml");

				if (data != null)
				{
					XmlSerializer xmlDeserializer = new XmlSerializer(typeof(ModManifestXml));
					ModManifestXml? manifestXml = xmlDeserializer.Deserialize(data) as ModManifestXml;

					if (manifestXml != null)
					{
						manifest                  = new ModManifest();
						manifest.mManifestVersion = manifestXml.manifest_version;
						manifest.mIdentifier      = manifestXml.identifier;
						manifest.mModVersion      = manifestXml.mod_version;
						manifest.mName            = manifestXml.name;
						manifest.mAuthor          = manifestXml.author;
						manifest.mDesc            = manifestXml.description;
						manifest.mGame            = manifestXml.game;
						manifest.mPlatform        = manifestXml.platform;
					}
				}
			}

			return manifest;
		}



		public static bool IsValidIdentifier(string identifier)
		{
			const string kIdentifierRegex = @"^[0-9a-z\-\+_]+\.[0-9a-z\-\+_]+\.[0-9a-z\-\+_]+$";

			return Regex.IsMatch(identifier, kIdentifierRegex);
		}



		/// <summary>
		/// Set identifier, requires the format is correct
		/// </summary>
		/// <param name="identifier">The new identifier to use</param>
		/// <returns>Whether or not the identifier was set</returns>
		public bool TrySetIdentifier(string identifier)
		{
			bool valid = IsValidIdentifier(identifier);

			if (valid)
			{
				mIdentifier = identifier;
			}

			return valid;
		}
	}
}