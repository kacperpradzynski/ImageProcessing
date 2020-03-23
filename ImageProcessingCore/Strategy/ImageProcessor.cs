using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;

namespace ImageProcessingCore.Strategy
{
    public class ImageProcessor
    {
        private IProcessingStrategy strategy;
        private Bitmap input;

        public ImageProcessor(IProcessingStrategy strategy, Bitmap input)
        {
            this.strategy = strategy;
            this.input = input;
        }

        public Bitmap Process()
        {
            return strategy.Process(input);
        }

    }
}
