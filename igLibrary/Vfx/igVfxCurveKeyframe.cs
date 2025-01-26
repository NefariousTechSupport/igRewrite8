/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Vfx
{
	[igStruct]
	public struct igVfxCurveKeyframe
	{
		public float _range;
		public bool _linear;
		public sbyte _x;
		public sbyte _y;
		public sbyte _variance;
		public sbyte _data1;
		public sbyte _data2;
	}
}