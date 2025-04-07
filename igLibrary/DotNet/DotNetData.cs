/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.DotNet
{
	public struct DotNetData
	{
		public object? _data;
		public DotNetType _type;
		public DataRepresentation _representation;
		public uint _maybeRepresentation;

		public enum DataRepresentation
		{
			Normal = 0,
			Complex = 1,
			Indirect = 2,
			RawIndirect = 4,
			FieldReference = 8,
		}

		public DotNetData()
		{
			_data = null;
			_type = new DotNetType();
			_representation = DataRepresentation.Normal;
			_maybeRepresentation = 0;
		}
	}
}