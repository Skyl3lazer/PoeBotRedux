using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoeBotRedux
{
    class SystemSettings
    {
        private data_System_Parameters systemParmsDataSet;
        private data_System_ParametersTableAdapters.System_ParametersTableAdapter systemParmsAdapter;

        public SystemSettings()
        {
            LoadData();
        }
        private void LoadData()
        {
            systemParmsDataSet = new data_System_Parameters();
            systemParmsAdapter = new data_System_ParametersTableAdapters.System_ParametersTableAdapter();

            systemParmsAdapter.Fill(systemParmsDataSet.System_Parameters);
        }
        public string getServerName()
        {
            return systemParmsAdapter.GetServerName();
        }
        public string getUserName()
        {
            return systemParmsAdapter.GetUserName();
        }
        public string getUserPassword()
        {
            return systemParmsAdapter.GetUserPassword();
        }
        public string getChannelName()
        {
            return systemParmsAdapter.GetChannelName();
        }
        public string getNick()
        {
            return systemParmsAdapter.GetNick();
        }
        public int getPort()
        {
            try
            {
                return int.Parse(systemParmsAdapter.GetPort());
            }
            catch (Exception e)
            {
                throw new Exception("Could not retreive server port", e);
            }
        }
    }
}
