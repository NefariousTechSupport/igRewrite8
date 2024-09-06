using HashDepot;
using System.Text;

namespace igLibrary.Core
{
	public static class igHash
	{
		public static uint Hash(byte[] input) => Fnv1a.Hash32(input);
		public static uint Hash(string input)
		{
			return Fnv1a.Hash32(Encoding.ASCII.GetBytes(input));
		}
		public static uint HashI(string input)
		{
			return Fnv1a.Hash32(Encoding.ASCII.GetBytes(input.ToLower()));
		}
	}
}