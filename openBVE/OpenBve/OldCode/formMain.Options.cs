﻿using System;
using System.Drawing;
using System.Windows.Forms;

namespace OpenBve {
	internal partial class formMain : Form {
		
		
		// =======
		// options
		// =======

		// language
		private void comboboxLanguages_SelectedIndexChanged(object sender, EventArgs e) {
			if (this.Tag != null) return;
			int i = comboboxLanguages.SelectedIndex;
			if (i >= 0 & i < LanguageFiles.Length) {
				string Code = System.IO.Path.GetFileNameWithoutExtension(LanguageFiles[i]);
				string Folder = Program.FileSystem.GetDataFolder("Flags");
				#if !DEBUG
				try {
					#endif
					Interface.LoadLanguage(LanguageFiles[i]);
					#if !DEBUG
				} catch (Exception ex) {
					MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
				#endif
				#if !DEBUG
				try {
					#endif
					string Flag = Interface.GetInterfaceString("language_flag");
					string File = OpenBveApi.Path.CombineFile(Folder, Flag);
					if (!System.IO.File.Exists(File)) {
						File = OpenBveApi.Path.CombineFile(Folder, "unknown.png");
					}
					if (System.IO.File.Exists(File)) {
						pictureboxLanguage.Image = Image.FromFile(File);
					} else {
						pictureboxLanguage.Image = null;
					}
					CurrentLanguageCode = Code;
					#if !DEBUG
				} catch { }
				#endif
				ApplyLanguage();
				TextboxRouteFilterTextChanged(null, null);
				TextboxTrainFilterTextChanged(null, null);
			}
		}

		// interpolation
		private void comboboxInterpolation_SelectedIndexChanged(object sender, EventArgs e) {
			int i = comboboxInterpolation.SelectedIndex;
			bool q = i == (int)Interface.InterpolationMode.AnisotropicFiltering;
			labelAnisotropic.Enabled = q;
			updownAnisotropic.Enabled = q;
			q = i != (int)Interface.InterpolationMode.NearestNeighbor & i != (int)Interface.InterpolationMode.Bilinear;
		}

		
		// =======
		// options
		// =======

		// joysticks enabled
		private void checkboxJoysticksUsed_CheckedChanged(object sender, EventArgs e) {
			groupboxJoysticks.Enabled = checkboxJoysticksUsed.Checked;
		}

		
		
	}
}