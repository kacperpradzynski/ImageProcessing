using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingCore.Strategy
{
    public interface IProcessingStrategy
    {
        unsafe Bitmap Process(Bitmap input);
    }
}
