namespace igLibrary
{
	public class CMotionPathPrecacher : CResourcePrecacher
	{
		public override void Precache(string filePath)
		{
			//Unimplemented
			igObjectStreamManager.Singleton.Load(filePath);
		}
	}
}