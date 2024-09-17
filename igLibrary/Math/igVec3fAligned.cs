namespace igLibrary.Math
{
	public struct igVec3fAligned
	{
		public igVec3fAligned Zero => new igVec3fAligned(0, 0, 0);
		public igVec3fAligned One => new igVec3fAligned(1, 1, 1);
		public igVec3fAligned UnitX => new igVec3fAligned(1, 0, 0);
		public igVec3fAligned UnitY => new igVec3fAligned(0, 1, 0);
		public igVec3fAligned UnitZ => new igVec3fAligned(0, 0, 1);

		public float SqrMagnitude => _x * _x + _y * _y + _z * _z;
		public float Magnitude => MathF.Sqrt(SqrMagnitude);

		public float _x;
		public float _y;
		public float _z;
		public igVec3fAligned(float x, float y, float z)
		{
			_x = x;
			_y = y;
			_z = z;
		}
		public static implicit operator System.Numerics.Vector3(igVec3fAligned vec)
		{
			return new System.Numerics.Vector3(vec._x, vec._y, vec._z);
		}
		public static implicit operator igVec3fAligned(System.Numerics.Vector3 vec)
		{
			return new igVec3fAligned(vec.X, vec.Y, vec.Z);
		}
		public static igVec3fAligned operator+(igVec3fAligned a, igVec3fAligned b) => new igVec3fAligned(a._x + b._x, a._y + b._y, a._z + b._z);
		public static igVec3fAligned operator-(igVec3fAligned a, igVec3fAligned b) => new igVec3fAligned(a._x - b._x, a._y - b._y, a._z - b._z);
	}
}