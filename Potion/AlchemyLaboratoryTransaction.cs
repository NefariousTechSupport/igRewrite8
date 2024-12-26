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


		public override void Revert(BlobManager blobs)
		{
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
					FileInfo? fi = blobs.BlobToFile(_oldData);
					if (fi != null)
					{
						FileStream fs = fi.OpenRead();
						fs.Seek(0, SeekOrigin.Begin);
						igFileContext.Singleton.
					}
					break;
			}
		}
	}
}