/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Math
{
	public struct igMatrix44f
	{
		/// <summary>
		/// Identity Matrix
		/// </summary>
		public igMatrix44f Identity => new igMatrix44f()
		{
			_m11 = 1f, _m12 = 0f, _m13 = 0f, _m14 = 0f,
			_m21 = 0f, _m22 = 1f, _m23 = 0f, _m24 = 0f,
			_m31 = 0f, _m32 = 0f, _m33 = 1f, _m34 = 0f,
			_m41 = 0f, _m42 = 0f, _m43 = 0f, _m44 = 1f
		};

		public float _m11;
		public float _m12;
		public float _m13;
		public float _m14;
		public float _m21;
		public float _m22;
		public float _m23;
		public float _m24;
		public float _m31;
		public float _m32;
		public float _m33;
		public float _m34;
		public float _m41;
		public float _m42;
		public float _m43;
		public float _m44;


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="m11">Matrix value in column 1 and row 1</param>
		/// <param name="m12">Matrix value in column 2 and row 1</param>
		/// <param name="m13">Matrix value in column 3 and row 1</param>
		/// <param name="m14">Matrix value in column 4 and row 1</param>
		/// <param name="m21">Matrix value in column 1 and row 2</param>
		/// <param name="m22">Matrix value in column 2 and row 2</param>
		/// <param name="m23">Matrix value in column 3 and row 2</param>
		/// <param name="m24">Matrix value in column 4 and row 2</param>
		/// <param name="m31">Matrix value in column 1 and row 3</param>
		/// <param name="m32">Matrix value in column 2 and row 3</param>
		/// <param name="m33">Matrix value in column 3 and row 3</param>
		/// <param name="m34">Matrix value in column 4 and row 3</param>
		/// <param name="m41">Matrix value in column 1 and row 4</param>
		/// <param name="m42">Matrix value in column 2 and row 4</param>
		/// <param name="m43">Matrix value in column 3 and row 4</param>
		/// <param name="m44">Matrix value in column 4 and row 4</param>
		public igMatrix44f(
			float m11, float m12, float m13, float m14,
			float m21, float m22, float m23, float m24,
			float m31, float m32, float m33, float m34,
			float m41, float m42, float m43, float m44
		)
		{
			_m11 = m11;
			_m12 = m12;
			_m13 = m13;
			_m14 = m14;
			_m21 = m21;
			_m22 = m22;
			_m23 = m23;
			_m24 = m24;
			_m31 = m31;
			_m32 = m32;
			_m33 = m33;
			_m34 = m34;
			_m41 = m41;
			_m42 = m42;
			_m43 = m43;
			_m44 = m14;
		}


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="mat">16 length float array containing matrix values</param>
		public igMatrix44f(float[] mat)
		{
			if(mat.Length != 16) throw new ArgumentException("4x4 Matrix array must be of length 16");

			_m11 = mat[00];
			_m12 = mat[01];
			_m13 = mat[02];
			_m14 = mat[03];
			_m21 = mat[04];
			_m22 = mat[05];
			_m23 = mat[06];
			_m24 = mat[07];
			_m31 = mat[08];
			_m32 = mat[09];
			_m33 = mat[10];
			_m34 = mat[11];
			_m41 = mat[12];
			_m42 = mat[13];
			_m43 = mat[14];
			_m44 = mat[15];
		}
	}
}