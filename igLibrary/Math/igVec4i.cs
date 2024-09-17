namespace igLibrary.Math
{
	public struct igVec4i
	{
		public igVec4i Zero => new igVec4i(0, 0, 0, 0);
		public igVec4i One => new igVec4i(1, 1, 1, 1);
		public igVec4i UnitX => new igVec4i(1, 0, 0, 0);
		public igVec4i UnitY => new igVec4i(0, 1, 0, 0);
		public igVec4i UnitZ => new igVec4i(0, 0, 1, 0);
		public igVec4i UnitW => new igVec4i(0, 0, 0, 1);

		public float SqrMagnitude => _x * _x + _y * _y + _z * _z + _w * _w;
		public float Magnitude => MathF.Sqrt(SqrMagnitude);

		public int _x;
		public int _y;
		public int _z;
		public int _w;
		public igVec4i(int x, int y, int z, int w)
		{
			_x = x;
			_y = y;
			_z = z;
			_w = w;
		}
		public static implicit operator System.Numerics.Vector4(igVec4i vec)
		{
			return new System.Numerics.Vector4(vec._x, vec._y, vec._z, vec._w);
		}
		public static implicit operator igVec4i(System.Numerics.Vector4 vec)
		{
			return new igVec4i((int)vec.X, (int)vec.Y, (int)vec.Z, (int)vec.W);
		}
		public static igVec4i operator+(igVec4i a, igVec4i b) => new igVec4i(a._x + b._x, a._y + b._y, a._z + b._z, a._w + b._w);
		public static igVec4i operator-(igVec4i a, igVec4i b) => new igVec4i(a._x - b._x, a._y - b._y, a._z - b._z, a._w - b._w);
	}
}