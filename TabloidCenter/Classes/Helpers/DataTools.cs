using System;
using System.Data;
using System.Data.Common;

namespace Tabloid.Classes.Data
{
    public static class DataTools
    {
        private static DbProviderFactory _defaultFactory;
        private static string _defaultProviderName;

        public static string DefaultProviderName
        {
            get { return _defaultProviderName; }

            set
            {
                DefaultFactory = null;
                _defaultProviderName = value;
            }
        }

        public static DataSet TabloidDataset { get; set; }
        public static DbProviderFactory DefaultFactory
        {
            get { return _defaultFactory ?? (_defaultFactory = DbProviderFactories.GetFactory(DefaultProviderName)); }

            set { _defaultFactory = value; }
        }

        public static DbConnection SetConnection(string connectionString)
        {
            return SetConnection(connectionString, null);
        }
        public static DbConnection SetConnection(string connectionString, out string lastError)
        {
            return SetConnection(connectionString, null, out lastError);
        }
        /// <summary>
        /// Connection handler
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="providerName"></param>
        /// <returns></returns>
        private static DbConnection SetConnection(string connectionString, string providerName)
        {
            DbConnection conn = null;

            var factory =
                !string.IsNullOrEmpty(providerName)
                    ? DbProviderFactories.GetFactory(providerName)
                    : DefaultFactory;

            conn = factory.CreateConnection();
            if (conn != null) conn.ConnectionString = connectionString;

            return conn;
        }
        /// <summary>
        /// Silent connection handler
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="providerName"></param>
        /// <param name="lastError"></param>
        /// <returns></returns>
        private static DbConnection SetConnection(string connectionString, string providerName, out string lastError)
        {
            DbConnection conn = null;
            lastError = "";

            try
            {
                conn = SetConnection(connectionString, providerName);
            }
            catch (Exception ex)
            {
                lastError = ex + "\n" + providerName + "\n" + connectionString;
            }
            return conn;
        }

        /// <summary>
        ///     retourne une table correstondant au select fourni
        /// </summary>
        /// <param name="sql">requete à executer</param>
        /// <param name="connectionString">Chaine de connection</param>
        /// <param name="lastError"></param>
        public static DataTable Data(string sql, string connectionString, out string lastError,
            DbParameter[] param = null)
        {
            var conn = SetConnection(connectionString, out lastError);
            return conn != null ? Data(sql, conn, out lastError, param) : null;
        }

