using System.Reflection;

namespace igLibrary.Core
{
	public class __internalObjectBase
	{
		public uint refCount;
		public igMemoryPool internalMemoryPool;
		internal bool dynamicMeta;
		internal igMetaObject? internalMeta;

		public virtual igMetaObject GetMeta()
		{
			if(internalMeta == null)
			{
				internalMeta = igArkCore.GetObjectMeta(GetType().Name);
				if(internalMeta == null) throw new TypeLoadException($"Failed to load meta for {GetType().Name}");
			}
			return internalMeta;
		}
	}
}