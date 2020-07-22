using Pengu.Renderer;
using Pengu.Support;
using Pengu.VirtualMachine;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pengu
{
    class Program
    {
        static async Task Main(string[] _)
        {
#if DEBUG 
            const bool debug = true;
#else
            const bool debug = false;
#endif

            await Exercises.ReadExercises().ConfigureAwait(true);

            using var renderer = new VulkanContext(debug);
            renderer.Run();
        }
    }
}
