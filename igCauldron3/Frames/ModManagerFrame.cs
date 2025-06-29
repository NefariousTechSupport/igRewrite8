/*
	Copyright (c) 2022-2025, The igCauldron Contributors.
	igCauldron and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using ImGuiNET;
using igLibrary.Core;
using Potion;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using System.Text.RegularExpressions;

namespace igCauldron3
{
	/// <summary>
	/// Main Mod Manager window
	/// </summary>
	public class ModManagerFrame : Frame
	{
		enum UIState
		{
			Picker,
			Creator,
			Manager,
		}

		UIState                        mState;
		List<InstallationParams>       mInstallations;
		InstallationParams?            mWipInstallation;
		Installation?                  mInstall;
		bool                           mSuccessfulRead;



		public static readonly (EConnectionType, string)[] sConnectionNames = new (EConnectionType, string)[]
		{
			(EConnectionType.kNone,    "Please select a connection type"),
			(EConnectionType.kFile,    "Local file system"),
			(EConnectionType.kFtp,     "FTP")
		};



		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="wnd">The window this frame belongs to</param>
		public ModManagerFrame(Window wnd) : base(wnd)
		{
			mState           = UIState.Picker;
			mInstallations   = new List<InstallationParams>();
			mWipInstallation = null;

			mSuccessfulRead  = Read();
		}



		/// <summary>
		/// Attempts to read the installations file
		/// </summary>
		/// <returns>Whether the installations file was properly read</returns>
		private bool Read()
		{
			if (!File.Exists(CauldronConfig.InstallationsPath))
			{
				// Consider this a success
				return true;
			}

			try
			{
				string input = File.ReadAllText(CauldronConfig.InstallationsPath);

				Deserializer yamlDeserializer = new Deserializer();
				mInstallations = yamlDeserializer.Deserialize<List<InstallationParams>>(input);
			}
			catch (Exception)
			{
				return false;
			}

			return true;
		}



		public override void Render()
		{
			ImGui.Begin("Mod Manager", ImGuiWindowFlags.HorizontalScrollbar);

			if (!mSuccessfulRead)
			{
				ImGui.TextColored(Styles._errorTxt, $"Failed to read the installations file at {CauldronConfig.InstallationsPath}! You may wanna try fixing it manually or you can remove it");
			}

			switch (mState)
			{
				case UIState.Picker:
					RenderPicker();
					break;

				case UIState.Creator:
					RenderCreator();
					break;
			}

			ImGui.End();
		}
#region Installation Picker
		private void RenderPicker()
		{
			for (int i = 0; i < mInstallations.Count; i++)
			{
				if (ImGui.CollapsingHeader($"Installation {i}"))
				{
					InstallationParams parameters = mInstallations[i];

					ImGui.Text($"Connection Type: {parameters.ConnectionType.ToString()}");
					ImGui.Text($"Platform: {parameters.Platform.ToString()}");

					if (ImGui.Button("Open"))
					{
						mInstall = new Installation(parameters);
						mInstall.Open().Wait();
					}
				}
			}

			if (ImGui.Button("+"))
			{
				mState = UIState.Creator;
			}
		}

#endregion // Installation Picker
#region Installation Creator
		private void RenderCreator()
		{
			if (mWipInstallation == null)
			{
				mWipInstallation = new InstallationParams();
			}

			bool valid = true;

			UIUtil.EnumComboBox("Connection Type", sConnectionNames,      ref mWipInstallation.ConnectionType);
			UIUtil.EnumComboBox("Platform",        UIUtil.sPlatformNames, ref mWipInstallation.Platform);

			valid &= mWipInstallation.ConnectionType != EConnectionType.kNone;
			valid &= mWipInstallation.Platform       != IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT;

			ImGui.NewLine();
			ImGui.Text("Connection Configuration");
			ImGui.NewLine();

			switch (mWipInstallation.ConnectionType)
			{
				case EConnectionType.kFile:
					RenderFileConfig(mWipInstallation, ref valid);
					break;
				case EConnectionType.kFtp:
					RenderFtpConfig(mWipInstallation, ref valid);
					break;
			}

			ImGui.NewLine();
			ImGui.Text("Platform Configuration");
			ImGui.NewLine();

			switch (mWipInstallation.Platform)
			{
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_PS3:
					RenderPS3Config(mWipInstallation, ref valid);
					break;
			}

			ImGui.BeginDisabled(!valid);
			if (ImGui.Button("Create"))
			{
				mInstallations.Add(mWipInstallation);
				mWipInstallation = null;
				mState = UIState.Picker;
			}
			ImGui.EndDisabled();
		}



		/// <summary>
		/// Render the local file system configuration data
		/// </summary>
		/// <param name="parameters">The parameters object to edit</param>
		/// <param name="valid">Whether or not the configuration is valid</param>
		private void RenderFileConfig(InstallationParams parameters, ref bool valid)
		{
			InstallationParams.FileYaml fileParams;
			if (parameters.File.HasValue)
			{
				fileParams = parameters.File.Value;
			}
			else
			{
				fileParams = new InstallationParams.FileYaml();
				fileParams.Root = string.Empty;
			}

			string preview;
			switch (parameters.Platform)
			{
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_PS3:
					preview = "<rpcs3 folder>/dev_hdd0/game/<titleid>/mods";
					break;
				default:
					preview = string.Empty;
					break;
			}

			UIUtil.RenderTextField("Mod loader root folder", "file.root", ref fileParams.Root!, preview);

			valid &= Path.IsPathFullyQualified(fileParams.Root) || Path.IsPathRooted(fileParams.Root);

			parameters.File = fileParams;
		}



		/// <summary>
		/// Render the FTP configuration data
		/// </summary>
		/// <param name="parameters">The parameters object to edit</param>
		/// <param name="valid">Whether or not the configuration is valid</param>
		private void RenderFtpConfig(InstallationParams parameters, ref bool valid)
		{
			InstallationParams.FtpYaml ftpParams;
			if (parameters.Ftp.HasValue)
			{
				ftpParams = parameters.Ftp.Value;
			}
			else
			{
				ftpParams          = new InstallationParams.FtpYaml();
				ftpParams.Host     = string.Empty;
				ftpParams.Username = string.Empty;
				ftpParams.Password = string.Empty;
				ftpParams.Port     = 1;
				ftpParams.Root     = string.Empty;
			}

			int port = ftpParams.Port!.Value;

			ImGui.Text("igCauldron stores FTP passwords in plaintext, only use anonymous passwords with this!");
			UIUtil.RenderTextField("FTP Hostname", "ftp.hostname", ref ftpParams.Host!);
			UIUtil.RenderTextField("FTP Username", "ftp.username", ref ftpParams.Username!);
			UIUtil.RenderTextField("FTP Password", "ftp.password", ref ftpParams.Password!);
			UIUtil.RenderIntField( "FTP Port",     "ftp.port",     ref port, 1, ushort.MaxValue);

			// We hardcode the Ps3 mod folder
			switch (parameters.Platform)
			{
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_PS3:
					break;
				default:
					UIUtil.RenderTextField("FTP Root", "ftp.root", ref ftpParams.Root!);
					break;
			}


			valid &= Uri.CheckHostName(ftpParams.Host) != UriHostNameType.Unknown;

			ftpParams.Port = port;
			parameters.Ftp = ftpParams;
		}



		/// <summary>
		/// Render the ps3 configuration data
		/// </summary>
		/// <param name="parameters">The parameters object to edit</param>
		/// <param name="valid">Whether or not the configuration is valid</param>
		private void RenderPS3Config(InstallationParams parameters, ref bool valid)
		{
			InstallationParams.Ps3Yaml ps3Params;
			if (parameters.Ps3.HasValue)
			{
				ps3Params = parameters.Ps3.Value;
			}
			else
			{
				ps3Params = new InstallationParams.Ps3Yaml();
				ps3Params.TitleId = string.Empty;
			}

			UIUtil.RenderTextField("PS3 Title ID", "ps3.titleid", ref ps3Params.TitleId!);

			// See https://www.psdevwiki.com/ps3/TITLE_ID
			valid &= Regex.IsMatch(ps3Params.TitleId, "(((B|X)(C|L)[ACEHJKU][BCDMSTVXZ])|(NP[AEHJKUIX][A-Z]))[0-9]{5}");

			parameters.Ps3 = ps3Params;
		}
#endregion // Installation Creator
#region Mod Manager
		public void RenderModManager()
		{
			
		}
#endregion // Mod Manager
	}
}