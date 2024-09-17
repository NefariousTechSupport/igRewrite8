namespace igLibrary.Core
{
	public class igInfo : igReferenceResolver
	{
		public igDirectory _directory;
		public bool _resolveState;
	}
	public class igInfoList : igTObjectList<igInfo> {}
}