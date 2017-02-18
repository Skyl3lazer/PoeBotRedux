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
using System.Data;
using ChatSharp.Events;

namespace PoeBotRedux
{
    class PoeBot
    {
        private static SystemSettings systemSettings;
        private static IrcClient client;
        private static InviteManagemer inviteManager;
        private static string[] admins = { "skyl3lazer" };
        static void Main(string[] args)
        {
            systemSettings = new SystemSettings();
            inviteManager = new InviteManagemer();

            Client();
            Listen();

            client.ConnectAsync();

            while (true) ;
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
                IrcChannel channel = client.Channels[e.PrivateMessage.Source];
                if (e.PrivateMessage.Message == "!help")
                {
                    Help(channel, e);
                }
                else if (e.PrivateMessage.Message.StartsWith("!invite"))
                {
                    Invite(channel, e);
                }
                else if (e.PrivateMessage.Message == "!getinvites")
                {
                    GetInvites(channel, e);
                }
                else if (e.PrivateMessage.Message.StartsWith("!added"))
                {
                    Added(channel, e);
                }
            };
            return true;
        }
        private static void Help(IrcChannel channel, PrivateMessageEventArgs e)
        {
            client.SendMessage("Current Commands", e.PrivateMessage.User.Nick);
            client.SendMessage("!invite <SA_Username> - Request an invite for the given SA user", e.PrivateMessage.User.Nick);
            client.SendMessage("!getinvites - See a list of pending invites", e.PrivateMessage.User.Nick);
            //Admin Commands
            if (e.PrivateMessage.User.User.StartsWith("@") || e.PrivateMessage.User.User.StartsWith("~") || e.PrivateMessage.User.User.StartsWith("%"))
            {
                client.SendMessage("Admin Commands:", e.PrivateMessage.User.Nick);
                client.SendMessage("!added <saname> <charname> - Report that you've invited SA user with the specified charactername", e.PrivateMessage.User.Nick);
            }
        }
        private static void Invite(IrcChannel channel, PrivateMessageEventArgs e)
        {
            string saname = "";
            try
            {
                saname = e.PrivateMessage.Message.Substring(e.PrivateMessage.Message.IndexOf(' '));
                saname = saname.Trim();
                Result res = inviteManager.AddPendingInvite(saname, e.PrivateMessage.User.Nick);
                if (res.GetSuccess())
                {
                    channel.SendMessage("Added " + e.PrivateMessage.User.Nick + " to the invite queue.");
                    Console.WriteLine("Added " + e.PrivateMessage.User.Nick + " to the invite queue.");
                }
                else
                {
                    channel.SendMessage("Failed to add invite: " + res.GetMessage());
                    Console.WriteLine(e.PrivateMessage.User.Nick + " failed to add invite: " + res.GetMessage());
                }

            }
            catch (ArgumentOutOfRangeException ex)
            {
                client.SendMessage("Usage: !invite SomethingAwful_Username", e.PrivateMessage.User.Nick);
                client.SendMessage("Make sure your CHARACTER NAME is in your profile. For example, \"Poe Name: LeagueBlazer\"", e.PrivateMessage.User.Nick);
                client.SendMessage("An inviter will invite you at some point.", e.PrivateMessage.User.Nick);
            }
        }
        private static void GetInvites(IrcChannel channel, PrivateMessageEventArgs e)
        {
            Console.WriteLine(e.PrivateMessage.User.Nick + " requested invites list");
            DataTable dt = inviteManager.GetPendingInvites();
            foreach (DataRow row in dt.Rows)
            {
                client.SendMessage("User " + row["sa_name"].ToString() + " - " + row["date_requested"].ToString() + " - https://forums.somethingawful.com/member.php?action=getinfo&username=" + row["sa_name"].ToString(), e.PrivateMessage.User.Nick);
            }
            if (dt.Rows.Count == 0)
            {
                client.SendMessage("No outstanding invites.", e.PrivateMessage.User.Nick);
            }
            else if (e.PrivateMessage.User.User.StartsWith("@") || e.PrivateMessage.User.User.StartsWith("~") || e.PrivateMessage.User.User.StartsWith("%"))
            {
                client.SendMessage("Additionally, as an inviter you can use !added <SAName> <Charactername> to mark a user as invited.", e.PrivateMessage.User.Nick);
            }
        }
        private static void Added(IrcChannel channel, PrivateMessageEventArgs e)
        {
            if (!e.PrivateMessage.User.User.StartsWith("@") && !e.PrivateMessage.User.User.StartsWith("~") && !e.PrivateMessage.User.User.StartsWith("%"))
            {
                client.SendMessage("This is an inviter only command", e.PrivateMessage.User.Nick);
            }
            else
            {
                client.SendMessage("Until admin checking is fixed, this command is disabled", e.PrivateMessage.User.Nick);
            }
        }
    }
}
