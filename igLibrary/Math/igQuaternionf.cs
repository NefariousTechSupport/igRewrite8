namespace igLibrary.Math
{
	public struct igQuaternionf
	{
		public igQuaternionf Identity => new igQuaternionf(0, 0, 0, 1);

		public float _x;
		public float _y;
		public float _z;
		public float _w;
		public igQuaternionf(float x, float y, float z, float w)
		{
			_x = x;
			_y = y;
			_z = z;
			_w = w;
		}
	}
}