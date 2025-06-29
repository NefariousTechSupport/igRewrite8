/*
	Copyright (c) 2022-2025, The Potion Contributors.
	Potion and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Text;
using System.Text.Unicode;
using System.Xml.Serialization;

namespace Potion
{
	/// <summary>
	/// Represents a game installation that has a mod loader
	/// </summary>
	public sealed class Installation
	{
		/// <summary>
		/// The xml data for an "installation manifest", do not reference this class
		/// </summary>
		[XmlType(TypeName = "installation")]
		public class InstallationXml
		{
			public ulong               version;
			public List<EntryXml>      entries;



			public InstallationXml()
			{
				version = kCurrentVersion;
				entries = new List<EntryXml>();
			}
		}



		/// <summary>
		/// The xml data for an installed mod, do not reference this class
		/// </summary>
		public class EntryXml
		{
			public string              identifier;
			public bool                enabled;



			public EntryXml()
			{
				identifier = string.Empty;
				enabled    = false;
			}
		}



		/// <summary>
		/// The xml data for an installed mod
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
					bool? existingInstall = await mConnection.Exists("mod_list.xml");

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

				string xml = GetXml();
				string tempFile = Path.GetTempFileName();

				FileStream fs = File.Create(tempFile);
				await fs.WriteAsync(Encoding.UTF8.GetBytes(xml));
				fs.Seek(0, SeekOrigin.Begin);

				success &= await mConnection.Push(fs, "mod_list.xml");

			}

			// Only overwrite the local mod list if the operation succeeded
			if (success)
			{
				mMods = mods;
			}

			return success;
		}



		/// <summary>
		/// Serializes the installation into a xml file
		/// </summary>
		/// <returns>The serialized string</returns>
		public string GetXml()
		{
			InstallationXml xml = new InstallationXml();
			xml.version = kCurrentVersion;
			xml.entries = new List<EntryXml>();

			for (int i = 0; i < mMods.Count; i++)
			{
				EntryXml entry = new EntryXml();

				entry.enabled = mMods[i].mEnabled;
				entry.identifier = mMods[i].mManifest.Identifier;
			}

			Utf8Writer writer = new Utf8Writer();

			XmlSerializer xmlSerializer = new XmlSerializer(typeof(InstallationXml));
			xmlSerializer.Serialize(writer, xml);

			return writer.ToString();
		}
	}
}