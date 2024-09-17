using System.Reflection;

namespace igLibrary.Core
{
	public class igObject : __internalObjectBase
	{
		public virtual void ReadIGZFields(igIGZLoader loader)
		{
			uint objectOffset = loader._stream.Tell();

			igMetaObject meta = GetMeta();
			List<igMetaField> metaFields = meta._metaFields;

			for(int i = 0; i < metaFields.Count; i++)
			{
				if(metaFields[i] is igStaticMetaField) continue;
				if(metaFields[i] is igPropertyFieldMetaField) continue;
				if(!metaFields[i].IsApplicableForPlatform(loader._platform)) continue;

				//if(!metaFields[i]._properties._persistent) continue;

#if !DEBUG
				try
				{
#endif
					loader._stream.Seek(objectOffset + metaFields[i]._offsets[loader._platform]);

					object? data = metaFields[i].ReadIGZField(loader);

					FieldInfo? field = metaFields[i]._fieldHandle;
					if(field != null)
					{
						field.SetValue(this, data);
					}
#if !DEBUG
				}
				catch(Exception e)
				{
					throw new FieldReadException(e, loader._dir._path, loader._stream.Tell(), meta, metaFields[i]);
				}
#endif
			}
		}
		public void GetDependencies(IG_CORE_PLATFORM platform, igObjectDirectory directory, out igStringRefList? buildDeps, out igStringRefList? fileDeps)
		{
			buildDeps = null;
			fileDeps = null;
			igDependenciesAttribute? depAttr = GetMeta().GetAttribute<igDependenciesAttribute>();
			if(depAttr == null) return;
			igDependencyProvider depProvider = (igDependencyProvider)depAttr._value.ConstructInstance(igMemoryContext.Singleton.GetMemoryPoolByName("Default"));
			depProvider._platform = platform;
			depProvider._directory = directory;
			depProvider.GetBuildDependencies(this, out buildDeps);
			depProvider.GetFileDependencies(this, out fileDeps);
		}
		public virtual void WriteIGZFields(igIGZSaver saver, igIGZSaver.SaverSection section)
		{
			igMetaObject meta = GetMeta();
			List<igMetaField> metaFields = GetMeta()._metaFields;
			WriteIGZFieldsInternal(saver, section, metaFields);
			GetDependencies(saver._platform, saver._dir, out igStringRefList? buildDeps, out igStringRefList? fileDeps);
			if(buildDeps != null)
			{
				for(int i = 0; i < buildDeps._count; i++)
				{
					saver.AddBuildDependency(buildDeps[i]);
				}
			}
			if(fileDeps != null)
			{
				for(int i = 0; i < fileDeps._count; i++)
				{
					saver.AddFileDependency(fileDeps[i]);
				}
			}			
		}
		public virtual void WriteIGZFieldsInternal(igIGZSaver saver, igIGZSaver.SaverSection section, List<igMetaField> metaFields)
		{
			uint objectOffset = section._sh.Tell();

			for(int i = 0; i < metaFields.Count; i++)
			{
				if(metaFields[i] is igStaticMetaField) continue;
				if(metaFields[i] is igPropertyFieldMetaField) continue;
				if(!metaFields[i].IsApplicableForPlatform(saver._platform)) continue;

				section.PushAlignment(metaFields[i].GetAlignment(saver._platform));

				object? data = null;

				if(metaFields[i]._properties._persistent)
				{
					FieldInfo? field = metaFields[i]._fieldHandle;

					if(field == null) continue;

					data = field.GetValue(this);
				}
				else
				{
					data = metaFields[i].GetDefault(internalMemoryPool);
					if((metaFields[i].GetOutputType().IsValueType || metaFields[i].GetOutputType() == typeof(string)) && data == null) continue;
				}
				
				section._sh.Seek(objectOffset + metaFields[i]._offsets[saver._platform]);

				metaFields[i].WriteIGZField(saver, section, data);
			}
		}
		public virtual void ResetFields() => ResetFields(GetMeta());
		public virtual void ResetFields(igMetaObject meta)
		{
			for(int i = 0; i < meta._metaFields.Count; i++)
			{
				if(meta._metaFields[i] is igStaticMetaField || meta._metaFields[i] is igPropertyFieldMetaField || meta._metaFields[i] is igBitFieldMetaField) continue;

				FieldInfo? field = meta._metaFields[i]._fieldHandle;

				object? data = meta._metaFields[i].GetDefault(internalMemoryPool);

				if(meta._metaFields[i].GetOutputType().IsValueType && data == null) continue;

				field.SetValue(this, data);
			}
		}
	}
}