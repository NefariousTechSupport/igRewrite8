namespace igLibrary
{
	public class CBehaviorGraphDataPrecacher : CResourcePrecacher
	{
		public override void Precache(string filePath)
		{
			//Unimplemented
			igObjectStreamManager.Singleton.Load(filePath);
		}
	}
}