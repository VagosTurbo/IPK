namespace ChatClient
{
    public class Message
    {
        public string Type { get; set; }
        public string DisplayName { get; set; }
        public string MessageContent { get; set; }
        public string Username { get; set; }
        public string Secret { get; set; }
        public string ChannelID { get; set; }

        public Message(string type, string displayName = null, string messageContent = null, string username = null, string secret = null, string channelID = null)
        {
            Type = type;
            DisplayName = displayName;
            MessageContent = messageContent;
            Username = username;
            Secret = secret;
            ChannelID = channelID;
        }

        // static method to create a new instance of Message from raw server message
        public static Message ParseMessage(string fullMessage)
        {
            
            if (string.IsNullOrWhiteSpace(fullMessage))
            {
                return null;
            }
            
            // Initialize variables to store message properties
            string displayName = null;
            string messageContent = null;
            string username = null;
            string secret = null;
            string channelID = null;
            
            string[] parts = fullMessage.Split(' ');

            string type = parts[0];


            // Extract other properties based on message type
            switch (type)
            {
                case "ERR":
                    displayName = parts[2];
                    messageContent = JoinMessageContent(parts, 4);
                    break;
                case "AUTH":
                    username = parts[1];
                    displayName = parts[3];
                    secret = parts[5];
                    break;
                case "JOIN":
                    channelID = parts[1];
                    displayName = parts[3];
                    break;
                case "MSG":
                    displayName = parts[2];
                    messageContent = JoinMessageContent(parts, 4);
                    break;
                case "REPLY":
                    if (parts[1] == "OK")
                    {
                        type = "OK";
                        messageContent = JoinMessageContent(parts, 3);
                    }
                    else if (parts[1] == "NOK")
                    {
                        type = "NOK";
                        messageContent = JoinMessageContent(parts, 3);
                    }
                    break;
                case "BYE":
                    type = "BYE";
                    break;
                default:
                    break;
            }

            // Create and return a new instance of Message
            return new Message(type, displayName, messageContent, username, secret, channelID);
        }

        // Method to extract message content from the given index to the end of the string
        private static string JoinMessageContent(string[] parts, int startIndex)
        {
            return string.Join(" ", parts[startIndex..]);
        }
        
        public string GetMessage()
        {
            switch (Type)
            {
                case "ERR":
                    return $"ERR FROM {DisplayName} IS {MessageContent}\r";
                case "AUTH":
                    return $"AUTH {Username} AS {DisplayName} USING {Secret}\r";
                case "JOIN":
                    return $"JOIN {ChannelID} AS {DisplayName}\r";
                case "MSG":
                    return $"MSG FROM {DisplayName} IS {MessageContent}\r";
                case "BYE\r":
                    return "BYE\r";
                default:
                    return string.Empty;
            }
        }
        
        public void PrintMessage()
        {
            switch (Type)
            {
                case "ERR":
                    Console.Error.WriteLine($"ERR FROM {DisplayName}: {MessageContent}");
                    break;
                case "AUTH":
                    Console.WriteLine($"Authenticating with TCP: Username: {Username}, Display Name: {DisplayName}, Secret: {Secret}");
                    break;
                case "JOIN":
                    Console.WriteLine($"Joining channel {ChannelID} with TCP");
                    break;
                case "MSG":
                    Console.WriteLine($"{DisplayName}: {MessageContent}");
                    break;
                case "BYE":
                    Console.WriteLine("Disconnecting with TCP");
                    break;
                case "OK":
                    Console.Error.WriteLine($"Success: {MessageContent}");
                    break;
                case "NOK":
                    Console.Error.WriteLine($"Failure: {MessageContent}");
                    break;
                default:
                    Console.WriteLine("Invalid message type");
                    break;
            }
        }
    }
}
