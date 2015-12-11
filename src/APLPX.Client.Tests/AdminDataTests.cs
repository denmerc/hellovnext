using ServiceStack.OrmLite;
using System;
using System.Data;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Configuration;
using Newtonsoft.Json;
using System.Data.SqlClient;
using Dapper;
using APLPX.Client.Sql;
using DB = APLPX.Client.Sql.Models;
using ENT = APLPX.Entity;
using APLPX.Entity;


namespace APLPX.Client.Tests
{
    [TestClass, Ignore]
    public class AdminDataTests
    {
        public AdminData AdminRepo { get; set; }
        int minDriver = 1, maxDriver = 3;
        int minGroup = 1; int maxGroup = 15;
        int minDecimal = 1500; int maxDecimal = 500000;
        int minFilter = 1000; int maxFilter = 1500;
        int minPriceList = 1; int maxPriceList = 12;
        Random r = new Random();

        public OrmLiteConnectionFactory DBConnectionFactory { get; set; }
        public int UserIdToTest { get; set; }
        public int TemplateIdToTest { get; set; }
        [TestInitialize]
        public void Initialize()
        {
            DBConnectionFactory = new OrmLiteConnectionFactory(ConfigurationManager.AppSettings["localConnectionString"], SqlServerDialect.Provider); 
            OrmLiteConfig.DialectProvider.NamingStrategy = new OrmLiteNamingStrategyBase();

            AdminRepo = new AdminData();
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {


                db.Delete<DB.User>(u => u.Login == "test");
                db.Delete<DB.User>(u => u.Login == "testInsert");
                db.Delete<DB.User>(u => u.Login == "testSave");
                db.Delete<DB.Template>(t => t.Name == "testTemplate");
                db.Delete<DB.Template>(t => t.Name == "testTemplate-Inserted");
                db.Delete<DB.Template>(t => t.Name == "testTemplate-Updated");


                db.InsertAll(new[]
                {
                   new DB.User
                    { 
                        Login = "test", Password = "password",  
                        Roles = new [] {
                            (int)(ENT.UserRoleType.AplUserRoleAdministrator)
                        }, 
                        FirstName = "Bon", LastName="Jovi", 
                        FolderList = new DB.Folder[]{ 
                            new DB.Folder { FolderId = 76, Name="Folder76"}, 
                            new DB.Folder { FolderId = 1, Name = "Recent"}, 
                            new DB.Folder { FolderId = 2, Name = "Shared" }
                        }, 
                        CreateTS = DateTime.UtcNow               
                    }
               });


                UserIdToTest = db.Single<DB.User>(u => u.Login == "test").UserId;

                db.InsertAll(new[]
                {
                    //new DB.User{ Login = "admin", Password = "password",  UserRole = (int)(ENT.UserRoleType.AplUserRoleAdministrator), FirstName = "Bon", LastName="Jovi", SearchGroups = new []{ "Folder76", "Recent", "Shared" } , DateCreated = DateTime.UtcNow},
                    //new DB.User{ Login = "analyst", Password = "password", UserRole = (int) (ENT.UserRoleType.AplUserRolePricingAnalyst), FirstName = "Van", LastName = "Halen", SearchGroups = new []{ "Folder76"} , DateCreated = DateTime.UtcNow},
                    new DB.Template
                    { 
                       
                        Name = "testTemplate",
                        Description = "testTemplateDescription",
                        Notes = "testNotes",
                        TemplateType = (int) TemplateType.Markup,
                        CreateTS = DateTime.Now,
                        Rules = JsonConvert.SerializeObject(
                            new List<DB.PriceMarkupRule>
                            {
                                new DB.PriceMarkupRule{ 
                                    DollarRangeLower = 1M, 
                                    DollarRangeUpper = 2M, 
                                    PercentLimitLower = 3M, 
                                    PercentLimitUpper= 4M}
                            })  
                    }



               });


                TemplateIdToTest = db.Single<DB.Template>(t => t.Name == "testTemplate").TemplateId;



            }


        }

        [TestCleanup]
        public void Cleanup()
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {

                db.Delete<DB.User>(u => u.Login == "test");
                db.Delete<DB.User>(u => u.Login == "testInsert");
                db.Delete<DB.User>(u => u.Login == "testSave");
                db.Delete<DB.Template>(t => t.Name == "testTemplate");
                db.Delete<DB.Template>(t => t.Name == "testTemplate-Inserted");
                db.Delete<DB.Template>(t => t.Name == "testTemplate-Updated");

            }
        }

        [TestMethod]
        public void Admin_LoadUserList()
        {
            var response = AdminRepo.LoadUserList();
            Assert.IsNotNull(response);
        }


        [TestMethod]
        public void Admin_User_SaveNew()
        {
            var u = new User
            {
                Id = 0,
                Login = "testInsert",
                IsActive = true,
                FirstName = "test first name",
                LastName = "test last name",
                Greeting = "test greeting",
                Role = ENT.UserRoleType.AplUserRoleAdministrator,
                Password = "test password",
                CreatedDate = DateTime.Now
            };

            var response = AdminRepo.SaveUser(u);
            Assert.IsNotNull(response);


        }


