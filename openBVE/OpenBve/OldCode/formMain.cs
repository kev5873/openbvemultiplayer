﻿using System;
using System.Drawing;
using System.Windows.Forms;
using Tao.Sdl;
using System.Text;

namespace OpenBve {
	internal partial class formMain : Form {
		internal formMain() {
			InitializeComponent();
		}

		// show main dialog
		internal struct MainDialogResult {
			internal bool Start;
			internal string RouteFile;
			internal System.Text.Encoding RouteEncoding;
			internal string TrainFolder;
			internal System.Text.Encoding TrainEncoding;
		}
		internal static MainDialogResult ShowMainDialog(MainDialogResult initial) {
			formMain Dialog = new formMain();
			Dialog.Result = initial;
			Dialog.ShowDialog();
			MainDialogResult result = Dialog.Result;
			Dialog.Dispose();
			return result;
		}

		// members
		private MainDialogResult Result;
		private int[] EncodingCodepages = new int[0];
		private Image JoystickImage = null;
		private string[] LanguageFiles = new string[0];
		private string CurrentLanguageCode = "en-US";

		
		
		// ====
		// form
		// ====

		// load
		private void formMain_Load(object sender, EventArgs e) {
			this.MinimumSize = this.Size;
			if (Interface.CurrentOptions.MainMenuWidth == -1 & Interface.CurrentOptions.MainMenuHeight == -1) {
				this.WindowState = FormWindowState.Maximized;
			} else if (Interface.CurrentOptions.MainMenuWidth > 0 & Interface.CurrentOptions.MainMenuHeight > 0) {
				this.Size = new Size(Interface.CurrentOptions.MainMenuWidth, Interface.CurrentOptions.MainMenuHeight);
				this.CenterToScreen();
			}
			#pragma warning disable 0162 // Unreachable code
			if (Program.IsDevelopmentVersion) {
				labelVersion.Text = "v" + Application.ProductVersion + Program.VersionSuffix + " (development)";
				labelVersion.BackColor = Color.Firebrick;
				panelInfo.BackColor = Color.Firebrick;
				linkHomepage.BackColor = Color.Firebrick;
			} else {
				labelVersion.Text = "v" + Application.ProductVersion + Program.VersionSuffix;
			}
			#pragma warning restore 0162 // Unreachable code
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			// form icon
			try {
				string File = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder(), "icon.ico");
				this.Icon = new Icon(File);
			} catch { }
			// use button-style radio buttons on non-Mono
			if (!Program.CurrentlyRunningOnMono) {
				radiobuttonStart.Appearance = Appearance.Button;
				radiobuttonStart.AutoSize = false;
				radiobuttonStart.Size = new Size(buttonClose.Width, buttonClose.Height);
				radiobuttonStart.TextAlign = ContentAlignment.MiddleCenter;
				radiobuttonReview.Appearance = Appearance.Button;
				radiobuttonReview.AutoSize = false;
				radiobuttonReview.Size = new Size(buttonClose.Width, buttonClose.Height);
				radiobuttonReview.TextAlign = ContentAlignment.MiddleCenter;
				radiobuttonControls.Appearance = Appearance.Button;
				radiobuttonControls.AutoSize = false;
				radiobuttonControls.Size = new Size(buttonClose.Width, buttonClose.Height);
				radiobuttonControls.TextAlign = ContentAlignment.MiddleCenter;
				radiobuttonOptions.Appearance = Appearance.Button;
				radiobuttonOptions.AutoSize = false;
				radiobuttonOptions.Size = new Size(buttonClose.Width, buttonClose.Height);
				radiobuttonOptions.TextAlign = ContentAlignment.MiddleCenter;
				radiobuttonGetAddOns.Appearance = Appearance.Button;
				radiobuttonGetAddOns.AutoSize = false;
				radiobuttonGetAddOns.Size = new Size(buttonClose.Width, buttonClose.Height);
				radiobuttonGetAddOns.TextAlign = ContentAlignment.MiddleCenter;
			}
			// options
			Interface.LoadLogs();
			ListLanguages();
			{
				int Tab = 0;
				string[] Args = System.Environment.GetCommandLineArgs();
				for (int i = 1; i < Args.Length; i++) {
					switch (Args[i].ToLowerInvariant()) {
							case "/newgame": Tab = 0; break;
							case "/review": Tab = 1; break;
							case "/controls": Tab = 2; break;
							case "/options": Tab = 3; break;
					}
				}
				switch (Tab) {
						case 1: radiobuttonReview.Checked = true; break;
						case 2: radiobuttonControls.Checked = true; break;
						case 3: radiobuttonOptions.Checked = true; break;
						default: radiobuttonStart.Checked = true; break;
				}
			}
			// icons and images
			string MenuFolder = Program.FileSystem.GetDataFolder("Menu");
			Image ParentIcon = LoadImage(MenuFolder, "icon_parent.png");
			Image FolderIcon = LoadImage(MenuFolder, "icon_folder.png");
			Image RouteIcon = LoadImage(MenuFolder, "icon_route.png");
			Image TrainIcon = LoadImage(MenuFolder, "icon_train.png");
			Image LibraryIcon = LoadImage(MenuFolder, "icon_library.png");
			Image KeyboardIcon = LoadImage(MenuFolder, "icon_keyboard.png");
			Image MouseIcon = LoadImage(MenuFolder, "icon_mouse.png");
			Image JoystickIcon = LoadImage(MenuFolder, "icon_joystick.png");
			Image GamepadIcon = LoadImage(MenuFolder, "icon_gamepad.png");
			JoystickImage = LoadImage(MenuFolder, "joystick.png");
			Image Logo = LoadImage(MenuFolder, "logo.png");
			if (Logo != null) pictureboxLogo.Image = Logo;
			string flagsFolder = Program.FileSystem.GetDataFolder("Flags");
			string[] flags = System.IO.Directory.GetFiles(flagsFolder);
			// route selection
			listviewRouteFiles.SmallImageList = new ImageList();
			listviewRouteFiles.SmallImageList.TransparentColor = Color.White;
			if (ParentIcon != null) listviewRouteFiles.SmallImageList.Images.Add("parent", ParentIcon);
			if (FolderIcon != null) listviewRouteFiles.SmallImageList.Images.Add("folder", FolderIcon);
			if (RouteIcon != null) listviewRouteFiles.SmallImageList.Images.Add("route", RouteIcon);
			treeviewRouteAddOns.ImageList = new ImageList();
			if (FolderIcon != null) treeviewRouteAddOns.ImageList.Images.Add("folder", FolderIcon);
			if (RouteIcon != null) treeviewRouteAddOns.ImageList.Images.Add("route", RouteIcon);
			foreach (string flag in flags) {
				try {
					treeviewRouteAddOns.ImageList.Images.Add(System.IO.Path.GetFileNameWithoutExtension(flag), Image.FromFile(flag));
				} catch { }
			}
			listviewRouteFiles.Columns.Clear();
			listviewRouteFiles.Columns.Add("");
			listviewRouteRecently.Items.Clear();
			listviewRouteRecently.Columns.Add("");
			listviewRouteRecently.SmallImageList = new ImageList();
			listviewRouteRecently.SmallImageList.TransparentColor = Color.White;
			if (RouteIcon != null) listviewRouteRecently.SmallImageList.Images.Add("route", RouteIcon);
			for (int i = 0; i < Interface.CurrentOptions.RecentlyUsedRoutes.Length; i++) {
				ListViewItem Item = listviewRouteRecently.Items.Add(System.IO.Path.GetFileName(Interface.CurrentOptions.RecentlyUsedRoutes[i]));
				Item.ImageKey = "route";
				Item.Tag = Interface.CurrentOptions.RecentlyUsedRoutes[i];
			}
			listviewRouteRecently.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
			// train selection
			listviewTrainFolders.SmallImageList = new ImageList();
			listviewTrainFolders.SmallImageList.TransparentColor = Color.White;
			if (ParentIcon != null) listviewTrainFolders.SmallImageList.Images.Add("parent", ParentIcon);
			if (FolderIcon != null) listviewTrainFolders.SmallImageList.Images.Add("folder", FolderIcon);
			if (TrainIcon != null) listviewTrainFolders.SmallImageList.Images.Add("train", TrainIcon);
			treeviewTrainAddOns.ImageList = new ImageList();
			if (FolderIcon != null) treeviewTrainAddOns.ImageList.Images.Add("folder", FolderIcon);
			if (RouteIcon != null) treeviewTrainAddOns.ImageList.Images.Add("train", TrainIcon);
			foreach (string flag in flags) {
				try {
					treeviewTrainAddOns.ImageList.Images.Add(System.IO.Path.GetFileNameWithoutExtension(flag), Image.FromFile(flag));
				} catch { }
			}
			listviewTrainFolders.Columns.Clear();
			listviewTrainFolders.Columns.Add("");
			listviewTrainRecently.Columns.Clear();
			listviewTrainRecently.Columns.Add("");
			listviewTrainRecently.SmallImageList = new ImageList();
			listviewTrainRecently.SmallImageList.TransparentColor = Color.White;
			if (TrainIcon != null) listviewTrainRecently.SmallImageList.Images.Add("train", TrainIcon);
			for (int i = 0; i < Interface.CurrentOptions.RecentlyUsedTrains.Length; i++) {
				ListViewItem Item = listviewTrainRecently.Items.Add(System.IO.Path.GetFileName(Interface.CurrentOptions.RecentlyUsedTrains[i]));
				Item.ImageKey = "train";
				Item.Tag = Interface.CurrentOptions.RecentlyUsedTrains[i];
			}
			listviewTrainRecently.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
			// text boxes
			if (Interface.CurrentOptions.RouteFolder.Length != 0 && System.IO.Directory.Exists(Interface.CurrentOptions.RouteFolder)) {
				textboxRouteFolder.Text = Interface.CurrentOptions.RouteFolder;
			} else {
				textboxRouteFolder.Text = Program.FileSystem.InitialRouteFolder;
			}
			if (Interface.CurrentOptions.TrainFolder.Length != 0 && System.IO.Directory.Exists(Interface.CurrentOptions.TrainFolder)) {
				textboxTrainFolder.Text = Interface.CurrentOptions.TrainFolder;
			} else {
				textboxTrainFolder.Text = Program.FileSystem.InitialTrainFolder;
			}
			// encodings
			{
				System.Text.EncodingInfo[] Info = System.Text.Encoding.GetEncodings();
				EncodingCodepages = new int[Info.Length + 1];
				string[] EncodingDescriptions = new string[Info.Length + 1];
				EncodingCodepages[0] = System.Text.Encoding.UTF8.CodePage;
				EncodingDescriptions[0] = "(UTF-8)";
				for (int i = 0; i < Info.Length; i++) {
					EncodingCodepages[i + 1] = Info[i].CodePage;
					try { // MoMA says that DisplayName is flagged with [MonoTodo]
						EncodingDescriptions[i + 1] = Info[i].DisplayName + " - " + Info[i].CodePage.ToString(Culture);
					} catch {
						EncodingDescriptions[i + 1] = Info[i].Name;
					}
				}
				Array.Sort<string, int>(EncodingDescriptions, EncodingCodepages, 1, Info.Length);
				comboboxRouteEncoding.Items.Clear();
				comboboxTrainEncoding.Items.Clear();
				for (int i = 0; i < Info.Length + 1; i++) {
					comboboxRouteEncoding.Items.Add(EncodingDescriptions[i]);
					comboboxTrainEncoding.Items.Add(EncodingDescriptions[i]);
				}
			}
			// modes
			comboboxMode.Items.Clear();
			comboboxMode.Items.AddRange(new string[] { "", "", "" });
			comboboxMode.SelectedIndex = Interface.CurrentOptions.GameMode == Interface.GameMode.Arcade ? 0 : Interface.CurrentOptions.GameMode == Interface.GameMode.Expert ? 2 : 1;
			// review last game
			{
				if (Game.LogRouteName.Length == 0 | Game.LogTrainName.Length == 0) {
					radiobuttonReview.Enabled = false;
				} else {
					double ratio = Game.CurrentScore.Maximum == 0 ? 0.0 : (double)Game.CurrentScore.Value / (double)Game.CurrentScore.Maximum;
					if (ratio < 0.0) ratio = 0.0;
					if (ratio > 1.0) ratio = 1.0;
					int index = (int)Math.Floor(ratio * (double)Interface.RatingsCount);
					if (index >= Interface.RatingsCount) index = Interface.RatingsCount - 1;
					labelReviewRouteValue.Text = Game.LogRouteName;
					labelReviewTrainValue.Text = Game.LogTrainName;
					labelReviewDateValue.Text = Game.LogDateTime.ToString("yyyy-MM-dd", Culture);
					labelReviewTimeValue.Text = Game.LogDateTime.ToString("HH:mm:ss", Culture);
					switch (Interface.CurrentOptions.GameMode) {
							case Interface.GameMode.Arcade: labelRatingModeValue.Text = Interface.GetInterfaceString("mode_arcade"); break;
							case Interface.GameMode.Normal: labelRatingModeValue.Text = Interface.GetInterfaceString("mode_normal"); break;
							case Interface.GameMode.Expert: labelRatingModeValue.Text = Interface.GetInterfaceString("mode_expert"); break;
							default: labelRatingModeValue.Text = Interface.GetInterfaceString("mode_unkown"); break;
					}
					if (Game.CurrentScore.Maximum == 0) {
						labelRatingColor.BackColor = Color.Gray;
						labelRatingDescription.Text = Interface.GetInterfaceString("rating_unkown");
					} else {
						Color[] Colors = new Color[] { Color.PaleVioletRed, Color.IndianRed, Color.Peru, Color.Goldenrod, Color.DarkKhaki, Color.YellowGreen, Color.MediumSeaGreen, Color.MediumAquamarine, Color.SkyBlue, Color.CornflowerBlue };
						if (index >= 0 & index < Colors.Length) {
							labelRatingColor.BackColor = Colors[index];
						} else {
							labelRatingColor.BackColor = Color.Gray;
						}
						labelRatingDescription.Text = Interface.GetInterfaceString("rating_" + index.ToString(Culture));
					}
					labelRatingAchievedValue.Text = Game.CurrentScore.Value.ToString(Culture);
					labelRatingMaximumValue.Text = Game.CurrentScore.Maximum.ToString(Culture);
					labelRatingRatioValue.Text = (100.0 * ratio).ToString("0.00", Culture) + "%";
				}
			}
			comboboxBlackBoxFormat.Items.Clear();
			comboboxBlackBoxFormat.Items.AddRange(new string[] { "", "" });
			comboboxBlackBoxFormat.SelectedIndex = 1;
			if (Game.BlackBoxEntryCount == 0) {
				labelBlackBox.Enabled = false;
				labelBlackBoxFormat.Enabled = false;
				comboboxBlackBoxFormat.Enabled = false;
				buttonBlackBoxExport.Enabled = false;
			}
			// controls
			listviewControls.SmallImageList = new ImageList();
			listviewControls.SmallImageList.TransparentColor = Color.White;
			if (KeyboardIcon != null) listviewControls.SmallImageList.Images.Add("keyboard", KeyboardIcon);
			if (MouseIcon != null) listviewControls.SmallImageList.Images.Add("mouse", MouseIcon);
			if (JoystickIcon != null) listviewControls.SmallImageList.Images.Add("joystick", JoystickIcon);
			if (GamepadIcon != null) listviewControls.SmallImageList.Images.Add("gamepad", GamepadIcon);
			// options
			if (Interface.CurrentOptions.FullscreenMode) {
				radiobuttonFullscreen.Checked = true;
			} else {
				radiobuttonWindow.Checked = true;
			}
			comboboxVSync.Items.Clear();
			comboboxVSync.Items.Add("");
			comboboxVSync.Items.Add("");
			comboboxVSync.SelectedIndex = Interface.CurrentOptions.VerticalSynchronization ? 1 : 0;
			updownWindowWidth.Value = (decimal)Interface.CurrentOptions.WindowWidth;
			updownWindowHeight.Value = (decimal)Interface.CurrentOptions.WindowHeight;
			updownFullscreenWidth.Value = (decimal)Interface.CurrentOptions.FullscreenWidth;
			updownFullscreenHeight.Value = (decimal)Interface.CurrentOptions.FullscreenHeight;
			comboboxFullscreenBits.Items.Clear();
			comboboxFullscreenBits.Items.Add("16");
			comboboxFullscreenBits.Items.Add("32");
			comboboxFullscreenBits.SelectedIndex = Interface.CurrentOptions.FullscreenBits == 16 ? 0 : 1;
			comboboxInterpolation.Items.Clear();
			comboboxInterpolation.Items.AddRange(new string[] { "", "", "", "", "", "" });
			if ((int)Interface.CurrentOptions.Interpolation >= 0 & (int)Interface.CurrentOptions.Interpolation < comboboxInterpolation.Items.Count) {
				comboboxInterpolation.SelectedIndex = (int)Interface.CurrentOptions.Interpolation;
			} else {
				comboboxInterpolation.SelectedIndex = 3;
			}
			if (Interface.CurrentOptions.AnisotropicFilteringMaximum <= 0) {
				labelAnisotropic.Enabled = false;
				updownAnisotropic.Enabled = false;
				updownAnisotropic.Minimum = (decimal)0;
				updownAnisotropic.Maximum = (decimal)0;
			} else {
				updownAnisotropic.Minimum = (decimal)1;
				updownAnisotropic.Maximum = (decimal)Interface.CurrentOptions.AnisotropicFilteringMaximum;
				if ((decimal)Interface.CurrentOptions.AnisotropicFilteringLevel >= updownAnisotropic.Minimum & (decimal)Interface.CurrentOptions.AnisotropicFilteringLevel <= updownAnisotropic.Maximum) {
					updownAnisotropic.Value = (decimal)Interface.CurrentOptions.AnisotropicFilteringLevel;
				} else {
					updownAnisotropic.Value = updownAnisotropic.Minimum;
				}
			}
			updownAntiAliasing.Value = (decimal)Interface.CurrentOptions.AntiAliasingLevel;
			updownDistance.Value = (decimal)Interface.CurrentOptions.ViewingDistance;
			comboboxMotionBlur.Items.Clear();
			comboboxMotionBlur.Items.AddRange(new string[] { "", "", "", "" });
			comboboxMotionBlur.SelectedIndex = (int)Interface.CurrentOptions.MotionBlur;
			trackbarTransparency.Value = (int)Interface.CurrentOptions.TransparencyMode;
			checkboxToppling.Checked = Interface.CurrentOptions.Toppling;
			checkboxCollisions.Checked = Interface.CurrentOptions.Collisions;
			checkboxDerailments.Checked = Interface.CurrentOptions.Derailments;
			checkboxBlackBox.Checked = Interface.CurrentOptions.BlackBox;
			checkboxJoysticksUsed.Checked = Interface.CurrentOptions.UseJoysticks;
			{
				double a = (double)(trackbarJoystickAxisThreshold.Maximum - trackbarJoystickAxisThreshold.Minimum) * Interface.CurrentOptions.JoystickAxisThreshold + (double)trackbarJoystickAxisThreshold.Minimum;
				int b = (int)Math.Round(a);
				if (b < trackbarJoystickAxisThreshold.Minimum) b = trackbarJoystickAxisThreshold.Minimum;
				if (b > trackbarJoystickAxisThreshold.Maximum) b = trackbarJoystickAxisThreshold.Maximum;
				trackbarJoystickAxisThreshold.Value = b;
			}
			updownSoundNumber.Value = (decimal)Interface.CurrentOptions.SoundNumber;
			checkboxWarningMessages.Checked = Interface.CurrentOptions.ShowWarningMessages;
			checkboxErrorMessages.Checked = Interface.CurrentOptions.ShowErrorMessages;
			// language
			{
				string Folder = Program.FileSystem.GetDataFolder("Languages");
				int j;
				for (j = 0; j < LanguageFiles.Length; j++) {
					string File = OpenBveApi.Path.CombineFile(Folder, Interface.CurrentOptions.LanguageCode + ".cfg");
					if (string.Compare(File, LanguageFiles[j], StringComparison.OrdinalIgnoreCase) == 0) {
						comboboxLanguages.SelectedIndex = j;
						break;
					}
				}
				if (j == LanguageFiles.Length) {
					#if !DEBUG
					try {
						#endif
						string File = OpenBveApi.Path.CombineFile(Folder, "en-US.cfg");
						Interface.LoadLanguage(File);
						ApplyLanguage();
						#if !DEBUG
					} catch (Exception ex) {
						MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
					}
					#endif
				}
			}
			// lists
			ShowScoreLog(checkboxScorePenalties.Checked);
			// get add-ons
			checkboxFilterRoutes.Image = RouteIcon;
			checkboxFilterTrains.Image = TrainIcon;
			checkboxFilterLibraries.Image = LibraryIcon;
			checkboxFilterSharedLibraries.Image = LibraryIcon;
			treeviewPackages.ImageList = new ImageList();
			treeviewPackages.ImageList.Images.Add("route_notinstalled", LoadImage(MenuFolder, "icon_route_notinstalled.png"));
			treeviewPackages.ImageList.Images.Add("route_outdatedversion", LoadImage(MenuFolder, "icon_route_outdatedversion.png"));
			treeviewPackages.ImageList.Images.Add("route_latestversion", LoadImage(MenuFolder, "icon_route_latestversion.png"));
			treeviewPackages.ImageList.Images.Add("route_protected", LoadImage(MenuFolder, "icon_route_protected.png"));
			treeviewPackages.ImageList.Images.Add("train_notinstalled", LoadImage(MenuFolder, "icon_train_notinstalled.png"));
			treeviewPackages.ImageList.Images.Add("train_outdatedversion", LoadImage(MenuFolder, "icon_train_outdatedversion.png"));
			treeviewPackages.ImageList.Images.Add("train_latestversion", LoadImage(MenuFolder, "icon_train_latestversion.png"));
			treeviewPackages.ImageList.Images.Add("train_protected", LoadImage(MenuFolder, "icon_train_protected.png"));
			treeviewPackages.ImageList.Images.Add("library_notinstalled", LoadImage(MenuFolder, "icon_library_notinstalled.png"));
			treeviewPackages.ImageList.Images.Add("library_outdatedversion", LoadImage(MenuFolder, "icon_library_outdatedversion.png"));
			treeviewPackages.ImageList.Images.Add("library_latestversion", LoadImage(MenuFolder, "icon_library_latestversion.png"));
			treeviewPackages.ImageList.Images.Add("library_protected", LoadImage(MenuFolder, "icon_library_protected.png"));
			treeviewPackages.ImageList.Images.Add("folder", LoadImage(MenuFolder, "icon_folder.png"));
			foreach (string flag in flags) {
				try {
					treeviewPackages.ImageList.Images.Add(System.IO.Path.GetFileNameWithoutExtension(flag), Image.FromFile(flag));
				} catch { }
			}
			// result
			Result.Start = false;
//			Result.RouteFile = null;
//			Result.RouteEncoding = System.Text.Encoding.UTF8;
//			Result.TrainFolder = null;
//			Result.TrainEncoding = System.Text.Encoding.UTF8;
		}

