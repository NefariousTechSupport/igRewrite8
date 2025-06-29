/*
	Copyright (c) 2022-2025, The Potion Contributors.
	Potion and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using FluentFTP;
using igLibrary.Core;

namespace Potion
{
	/// <summary>
	/// connection class responsible or providing an interface for
	/// reading/writing files via the local file system
	/// </summary>
	public sealed class FileConnection : Connection
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public FileConnection() : base()
		{
		}



		/// <summary>
		/// Initialises a connection
		/// </summary>
		/// <returns>Whether the connection succeeded</returns>
		public override async Task<bool> Connect()
		{
			// There's no work to do here
			await Task.CompletedTask;

			return true;
		}



		/// <summary>
		/// Pulls a file from the source
		/// </summary>
		/// <param name="path">The file path to pull</param>
		/// <returns>A <c>Stream</c> containing the data, or null if the request failed</returns>
		public override async Task<Stream?> Pull(string path)
		{
			string? root = Params!.File!.Value.Root;
			Stream? data = null;

			if (root != null)
			{
				data = File.OpenRead(Path.Combine(root, path));
			}

			// No async work to do here
			await Task.CompletedTask;

			return data;
		}



		/// <summary>
		/// Checks whether the file exists
		/// </summary>
		/// <param name="path">The file path to check</param>
		/// <returns>Whether the remote file exists, or null if the request failed</returns>
		public override async Task<bool?> Exists(string path)
		{
			string? root = Params!.File!.Value.Root;
			bool? exists = false;

			if (root != null)
			{
				exists = File.Exists(Path.Combine(root, path));
			}

			// No async work to do here
			await Task.CompletedTask;

			return exists;
		}



		/// <summary>
		/// Grabs a list of files in the provided directory
		/// </summary>
		/// <param name="path">The file path to list</param>
		/// <returns>A list of filenames, or null if the request failed</returns>
		public override async Task<List<FileProps>?> ListDirectory(string path)
		{
			string? root = Params!.File!.Value.Root;
			List<FileProps>? files = null;

			if (root != null)
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(root, path));

				FileInfo[] dnFiles = directoryInfo.GetFiles();

				files = new List<FileProps>();

				for (int i = 0; i < dnFiles.Length; i++)
				{
					files.Add(new FileProps(dnFiles[i]));
				}
			}

			// No async work to do here
			await Task.CompletedTask;

			return files;
		}



		/// <summary>
		/// Pushes a file to the local source, overwriting any existing
		/// file there
		/// </summary>
		/// <param name="data">The <c>Stream</c> containing the data to push</param>
		/// <param name="path">The path to push the data to</param>
		/// <returns>Whether the task succeeded</returns>
		public override async Task<bool> Push(Stream data, string path)
		{
			string? root = Params!.File!.Value.Root;
			bool success = false;

			if (root != null)
			{
				FileStream output = File.Open(Path.Combine(root, path), FileMode.OpenOrCreate, FileAccess.Write);

				data.Seek(0, SeekOrigin.Begin);
				await data.CopyToAsync(output);

				output.Close();

				success = true;
			}

			return success;
		}
	}
}