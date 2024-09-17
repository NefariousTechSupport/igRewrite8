namespace igLibrary.Math
{
	public struct igMatrix44f
	{
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
		public unsafe igMatrix44f(float[] mat)
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