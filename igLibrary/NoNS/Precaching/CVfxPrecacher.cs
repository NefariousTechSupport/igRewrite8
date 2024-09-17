namespace igLibrary
{
	public class CVfxPrecacher : CResourcePrecacher
	{
		public override void Precache(string filePath)
		{
			//Unimplemented
			igObjectStreamManager.Singleton.Load(filePath);
		}
	}
}