/*
	Copyright (c) 2022-2025, The Potion Contributors.
	Potion and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using igLibrary.Core;

namespace Potion
{
	/// <summary>
	/// A PS3 specific installation, handles FTP
	/// </summary>
	public sealed class PS3Installation : Installation
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parameters">Configuration settings for this installation</param>
		public PS3Installation(InstallationParams parameters) : base(parameters)
		{
		}



		public override IG_CORE_PLATFORM Platform => IG_CORE_PLATFORM.IG_CORE_PLATFORM_PS3;
	}
}