// using System;
// using System.IO;
using System.Net.Sockets;
// using System.Text;
using System.Threading.Tasks;


namespace ChatClient.Client
{
    public class TCPChatClient : IChatClient
    {
        private readonly string serverAddress;
        private readonly int serverPort;
        private string displayName;
        private TcpClient client;
        private StreamReader reader;
        private StreamWriter writer;
        public TCPChatClient(string serverAddress, int serverPort)
        {
            this.serverAddress = serverAddress;
            this.serverPort = serverPort;
        }

        public async Task Authenticate(string username, string secret, string displayName, FiniteStateMachine fsm)
        {
            await ConnectAsync();
            await SendMessage(new Message("AUTH", username: username, secret: secret, displayName: displayName));
            string reply = await ReceiveMessageAsync();
            Message.ParseMessage(reply).PrintMessage();
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
                            if (message.Type == "BYE")
                            {
                                fsm.TransitionTo(FiniteStateMachine.State.End);
                                break;
                            }

                            message.PrintMessage();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error while receiving message: {ex.Message}");
                            fsm.TransitionTo(FiniteStateMachine.State.Error);
                            // Optionally, you can handle the exception or log it
                        }
                    }
                });
            }
            else if(reply.Contains("REPLY NOK"))
            {
                Console.WriteLine("Failure: Authentication failed.");
            }
            else if (reply.Contains("ERR"))
            {
                Message message = Message.ParseMessage(reply);
                message.PrintMessage();
                fsm.TransitionTo(FiniteStateMachine.State.End);
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
            await writer.WriteLineAsync(msg.GetMessage());
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
            client = new TcpClient();
            await client.ConnectAsync(serverAddress, serverPort);
            Console.WriteLine("Connected to server.");
            NetworkStream stream = client.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream) { AutoFlush = true };
        }

        public async Task<string> ReceiveMessageAsync()
        {
            if (reader != null)
            {
                return await reader.ReadLineAsync();
            }
            else
            {
                // If the reader is null
                Console.WriteLine("Error: StreamReader is not initialized.");
                return string.Empty;
            }
        }

        
        public void Disconnect()
        {
            Console.WriteLine("Disconnecting from server.");
            client.Close();
        }
    }
}
