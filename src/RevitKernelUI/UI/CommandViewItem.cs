using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitKernelUI
{
    public class CommandViewItem
    {
        public string KernelEvent { get; set; }

        public string Command { get; set; }
        public string TargetKernel { get; internal set; }
    }
}
