using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoProcessor.DTO.enums
{
    public enum JobStatus
    {
        None = 0,
        Queued = 1,
        Running = 2,
        Failed= 3,
        Done = 4,
    }
}
