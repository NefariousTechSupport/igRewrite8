using Microsoft.VisualBasic.FileIO;

namespace igLibrary.Core
{
	public class igFileContext : igSingleton<igFileContext>
	{
		private List<igFileDescriptor> _fileDescriptorPool = new List<igFileDescriptor>();
		public List<igFileWorkItem> _fileWorkItemPool = new List<igFileWorkItem>();		//TODO: Make this an igObjectPool
		private List<igStorageDevice> _devices = new List<igStorageDevice>();
		private igFileWorkItemProcessor _processorStack;
		public igArchiveMountManager _archiveMountManager = new igArchiveMountManager();
		public igArchiveManager _archiveManager = new igArchiveManager();

		Dictionary<string, string> _virtualDevices = new Dictionary<string, string>()
		{
			{"actors", "actors"},
			{"anims", "anims"},
			{"behavior_events", "behavior_events"},
			{"animation_events", "animation_events"},
			{"behaviors", "behaviors"},
			{"cutscene", "cutscene"},
			{"data", ""},
			{"fonts", "fonts"},
			{"graphs", "graphs"},
			{"vsc", "vsc"},
			{"loosetextures", "loosetextures"},
			{"luts", "loosetextures/luts"},
			{"maps", "maps"},
			{"materials", "materialInstances"},
			{"models", "models"},
			{"motionpaths", "motionpaths"},
			{"renderer", "renderer"},
			{"scripts", "scripts"},
			{"shaders", "shaders"},
			{"sky", "sky"},
			{"sounds", "sounds"},
			{"spawnmeshes", "spawnmeshes"},
			{"textures", "textures"},
			{"ui", "ui"},
			{"vfx", "vfx"},
			{"cwd", ""},
			{"app", ""},
		};

		public string _root { get; private set;}

		public igFileContext()
		{
			_root = string.Empty;

			//Not Accurate
			//_processorStack = new igWin32StorageDevice();
			//_processorStack._nextProcessor = _archiveMountManager;

			//Accurate
			_processorStack = _archiveMountManager;

			_archiveMountManager._nextProcessor = _archiveManager;
			_archiveManager._nextProcessor = new igWin32StorageDevice();
		}

		public string GetVirtualStorageDevicePath(string virtualDeviceName) => GetMediaDirectory(virtualDeviceName);
		//Rename to GetVirtualStorageDevicePath
		public string GetMediaDirectory(string media)
		{
			string lower = media.ToLower().TrimEnd(':');
			if(_virtualDevices.ContainsKey(lower))
			{
				return _virtualDevices[lower];
			}
			else return media;
		}
		private void CreateWorkItem(igFileDescriptor? fd, igFileWorkItem.WorkType workType, object? buffer, ulong offset, ulong size, uint flags, string path, igBlockingType blockingType, igFileWorkItem.Priority priority, Action? callback, object[]? callbackData)
		{
			igFileWorkItem workItem = AllocateWorkItem();

			workItem._file = fd;
			workItem._buffer = buffer;
			workItem._offset = offset;
			workItem._size = (uint)size;
			workItem._flags = flags;
			igFilePath fp = new igFilePath();
			fp.Set(path);
			workItem._path = fp._path;
			workItem._type = workType;
			workItem._blocking = blockingType;
			workItem._priority = priority;

			//Didn't see this happening but it felt right
			workItem._status = igFileWorkItem.Status.kStatusActive;

			_processorStack.Process(workItem);
		}

		public igFileWorkItem AllocateWorkItem()
		{
			igFileWorkItem workItem;
			//int index = _fileWorkItemPool.FindIndex(x => x._status != igFileWorkItem.Status.kStatusActive);
			int index = -1;
			if(index < 0)
			{
				workItem = new igFileWorkItem();
				_fileWorkItemPool.Add(workItem);
			}
			else
			{
				workItem = _fileWorkItemPool[index];
			}
			return workItem;
		}

		public void Initialize(string root)
		{
			_root = root.TrimEnd('/');
			_root = _root.TrimEnd('\\');
		}
		public void InitializeUpdate(string updatePath)
		{
			igArchive arc = new igArchive();
			arc.Open(updatePath, igBlockingType.kMayBlock);
			_archiveManager._patchArchives.Append(arc);
		}

		public igArchive LoadArchive(string path) => _archiveManager.LoadArchive(path);
		public void AddStorageDevice(igStorageDevice device)
		{
			_devices.Add(device);
		}

		public igFileDescriptor Open(string path)
		{
			igFilePath fp = new igFilePath();
			fp.Set(path);
			int fdIndex = _fileDescriptorPool.FindIndex(x => x._path == fp._path.ToString());

			if(fdIndex >= 0) return _fileDescriptorPool[fdIndex];
			else
			{
				Stream? ms = null;
				if(File.Exists(fp.getNativePath()))
				{
					ms = File.Open(fp.getNativePath(), FileMode.Open);
				}
				else
				{
					uint hash = igHash.Hash(fp._path.ToString());
					for(int i = 0; i < _devices.Count; i++)
					{
						if(_devices[i] is igArchive iga)
						{
							if(iga.HasFile(hash))
							{
								ms = new MemoryStream();
								iga.Decompress(hash, ms);
							}
						}
					}
					if(ms == null) throw new FileNotFoundException($"Failed to find file {fp._path}");
				}
				_fileDescriptorPool.Add(new igFileDescriptor(ms, fp._path.ToString()));
				return _fileDescriptorPool.Last();
			}
		}
		public static uint GetOpenFlags(FileAccess access, FileMode mode)
		{
			return ((uint)mode << 8) | (uint)access;
		}
		private igFileDescriptor Prepare(string path, uint flags)
		{
			igFileDescriptor fd = new igFileDescriptor();
			_fileDescriptorPool.Add(fd);
			igFilePath fp = new igFilePath();
			fp.Set(path);
			fd._path = fp._path;
			return fd;
		}
		public void Open(string path, uint flags, out igFileDescriptor fd, igBlockingType blockingType, igFileWorkItem.Priority priority)
		{
			fd = Prepare(path, flags);
			CreateWorkItem(fd, igFileWorkItem.WorkType.kTypeOpen, null, 0, 0, flags, fd._path, blockingType, priority, null, null);

			if(fd._handle == null)
			{
				throw new FileNotFoundException($"Failed to open file \"{path}\"");
			}
		}
		public void Close(igFileDescriptor fd, igBlockingType blockingType, igFileWorkItem.Priority priority)
		{
			CreateWorkItem(fd, igFileWorkItem.WorkType.kTypeClose, null, 0, 0, 0, fd._path, blockingType, priority, null, null);
			_fileDescriptorPool.Remove(fd);
		}
		public void FileList(string dir, out igStringRefList list, igBlockingType blockingType, igFileWorkItem.Priority priority)
		{
			list = new igStringRefList();
			CreateWorkItem(null, igFileWorkItem.WorkType.kTypeFileList, list, 0, 0, 0, dir, blockingType, priority, null, null);
		}
	}
}