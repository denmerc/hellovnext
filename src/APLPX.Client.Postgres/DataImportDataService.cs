using APLPX.Client.Contracts;
using APLPX.Entity;
using Microsoft.Win32;
using ServiceStack.OrmLite;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using DB = APLPX.Client.Postgres.Models;
using ENT = APLPX.Entity;
using System.Text;


namespace APLPX.Client.Postgres
{
    public class DataImportDataService : IDataImportDataService
    {

        #region Private instance

        private const string HKEY_SOFTWARE = "SOFTWARE";
        private const string HKEY_APL = "APL";
        private const string HKEY_PRICEXPERT = "PriceXpert";
        private const int INSERT_BATCH_SIZE = 1024;

        private const string HKEY_PRICEXPERT_INPUT = "UploadPath";
        private const string HKEY_PRICEXPERT_INPUT_DEFAULT = "C:\\APL\\PriceXpert\\UploadPath";

        public OrmLiteConnectionFactory DBConnectionFactory { get; set; }
        AdminDataService _adminData;


        #endregion

        #region Constructor
        /// <summary>
        ///Constructor 
        /// </summary>
        public DataImportDataService()
        {
            //GlobalDBConnectionStr = GetGlobalConnectionString();

            DBConnectionFactory = new OrmLiteConnectionFactory(
                ConfigurationManager.AppSettings["localConnectionString"],
                    PostgreSqlDialect.Provider
                );
            OrmLiteConfig.DialectProvider.NamingStrategy = new OrmLiteNamingStrategyBase();

            _adminData = new AdminDataService();
        }

        #endregion


        #region Import data

