using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using ChatSharp;

namespace PoeBotRedux
{
    class PoeBot
    {
        private static SystemSettings systemSettings;
        private static IrcClient client;

        static void Main(string[] args)
        {
            systemSettings = new SystemSettings();

            Client();
            Listen();

            client.ConnectAsync();

            while (true);
        }
        private static bool Client()
        {
            client = new IrcClient(systemSettings.getServerName(), new IrcUser(systemSettings.getNick(), systemSettings.getUserName()));

            client.ConnectionComplete += (s, e) => client.JoinChannel(systemSettings.getChannelName());

            return true;
        }
        private static bool Listen()
        {
            client.ChannelMessageRecieved += (s, e) =>
            {
                var channel = client.Channels[e.PrivateMessage.Source];

                if (e.PrivateMessage.Message == ".list")
                    channel.SendMessage(string.Join(", ", channel.Users.Select(u => u.Nick)));
            };
            return true;
        }
    }
}
