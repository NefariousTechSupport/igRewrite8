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
	public abstract class igRefMetaField : igMetaField
	{
		public bool _construct;
		public bool _destruct;
		public bool _reconstruct;
		public bool _refCounted = true;

		public override void DumpArkData(igArkCoreFile saver, StreamHelper sh)
		{
			base.DumpArkData(saver, sh);
			sh.WriteByte((byte)((_construct ? 0b1000 : 0u) | (_destruct ? 0b0100 : 0u) | (_reconstruct ? 0b0010 : 0u) | (_refCounted ? 0b0001 : 0u)));
		}
		public override void UndumpArkData(igArkCoreFile loader, StreamHelper sh)
		{
			base.UndumpArkData(loader, sh);
			byte data = sh.ReadByte();
			_construct   = ((data >> 3) & 1) != 0;
			_destruct    = ((data >> 2) & 1) != 0;
			_reconstruct = ((data >> 1) & 1) != 0;
			_refCounted  = ((data >> 0) & 1) != 0;
		}

		public override uint GetSize(IG_CORE_PLATFORM platform) => igAlchemyCore.GetPointerSize(platform);
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => igAlchemyCore.GetPointerSize(platform);
	}
}