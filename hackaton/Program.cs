using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hackaton
{
    class Program
    {
        static void Main(string[] args)
        {
            Logic.hashFile(args[0], "output.json");
        }
    }
}
