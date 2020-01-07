using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml;
using Tabloid.Classes.Data;

namespace TabloidCenter.Classes
{
    public class SiteConfig
    {

        private string passphrase = "emfklvkzdc12";
        private string passwordStore;

        #region properties


        public string Titre { get; set; }

        public string URL { get; set; }

        public string Icone { get; set; }
        /// <summary>
        /// URL of doc file if exist
        /// </summary>
        public string DocURL { get; set; }

        public string Schema { get; set; }
        /// <summary>
        /// Site connection string
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
        /// Site ProviderName
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// Build a Css class name 
        /// </summary>
        public string Class
        {
            get
            {
                if (Titre == null)
                    return "tile1";
                //return class name
                return $"tile{Math.Min(Math.Round(((double)Titre.Length / 2)), 9)}";
            }
        }
        /// <summary>
        /// Get description from database
        /// </summary>
        public string Description
        {
            get; set;
        }
        #endregion properties


        #region method
        /// <summary>
        /// Load an object from an xml file
        /// </summary>
        /// <param name="SitePath">Xml file name</param>
        /// <returns>The object created from the xml file</returns>
        public static SiteConfig Load(string SitePath)
        {
            SimpleLogger.SimpleLog.Log($"Getting informations for {SitePath}");
            var xmlDoc = new XmlDocument();

            xmlDoc.Load(SitePath + "\\Configs\\appSettings.config");

            var config = new SiteConfig();

            config.ReadAppSetting(xmlDoc);

            xmlDoc.Load(SitePath + "\\Configs\\connections.config");

            ReadConnection(xmlDoc, config);

            var docPath = SitePath + "\\uploads\\docs\\doc.pdf";
            if (new FileInfo(docPath).Exists)
                config.DocURL = config.URL + "/Uploads//docs/doc.pdf";

            return config;
        }

        private static void ReadConnection(XmlDocument xmlDoc, SiteConfig config)
        {
            var nodeConnection = xmlDoc.SelectNodes("/connectionStrings/add[@name='TabloidConnection']");

            if (nodeConnection.Count > 0)
            {
                config.ConnectionString = nodeConnection[0].Attributes["connectionString"].Value;
                config.ProviderName = nodeConnection[0].Attributes["providerName"].Value;

                if (!string.IsNullOrEmpty(config.ConnectionString) && string.Equals(config.ProviderName, "Npgsql", StringComparison.InvariantCultureIgnoreCase))
                {
                    var sql = $"select description from pg_catalog.pg_description join pg_namespace on oid = objoid where nspname = '{config.Schema}'";
                    DataTools.DefaultProviderName = config.ProviderName;
                    string error;
                    config.Description = (string)DataTools.ScalarCommand(sql, null, config.ConnectionString, out error);

                    if (config.Description != null && config.Description.Length > 300) config.Description = config.Description.Substring(0, 300) + "..";
                }
            }
        }

        /// <summary>
        ///     Set properties from app.config
        /// </summary>
        /// <param name="src">xmlFile witch contain web.config</param>
        public void ReadAppSetting(XmlDocument src)
        {
            foreach (var pi in GetType().GetProperties())
            {
                switch (pi.Name)
                {
                    default:
                        object val = GetValueFromKey(src, "/appSettings/add", pi.Name);

                        if (pi.PropertyType.Name == "Boolean") val = Convert.ToBoolean(val);
                        if (pi.PropertyType.BaseType.Name == "Enum" && val != null) val = Enum.Parse(pi.PropertyType, val.ToString()) as Enum;

                        if (pi.SetMethod != null) pi.SetValue(this, val, null);

                        break;
                }
            }
        }
        private string GetValueFromKey(XmlDocument src, string collectionPath, string key)
        {
            var n = GetNodeFromCollection(src, collectionPath, key, "key");
            if (n == null) return null;
            return n.Attributes == null ? null : n.Attributes["value"].Value;
        }

        private XmlNode GetNodeFromCollection(XmlDocument src, string collectionPath, string name, string key)
        {
            var xnList = src.SelectNodes(collectionPath);
            return xnList == null ?
                null :
                xnList.Cast<XmlNode>().FirstOrDefault(n => n.Attributes != null && n.Attributes[key].Value == name);
        }

        public bool CheckAuth(string login)
        {
            
            var sql = ProviderName == "Npgsql" ?
                $"select count(*)>0 from utilisateurs join lst_roles on (id_utilisateur = lst_roles.utilisateurs_id and deleted_lst_roles=0) where deleted_utilisateurs=0 and logon ilike '{login}'" :
                $"select count(*)>0 from utilisateurs join lst_roles on (id_utilisateur = lst_roles.utilisateurs_id and deleted_lst_roles=0) where deleted_utilisateurs=0 and logon like '{login}'";

            DataTools.DefaultProviderName = ProviderName;
            string error;

            var val = DataTools.ScalarCommand(sql, null, ConnectionString, out error);


            if (!string.IsNullOrEmpty(error))//for tabloid without lst_roles
            {
                SimpleLogger.SimpleLog.Log($"Getting error {error}");

                sql = ProviderName == "Npgsql" ?
                    $"select count(*)>0 from utilisateurs join roles on (role = id_role and deleted_roles=0) where deleted_utilisateurs=0 and logon ilike '{login}'" :
                    $"select count(*)>0 from utilisateurs join roles on (role = id_role and deleted_roles=0) where deleted_utilisateurs=0 and logon like '{login}'";

                val = DataTools.ScalarCommand(sql, null, ConnectionString, out error);

                SimpleLogger.SimpleLog.Log($"Getting error trying for older Tabloid version : {error}");
            }

            SimpleLogger.SimpleLog.Log($"{val}");

            if (val == DBNull.Value) return false;

            var strVal = val.ToString();
            if (strVal == "0" || strVal == "False") return false;

            return true;

        }
        #endregion method
    }
}