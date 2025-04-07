/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.DotNet
{
	public struct DotNetFieldDefinition
	{
		public string Name;
		public FieldDefFlags Flags;
		public DotNetData Data;

		public enum FieldDefFlags
		{
			kHandle = 1,
			kConstruct = 2
		}
	}
	public class DotNetFieldDefinitionList : igTDataList<DotNetFieldDefinition>{}
}