		// apply language
		private void ApplyLanguage() {
			// panel
			radiobuttonStart.Text = Interface.GetInterfaceString("panel_start");
			radiobuttonReview.Text = Interface.GetInterfaceString("panel_review");
			radiobuttonGetAddOns.Text = Interface.GetInterfaceString("panel_getaddons");
			radiobuttonControls.Text = Interface.GetInterfaceString("panel_controls");
			radiobuttonOptions.Text = Interface.GetInterfaceString("panel_options");
			linkHomepage.Text = Interface.GetInterfaceString("panel_homepage");
			linkUpdates.Text = Interface.GetInterfaceString("panel_updates");
			buttonClose.Text = Interface.GetInterfaceString("panel_close");
			// options
			labelOptionsTitle.Text = Interface.GetInterfaceString("options_title");
			groupboxDisplayMode.Text = Interface.GetInterfaceString("options_display_mode");
			radiobuttonWindow.Text = Interface.GetInterfaceString("options_display_mode_window");
			radiobuttonFullscreen.Text = Interface.GetInterfaceString("options_display_mode_fullscreen");
			labelVSync.Text = Interface.GetInterfaceString("options_display_vsync");
			comboboxVSync.Items[0] = Interface.GetInterfaceString("options_display_vsync_off");
			comboboxVSync.Items[1] = Interface.GetInterfaceString("options_display_vsync_on");
			groupboxWindow.Text = Interface.GetInterfaceString("options_display_window");
			labelWindowWidth.Text = Interface.GetInterfaceString("options_display_window_width");
			labelWindowHeight.Text = Interface.GetInterfaceString("options_display_window_height");
			groupboxFullscreen.Text = Interface.GetInterfaceString("options_display_fullscreen");
			labelFullscreenWidth.Text = Interface.GetInterfaceString("options_display_fullscreen_width");
			labelFullscreenHeight.Text = Interface.GetInterfaceString("options_display_fullscreen_height");
			labelFullscreenBits.Text = Interface.GetInterfaceString("options_display_fullscreen_bits");
			groupboxInterpolation.Text = Interface.GetInterfaceString("options_quality_interpolation");
			labelInterpolation.Text = Interface.GetInterfaceString("options_quality_interpolation_mode");
			comboboxInterpolation.Items[0] = Interface.GetInterfaceString("options_quality_interpolation_mode_nearest");
			comboboxInterpolation.Items[1] = Interface.GetInterfaceString("options_quality_interpolation_mode_bilinear");
			comboboxInterpolation.Items[2] = Interface.GetInterfaceString("options_quality_interpolation_mode_nearestmipmap");
			comboboxInterpolation.Items[3] = Interface.GetInterfaceString("options_quality_interpolation_mode_bilinearmipmap");
			comboboxInterpolation.Items[4] = Interface.GetInterfaceString("options_quality_interpolation_mode_trilinearmipmap");
			comboboxInterpolation.Items[5] = Interface.GetInterfaceString("options_quality_interpolation_mode_anisotropic");
			labelAnisotropic.Text = Interface.GetInterfaceString("options_quality_interpolation_anisotropic_level");
			labelAntiAliasing.Text = Interface.GetInterfaceString("options_quality_interpolation_antialiasing_level");
			labelTransparency.Text = Interface.GetInterfaceString("options_quality_interpolation_transparency");
			labelTransparencyPerformance.Text = Interface.GetInterfaceString("options_quality_interpolation_transparency_sharp");
			labelTransparencyQuality.Text = Interface.GetInterfaceString("options_quality_interpolation_transparency_smooth");
			groupboxDistance.Text = Interface.GetInterfaceString("options_quality_distance");
			labelDistance.Text = Interface.GetInterfaceString("options_quality_distance_viewingdistance");
			labelDistanceUnit.Text = Interface.GetInterfaceString("options_quality_distance_viewingdistance_meters");
			labelMotionBlur.Text = "options_quality_distance_motionblur";
			comboboxMotionBlur.Items[0] = Interface.GetInterfaceString("options_quality_distance_motionblur_none");
			comboboxMotionBlur.Items[1] = Interface.GetInterfaceString("options_quality_distance_motionblur_low");
			comboboxMotionBlur.Items[2] = Interface.GetInterfaceString("options_quality_distance_motionblur_medium");
			comboboxMotionBlur.Items[3] = Interface.GetInterfaceString("options_quality_distance_motionblur_high");
			labelMotionBlur.Text = Interface.GetInterfaceString("options_quality_distance_motionblur");
			groupboxSimulation.Text = Interface.GetInterfaceString("options_misc_simulation");
			checkboxToppling.Text = Interface.GetInterfaceString("options_misc_simulation_toppling");
			checkboxCollisions.Text = Interface.GetInterfaceString("options_misc_simulation_collisions");
			checkboxDerailments.Text = Interface.GetInterfaceString("options_misc_simulation_derailments");
			checkboxBlackBox.Text = Interface.GetInterfaceString("options_misc_simulation_blackbox");
			groupboxControls.Text = Interface.GetInterfaceString("options_misc_controls");
			checkboxJoysticksUsed.Text = Interface.GetInterfaceString("options_misc_controls_joysticks");
			labelJoystickAxisThreshold.Text = Interface.GetInterfaceString("options_misc_controls_threshold");
			groupboxSound.Text = Interface.GetInterfaceString("options_misc_sound");
			labelSoundNumber.Text = Interface.GetInterfaceString("options_misc_sound_number");
			groupboxVerbosity.Text = Interface.GetInterfaceString("options_verbosity");
			checkboxWarningMessages.Text = Interface.GetInterfaceString("options_verbosity_warningmessages");
			checkboxErrorMessages.Text = Interface.GetInterfaceString("options_verbosity_errormessages");
			// start
			labelStartTitle.Text = Interface.GetInterfaceString("start_title");
			labelRoute.Text = " " + Interface.GetInterfaceString("start_route");
			groupboxRouteSelection.Text = Interface.GetInterfaceString("start_route_selection");
			tabpageRouteManaged.Text = Interface.GetInterfaceString("start_route_addons");
			tabpageRouteBrowse.Text = Interface.GetInterfaceString("start_route_browse");
			tabpageRouteRecently.Text = Interface.GetInterfaceString("start_route_recently");
			groupboxRouteDetails.Text = Interface.GetInterfaceString("start_route_details");
			tabpageRouteDescription.Text = Interface.GetInterfaceString("start_route_description");
			tabpageRouteMap.Text = Interface.GetInterfaceString("start_route_map");
			tabpageRouteGradient.Text = Interface.GetInterfaceString("start_route_gradient");
			tabpageRouteSettings.Text = Interface.GetInterfaceString("start_route_settings");
			labelRouteEncoding.Text = Interface.GetInterfaceString("start_route_settings_encoding");
			comboboxRouteEncoding.Items[0] = Interface.GetInterfaceString("(UTF-8)");
			labelRouteEncodingPreview.Text = Interface.GetInterfaceString("start_route_settings_encoding_preview");
			labelTrain.Text = " " + Interface.GetInterfaceString("start_train");
			groupboxTrainSelection.Text = Interface.GetInterfaceString("start_train_selection");
			tabpageTrainManaged.Text = Interface.GetInterfaceString("start_train_addons");
			tabpageTrainBrowse.Text = Interface.GetInterfaceString("start_train_browse");
			tabpageTrainRecently.Text = Interface.GetInterfaceString("start_train_recently");
			tabpageTrainDefault.Text = Interface.GetInterfaceString("start_train_default");
			checkboxTrainDefault.Text = Interface.GetInterfaceString("start_train_usedefault");
			groupboxTrainDetails.Text = Interface.GetInterfaceString("start_train_details");
			tabpageTrainDescription.Text = Interface.GetInterfaceString("start_train_description");
			tabpageTrainSettings.Text = Interface.GetInterfaceString("start_train_settings");
			labelTrainEncoding.Text = Interface.GetInterfaceString("start_train_settings_encoding");
			comboboxTrainEncoding.Items[0] = Interface.GetInterfaceString("(UTF-8)");
			labelTrainEncodingPreview.Text = Interface.GetInterfaceString("start_train_settings_encoding_preview");
			labelStart.Text = " " + Interface.GetInterfaceString("start_start");
			labelMode.Text = Interface.GetInterfaceString("start_start_mode");
			buttonStart.Text = Interface.GetInterfaceString("start_start_start");
			comboboxMode.Items[0] = Interface.GetInterfaceString("mode_arcade");
			comboboxMode.Items[1] = Interface.GetInterfaceString("mode_normal");
			comboboxMode.Items[2] = Interface.GetInterfaceString("mode_expert");
			// getaddons
			labelGetAddOnsTitle.Text = Interface.GetInterfaceString("getaddons_title");
			labelFilter.Text = Interface.GetInterfaceString("getaddons_filter");
			checkboxFilterRoutes.Text = Interface.GetInterfaceString("getaddons_filter_routes");
			checkboxFilterTrains.Text = Interface.GetInterfaceString("getaddons_filter_trains");
			checkboxFilterLibraries.Text = Interface.GetInterfaceString("getaddons_filter_libraries");
			checkboxFilterSharedLibraries.Text = Interface.GetInterfaceString("getaddons_filter_sharedlibraries");
			checkboxFilterNoWIPs.Text = Interface.GetInterfaceString("getaddons_filter_nowips");
			checkboxFilterUpdates.Text = Interface.GetInterfaceString("getaddons_filter_onlyupdates");
			groupboxPackage.Text = Interface.GetInterfaceString("getaddons_package");
			buttonPackageInstall.Text = Interface.GetInterfaceString("getaddons_package_install");
			buttonPackageRemove.Text = Interface.GetInterfaceString("getaddons_package_remove");
			buttonScreenshotPrevious.Text = Interface.GetInterfaceString("getaddons_screenshot_previous");
			buttonScreenshotNext.Text = Interface.GetInterfaceString("getaddons_screenshot_next");
			// review
			labelReviewTitle.Text = Interface.GetInterfaceString("review_title");
			labelConditions.Text = " " + Interface.GetInterfaceString("review_conditions");
			groupboxReviewRoute.Text = Interface.GetInterfaceString("review_conditions_route");
			labelReviewRouteCaption.Text = Interface.GetInterfaceString("review_conditions_route_file");
			groupboxReviewTrain.Text = Interface.GetInterfaceString("review_conditions_train");
			labelReviewTrainCaption.Text = Interface.GetInterfaceString("review_conditions_train_folder");
			groupboxReviewDateTime.Text = Interface.GetInterfaceString("review_conditions_datetime");
			labelReviewDateCaption.Text = Interface.GetInterfaceString("review_conditions_datetime_date");
			labelReviewTimeCaption.Text = Interface.GetInterfaceString("review_conditions_datetime_time");
			labelScore.Text = " " + Interface.GetInterfaceString("review_score");
			groupboxRating.Text = Interface.GetInterfaceString("review_score_rating");
			labelRatingModeCaption.Text = Interface.GetInterfaceString("review_score_rating_mode");
			switch (Interface.CurrentOptions.GameMode) {
					case Interface.GameMode.Arcade: labelRatingModeValue.Text = Interface.GetInterfaceString("mode_arcade"); break;
					case Interface.GameMode.Normal: labelRatingModeValue.Text = Interface.GetInterfaceString("mode_normal"); break;
					case Interface.GameMode.Expert: labelRatingModeValue.Text = Interface.GetInterfaceString("mode_expert"); break;
					default: labelRatingModeValue.Text = Interface.GetInterfaceString("mode_unkown"); break;
			}
			{
					double ratio = Game.CurrentScore.Maximum == 0 ? 0.0 : (double)Game.CurrentScore.Value / (double)Game.CurrentScore.Maximum;
					if (ratio < 0.0) ratio = 0.0;
					if (ratio > 1.0) ratio = 1.0;
					int index = (int)Math.Floor(ratio * (double)Interface.RatingsCount);
					if (index >= Interface.RatingsCount) index = Interface.RatingsCount - 1;
					if (Game.CurrentScore.Maximum == 0) {
						labelRatingDescription.Text = Interface.GetInterfaceString("rating_unkown");
					} else {
						labelRatingDescription.Text = Interface.GetInterfaceString("rating_" + index.ToString(System.Globalization.CultureInfo.InvariantCulture));
					}
			}
			labelRatingAchievedCaption.Text = Interface.GetInterfaceString("review_score_rating_achieved");
			labelRatingMaximumCaption.Text = Interface.GetInterfaceString("review_score_rating_maximum");
			labelRatingRatioCaption.Text = Interface.GetInterfaceString("review_score_rating_ratio");
			groupboxScore.Text = Interface.GetInterfaceString("review_score_log");
			listviewScore.Columns[0].Text = Interface.GetInterfaceString("review_score_log_list_time");
			listviewScore.Columns[1].Text = Interface.GetInterfaceString("review_score_log_list_position");
			listviewScore.Columns[2].Text = Interface.GetInterfaceString("review_score_log_list_value");
			listviewScore.Columns[3].Text = Interface.GetInterfaceString("review_score_log_list_cumulative");
			listviewScore.Columns[4].Text = Interface.GetInterfaceString("review_score_log_list_reason");
			ShowScoreLog(checkboxScorePenalties.Checked);
			checkboxScorePenalties.Text = Interface.GetInterfaceString("review_score_log_penalties");
			buttonScoreExport.Text = Interface.GetInterfaceString("review_score_log_export");
			labelBlackBox.Text = " " + Interface.GetInterfaceString("review_blackbox");
			labelBlackBoxFormat.Text = Interface.GetInterfaceString("review_blackbox_format");
			comboboxBlackBoxFormat.Items[0] = Interface.GetInterfaceString("review_blackbox_format_csv");
			comboboxBlackBoxFormat.Items[1] = Interface.GetInterfaceString("review_blackbox_format_text");
			buttonBlackBoxExport.Text = Interface.GetInterfaceString("review_blackbox_export");
			// controls
			for (int i = 0; i < listviewControls.SelectedItems.Count; i++) {
				listviewControls.SelectedItems[i].Selected = false;
			}
			labelControlsTitle.Text = Interface.GetInterfaceString("controls_title");
			listviewControls.Columns[0].Text = Interface.GetInterfaceString("controls_list_command");
			listviewControls.Columns[1].Text = Interface.GetInterfaceString("controls_list_type");
			listviewControls.Columns[2].Text = Interface.GetInterfaceString("controls_list_description");
			listviewControls.Columns[3].Text = Interface.GetInterfaceString("controls_list_assignment");
			buttonControlAdd.Text = Interface.GetInterfaceString("controls_add");
			buttonControlRemove.Text = Interface.GetInterfaceString("controls_remove");
			buttonControlsImport.Text = Interface.GetInterfaceString("controls_import");
			buttonControlsExport.Text = Interface.GetInterfaceString("controls_export");
			buttonControlUp.Text = Interface.GetInterfaceString("controls_up");
			buttonControlDown.Text = Interface.GetInterfaceString("controls_down");
			groupboxControl.Text = Interface.GetInterfaceString("controls_selection");
			labelCommand.Text = Interface.GetInterfaceString("controls_selection_command");
			radiobuttonKeyboard.Text = Interface.GetInterfaceString("controls_selection_keyboard");
			labelKeyboardKey.Text = Interface.GetInterfaceString("controls_selection_keyboard_key");
			labelKeyboardModifier.Text = Interface.GetInterfaceString("controls_selection_keyboard_modifiers");
			checkboxKeyboardShift.Text = Interface.GetInterfaceString("controls_selection_keyboard_modifiers_shift");
			checkboxKeyboardCtrl.Text = Interface.GetInterfaceString("controls_selection_keyboard_modifiers_ctrl");
			checkboxKeyboardAlt.Text = Interface.GetInterfaceString("controls_selection_keyboard_modifiers_alt");
			radiobuttonJoystick.Text = Interface.GetInterfaceString("controls_selection_joystick");
			labelJoystickAssignmentCaption.Text = Interface.GetInterfaceString("controls_selection_joystick_assignment");
			textboxJoystickGrab.Text = Interface.GetInterfaceString("controls_selection_joystick_assignment_grab");
			groupboxJoysticks.Text = Interface.GetInterfaceString("controls_attached");
			{
				listviewControls.Items.Clear();
				comboboxCommand.Items.Clear();
				for (int i = 0; i < Interface.CommandInfos.Length; i++) {
					comboboxCommand.Items.Add(Interface.CommandInfos[i].Name + " - " + Interface.CommandInfos[i].Description);
				}
				comboboxKeyboardKey.Items.Clear();
				for (int i = 0; i < Interface.Keys.Length; i++) {
					comboboxKeyboardKey.Items.Add(Interface.Keys[i].Description);
				}
				ListViewItem[] Items = new ListViewItem[Interface.CurrentControls.Length];
				for (int i = 0; i < Interface.CurrentControls.Length; i++) {
					Items[i] = new ListViewItem(new string[] { "", "", "", "" });
					UpdateControlListElement(Items[i], i, false);
				}
				listviewControls.Items.AddRange(Items);
				listviewControls.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
			}
		}

