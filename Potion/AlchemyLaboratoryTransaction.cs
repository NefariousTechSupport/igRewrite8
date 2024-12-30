using igLibrary;
using igLibrary.Core;

namespace Potion
{
	public sealed class AlchemyLaboratoryTransaction : Transaction
	{
		private string _path;
		private igArchive? _archive;
		private Blob _oldData;
		private Blob _newData;


		public AlchemyLaboratoryTransaction(BlobManager blobs, string path, Blob newData)
		{
			_path = path;
			_newData = newData;
			_oldData = Blob.NullBlob;

			_type = TransactionType.Addition;

			if (igFileContext.Singleton.Exists(_path))
			{
				igFileDescriptor fd = igFileContext.Singleton.Open(_path);
				_oldData = blobs.CreateBlob(fd._handle);

				_type = TransactionType.Modification;

				if (newData == Blob.NullBlob)
				{
					_type = TransactionType.Deletion;
				}
			}
		}


		public override void Apply(BlobManager blobs)
		{
			FileInfo? fi = null;
			switch (_type)
			{
				case TransactionType.Addition:
				case TransactionType.Modification:
					fi = blobs.BlobToFile(_newData);
					if (fi != null)
					{
						FileStream fs = fi.OpenRead();

						Stream output;
						if (_archive != null)
						{
							output = new MemoryStream();
						}
						else
						{
							igFileContext.Singleton.Open(_path, igFileContext.GetOpenFlags(FileAccess.Write, FileMode.Create), out igFileDescriptor fd, igBlockingType.kMayBlock, igFileWorkItem.Priority.kPriorityNormal);
							output = fd._stream.BaseStream;
						}

						output.SetLength(fs.Length);

						fs.Seek(0, SeekOrigin.Begin);
						output.Seek(0, SeekOrigin.Begin);
						fs.CopyTo(output);

						if (_archive != null)
						{
							_archive.Compress(_archive.GetAddFile(_path), output);
							output.Close();
						}
						else
						{
							Logging.Error("Unable to overwrite/create file {0} as the operation is not implemented", _path);
						}

						fs.Close();
					}
					break;
				case TransactionType.Deletion:
					if (_archive != null)
					{
						_archive.Delete(_path);
					}
					else
					{
						igFileContext.Singleton.Unlink(_path);
					}
					break;
			}
		}



		public override void Revert(BlobManager blobs)
		{
			FileInfo? fi = null;
			switch (_type)
			{
				case TransactionType.Addition:
					if (_archive != null)
					{
						_archive.Delete(_path);
					}
					else
					{
						igFileContext.Singleton.Unlink(_path);
					}
					break;
				case TransactionType.Modification:
				case TransactionType.Deletion:
					fi = blobs.BlobToFile(_oldData);
					if (fi != null)
					{
						FileStream fs = fi.OpenRead();

						Stream output;
						if (_archive != null)
						{
							output = new MemoryStream();
						}
						else
						{
							igFileContext.Singleton.Open(_path, igFileContext.GetOpenFlags(FileAccess.Write, FileMode.Create), out igFileDescriptor fd, igBlockingType.kMayBlock, igFileWorkItem.Priority.kPriorityNormal);
							output = fd._stream.BaseStream;
						}

						output.SetLength(fs.Length);

						fs.Seek(0, SeekOrigin.Begin);
						output.Seek(0, SeekOrigin.Begin);
						fs.CopyTo(output);

						if (_archive != null)
						{
							_archive.Compress(_archive.GetAddFile(_path), output);
							output.Close();
						}
						else
						{
							Logging.Error("Unable to overwrite/create file {0} as the operation is not implemented", _path);
						}

						fs.Close();
					}
					break;
			}
		}
	}
}