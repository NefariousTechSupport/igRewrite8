/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	public class igFile : igObject
	{
		public igFileDescriptor _file;
		public long _offset = 0;
		public igFileWorkItem.Priority _priority = igFileWorkItem.Priority.kPriorityNormal;
		public void Open(string path)
		{
			igFileContext.Singleton.Open(path, igFileContext.GetOpenFlags(FileAccess.Read, FileMode.Open), out _file, igBlockingType.kBlocking, _priority);
		}
	}
}