        /// <summary>
        ///Staging competition data 
        /// </summary>
        /// <param name="importedDataDto">List<ImportData></param>
        /// <returns>bool result</returns>
        public bool SaveImportData(List<ImportData> importedDataDto, string sourceFileName = "")
        {
            string filePath = GetStagingLocation();
            string dateFormat = DateTime.Now.ToString();
            dateFormat = dateFormat.Replace(":", "");
            dateFormat = dateFormat.Replace("/", "");
            string fileName = "Competition(" + Path.GetFileNameWithoutExtension(sourceFileName) + ")" + ".csv";
            string batchIdentifier = sourceFileName + DateTime.Now;


            if (!Directory.Exists(filePath))
            {
                throw new ApplicationException("Staging location is not configured.");
            }

            filePath = Path.Combine(filePath, fileName);
            string rowItems = string.Empty;
            try
            {
                using (StreamWriter str = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
                {

                    DataTable dsErrorvalues = ToDataTable<ImportData>(importedDataDto);
                    dsErrorvalues.Columns.Remove("ErrorMsg");
                    if (dsErrorvalues.Columns.Contains("RowsInFile"))
                        dsErrorvalues.Columns.Remove("RowsInFile");
                    if (dsErrorvalues.Columns.Contains("RowsImported"))
                        dsErrorvalues.Columns.Remove("RowsImported");
                    if (dsErrorvalues.Columns.Contains("RowsWithError"))
                        dsErrorvalues.Columns.Remove("RowsWithError");
                    if (dsErrorvalues.Columns.Contains("ErrorFilePath"))
                        dsErrorvalues.Columns.Remove("ErrorFilePath");
                    if (dsErrorvalues.Columns.Contains("ErrorFileName"))
                        dsErrorvalues.Columns.Remove("ErrorFileName");
                    if (dsErrorvalues.Columns.Contains("StartTime"))
                        dsErrorvalues.Columns.Remove("StartTime");
                    if (dsErrorvalues.Columns.Contains("EndTime"))
                        dsErrorvalues.Columns.Remove("EndTime");
                    if (dsErrorvalues.Columns.Contains("Duration"))
                        dsErrorvalues.Columns.Remove("Duration");
                    if (dsErrorvalues.Columns.Contains("FileName"))
                        dsErrorvalues.Columns.Remove("FileName");
                    if (dsErrorvalues.Columns.Contains("Format"))
                        dsErrorvalues.Columns.Remove("Format");
                    if (dsErrorvalues.Columns.Contains("FileStatus"))
                        dsErrorvalues.Columns.Remove("FileStatus");
                    if (dsErrorvalues.Columns.Contains("LineNumber"))
                        dsErrorvalues.Columns.Remove("LineNumber");


                    foreach (DataRow datarow in dsErrorvalues.Rows)
                    {
                        string row = string.Empty;
                        int loopCount = 0;
                        foreach (object item in datarow.ItemArray)
                        {
                            rowItems = item.ToString();

                            if (loopCount == 0)
                            {
                                row += rowItems.ToString();
                            }
                            else
                            {
                                row += "," + rowItems.ToString();
                            }
                            loopCount += 1;
                            rowItems = string.Empty;
                        }
                        str.WriteLine(row);

                    }
                    str.Flush();
                }
            }

            catch (Exception ex)
            {
                throw ex;
            }
            return true;

        }

        /// <summary>
        ///Update the result of competition data staging information  
        /// </summary>
        /// <param name="importDataResultDto"></param>
        /// <returns></returns>
        public bool UpdateResult(ImportData importDataResultDto)
        {

            //try
            //{

            //    TimeSpan duration = System.DateTime.Now.Subtract(importDataResultDto.StartTime);
            //    importDataResultDto.Duration = String.Format("{0:0.00} Sec", duration.TotalSeconds);

            //    using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            //    {

            //        var q = db.From<DB.result>().Where<DB.result>(token => (token.filename == importDataResultDto.FileName) && (token.rowsinfile == importDataResultDto.RowsInFile));
            //        var flag = db.Exists<DB.result>(q);

            //        if (!flag)
            //        {
            //            //insert
            //            db.Insert(new DB.result
            //            {
            //                filename = importDataResultDto.FileName,
            //                format = importDataResultDto.Format,
            //                rowsinfile = importDataResultDto.RowsInFile,
            //                rowsimported = importDataResultDto.RowsImported,
            //                rowswitherror = importDataResultDto.RowsWithError,
            //                errorfilename = importDataResultDto.ErrorFileName,
            //                errorfilepath = importDataResultDto.ErrorFilePath,
            //                starttime = importDataResultDto.StartTime,
            //                endtime = System.DateTime.Now,
            //                duration = importDataResultDto.Duration,
            //                UserId = 1
            //            });
            //            return true;
            //        }
            //        else
            //        {
            //            //update
            //            db.UpdateOnly(new DB.result
            //            {

            //                filename = importDataResultDto.FileName,
            //                format = importDataResultDto.Format,
            //                rowsinfile = importDataResultDto.RowsInFile,
            //                rowsimported = importDataResultDto.RowsImported,
            //                rowswitherror = importDataResultDto.RowsWithError,
            //                errorfilename = importDataResultDto.ErrorFileName,
            //                errorfilepath = importDataResultDto.ErrorFilePath,
            //                starttime = importDataResultDto.StartTime,
            //                endtime = System.DateTime.Now,
            //                duration = importDataResultDto.Duration,
            //                UserId = 1
            //            }
            //           , onlyFields: p => new { p.filename, p.format, p.rowsinfile, p.rowsimported, p.rowswitherror, p.errorfilename, p.errorfilepath, p.starttime, p.endtime, p.duration }
            //            , where: (p => p.filename == importDataResultDto.FileName && p.rowsinfile == importDataResultDto.RowsInFile));
            return true;
            //        }
            //    }
            //}
            //catch (Exception)
            //{
            //    throw;
            //}
        }

        /// <summary>
        /// Get recently processed competition data results.
        /// </summary>
        /// <returns></returns>
        //public List<ImportData> GetCompetitionDataResults()
        //{
        //    using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
        //    {
        //        // Removed .Where(c => DateTime.Now.Subtract(c.endtime) <= TimeSpan.FromMinutes(5)) because there shouldn't be an arbitrary time limit
        //        var currentBatch = db.Select<DB.result>().Select(ent => new ENT.ImportData
        //        {
        //            FileName = ent.filename,
        //            RowsInFile = ent.rowsinfile,
        //            RowsImported = ent.rowsimported,
        //            RowsWithError = ent.rowswitherror,
        //            ErrorFilePath = ent.errorfilepath,
        //            ErrorFileName = ent.errorfilename,
        //            StartTime = ent.starttime,
        //            //EndTime = ent.endtime,
        //            Duration = ent.duration,
        //            FileStatus = "Determining status..."
        //        });
        //        //return 

        //        //TODO: needs to refactor
        //        List<ImportData> importResults = new List<ImportData>();
        //        importResults = currentBatch.ToList();
        //        List<ETLBatchStatus> responseData = new List<ETLBatchStatus>();
        //        AdminDataService adminData = new AdminDataService();

        //        if (importResults.Count > 0)
        //        {
        //            foreach (ImportData val in importResults)
        //            {
        //                if (Path.GetExtension(val.FileName).ToLower().Contains("csv") || Path.GetExtension(val.FileName).ToLower().Contains("txt"))
        //                {
        //                    responseData = adminData.CheckETLBatchStatus("Competition", Path.GetFileNameWithoutExtension(val.FileName));
        //                }
        //                else
        //                {
        //                    responseData = adminData.CheckETLBatchStatus("Competition", val.FileName);

        //                    foreach (ETLBatchStatus objVal in responseData)
        //                    {
        //                        TimeSpan difference = val.EndTime.Subtract(objVal.BatchStart);
        //                        if (difference.TotalMinutes < 5)
        //                        {
        //                            val.Name = objVal.BatchName;
        //                            val.FileStatus = objVal.Status;
        //                            val.EndTime = objVal.BatchEnd;
        //                        }

        //                    }
        //                }
        //            }
        //        }
        //        return importResults;
        //        //TODO: End

        //    }
        //}

        public void WriteError(string fileName, int lineNumber, string errorMsg)
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {

                db.Insert(new DB.CompetitionImportError()
                            {
                                FileName = fileName,
                                LineNumber = lineNumber,
                                ErrorMessage = errorMsg
                            }
                         );
            }
        }

