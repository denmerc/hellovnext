using APLPX.Entity;
using System.Collections.Generic;
using System;

namespace APLPX.Client.Contracts
{
    public interface IDataImportDataService
    {
        #region DataImport

        bool SaveImportData(List<ImportData> importedDataDto,string fileName="");

        bool UpdateResult(ImportData importDataResultDto);

        void WriteError(string fileName, int lineNumber, string errorMsg);
        void WriteErrorsInBatch(string fileName, List<ImportData> failedDataRows);

        void CleanErrors(string errorFileName);
        void CleanErrorSet(string fileName);

        void CleanCompetitionImportResults();

        List<ImportError> GetImportErrors(string fileName);

        string GetImportDataResult(string fileName, DateTime importStart);

        #endregion

    }
}
