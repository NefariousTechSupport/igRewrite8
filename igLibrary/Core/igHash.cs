using HashDepot;
using System.Text;

namespace igLibrary.Core
{
	/// <summary>
	/// Wrapper for Fnv1a hashing
	/// </summary>
	public static class igHash
	{
		/// <summary>
		/// Hashes a string to a 32 bit uint with Fnv1a
		/// </summary>
		/// <param name="input">The string</param>
		/// <returns>The hash</returns>
		public static uint Hash(string input)
		{
			return Fnv1a.Hash32(Encoding.ASCII.GetBytes(input));
		}


		/// <summary>
		/// Hashes a byte array to a 32 bit uint with Fnv1a
		/// </summary>
		/// <param name="input">The byte array</param>
		/// <returns>The hash</returns>
		public static uint Hash(byte[] input)
		{
			return Fnv1a.Hash32(input);
		}


		/// <summary>
		/// Hashes a string as lowercase to a 32 bit uint with Fnv1a
		/// </summary>
		/// <param name="input">The string</param>
		/// <returns>The hash</returns>
		public static uint HashI(string input)
		{
			return Fnv1a.Hash32(Encoding.ASCII.GetBytes(input.ToLower()));
		}
	}
}