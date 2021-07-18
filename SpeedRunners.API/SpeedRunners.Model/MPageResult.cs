using System;
using System.Collections.Generic;
using System.Text;

namespace SpeedRunners.Model
{
    public class MPageResult<T> where T : class
    {
        public int Total { get; set; }
        public IEnumerable<T> List { get; set; }
    }
}
