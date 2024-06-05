/*
 * @Author: Boris Semanco <xseman06>
 * @project Sniffer
 * @file Program.cs
 * @description Entry point of the program
 */

namespace Sniffer
{
    public class Program
    {
        static void Main(string[] args)
        {
            ArgParser argParser = new ArgParser(args);
            
            Sniffer sniffer = new Sniffer(argParser);
            
            sniffer.StartCapture();
            
        }
    }
}
