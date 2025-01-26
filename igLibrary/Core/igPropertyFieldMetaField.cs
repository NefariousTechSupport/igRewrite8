/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Reflection;

namespace igLibrary.Core
{
	public class igPropertyFieldMetaField : igMetaField
	{
		public igMetaField _innerMetaField;
		[Obsolete("This exists for the reflection system, do not use.")] public object? _setCallbackFunction;
		[Obsolete("This exists for the reflection system, do not use.")] public object? _getCallbackFunction;

		public override void DumpArkData(igArkCoreFile saver, StreamHelper sh)
		{
			base.DumpArkData(saver, sh);

			saver.SaveMetaField(sh, _innerMetaField);
		}
		public override void UndumpArkData(igArkCoreFile loader, StreamHelper sh)
		{
			base.UndumpArkData(loader, sh);

			_innerMetaField = loader.ReadMetaField(sh);
		}

		// These two aren't meant to be used
		public override object? ReadIGZField(igIGZLoader loader) => null;
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value) {}

		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0;
		public override Type GetOutputType() => _innerMetaField.GetOutputType();
	}
}