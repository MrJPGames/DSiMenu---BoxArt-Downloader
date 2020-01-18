using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSiMenu___BoxArt_Downloader {
	class BoxArtDownloader {
		public static string GetGameCode(string file) {
			FileStream fs = File.OpenRead(file);
			byte[] gameCode = new byte[4];
			fs.Seek(0x0C, SeekOrigin.Begin);
			int nBytesRead = fs.Read(gameCode, 0, 4);
			return System.Text.Encoding.Default.GetString(gameCode);
		}

		public static int GetSystemType(string file) {
			FileStream fs = File.OpenRead(file);
			byte[] systemType = new byte[1];
			fs.Seek(0x12, SeekOrigin.Begin);
			int nBytesRead = fs.Read(systemType, 0, 1);
			return (int)systemType[0];
		}

		public static bool DownloadBitmap(string url, ref Bitmap b) {
			System.Net.WebRequest request = System.Net.WebRequest.Create(url);
			try {
				System.Net.WebResponse response = request.GetResponse();
				System.IO.Stream responseStream = response.GetResponseStream();
				b = new Bitmap(responseStream);
				responseStream.Close();
				response.Close();
				return true;
			} catch (Exception) {
				return false;
			}
		}

		public static Bitmap DownloadArtNTR(string gameCode, Control statusObj) {
			statusObj.Text = "Downloading box art for " + gameCode;
			statusObj.Invalidate();
			statusObj.Update();
			Bitmap ret = null;
			String US_Url = "http://art.gametdb.com/ds/coverS/EN/" + gameCode + ".png";
			String EU_Url = "http://art.gametdb.com/ds/coverS/US/" + gameCode + ".png";
			String JP_Url = "http://art.gametdb.com/ds/coverS/JA/" + gameCode + ".png";
			String AU_Url = "http://art.gametdb.com/ds/coverS/AU/" + gameCode + ".png";
			if (gameCode[3] == 'E') {
				//Rom is most likely US
				if (!DownloadBitmap(US_Url, ref ret))
					if (!DownloadBitmap(EU_Url, ref ret))
						DownloadBitmap(JP_Url, ref ret); //Just to be sure (in case a non ***J game is JAP)
			} else if (gameCode[3] == 'J') {
				//Rom is most likely JAP
				if (!DownloadBitmap(JP_Url, ref ret))
					if (!DownloadBitmap(US_Url, ref ret))
						DownloadBitmap(EU_Url, ref ret);
			} else {
				//So it's EU?
				if (!(gameCode[3] == 'H' && DownloadBitmap("http://art.gametdb.com/ds/coverS/JA/" + gameCode + ".png", ref ret))) {
					if (!DownloadBitmap(EU_Url, ref ret))
						if (!DownloadBitmap(AU_Url, ref ret)) //Do AU box art if default PAL/EU boxart is not available (still English)
							if (!DownloadBitmap(US_Url, ref ret)) //Get US if no EU available (in english)
								DownloadBitmap(JP_Url, ref ret); //Just to be sure (in case a non ***J game is JAP)
				}
			}
			return ret;
		}

		public static void SaveBoxArt(Bitmap b, String saveTo, String gameCode) {
			b.Save(saveTo + gameCode + ".png", ImageFormat.Png);
		}

		public static Bitmap ConvertTo16bpp(Image img) {
			var bmp = new Bitmap(img.Width, img.Height, PixelFormat.Format16bppRgb555);
			using (var gr = Graphics.FromImage(bmp))
				gr.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height));
			return bmp;
		}
	}
}