        [TestMethod]
        public void Admin_User_SaveNew_ButLoginAlreadyExists() //TODO: EXPECT  FALSE
        {
            var u = new User
            {
                Id = 0,
                Login = "test",
                IsActive = true,
                FirstName = "test first name",
                LastName = "test last name",
                Greeting = "test greeting",
                Role = ENT.UserRoleType.AplUserRoleAdministrator,
                Password = "test password",
                CreatedDate = DateTime.Now
            };

            var response = AdminRepo.SaveUser(u);
            Assert.IsFalse(response);

        }

        [TestMethod]
        public void Admin_User_UpdateExisting()
        {
            var u = new User
            {
                Id = UserIdToTest,
                Login = "test",
                IsActive = true,
                FirstName = "test first name2",
                LastName = "test last name",
                Greeting = "test greeting",
                Role = ENT.UserRoleType.AplUserRoleAdministrator,
                Password = "test password"
            };

            var response = AdminRepo.SaveUser(u);
            Assert.IsNotNull(response);

            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                DB.User updatedUser = db.Single<DB.User>(up => up.Login == "test");
                Assert.AreEqual("test first name2", updatedUser.FirstName);
            }


        }

        [TestMethod]
        public void Admin_User_UpdateExistingWithoutSpecifyingId()
        {
            var u = new User
            {
                Login = "test",
                IsActive = true,
                FirstName = "test first name2",
                LastName = "test last name",
                Greeting = "test greeting",
                Role = ENT.UserRoleType.AplUserRoleAdministrator,
                Password = "test password"
            };

            var response = AdminRepo.SaveUser(u);
            Assert.IsFalse(response);

        }

        [TestMethod]
        public void Admin_Template_LoadTemplateList()
        {
            var response = AdminRepo.LoadTemplateList();
            Assert.IsNotNull(response);
        }

        [TestMethod]
        public void Admin_Template_SaveNewMarkupTemplate()
        {

            var temp = new PriceMarkupTemplate
            {
                Id = 0,
                Name = "testTemplate-Inserted",
                Description = "testTemplate-Description-Inserted",
                Notes = "testNotes",
                TemplateType = TemplateType.Markup,
                Rules = new List<PriceMarkupRule>
                {
                    new PriceMarkupRule{ Id = 0, DollarRangeLower = 1M, DollarRangeUpper = 2M, PercentLimitLower = 3M, PercentLimitUpper=4M}
                }
            };
            var response = AdminRepo.SaveTemplate(temp);
            Assert.IsTrue(response);


            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                var template = db.Single<DB.Template>(t => t.Name == "testTemplate-Inserted");
                Assert.AreEqual(template.Description, "testTemplate-Description-Inserted");
                Assert.AreEqual(template.TemplateType, (int)TemplateType.Markup);
            }

        }

        [TestMethod]
        public void Admin_Template_SaveNewRoundingTemplate()
        {

            var temp = new PriceRoundingTemplate
            {
                Id = 0,
                Name = "testTemplate-Inserted",
                Description = "testTemplate-Description-Inserted",
                Notes = "testNotes",
                TemplateType = TemplateType.Rounding,
                Rules = new List<PriceRoundingRule>
                {
                    new PriceRoundingRule{ Id = 0, DollarRangeLower = 1M, DollarRangeUpper=10M, RoundingType = RoundingType.RoundDown, ValueChange=30M}
                }
            };
            var response = AdminRepo.SaveTemplate(temp);
            Assert.IsTrue(response);


            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                var template = db.Single<DB.Template>(t => t.Name == "testTemplate-Inserted");
                Assert.AreEqual(template.Description, "testTemplate-Description-Inserted");
                Assert.AreEqual(template.TemplateType, (int)TemplateType.Rounding);
            }

        }

        [TestMethod]
        public void Admin_Template_UpdateExistingMarkupTemplate()
        {

            var temp = new PriceMarkupTemplate
            {
                Id = TemplateIdToTest,
                Name = "testTemplate-Updated",
                Description = "testTemplateDescription-Updated",
                Notes = "testNotes",
                TemplateType = TemplateType.Markup,
                Rules = new List<PriceMarkupRule>
                {
                    new PriceMarkupRule{ Id = 0, DollarRangeLower = 1M, DollarRangeUpper = 2M, PercentLimitLower = 3M, PercentLimitUpper=4M}
                }
            };

            var response = AdminRepo.SaveTemplate(temp);
            Assert.IsTrue(response);

            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                var template = db.Single<DB.Template>(t => t.Name == "testTemplate-Updated");
                Assert.AreEqual(template.Description, "testTemplateDescription-Updated");
                Assert.AreEqual(template.TemplateType, (int)TemplateType.Markup);
            }
        }

        [TestMethod]
        public void Admin_Template_UpdateExistingOptimizationTemplate()
        {

            var temp = new PriceOptimizationTemplate
            {
                Id = TemplateIdToTest,
                Name = "testTemplate-Updated",
                Description = "testTemplateDescription-Updated",
                Notes = "testNotes",
                TemplateType = TemplateType.Optimization,
                Rules = new List<PriceOptimizationRule>
                {
                    new PriceOptimizationRule{ Id = 0, DollarRangeLower = 1M, DollarRangeUpper = 2M, PercentChange = 20M}
                }
            };

            var response = AdminRepo.SaveTemplate(temp);
            Assert.IsTrue(response);

            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                var template = db.Single<DB.Template>(t => t.Name == "testTemplate-Updated");
                Assert.AreEqual(template.Description, "testTemplateDescription-Updated");
                Assert.AreEqual(template.TemplateType, (int)TemplateType.Optimization);
            }
        }



    }

}
