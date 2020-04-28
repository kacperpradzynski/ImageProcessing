using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingCore.Strategy
{
    public class ImageProcessor
    {
        public IProcessingStrategy strategy;
        private ImageModel input;

        public ImageProcessor(IProcessingStrategy strategy, ImageModel input)
        {
            this.strategy = strategy;
            this.input = input;
        }

        public ImageModel Process()
        {
            return strategy.Process(input);
        }

    }
}
