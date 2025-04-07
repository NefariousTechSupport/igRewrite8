/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.DotNet
{
	public class igDotNetMetaOnlyLibraryLoader : igLibraryLoader
	{
		public DotNetRuntime _runtime;
		public static object _loadCallback;
		public override string GetLoaderExtension() => "vvl";
		public override string GetLoaderType() => "DotNet";
		public override uint GetTestFileMemorySize() => 0x28;
		public override void ReadFile(igObjectDirectory dir, string filePath, igBlockingType blockingType)
		{
			if(dir._nameList == null)
			{
				dir._nameList = new igNameList();
				dir._useNameList = true;
			}
			_runtime = CDotNetaManager._Instance._runtime;
			DotNetLibrary lib = VvlLoader.Load(filePath, _runtime, out bool success);
			for(int i = 0; i < lib._ownedTypes._count; i++)
			{
				if(lib._ownedTypes[i] is igDotNetDynamicMetaObject metaObject)
				{
					metaObject.AppendToArkCore();
				}
				else if(lib._ownedTypes[i] is igDotNetDynamicMetaEnum metaEnum)
				{
					metaEnum.AppendToArkCore();
				}
				else if(lib._ownedTypes[i] is not igMetaObject && lib._ownedTypes[i] is not igMetaEnum)
				{
					throw new NotSupportedException("What??");
				}
			}
			//Don't use AddObject, that's only for external use cos it messes with igHandles
			dir._objectList.Append(lib);
			dir._nameList.Append(new igName("library"));
		}
	}
}