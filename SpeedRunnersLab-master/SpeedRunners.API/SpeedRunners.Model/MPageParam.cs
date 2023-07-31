using System;
using System.Collections.Generic;
using System.Text;

namespace SpeedRunners.Model
{
    public class MPageParam
    {
        public int PageNo { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string Keywords { get; set; }
        public string FuzzyKeywords => $"%{Keywords}%";
    }
}