        public void WriteErrorsInBatch(string fileName, List<ImportData> failedDataRows)
        {
            // do batched insert and a single large transaction for the whole set of errors
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                string sqlTemplate = "INSERT INTO \"PX_Competition\".\"CompetitionImportError\" VALUES ";
                string rowTemplate = "('{0}',{1},'{2}')";
                string fileNameQuote = fileName.Replace("'","''"); // deal with single quotes in the file name

                
                int batchSize = 0;


                var transaction = db.BeginTransaction();

                StringBuilder sql = new StringBuilder(sqlTemplate, INSERT_BATCH_SIZE * 1024 );
                foreach (var fr in failedDataRows)
                {
                    
                    batchSize++;

                    if (batchSize != 1)
                    {
                        sql.Append(",");
                    }
                    sql.AppendFormat(rowTemplate, fileNameQuote, fr.LineNumber, fr.ErrorMsg.Replace("'","''"));
                     
                    if (batchSize >= INSERT_BATCH_SIZE)
                    {
                        db.ExecuteNonQuery(sql.ToString());

                        // reset batch
                        sql.Clear();
                        sql.Append(sqlTemplate);
                        batchSize = 0;
                    }
                
                }

                //close out last batch
                if (batchSize>=1)
                {
                    db.ExecuteNonQuery(sql.ToString());
                }

                // finally commit the transaction
                transaction.Commit();
            }
        }

        public void CleanErrors(string errorFileName)
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {

                db.Delete<DB.CompetitionImportError>(i => i.FileName == errorFileName);
            }
        }

        public void CleanErrorSet(string fileName)
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                db.ExecuteNonQuery(string.Format("DELETE FROM \"PX_Competition\".\"CompetitionImportError\" WHERE (\"FileName\"='{0}' OR \"FileName\" LIKE '[{0}]%')", fileName));
            }
        }

        public void CleanCompetitionImportResults()
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                db.Delete<DB.result>();
                db.Delete<DB.CompetitionImportError>();
            }
        }

        #endregion
        #region Private methods

        /// <summary>
        ///Get staging location for competition file placeholder. 
        /// </summary>
        /// <returns></returns>
        private string GetStagingLocation()
        {
            string fileLandingPath = string.Empty;
            try
            {
                //RegistryKey localKey = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);

                //RegistryKey pxKey = localKey.OpenSubKey(HKEY_SOFTWARE, false).OpenSubKey(HKEY_APL, false).OpenSubKey(HKEY_PRICEXPERT);
                //fileLandingPath = (string)pxKey.GetValue(HKEY_PRICEXPERT_INPUT, HKEY_PRICEXPERT_INPUT_DEFAULT);

                fileLandingPath = ConfigurationManager.AppSettings["uploadPath"];

            }
            catch
            {
                throw new Exception("Unable to find the staging location.");
            }

            return fileLandingPath;
        }

        /// <summary>
        ///Utility helper convertion method  
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        private static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        #endregion



        public List<ImportError> GetImportErrors(string fileName)
        {


            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                var errors = db.Select<DB.CompetitionImportError>(string.Format("(\"FileName\"='{0}' OR \"FileName\" LIKE '[{0}]%')", fileName));
                return errors.ConvertAll(d => d.ConvertTo<ImportError>());
            }

        }



        public string GetImportDataResult(string fileName, DateTime importStart)
        {
            List<ETLBatchStatus> responseData = new List<ETLBatchStatus>();
            responseData = _adminData.CheckETLBatchStatus("Competition", Path.GetFileNameWithoutExtension(fileName));
            string result = "";

            if (responseData.Count > 0)
            {
                DateTime maxStart = DateTime.MinValue;
                ETLBatchStatus mostRecent = null;
                // if there is more than one, find the one with the most recent start date
                foreach (var r in responseData)
                {
                    if ((r.BatchStart > maxStart) && (r.BatchStart > importStart))
                    {
                        maxStart = r.BatchStart;
                        mostRecent = r;
                    }
                }

                if (mostRecent != null)
                {
                    result = mostRecent.Status;
                }
            }

            return result;
        }


    }
}
