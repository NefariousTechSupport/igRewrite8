/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	/// <summary>
	/// Exception for when the tool fails to read a field
	/// </summary>
	public class FieldReadException : Exception
	{
		public new Exception InnerException { get; private set; }
		public string FilePath { get; private set; }
		public uint Offset { get; private set; }
		public igMetaObject MetaObject { get; private set; }
		public igMetaField Field { get; private set; }
		public override string Message
		{
			get
			{
				return $"Failed to read {MetaObject._name}::{Field._fieldName} from file {FilePath} at {Offset.ToString("X08")}.\n{InnerException.Message}";
			}
		}
		public override string? StackTrace => InnerException.StackTrace;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="innerException">The root exception</param>
		/// <param name="filePath">The path of the file</param>
		/// <param name="offset">The offset within the file</param>
		/// <param name="metaObject">The metaobject that failed to be read</param>
		/// <param name="field">The field that failed to be read</param>
		public FieldReadException(Exception innerException, string filePath, uint offset, igMetaObject metaObject, igMetaField field)
		{
			InnerException = innerException;
			FilePath = filePath;
			Offset = offset;
			MetaObject = metaObject;
			Field = field;
		}
	}
}