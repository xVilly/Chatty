using Chatty.Filters;
using Chatty.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Chatty.Hubs
{
    public class ChatHub : Hub
    {
        // User Connections mapping
        private readonly static Dictionary<string, List<string>> _connections =
            new Dictionary<string, List<string>>();

        // Channel Connections mapping
        private readonly static Dictionary<string, List<string>> _channelConnections =
            new Dictionary<string, List<string>>();


        public ChatHub()
        {
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.Client(Context.ConnectionId).SendAsync("Status", -1, "Welcome! Please identify using your user token.");
            Console.WriteLine($"Client {Context.ConnectionId} has connected.");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string userName = "none";
            // Remove connection from user map
            foreach (var user in _connections)
            {
                if (user.Value.Contains(Context.ConnectionId))
                {
                    userName = user.Key;
                    _connections[user.Key].Remove(Context.ConnectionId);
                    break;
                }
            }
            // Remove connection from channel map
            foreach (var channel in _channelConnections)
            {
                if (channel.Value.Contains(Context.ConnectionId))
                {
                    _channelConnections[channel.Key].Remove(Context.ConnectionId);
                }
            }
            await base.OnDisconnectedAsync(exception);
            Console.WriteLine($"Client {Context.ConnectionId} has disconnected. (user: {userName})");
        }

        public async Task Identify(string token)
        {
            User? user = null; // ex
            if (user == null)
                await Clients.Client(Context.ConnectionId).SendAsync("Status", 1, "Invalid token.");
            else {
                if (_connections.ContainsKey(token))
                    _connections[token].Add(Context.ConnectionId);
                else
                    _connections.Add(token, new List<string> { Context.ConnectionId });
                await Clients.Client(Context.ConnectionId).SendAsync("Status", -1, $"Successfully identified as {user.Name}.");
                Console.WriteLine($"User '{user.Name}' has identified (CID:{Context.ConnectionId}).");
            }
        }

        public async Task JoinChannel(string channelName)
        {
            User? u = null;
            foreach (var user in _connections)
            {
                if (user.Value.Contains(Context.ConnectionId))
                {
                    u = null; // ex
                    break;
                }
            }
            if (u == null)
            {
                await Clients.Client(Context.ConnectionId).SendAsync("Status", 2, "Invalid session. Identify again.");
                return;
            }
            if (_channelConnections.ContainsKey(channelName))
                _channelConnections[channelName].Add(Context.ConnectionId);
            else
                _channelConnections.Add(channelName, new List<string> { Context.ConnectionId });
            await Groups.AddToGroupAsync(Context.ConnectionId, channelName);
            await Clients.Client(Context.ConnectionId).SendAsync("ReceiveBroadcast", $"You've joined the channel '{channelName}'.");
            await Clients.Group(channelName).SendAsync("ReceiveBroadcast", $"[!] {u.Name} has joined the channel.");
        }

        public async Task LeaveChannel(string channelName)
        {
            User? u = null;
            foreach (var user in _connections)
            {
                if (user.Value.Contains(Context.ConnectionId))
                {
                    u = null; // ex
                    break;
                }
            }
            if (u == null)
            {
                await Clients.Client(Context.ConnectionId).SendAsync("Status", 2, "Invalid session. Identify again.");
                return;
            }
            if (_channelConnections.ContainsKey(channelName))
                _channelConnections[channelName].Remove(Context.ConnectionId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, channelName);
            await Clients.Group(channelName).SendAsync("ReceiveBroadcast", $"[!] {u.Name} has left the channel.");
            await Clients.Client(Context.ConnectionId).SendAsync("ReceiveBroadcast", $"You've left the channel '{channelName}'.");
        }

        public async Task SendMessage(string channel, string message)
        {
            User? u = null;
            foreach (var user in _connections){
                if (user.Value.Contains(Context.ConnectionId))
                {
                    u = null; // ex
                    break;
                }
            }
            if (u == null)
            {
                await Clients.Client(Context.ConnectionId).SendAsync("Status", 2, "Invalid session. Identify again.");
                return;
            }
            await Clients.Group(channel).SendAsync("ReceiveMessage", u.Name, message);
        }
    }
}
