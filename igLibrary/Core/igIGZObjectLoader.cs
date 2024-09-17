namespace igLibrary.Core
{
	public class igIGZObjectLoader : igObjectLoader
	{
		[Obsolete("This exists for the reflection system, do not use.")] public igIgzDeferredConstructionObjectsList? _deferredConstructionObjects;
		public override string GetLoaderExtension() => "igz";
		public override string GetLoaderName() => "Alchemy Platform";
		public override string GetLoaderType() => "Alchemy";
		public override uint GetTestFileMemorySize() => 4;
		public override void ReadFile(igObjectDirectory dir, string filePath, igBlockingType blockingType)
		{
			igIGZLoader loader = new igIGZLoader(dir, filePath, true);
			dir._type = igObjectDirectory.FileType.kIGZ;
			loader.Read(dir, true);
			dir._fd = loader._fd;
		}
	}
}