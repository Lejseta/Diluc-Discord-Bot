using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Drawing;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;
using System.Drawing.Imaging;

namespace Bot
{
    public class ImageProcessing
    {
        public static Bitmap ConvertToGray(Bitmap image)
        {
            for (int i = 0; i < image.Width; i++)
                for (int j = 0; j < image.Height; j++)
                {
                    Color c1 = image.GetPixel(i, j);
                    //rgb
                    int r = c1.R;
                    int g = c1.G;
                    int b = c1.B;
                    int gray = (byte)(0.299 * r + 0.587 * g + 0.114 * b);
                    r = gray;
                    g = gray;
                    b = gray;
                    image.SetPixel(i, j, Color.FromArgb(r, g, b));
                }
            return image;
        }

        public static async Task<Stream> DownloadStreamImage(string url)
        {
            using (HttpClient http = new HttpClient())
            {
                
                var web = await http.GetAsync(url);
                return web.Content.ReadAsStreamAsync().Result;
            }
        }
        public static async Task<byte[]> DownloadRawImage(string url)
        {
            using (HttpClient http = new HttpClient())
            {
                var web = await http.GetAsync(url);
                return web.Content.ReadAsByteArrayAsync().Result;
            }
        }

        public static Image BytesToImage(byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                return Image.FromStream(stream);
            }
        }
        public static async Task<Image> DownloadImage(string url)
        {
            using (HttpClient http = new HttpClient())
            {
                var web = await http.GetAsync(url);
                return (BytesToImage(web.Content.ReadAsByteArrayAsync().Result));
            }
        }
    }

}
