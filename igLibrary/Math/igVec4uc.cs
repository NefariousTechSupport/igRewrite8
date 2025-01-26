/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Math
{
	public struct igVec4uc
	{
		public igVec4uc Zero => new igVec4uc(0, 0, 0, 0);
		public igVec4uc One => new igVec4uc(1, 1, 1, 1);
		public igVec4uc UnitX => new igVec4uc(1, 0, 0, 0);
		public igVec4uc UnitY => new igVec4uc(0, 1, 0, 0);
		public igVec4uc UnitZ => new igVec4uc(0, 0, 1, 0);
		public igVec4uc UnitW => new igVec4uc(0, 0, 0, 1);

		public float SqrMagnitude => _r * _r + _g * _g + _b * _b + _a * _a;
		public float Magnitude => MathF.Sqrt(SqrMagnitude);

		public byte _r;
		public byte _g;
		public byte _b;
		public byte _a;
		public igVec4uc(byte r, byte g, byte b, byte a)
		{
			_r = r;
			_g = g;
			_b = b;
			_a = a;
		}
		public static implicit operator System.Drawing.Color(igVec4uc vec)
		{
			return System.Drawing.Color.FromArgb(vec._a, vec._r, vec._g, vec._b);
		}
		public static implicit operator igVec4uc(System.Drawing.Color vec)
		{
			return new igVec4uc(vec.R, vec.G, vec.B, vec.A);
		}
		public static igVec4uc operator+(igVec4uc a, igVec4uc b) => new igVec4uc((byte)(a._r + b._r), (byte)(a._g + b._g), (byte)(a._b + b._b), (byte)(a._a + b._a));
		public static igVec4uc operator-(igVec4uc a, igVec4uc b) => new igVec4uc((byte)(a._r - b._r), (byte)(a._g - b._g), (byte)(a._b - b._b), (byte)(a._a - b._a));
	}
}