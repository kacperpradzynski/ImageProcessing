using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingCore.Helpers
{
    public static class ImageHelper
    {
		public static byte GetBitsPerPixel(PixelFormat pixelFormat)
		{
			switch (pixelFormat)
			{
				case PixelFormat.Format1bppIndexed:
					return 1;
				case PixelFormat.Format8bppIndexed:
					return 8;
				case PixelFormat.Format24bppRgb:
					return 24;
				case PixelFormat.Format32bppArgb:
				case PixelFormat.Format32bppPArgb:
				case PixelFormat.Format32bppRgb:
					return 32;
				default:
					throw new ArgumentException("Only 1, 8, 24 and 32 bit images are supported");

			}
		}

		public static Bitmap ConvertToPixelFormat(Bitmap input, PixelFormat pixelFormat)
		{
			return input.Clone(new Rectangle(0, 0, input.Width, input.Height), pixelFormat);
		}

		public static byte Range(int color, int min, int max)
		{
			if (color > max)
				return (byte)max;
			if (color < min)
				return (byte)min;
			return (byte)color;
		}
	}
}
