/*
	Copyright (c) 2022-2026, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Graphics
{
	public class igShaderConstantValue : igObject
	{
		public igGraphicsShaderConstant _constant;
	}

	public class igShaderConstantValueBool : igShaderConstantValue
	{
		public bool _value;
	}

	public class igShaderConstantValueInt : igShaderConstantValue
	{
		public int _value;
	}

	public class igShaderConstantValueFloat : igShaderConstantValue
	{
		public float _value;
	}

	public class igShaderConstantValueVector : igShaderConstantValue
	{
		public igVec4f _value;
	}

	public class igShaderConstantValueMatrix : igShaderConstantValue
	{
		public igMatrix44f _value;
	}

	public class igShaderConstantValueList : igGraphicsObject
	{
		public igVector<igShaderConstantValue> _values;
	}
}
