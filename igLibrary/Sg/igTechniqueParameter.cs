/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Sg
{
	public class igTechniqueParameter : igObject
	{
		public enum PARAMETER_TYPE : int
		{
			PARAMETER_TYPE_MATRIX = 0,
			PARAMETER_TYPE_VECTOR = 1,
			PARAMETER_TYPE_INTEGER = 2,
			PARAMETER_TYPE_BOOL = 3,
			PARAMETER_TYPE_BOOL_ARRAY = 4,
			PARAMETER_TYPE_VECTORI = 5,
			PARAMETER_TYPE_MATRIX_ARRAY = 6,
			PARAMETER_TYPE_VECTOR_ARRAY = 7,
			PARAMETER_TYPE_INTEGER_ARRAY = 8,
			PARAMETER_TYPE_VECTORI_ARRAY = 9,
			PARAMETER_TYPE_FLOAT = 10,
			PARAMETER_TYPE_FLOAT_ARRAY = 11,
			PARAMETER_TYPE_UNKNOWN = 65536,
		}

		public string _shaderName;
		public string _engineName;
		public bool _isEngine;
		public PARAMETER_TYPE _type;
		public int _vectorWidth;
		public int _elementIndex;
		public int _elementSize;
		public int _elementCount;
	}

	public class igTechniqueParameterList : igTObjectList<igTechniqueParameter>
	{
	}
}