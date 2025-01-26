/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Math
{
	public struct igVec3uc
	{
		public igVec3uc Zero => new igVec3uc(0, 0, 0);
		public igVec3uc One => new igVec3uc(1, 1, 1);
		public igVec3uc UnitX => new igVec3uc(1, 0, 0);
		public igVec3uc UnitY => new igVec3uc(0, 1, 0);
		public igVec3uc UnitZ => new igVec3uc(0, 0, 1);

		public float SqrMagnitude => _x * _x + _y * _y + _z * _z;
		public float Magnitude => MathF.Sqrt(SqrMagnitude);

		public byte _x;
		public byte _y;
		public byte _z;
		public igVec3uc(byte x, byte y, byte z)
		{
			_x = x;
			_y = y;
			_z = z;
		}
		public static implicit operator System.Numerics.Vector3(igVec3uc vec)
		{
			return new System.Numerics.Vector3(vec._x, vec._y, vec._z);
		}
		public static implicit operator igVec3uc(System.Numerics.Vector3 vec)
		{
			return new igVec3uc((byte)vec.X, (byte)vec.Y, (byte)vec.Z);
		}
		public static igVec3uc operator+(igVec3uc a, igVec3uc b) => new igVec3uc((byte)(a._x + b._x), (byte)(a._y + b._y), (byte)(a._z + b._z));
		public static igVec3uc operator-(igVec3uc a, igVec3uc b) => new igVec3uc((byte)(a._x - b._x), (byte)(a._y - b._y), (byte)(a._z - b._z));
	}
}