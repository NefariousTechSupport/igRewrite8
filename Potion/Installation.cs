/*
	Copyright (c) 2022-2025, The Potion Contributors.
	Potion and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using igLibrary.Core;
using YamlDotNet.Serialization;

namespace Potion
{
	/// <summary>
	/// Represents a game installation that has a mod loader
	/// </summary>
	public abstract class Installation
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
			public bool                enabled;
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



		/// <summary>
		/// The platform
		/// </summary>
		public abstract IG_CORE_PLATFORM Platform { get; }



		private Connection?            mConnection;
		private InstallationParams     mParams;



		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parameters">Configuration settings for this installation</param>
		public Installation(InstallationParams parameters)
		{
			mParams = parameters;
		}
	}
}