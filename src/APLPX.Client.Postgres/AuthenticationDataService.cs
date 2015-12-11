using APLPX.Entity;
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
using System.Security.Cryptography;
using System.Text;
using System.Security.Authentication;
using APLPX.Client.Postgres.Helpers;

namespace APLPX.Client.Postgres
{
    public class AuthenticationDataService : IAuthenticationDataService
    {
        public OrmLiteConnectionFactory DBConnectionFactory { get; set; }
        public AuthenticationDataService()
        {
            DBConnectionFactory = new OrmLiteConnectionFactory(
                ConfigurationManager.AppSettings["localConnectionString"],
                    PostgreSqlDialect.Provider
                );
            OrmLiteConfig.DialectProvider.NamingStrategy = new OrmLiteNamingStrategyBase();
        }




        public User Authenticate(string userName, string password)
        {
            if (userName == string.Empty) { throw new ArgumentNullException("Username is invalid."); }
            if (password == string.Empty) { throw new ArgumentNullException("Password is invalid."); }
            
            using ( IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {


                var user = db.SingleWhere<DB.User>("Login", userName.ToLower());
                if (user == null) { throw new KeyNotFoundException("User not found.");}
                if (user.IsActive == false) { throw new InvalidCredentialException("User Not Authorized."); }

                var pwdBytes = Encoding.UTF8.GetBytes(password);
                var idBytes = Encoding.UTF8.GetBytes(user.Login.ToString());
                if (user.Password != Convert.ToBase64String(Hash.HashPasswordWithSalt(pwdBytes, idBytes))) throw new InvalidCredentialException("User Not Authorized.");
                
                var modules = db.LoadSelect(db.From<DB.Module>().Join<DB.ModuleFeature>());
                if (modules == null) { modules = new List<DB.Module>(); }
                return new User 
                        {
                            Id = user.UserId,
                            Login = user.Login,
                            Roles = user.Roles.Select( r => (UserRoleType) r).ToList(),
                            IsActive = user.IsActive,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Folders = user.FolderList == null ? new List<ENT.Folder>() : user.FolderList.Select( f => new Folder {FolderId = f.FolderId, Name = f.Name, ModuleFeatureId = f.ModuleFeatureId}).ToList(),
                            Modules = modules.Select(m => new ENT.Module
                                    {
                                        Name = m.Name,
                                        Title = m.Description,
                                        Type = (ENT.ModuleType) m.Id,
                                        Features = m.ModuleFeatures.Select(mf => new ENT.ModuleFeature 
                                                                    { 
                                                                        Type = (ENT.ModuleFeatureType) mf.Id,
                                                                        Name = mf.Name,
                                                                        Description = mf.Description
                                                                    }).ToList()
                                    }).ToList()
                        }; 
            }

        }

    }
}
