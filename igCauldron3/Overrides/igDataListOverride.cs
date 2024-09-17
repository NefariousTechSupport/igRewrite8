using igLibrary.Core;

namespace igCauldron3
{
	public class igDataListOverride : InspectorDrawOverride
	{
		public igDataListOverride()
		{
			_t = typeof(IigDataList);
		}
		public override void Draw2(DirectoryManagerFrame dirFrame, string id, igObject obj, igMetaObject meta)
		{
			IigDataList dataList = (IigDataList)obj;
			IigMemory memValue = dataList.GetData();
			object? castedObject = memValue;
			FieldRenderer.RenderField(id, "Data", castedObject, meta._metaFields[2], (value) => {
				dataList.SetData(memValue);
				dataList.SetCount(memValue.GetData().Length);
				dataList.SetCapacity(memValue.GetData().Length);
			});
		}
	}
}