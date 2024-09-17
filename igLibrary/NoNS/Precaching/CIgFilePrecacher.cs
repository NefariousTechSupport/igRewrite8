namespace igLibrary
{
	public class CIgFilePrecacher : CResourcePrecacher
	{
		public override void Precache(string filePath)
		{
			igObjectStreamManager.Singleton.Load(filePath);
		}
	}
}