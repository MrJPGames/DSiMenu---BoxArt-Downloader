using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSiMenu___BoxArt_Downloader {
	public partial class Form1 : Form {
		private static string boxArtDirectory = "_nds\\dsimenuplusplus\\boxart\\";
		List<String> drives = new List<String>();

		bool validDriveSelected = false;

		public Form1() {
			InitializeComponent();

			//Detect all drives (Only USB will be added (SD is also seen as USB or "removable"))
			UpdateAvailableDrives();
		}

		//Needed input for function to allow it as button click event call
		private void UpdateAvailableDrives(object s = null, EventArgs e = null) {
			//Clear drives list
			drives.RemoveRange(0, drives.Count);

			//Detect drives
			System.IO.DriveInfo[] drivesListing = System.IO.DriveInfo.GetDrives();
			foreach (var drive in drivesListing) {
				System.IO.DriveType driveType = drive.DriveType;
				string driveName = drive.Name; // C:\, E:\, etc:\
				switch (driveType) {
					case System.IO.DriveType.Removable:
						// Usually a USB Drive
						drives.Add(drive.Name);
						break;
				}
			}

			comboBox1.Items.Clear();
			comboBox1.Text = "";

			//Add drives to combobox
			foreach(string d in drives) {
				comboBox1.Items.Add(d);
			}
		}

		private void StartBoxArtDownload(object sender, EventArgs e) {
			int total = 0;
			int current = 0;
			int totalSuccesful = 0;

			if (validDriveSelected) {
				var ext = new List<string> { ".nds" };
				var myFiles = Directory.GetFiles(comboBox1.Text, "*.*", SearchOption.AllDirectories)
					 .Where(s => ext.Contains(Path.GetExtension(s)));

				//Really ugly but couldn't find a way to find the Length/Count of myFiles
				foreach (string f in myFiles) {
					string gameCode = BoxArtDownloader.GetGameCode(f);
					if (BoxArtDownloader.GetSystemType(f) != 0x03 && gameCode != "####" && gameCode != "KBSE")
						total++;
				}
				foreach (string f in myFiles) {
					string gameCode = BoxArtDownloader.GetGameCode(f);
					if (gameCode != "####" && gameCode != "KBSE") {
						//Skip homebrew titles!
						Bitmap boxArt = null;
						if (BoxArtDownloader.GetSystemType(f) != 0x03) {
							//Game is NTR or DSi-Enhanced (DS game carts)
							current++;
							Console.WriteLine(current);
							progressBar1.Value = (int)(((float)current / total)*100);
							progressBar1.Invalidate();
							progressBar1.Update();
							boxArt = BoxArtDownloader.DownloadArtNTR(gameCode, button3);

							if (boxArt != null) {
								totalSuccesful++;
								BoxArtDownloader.SaveBoxArt(boxArt, comboBox1.Text + boxArtDirectory,gameCode);
								pictureBox1.Image = boxArt;
								pictureBox1.Invalidate();
								pictureBox1.Update();
							} else {
								Console.WriteLine("Failed to get boxart for: " + gameCode + ", " + f);
							}
						} else {
							//Game is DSi-Exclusive or DSiWare
							//Skip until a site/database of DSiWare cover art data is found
						}
					}
				}
			}
			label2.Text = totalSuccesful + "/" + total + " found and downloaded!";
			button3.Text = "Download BoxArt!";
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) {
			int total = 0;
			if (Directory.Exists(comboBox1.Text + "_nds\\dsimenuplusplus\\boxart")) {
				var ext = new List<string> { ".nds" };
				var myFiles = Directory.GetFiles(comboBox1.Text, "*.*", SearchOption.AllDirectories)
					 .Where(s => ext.Contains(Path.GetExtension(s)));

				//Really ugly but couldn't find a way to find the Length/Count of myFiles
				foreach (string f in myFiles) {
					string gameCode = BoxArtDownloader.GetGameCode(f);
					if (BoxArtDownloader.GetSystemType(f) != 0x03 &&  gameCode != "####" && gameCode != "KBSE")
						total++;
				}
				validDriveSelected = true;
				label2.Text = total + " NTR roms detected";
				button3.Enabled = true;
			} else {
				validDriveSelected = false;
				label2.Text = "SD Card with no or old DSiMenu++";
				button3.Enabled = false;
			}
		}

		private void manualDirectorySelectToolStripMenuItem_Click(object sender, EventArgs e) {
			var win = new ManualMode();
			win.ShowDialog();
		}
	}
}
