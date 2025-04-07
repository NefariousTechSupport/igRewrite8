/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Math
{
	public struct igQuaternionf
	{
		public igQuaternionf Identity => new igQuaternionf(0, 0, 0, 1);

		public float _x;
		public float _y;
		public float _z;
		public float _w;
		public igQuaternionf(float x, float y, float z, float w)
		{
			_x = x;
			_y = y;
			_z = z;
			_w = w;
		}
	}
}