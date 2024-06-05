/*
 * @Author: Boris Semanco <xseman06>
 * @project Sniffer
 * @file ArgParser.cs
 * @description Class that parses command line arguments
 */

namespace Sniffer;

public class ArgParser
{
    public string? interfaceName { get; set; }
    public bool tcp { get; set; }
    public bool udp { get; set; }
    public int? portSource { get; set; }
    public int? portDestination { get; set; }
    public int? port { get; set; }
    public bool arp { get; set; }
    public bool ndp { get; set; }
    public bool icmp4 { get; set; }
    public bool icmp6 { get; set; }
    public bool igmp { get; set; }
    public bool mld { get; set; }
    public int num { get; set; }
    public bool noFilters { get; set; }

    public ArgParser(string[] args)
    {
        if (args.Length == 0)
        {
            PrintHelp();
        }
        // Parse command line arguments
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-i":
                case "--interface":
                    if (i + 1 >= args.Length || args[i + 1].StartsWith("-"))
                    {
                        interfaceName = null;
                        break;
                    }
                    interfaceName = args[++i];
                    break;
                case "-t":
                case "--tcp":
                    tcp = true;
                    noFilters = false;
                    break;
                case "-u":
                case "--udp":
                    udp = true;
                    noFilters = false;
                    break;
                case "-p":
                    if (i + 1 >= args.Length || !int.TryParse(args[i + 1], out _))
                    {
                        Console.Error.WriteLine("Missing argument for source port.");
                        Environment.Exit(1);
                    }

                    port = ushort.Parse(args[++i]);
                    break;
                case "--port-source":
                    if (i + 1 >= args.Length || !int.TryParse(args[i + 1], out _))
                    {
                        Console.Error.WriteLine("Missing argument for source port.");
                        Environment.Exit(1);
                    }

                    portSource = ushort.Parse(args[++i]);
                    break;
                case "--port-destination":
                    if (i + 1 >= args.Length || !int.TryParse(args[i + 1], out _))
                    {
                        Console.Error.WriteLine("Missing argument for destination port.");
                        Environment.Exit(1);
                    }

                    portDestination = ushort.Parse(args[++i]);
                    break;
                case "--arp":
                    arp = true;
                    noFilters = false;
                    break;
                case "--ndp":
                    ndp = true;
                    noFilters = false;
                    break;
                case "--icmp4":
                    icmp4 = true;
                    noFilters = false;
                    break;
                case "--icmp6":
                    icmp6 = true;
                    noFilters = false;
                    break;
                case "--igmp":
                    igmp = true;
                    noFilters = false;
                    break;
                case "--mld":
                    mld = true;
                    noFilters = false;
                    break;
                case "-n":
                    if (i + 1 >= args.Length || !int.TryParse(args[i + 1], out _))
                    {
                        Console.Error.WriteLine("Missing argument for number of packets.");
                        Environment.Exit(1);
                    }

                    num = int.Parse(args[++i]);
                    break;
                default:
                    Console.WriteLine("Invalid argument: " + args[i]);
                    Environment.Exit(1);
                    break;
            }
        }
    }
    
    private void PrintHelp()
    {
        Console.WriteLine("Usage: Sniffer [options]");
        Console.WriteLine("Options:");
        Console.WriteLine("  -i, --interface <interface>  Interface to capture packets from.");
        Console.WriteLine("  -t, --tcp                    Capture only TCP packets.");
        Console.WriteLine("  -u, --udp                    Capture only UDP packets.");
        Console.WriteLine("  -p <port>                    Capture packets with specified port.");
        Console.WriteLine("      --port-source <port>     Capture packets with specified source port.");
        Console.WriteLine("      --port-destination <port> Capture packets with specified destination port.");
        Console.WriteLine("      --arp                    Capture only ARP packets.");
        Console.WriteLine("      --ndp                    Capture only NDP packets.");
        Console.WriteLine("      --icmp4                  Capture only ICMPv4 packets.");
        Console.WriteLine("      --icmp6                  Capture only ICMPv6 packets.");
        Console.WriteLine("      --igmp                   Capture only IGMP packets.");
        Console.WriteLine("      --mld                    Capture only MLD packets.");
        Console.WriteLine("  -n <number>                  Number of packets to capture.");
        Environment.Exit(0);
    }
}