using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Potion
{
	public struct Blob
	{
		public static readonly Blob NullBlob = new Blob { _sha1 = new byte[SHA1.HashSizeInBytes] };


		private byte[] _sha1;

		public byte[] Sha1 => _sha1;


		private Blob(byte[] sha1)
		{
			_sha1 = sha1;
		}


		public static Blob? Construct(string hexString)
		{
			if (string.IsNullOrEmpty(hexString)
			 || hexString.Length != SHA1.HashSizeInBytes * 2
			 || !Regex.IsMatch(hexString, "^[0-9a-fA-F]{40}$"))
			{
				return null;
			}

			byte[] sha1 = new byte[SHA1.HashSizeInBytes];
			for(int i = 0; i < SHA1.HashSizeInBytes; i++)
			{
				sha1[i] = Convert.ToByte(hexString.Substring(i*2, 2), 16);
			}

			return new Blob(sha1);
		}


		public static Blob? Construct(byte[] sha1)
		{
			if (sha1 == null
			 || sha1.Length != SHA1.HashSizeInBytes)
			{
				return null;
			}

			return new Blob(sha1);
		}


		public static bool operator==(Blob a, Blob b)
		{
			for (int i = 0; i < SHA1.HashSizeInBytes; i++)
			{
				if (a._sha1[i] != b._sha1[i])
				{
					return false;
				}
			}

			return true;
		}


		public static bool operator!=(Blob a, Blob b)
		{
			for (int i = 0; i < SHA1.HashSizeInBytes; i++)
			{
				if (a._sha1[i] == b._sha1[i])
				{
					return false;
				}
			}

			return true;
		}


		public override string ToString()
		{
			return Convert.ToHexString(_sha1);
		}
	}
}