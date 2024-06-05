
/*
 * @Author: Boris Semanco <xseman06>
 * @project Sniffer
 * @file Sniffer.cs
 * @description Sniffer class that handles packet capturing and filtering
 */

using PacketDotNet;
using SharpPcap;
using System.Globalization;
using System.Net;
using System.Text;


namespace Sniffer;


    public class Sniffer
    {
        private int _packetNum;
        private ArgParser _argParser;
        private ICaptureDevice? device;
        public Sniffer(ArgParser argParser)
        {
            this._argParser = argParser;
            this._packetNum = 0;
            this.device = null;
        }
        
        public void StartCapture()
        {
            GetInterface();

            if (device != null)
            {
                // Open the device for capturing
                device.OnPacketArrival += (sender, e) => SniffPackets(sender, e);
                device.Open(DeviceModes.Promiscuous);

                device.StartCapture();

                // Wait until capturing is stopped
                while (device.Started)
                {
                    Thread.Sleep(10);
                }
            }
            else
            {
                Console.WriteLine("No interface selected.");
                Environment.Exit(1);
            }
        }
        
        private void GetInterface()
        {
            CaptureDeviceList devices = CaptureDeviceList.Instance;
            if (devices.Count < 1)
            {
                Console.WriteLine("No network devices found.");
                Environment.Exit(1);
            }
            
            // If no interface name is provided, list all interfaces
            if (string.IsNullOrEmpty(_argParser.interfaceName))
            {
                foreach (var dev in devices)
                {
                    Console.WriteLine(dev.Name);
                }
                Environment.Exit(0);
            }
            
            // find the interface in the list
            foreach (var dev in devices)
            {
                if (dev.Name == _argParser.interfaceName)
                {
                    device = dev;
                    break;
                }
            }
            
            // if the interface was not found
            if (device == null)
            {
                Console.WriteLine("Interface not found: " + _argParser.interfaceName);
                Environment.Exit(1);
            }
        }
        
        public void SniffPackets(object sender, PacketCapture e)
        {
            if (e.GetPacket().LinkLayerType != PacketDotNet.LinkLayers.Ethernet)
            {
                Console.WriteLine("Only Ethernet packets are supported.");
                return;
            }
            
            var packet = Packet.ParsePacket(e.GetPacket().LinkLayerType, e.GetPacket().Data);
            
            var ethernetPacket = packet.Extract<EthernetPacket>();
            var ipPacket = packet.Extract<IPPacket>();
            var tcpPacket = packet.Extract<TcpPacket>();
            var udpPacket = packet.Extract<UdpPacket>();
            var arpPacket = packet.Extract<ArpPacket>();
            var icmp4Packet = packet.Extract<IcmpV4Packet>();
            var icmp6Packet = packet.Extract<IcmpV6Packet>();
            var ndpPacket = packet.Extract<NdpPacket>();
            
            // Skip non ethernet packets
            if(ethernetPacket == null)
            {
                return;
            }
            
            // get general packet information
            var timestamp = e.GetPacket().Timeval.Date.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");
            var frameLength = e.GetPacket().Data.Length;
            string srcMac = ethernetPacket.SourceHardwareAddress.ToString();
            string dstMac = ethernetPacket.DestinationHardwareAddress.ToString();
            IPAddress srcIP = IPAddress.None;
            IPAddress dstIP = IPAddress.None;
            ushort? srcPort = null;
            ushort? dstPort = null;
            ProtocolType? protocol = null;
            
            if (ipPacket != null)
            {
                srcIP = ipPacket.SourceAddress;
                dstIP = ipPacket.DestinationAddress;
                protocol = ipPacket.Protocol;
            }
            
            
            if(protocol == null)
            {
                return;
            }

            switch (protocol)
                {
                    case ProtocolType.Tcp:
                        // check if we want to capture TCP packets
                        if (tcpPacket != null && (_argParser.tcp || _argParser.noFilters))
                        {
                            srcPort = tcpPacket.SourcePort;
                            dstPort = tcpPacket.DestinationPort;
                            
                            // check port filters
                            if (!PortsFilter(srcPort, dstPort))
                            {
                                return;
                            }
                            
                        }
                        break;
                    case ProtocolType.Udp:
                        // check if we want to capture UDP packets
                        if (udpPacket != null && (_argParser.udp || _argParser.noFilters))
                        {
                            srcPort = udpPacket.SourcePort;
                            dstPort = udpPacket.DestinationPort;
                            
                            // check port filters
                            if (!PortsFilter(srcPort, dstPort))
                            {
                                return;
                            }
                        }
                        break;
                    case ProtocolType.Icmp:
                        if(icmp4Packet != null && (_argParser.icmp4 || _argParser.noFilters))
                        {
                            break;
                        }
                        return;
                    case ProtocolType.IcmpV6:
                        if(icmp6Packet != null && (_argParser.icmp6 || _argParser.noFilters))
                        {
                            break;
                        }
                        return;
                    default:
                        return;
                }
            
            
            
            PrintPacket(timestamp, srcMac, dstMac, srcIP.ToString(), dstIP.ToString(), srcPort, dstPort, frameLength);
            Console.WriteLine();
            Console.Write(FormatAsHex(e.GetPacket().Data));
            if(++_packetNum >= _argParser.num && device != null)
            {
                device.StopCapture();
                device.Close();
                Environment.Exit(0);
            }
            Console.WriteLine();
        }
        
        private void PrintPacket(string timestamp, string srcMac, string dstMac, string srcIP, string dstIP, ushort? srcPort, ushort? dstPort, int frameLength)
        {
            Console.WriteLine("timestamp: " + timestamp);
            Console.WriteLine("src MAC: " + srcMac);
            Console.WriteLine("dst MAC: " + dstMac);
            Console.WriteLine("frame length: " + frameLength + " bytes");
            if (srcPort != null || dstPort != null){
                Console.WriteLine("src IP: " + srcIP);
                Console.WriteLine("dst IP: " + dstIP);
            }
            Console.WriteLine("src port: " + srcPort);
            Console.WriteLine("dst port: " + dstPort);
        }
        
        
        // https://stackoverflow.com/a/52580931
        private static string FormatAsHex(ReadOnlySpan<byte> data)
        {
            byte ReplaceControlCharacterWithDot(byte character) 
                => character < 31 || character >= 127 ? (byte)46 /* dot */ : character;
            byte[] ReplaceControlCharactersWithDots(byte[] characters) 
                => characters.Select(ReplaceControlCharacterWithDot).ToArray();

            var result = new StringBuilder();
            const int lineWidth = 16;
            for (var pos = 0; pos < data.Length;)
            {
                var line = data.Slice(pos, Math.Min(lineWidth, data.Length - pos)).ToArray();
                var asHex = string.Join(" ", line.Select(v => v.ToString("X2", CultureInfo.InvariantCulture)));
                asHex += new string(' ', lineWidth * 3 - 1 - asHex.Length);
                var asCharacters = Encoding.ASCII.GetString(ReplaceControlCharactersWithDots(line));
                result.Append(FormattableString.Invariant($"{pos:X4} {asHex} {asCharacters}\n"));
                pos += line.Length;
            }
            return result.ToString();
        }

        private bool PortsFilter(ushort? srcPort, ushort? dstPort)
        {
            if(_argParser.port != null && (srcPort != _argParser.port && dstPort != _argParser.port))
            {
                return false;
            }
            if(_argParser.portSource != null && srcPort != _argParser.portSource)
            {
                return false;
            }
            if(_argParser.portDestination != null && dstPort != _argParser.portDestination)
            {
                return false;
            }
            return true;
        }
    }
        

    