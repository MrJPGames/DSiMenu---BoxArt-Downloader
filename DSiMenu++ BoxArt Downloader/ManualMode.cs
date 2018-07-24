using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSiMenu___BoxArt_Downloader {
	public partial class ManualMode : Form {
		public ManualMode() {
			InitializeComponent();
		}

		private void DownloadBoxArt(object sender, EventArgs e) {
			button1.Enabled = false;
			button1.Invalidate();
			button1.Update();
			if (textBox1.Text != "" && textBox2.Text != "") {
				if (Directory.Exists(textBox1.Text) && Directory.Exists(textBox1.Text)) {
					int total = 0;
					int current = 0;
					int totalSuccesful = 0;
					var ext = new List<string> { ".nds" };
					var myFiles = Directory.GetFiles(textBox1.Text, "*.*", SearchOption.AllDirectories)
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
								progressBar1.Value = (int)(((float)current / total) * 100);
								progressBar1.Invalidate();
								progressBar1.Update();
								boxArt = BoxArtDownloader.DownloadArtNTR(gameCode, label4);

								if (boxArt != null) {
									totalSuccesful++;
									BoxArtDownloader.SaveBoxArt(boxArt, textBox2.Text + "\\", gameCode);
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
					label4.Text = totalSuccesful + "/" + total + " found and downloaded!";
				}
			}
			button1.Enabled = true;
		}

		private void ChangeRomsDir(object sender, EventArgs e) {
			DialogResult result = folderBrowserDialog1.ShowDialog();
			if (result == DialogResult.OK) {
				textBox1.Text = folderBrowserDialog1.SelectedPath;
			}
		}

		private void ChangeOutputDir(object sender, EventArgs e) {
			DialogResult result = folderBrowserDialog1.ShowDialog();
			if (result == DialogResult.OK) {
				textBox2.Text = folderBrowserDialog1.SelectedPath;
			}
		}
	}
}
