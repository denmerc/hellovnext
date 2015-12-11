using APLPX.Entity;
using ServiceStack;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.PostgreSQL;
using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using DB = APLPX.Client.Postgres.Models;
using ENT = APLPX.Entity;
using System.Configuration;
using Newtonsoft.Json;
using APLPX.Client.Contracts;
using System.Security.Authentication;
using System.Text;
using APLPX.Client.Postgres.Helpers;


namespace APLPX.Client.Postgres
{
    public class AdminDataService : IAdminDataService
    {
        public OrmLiteConnectionFactory DBConnectionFactory { get; set; }
        public AdminDataService()
        {
            DBConnectionFactory = new OrmLiteConnectionFactory(
                ConfigurationManager.AppSettings["localConnectionString"],
                    PostgreSqlDialect.Provider
                );
            OrmLiteConfig.DialectProvider.NamingStrategy = new OrmLiteNamingStrategyBase();

        }


        public List<User> LoadUserList()
        {
            try
            {
                using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
                {
                        var users = db.Select<DB.User>().Select(ent => new ENT.User
                        {
                            Id = ent.UserId,
                            FirstName = ent.FirstName,
                            LastName = ent.LastName,
                            Password = ent.Password,
                            CreatedDate = ent.CreateTS,
                            IsActive = ent.IsActive,
                            Login = ent.Login,
                            Email = ent.Email,
                            Folders = ent.FolderList.Select(f => new Folder 
                                    { 
                                        
                                        FolderId = f.FolderId,
                                        Name = f.Name,
                                        ModuleFeatureId = f.ModuleFeatureId,
                                    }).ToList(),
                            Roles = ent.Roles
                                    .Select( r => (UserRoleType) r)
                                    .ToList()
                        });
                        return users.ToList();                 
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public long SaveUser(User userToSave)
        {
            try
            {
                var idBytes = Encoding.UTF8.GetBytes(userToSave.Login.ToString().ToLower());
                var newBytes = Encoding.UTF8.GetBytes(userToSave.Password);
                //userToSave.Password = Convert.ToBase64String(Hash.HashPasswordWithSalt(newBytes, idBytes));

                if(userToSave.Folders == null || userToSave.Folders.Count() == 0)
                {

                    List<Folder> userFolders = new List<Folder>();

                    userFolders.Add(new Folder() { FolderId = 1, Name = "Folder1" });
                    userFolders.Add(new Folder() { FolderId = 2, Name = "Folder2" });
                    userFolders.Add(new Folder() { FolderId = 3, Name = "Folder3" });
                    userFolders.Add(new Folder() { FolderId = 4, Name = "Folder4" });
                    userFolders.Add(new Folder() { FolderId = 5, Name = "Folder5" });
                    userFolders.Add(new Folder() { FolderId = 6, Name = "Folder6" });

                    userToSave.Folders = userFolders;

                }

                using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
                {
                    if (userToSave.Id == 0)
                    {
                            //insert
                            db.Insert(new DB.User
                            {

                                Login = userToSave.Login.ToLower(),
                                FirstName = userToSave.FirstName,
                                LastName = userToSave.LastName,
                                ModifyTS = DateTime.Now,
                                UserId  = userToSave.Id,
                                Email = userToSave.Email,
                                //Greeting = 
                                //LoginDate = 
                                CreateTS = DateTime.Now,
                                IsActive = userToSave.IsActive,
                                Roles = userToSave.Roles.Select(r => (int)r).ToArray(), 
                                Password = Convert.ToBase64String(Hash.HashPasswordWithSalt(newBytes, idBytes)),
                                FolderList = userToSave.Folders.Select(f => new DB.Folder
                                    { FolderId = f.FolderId, Name = f.Name, ModuleFeatureId = f.ModuleFeatureId}).ToArray()
                            });

                        return db.LastInsertId();      
                    }
                    else
                    {
                        var tempUser = LoadUserList().Find(usr => usr.Id == userToSave.Id);

                        if(tempUser != null && tempUser.Password != userToSave.Password)
                            userToSave.Password = Convert.ToBase64String(Hash.HashPasswordWithSalt(newBytes, idBytes));

                        //update
                        db.Save(new DB.User
                        {

                            Login = userToSave.Login.ToLower(),
                            FirstName = userToSave.FirstName,
                            LastName = userToSave.LastName,
                            ModifyTS = DateTime.Now,
                            UserId = userToSave.Id,
                            CreateTS = userToSave.CreatedDate,
                            FolderList = userToSave.Folders.Select(f => new DB.Folder
                                    {
                                        FolderId = f.FolderId, 
                                        ModuleFeatureId = f.ModuleFeatureId, 
                                        Name = f.Name 
                                    }).ToArray(),
                            Email = userToSave.Email,
                            //Greeting = 
                            //LoginDate = 
                            IsActive = userToSave.IsActive,
                            Roles = userToSave.Roles.Select(r => (int)r).ToArray(),
                            Password = userToSave.Password
                        });
                            //, onlyFields: p => new { p.Login, p.FirstName, p.LastName, p.IsActive, p.Roles, p.Password }
                            //, where: p => p.UserId == userToSave.Id);
                            return userToSave.Id;
                    }
                }
            }
                catch (Exception)
            {
                throw;
            }
        }

        public int DeleteUser(int userToDeleteId)
        {
            try
            {
                using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
                {
                    return db.Delete<DB.User>(usr => usr.Where(u => u.UserId == userToDeleteId));
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            if (userName == string.Empty) { throw new ArgumentOutOfRangeException("Invalid parameter: userName."); }
            if (oldPassword == string.Empty) { throw new ArgumentOutOfRangeException("Invalid parameter: oldPassword."); }
            if (newPassword == string.Empty) { throw new ArgumentOutOfRangeException("Invalid parameter: newPassword."); }
            
            using (IDbConnection db = DBConnectionFactory.CreateDbConnection())
            {
                var user = db.Single<DB.User>(u => u.Login == userName.ToLower());
                if (user == null)
                {
                    throw new KeyNotFoundException( "User not found.");
                }

                var oldBytes = Encoding.UTF8.GetBytes(oldPassword);
                var idBytes = Encoding.UTF8.GetBytes(user.UserId.ToString());
                if ( user.Password != Convert.ToBase64String(Hash.HashPasswordWithSalt( oldBytes, idBytes )))  
                {
                    throw new InvalidCredentialException("User not authorized.");
                }
                var newBytes = Encoding.UTF8.GetBytes(newPassword);
                user.Password = Convert.ToBase64String(Hash.HashPasswordWithSalt( newBytes, idBytes));
                db.Save<DB.User>(user);
                return true;
            }
        }

        public List<ENT.IPricingTemplate> LoadTemplateList()
        {
            try
            {
                using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
                {

                    List<ENT.IPricingTemplate> templateList = new List<ENT.IPricingTemplate>();

                        var temps = db.Select<DB.Template>().OrderBy( o => o.TemplateType).ToList();
                        
                        temps.ForEach(x =>
                        {
                            TemplateType type = (TemplateType)x.TemplateType;
                    
                            switch (type)
                            {
                                case TemplateType.NotSet:
                                    break;
                                case TemplateType.Optimization:
                                    var ot = new ENT.PriceOptimizationTemplate
                                          {
                                              Id = x.TemplateId,
                                              Name = x.Name,
                                              Description = x.Description,
                                              Notes = x.Notes,
                                              TemplateType = (TemplateType)x.TemplateType,
                                              Rules = JsonConvert.DeserializeObject<List<PriceOptimizationRule>>(x.Rules),
                                              DateCreated = x.CreateTS
                                              //Rules = t.Rules.Select(or => new ENT.PriceOptimizationRule
                                              //{
                                              //    PercentChange = or.OptimizationPercentage,
                                              //    DollarRangeLower = or.MinValue,
                                              //    DollarRangeUpper = or.MaxValue
                                              //}).ToList()
                                          };
                                    ot.Rules.ForEach(rule => rule.IsUpperRangeEditable = true);
                                    ot.Rules.ForEach(rule => rule.IsLowerRangeEditable = true);

                                    if (ot.Rules[ot.Rules.Count() - 1].DollarRangeUpper == null)
                                        ot.Rules[ot.Rules.Count() - 1].IsUpperRangeEditable = false;

                                    templateList.Add(ot);
                                    break;
                                case TemplateType.Rounding:
                                    var rt = new ENT.PriceRoundingTemplate
                                          {
                                              Id = x.TemplateId,
                                              Name = x.Name,
                                              Description = x.Description,
                                              Notes = x.Notes,
                                              TemplateType = (TemplateType)x.TemplateType,
                                              Rules = JsonConvert.DeserializeObject<List<PriceRoundingRule>>(x.Rules),
                                              DateCreated = x.CreateTS


                                          };
                                    rt.Rules.ForEach(rule => rule.IsUpperRangeEditable = true);
                                    rt.Rules.ForEach(rule => rule.IsLowerRangeEditable = true);

                                    if (rt.Rules[rt.Rules.Count() - 1].DollarRangeUpper == null)
                                        rt.Rules[rt.Rules.Count() - 1].IsUpperRangeEditable = false;

                                    templateList.Add(rt);
                                    break;
                                case TemplateType.Markup:
                                    var mt = new ENT.PriceMarkupTemplate
                                          {
                                              Id = x.TemplateId,
                                              Name = x.Name,
                                              Description = x.Description,
                                              Notes = x.Notes,
                                              TemplateType = (TemplateType)x.TemplateType,
                                              Rules = JsonConvert.DeserializeObject<List<PriceMarkupRule>>(x.Rules),
                                              DateCreated = x.CreateTS
                                          };
                                    mt.Rules.ForEach(rule => rule.IsUpperRangeEditable = true);
                                    mt.Rules.ForEach(rule => rule.IsLowerRangeEditable = true);
                                    if (mt.Rules[mt.Rules.Count() - 1].DollarRangeUpper == null)
                                        mt.Rules[mt.Rules.Count() - 1].IsUpperRangeEditable = false;

                                    templateList.Add(mt);
                                    break;  
                                default:
                                    break;
                            }
                        }
                        );

                        return templateList;
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        public long SaveTemplate(IPricingTemplate template)
        {
            if (template == null) { throw new ArgumentNullException("No template specified for saving."); }

            DB.Template t = new DB.Template
            {
                Name = template.Name,
                Description = template.Description,
                TemplateType = (int)template.TemplateType,
                Notes = template.Notes,
                CreateTS = DateTime.Now, //only used for new templates
                
            };
           switch (template.TemplateType)
            {
                case TemplateType.NotSet:
                    break;
                case TemplateType.Optimization:

                    t.Rules = JsonConvert.SerializeObject(((PriceOptimizationTemplate)template).Rules);
                    
                    //t.Rules = ((PriceOptimizationTemplate)template).Rules.Select(r => new DB.OptimizationTemplateRule
                    //    {
                    //        MinValue = r.DollarRangeLower,
                    //        MaxValue = r.DollarRangeUpper,
                    //        PercentChange = r.PercentChange
                    //    }).Cast<DB.TemplateRule>().ToList();
                    
                    break;
                case TemplateType.Rounding:

                    t.Rules = JsonConvert.SerializeObject(((PriceRoundingTemplate)template).Rules);
                    //t.Rules = ((PriceRoundingTemplate)template).Rules.Select(r => new DB.PriceRoundingRule
                    //    {
                    //        MinValue = r.DollarRangLeower,
                    //        MaxValue = r.DollarRangeUpper, 
                    //        RoundingType = (int) r.RoundingType
                    //    }).ToList();
                    break;
                case TemplateType.Markup:
                    t.Rules = JsonConvert.SerializeObject(((PriceMarkupTemplate)template).Rules);

                    //t.Rules = ((PriceMarkupTemplate)template).Rules.Select(r => new DB.PriceMarkupRule
                    //    {
                    //        MinValue = r.DollarRangeLower,
                    //        MaxValue = r.DollarRangeUpper,
                    //        PercentLimitLower = r.PercentLimitLower,
                    //        PercentLimitUpper = r.PercentLimitUpper
                    //    }).ToList();
                    break;
                default:
                    break;
            }

            try
            {
                using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
                {

                    if (template.Id == 0)
                    {
                            db.Insert(t);
                            return db.LastInsertId();
                    }
                    else
                    {
                            db.UpdateOnly(
                                t
                                , onlyFields: p => new { p.Name, p.Description, p.Notes, p.Rules, p.TemplateType }
                                , where: p => p.TemplateId == template.Id);
                            return template.Id;
                    }
                }
            }

            catch (Exception)
            {
                throw;
            }
        }

        public int DeleteTemplate(int templateId)
        {
            try
            {
                using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
                {
                    return db.Delete<DB.Template>(template => template.Where(temp => temp.TemplateId == templateId));
                }
            }

            catch (Exception)
            {
                throw;
            }
        }

        public List<ENT.GlobalParameter> LoadGlobalParameterList()
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                return db.Select<DB.GlobalParameter>().Select( gp => new GlobalParameter
                    {
                        ParamDefaultValue = gp.ParamDefaultValue,
                        ParamDescription = gp.ParamDescription,
                        ParamName = gp.ParamName,
                        ParamPrompt = gp.ParamPrompt,
                        ParamValue = gp.ParamValue,
                        ParamDomain = new ParameterDomain
                        {
                            FormatString = gp.ParamDomain.FormatString,
                            ListValues = gp.ParamDomain.ListValues,
                            MaxLength = gp.ParamDomain.MaxLength,
                            MaxValue = gp.ParamDomain.MaxValue,
                            MinValue = gp.ParamDomain.MinValue,
                            MultilineText = gp.ParamDomain.MultilineText,
                            ParamDomainType = (ENT.ParamDomainType) gp.ParamDomain.ParamDomainType,                          
                        }
                        
                    }).ToList();
            }
        }

        public ENT.GlobalParameter LoadGlobalParameter(string paramName)
        {
            if (paramName == string.Empty) { throw new ArgumentNullException ( "Global Parameter - paramName cannot be empty.");}
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                var param = db.Single<DB.GlobalParameter>(gp => gp.ParamName == paramName);
                if (param == null) { throw new KeyNotFoundException("Global Parameter not found."); }
                return new ENT.GlobalParameter
                {
                    ParamDefaultValue = param.ParamDefaultValue,
                    ParamDescription = param.ParamDescription,
                    ParamDomain = new ENT.ParameterDomain 
                        { 
                            FormatString = param.ParamDomain.FormatString,
                            ListValues = param.ParamDomain.ListValues,
                            MaxLength = param.ParamDomain.MaxLength,
                            MinValue = param.ParamDomain.MinValue,
                            MultilineText = param.ParamDomain.MultilineText,
                            ParamDomainType = (ENT.ParamDomainType) param.ParamDomain.ParamDomainType
                        },
                    ParamName = param.ParamName,
                    ParamPrompt = param.ParamPrompt,
                    ParamValue = param.ParamValue
                };
            }
        }

        public bool SaveGlobalParameter(GlobalParameter parameter)
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                db.UpdateOnly(new DB.GlobalParameter
                {
                    ParamValue = parameter.ParamValue
                }
                , onlyFields: p => new { p.ParamValue }
                , where: p => p.ParamName == parameter.ParamName
                );
                return true;
            }
        }


        public List<ENT.ETLBatch> LoadEtlBatchList()
        {
            using(IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                return db.Select<DB.ETLBatch>().Select(b => new ETLBatch
                    {
                        BatchId = b.BatchID,
                        Name = b.Name,
                        EffectiveDate = b.EffectiveDate,
                        StartTS = b.StartTS,
                        EndTS = b.EndTS                       
                    }).ToList();
            }
        }


        public bool RenameUserFolder(int folderId, int userId, string folderName)
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                var user = db.Single<DB.User>(u => u.UserId == userId);
                if (user == null)
                {
                    throw new KeyNotFoundException("User not found.");
                }

                var folderToUpdate = user.FolderList.SingleOrDefault(u => u.FolderId == folderId);
                if (folderToUpdate == null)
                {
                    throw new KeyNotFoundException("User Folder not found");
                }
                folderToUpdate.Name = folderName;
                db.Save<DB.User>(user);
                return true;
            }
        }


        public List<ETLBatchStatus> CheckETLBatchStatus(string batchName, string batchIdentifier)
        {
            using( IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                var statuses = db.Select<DB.ETLBatchStatus>(string.Format("SELECT * FROM \"PX_Stage\".\"FN_CheckBatchStatus\"('{0}', '{1}')", batchName, batchIdentifier));
                return statuses.Select(s => new ENT.ETLBatchStatus
                                                {
                                                    BatchID = s.BatchID,
                                                    BatchIdentifier = s.BatchIdentifier,
                                                    Status = s.Status,
                                                    StepName = s.StepName,
                                                    ErrorMsg = s.ErrorMsg,
                                                    BatchStart = s.BatchStart,
                                                    BatchEnd = s.BatchEnd,
                                                    EffectiveDate = s.EffectiveDate, 
                                                    BatchName = s.Name
                                                }).ToList();
            }
        }


        public List<PercentilePriceRange> GeneratePercentilePriceRanges(int numGroups)
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                var ranges = db.Select<DB.PercentilePriceRange>(string.Format("SELECT * FROM \"PX_Main\".\"FN_TemplatePercentilePriceRanges\"({0})", numGroups));
                return ranges.Select(s => new ENT.PercentilePriceRange
                {
                    MinPrice=s.MinPrice,
                    MaxPrice=s.MaxPrice,
                    SKUCount=s.SKUCount
                }).ToList();
            }
        }

