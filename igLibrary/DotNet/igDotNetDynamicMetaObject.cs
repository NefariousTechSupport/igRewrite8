/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.DotNet
{
	public class igDotNetDynamicMetaObject : igDotNetMetaObject
	{
		public DotNetLibrary _owner;
		public void FinalizeAppendToArkCore()
		{
			igDynamicMetaObject.setMetaDataField(this);
		}
		public override igObject ConstructInstance(igMemoryPool memPool, bool setFields = true)
		{
			igObject obj = base.ConstructInstance(memPool, setFields);
			obj.dynamicMeta = true;
			_vTablePointer!.GetField("_meta")!.SetValue(obj, this);
			return obj;
		}
	}
}