/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Text.RegularExpressions;
using igLibrary.Core;
using YamlDotNet.Serialization;


namespace Potion
{
	public class ModManifest
	{
		private struct ModManifestYaml
		{
			[YamlMember(ApplyNamingConventions = false)]
			public ulong               manifest_version;
			[YamlMember(ApplyNamingConventions = false)]
			public string              identifier;
			[YamlMember(ApplyNamingConventions = false)]
			public ulong               mod_version;
			[YamlMember(ApplyNamingConventions = false)]
			public string              name;
			[YamlMember(ApplyNamingConventions = false)]
			public string              author;
			[YamlMember(ApplyNamingConventions = false)]
			public string              description;
			[YamlMember(ApplyNamingConventions = false)]
			public igArkCore.EGame     game;
			[YamlMember(ApplyNamingConventions = false)]
			public IG_CORE_PLATFORM    platform;
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
		/// Set identifier, requires the format is correct
		/// </summary>
		/// <param name="identifier">The new identifier to use</param>
		/// <returns>Whether or not the identifier was set</returns>
		public bool TrySetIdentifier(string identifier)
		{
			const string kIdentifierRegex = @"^[0-9a-z\-\+_]+\.[0-9a-z\-\+_]+\.[0-9a-z\-\+_]+$";

			bool success = Regex.IsMatch(identifier, kIdentifierRegex);

			if (success)
			{
				mIdentifier = identifier;
			}

			return success;
		}
	}
}