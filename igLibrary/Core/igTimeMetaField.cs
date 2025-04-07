/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	public class igTimeMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader) => new igTime(loader._stream.ReadSingle());
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value) => section._sh.WriteSingle(((igTime)value!)._elapsedDays);
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 4;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 4;
		public override Type GetOutputType() => typeof(igTime);


		/// <summary>
		/// Sets the target variable based on the string representation of the input
		/// </summary>
		/// <param name="target">The output field</param>
		/// <param name="input">The input field</param>
		/// <returns>boolean indicating whether the input was read successfully</returns>
		public override bool SetMemoryFromString(ref object? target, string input)
		{
			if (!float.TryParse(input, out float buffer))
			{
				return false;
			}
			igTime tempTarget;
			tempTarget._elapsedDays = buffer >= 0 ? buffer * 8192 : -1;
			target = tempTarget;
			return true;
		}
	}
}