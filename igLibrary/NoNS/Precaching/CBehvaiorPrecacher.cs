namespace igLibrary
{
	public class CBehaviorPrecacher : CResourcePrecacher
	{
        public override void Precache(string filePath)
        {
			//Unimplemented
            igObjectStreamManager.Singleton.Load(filePath);
        }
    }
}