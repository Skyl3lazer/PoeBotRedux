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
using System.Collections;
using System.Text.RegularExpressions;

namespace PoeBotRedux
{
    class PoeBot
    {
        private static SystemSettings systemSettings;
        private static IrcClient client;
        private static InviteManagemer inviteManager;
        private static List<IrcUser> admins;
        static void Main(string[] args)
        {
            systemSettings = new SystemSettings();
            inviteManager = new InviteManagemer();

            Client();
            Listen();
            Connect();
            
            while (true) ;
        }
        private static bool Client()
        {
            client = new IrcClient(systemSettings.getServerName(), new IrcUser(systemSettings.getNick(), systemSettings.getUserName(), systemSettings.getUserPassword()));
            admins = new List<IrcUser>();

            client.ConnectionComplete += (s, e) => { client.JoinChannel(systemSettings.getChannelName());  };
            client.ChannelTopicReceived += (s, e) => { client.Channels[0].UsersByMode = new Dictionary<char?, UserPoolView>(); };
            client.NetworkError += (s, e) => { Connect();  };
            client.NickInUse += (s, e) => { GhostNick(); };
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
                else if (e.PrivateMessage.Message == "!getinvites" || e.PrivateMessage.Message == "!queue")
                {
                    GetInvites(channel, e);
                }
                else if (e.PrivateMessage.Message.StartsWith("!added"))
                {
                    Added(channel, e);
                }
                else if (e.PrivateMessage.Message.StartsWith("!check"))
                {
                    CheckAdmin(channel, e);
                }
            };
            return true;
        }
        private static void Connect()
        {
            client.ConnectAsync();
        }
        private static void GhostNick()
        {
            client.SendIrcMessage(new IrcMessage("/ns ghost " + systemSettings.getNick() + " " + systemSettings.getUserPassword()));
            Thread.Sleep(5000);
            client.SendRawMessage("PASS {0}", systemSettings.getUserPassword());
            client.SendRawMessage("NICK {0}", systemSettings.getNick());
        }
        private static void Help(IrcChannel channel, PrivateMessageEventArgs e)
        {
            client.SendNotice("Current Commands", e.PrivateMessage.User.Nick);
            client.SendNotice("!invite <SA_Username> - Request an invite for the given SA user", e.PrivateMessage.User.Nick);
            client.SendNotice("!getinvites - See a list of pending invites", e.PrivateMessage.User.Nick);
            //Admin Commands
            if (IsAdmin(channel, e.PrivateMessage.User))
            {
                client.SendNotice("Admin Commands:", e.PrivateMessage.User.Nick);
                client.SendNotice("!added <saname> <charname> - Report that you've invited SA user with the specified charactername", e.PrivateMessage.User.Nick);
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
                    channel.SendMessage("Added " + e.PrivateMessage.User.Nick + " to the invite queue. Make sure your CHARACTER NAME is on your SA Profile.");
                    Console.WriteLine("Added " + e.PrivateMessage.User.Nick + " to the invite queue.");
                }
                else
                {
                    Console.WriteLine(e.PrivateMessage.User.Nick + " failed to add invite: " + res.GetMessage());
                    channel.SendMessage("Failed to add invite: " + res.GetMessage());
                }

            }
            catch (ArgumentOutOfRangeException ex)
            {
                client.SendNotice("Usage: !invite SomethingAwful_Username", e.PrivateMessage.User.Nick);
                client.SendNotice("Make sure your CHARACTER NAME is in your profile. For example, \"Poe Name: LeagueBlazer\"", e.PrivateMessage.User.Nick);
                client.SendNotice("An inviter will invite you at some point.", e.PrivateMessage.User.Nick);
            }
        }
        private static void GetInvites(IrcChannel channel, PrivateMessageEventArgs e)
        {
            Console.WriteLine(e.PrivateMessage.User.Nick + " requested invites list");

            DataTable dt = inviteManager.GetPendingInvites();
            client.SendNotice("Pending Invites:", e.PrivateMessage.User.Nick);
            foreach (DataRow row in dt.Rows)
            {
                client.SendNotice("User " + row["sa_name"].ToString() + "[irc:"+row["irc_name"].ToString()+"] - " + row["date_requested"].ToString() + " - https://forums.somethingawful.com/member.php?action=getinfo&username=" + row["sa_name"].ToString(), e.PrivateMessage.User.Nick);
            }
            if (dt.Rows.Count == 0)
            {
                client.SendNotice("No outstanding invites.", e.PrivateMessage.User.Nick);
            }
            else
            {
                if (IsAdmin(channel, e.PrivateMessage.User))
                {
                    client.SendNotice("Additionally, as an inviter you can use !added <SAName> <Charactername> to mark a user as invited.", e.PrivateMessage.User.Nick);
                }
            }
        }
        private static void Added(IrcChannel channel, PrivateMessageEventArgs e)
        {
            if (!IsAdmin(channel, e.PrivateMessage.User))
            {
                client.SendNotice("This is an inviter only command", e.PrivateMessage.User.Nick);
            }
            else
            {
                string[] command = Regex.Split(e.PrivateMessage.Message, " ");
                if (command.Length != 3)
                {
                    Console.WriteLine(e.PrivateMessage.User.Nick + " had a malformed !added command");
                    client.SendNotice("USAGE: !added <SA_name> <character_name> - Registers that you are inviting that given SA name on the given char name", e.PrivateMessage.User.Nick);
                }
                else
                {
                    try
                    {
                        Result res = inviteManager.ApprovedInvite(command[1], command[2]);
                        if (res.GetSuccess())
                        {
                            channel.SendMessage("Confirmed " + command[1] + " invited to guild.");
                            Console.WriteLine("Confirmed " + command[1] + " invited to guild.");
                        }
                        else
                        {
                            Console.WriteLine(command[1] + " failed to update invite status: " + res.GetMessage());
                            channel.SendMessage("Failed to update invite status: " + res.GetMessage());
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Unable to update DB on add");
                    }
                }
            }
        }
        private static void CheckAdmin(IrcChannel channel, PrivateMessageEventArgs e)
        {
            channel.SendMessage(IsAdmin(channel, e.PrivateMessage.User).ToString());
        }
        private static bool IsAdmin(IrcChannel c, IrcUser u)
        {
            if (c.UsersByMode.ContainsKey('o'))
            {
                if(c.UsersByMode['o'].Contains(u.Nick))
                    return true;
            }
            if (c.UsersByMode.ContainsKey('h'))
            {
                if (c.UsersByMode['h'].Contains(u.Nick))
                    return true;
            }
            if (u.ChannelModes.ContainsKey(c) &&(u.ChannelModes[c] != 'o' || u.ChannelModes[c] != 'h'))
                return true;
            return false;
        }
    }
}
