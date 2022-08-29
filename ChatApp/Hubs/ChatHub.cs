using Microsoft.AspNetCore.SignalR;
using ChatApp.Supporters.Constants;

namespace ChatApp.Hubs
{
    public class ChatHub : Hub
    {
        public async Task AddToRespondentGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName.RESPONDENT_GROUP);
        }
        public async Task RemoveFromRespondentGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName.RESPONDENT_GROUP);
        }
        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }
    }
}
