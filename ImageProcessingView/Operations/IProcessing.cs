using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageProcessingCore.Strategy;

namespace ImageProcessingView.Operations
{
    public interface IProcessing
    {
        IProcessingStrategy GetOperationStrategy();
    }
}
