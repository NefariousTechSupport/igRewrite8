namespace igLibrary
{
	public class CBehaviorEventPrecacher : CResourcePrecacher
	{
		public override void Precache(string filePath)
		{
			//Unimplemented
			igObjectStreamManager.Singleton.Load(filePath);
		}
	}
}