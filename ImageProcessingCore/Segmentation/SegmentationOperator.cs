using ImageProcessingCore.Helpers;
using ImageProcessingCore.Strategy;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingCore.Segmentation
{
    public class SegmentationOperator : IProcessingStrategy
	{
		private int threshold, minPixelsNumber;
		private string path;
        public List<Bitmap> masks;
		public SegmentationOperator(int threshold, int minPixelsNumber, string path)
		{
			this.threshold = threshold;
			this.minPixelsNumber = minPixelsNumber;
			this.path = path;
		}
		public unsafe ImageModel Process(ImageModel input)
		{
			Bitmap output = input.SpatialDomain.Clone(new Rectangle(0, 0, input.SpatialDomain.Width, input.SpatialDomain.Height), input.SpatialDomain.PixelFormat);

			BitmapData bData = output.LockBits(new Rectangle(0, 0, output.Width, output.Height), ImageLockMode.ReadWrite, output.PixelFormat);
			byte bitsPerPixel = ImageHelper.GetBitsPerPixel(bData.PixelFormat);
			byte* scan0 = (byte*)bData.Scan0.ToPointer();

			if (bitsPerPixel == 8)
			{
                List<int> seeds = GenerateSeeds(input.SpatialDomain);
                int[] imagePixels = new int[input.FrequencyDomain.Length];
                List<HashSet<int>> regions = new List<HashSet<int>>();
                AllocateRegions(scan0, seeds, threshold, minPixelsNumber, imagePixels, regions, input.SpatialDomain);
                ProcessUnallocatedPixels(scan0, imagePixels, regions, input.SpatialDomain);
                int color = 255 / regions.Count;

                for (int i = 0; i < imagePixels.Length; i++)
                {
                    byte* data = scan0 + i;
                    data[0] = (byte)((imagePixels[i] * color) - color);
                }

                masks = new List<Bitmap>();

                int counter = 1;
                foreach (var region in regions)
                {
                    var mask = CreateMaskImage(input.SpatialDomain, region);
                    mask.Save($"./{counter++}_mask.png");
                    masks.Add(mask);
                }

            }

			output.UnlockBits(bData);

			return new ImageModel(output);
		}

        private unsafe Bitmap CreateMaskImage(Bitmap input, HashSet<int> region)
        {
            Bitmap output = input.Clone(new Rectangle(0, 0, input.Width, input.Height), input.PixelFormat);
            BitmapData bData = output.LockBits(new Rectangle(0, 0, output.Width, output.Height), ImageLockMode.ReadWrite, output.PixelFormat);
            byte bitsPerPixel = ImageHelper.GetBitsPerPixel(bData.PixelFormat);
            byte* scan0 = (byte*)bData.Scan0.ToPointer();


            for (int i = 0; i < bData.Height; ++i)
            {
                for (int j = 0; j < bData.Width; ++j)
                {
                    byte* data = scan0 + i * bData.Stride + j * bitsPerPixel / 8;
                    var index = i * bData.Stride + j * bitsPerPixel / 8;
                    data[0] = (byte)(region.Contains(index) ? 255 : 0);
                }
            }

            output.UnlockBits(bData);
            return output;
            
        }

        public static unsafe Bitmap ApplyMask(Bitmap input, Bitmap mask)
        {
            Bitmap output = input.Clone(new Rectangle(0, 0, input.Width, input.Height), input.PixelFormat);
            BitmapData bData = output.LockBits(new Rectangle(0, 0, output.Width, output.Height), ImageLockMode.ReadWrite, output.PixelFormat);
            byte bitsPerPixel = ImageHelper.GetBitsPerPixel(bData.PixelFormat);
            byte* scan0 = (byte*)bData.Scan0.ToPointer();

            BitmapData bDataMask = mask.LockBits(new Rectangle(0, 0, mask.Width, mask.Height), ImageLockMode.ReadWrite, mask.PixelFormat);
            byte* scan0Mask = (byte*)bDataMask.Scan0.ToPointer();

            for (int i = 0; i < bData.Height; ++i)
            {
                for (int j = 0; j < bData.Width; ++j)
                {
                    byte* data = scan0 + i * bData.Stride + j * bitsPerPixel / 8;
                    byte* dataMask = scan0Mask + i * bDataMask.Stride + j * bitsPerPixel / 8;
                    data[0] = dataMask[0] == 0 ? data[0] : (byte)255; 
                }
            }

            output.UnlockBits(bData);
            mask.UnlockBits(bDataMask);

            return output;
        }

        private List<int> GenerateSeeds(Bitmap image)
		{
			List<int> seeds = new List<int>();
			byte bitsPerPixel = ImageHelper.GetBitsPerPixel(image.PixelFormat);

			for (int i = 0; i < image.Width; i++)
			{
				for (int j = 0; j < image.Height; j++)
				{
					if (i % 2 == 0 && j % 2 == 0)
					{
						seeds.Add(i * image.Height + j * bitsPerPixel / 8);
					}
				}
			}

			seeds.Shuffle();

			return seeds;
		}

        private List<int> GetNeighbourhood(int position, HashSet<int> visited, int[] imagePixels, Bitmap image)
        {
            int WidthInBytes = image.Width * ImageHelper.GetBitsPerPixel(image.PixelFormat) / 8;
            int HeightInPixels = image.Height;
            int x = position % WidthInBytes;
            int y = position / WidthInBytes;
            var neighbourhood = new List<int>();

            for (int j = -1; j < 2; j++)
            {
                for (int k = -1; k < 2; k++)
                {
                    var currY = y + j;
                    var currX = x + k;
                    var currPos = currY * WidthInBytes + currX;
                    if (currY >= 0 && currY < HeightInPixels &&
                        currX >= 0 && currX < WidthInBytes &&
                        !visited.Contains(currPos) && imagePixels[currPos] == 0)
                    {
                        neighbourhood.Add(currPos);
                    }
                }
            }

            return neighbourhood;
        }

        public static List<int> GetProcessedNeighbourhood(int position, int[] imagePixels, Bitmap image)
        {
            int WidthInBytes = image.Width * ImageHelper.GetBitsPerPixel(image.PixelFormat) / 8;
            int HeightInPixels = image.Height;
            int x = position % WidthInBytes;
            int y = position / WidthInBytes;
            var neighbourhood = new List<int>();

            for (int j = -2; j < 3; j++)
            {
                for (int k = -2; k < 3; k++)
                {
                    var currY = y + j;
                    var currX = x + k;
                    var currPos = currY * WidthInBytes + currX;
                    if (currY >= 0 && currY < HeightInPixels &&
                        currX >= 0 && currX < WidthInBytes &&
                        imagePixels[currPos] != 0)
                    {
                        neighbourhood.Add(currPos);
                    }
                }
            }

            return neighbourhood;
        }

        private unsafe void AllocateRegions(byte* scan0, List<int> seeds, int threshold, int minPixelsInRegion, int[] imagePixels, List<HashSet<int>> regions, Bitmap image)
        {
            int counter = 1;
            for (int i = 0; i < seeds.Count; i++)
            {
                var seed = seeds[i];

                if (imagePixels[seed] != 0)
                {
                    continue;
                }

                var stack = new Stack<int>();
                var region = new HashSet<int>();
                var visited = new HashSet<int>();

                var regionMin = (scan0 + seed)[0];
                var regionMax = (scan0 + seed)[0];

                stack.Push(seed);
                region.Add(seed);
                visited.Add(seed);

                while (stack.Count > 0)
                {
                    var currentPixel = stack.Pop();
                    var neighbourhood = GetNeighbourhood(currentPixel, visited, imagePixels, image);

                    foreach (var neighbour in neighbourhood)
                    {
                        visited.Add(neighbour);
                        var neighbourValue = (scan0 + neighbour)[0];
                        var min = regionMin > neighbourValue ? neighbourValue : regionMin;
                        var max = regionMax < neighbourValue ? neighbourValue : regionMax;

                        if (max - min < threshold)
                        {
                            stack.Push(neighbour);
                            region.Add(neighbour);
                            regionMin = min;
                            regionMax = max;
                        }
                    }
                }

                if (region.Count > minPixelsInRegion)
                {
                    foreach (var item in region)
                    {
                        imagePixels[item] = counter;
                    }

                    regions.Add(region);
                    counter++;
                }
            }
        }

        private unsafe void ProcessUnallocatedPixels(byte* scan0, int[] imagePixels, List<HashSet<int>> regions, Bitmap image)
        {
            var unallocatedCount = imagePixels.Count(c => c == 0);
            while (unallocatedCount != 0)
            {
                for (int i = 0; i < imagePixels.Length; i++)
                {
                    if (imagePixels[i] == 0)
                    {
                        var currentValue = (scan0)[i];
                        var neighbourhood = GetProcessedNeighbourhood(i, imagePixels, image);
                        var closestRegion = 0;
                        var closestDiff = 255;

                        foreach (var neighbour in neighbourhood)
                        {
                            var neighbourValue = (scan0)[neighbour];
                            var currDiff = Math.Abs(currentValue - neighbourValue);

                            if (closestDiff > currDiff)
                            {
                                closestRegion = imagePixels[neighbour];
                                closestDiff = currDiff;
                            }
                        }

                        if (closestRegion != 0)
                        {
                            regions[closestRegion - 1].Add(i);
                        }

                        imagePixels[i] = closestRegion;
                    }
                }

                unallocatedCount = imagePixels.Count(c => c == 0);
            }
        }
    }
}
