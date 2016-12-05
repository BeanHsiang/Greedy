using System.Configuration;

namespace Greedy.MySqlProxy
{
    static class MySqlProxyConfig
    {
        internal static MySqlProxySectionGroup MySqlProxyGroup;
        internal static ServerSection Server;

        static MySqlProxyConfig()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            MySqlProxyGroup = config.GetSectionGroup("mysqlproxy") as MySqlProxySectionGroup;
            Server = MySqlProxyGroup.Server;
        }
    }

    class MySqlProxySectionGroup : ConfigurationSectionGroup
    {
        [ConfigurationProperty("server")]
        public ServerSection Server { get { return this.Sections["server"] as ServerSection; } }
    }

    class ServerSection : ConfigurationSection
    {
        [ConfigurationProperty("ip")]
        public string IP { get { return this["ip"].ToString(); } }

        [ConfigurationProperty("port")]
        public int Port { get { return (int)this["port"]; } }

        [ConfigurationProperty("backlog")]
        public int Backlog { get { return (int)this["backlog"]; } }
    }
}