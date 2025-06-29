/*
	Copyright (c) 2022-2025, The Potion Contributors.
	Potion and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Text;
using igLibrary.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Potion
{
	/// <summary>
	/// Represents a game installation that has a mod loader
	/// </summary>
	public sealed class Installation
	{
		/// <summary>
		/// The yaml data for an "installation manifest"
		/// </summary>
		private struct InstallationYaml
		{
			[YamlMember(ApplyNamingConventions = false)]
			public ulong               version;
			[YamlMember(ApplyNamingConventions = false)]
			public List<EntryYaml>     entries;
		}



		/// <summary>
		/// The yaml data for an installed mod
		/// </summary>
		private struct EntryYaml
		{
			[YamlMember(ApplyNamingConventions = false)]
			public string              identifier;
			[YamlMember(ApplyNamingConventions = false)]
			public bool                enabled;
		}



		/// <summary>
		/// The yaml data for an installed mod
		/// </summary>
		private class InstalledMod
		{
			public ModManifest         mManifest;
			public bool                mEnabled;



			public InstalledMod(ModManifest manifest, bool enabled)
			{
				mManifest = manifest;
				mEnabled  = enabled;
			}
		}



		/// <summary>
		/// Wrapper around file IO api
		/// </summary>
		public Connection? Connection
		{
			get => mConnection;
			set
			{
				mConnection = value;

				if (mConnection != null)
				{
					mConnection.Params = mParams;
				}
			}
		}



		/// <summary>
		/// Installation parameters providing configuration settings
		/// </summary>
		public InstallationParams Params
		{
			get => mParams;
		}



		private const ulong            kCurrentVersion = 0;



		private Connection?            mConnection;
		private InstallationParams     mParams;
		private List<InstalledMod>     mMods;



		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parameters">Configuration settings for this installation</param>
		public Installation(InstallationParams parameters)
		{
			mConnection = null;
			mParams     = parameters;
			mMods       = new List<InstalledMod>();
		}



		/// <summary>
		/// Opens the installation
		/// </summary>
		/// <returns>Whether the installation was opened successfully</returns>
		public async Task<bool> Open()
		{
			bool success = true;

			switch (mParams.ConnectionType)
			{
				case EConnectionType.kNone:
					success = false;
					break;
				case EConnectionType.kFile:
					mConnection = new FileConnection();
					break;
				case EConnectionType.kFtp:
					mConnection = new FTPConnection();
					break;
			}

			if (mConnection != null)
			{
				mConnection.Params = mParams;
				success &= await mConnection.Connect();

				if (success)
				{
					bool? existingInstall = await mConnection.Exists("mod_list.yaml");

					success &= existingInstall.HasValue;

					if (success && !existingInstall.Value)
					{
						success &= await CreateInstall();
					}
				}
			}

			return success;
		}



		/// <summary>
		/// Creates a fresh install
		/// </summary>
		/// <returns>Whether the operation succeeded</returns>
		public async Task<bool> CreateInstall()
		{
			bool success = true;

			List<InstalledMod> mods = new List<InstalledMod>();

			if (mConnection != null)
			{
				List<FileProps>? files = await mConnection.ListDirectory("");

				success &= files != null;
				if (files != null)
				{
					for (int i = 0; i < files.Count; i++)
					{
						// TODO: We should have some way of detecting if the connection died, this is
						// okay for now though.
						// We do not want to mark an invalid manifest as a failure to create the install
						ModManifest? modManifest = await ModManifest.Read(mConnection, files[i]);

						if (modManifest != null)
						{
							mods.Add(new InstalledMod(modManifest, false));
						}
					}
				}

				string yaml = GetYaml();
				string tempFile = Path.GetTempFileName();

				FileStream fs = File.Create(tempFile);
				await fs.WriteAsync(Encoding.UTF8.GetBytes(yaml));
				fs.Seek(0, SeekOrigin.Begin);

				success &= await mConnection.Push(fs, "mod_list.yaml");

			}

			// Only overwrite the local mod list if the operation succeeded
			if (success)
			{
				mMods = mods;
			}

			return success;
		}



		/// <summary>
		/// Serializes the installation into a yaml file
		/// </summary>
		/// <returns>The serialized string</returns>
		public string GetYaml()
		{
			InstallationYaml yaml = new InstallationYaml();
			yaml.version = kCurrentVersion;
			yaml.entries = new List<EntryYaml>();

			for (int i = 0; i < mMods.Count; i++)
			{
				EntryYaml entry = new EntryYaml();

				entry.enabled = mMods[i].mEnabled;
				entry.identifier = mMods[i].mManifest.Identifier;
			}

			SerializerBuilder builder = new SerializerBuilder();
			builder.WithNamingConvention(NullNamingConvention.Instance);
			ISerializer serializer = builder.Build();

			return serializer.Serialize(yaml, typeof(InstallationYaml));
		}
	}
}