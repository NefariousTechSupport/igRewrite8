/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary
{
	public class CScriptPrecacher : CResourcePrecacher
	{
		public override void Precache(string filePath)
		{
			if(!CDotNetaManager._Instance.IsLibraryLoaded(filePath))
			{
				igObjectStreamManager.Singleton.Load(filePath);
				//CDotNetaManager._Instance.LoadScript(filePath, mDestMemoryPoolId, false);
			}
		}
	}
}