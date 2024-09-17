namespace igLibrary.Math
{
	public struct igVec4f
	{
		public igVec4f Zero => new igVec4f(0, 0, 0, 0);
		public igVec4f One => new igVec4f(1, 1, 1, 1);
		public igVec4f UnitX => new igVec4f(1, 0, 0, 0);
		public igVec4f UnitY => new igVec4f(0, 1, 0, 0);
		public igVec4f UnitZ => new igVec4f(0, 0, 1, 0);
		public igVec4f UnitW => new igVec4f(0, 0, 0, 1);

		public float SqrMagnitude => _x * _x + _y * _y + _z * _z + _w * _w;
		public float Magnitude => MathF.Sqrt(SqrMagnitude);

		public float _x;
		public float _y;
		public float _z;
		public float _w;
		public igVec4f(float x, float y, float z, float w)
		{
			_x = x;
			_y = y;
			_z = z;
			_w = w;
		}
		public static implicit operator System.Numerics.Vector4(igVec4f vec)
		{
			return new System.Numerics.Vector4(vec._x, vec._y, vec._z, vec._w);
		}
		public static implicit operator igVec4f(System.Numerics.Vector4 vec)
		{
			return new igVec4f(vec.X, vec.Y, vec.Z, vec.W);
		}
		public static igVec4f operator+(igVec4f a, igVec4f b) => new igVec4f(a._x + b._x, a._y + b._y, a._z + b._z, a._w + b._w);
		public static igVec4f operator-(igVec4f a, igVec4f b) => new igVec4f(a._x - b._x, a._y - b._y, a._z - b._z, a._w - b._w);
	}
}