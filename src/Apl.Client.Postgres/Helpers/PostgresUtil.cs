using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APLPX.Client.Postgres.Models;
using ServiceStack.OrmLite;

namespace APLPX.Client.Postgres.Helpers
{
    public class PostgresUtil
    {
        // data type constants
        public const string DATATYPE_TEXT = "text";
        public const string DATATYPE_CHARACTER_VARYING = "varchar";
        public const string DATATYPE_CHARACTER = "bpchar";
        public const string DATATYPE_DATE = "date";
        public const string DATATYPE_BIGINT = "int8";
        public const string DATATYPE_TIMESTAMP_NO_TZ = "timestamp";
        public const string DATATYPE_BOOLEAN = "bool";
        public const string DATATYPE_INTEGER = "int4";
        public const string DATATYPE_NUMERIC = "numeric";
        public const string DATATYPE_HSTORE = "hstore";
        public const string DATATYPE_JSONB = "jsonb";


        public static List<PGColumnInfo> GetColumnList( string schemaName, string tableName)
        {
            List<PGColumnInfo> columns;

            OrmLiteConnectionFactory DBConnectionFactory = new OrmLiteConnectionFactory(
                ConfigurationManager.AppSettings["localConnectionString"],
                    PostgreSqlDialect.Provider
                );
            OrmLiteConfig.DialectProvider.NamingStrategy = new OrmLiteNamingStrategyBase();


            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                string sqlColumnInfo = string.Format("SELECT column_name, ordinal_position, is_nullable, udt_name, character_maximum_length " +
                                                    " FROM information_schema.columns " +
                                                    " WHERE  table_schema='{0}'" +
                                                    " AND  table_name='{1}'", schemaName, tableName);

                columns = db.Select<PGColumnInfo>(sqlColumnInfo);

            }

            return columns;
        }
    }
}
