namespace igLibrary
{
	public class CLanguageFilePrecacher : CResourcePrecacher
	{
        public override void Precache(string filePath)
        {
			//Unimplemented
            igObjectStreamManager.Singleton.Load(filePath);
        }
    }
}