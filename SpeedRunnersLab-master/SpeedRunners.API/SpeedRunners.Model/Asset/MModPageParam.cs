﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SpeedRunners.Model.Asset
{
    public class MModPageParam : MPageParam
    {
        public int Tag { get; set; }
        public bool OnlyStar { get; set; } = false;
    }
}
