using APLPX.Entity;
using System.Collections.Generic;

namespace APLPX.Client.Contracts
{
    public interface IAdminDataService
    {
        List<User> LoadUserList();
        long SaveUser(User user);

        int DeleteUser(int userId);

        
        bool RenameUserFolder(int folderId, int userId, string folderName);


        bool ChangePassword(string userName, string oldPassword, string newPassword);

        List<IPricingTemplate> LoadTemplateList();
        long SaveTemplate(IPricingTemplate template);
        int DeleteTemplate(int templateId);
        List<PercentilePriceRange> GeneratePercentilePriceRanges(int numGroups);
        PriceRange GetOverallPriceRange();


        List<GlobalParameter> LoadGlobalParameterList();

        GlobalParameter LoadGlobalParameter(string paramName);

        bool SaveGlobalParameter(GlobalParameter parameter);
        //List<ETLBatch> LoadEtlBatchList();
        List<ETLBatchStatus> CheckETLBatchStatus(string batchName, string batchIdentifier);
        List<ETLBatchStatus> GetAllJobLastRunStatus();
        List<ETLValidationError> GetValidationErrors(long batchID, string stepName);
    }


}
