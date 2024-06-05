using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Client
{
    public class UDPChatClient : IChatClient
    {
        private readonly string serverAddress;
        private readonly int serverPort;
        private string displayName;
        private UdpClient udpClient;
        private IPEndPoint remoteEndPoint;

        private ushort udpTimeout;
        private byte udpRetransmissions;

        public UDPChatClient(string serverAddress, int serverPort, ushort udpTimeout, byte udpRetransmissions)
        {
            this.serverAddress = serverAddress;
            this.serverPort = serverPort;
            this.udpTimeout = udpTimeout;
            this.udpRetransmissions = udpRetransmissions;
        }

        public async Task Authenticate(string username, string secret, string displayName, FiniteStateMachine fsm)
        {
            await ConnectAsync();
            await SendMessage(new Message("AUTH", username: username, secret: secret, displayName: displayName));
            string reply = await ReceiveMessageAsync();
            if (reply.StartsWith("REPLY OK"))
            {
                this.displayName = displayName;
                fsm.TransitionTo(FiniteStateMachine.State.Open);

                // Task to continuously read messages from the server
                Task messageTask = Task.Run(async () =>
                {
                    while (true)
                    {
                        try
                        {
                            string serverMessage = await ReceiveMessageAsync();
                            Message message = Message.ParseMessage(serverMessage);
                            message.PrintMessage();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"ERR: {ex.Message}");
                            fsm.TransitionTo(FiniteStateMachine.State.Error);
                        }
                    }
                });
            }
            else if (reply.Contains("REPLY NOK"))
            {
                Console.WriteLine("Failure: Authentication failed.");
            }
            else if (reply.Contains("ERR"))
            {
                Console.WriteLine($"ERR FROM {displayName}: {reply}");
            }
        }

        public async Task JoinChannel(string channelId)
        {
            await SendMessage(new Message("JOIN", displayName: displayName, channelID: channelId));
            string reply = await ReceiveMessageAsync();
            if (reply.Contains("REPLY OK"))
            {
                Console.WriteLine($"Success: {displayName} has joined {channelId}.");
            }
            else
            {
                Console.WriteLine($"Failure: Unable to join {channelId}.");
            }
        }

        public async Task SendMessage(Message msg)
        {
            byte[] data = Encoding.UTF8.GetBytes(msg.GetMessage());
            await udpClient.SendAsync(data, data.Length, remoteEndPoint);
        }

        public void SetDisplayName(string displayName)
        {
            Console.WriteLine($"Setting display name to {displayName}");
            this.displayName = displayName;
        }

        public string GetDisplayName()
        {
            return displayName;
        }

        public async Task ConnectAsync()
        {
            Console.WriteLine($"Connecting to server at {serverAddress}:{serverPort}");
            udpClient = new UdpClient();
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(serverAddress), serverPort);
            udpClient.Client.ReceiveTimeout = udpTimeout;
            udpClient.Client.SendTimeout = udpTimeout;
        }

        public async Task<string> ReceiveMessageAsync()
        {
            try
            {
                // Receive message from server
                UdpReceiveResult result = await udpClient.ReceiveAsync();
                string message = Encoding.ASCII.GetString(result.Buffer);
                // Console.WriteLine($"Decoded message: {message}");
                return message;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERR: {ex.Message}");
                return string.Empty;
            }
        }


        public void Disconnect()
        {
            Console.WriteLine("Disconnecting from server.");
            udpClient.Close();
        }
    }
}
