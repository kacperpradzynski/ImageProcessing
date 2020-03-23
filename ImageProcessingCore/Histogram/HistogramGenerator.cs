using ImageProcessingCore.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingCore.Histogram
{
    public static class HistogramGenerator
    {
        public static unsafe List<int[]> Generate(Bitmap image)
		{
            List<int[]> toReturn = new List<int[]>();
            int[] red = new int[256];
            int[] green = new int[256];
            int[] blue = new int[256];

			BitmapData bData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, image.PixelFormat);
			byte bitsPerPixel = ImageHelper.GetBitsPerPixel(bData.PixelFormat);
			byte* scan0 = (byte*)bData.Scan0.ToPointer();

			if(bitsPerPixel == 8)
			{
				for (int i = 0; i < bData.Height; ++i)
				{
					for (int j = 0; j < bData.Width; ++j)
					{
						byte* data = scan0 + i * bData.Stride + j * bitsPerPixel / 8;

						red[data[0]]++;
					}
				}
				green = red;
				blue = red;
			} else
			{
				for (int i = 0; i < bData.Height; ++i)
				{
					for (int j = 0; j < bData.Width; ++j)
					{
						byte* data = scan0 + i * bData.Stride + j * bitsPerPixel / 8;

						red[data[0]]++;
						green[data[1]]++;
						blue[data[2]]++;
					}
				}
			}

			image.UnlockBits(bData);

			toReturn.Add(red);
			toReturn.Add(green);
			toReturn.Add(blue);

			return toReturn;
        }
	}
}
