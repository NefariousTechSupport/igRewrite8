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
	public sealed class InstallationParams
	{
		// General
		[YamlMember(ApplyNamingConventions = false, Alias = "connection")]
		public EConnectionType ConnectionType = EConnectionType.kNone;
		[YamlMember(ApplyNamingConventions = false, Alias = "platform")]
		public IG_CORE_PLATFORM Platform;
		[YamlMember(ApplyNamingConventions = false, Alias = "ftp")]
		public FtpYaml? Ftp;
		[YamlMember(ApplyNamingConventions = false, Alias = "file")]
		public FileYaml? File;
		[YamlMember(ApplyNamingConventions = false, Alias = "ps3")]
		public Ps3Yaml? Ps3;



		/// <summary>
		/// FTP Parameters
		/// </summary>
		public struct FtpYaml
		{
			[YamlMember(ApplyNamingConventions = false, Alias = "host")]
			public string? Host;
			[YamlMember(ApplyNamingConventions = false, Alias = "username")]
			public string? Username;
			[YamlMember(ApplyNamingConventions = false, Alias = "password")]
			public string? Password;
			[YamlMember(ApplyNamingConventions = false, Alias = "port")]
			public int? Port;
			[YamlMember(ApplyNamingConventions = false, Alias = "root")]
			public string? Root;
		}



		/// <summary>
		/// FTP Parameters
		/// </summary>
		public struct FileYaml
		{
			[YamlMember(ApplyNamingConventions = false, Alias = "root")]
			public string? Root;
		}



		/// <summary>
		/// PS3 Parameters
		/// </summary>
		public struct Ps3Yaml
		{
			[YamlMember(ApplyNamingConventions = false, Alias = "titleid")]
			public string? TitleId;
		}
	}



	/// <summary>
	/// Connection Types
	/// </summary>
	public enum EConnectionType
	{
		/// <summary>
		/// Uninitialised
		/// </summary>
		kNone,

		/// <summary>
		/// Local file system, useful for stuff like sdcards or emulators
		/// </summary>
		kFile,

		/// <summary>
		/// FTP, useful for consoles without things like SD cards
		/// </summary>
		kFtp
	}
}
