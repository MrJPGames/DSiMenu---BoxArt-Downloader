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
	public partial class HiyaSplashSetterView : Form {
		bool fileSet = false;
		bool driveSet = false;

		public HiyaSplashSetterView() {
			InitializeComponent();
			UpdateAvailableDrives();
		}

		private void UpdateAvailableDrives(object s = null, EventArgs e = null) {
			List<String> drives = new List<String>();

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
			foreach (string d in drives) {
				comboBox1.Items.Add(d);
			}
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) {
			driveSet = false;
			if (!Directory.Exists(comboBox1.Text + "hiya")) {
				MessageBox.Show("HiyaCFW was not found on this drive!\nPlease select a different drive, or install HiyaCFW on this one first!");
				comboBox1.Text = "";
			} else {
				driveSet = true;
				UpdateGoButton();
			}
		}

		private void UpdateGoButton() {
			if (driveSet && fileSet) {
				button2.Enabled = true;
			} else {
				button2.Enabled = false;
			}
		}

		private void SelectSplashFile(object sender, EventArgs e) {
			//In case of re-setting file being incorrect
			fileSet = false;
			if (openFileDialog1.ShowDialog() == DialogResult.OK) {
				try {
					Bitmap splash = new Bitmap(openFileDialog1.FileName);
					if (splash.Width != 256 || splash.Height != 192) {
						MessageBox.Show("Image has to have the dimensions of 256x192 pixels!");
					} else {
						textBox1.Text = openFileDialog1.FileName;
						fileSet = true;
						UpdateGoButton();
					}
				} catch (Exception) {
					MessageBox.Show("Unsupported file type!");
				}
			}
		}

		private void button3_Click(object sender, EventArgs e) {
			this.Close();
		}

		//Set splash screen
		private void button2_Click(object sender, EventArgs e) {
			Bitmap final = ConvertTo16bpp(new Bitmap(openFileDialog1.FileName));
			final.Save(comboBox1.Text + "hiya\\splashtop.bmp", ImageFormat.Bmp);
			this.Close();
		}

		public static Bitmap ConvertTo16bpp(Image img) {
			var bmp = new Bitmap(img.Width, img.Height, PixelFormat.Format16bppRgb555);
			using (var gr = Graphics.FromImage(bmp))
				gr.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height));
			return bmp;
		}
	}
}
