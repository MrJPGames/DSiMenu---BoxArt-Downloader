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
		private string gamesDirectory = "";
		private string boxArtDirectory = "";
		public Form1() {
			InitializeComponent();
		}

		private void ChangeGamesDirectory(object sender, EventArgs e) {
			// Show the FolderBrowserDialog.
			DialogResult result = folderBrowserDialog1.ShowDialog();
			if (result == DialogResult.OK) {
				gamesDirectory = folderBrowserDialog1.SelectedPath;
				textBox1.Text = gamesDirectory;
			}
		}

		private void ChangeBoxArtDirectory(object sender, EventArgs e) {
			// Show the FolderBrowserDialog.
			DialogResult result = folderBrowserDialog1.ShowDialog();
			if (result == DialogResult.OK) {
				boxArtDirectory = folderBrowserDialog1.SelectedPath;
				textBox2.Text = boxArtDirectory;
			}
		}

		private void StartBoxArtDownload(object sender, EventArgs e) {
			Console.WriteLine(gamesDirectory);
			Console.WriteLine(boxArtDirectory);
			if (gamesDirectory != "" && boxArtDirectory != "" && Directory.Exists(gamesDirectory) && Directory.Exists(boxArtDirectory)) {
				Console.WriteLine("Initial check done!");
				var ext = new List<string> { ".nds" };
				var myFiles = Directory.GetFiles(gamesDirectory, "*.*", SearchOption.AllDirectories)
					 .Where(s => ext.Contains(Path.GetExtension(s)));

				//Really ugly but couldn't find a way to find the Length/Count of myFiles
				int total = 0;
				foreach (string f in myFiles) {
					total++;
				}
				int current = 0;
				foreach (string f in myFiles) {
					current++;
					Console.WriteLine(current + " " +  total + " " +  (int)((float)current / total));
					progressBar1.Value = (int)((float)current * 100 / total);
					progressBar1.Invalidate();
					progressBar1.Update();
					string gameCode = GetGameCode(f);
					if (gameCode != "####") {
						//Skip homebrew titles!
						int systemType = GetSystemType(f);
						Bitmap boxArt = null;
						if (systemType != 0x03) {
							//Game is NTR or DSi-Enhanced (DS game carts)
							boxArt = DownloadArtNTR(gameCode);
						} else {
							//Game is DSi-Exclusive or DSiWare
							//Skip until a site/database of DSiWare cover art data is found
						}
						if (boxArt != null)
							SaveBoxArt(boxArt, gameCode);
					}
				}
			}
			button3.Text = "Download BoxArt!";
		}

		private string GetGameCode(string file) {
			FileStream fs = File.OpenRead(file);
			byte[] gameCode = new byte[4];
			fs.Seek(0x0C, SeekOrigin.Begin);
			int nBytesRead = fs.Read(gameCode, 0, 4);
			return System.Text.Encoding.Default.GetString(gameCode);
		}

		private int GetSystemType(string file) {
			FileStream fs = File.OpenRead(file);
			byte[] systemType = new byte[1];
			fs.Seek(0x12, SeekOrigin.Begin);
			int nBytesRead = fs.Read(systemType, 0, 1);
			return (int)systemType[0];
		}

		private Bitmap DownloadArtNTR(string gameCode) {
			button3.Text = "Downloading box art for " + gameCode;
			button3.Invalidate();
			button3.Update();
			System.Net.WebRequest request = System.Net.WebRequest.Create("http://art.gametdb.com/ds/coverS/EN/" + gameCode + ".png");
			try {
				System.Net.WebResponse response = request.GetResponse();
				System.IO.Stream responseStream = response.GetResponseStream();
				return new Bitmap(responseStream);
			} catch(Exception e) {
			}
			return null;
		}

		private void SaveBoxArt(Bitmap b, String gameCode) {
			/*
			Byte[] imageData = new Byte[128 * 115 * 2];
			UInt16 dataPoint;
			for (int i=0; i<256; i+=2) {
				for (int j=0; j<115; j++) {
					Color c = b.GetPixel(i/2, j);
					//A1R5G5B5
					dataPoint = (UInt16)(1 << 15 | (int)((c.R / 255.0) * 32) << 10 | (int)((c.G / 255.0)*32) | (int)((c.B / 255.0)*32));
					imageData[i + j * 256] = (Byte)(dataPoint >> 8);
					imageData[i + j * 256 + 1] = (Byte)(dataPoint);
				}
			}
			File.WriteAllBytes(boxArtDirectory + "\\" + gameCode + ".bmp", imageData);
			*/
			Bitmap finalImage = ConvertTo16bpp(b);
			finalImage.Save(boxArtDirectory + "\\" + gameCode + ".bmp", ImageFormat.Bmp);
		}

		public static Bitmap ConvertTo16bpp(Image img) {
			var bmp = new Bitmap(img.Width, img.Height, PixelFormat.Format16bppRgb555);
			using (var gr = Graphics.FromImage(bmp))
				gr.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height));
			return bmp;
		}
	}
}
