
using System.Threading.Tasks;

namespace ChatClient.Client
{
    public interface IChatClient
    {
        Task Authenticate(string username, string secret, string displayName, FiniteStateMachine fsm);
        Task JoinChannel(string channelId);
        Task SendMessage(Message msg);
        void SetDisplayName(string displayName);
        
        string GetDisplayName();
        
        Task ConnectAsync();

        Task<string> ReceiveMessageAsync();
        
        void Disconnect();
    }
}