using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.OrmLite;
using DB = APLPX.Client.Postgres.Models;

namespace APLPX.Client.Postgres
{
    public class GlobalParameters
    {
        private List<DB.GlobalParameter> _params; 


        public OrmLiteConnectionFactory DBConnectionFactory { get; set; }
        
        //load parameter names
        public static string PARAM_LoadFrequency="LoadFrequency";
        public static string PARAM_CSVFieldDelimiter="CSVFieldDelimiter";
	    public static string PARAM_CSVStringDelimiter="CSVStringDelimiter";

        // email notification parameters
        public static string PARAM_LoadAlertEmailAddr = "LoadAlertEmailAddr";
        public static string PARAM_LoadSendEmail = "LoadSendEmail";
        public static string PARAM_SMTPSSLEnabled = "SMTPSSLEnabled";
        public static string PARAM_SMTPServer = "SMTPServer";
        public static string PARAM_SMTPPort = "SMTPPort";
        public static string PARAM_SMTPUser = "SMTPUser";
        public static string PARAM_SMTPUserPassword = "SMTPUserPassword";

        //purge parameter names
	    public static string PARAM_KeepCompleteFileDays ="KeepCompleteFileDays";
	    public static string PARAM_KeepErrorFileDays ="KeepErrorFileDays";
	    public static string PARAM_KeepHistoryDays ="KeepHistoryDays";
	    public static string PARAM_KeepAggregateHistoryDays ="KeepAggregateHistoryDays";
	    public static string PARAM_KeepCompetitionImportResultsDays ="KeepCompetitionImportResultsDays";
        public static string PARAM_PurgeTime = "PurgeTime";

        //forecast metrics update schedule parameter names
        public static string PARAM_ForecastFrequency ="ForecastFrequency";
        public static string PARAM_ForecastTime = "ForecastTime";
        public static string PARAM_ForecastDayOfWeek = "ForecastDayOfWeek";
        public static string PARAM_ForecastDayOfMonth = "ForecastDayOfMonth";

        //database maintenance update schedule parameter names
	    public static string PARAM_DBMaintenanceFrequency ="DBMaintenanceFrequency";
        public static string PARAM_DBMaintenanceTime = "DBMaintenanceTime";
        public static string PARAM_DBMaintenanceDayOfWeek = "DBMaintenanceDayOfWeek";
        public static string PARAM_DBMaintenanceDayOfMonth = "DBMaintenanceDayOfMonth";
        public static string PARAM_DBMaintenanceBackupCopies = "DBMaintenanceBackupCopies";
        public static string PARAM_DBMaintenanceBackupPath = "DBMaintenanceBackupPath";   

        // UI control parameters
	    public static string PARAM_DefaultAggregationDays ="DefaultAggregationDays";
	    public static string PARAM_DefaultGroups ="DefaultGroups";
	    public static string PARAM_RequireImpactAnalysis ="RequireImpactAnalysis";
        public static string PARAM_AllowSelfApproval ="AllowSelfApproval";


        public GlobalParameters()
        {
            DBConnectionFactory = new OrmLiteConnectionFactory(
                ConfigurationManager.AppSettings["localConnectionString"],
                    PostgreSqlDialect.Provider
                );
            OrmLiteConfig.DialectProvider.NamingStrategy = new OrmLiteNamingStrategyBase();

            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                _params = db.Select<DB.GlobalParameter>();
               
            }
        }

        /// <summary>
        /// Retrieves the value of a Global Parameter 
        /// </summary>
        /// <param name="paramName"></param>
        /// <returns>The value of the global parameter.</returns>
        public string GetValue( string paramName)
        {
            DB.GlobalParameter p = GetParamInfo(paramName);
                 
            
            return p.ParamValue;
        }

       
        /// <summary>
        /// Stores the value for a global parameter in memory and in the database.
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="paramValue"></param>
        public void SetValue(string paramName, string paramValue)
        { 
            DB.GlobalParameter p = GetParamInfo(paramName);

            p.ParamValue = paramValue;

            SetParamInfo(p);
        }

        public DB.GlobalParameter GetParamInfo(string paramName)
        {
            DB.GlobalParameter result= _params.Find(x => x.ParamName.Equals(paramName));;

            if (result ==null)
            {
                throw (new KeyNotFoundException(string.Format("Parameter {0} not found in Global Parameters.", paramName)));
            }

            return result;
        }

        public void SetParamInfo(DB.GlobalParameter param)
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                db.Update<DB.GlobalParameter>(param);
                
                DB.GlobalParameter p = _params.Find(x => x.ParamName.Equals(param.ParamName));
                _params.Remove(p);
                _params.Add(param);
            }
        }
    }
}
