/*
	Copyright (c) 2022-2025, The Potion Contributors.
	Potion and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace Potion
{
	/// <summary>
	/// Abstract connection class responsible or providing an interface for
	/// reading/writing files
	/// </summary>
	public abstract class Connection
	{
		/// <summary>
		/// Parameters object, used for reading settings for the connection
		/// </summary>
		public InstallationParams? Params
		{
			get => mParams;
			set => mParams = value;
		}



		private InstallationParams? mParams;



		/// <summary>
		/// Initialises a connection
		/// </summary>
		/// <returns>Whether the connection succeeded</returns>
		public virtual Task<bool> Connect()
		{
			throw new NotImplementedException($"Method {GetType().Name}.Connect is not implemented");
		}



		/// <summary>
		/// Pulls a file from the (possibly) remote source
		/// </summary>
		/// <param name="path">The file path to pull</param>
		/// <returns>A <c>Stream</c> containing the remote data, or null if the request failed</returns>
		public virtual Task<Stream?> Pull(string path)
		{
			throw new NotImplementedException($"Method {GetType().Name}.Pull is not implemented");
		}



		/// <summary>
		/// Pushes a file to the (possibly) remote source, overwriting any
		/// existing file there
		/// </summary>
		/// <param name="data">The <c>Stream</c> containing the data to push</param>
		/// <param name="path">The path to push the data to</param>
		/// <returns>Whether the task succeeded</returns>
		public virtual Task<bool> Push(Stream data, string path)
		{
			throw new NotImplementedException($"Method {GetType().Name}.Push is not implemented");
		}
	}
}