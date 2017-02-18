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

            DataTable allUsers = allUsersAdapter.GetDataBy(ircname, saname);
            if (allUsers.Rows.Count > 0)
            {
                res = new Result(false, "There is already a user registered under this SA/IRC Name. SA: " + allUsers.Rows[0]["sa_name"].ToString() + " IRC: " + allUsers.Rows[0]["irc_name"].ToString());
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
        public DataTable GetPendingInvites()
        {
            return pendingInvitesDataSet.PendingInvites;
        }
    }
}
