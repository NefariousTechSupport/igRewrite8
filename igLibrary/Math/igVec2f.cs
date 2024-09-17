namespace igLibrary.Math
{
	public struct igVec2f
	{
		public igVec2f Zero => new igVec2f(0, 0);
		public igVec2f One => new igVec2f(1, 1);
		public igVec2f UnitX => new igVec2f(1, 0);
		public igVec2f UnitY => new igVec2f(0, 1);

		public float SqrMagnitude => _x * _x + _y * _y;
		public float Magnitude => MathF.Sqrt(SqrMagnitude);

		public float _x;
		public float _y;
		public igVec2f(float x, float y)
		{
			_x = x;
			_y = y;
		}
		public static implicit operator System.Numerics.Vector2(igVec2f vec)
		{
			return new System.Numerics.Vector2(vec._x, vec._y);
		}
		public static implicit operator igVec2f(System.Numerics.Vector2 vec)
		{
			return new igVec2f(vec.X, vec.Y);
		}
		public static igVec2f operator+(igVec2f a, igVec2f b) => new igVec2f(a._x + b._x, a._y + b._y);
		public static igVec2f operator-(igVec2f a, igVec2f b) => new igVec2f(a._x - b._x, a._y - b._y);
	}
}