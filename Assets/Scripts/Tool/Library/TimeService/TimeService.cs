using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Kun.Tool
{
    public class UnityTimeService : ITimeService
    {
        float ITimeService.SysTime => Time.time;
    }
}