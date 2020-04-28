using ImageProcessingCore.FourierTransform;
using ImageProcessingCore.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingCore.FourierOperations
{
    public class PhaseModificationOperator : IProcessingStrategy
	{
		private int k, l;
		public PhaseModificationOperator(int k, int l)
		{
			this.k = k;
			this.l = l;
		}

		public ImageModel Process(ImageModel input)
		{
			Complex[,] frequencyDomainCopy = input.FrequencyDomain.Clone() as Complex[,];
			FFT.SwapQuadrants(frequencyDomainCopy);
			double imageWidth = input.FrequencyDomain.GetUpperBound(0) + 1;
			double imageHeight = input.FrequencyDomain.GetUpperBound(1) + 1;
			
			for (int i = 0; i < imageWidth; i++)
			{
				for (int j = 0; j < imageHeight; j++)
				{
					frequencyDomainCopy[i, j] *= Complex.Exp(
						new Complex(0,
						(-i * k * 2 * Math.PI / imageHeight) +
						(-j * l * 2 * Math.PI / imageWidth) +
						(Math.PI * (k + l))));
				}
			}

			FFT.SwapQuadrants(frequencyDomainCopy);
			return new ImageModel(frequencyDomainCopy, input.SpatialDomain);
		}
	}
}
