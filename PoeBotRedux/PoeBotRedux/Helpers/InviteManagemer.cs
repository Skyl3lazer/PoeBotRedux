using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoeBotRedux
{
    class InviteManagemer
    {
        private data_Pending_Invites pendingInvitesDataSet;
        private data_Pending_InvitesTableAdapters.PendingInvitesTableAdapter pendingInvitesAdapter;
        private data_All_Users allUsersDataSet;
        private data_All_UsersTableAdapters.AllUsersTableAdapter allUsersAdapter;
        public InviteManagemer()
        {
            LoadPendingInvites();
            LoadAllUsers();
        }
        private void LoadPendingInvites()
        {
            pendingInvitesDataSet = new data_Pending_Invites();
            pendingInvitesAdapter = new data_Pending_InvitesTableAdapters.PendingInvitesTableAdapter();

            pendingInvitesAdapter.Fill(pendingInvitesDataSet.PendingInvites);
        }
        private void LoadAllUsers()
        {
            allUsersDataSet = new data_All_Users();
            allUsersAdapter = new data_All_UsersTableAdapters.AllUsersTableAdapter();

            allUsersAdapter.Fill(allUsersDataSet.AllUsers);
        }
        public Result AddPendingInvite(string saname, string ircname)
        {
            Result res;

            LoadAllUsers();

            DataTable allUsers = allUsersAdapter.GetDataBy(ircname, saname);
            if (allUsers.Rows.Count > 0 && allUsers.Rows[0]["invited"].ToString() == "False")
            {
                res = new Result(false, "There is already a user registered under this SA/IRC Name. SA: " + allUsers.Rows[0]["sa_name"].ToString() + " IRC: " + allUsers.Rows[0]["irc_name"].ToString());
                return res;
            }
            //If user was previously invited but needs a new invite
            else if (allUsers.Rows.Count > 0 && allUsers.Rows[0]["invited"].ToString() == "True")
            {
                int row = int.Parse(allUsers.Rows[0]["invite_id"].ToString()) - 1;
                allUsersDataSet.AllUsers.Rows[row]["invited"] = 0;
                allUsersDataSet.AllUsers.Rows[row]["date_requested"] = System.DateTime.Now;
                allUsersDataSet.AllUsers.Rows[row]["sa_name"] = saname;
                try
                {
                    allUsersAdapter.Update(allUsersDataSet);
                    res = new Result(true);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    res = new Result(false, e.Message);
                    throw new Exception("Could not update database", e);
                }

                return res;

            }

            DataRow newPendingInviteRow = pendingInvitesDataSet.PendingInvites.NewRow();
            newPendingInviteRow["irc_name"] = ircname;
            newPendingInviteRow["sa_name"] = saname;
            newPendingInviteRow["date_requested"] = System.DateTime.Now;
            pendingInvitesDataSet.PendingInvites.Rows.Add(newPendingInviteRow);

            try
            {
                pendingInvitesAdapter.Update(pendingInvitesDataSet);
                res = new Result(true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                res = new Result(false, e.Message);
                throw new Exception("Could not update database", e);
            }

            return res;
        }
        public Result ApprovedInvite(string saname, string charname)
        {
            Result res;

            LoadPendingInvites();
            LoadAllUsers();

            DataTable pendingInvites = pendingInvitesAdapter.GetDataBySAName(saname);
            if (pendingInvites.Rows.Count == 0)
            {
                res = new Result(false, "There is no pending invite for SA user " + saname);
                return res;
            }

            int pk = int.Parse(allUsersDataSet.AllUsers.Where(a => a.sa_name == saname).First()["invite_id"].ToString()) - 1;
            allUsersDataSet.AllUsers.Rows[pk]["character_name"] = charname;
            allUsersDataSet.AllUsers.Rows[pk]["invited"] = 1;
            try
            {
                allUsersAdapter.Update(allUsersDataSet);
                res = new Result(true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                res = new Result(false, e.Message);
                throw new Exception("Could not update database", e);
            }

            return res;

        }
        public DataTable GetPendingInvites()
        {
            LoadPendingInvites();
            return pendingInvitesDataSet.PendingInvites;
        }
    }
}
