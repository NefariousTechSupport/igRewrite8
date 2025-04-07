/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	public class igDynamicMetaObject : igMetaObject
	{
		public static void setMetaDataField(igMetaObject meta)
		{
			int metaMetaFieldIndex = meta.GetFieldIndexByName("_meta");

			if(metaMetaFieldIndex < 0) return;

			igMetaField field = meta._metaFields[metaMetaFieldIndex].CreateFieldCopy();

			field._default = meta;

			meta.ValidateAndSetField(metaMetaFieldIndex, field);
		}
	}
}