﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roadlight.Model
{
    [Serializable]
    [Flags]
    public enum TaskMessageStatusEnum
    {
        None = 0,
        Executed = 1
    }
}
