namespace igLibrary.Core
{
	public class igResource : igObject
	{
		public igDirectoryList _directoryList;
		public string _relativeFilePath;
		public string _absoluteFilePath;
		public bool _autoCompatibility;
		public bool _IGBSharingState;
		public int _IGBChunkSize;
		public bool _useMemoryPoolAssignmentsState;
		public igReferenceResolverSet _referenceResolverSet;
	}
}