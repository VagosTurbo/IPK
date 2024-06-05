using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Net.Sockets;
using ChatClient.Client;

namespace ChatClient
{
    class Program
    {
        // Ctrl+C handler
        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e, FiniteStateMachine fsm)
        {
            // Check if the Ctrl+C key combination was pressed
            if (e.SpecialKey == ConsoleSpecialKey.ControlC)
            {
                fsm.TransitionTo(FiniteStateMachine.State.End);
            }
        }
        static async Task Main(string[] args)
        {
            if (args.Length < 1)
            {
                PrintHelp();
                System.Environment.Exit(1);
            }
            
            // Parse command-line arguments
            if (args.Length == 0 || args[0] == "-h")
            {
                PrintHelp();
                return;
            }

            string transportProtocol = "tcp";
            string serverAddress = "";
            ushort serverPort = 4567; // Default port
            ushort udpTimeout = 250; // Default UDP timeout
            byte udpRetransmissions = 3; // Default UDP retransmissions

            for (int i = 0; i < args.Length; i += 2)
            {
                string arg = args[i];
                string value = args[i + 1];

                switch (arg)
                {
                    case "-t":
                        transportProtocol = value.ToLower();
                        break;
                    case "-s":
                        serverAddress = value;
                        break;
                    case "-p":
                        ushort.TryParse(value, out serverPort);
                        break;
                    case "-d":
                        ushort.TryParse(value, out udpTimeout);
                        break;
                    case "-r":
                        byte.TryParse(value, out udpRetransmissions);
                        break;
                    case "-h":
                        PrintHelp();
                        return;
                    default:
                        Console.WriteLine($"Unknown argument: {arg}");
                        PrintHelp();
                        return;
                }
            }

            // Initialize chat client based on transport protocol
            Client.IChatClient chatClient = null;
            if (transportProtocol == "tcp")
            {
                chatClient = new TCPChatClient(serverAddress, serverPort);
            }
            else if (transportProtocol == "udp")
            {
                chatClient = new UDPChatClient(serverAddress, serverPort, udpTimeout, udpRetransmissions);
            }
            else
            {
                Console.WriteLine("Invalid transport protocol specified.");
                PrintHelp();
                return;
            }

            FiniteStateMachine fsm = new FiniteStateMachine();
            fsm.TransitionTo(FiniteStateMachine.State.Auth);

            await chatClient.ConnectAsync();
            
            Console.CancelKeyPress += (sender, e) => Console_CancelKeyPress(sender, e, fsm);
            
            // Main loop for interacting with the chat client
            while (true)
            {
                if (fsm.GetState() == FiniteStateMachine.State.End)
                {
                    break;
                }
                
                // Read user input
                string input = Console.ReadLine();
                
                // Ignore empty input
                if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }

                else if (input.StartsWith("/"))
                {
                    // Process local command
                    ProcessLocalCommand(input, chatClient, fsm);
                }
                else if (fsm.GetState() == FiniteStateMachine.State.Auth)
                {
                    Console.WriteLine("ERR: You need to authenticate first. Use /auth command.");
                    System.Environment.Exit(1);
                }
                else if (fsm.GetState() == FiniteStateMachine.State.Open)
                {
                    // Send message to server
                   await  chatClient.SendMessage(new Message("MSG", displayName: chatClient.GetDisplayName(), messageContent: input));
                }
            }
            
            // Disconnect from the server
            await chatClient.SendMessage(new Message("BYE"));
            chatClient.Disconnect();
            Console.WriteLine("Disconnected from the server.");
        }

        static void PrintHelp() 
        {
            Console.WriteLine("Usage: ipk24chat-client -t <tcp|udp> -s <server_address> -p <server_port> -d <udp_timeout> -r <udp_retransmissions>");
            Console.WriteLine("Arguments:");
            Console.WriteLine("  -t <tcp|udp>: Transport protocol (default: tcp)");
            Console.WriteLine("  -s <server_address>: Server IP address or hostname (mandatory)");
            Console.WriteLine("  -p <server_port>: Server port (default: 4567)");
            Console.WriteLine("  -d <udp_timeout>: UDP confirmation timeout (default: 250)");
            Console.WriteLine("  -r <udp_retransmissions>: Maximum number of UDP retransmissions (default: 3)");
            Console.WriteLine("  -h: Print this help message");
        }

        static void ProcessLocalCommand(string input, Client.IChatClient chatClient, FiniteStateMachine fsm)
        {
            // Process local commands
            string[] parts = input.Split(' ', 2);
            string command = parts[0].ToLower();
            string parameters = parts.Length > 1 ? parts[1] : "";

            switch (command)
            {
                case "/auth":
                    if (fsm.GetState() == FiniteStateMachine.State.Auth)
                    {
                        string[] authParams = parameters.Split(' ');
                        if (authParams.Length == 3)
                        {
                            string username = authParams[0];
                            string secret = authParams[1];
                            string displayName = authParams[2];
                            chatClient.Authenticate(username, secret, displayName, fsm);
                        }
                        else
                        {
                            Console.WriteLine("Invalid /auth command format. Usage: /auth <username> <secret> <display_name>");
                        }
                    }
                    else
                    {
                        Console.WriteLine("You are already authorized");
                    }
                    break;
                case "/join":
                    if (!string.IsNullOrWhiteSpace(parameters))
                    {
                        chatClient.JoinChannel(parameters);
                    }
                    else
                    {
                        Console.WriteLine("Invalid /join command format. Usage: /join <channel_id>");
                    }
                    break;
                case "/rename":
                    if (!string.IsNullOrWhiteSpace(parameters))
                    {
                        chatClient.SetDisplayName(parameters);
                    }
                    else
                    {
                        Console.WriteLine("Invalid /rename command format. Usage: /rename <display_name>");
                    }
                    break;
                case "/help":
                    // Print help message for local commands
                    Console.WriteLine("Supported local commands:");
                    Console.WriteLine("  /auth <username> <secret> <display_name>: Authenticate with the server");
                    Console.WriteLine("  /join <channel_id>: Join a channel");
                    Console.WriteLine("  /rename <display_name>: Change display name");
                    Console.WriteLine("  /help: Print this help message");
                    break;
                default:
                    Console.WriteLine($"Unknown command: {command}. Type /help for a list of supported commands.");
                    break;
            }
        }
    }
}
