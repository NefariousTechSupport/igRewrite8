
namespace igLibrary.Core
{
	public class igOrderedMapMetaField : igCompoundMetaField
	{
		public igMetaField _t;
		public igMetaField _u;
		private List<igMetaField>? _internalFieldList;
		public override void SetTemplateParameter(uint index, igMetaField meta)
		{
			if(index > 1 || index < 0) throw new IndexOutOfRangeException("igOrderedMap only has 2 template arguments");

			if(index == 0) _t = meta;
			if(index == 1) _u = meta;
		}
		public override void SetTemplateParameterCount(uint count)
		{
			if(count != 2) throw new IndexOutOfRangeException("igOrderedMap can only have 2 template arguments");
		}
		public override igMetaField GetTemplateParameter(uint index)
		{
			if(index > 1 || index < 0) throw new IndexOutOfRangeException("igOrderedMap only has 2 template arguments");

			if(index == 0) return _t;
			else return _u;
		}
		/*public override object? ReadIGZField(igIGZLoader loader)
		{
			igVectorMetaField _0 = (igVectorMetaField)_compoundFieldInfo._fieldList[0];
			igVectorMetaField _1 = (igVectorMetaField)_compoundFieldInfo._fieldList[1];
			_0._memType = _t;
			_1._memType = _u;
			object? data = base.ReadIGZField(loader);
			_0._memType = null;
			_1._memType = null;
			return data;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			igVectorMetaField _0 = (igVectorMetaField)_compoundFieldInfo._fieldList[0];
			igVectorMetaField _1 = (igVectorMetaField)_compoundFieldInfo._fieldList[1];
			_0._memType = _t;
			_1._memType = _u;
			base.WriteIGZField(saver, section, value);
			_0._memType = null;
			_1._memType = null;
		}
		public override object? GetDefault(igMemoryPool pool)
		{
			igVectorMetaField _0 = (igVectorMetaField)_compoundFieldInfo._fieldList[0];
			igVectorMetaField _1 = (igVectorMetaField)_compoundFieldInfo._fieldList[1];
			_0._memType = _t;
			_1._memType = _u;
			object? ret = base.GetDefault(pool);
			_0._memType = null;
			_1._memType = null;
			return ret;
		}*/
		public override uint GetTemplateParameterCount() => 2;
		public override List<igMetaField> GetFieldList()
		{
			if(_internalFieldList == null)
			{
				_internalFieldList = new List<igMetaField>();
				_internalFieldList.Add(_compoundFieldInfo._fieldList[0].CreateFieldCopy());
				_internalFieldList.Add(_compoundFieldInfo._fieldList[1].CreateFieldCopy());
				_internalFieldList[0].SetTemplateParameter(0, _t);
				_internalFieldList[1].SetTemplateParameter(0, _u);
				_internalFieldList[0]._fieldHandle = GetOutputType().GetField("_unk0");
				_internalFieldList[1]._fieldHandle = GetOutputType().GetField("_unk1");
			}
			return _internalFieldList;
		}
		public override Type GetOutputType()
		{
			return typeof(igOrderedMap<,>).MakeGenericType(_t.GetOutputType(), _u.GetOutputType());
		}
		public override void UndumpArkData(igArkCoreFile loader, StreamHelper sh)
		{
			base.UndumpArkData(loader, sh);

			_compoundFieldInfo = igArkCore.GetCompoundFieldInfo("igOrderedMapMetaField");
		}
	}
}