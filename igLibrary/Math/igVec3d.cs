/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Math
{
	public struct igVec3d
	{
		public igVec3d Zero => new igVec3d(0, 0, 0);
		public igVec3d One => new igVec3d(1, 1, 1);
		public igVec3d UnitX => new igVec3d(1, 0, 0);
		public igVec3d UnitY => new igVec3d(0, 1, 0);
		public igVec3d UnitZ => new igVec3d(0, 0, 1);

		public double SqrMagnitude => _x * _x + _y * _y + _z * _z;
		public double Magnitude => MathF.Sqrt((float)SqrMagnitude);

		public double _x;
		public double _y;
		public double _z;
		public igVec3d(double x, double y, double z)
		{
			_x = x;
			_y = y;
			_z = z;
		}
		public static implicit operator System.Numerics.Vector3(igVec3d vec)
		{
			return new System.Numerics.Vector3((float)vec._x, (float)vec._y, (float)vec._z);
		}
		public static implicit operator igVec3d(System.Numerics.Vector3 vec)
		{
			return new igVec3d(vec.X, vec.Y, vec.Z);
		}
		public static igVec3d operator+(igVec3d a, igVec3d b) => new igVec3d(a._x + b._x, a._y + b._y, a._z + b._z);
		public static igVec3d operator-(igVec3d a, igVec3d b) => new igVec3d(a._x - b._x, a._y - b._y, a._z - b._z);
	}
}