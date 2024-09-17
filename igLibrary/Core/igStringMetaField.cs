namespace igLibrary.Core
{
	public class igStringMetaField : igRefMetaField
	{
		private static igStringMetaField _MetaField = new igStringMetaField();
		public static igStringMetaField GetMetaField() => _MetaField;
		public override object? ReadIGZField(igIGZLoader loader)
		{
			uint basePos = loader._stream.Tell();
			bool isRef = loader._runtimeFields._stringRefs.BinarySearch(basePos) >= 0;
			bool isTable = loader._runtimeFields._stringTables.BinarySearch(basePos) >= 0;

			string? data = null;
			ulong raw = loader.ReadRawOffset();

			if(isRef)
			{
				ulong offset = loader.DeserializeOffset(raw);
				loader._stream.Seek(offset);
				data = loader._stream.ReadString();
			}
			else if (isTable)
			{
				data = loader._stringList[(int)raw];
			}
			
			loader._stream.Seek(basePos + igAlchemyCore.GetPointerSize(loader._platform));

			return data;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			uint basePos = section._sh.Tell();
			
			if(value == null)
			{
				saver.WriteRawOffset(0, section);
			}
			else
			{
				igBuildDependencyAttribute? dependencyAttribute1 = GetAttribute<igBuildDependencyAttribute>();
				CBuildDependencyAttribute? dependencyAttribute2 = GetAttribute<CBuildDependencyAttribute>();
				if(dependencyAttribute1 != null) dependencyAttribute1.GenerateBuildDependancies(saver, (string)value);
				if(dependencyAttribute2 != null) dependencyAttribute2.GenerateBuildDependancies(saver, value);

				//why use string refs when you can use the string table
				//Because alchemy sucks
				if(igAlchemyCore.isPlatform64Bit(saver._platform))
				{
					int index = saver._stringList.FindIndex(x => x == (string)value);
					if(index < 0)
					{
						index = saver._stringList.Count;
						saver._stringList.Add((string)value);
					}
					saver.WriteRawOffset((ulong)index, section);
					section._runtimeFields._stringTables.Add(basePos);
				}
				else
				{
					if(!saver._stringRefList.TryGetValue((string)value, out uint offset))
					{
						igIGZSaver.SaverSection stringSection = saver.GetSaverSection(igMemoryContext.Singleton.GetMemoryPoolByName("String"));
						stringSection._sh.Seek(stringSection.FindFreeMemory((byte)((saver._version < 7) ? 1u : 2u)));
						offset = saver.SerializeOffset(stringSection._sh.Tell(), stringSection);
						saver._stringRefList.Add((string)value, offset);
						stringSection._sh.WriteString((string)value);
						stringSection.PushAlignment((saver._version < 7) ? 1u : 2u);
					}
					section._runtimeFields._stringRefs.Add(basePos);
					section._sh.WriteUInt32(offset);
				}
			}
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => igAlchemyCore.GetPointerSize(platform);
		public override uint GetSize(IG_CORE_PLATFORM platform) => igAlchemyCore.GetPointerSize(platform);
		public override Type GetOutputType() => typeof(string);
		public override void DumpDefault(igArkCoreFile saver, StreamHelper sh)
		{
			sh.WriteUInt32(4);
			saver.SaveString(sh, (string?)_default);
		}
		public override void UndumpDefault(igArkCoreFile loader, StreamHelper sh)
		{
			_default = loader.ReadString(sh);
		}
	}
}