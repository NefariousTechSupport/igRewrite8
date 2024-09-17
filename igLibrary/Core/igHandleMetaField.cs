using System.Reflection;

namespace igLibrary.Core
{
	public class igHandleMetaField : igRefMetaField
	{
		public igMetaObject _metaObject;

		public override void DumpArkData(igArkCoreFile saver, StreamHelper sh)
		{
			base.DumpArkData(saver, sh);

			saver.SaveString(sh, _metaObject._name);
		}
		public override void UndumpArkData(igArkCoreFile loader, StreamHelper sh)
		{
			base.UndumpArkData(loader, sh);

			_metaObject = igArkCore.GetObjectMeta(loader.ReadString(sh));
		}
		public override object ReadIGZField(igIGZLoader loader)
		{
			if(loader._runtimeFields._handles.BinarySearch(loader._stream.Tell64()) < 0) return null;

			uint raw = loader._stream.ReadUInt32();
			if((raw & 0x80000000) != 0)
			{
				return loader._namedHandleList[(int)(raw & 0x3FFFFFFF)];
			}
			else
			{
				return loader._externalList[(int)(raw & 0x3FFFFFFF)];
			}
		}

		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			if(value == null) return;

			igHandle hnd = (igHandle)value;
			bool namedExternal = hnd._alias._string != null && hnd._namespace._string != null;
			List<igHandle> handleList = null;

			if(!namedExternal) handleList = saver._externalList;
			else
			{
				//I hate this
				handleList = new List<igHandle>();
				for(int i = 0; i < saver._namedList.Count; i++)
				{
					if(!saver._namedList[i].Item2) continue;
					handleList.Add(saver._namedList[i].Item1);
				}
			}

			int handleIndex = handleList.FindIndex(x => x == hnd);
			if(handleIndex < 0)
			{
				handleIndex = handleList.Count;
				if(!namedExternal) handleList.Add(hnd);
				else               saver._namedList.Add((hnd, true));
			}
			section._runtimeFields._handles.Add(section._sh.Tell64());
			if(namedExternal) section._sh.WriteUInt32(0x80000000u | (uint)handleIndex);
			else              section._sh.WriteUInt32((uint)handleIndex);
		}
		public override Type GetOutputType() => typeof(igHandle);
	}
}