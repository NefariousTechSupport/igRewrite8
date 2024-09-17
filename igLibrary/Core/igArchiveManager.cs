namespace igLibrary.Core
{
	public class igArchiveManager : igFileWorkItemProcessor
	{
		public igArchiveList _archiveList = new igArchiveList();
		public igArchiveList _patchArchives = new igArchiveList();
		[Obsolete("This exists for the reflection system, do not use.")] public object? _archiveListLock;           //igReadWriteLock
		[Obsolete("This exists for the reflection system, do not use.")] public uint _archiveListLockCounter;
		[Obsolete("This exists for the reflection system, do not use.")] public igMemoryPool _blockPool;
		[Obsolete("This exists for the reflection system, do not use.")] public object? _sharedSignal;              //igSignal
		[Obsolete("This exists for the reflection system, do not use.")] public object? _processLock;               //igSemaphore
		[Obsolete("This exists for the reflection system, do not use.")] public object? _freeArchiveItems;          //igArchiveFileWorkItemList
		[Obsolete("This exists for the reflection system, do not use.")] public object? _activeArchiveItems;        //igArchiveFileWorkItemList
		[Obsolete("This exists for the reflection system, do not use.")] public object? _allBlockItems;             //igArchiveBlockWorkItemList
		[Obsolete("This exists for the reflection system, do not use.")] public object? _freeBlockItems;            //igArchiveBlockWorkItemList
		[Obsolete("This exists for the reflection system, do not use.")] public object? _blockReadItems;            //igArchiveBlockWorkItemList
		[Obsolete("This exists for the reflection system, do not use.")] public object? _blockDecompressItems;      //igArchiveBlockWorkItemList
		[Obsolete("This exists for the reflection system, do not use.")] public object? _decompressionQueue;
		[Obsolete("This exists for the reflection system, do not use.")] public int _decompressionBatchCounter;
		[Obsolete("This exists for the reflection system, do not use.")] public bool _enableReadAhead;
		[Obsolete("This exists for the reflection system, do not use.")] public igArchive _lastDevice;
		[Obsolete("This exists for the reflection system, do not use.")] public object? _lastFile;
		[Obsolete("This exists for the reflection system, do not use.")] public uint _lastBlockIndex;
		[Obsolete("This exists for the reflection system, do not use.")] public ulong _lastConsumedOffset;
		[Obsolete("This exists for the reflection system, do not use.")] public object? _lastPriority;              //Priority
		[Obsolete("This exists for the reflection system, do not use.")] public int _overrideArchives;
		public igArchive LoadArchive(string path)
		{
			if(TryGetArchive(path, out igArchive? loaded)) return loaded;
			loaded = new igArchive();
			loaded.Open(path, igBlockingType.kMayBlock);
			_archiveList.Append(loaded);
			return loaded;
		}
		public bool TryGetArchive(string path, out igArchive? archive)
		{
			igFilePath fp = new igFilePath();
			fp.Set(path);
			for(int i = 0; i < _archiveList._count; i++)
			{
				if(_archiveList[i]._path.ToLower() == fp._path.ToLower())
				{
					archive = _archiveList[i];
					return true;
				}
			}
			archive = null;
			return false;
		}

		public override void Process(igFileWorkItem workItem)
		{
			if(workItem._type == igFileWorkItem.WorkType.kTypeFileList)
			{
				uint pathHash = igHash.Hash(workItem._path);
				for(int i = 0; i < _patchArchives._count; i++)
				{
					igArchive archive = _patchArchives[i];
					if(igHash.Hash(archive._path) == pathHash)
					{
						archive.Process(workItem);
						return;
					}
				}
				for(int i = 0; i < _archiveList._count; i++)
				{
					igArchive archive = _archiveList[i];
					if(igHash.Hash(archive._path) == pathHash)
					{
						archive.Process(workItem);
						return;
					}
				}
			}
			else
			{
				if(workItem._type == igFileWorkItem.WorkType.kTypeInvalid) goto giveUp;

				for(int i = 0; i < _patchArchives._count; i++)
				{
					igArchive archive = _patchArchives[i];
					archive.Process(workItem);
					if(workItem._status == igFileWorkItem.Status.kStatusComplete) return;
				}
				for(int i = 0; i < _archiveList._count; i++)
				{
					igArchive archive = _archiveList[i];
					archive.Process(workItem);
					if(workItem._status == igFileWorkItem.Status.kStatusComplete) return;
				}
			}

		giveUp:
			SendToNextProcessor(workItem);
		}
	}
}