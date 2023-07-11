using Chatty.Filters;
using Chatty.Models;
using Chatty.Services;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using System.Xml.Linq;

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

        private readonly UserService _userService;
        public ChatHub(UserService userService)
        {
            _userService = userService;
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.Client(Context.ConnectionId).SendAsync("Status", -1, "Welcome! Please identify using your name and password.");
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

        public async Task Identify(string? name, string? password)
        {
            if (String.IsNullOrEmpty(name) || String.IsNullOrEmpty(password))
            {
                await Clients.Client(Context.ConnectionId).SendAsync("Status", 0, "Name and Password (2) arguments required.");
                return;
            }
            User? user = _userService.GetByName(name);
            if (user == null)
                await Clients.Client(Context.ConnectionId).SendAsync("Status", 1, "User not found.");
            else if (user.Password != password)
                await Clients.Client(Context.ConnectionId).SendAsync("Status", 1, "Wrong password.");
            else {
                if (_connections.ContainsKey(user.Name))
                    _connections[user.Name].Add(Context.ConnectionId);
                else
                    _connections.Add(user.Name, new List<string> { Context.ConnectionId });
                await Clients.Client(Context.ConnectionId).SendAsync("Status", -1, $"Successfully identified as {user.Name}.");
                Console.WriteLine($"User '{user.Name}' has identified (CID:{Context.ConnectionId}).");
            }
        }

        public async Task JoinChannel(string? channelName)
        {
            if (String.IsNullOrEmpty(channelName))
            {
                await Clients.Client(Context.ConnectionId).SendAsync("Status", 0, "Channel name (1) argument required.");
                return;
            }
            User? u = null;
            foreach (var user in _connections)
            {
                if (user.Value.Contains(Context.ConnectionId))
                {
                    u = _userService.GetByName(user.Key);
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

        public async Task LeaveChannel(string? channelName)
        {
            if (String.IsNullOrEmpty(channelName))
            {
                await Clients.Client(Context.ConnectionId).SendAsync("Status", 0, "Channel name (1) argument required.");
                return;
            }
            User? u = null;
            foreach (var user in _connections)
            {
                if (user.Value.Contains(Context.ConnectionId))
                {
                    u = _userService.GetByName(user.Key);
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

        public async Task SendMessage(string? channel, string? message)
        {
            if (String.IsNullOrEmpty(channel) || String.IsNullOrEmpty(message))
            {
                await Clients.Client(Context.ConnectionId).SendAsync("Status", 0, "Channel name and message (2) arguments required.");
                return;
            }
            User? u = null;
            foreach (var user in _connections){
                if (user.Value.Contains(Context.ConnectionId))
                {
                    u = _userService.GetByName(user.Key);
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
