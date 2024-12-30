using System.Security.Cryptography;

namespace Potion
{
	public class BlobManager
	{
		private DirectoryInfo _blobDirectory;


		private BlobManager(DirectoryInfo blobDirectory)
		{
			_blobDirectory = blobDirectory;
		}


		public static BlobManager? Construct(string filePath)
		{
			try
			{
				return new BlobManager(Directory.CreateDirectory(filePath));
			}
			catch (Exception)
			{
				return null;
			}
		}


		public Blob CreateBlob(Stream stream)
		{
			SHA1 sha1 = SHA1.Create();

			stream.Seek(0, SeekOrigin.Begin);
			byte[] data = new byte[stream.Length];
			stream.Read(data);

			byte[] hash = sha1.ComputeHash(data);
			string filename = Convert.ToHexString(hash);

			string fullpath = Path.Combine(_blobDirectory.FullName, filename);
			if (!File.Exists(fullpath))
			{
				FileStream fs = File.Create(fullpath);
				fs.Write(data);
				fs.Close();
			}

			return Blob.Construct(hash)!.Value;
		}


		public FileInfo? BlobToFile(Blob blob)
		{
			string filename = Convert.ToHexString(blob.Sha1.ToArray());

			string fullpath = Path.Combine(_blobDirectory.FullName, filename);

			if (!File.Exists(fullpath))
			{
				return null;
			}

			return new FileInfo(fullpath);
		}
	}
}
