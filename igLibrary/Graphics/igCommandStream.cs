/*
	Copyright (c) 2022-2026, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Graphics
{
	public class igCommandStream : igObject
	{
		public ulong _writeHead;
		public ulong _writeChunkBegin;
		public ulong _writeChunkEnd;
		public ulong _readHead;
		public ulong _readChunkBegin;
		public ulong _readChunkEnd;
	}
}