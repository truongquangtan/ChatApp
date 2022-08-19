using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Hubs
{
    public class ChatHub : Hub
    {
        public async Task AddToRespondentGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Respondent");
        }
        public async Task RemoveFromRespondentGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Respondent");
        }
    }
}
