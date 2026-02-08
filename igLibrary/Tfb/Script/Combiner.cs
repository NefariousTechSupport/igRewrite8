using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace igLibrary.Tfb.Script
{
    public enum Combiner
    {
        include = 0,
        exclude = 1,
        intersect_with = 2,
        be_replaced_by = 3,
        add = 4,
        exclude_all = 5,
    }
}
