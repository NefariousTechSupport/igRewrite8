/*
	Copyright (c) 2022-2025, The Potion Contributors.
	Potion and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using YamlDotNet.Serialization;

namespace Potion
{
	public sealed class InstallationParams
	{
		// General
		[YamlMember(ApplyNamingConventions = false, Alias = "connection")]
		public string? ConnectionType;
		public FtpYaml? Ftp;
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
			public int?    Port;
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
}
