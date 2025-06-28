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
	/// reading/writing files via FTP
	/// </summary>
	public sealed class FTPConnection : Connection
	{
		private AsyncFtpClient mClient;



		/// <summary>
		/// Constructor
		/// </summary>
		public FTPConnection() : base()
		{
			mClient = new AsyncFtpClient();
		}



		/// <summary>
		/// Initialises a connection
		/// </summary>
		/// <returns>Whether the connection succeeded</returns>
		public override async Task<bool> Connect()
		{
			if (mClient.IsConnected)
			{
				await mClient.Disconnect();
			}

			// Validation
			// TODO: Expose failures to the user
			bool success = Params != null && Params.Ftp.HasValue;
			string? hostname = null;
			string? username = null;
			string? password = null;
			int? port = 0;

			if (success)
			{
				var ftpParams = Params!.Ftp!.Value;

				hostname = ftpParams.Host;
				username = ftpParams.Username;
				password = ftpParams.Password;
				port = ftpParams.Port;

				success = hostname != null
					   && username != null
					   && password != null
					   && port != null;

				// There is also UriHostNameType.Basic, we may need to check for this
				success &= Uri.CheckHostName(hostname) != UriHostNameType.Unknown;

				success &= port > 0;
			}

			if (success)
			{
				mClient = new AsyncFtpClient(
					hostname,
					username,
					password,
					port!.Value
				);
			}

			await mClient.AutoConnect();

			return mClient.IsConnected;
		}



		/// <summary>
		/// Pulls a file from the remote source
		/// </summary>
		/// <param name="path">The file path to pull</param>
		/// <returns>A <c>Stream</c> containing the remote data, or null if the request failed</returns>
		public override async Task<Stream?> Pull(string path)
		{
			bool connected = mClient.IsConnected;
			if (!connected)
			{
				connected = await Connect();
			}

			Stream? data = null;

			if (connected)
			{
				string tempPath = Path.Combine(Path.GetTempPath(), igHash.Hash(path).ToString());
				await mClient.DownloadFile(tempPath, path, FtpLocalExists.Overwrite, FtpVerify.Retry);
				data = File.OpenRead(tempPath);
			}

			return data;
		}



		/// <summary>
		/// Pushes a file to the remote source, overwriting any existing
		/// file there
		/// </summary>
		/// <param name="data">The <c>Stream</c> containing the data to push</param>
		/// <param name="path">The path to push the data to</param>
		/// <returns>Whether the task succeeded</returns>
		public override async Task<bool> Push(Stream data, string path)
		{
			bool connected = mClient.IsConnected;
			if (!connected)
			{
				connected = await Connect();
			}

			bool success = false;

			if (connected)
			{
				string tempPath = Path.GetTempFileName();
				FileStream tempData = File.Open(tempPath, FileMode.OpenOrCreate, FileAccess.Write);
				data.Seek(0, SeekOrigin.Begin);
				data.CopyTo(tempData);
				tempData.Close();

				FtpStatus status = await mClient.UploadFile(tempPath, path, FtpRemoteExists.Overwrite, true, FtpVerify.Retry);

				success = status == FtpStatus.Success;
			}

			return success;
		}
	}
}