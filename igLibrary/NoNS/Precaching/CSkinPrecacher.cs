namespace igLibrary
{
	public class CSkinPrecacher : CResourcePrecacher
	{
		public override void Precache(string filePath)
		{
			//Unimplemented
			igObjectStreamManager.Singleton.Load(filePath);
		}
	}
}