        public PriceRange GetOverallPriceRange()
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                var ranges = db.Single<DB.PriceRange>("SELECT * FROM \"PX_Main\".\"FN_TemplatePriceRange\"()");
                return new ENT.PriceRange
                {
                    MinPrice = ranges.MinPrice,
                    MaxPrice = ranges.MaxPrice
                };
            }
        }


        public List<ETLBatchStatus> GetAllJobLastRunStatus()
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                var jobs = db.Select<DB.ETLRecentJobStatus>();
                var jobSteps = db.Select<DB.ETLBatchStatusStep>();
                var result = jobs.ConvertAll<ETLBatchStatus>( s => s.ToDto());
                foreach (var job in result)
                {
                    job.JobSteps = jobSteps.Where( s => s.BatchID == job.BatchID).ToList().ConvertAll( t => t.ToDto());            
                }
                return result;
            }
        }

        public List<ETLValidationError> GetValidationErrors(long batchID, string stepName)
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                return db.Select<DB.ETLValidationError>().Where(e => e.BatchID == batchID && e.StepName == stepName).ToList().ConvertAll(ve => ve.ConvertTo<ETLValidationError>());
            }

        }


    }

    public static class AdminConvertExtensions
    {
        public static ETLBatchStatus ToDto(this DB.ETLRecentJobStatus from)
        {
            return from.ConvertTo<ETLBatchStatus>();
        }
        public static ETLBatchStatusStep ToDto(this DB.ETLBatchStatusStep from)
        {
            return from.ConvertTo<ETLBatchStatusStep>();
        }
    }
}
