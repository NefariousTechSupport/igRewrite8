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
	public abstract class Project
	{
		public ModManifest             Manifest { get => mManifest; }



		private ModManifest            mManifest;



		public Project(igArkCore.EGame game, IG_CORE_PLATFORM platform)
		{
			mManifest = new ModManifest(game, platform);
		}
	}
}
