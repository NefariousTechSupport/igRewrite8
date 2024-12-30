using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using igLibrary.Core;

namespace Potion
{
	/// <summary>
	/// Represents an identifier for binary data
	/// </summary>
	public struct Blob
	{
		/// <summary>
		/// A blob that represents nothing
		/// </summary>
		public static readonly Blob NullBlob = new Blob { _sha1 = new byte[SHA1.HashSizeInBytes] };


		private byte[] _sha1;


		/// <summary>
		/// The sha1 hash for the blob
		/// </summary>
		public byte[] Sha1 => _sha1;


		/// <summary>
		/// Private constructor for the blob
		/// </summary>
		/// <param name="sha1">The 20 byte long array</param>
		private Blob(byte[] sha1)
		{
			_sha1 = sha1;
		}


		/// <summary>
		/// Create a new blob
		/// </summary>
		/// <param name="sha1">The hex string for the blob, must be 40 characters</param>
		/// <returns>The blob on success, or null on failure</returns>
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


		/// <summary>
		/// Create a new blob
		/// </summary>
		/// <param name="sha1">The hex byte array for the blob, must be 20 bytes</param>
		/// <returns>The blob on success, or null on failure</returns>
		public static Blob? Construct(byte[] sha1)
		{
			if (sha1 == null
			 || sha1.Length != SHA1.HashSizeInBytes)
			{
				return null;
			}

			return new Blob(sha1);
		}


		/// <summary>
		/// Whether the two are equal
		/// </summary>
		/// <param name="a">The first blob</param>
		/// <param name="b">The second blob</param>
		/// <returns>Whether the two are equal</returns>
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


		/// <summary>
		/// Whether the two aren't equal
		/// </summary>
		/// <param name="a">The first blob</param>
		/// <param name="b">The second blob</param>
		/// <returns>Whether the two aren't equal</returns>
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


		/// <summary>
		/// Converts this blob into its string representation
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return Convert.ToHexString(_sha1);
		}


		/// <summary>
		/// Whether or not this object equals the parameter
		/// </summary>
		/// <param name="obj">The object to compare with</param>
		/// <returns>Whether the two are equal</returns>
		public override bool Equals([NotNullWhen(true)] object? obj)
		{
			if (obj is Blob blob)
			{
				return this == blob;
			}

			return false;
		}


		/// <summary>
		/// Compute an Fnv1a hash of the blob
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return unchecked((int)igHash.Hash(_sha1));
		}
	}
}