        /// <summary>
        ///     retourne une table correstondant au select fourni
        /// </summary>
        /// <param name="sql">requete à executer</param>
        /// <param name="provider">Connecteur MySql</param>
        /// <param name="lastError"></param>
        public static DataTable Data(string sql, DbConnection provider, out string lastError,
            DbParameter[] parameters = null)
        {
            var dt = new DataTable();
            lastError = "";

            try
            {
                provider.Open();
                using (var adapter = DefaultFactory.CreateDataAdapter())
                {
                    if (adapter != null)
                    {
                        adapter.SelectCommand = DefaultFactory.CreateCommand();

                        if (adapter.SelectCommand != null)
                        {
                            if (parameters != null) adapter.SelectCommand.Parameters.AddRange(parameters);

                            adapter.SelectCommand.Connection = provider;

                            adapter.SelectCommand.CommandText = sql;
                        }

                        adapter.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex.Message + "\n" + sql;
                return null;
            }
            finally
            {
                if (provider.State == ConnectionState.Open) provider.Close();
            }
            return dt;
        }

        /// <summary>
        ///     affecte une table dans un dataset
        /// </summary>
        /// <param name="sql">requete à executer</param>
        /// <param name="table">nom de la table</param>
        /// <param name="connectionString">chaine de connection</param>
        /// <param name="dset">dataset devant contenir la table</param>
        /// <param name="lastError"></param>
        public static void Data(string sql, string table, string connectionString, DataSet dset, out string lastError)
        {
            var conn = SetConnection(connectionString, out lastError);
            if (conn != null)
                Data(sql, table, conn, dset, out lastError);
        }

        /// <summary>
        ///     affecte une table dans un dataset
        /// </summary>
        /// <param name="sql">requete à executer</param>
        /// <param name="table">nom de la table</param>
        /// <param name="provider">Connecteur MySql</param>
        /// <param name="dset">dataset devant contenir la table</param>
        /// <param name="lastError"></param>
        public static void Data(string sql, string table, DbConnection provider, DataSet dset, out string lastError)
        {
            lastError = "";

            try
            {
                provider.Open();
                if (dset.Tables[table] != null) dset.Tables.Remove(table);
                dset.Tables.Add(Data(sql, provider, out lastError));
            }
            catch (Exception ex)
            {
                lastError = ex.Message + "\n" + sql;
            }
            finally
            {
                if (provider.State == ConnectionState.Open) provider.Close();
            }
        }

        /// <summary>
        ///     Execute command
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static int Command(string sql, DbParameter[] param, string connectionString)
        {
            var connecteur = SetConnection(connectionString);
            if (connecteur != null)
                return Command(sql, param, connecteur);
            return -1;
        }

        /// <summary>
        ///     Execute command
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="connectionString"></param>
        /// <param name="lastError"></param>
        /// <returns></returns>
        public static int Command(string sql, DbParameter[] param, string connectionString, out string lastError)
        {
            var connecteur = SetConnection(connectionString, out lastError);
            if (connecteur != null)
                return Command(sql, param, connecteur, out lastError);
            return -1;
        }

        public static object ScalarCommand(string sql, DbParameter[] param, string connectionString,
            out string lastError)
        {
            var connecteur = SetConnection(connectionString, out lastError);
            return connecteur != null ? ScalarCommand(sql, param, connecteur, out lastError) : null;
        }

        /// <summary>
        ///     Exécute la commande sql sur le connecteur spécifié
        ///     retourne le nombre de ligne modifié ou -1 en cas d'erreur
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="connecteur"></param>
        /// <param name="lastError"></param>
        /// <returns></returns>
        public static int Command(string sql, DbParameter[] param, DbConnection connecteur, out string lastError)
        {
            var num = -1;
            lastError = "";
            try
            {
                num = Command(sql, param, connecteur);
            }
            catch (Exception ex)
            {
                lastError = ex + "\n" + sql;
            }
            finally
            {
                if (connecteur.State == ConnectionState.Open)
                    connecteur.Close();
            }
            return num;
        }
        /// <summary>
        ///     Exécute la commande sql sur le connecteur spécifié
        ///     retourne le nombre de ligne modifié ou -1 en cas d'erreur
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="connecteur"></param>
        /// <returns></returns>
        public static int Command(string sql, DbParameter[] param, DbConnection connecteur)
        {
            var num = -1;
            try
            {
                if (connecteur.State != ConnectionState.Open)
                    connecteur.Open();

                using (var command = DefaultFactory.CreateCommand())
                {
                    if (command != null)
                    {
                        command.CommandText = sql;
                        command.Connection = connecteur;
                        if (param != null) command.Parameters.AddRange(param);
                        num = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (connecteur.State == ConnectionState.Open)
                    connecteur.Close();
            }
            return num;
        }
        public static object ScalarCommand(string sql, DbParameter[] param, DbConnection connecteur,
            out string lastError)
        {
            lastError = "";
            try
            {
                if (connecteur.State != ConnectionState.Open)
                    connecteur.Open();
                using (var command = DefaultFactory.CreateCommand())
                {
                    if (command != null)
                    {
                        command.CommandText = sql;
                        command.Connection = connecteur;
                        if (param != null) command.Parameters.AddRange(param);
                        return command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex + "\n" + sql;
            }
            finally
            {
                if (connecteur.State == ConnectionState.Open)
                    connecteur.Close();
            }
            return false;
        }

        /// <summary>
        ///     Escape characters
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static string StringToSql(string src)
        {
            if (src == null) return null;
            src = src.Replace("\"", "''");
            return src.Replace("'", "''");
        }

        /// <summary>
        ///     Format date in iso8601
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string FormatDate(DateTime dt)
        {
            return string.Format("{0:yyyy/MM/dd HH:mm:ss}", dt);
        }

        /// <summary>
        ///     Retourne la valeur entre guillemet si besoin
        /// </summary>
        /// <param name="t">type du champ</param>
        /// <returns>chaine complété</returns>
        public static bool isQuotedType(DbType t)
        {
            return
                t == DbType.String ||
                t == DbType.Date ||
                t == DbType.DateTime ||
                t == DbType.DateTime2 ||
                t == DbType.DateTimeOffset ||
                t == DbType.StringFixedLength ||
                t == DbType.Time ||
                t == DbType.Xml;
        }


        /// <summary>
        ///     Retourne la valeur entre guillemet si besoin
        /// </summary>
        /// <param name="t">type du champ</param>
        /// <returns>chaine complété</returns>
        public static bool isQuotedType(Type t)
        {
            return
                t == typeof(string) ||
                t == typeof(DateTime);
        }

        /// <summary>
        ///     Add quote if needed
        /// </summary>
        /// <param name="t">type du champ</param>
        /// <returns>chaine complété</returns>
        public static string QuotedifNeeded(object o, DbType t)
        {
            return isQuotedType(t) ? "'" + o + "'" : o.ToString();
        }

        /// <summary>
        ///     Add quote if needed
        /// </summary>
        /// <param name="t">type du champ</param>
        /// <returns>chaine complété</returns>
        public static string QuoteIfNeeded(object o, Type t)
        {
            return isQuotedType(t) ? "'" + o + "'" : o.ToString();
        }

        /// <summary>
        /// return char parameter identifier for current provider
        /// </summary>
        /// <returns></returns>
        public static string GetParamIdentifier()
        {
            switch (DefaultProviderName)
            {
                case "MySql.Data.MySqlClient":
                    return "?";
                default:
                    return "@";
            }
        }
    }

    //public static class dbTypeTools
    //{
    //    public static string getDbProviderType(DbType t)
    //    {
    //        DataTools.DefaultFactory.
    //    }
    //}
    public static class GenericParameter
    {
        public static DbParameter Get(string name, DbType type, object value)
        {
            var p = DataTools.DefaultFactory.CreateParameter();
            if (p == null) return null;
            p.ParameterName = name;
            p.DbType = type;

            p.Value = value;
            return p;
        }
    }
}