/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	public class igIGZObjectLoader : igObjectLoader
	{
		[Obsolete("This exists for the reflection system, do not use.")] public igIgzDeferredConstructionObjectsList? _deferredConstructionObjects;
		public override string GetLoaderExtension() => "igz";
		public override string GetLoaderName() => "Alchemy Platform";
		public override string GetLoaderType() => "Alchemy";
		public override uint GetTestFileMemorySize() => 4;
		public override void ReadFile(igObjectDirectory dir, string filePath, igBlockingType blockingType)
		{
			igIGZLoader loader = new igIGZLoader(dir, filePath, true);
			dir._type = igObjectDirectory.FileType.kIGZ;
			loader.Read(dir, true);
			dir._fd = loader._fd;
		}
	}
}