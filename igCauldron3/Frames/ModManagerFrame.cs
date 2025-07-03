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
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace igCauldron3
{
	/// <summary>
	/// Main Mod Manager window
	/// </summary>
	public class ModManagerFrame : Frame
	{
		enum UIState
		{
			InstallPicker,
			InstallCreator,
			ModManager,
			ModCreator,
		}

		UIState                        mState;
		List<InstallationParams>       mInstallations;
		InstallationParams?            mWipInstallation;
		Installation?                  mInstall;
		WipModManifest?                mWipMod;
		ConfigFrame?                   mGamePicker;
		bool                           mSuccessfulRead;



		private class WipModManifest
		{
			public string              mIdentifier = string.Empty;
			public string              mName = string.Empty;
			public string              mAuthor = string.Empty;
			public string              mDesc = string.Empty;
			public igArkCore.EGame     mGame;
			public IG_CORE_PLATFORM    mPlatform;
		}


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
			mState           = UIState.InstallPicker;
			mInstallations   = new List<InstallationParams>();
			mWipInstallation = null;
			mInstall         = null;
			mWipMod          = null;
			mGamePicker      = null;

			mSuccessfulRead  = Read();
		}



		/// <summary>
		/// Attempts to read the installations file
		/// </summary>
		/// <returns>Whether the installations file was properly read</returns>
		private bool Read()
		{
			bool success = false;

			if (!File.Exists(CauldronConfig.InstallationsPath))
			{
				// Consider this a success
				return true;
			}

			try
			{
				FileStream fs = File.OpenRead(CauldronConfig.InstallationsPath);

				XmlSerializer xmlDeserializer = new XmlSerializer(typeof(List<InstallationParams>));
				List<InstallationParams>? parameters = xmlDeserializer.Deserialize(fs) as List<InstallationParams>;

				if (parameters != null)
				{
					mInstallations = parameters;

					success = true;
				}
			}
			catch (Exception)
			{
			}

			return success;
		}



		public override void Render()
		{
			// If the user is picking a game then disable the mod manger
			// if they cancel then we show ourselves
			if (mGamePicker != null)
			{
				bool closePicker = mGamePicker.UserCancelled || mGamePicker.UserMadeUpTheirMind;

				if (mGamePicker.UserMadeUpTheirMind)
				{
					_wnd._frames.Remove(this);
				}

				if (closePicker)
				{
					mGamePicker.Close();
					mGamePicker = null;
				}
				else
				{
					mGamePicker.Render();
					return;
				}
			}

			ImGui.Begin("Mod Manager", ImGuiWindowFlags.HorizontalScrollbar);

			if (!mSuccessfulRead)
			{
				ImGui.TextColored(Styles._errorTxt, $"Failed to read the installations file at {CauldronConfig.InstallationsPath}! You may wanna try fixing it manually or you can remove it");
			}

			switch (mState)
			{
				case UIState.InstallPicker:
					RenderPicker();
					break;

				case UIState.InstallCreator:
					RenderCreator();
					break;

				case UIState.ModManager:
					RenderModManager();
					break;

				case UIState.ModCreator:
					RenderModCreator();
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
						mState = UIState.ModManager;
					}
				}
			}

			if (ImGui.Button("+"))
			{
				mState = UIState.InstallCreator;
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
				mState = UIState.InstallPicker;
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
			InstallationParams.FileXml fileParams;
			if (parameters.File.HasValue)
			{
				fileParams = parameters.File.Value;
			}
			else
			{
				fileParams = new InstallationParams.FileXml();
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
			InstallationParams.FtpXml ftpParams;
			if (parameters.Ftp.HasValue)
			{
				ftpParams = parameters.Ftp.Value;
			}
			else
			{
				ftpParams          = new InstallationParams.FtpXml();
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
			InstallationParams.Ps3Xml ps3Params;
			if (parameters.Ps3.HasValue)
			{
				ps3Params = parameters.Ps3.Value;
			}
			else
			{
				ps3Params = new InstallationParams.Ps3Xml();
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
			if (mInstall == null)
			{
				mState = UIState.InstallPicker;
				return;
			}

			List<Installation.InstalledMod> mods = mInstall.Mods;
			for (int m = 0; m < mods.Count; m++)
			{
				ModManifest manifest = mods[m].mManifest;
				if (ImGui.TreeNode($"{manifest.Name}##{manifest.Identifier}${manifest.ModVersion}"))
				{
					ImGui.Text($"Author: {manifest.Author}");
					ImGui.Text($"Version: v{manifest.ModVersion}");
					ImGui.Text($"Identifier: {manifest.Identifier}");
					ImGui.Text($"Description: {manifest.Desc}");

					UIUtil.RenderBoolField("Enabled", "Enabled", ref mods[m].mEnabled);

					if (mInstall.Connection is FileConnection && ImGui.Button("Edit"))
					{
						mGamePicker = new ConfigFrame(_wnd, manifest, mInstall);
					}

					ImGui.TreePop();
				}
			}

			if (ImGui.Button("Create"))
			{
				mState = UIState.ModCreator;
			}
		}
#endregion // Mod Manager
#region Mod Creator
		public void RenderModCreator()
		{
			if (mInstall == null)
			{
				mState = UIState.InstallPicker;
				return;
			}

			if (mWipMod == null)
			{
				mWipMod = new WipModManifest();
			}

			UIUtil.RenderTextField("Unique ID",    "Unique ID",    ref mWipMod.mIdentifier, "com.<author>.<name>");
			UIUtil.RenderTextField("Display Name", "Display Name", ref mWipMod.mName);
			UIUtil.RenderTextField("Author",       "Author",       ref mWipMod.mAuthor, "<your name here>");
			UIUtil.RenderTextField("Description",  "Description",  ref mWipMod.mDesc);

			UIUtil.EnumComboBox("Game",     UIUtil.sGameNames,     ref mWipMod.mGame);
			UIUtil.EnumComboBox("Platform", UIUtil.sPlatformNames, ref mWipMod.mPlatform);

			bool valid = ModManifest.IsValidIdentifier(mWipMod.mIdentifier)
			          && mWipMod.mGame     != igArkCore.EGame.EV_None
			          && mWipMod.mPlatform != IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT;

			ImGui.BeginDisabled(!valid);
			bool createPressed = ImGui.Button("Create");
			ImGui.EndDisabled();

			if (valid && createPressed)
			{
				ModManifest manifest = new ModManifest(mWipMod.mGame, mWipMod.mPlatform);
				manifest.TrySetIdentifier(mWipMod.mIdentifier);
				manifest.Name   = mWipMod.mName;
				manifest.Author = mWipMod.mAuthor;
				manifest.Desc   = mWipMod.mDesc;

				// Defaulted this to true but we may wanna add a checkbox for it in case
				// people want to specify it
				Installation.InstalledMod installedMod = new Installation.InstalledMod(manifest, true);

				mInstall.Mods.Add(installedMod);

				mWipMod = null;
				mState = UIState.ModManager;
			}
		}
#endregion // Mod Manager
	}
}