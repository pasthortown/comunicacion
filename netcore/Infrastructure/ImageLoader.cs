using System.Drawing;

namespace ImageActivityMonitor.Infrastructure
{
    public class ImageLoader
    {
        public Image LoadImage(string path, int desiredWidth, out int imgWidth, out int imgHeight)
        {
            var original = Image.FromFile(path);
            if (desiredWidth > 0)
            {
                float scale = (float)desiredWidth / original.Width;
                imgWidth = desiredWidth;
                imgHeight = (int)(original.Height * scale);
                return new Bitmap(original, new Size(imgWidth, imgHeight));
            }
            else
            {
                imgWidth = original.Width;
                imgHeight = original.Height;
                return original;
            }
        }
    }
}
