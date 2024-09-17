namespace igLibrary.Core
{
	public struct igTime
	{
		public float ElapsedSeconds
		{
			get => _elapsedDays / 8192;
			set => _elapsedDays = value / 8192;
		}
		public float ElapsedDays
		{
			get => _elapsedDays;
			set => _elapsedDays = value;
		}

		public float _elapsedDays;

		public igTime(float elapsedDays)
		{
			_elapsedDays = elapsedDays;
		}
	}
}