/*
	Copyright (c) 2022-2025, The Potion Contributors.
	Potion and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Xml.Serialization;
using igLibrary.Core;

namespace Potion
{
	public sealed class InstallationParams
	{
		// General
		[XmlElement("connection")]
		public EConnectionType ConnectionType = EConnectionType.kNone;
		[XmlElement("platform")]
		public IG_CORE_PLATFORM Platform;
		[XmlElement("ftp")]
		public FtpXml? Ftp;
		[XmlElement("file")]
		public FileXml? File;
		[XmlElement("ps3")]
		public Ps3Xml? Ps3;



		/// <summary>
		/// FTP Parameters
		/// </summary>
		public struct FtpXml
		{
			[XmlElement("host")]
			public string? Host;
			[XmlElement("username")]
			public string? Username;
			[XmlElement("password")]
			public string? Password;
			[XmlElement("port")]
			public int? Port;
			[XmlElement("root")]
			public string? Root;
		}



		/// <summary>
		/// FTP Parameters
		/// </summary>
		public struct FileXml
		{
			[XmlElement("root")]
			public string? Root;
		}



		/// <summary>
		/// PS3 Parameters
		/// </summary>
		public struct Ps3Xml
		{
			[XmlElement("titleid")]
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
