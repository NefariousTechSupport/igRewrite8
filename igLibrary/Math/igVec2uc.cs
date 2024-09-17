namespace igLibrary.Math
{
	public struct igVec2uc
	{
		public igVec2uc Zero => new igVec2uc(0, 0);
		public igVec2uc One => new igVec2uc(1, 1);
		public igVec2uc UnitX => new igVec2uc(1, 0);
		public igVec2uc UnitY => new igVec2uc(0, 1);

		public float SqrMagnitude => _x * _x + _y * _y;
		public float Magnitude => MathF.Sqrt(SqrMagnitude);

		public byte _x;
		public byte _y;
		public igVec2uc(byte x, byte y)
		{
			_x = x;
			_y = y;
		}
		public static implicit operator System.Numerics.Vector2(igVec2uc vec)
		{
			return new System.Numerics.Vector2(vec._x, vec._y);
		}
		public static implicit operator igVec2uc(System.Numerics.Vector2 vec)
		{
			return new igVec2uc((byte)vec.X, (byte)vec.Y);
		}
		public static igVec2uc operator+(igVec2uc a, igVec2uc b) => new igVec2uc((byte)(a._x + b._x), (byte)(a._y + b._y));
		public static igVec2uc operator-(igVec2uc a, igVec2uc b) => new igVec2uc((byte)(a._x - b._x), (byte)(a._y - b._y));
	}
}