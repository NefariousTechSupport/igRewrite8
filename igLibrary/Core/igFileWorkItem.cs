namespace igLibrary.Core
{
	public class igFileWorkItem : igObject
	{
		public enum WorkType
		{
			kTypeInvalid = 0,
			kTypeExists = 1,
			kTypeOpen = 2,
			kTypeClose = 3,
			kTypeRead = 4,
			kTypeWrite = 5,
			kTypeTruncate = 6,
			kTypeMkdir = 7,
			kTypeRmdir = 8,
			kTypeFileList = 9,
			kTypeFileListWithSizes = 10,
			kTypeUnlink = 11,
			kTypeRename = 12,
			kTypePrefetch = 13,
			kTypeFormat = 14,
			kTypeCommit = 15,
		}
		public enum Priority
		{
			kPriorityLow = 0,
			kPriorityNormal = 1,
			kPriorityHigh = 2,
		}
		public enum Status
		{
			kStatusInactive,
			kStatusActive,
			kStatusComplete,
			kStatusDeviceNotFound,
			kStatusInvalidPath,
			kStatusTooManyOpenFiles,
			kStatusBadParam,
			kStatusOutOfMemory,
			kStatusDiskFull,
			kStatusDoorOpen,
			kStatusReadError,
			kStatusWriteError,
			kStatusAlreadyInUse,
			kStatusAlreadyExists,
			kStatusEndOfFile,
			kStatusDeviceNotInitialized,
			kStatusMediaUnformatted,
			kStatusMediaCorrupt,
			kStatusPermissionDenied,
			kStatusGeneralError,
			kStatusStopped,
			kStatusUnsupported,
		}
		public string _path;
		public igFileDescriptor? _file;
		public object? _buffer;
		public ulong _offset;
		public uint _size;
		public uint _bytesProcessed;
		public Action	_callback;
		public object[] _callbackData;
		[Obsolete("This exists for the reflection system, do not use.")] public object? _signal;    //igSignal
		[Obsolete("This exists for the reflection system, do not use.")] public byte _bfStorage__0;
		public uint _flags;
		public WorkType _type;
		public Priority _priority;
		public Status _status;
		public igBlockingType _blocking;
		public void SetStatus(Status status)
		{
			_status = status;
		}
	}
	public class igFileWorkItemList : igTObjectList<igFileWorkItemList>{}
}