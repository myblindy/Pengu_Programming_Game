﻿using Pengu.Renderer;
using Pengu.VirtualMachine;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Pengu
{
    class Program
    {
        static void Main(string[] _)
        {
#if DEBUG 
            const bool debug = true;
#else
            const bool debug = false;
#endif

            using var renderer = new VulkanContext(debug);
            renderer.Run();
        }
    }
}
