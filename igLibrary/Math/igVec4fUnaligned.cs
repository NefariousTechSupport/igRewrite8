namespace igLibrary.Math
{
	public struct igVec4fUnaligned
	{
		public igVec4fUnaligned Zero => new igVec4fUnaligned(0, 0, 0, 0);
		public igVec4fUnaligned One => new igVec4fUnaligned(1, 1, 1, 1);
		public igVec4fUnaligned UnitX => new igVec4fUnaligned(1, 0, 0, 0);
		public igVec4fUnaligned UnitY => new igVec4fUnaligned(0, 1, 0, 0);
		public igVec4fUnaligned UnitZ => new igVec4fUnaligned(0, 0, 1, 0);
		public igVec4fUnaligned UnitW => new igVec4fUnaligned(0, 0, 0, 1);

		public float SqrMagnitude => _x * _x + _y * _y + _z * _z + _w * _w;
		public float Magnitude => MathF.Sqrt(SqrMagnitude);

		public float _x;
		public float _y;
		public float _z;
		public float _w;
		public igVec4fUnaligned(float x, float y, float z, float w)
		{
			_x = x;
			_y = y;
			_z = z;
			_w = w;
		}
		public static implicit operator System.Numerics.Vector4(igVec4fUnaligned vec)
		{
			return new System.Numerics.Vector4(vec._x, vec._y, vec._z, vec._w);
		}
		public static implicit operator igVec4fUnaligned(System.Numerics.Vector4 vec)
		{
			return new igVec4fUnaligned(vec.X, vec.Y, vec.Z, vec.W);
		}
		public static igVec4fUnaligned operator+(igVec4fUnaligned a, igVec4fUnaligned b) => new igVec4fUnaligned(a._x + b._x, a._y + b._y, a._z + b._z, a._w + b._w);
		public static igVec4fUnaligned operator-(igVec4fUnaligned a, igVec4fUnaligned b) => new igVec4fUnaligned(a._x - b._x, a._y - b._y, a._z - b._z, a._w - b._w);
	}
}