		// form closing
		private void formMain_FormClosing(object sender, FormClosingEventArgs e) {
			if (IsBusy()) {
				MessageBox.Show("The form cannot be closed because add-ons are currently being maintained.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				e.Cancel = true;
				return;
			}
			Interface.CurrentOptions.LanguageCode = CurrentLanguageCode;
			Interface.CurrentOptions.FullscreenMode = radiobuttonFullscreen.Checked;
			Interface.CurrentOptions.VerticalSynchronization = comboboxVSync.SelectedIndex == 1;
			Interface.CurrentOptions.WindowWidth = (int)Math.Round(updownWindowWidth.Value);
			Interface.CurrentOptions.WindowHeight = (int)Math.Round(updownWindowHeight.Value);
			Interface.CurrentOptions.FullscreenWidth = (int)Math.Round(updownFullscreenWidth.Value);
			Interface.CurrentOptions.FullscreenHeight = (int)Math.Round(updownFullscreenHeight.Value);
			Interface.CurrentOptions.FullscreenBits = comboboxFullscreenBits.SelectedIndex == 0 ? 16 : 32;
			Interface.CurrentOptions.Interpolation = (Interface.InterpolationMode)comboboxInterpolation.SelectedIndex;
			Interface.CurrentOptions.AnisotropicFilteringLevel = (int)Math.Round(updownAnisotropic.Value);
			Interface.CurrentOptions.AntiAliasingLevel = (int)Math.Round(updownAntiAliasing.Value);
			Interface.CurrentOptions.TransparencyMode = (Renderer.TransparencyMode)trackbarTransparency.Value;
			Interface.CurrentOptions.ViewingDistance = (int)Math.Round(updownDistance.Value);
			Interface.CurrentOptions.MotionBlur = (Interface.MotionBlurMode)comboboxMotionBlur.SelectedIndex;
			Interface.CurrentOptions.Toppling = checkboxToppling.Checked;
			Interface.CurrentOptions.Collisions = checkboxCollisions.Checked;
			Interface.CurrentOptions.Derailments = checkboxDerailments.Checked;
			Interface.CurrentOptions.GameMode = (Interface.GameMode)comboboxMode.SelectedIndex;
			Interface.CurrentOptions.BlackBox = checkboxBlackBox.Checked;
			Interface.CurrentOptions.UseJoysticks = checkboxJoysticksUsed.Checked;
			Interface.CurrentOptions.JoystickAxisThreshold = ((double)trackbarJoystickAxisThreshold.Value - (double)trackbarJoystickAxisThreshold.Minimum) / (double)(trackbarJoystickAxisThreshold.Maximum - trackbarJoystickAxisThreshold.Minimum);
			Interface.CurrentOptions.SoundNumber = (int)Math.Round(updownSoundNumber.Value);
			Interface.CurrentOptions.ShowWarningMessages = checkboxWarningMessages.Checked;
			Interface.CurrentOptions.ShowErrorMessages = checkboxErrorMessages.Checked;
			Interface.CurrentOptions.RouteFolder = textboxRouteFolder.Text;
			Interface.CurrentOptions.TrainFolder = textboxTrainFolder.Text;
			Interface.CurrentOptions.MainMenuWidth = this.WindowState == FormWindowState.Maximized ? -1 : this.Size.Width;
			Interface.CurrentOptions.MainMenuHeight = this.WindowState == FormWindowState.Maximized ? -1 : this.Size.Height;
			if (Result.Start) {
				// recently used routes
				if (Interface.CurrentOptions.RecentlyUsedLimit > 0) {
					int i; for (i = 0; i < Interface.CurrentOptions.RecentlyUsedRoutes.Length; i++) {
						if (string.Compare(Result.RouteFile, Interface.CurrentOptions.RecentlyUsedRoutes[i], StringComparison.OrdinalIgnoreCase) == 0) {
							break;
						}
					} if (i == Interface.CurrentOptions.RecentlyUsedRoutes.Length) {
						if (Interface.CurrentOptions.RecentlyUsedRoutes.Length < Interface.CurrentOptions.RecentlyUsedLimit) {
							Array.Resize<string>(ref Interface.CurrentOptions.RecentlyUsedRoutes, i + 1);
						} else {
							i--;
						}
					}
					for (int j = i; j > 0; j--) {
						Interface.CurrentOptions.RecentlyUsedRoutes[j] = Interface.CurrentOptions.RecentlyUsedRoutes[j - 1];
					}
					Interface.CurrentOptions.RecentlyUsedRoutes[0] = Result.RouteFile;
				}
				// recently used trains
				if (Interface.CurrentOptions.RecentlyUsedLimit > 0) {
					int i; for (i = 0; i < Interface.CurrentOptions.RecentlyUsedTrains.Length; i++) {
						if (string.Compare(Result.TrainFolder, Interface.CurrentOptions.RecentlyUsedTrains[i], StringComparison.OrdinalIgnoreCase) == 0) {
							break;
						}
					} if (i == Interface.CurrentOptions.RecentlyUsedTrains.Length) {
						if (Interface.CurrentOptions.RecentlyUsedTrains.Length < Interface.CurrentOptions.RecentlyUsedLimit) {
							Array.Resize<string>(ref Interface.CurrentOptions.RecentlyUsedTrains, i + 1);
						} else {
							i--;
						}
					}
					for (int j = i; j > 0; j--) {
						Interface.CurrentOptions.RecentlyUsedTrains[j] = Interface.CurrentOptions.RecentlyUsedTrains[j - 1];
					}
					Interface.CurrentOptions.RecentlyUsedTrains[0] = Result.TrainFolder;
				}
			}
			// remove non-existing recently used routes
			{
				int n = 0;
				string[] a = new string[Interface.CurrentOptions.RecentlyUsedRoutes.Length];
				for (int i = 0; i < Interface.CurrentOptions.RecentlyUsedRoutes.Length; i++) {
					if (System.IO.File.Exists(Interface.CurrentOptions.RecentlyUsedRoutes[i])) {
						a[n] = Interface.CurrentOptions.RecentlyUsedRoutes[i];
						n++;
					}
				}
				Array.Resize<string>(ref a, n);
				Interface.CurrentOptions.RecentlyUsedRoutes = a;
			}
			// remove non-existing recently used trains
			{
				int n = 0;
				string[] a = new string[Interface.CurrentOptions.RecentlyUsedTrains.Length];
				for (int i = 0; i < Interface.CurrentOptions.RecentlyUsedTrains.Length; i++) {
					if (System.IO.Directory.Exists(Interface.CurrentOptions.RecentlyUsedTrains[i])) {
						a[n] = Interface.CurrentOptions.RecentlyUsedTrains[i];
						n++;
					}
				}
				Array.Resize<string>(ref a, n);
				Interface.CurrentOptions.RecentlyUsedTrains = a;
			}
			// remove non-existing route encoding mappings
			{
				int n = 0;
				Interface.EncodingValue[] a = new Interface.EncodingValue[Interface.CurrentOptions.RouteEncodings.Length];
				for (int i = 0; i < Interface.CurrentOptions.RouteEncodings.Length; i++) {
					if (System.IO.File.Exists(Interface.CurrentOptions.RouteEncodings[i].Value)) {
						a[n] = Interface.CurrentOptions.RouteEncodings[i];
						n++;
					}
				}
				Array.Resize<Interface.EncodingValue>(ref a, n);
				Interface.CurrentOptions.RouteEncodings = a;
			}
			// remove non-existing train encoding mappings
			{
				int n = 0;
				Interface.EncodingValue[] a = new Interface.EncodingValue[Interface.CurrentOptions.TrainEncodings.Length];
				for (int i = 0; i < Interface.CurrentOptions.TrainEncodings.Length; i++) {
					if (System.IO.Directory.Exists(Interface.CurrentOptions.TrainEncodings[i].Value)) {
						a[n] = Interface.CurrentOptions.TrainEncodings[i];
						n++;
					}
				}
				Array.Resize<Interface.EncodingValue>(ref a, n);
				Interface.CurrentOptions.TrainEncodings = a;
			}
			// clear cache
			string directory = System.IO.Path.Combine(Program.FileSystem.SettingsFolder, "Cache");
			ClearCache(directory, NumberOfDaysScreenshotsAreCached);
			// finish
			#if !DEBUG
			try {
				#endif
				Interface.SaveOptions();
				#if !DEBUG
			} catch (Exception ex) {
				MessageBox.Show(ex.Message, "Save options", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
			#endif
			#if !DEBUG
			try {
				#endif
				Interface.SaveControls(null);
				#if !DEBUG
			} catch (Exception ex) {
				MessageBox.Show(ex.Message, "Save controls", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
			#endif
		}

		// resize
		private void formMain_Resize(object sender, EventArgs e) {
			try {
				int wt = panelStart.Width;
				int ox = labelStart.Left;
				int wa = (wt - 3 * ox) / 2;
				int wb = (wt - 3 * ox) / 2;
				groupboxRouteSelection.Width = wa;
				groupboxRouteDetails.Left = 2 * ox + wa;
				groupboxRouteDetails.Width = wb;
				groupboxTrainSelection.Width = wa;
				groupboxTrainDetails.Left = 2 * ox + wa;
				groupboxTrainDetails.Width = wb;
				int oy = (labelRoute.Top - labelStartTitleBackground.Height) / 2;
				int ht = (labelStart.Top - labelRoute.Top - 4 * oy) / 2 - labelRoute.Height - oy;
				groupboxRouteSelection.Height = ht;
				groupboxRouteDetails.Height = ht;
				labelTrain.Top = groupboxRouteSelection.Top + groupboxRouteSelection.Height + 2 * oy;
				groupboxTrainSelection.Top = labelTrain.Top + labelTrain.Height + oy;
				groupboxTrainDetails.Top = labelTrain.Top + labelTrain.Height + oy;
				groupboxTrainSelection.Height = ht;
				groupboxTrainDetails.Height = ht;
				tabcontrolRouteSelection.Width = groupboxRouteSelection.Width - 2 * tabcontrolRouteSelection.Left;
				tabcontrolRouteSelection.Height = groupboxRouteSelection.Height - 3 * tabcontrolRouteSelection.Top / 2;
				tabcontrolRouteDetails.Width = groupboxRouteDetails.Width - 2 * tabcontrolRouteDetails.Left;
				tabcontrolRouteDetails.Height = groupboxRouteDetails.Height - 3 * tabcontrolRouteDetails.Top / 2;
				tabcontrolTrainSelection.Width = groupboxTrainSelection.Width - 2 * tabcontrolTrainSelection.Left;
				tabcontrolTrainSelection.Height = groupboxTrainSelection.Height - 3 * tabcontrolTrainSelection.Top / 2;
				tabcontrolTrainDetails.Width = groupboxTrainDetails.Width - 2 * tabcontrolTrainDetails.Left;
				tabcontrolTrainDetails.Height = groupboxTrainDetails.Height - 3 * tabcontrolTrainDetails.Top / 2;
			} catch { }
			try {
				int width = Math.Min((panelOptions.Width - 24) / 2, 420);
				panelOptionsLeft.Width = width;
				panelOptionsRight.Left = panelOptionsLeft.Left + width + 8;
				panelOptionsRight.Width = width;
			} catch { }
			try {
				int width = Math.Min((panelReview.Width - 32) / 3, 360);
				groupboxReviewRoute.Width = width;
				groupboxReviewTrain.Left = groupboxReviewRoute.Left + width + 8;
				groupboxReviewTrain.Width = width;
				groupboxReviewDateTime.Left = groupboxReviewTrain.Left + width + 8;
				groupboxReviewDateTime.Width = width;
			} catch { }
		}

		// shown
		private void formMain_Shown(object sender, EventArgs e) {
			if (radiobuttonStart.Checked) {
				listviewRouteFiles.Focus();
			} else if (radiobuttonReview.Checked) {
				listviewScore.Focus();
			} else if (radiobuttonControls.Checked) {
				listviewControls.Focus();
			} else if (radiobuttonOptions.Checked) {
				comboboxLanguages.Focus();
			}
			formMain_Resize(null, null);
			if (this.WindowState != FormWindowState.Maximized) {
				Size sss = this.ClientRectangle.Size;
				System.Windows.Forms.Screen s = System.Windows.Forms.Screen.FromControl(this);
				if ((double)this.Width >= 0.95 * (double)s.WorkingArea.Width | (double)this.Height >= 0.95 * (double)s.WorkingArea.Height) {
					this.WindowState = FormWindowState.Maximized;
				}
			}
			// add-ons
			TextboxTrainFilterTextChanged(null, null);
			if (treeviewTrainAddOns.Nodes.Count == 0) {
				tabcontrolTrainSelection.TabPages.RemoveAt(0);
			}
			TextboxRouteFilterTextChanged(null, null);
			if (treeviewRouteAddOns.Nodes.Count == 0) {
				tabcontrolRouteSelection.TabPages.RemoveAt(0);
			}
			radiobuttonStart.Focus();
			// command line arguments
			if (Result.TrainFolder != null) {
				if (checkboxTrainDefault.Checked) checkboxTrainDefault.Checked = false;
				ShowTrain(false);
			}
			if (Result.RouteFile != null) {
				ShowRoute(false);
			}
		}

		// list languages
		private void ListLanguages() {
			string Folder = Program.FileSystem.GetDataFolder("Languages");
			if (System.IO.Directory.Exists(Folder)) {
				string[] Files = System.IO.Directory.GetFiles(Folder);
				string[] LanguageNames = new string[Files.Length];
				LanguageFiles = new string[Files.Length];
				int n = 0;
				for (int i = 0; i < Files.Length; i++) {
					string Title = System.IO.Path.GetFileName(Files[i]);
					if (Title.EndsWith(".cfg", StringComparison.OrdinalIgnoreCase)) {
						string Code = Title.Substring(0, Title.Length - 4);
						string[] Lines = System.IO.File.ReadAllLines(Files[i], System.Text.Encoding.UTF8);
						string Section = "";
						string Name = Code;
						for (int j = 0; j < Lines.Length; j++) {
							Lines[j] = Lines[j].Trim();
							if (Lines[j].StartsWith("[", StringComparison.Ordinal) & Lines[j].EndsWith("]", StringComparison.Ordinal)) {
								Section = Lines[j].Substring(1, Lines[j].Length - 2).Trim().ToLowerInvariant();
							} else if (!Lines[j].StartsWith(";", StringComparison.OrdinalIgnoreCase)) {
								int k = Lines[j].IndexOf('=');
								if (k >= 0) {
									string Key = Lines[j].Substring(0, k).TrimEnd().ToLowerInvariant();
									string Value = Lines[j].Substring(k + 1).TrimStart();
									if (Section == "language" & Key == "name") {
										Name = Value;
										break;
									}
								}
							}
						}
						LanguageFiles[n] = Files[i];
						LanguageNames[n] = Name;
						n++;
					}
				}
				Array.Resize<string>(ref LanguageFiles, n);
				Array.Resize<string>(ref LanguageNames, n);
				Array.Sort<string, string>(LanguageNames, LanguageFiles);
				comboboxLanguages.Items.Clear();
				for (int i = 0; i < n; i++) {
					comboboxLanguages.Items.Add(LanguageNames[i]);
				}
			} else {
				LanguageFiles = new string[] { };
				comboboxLanguages.Items.Clear();
			}
		}

		
		
		// ========
		// top page
		// ========

		// page selection
		private void radiobuttonStart_CheckedChanged(object sender, EventArgs e) {
			panelStart.Visible = true;
			panelReview.Visible = false;
			panelControls.Visible = false;
			panelOptions.Visible = false;
			panelGetAddOns.Visible = false;
			panelPanels.BackColor = labelStartTitle.BackColor;
			pictureboxJoysticks.Visible = false;
			radiobuttonStart.BackColor = SystemColors.ButtonHighlight;
			radiobuttonReview.BackColor = SystemColors.ButtonFace;
			radiobuttonControls.BackColor = SystemColors.ButtonFace;
			radiobuttonOptions.BackColor = SystemColors.ButtonFace;
			radiobuttonGetAddOns.BackColor = SystemColors.ButtonFace;
			UpdateRadioButtonBackColor();
		}
		private void radiobuttonReview_CheckedChanged(object sender, EventArgs e) {
			panelReview.Visible = true;
			panelStart.Visible = false;
			panelControls.Visible = false;
			panelOptions.Visible = false;
			panelGetAddOns.Visible = false;
			panelPanels.BackColor = labelReviewTitle.BackColor;
			pictureboxJoysticks.Visible = false;
			radiobuttonStart.BackColor = SystemColors.ButtonFace;
			radiobuttonReview.BackColor = SystemColors.ButtonHighlight;
			radiobuttonControls.BackColor = SystemColors.ButtonFace;
			radiobuttonOptions.BackColor = SystemColors.ButtonFace;
			radiobuttonGetAddOns.BackColor = SystemColors.ButtonFace;
			UpdateRadioButtonBackColor();
		}
		private void radiobuttonControls_CheckedChanged(object sender, EventArgs e) {
			panelControls.Visible = true;
			panelStart.Visible = false;
			panelReview.Visible = false;
			panelOptions.Visible = false;
			panelGetAddOns.Visible = false;
			panelPanels.BackColor = labelControlsTitle.BackColor;
			pictureboxJoysticks.Visible = true;
			radiobuttonStart.BackColor = SystemColors.ButtonFace;
			radiobuttonReview.BackColor = SystemColors.ButtonFace;
			radiobuttonControls.BackColor = SystemColors.ButtonHighlight;
			radiobuttonOptions.BackColor = SystemColors.ButtonFace;
			radiobuttonGetAddOns.BackColor = SystemColors.ButtonFace;
			UpdateRadioButtonBackColor();
		}
		private void radiobuttonOptions_CheckedChanged(object sender, EventArgs e) {
			panelOptions.Visible = true;
			panelStart.Visible = false;
			panelReview.Visible = false;
			panelControls.Visible = false;
			panelGetAddOns.Visible = false;
			panelPanels.BackColor = labelOptionsTitle.BackColor;
			pictureboxJoysticks.Visible = false;
			radiobuttonStart.BackColor = SystemColors.ButtonFace;
			radiobuttonReview.BackColor = SystemColors.ButtonFace;
			radiobuttonControls.BackColor = SystemColors.ButtonFace;
			radiobuttonOptions.BackColor = SystemColors.ButtonHighlight;
			radiobuttonGetAddOns.BackColor = SystemColors.ButtonFace;
			UpdateRadioButtonBackColor();
		}
		private void RadiobuttonGetAddOnsCheckedChanged(object sender, EventArgs e) {
			panelGetAddOns.Visible = true;
			panelStart.Visible = false;
			panelReview.Visible = false;
			panelControls.Visible = false;
			panelOptions.Visible = false;
			panelPanels.BackColor = labelGetAddOnsTitle.BackColor;
			pictureboxJoysticks.Visible = false;
			radiobuttonStart.BackColor = SystemColors.ButtonFace;
			radiobuttonReview.BackColor = SystemColors.ButtonFace;
			radiobuttonControls.BackColor = SystemColors.ButtonFace;
			radiobuttonOptions.BackColor = SystemColors.ButtonFace;
			radiobuttonGetAddOns.BackColor = SystemColors.ButtonHighlight;
			UpdateRadioButtonBackColor();
			if (radiobuttonGetAddOns.Checked) {
				EnterGetAddOns();
			}
		}
		private void UpdateRadioButtonBackColor() {
			// work-around for button-style radio buttons on Mono
			if (Program.CurrentlyRunningOnMono) {
				radiobuttonStart.BackColor = panelPanels.BackColor;
				radiobuttonReview.BackColor = panelPanels.BackColor;
				radiobuttonControls.BackColor = panelPanels.BackColor;
				radiobuttonOptions.BackColor = panelPanels.BackColor;
				radiobuttonGetAddOns.BackColor = panelPanels.BackColor;
			}
		}

		// homepage
		private void linkHomepage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
			const string Url = "http://odakyufan.zxq.net/openbve/index.html";
			try {
				System.Diagnostics.Process.Start(Url);
			} catch (Exception ex) {
				MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}

		// updates
		private static bool CurrentlyCheckingForUpdates = false;
		private void linkUpdates_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
			if (CurrentlyCheckingForUpdates) {
				return;
			}
			const string url = "http://www.example.com";
			CurrentlyCheckingForUpdates = true;
			this.Cursor = Cursors.WaitCursor;
			Application.DoEvents();
			try {
				byte[] bytes = Internet.DownloadBytesFromUrl(url);
				System.Text.Encoding Encoding = new System.Text.UTF8Encoding();
				string Text = Encoding.GetString(bytes);
				string[] Lines = Text.Split(new char[] { '\r', '\n' });
				if (Lines.Length == 0 || !Lines[0].Equals("$OpenBveVersionInformation", StringComparison.OrdinalIgnoreCase)) {
					MessageBox.Show(Interface.GetInterfaceString("panel_updates_invalid"), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				} else {
					string StableVersion = "0.0.0.0";
					string StableDate = "0000-00-00";
					string DevelopmentVersion = "0.0.0.0";
					string DevelopmentDate = "0000-00-00";
					int i; for (i = 1; i < Lines.Length; i++) {
						if (Lines[i].Equals("----")) break;
						int h = Lines[i].IndexOf('=');
						if (h >= 0) {
							string a = Lines[i].Substring(0, h).TrimEnd();
							string b = Lines[i].Substring(h + 1).TrimStart();
							if (a.Equals("version", StringComparison.OrdinalIgnoreCase)) {
								StableVersion = b;
							} else if (a.Equals("date", StringComparison.OrdinalIgnoreCase)) {
								StableDate = b;
							} else if (a.Equals("developmentversion", StringComparison.OrdinalIgnoreCase)) {
								DevelopmentVersion = b;
							} else if (a.Equals("developmentdate", StringComparison.OrdinalIgnoreCase)) {
								DevelopmentDate = b;
							}
						}
					}
					StringBuilder StableText = new StringBuilder();
					StringBuilder DevelopmentText = new StringBuilder();
					int j; for (j = i + 1; j < Lines.Length; j++) {
						if (Lines[j].Equals("----")) break;
						StableText.AppendLine(Lines[j]);
					}
					for (int k = j + 1; k < Lines.Length; k++) {
						if (Lines[k].Equals("----")) break;
						DevelopmentText.AppendLine(Lines[k]);
					}
					bool Found = false;
					if (ManagedContent.CompareVersions(Application.ProductVersion, StableVersion) < 0) {
						string Message = Interface.GetInterfaceString("panel_updates_new") + StableText.ToString().Trim();
						Message = Message.Replace("[version]", StableVersion);
						Message = Message.Replace("[date]", StableDate);
						MessageBox.Show(Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
						Found = true;
					}
					#pragma warning disable 0162 // Unreachable code
					if (Program.IsDevelopmentVersion) {
						if (ManagedContent.CompareVersions(Application.ProductVersion, DevelopmentVersion) < 0) {
							string Message = Interface.GetInterfaceString("panel_updates_new") + DevelopmentText.ToString().Trim();
							Message = Message.Replace("[version]", DevelopmentVersion);
							Message = Message.Replace("[date]", DevelopmentDate);
							MessageBox.Show(Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
							Found = true;
						}
					}
					#pragma warning restore 0162 // Unreachable code
					if (!Found) {
						string Message = Interface.GetInterfaceString("panel_updates_old");
						MessageBox.Show(Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
				}
			} catch (Exception ex) {
				MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
			this.Cursor = Cursors.Default;
			CurrentlyCheckingForUpdates = false;
		}

		// close
		private void buttonClose_Click(object sender, EventArgs e) {
			this.Close();
		}



		// ======
		// events
		// ======

		// tick
		private void timerEvents_Tick(object sender, EventArgs e) {
			if (textboxJoystickGrab.Focused & this.Tag == null & listviewControls.SelectedIndices.Count == 1) {
				int j = listviewControls.SelectedIndices[0];
				Sdl.SDL_JoystickUpdate();
				for (int k = 0; k < Joysticks.AttachedJoysticks.Length; k++) {
					IntPtr handle = Joysticks.AttachedJoysticks[k].SdlHandle;
					int axes = Sdl.SDL_JoystickNumAxes(handle);
					for (int i = 0; i < axes; i++) {
						double a = (double)Sdl.SDL_JoystickGetAxis(handle, i) / 32768.0;
						if (a < -0.75) {
							Interface.CurrentControls[j].Device = k;
							Interface.CurrentControls[j].Component = Interface.JoystickComponent.Axis;
							Interface.CurrentControls[j].Element = i;
							Interface.CurrentControls[j].Direction = -1;
							radiobuttonJoystick.Focus();
							UpdateJoystickDetails();
							UpdateControlListElement(listviewControls.Items[j], j, true);
							return;
						} else if (a > 0.75) {
							Interface.CurrentControls[j].Device = k;
							Interface.CurrentControls[j].Component = Interface.JoystickComponent.Axis;
							Interface.CurrentControls[j].Element = i;
							Interface.CurrentControls[j].Direction = 1;
							radiobuttonJoystick.Focus();
							UpdateJoystickDetails();
							UpdateControlListElement(listviewControls.Items[j], j, true);
							return;
						}
					}
					int buttons = Sdl.SDL_JoystickNumButtons(handle);
					for (int i = 0; i < buttons; i++) {
						if (Sdl.SDL_JoystickGetButton(handle, i) == 1) {
							Interface.CurrentControls[j].Device = k;
							Interface.CurrentControls[j].Component = Interface.JoystickComponent.Button;
							Interface.CurrentControls[j].Element = i;
							Interface.CurrentControls[j].Direction = 1;
							radiobuttonJoystick.Focus();
							UpdateJoystickDetails();
							UpdateControlListElement(listviewControls.Items[j], j, true);
							return;
						}
					}
					int hats = Sdl.SDL_JoystickNumHats(handle);
					for (int i = 0; i < hats; i++) {
						int hat = Sdl.SDL_JoystickGetHat(handle, i);
						if (hat != 0) {
							Interface.CurrentControls[j].Device = k;
							Interface.CurrentControls[j].Component = Interface.JoystickComponent.Hat;
							Interface.CurrentControls[j].Element = i;
							Interface.CurrentControls[j].Direction = hat;
							radiobuttonJoystick.Focus();
							UpdateJoystickDetails();
							UpdateControlListElement(listviewControls.Items[j], j, true);
							return;
						}
					}
				}
			}
			Sdl.SDL_Event Event;
			while (Sdl.SDL_PollEvent(out Event) != 0) { }
			pictureboxJoysticks.Invalidate();
		}

		
		
		// =========
		// functions
		// =========
		
		// load image
		private Image LoadImage(string Folder, string Title) {
			string File = OpenBveApi.Path.CombineFile(Folder, Title);
			if (System.IO.File.Exists(File)) {
				try {
					return Image.FromFile(File);
				} catch { }
			}
			return null;
		}

		// try load image
		private bool TryLoadImage(PictureBox Box, string Title) {
			string Folder = Program.FileSystem.GetDataFolder("Menu");
			string File = OpenBveApi.Path.CombineFile(Folder, Title);
			if (System.IO.File.Exists(File)) {
				try {
					Box.Image = Image.FromFile(File);
					return true;
				} catch {
					Box.Image = Box.ErrorImage;
					return false;
				}
			} else {
				Box.Image = Box.ErrorImage;
				return false;
			}
		}
		
	}
}