using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Linq;
using Newtonsoft.Json;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.SqlServer;
using ENT = APLPX.Entity;
using DB = APLPX.Client.Sql.Models;
using APLPX.Client.Sql.Models;

namespace APLPX.Client.Tests
{
    [TestClass, Ignore]
    public class Migrate
    {

        int minAID = 1; int maxAID = 2;
        int minInt = 100; int maxInt = 100000;
        int minSales = 1000; int maxSales = 750000;
        int minDays = 1; int maxDays = 150;
        int minMarkup = 1; int maxMarkup = 100;
        int minMovement = 50; int maxMovement = 2000;
        int minFilter = 1000; int maxFilter = 2042;
        int minFilterType = 1; int maxFilterType = 11;

        int minFilterCount = 0; int maxFilterCount = 2000;


        Random r = new Random();
        public OrmLiteConnectionFactory DBConnectionFactory { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            DBConnectionFactory = new OrmLiteConnectionFactory(ConfigurationManager.AppSettings["localConnectionString"], SqlServerDialect.Provider);
            //Turn on camel case naming convention
            OrmLiteConfig.DialectProvider.NamingStrategy = new OrmLiteNamingStrategyBase();

        }

        [TestMethod, Ignore]
        [TestCategory("MigrateSql")]
        public void Step1_Drop_Analytics()
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                //if (db.TableExists<AnalyticPriceList>()) db.DropTable<AnalyticPriceList>();

                if (db.TableExists<DB.AnalyticDriver>()) db.DropTable<DB.AnalyticDriver>();
                //if (db.TableExists<AnalyticFilter>()) db.DropTable<AnalyticFilter>();
                if (db.TableExists<DB.Analytic>()) db.DropTable<DB.Analytic>();


            }


        }


        [TestCategory("MigrateSql")]
        [TestMethod, Ignore]
        public void Step2_DropTypes()
        {


            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                if (db.TableExists<DB.FilterValue>()) db.DropTable<DB.FilterValue>();
                //if (db.TableExists<UserRole>()) db.DropTable<UserRole>();
                if (db.TableExists<DB.PriceList>()) db.DropTable<DB.PriceList>();
                if (db.TableExists<DB.FilterType>()) db.DropTable<DB.FilterType>();
                if (db.TableExists<DB.DriverType>()) db.DropTable<DB.DriverType>();


            }
        }

        [TestCategory("MigrateSql")]
        [TestMethod, Ignore]
        public void Step3_DropUsersAndModules()
        {

            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {

                if (db.TableExists<DB.Role>()) db.DropTable<DB.Role>();
                if (db.TableExists<DB.User>()) db.DropTable<DB.User>();
                if (db.TableExists<DB.ModuleFeature>()) db.DropTable<DB.ModuleFeature>();
                if (db.TableExists<DB.Module>()) db.DropTable<DB.Module>();
            }
        }

        [TestCategory("MigrateSql")]
        [TestMethod, Ignore]
        public void Step4_DropTemplates()
        {

            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {

                if (db.TableExists<DB.RoundingTemplate>()) db.DropTable<DB.RoundingTemplate>();
                if (db.TableExists<DB.OptimizationTemplate>()) db.DropTable<DB.OptimizationTemplate>();
                if (db.TableExists<DB.MarkupTemplate>()) db.DropTable<DB.MarkupTemplate>();
                if (db.TableExists<DB.Template>()) db.DropTable<DB.Template>();
                if (db.TableExists<DB.TemplateType>()) db.DropTable<DB.TemplateType>();
                if (db.TableExists<DB.RoundingType>()) db.DropTable<DB.RoundingType>();

            }
        }


        [TestCategory("MigrateSql")]
        [TestMethod]
        public void Step8_SeedAnalytics()
        {
            int minDriver = 1, maxDriver = 3;
            int minGroup = 1; int maxGroup = 15;
            int minDecimal = 1500; int maxDecimal = 500000;
            int minFilter = 1000; int maxFilter = 2042;
            int minPriceList = 1; int maxPriceList = 12;
            Random r = new Random();
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                db.DeleteAll<AnalyticDriver>();
                db.DeleteAll<Analytic>();
                //db.CreateTable<Analytic>();
                var filters = db.Select<DB.FilterValue>().Where(a => a.FilterTypeId == 5);
                var filterGroups = (from p in filters
                                    group p by p.FilterTypeId into g
                                    select new DB.FilterGroup
                                    {
                                        Name = ((ENT.FilterType)g.Key).ToString(),
                                        Filters = g.Select(fv => new DB.Filter
                                        {
                                            Value = fv.Value
                                        }).ToArray()
                                    }).ToArray();


                db.Insert(new DB.Analytic
                {
                    //AnalyticId = 1,
                    Name = "Admin - Everyday - All Filters - Movement Only",
                    Description = "Admin - Everyday - All Filters - Movement Only",
                    FolderID = 76,
                    Notes = "Optional notes...",
                    CreateTS = DateTime.UtcNow,
                    OwnerId = 2,
                    //ProductFilters = Enumerable.Repeat(0,500).Select( fv => new DB.FilterValue
                    //                {
                    //                    BatchID = 0,
                    //                    FilterTypeId = r.Next(minFilterType, maxFilterType),
                    //                    Value = string.Format("Code{0}", r.Next(minFilter, maxFilter)),
                    //                    ValueDescription = string.Format("Description{0}", r.Next(minFilter, maxFilter))

                    //                }).ToArray(),
                    ProductFilters = filterGroups,
                    PriceLists = Enumerable.Repeat(0, 2).Select(i => r.Next(minPriceList, maxPriceList)).ToArray(),
                    ClusterType = "D"



                });




                db.Insert(new DB.Analytic
                {
                    //AnalyticId = 2,
                    Name = "Admin - Everyday - Movement & Markup",
                    Description = "Admin - Everyday - Movement & Markup",

                    FolderID = 76,
                    Notes = "Optional notes...",
                    CreateTS = DateTime.UtcNow,
                    OwnerId = 1,
                    ProductFilters = filterGroups,
                    PriceLists = Enumerable.Repeat(0, 2).Select(i => r.Next(minPriceList, maxPriceList)).ToArray()
                    ,
                    ClusterType = "D"
                });


                //db.CreateTable<AnalyticFilter>();
                //db.Insert(new AnalyticFilter { AnalyticId = 1, FilterId = 1002});//FilterName = "2 = 5% off List" 
                //db.Insert(new AnalyticFilter { AnalyticId = 1, FilterId = 1003 });//FilterName = "3 = 10% off List"
                //db.Insert(new AnalyticFilter { AnalyticId = 1, FilterId = 1004 });//FilterName = "4 = 15% off List"

                //db.CreateTable<AnalyticPriceList>();
                //db.Insert(new AnalyticPriceList { AnalyticId = 1, PriceListId = 1 });
                //db.Insert(new AnalyticPriceList { AnalyticId = 1, PriceListId = 2 });


                //db.CreateTable<AnalyticDriver>();



                var drivers = Enumerable.Range(1, r.Next(minDriver, maxDriver)).Select(i => new DB.AnalyticDriver
                {
                    AnalyticId = db.Scalar<Analytic, int>(x => ServiceStack.OrmLite.Sql.Max(x.AnalyticId)),
                    DriverId = r.Next(minDriver, maxDriver),
                    Encoding = new[]
                                {
                                    new DB.Encoding{ 
                                        Type = "Groups", 
                                        Definition  = new DB.EncodingDefinition[] { 
                                            new DB.EncodingDefinition{ GroupNum = 1, RangeMin = -99999999999, RangeMax = 8, },
                                            new DB.EncodingDefinition{ GroupNum = 2, RangeMin = 100, RangeMax = -99999999999}
                                        }
                                    }
                                },
                    EncodingResult = new[] 
                                    {
                                        new DB.EncodingResult
                                        {
                                            Type = "Groups",
                                            Result = 
                                                    Enumerable.Range(1, r.Next(minGroup, maxGroup))
                                                        .Select( k  => 
                                                            new DB.EncodingResultDefinition
                                                            {
                                                                GroupNum = k,
                                                                SalesAmount = r.Next(minDecimal,maxDecimal),
                                                                SalesQuantity = r.Next(minDecimal,maxDecimal),
                                                                ProductCount = r.Next(minInt, maxInt)
                                                            }
                                                        ).ToArray()

                                        }
                                   }




                }).ToArray();



                db.InsertAll<DB.AnalyticDriver>(drivers);
                //db.Save(driver1, driver2);

            }


        }

        [TestMethod]
        [TestCategory("MigrateSql")]
        public void Step5_SeedUserAndModules()
        {



            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {

                //Users & UserRole
                //db.CreateTable<Role>();
                db.InsertAll(new[]
                {                        
                    new DB.Role { RoleId = 1, Name = "Administrator"},
                    new DB.Role { RoleId = 2, Name = "Pricing Analyst"},
                    new DB.Role { RoleId = 3, Name = "Approver"},
                    new DB.Role { RoleId = 4, Name = "Reviewer"}
                });

                //db.CreateTable<User>();
                db.InsertAll(new[]
                {
                    new DB.User{ 
                        Login = "admin", 
                        Password = "password",                                  
                        Roles = new [] {(int)ENT.UserRoleType.AplUserRoleAdministrator}, 
                        FirstName = "Bon", LastName="Jovi", 
                        FolderList = new DB.Folder[]{ 
                            new DB.Folder { FolderId = 76, Name="Folder76"}, 
                            new DB.Folder { FolderId = 1, Name = "Recent"}, 
                            new DB.Folder { FolderId = 2, Name = "Shared" }
                        },
                        CreateTS = DateTime.UtcNow},
                    new DB.User{ 
                        Login = "analyst", 
                        Password = "password",      
                        Roles = new [] {(int) ENT.UserRoleType.AplUserRolePricingAnalyst}, 
                        FirstName = "Van", LastName = "Halen", 
                        FolderList = new DB.Folder[]{ 
                            new DB.Folder { FolderId = 76, Name="Folder76"}, 
                            new DB.Folder { FolderId = 1, Name = "Recent"}, 
                            new DB.Folder { FolderId = 2, Name = "Shared" }
                        }, 
                        CreateTS = DateTime.UtcNow}
                });


                //ModulesByRole
                //db.CreateTable<Module>();
                db.InsertAll(new[]
                    {
                        new DB.Module 
                        { 
                            Id = 1, 
                            Name = "Planning",
                            Description = "Planning",
                            Roles = new [] {1,2,3,4},
                        },
                        new DB.Module
                        {
                            Id = 2,
                            Name = "Tracking",
                            Description = "Tracking",
                            Roles = new [] {1,2,3,4}
                        },
                        new DB.Module
                        {
                            Id = 3,
                            Name = "Reporting",
                            Description = "Reporting",
                            Roles = new [] {1,2,3,4}
   
                        },
                        new DB.Module
                        {
                            Id = 4,
                            Name = "Administration",
                            Description = "Administration",
                            Roles = new [] {1,2,3,4}
   
                        }
                    });



                //ModuleFeatureByRole
                //db.CreateTable<ModuleFeature>();
                db.InsertAll(new[]
                    {
                        new DB.ModuleFeature { Id = 1, ModuleId = 1, ModuleName = "Planning", Name = "Home", Description = "Home", Roles = new [] {1,2,3,4}},
                        new DB.ModuleFeature { Id = 2, ModuleId = 1, ModuleName = "Planning",  Name = "Analytics", Description = "Analytics", Roles = new []{1,2,3,4} },
                        new DB.ModuleFeature { Id = 3, ModuleId = 1, ModuleName = "Planning",  Name = "PricingEveryday", Description = "Pricing Everyday", Roles = new []{1,2,3,4}},
                        new DB.ModuleFeature { Id = 4, ModuleId = 1, ModuleName = "Planning", Name = "PricingKits", Description = "Pricing Kits", Roles = new [] {1,2,3,4}},
                        new DB.ModuleFeature { Id = 5, ModuleId = 1, ModuleName = "Planning", Name = "PricingPromo", Description = "Pricing Promo", Roles = new [] {1,2,3,4}}
                    });
            }
        }


        [TestMethod]
        [TestCategory("MigrateSql")]
        public void Step6_SeedTemplates()
        {



            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {

                db.DeleteAll<RoundingType>();
                //db.DeleteAll<TemplateType>();
                db.DeleteAll<Template>();


                //db.CreateTable<RoundingType>();
                db.InsertAll(new[]
                {                        
                    new DB.RoundingType { Id = 1, Name = "RoundUp"},
                    new DB.RoundingType { Id = 2, Name = "RoundDown"},
                    new DB.RoundingType { Id = 3, Name = "RoundNear"}
                    
                });

                //db.CreateTable<TemplateType>();
                //db.InsertAll(new[]
                //{
                //    new DB.TemplateType { Id = 1, Name = "Optimization" },
                //    new DB.TemplateType { Id = 2, Name = "Rounding"},
                //    new DB.TemplateType { Id = 3, Name = "Markup"}
                //});

                //db.CreateTable<Template>();
                db.Insert<DB.Template>
                    (
                        new DB.Template[] {
                        
                        new DB.Template 
                        {
                            TemplateType = (int) ENT.TemplateType.Rounding,
                                Name = "Rounding Template 1",
                                Description = "Rounding Template 1 Desc",
                                Notes = " Rounding Template 1 Notes",
                                CreateTS = DateTime.UtcNow,
                                Rules = JsonConvert.SerializeObject( new List<DB.PriceRoundingRule>()
                                {
                                    new DB.PriceRoundingRule 
                                    {
                                        MinValue = .55M,
                                        MaxValue = 5.55M,
                                        RoundingType = (int) ENT.RoundingType.RoundDown,
                                        ValueChange = 555.55M
                                    },
                                    new DB.PriceRoundingRule
                                    {
                                        MinValue = .66M,
                                        MaxValue = 6.66M,
                                        RoundingType = (int) ENT.RoundingType.RoundNear,
                                        ValueChange = 666.66M
                                    },
                                    new DB.PriceRoundingRule 
                                    {
                                        MinValue = .77M,
                                        MaxValue = 7.77M,
                                        RoundingType = (int) ENT.RoundingType.RoundUp,
                                        ValueChange = 777.77M
                                    }
                                })
                        
                        
                        },
                        new Template
                        {
                            //Id = 2, 
                            TemplateType = (int) ENT.TemplateType.Optimization,
                            Name = "Optimization Template 2",
                            Description = "Optimization Template 2 Desc",
                            Notes = " Optimization Template 2 Notes",
                            CreateTS = DateTime.UtcNow,
                            Rules = JsonConvert.SerializeObject( new List<DB.OptimizationTemplateRule>()
                            {
                                new DB.OptimizationTemplateRule 
                                {
                                    MinValue = .55M,
                                    MaxValue = 5.55M,
                                    PercentChange = 55.00M,

                                },
                                new DB.OptimizationTemplateRule 
                                {
                                    MinValue = .66M,
                                    MaxValue = 6.66M,
                                    PercentChange = 55.00M,
                                },
                                new DB.OptimizationTemplateRule 
                                {
                                    MinValue = .77M,
                                    MaxValue = 7.77M,
                                    PercentChange = 55.00M,
                                }
                            })

                        }

                        
                        }
                    );




                //db.CreateTable<RoundingTemplate>();
                //db.InsertAll(new[]
                //        {
                //            new RoundingTemplate 
                //            { 
                //                //Id = 1, 
                //                TemplateType = (int) ENT.TemplateType.Rounding,
                //                Name = "Rounding Template 1",
                //                Description = "Rounding Template 1 Desc",
                //                Notes = " Rounding Template 1 Notes",
                //                DateCreated = DateTime.UtcNow,
                //                Rules = new List<RoundingTemplateRule>()
                //                {
                //                    new RoundingTemplateRule 
                //                    {
                //                        MinValue = .55M,
                //                        MaxValue = 5.55M,
                //                        RoundingType = (int) ENT.RoundingType.RoundDown,
                //                        RoundingValue = 555.55M
                //                    },
                //                    new RoundingTemplateRule 
                //                    {
                //                        MinValue = .66M,
                //                        MaxValue = 6.66M,
                //                        RoundingType = (int) ENT.RoundingType.RoundNear,
                //                        RoundingValue = 666.66M
                //                    },
                //                    new RoundingTemplateRule 
                //                    {
                //                        MinValue = .77M,
                //                        MaxValue = 7.77M,
                //                        RoundingType = (int) ENT.RoundingType.RoundUp,
                //                        RoundingValue = 777.77M
                //                    }
                //                }
                //            }
                //        });


                //db.CreateTable<OptimizationTemplate>();
                //db.InsertAll(new[]
                //    {
                //        new OptimizationTemplate
                //        {
                //            //Id = 2, 
                //            TemplateType = (int) ENT.TemplateType.Optimization,
                //            Name = "Optimization Template 2",
                //            Description = "Optimization Template 2 Desc",
                //            Notes = " Optimization Template 2 Notes",
                //            DateCreated = DateTime.UtcNow,
                //            Rules = new List<OptimizationTemplateRule>()
                //            {
                //                new OptimizationTemplateRule 
                //                {
                //                    MinValue = .55M,
                //                    MaxValue = 5.55M,
                //                    OptimizationPercentage = 55.00M,

                //                },
                //                new OptimizationTemplateRule 
                //                {
                //                    MinValue = .66M,
                //                    MaxValue = 6.66M,
                //                    OptimizationPercentage = 55.00M,
                //                },
                //                new OptimizationTemplateRule 
                //                {
                //                    MinValue = .77M,
                //                    MaxValue = 7.77M,
                //                    OptimizationPercentage = 55.00M,
                //                }
                //            }

                //        }
                //    });


                //db.CreateTable<MarkupTemplate>();
                //db.InsertAll(new[]
                //        {
                //            new MarkupTemplate
                //            {
                //                //Id = 3, 
                //                TemplateType = (int) ENT.TemplateType.Markup,
                //                Name = "Markup Template 3",
                //                Description = "Markup Template 3 Desc",
                //                Notes = "Markup Template 3 Notes",
                //                DateCreated = DateTime.UtcNow,
                //                Rules = new List<MarkupTemplateRule>()
                //                {
                //                    new MarkupTemplateRule 
                //                    {
                //                        MinValue = .55M,
                //                        MaxValue = 5.55M,
                //                        MinMarkup = 55.00M,
                //                        MaxMarkup = 55.00M
                //                    },
                //                    new MarkupTemplateRule 
                //                    {
                //                        MinValue = .66M,
                //                        MaxValue = 6.66M,
                //                        MinMarkup = 55.00M,
                //                        MaxMarkup = 55.00M
                //                    },
                //                    new MarkupTemplateRule 
                //                    {
                //                        MinValue = .77M,
                //                        MaxValue = 7.77M,
                //                        MinMarkup = 55.00M,
                //                        MaxMarkup = 55.00M
                //                    }
                //                }
                //            }
                //        });




            }
        }

        [TestMethod]
        [TestCategory("MigrateSql")]
        public void Step4a_SeedFilters()
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                db.DeleteAll<DB.DriverType>();
                db.DeleteAll<DB.FilterValue>();
                db.DeleteAll<DB.FilterType>();
                db.DeleteAll<DB.PriceList>();
                db.DeleteAll<ETLBatch>();
                //db.CreateTable<FilterType>();

                int i = 0;
                var id = db.Insert(new ETLBatch { Name = string.Format("Initial Load{0}", ++i), TotalImportRecords = 11, TotalRecordsApplied = 11 });
                var s = string.Format("Initial Load{0}", i);
                var IdToTest = db.Single<DB.ETLBatch>(u => u.Name == s);
                //TODO: "Ent FilterType for each"
                db.Insert(new DB.FilterType { FilterTypeId = 1, KeyName = "DiscountType", BatchID = IdToTest.BatchID });
                db.Insert(new DB.FilterType { FilterTypeId = 2, KeyName = "Hierarchy", BatchID = IdToTest.BatchID });
                db.Insert(new DB.FilterType { FilterTypeId = 3, KeyName = "InventoryCategoryLine", BatchID = IdToTest.BatchID });

                db.Insert(new DB.FilterType { FilterTypeId = 4, KeyName = "InventoryStatus", BatchID = IdToTest.BatchID });
                db.Insert(new DB.FilterType { FilterTypeId = 5, KeyName = "Location", BatchID = IdToTest.BatchID });
                db.Insert(new DB.FilterType { FilterTypeId = 6, KeyName = "PackageType", BatchID = IdToTest.BatchID });
                db.Insert(new DB.FilterType { FilterTypeId = 7, KeyName = "PricingType", BatchID = IdToTest.BatchID });
                db.Insert(new DB.FilterType { FilterTypeId = 8, KeyName = "ProductIntroduced", BatchID = IdToTest.BatchID });
                db.Insert(new DB.FilterType { FilterTypeId = 9, KeyName = "ProductType", BatchID = IdToTest.BatchID });
                db.Insert(new DB.FilterType { FilterTypeId = 10, KeyName = "StockSupplyClassification", BatchID = IdToTest.BatchID });
                db.Insert(new DB.FilterType { FilterTypeId = 11, KeyName = "VendorCode", BatchID = IdToTest.BatchID });

                //db.CreateTable<DriverType>();
                db.InsertAll(new[]
                {                        
                    new DB.DriverType { DriverId = 1, Name = "Movement"},
                    new DB.DriverType { DriverId = 2, Name = "DaysOnHand"},
                    new DB.DriverType { DriverId = 3, Name = "Margin"}      
                });







                //db.CreateTable<Filter>();
                //DiscountType
                db.Insert(new FilterValue { Value = "01", ValueDescription = "1 = Not On Sale", FilterTypeId = (int)(ENT.FilterType.DiscountType), BatchID = 0 });
                db.Insert(new FilterValue { Value = "02", ValueDescription = "2 = 5% off List", FilterTypeId = (int)(ENT.FilterType.DiscountType) });
                db.Insert(new FilterValue { Value = "03", ValueDescription = "3 = 10% off List", FilterTypeId = (int)(ENT.FilterType.DiscountType) });
                db.Insert(new FilterValue { Value = "04", ValueDescription = "4 = 15% off List", FilterTypeId = (int)(ENT.FilterType.DiscountType) });
                db.Insert(new FilterValue { Value = "05", ValueDescription = "5 = 20% off List", FilterTypeId = (int)(ENT.FilterType.DiscountType) });
                db.Insert(new FilterValue { Value = "06", ValueDescription = "6 = 25% off List", FilterTypeId = (int)(ENT.FilterType.DiscountType) });
                db.Insert(new FilterValue { Value = "07", ValueDescription = "7 = 30% off List", FilterTypeId = (int)(ENT.FilterType.DiscountType) });
                db.Insert(new FilterValue { Value = "08", ValueDescription = "8 = 35% off List", FilterTypeId = (int)(ENT.FilterType.DiscountType) });
                db.Insert(new FilterValue { Value = "09", ValueDescription = "9 = 40% off List", FilterTypeId = (int)(ENT.FilterType.DiscountType) });
                db.Insert(new FilterValue { Value = "10", ValueDescription = "10 = 50% off List", FilterTypeId = (int)(ENT.FilterType.DiscountType) });
                db.Insert(new FilterValue { Value = "11", ValueDescription = "11 = Above 50%", FilterTypeId = (int)(ENT.FilterType.DiscountType) });
                db.Insert(new FilterValue { Value = "00", ValueDescription = "MISSING", FilterTypeId = (int)(ENT.FilterType.DiscountType) });

                //Hierarchy?
                db.Insert(new FilterValue { Value = "A1", ValueDescription = "Air &amp; Fuel Delivery", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "B1", ValueDescription = "Bed Mats &amp; Tonneau Covers", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "B2", ValueDescription = "Books", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "B3", ValueDescription = "Brake Systems", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "B4", ValueDescription = "Bumpers &amp; Hardware", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "C1", ValueDescription = "Car Care &amp; Paint", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "C2", ValueDescription = "Chassis &amp; Suspension", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "C3", ValueDescription = "Convertible Tops &amp; Accessories", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "C4", ValueDescription = "Cooling &amp; Heating", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "D1", ValueDescription = "Decals", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "D2", ValueDescription = "Door Handles &amp; Hardware", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "D3", ValueDescription = "Drivetrain", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "E1", ValueDescription = "Emblems", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "E2", ValueDescription = "Engines &amp; Components", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "E3", ValueDescription = "Exhaust", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "E6", ValueDescription = "Exhaust", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "F1", ValueDescription = "Fasteners &amp; Hardware", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "F2", ValueDescription = "Fittings &amp; Hoses", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "G1", ValueDescription = "Gaskets &amp; Seals", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "G2", ValueDescription = "Gauges &amp; Accessories", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "G3", ValueDescription = "Gifts &amp; Apparel", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "I1", ValueDescription = "Ignition &amp; Electrical", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "I2", ValueDescription = "Interior Accessories", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "K1", ValueDescription = "Keys &amp; Locks", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "L1", ValueDescription = "Lamps &amp; Lenses", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "M1", ValueDescription = "Mirrors &amp; Hardware", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "M3", ValueDescription = "Mobile Electronics", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "M4", ValueDescription = "Moldings", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "O1", ValueDescription = "Oils, Fluids &amp; Sealer", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "S1", ValueDescription = "Sheet Metal &amp; Body Panels", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "T1", ValueDescription = "Tools &amp; Shop Equipment", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "T2", ValueDescription = "Trunk Panels &amp; Accessories", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "V1", ValueDescription = "Vinyl Tops", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "W1", ValueDescription = "Wheels &amp; Accessories", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "W2", ValueDescription = "Window", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "W3", ValueDescription = "Windshield Washer", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });
                db.Insert(new FilterValue { Value = "00", ValueDescription = "MISSING", FilterTypeId = (int)(ENT.FilterType.Hierarchy), BatchID = 0 });

                //Inventory Catalog Line
                db.Insert(new FilterValue { Value = "A", ValueDescription = "Chevrolet Chevelle", FilterTypeId = (int)(ENT.FilterType.InventoryCatalogLine), BatchID = 0 });
                db.Insert(new FilterValue { Value = "B", ValueDescription = "Chevrolet Monte Carlo", FilterTypeId = (int)(ENT.FilterType.InventoryCatalogLine), BatchID = 0 });
                db.Insert(new FilterValue { Value = "C", ValueDescription = "Chevrolet El Camino", FilterTypeId = (int)(ENT.FilterType.InventoryCatalogLine), BatchID = 0 });
                db.Insert(new FilterValue { Value = "D", ValueDescription = "Pontiac &quot;A&quot; Body", FilterTypeId = (int)(ENT.FilterType.InventoryCatalogLine), BatchID = 0 });
                db.Insert(new FilterValue { Value = "E", ValueDescription = "Pontiac Grand Prix", FilterTypeId = (int)(ENT.FilterType.InventoryCatalogLine), BatchID = 0 });
                db.Insert(new FilterValue { Value = "F", ValueDescription = "Buick Special-Skylark", FilterTypeId = (int)(ENT.FilterType.InventoryCatalogLine), BatchID = 0 });
                db.Insert(new FilterValue { Value = "G", ValueDescription = "Cadillac DeVille-Calais", FilterTypeId = (int)(ENT.FilterType.InventoryCatalogLine), BatchID = 0 });
                db.Insert(new FilterValue { Value = "I", ValueDescription = "Cadillac Series 62", FilterTypeId = (int)(ENT.FilterType.InventoryCatalogLine), BatchID = 0 });
                db.Insert(new FilterValue { Value = "N", ValueDescription = "Cadillac Fleetwood", FilterTypeId = (int)(ENT.FilterType.InventoryCatalogLine), BatchID = 0 });
                db.Insert(new FilterValue { Value = "O", ValueDescription = "Cadillac Eldorado", FilterTypeId = (int)(ENT.FilterType.InventoryCatalogLine), BatchID = 0 });
                db.Insert(new FilterValue { Value = "P", ValueDescription = "Oldsmobile Cutlass &amp; 442", FilterTypeId = (int)(ENT.FilterType.InventoryCatalogLine), BatchID = 0 });
                db.Insert(new FilterValue { Value = "V", ValueDescription = "Buick Riviera", FilterTypeId = (int)(ENT.FilterType.InventoryCatalogLine), BatchID = 0 });
                db.Insert(new FilterValue { Value = "X", ValueDescription = "Pontiac Catalina", FilterTypeId = (int)(ENT.FilterType.InventoryCatalogLine), BatchID = 0 });
                db.Insert(new FilterValue { Value = "Y", ValueDescription = "Pontiac Bonneville", FilterTypeId = (int)(ENT.FilterType.InventoryCatalogLine), BatchID = 0 });
                db.Insert(new FilterValue { Value = "00", ValueDescription = "MISSING", FilterTypeId = (int)(ENT.FilterType.InventoryCatalogLine), BatchID = 0 });

                //Inventory Status
                db.Insert(new FilterValue { Value = "A", ValueDescription = "Available", FilterTypeId = (int)(ENT.FilterType.InventoryStatus), BatchID = 0 });
                db.Insert(new FilterValue { Value = "C", ValueDescription = "Component, DoNotSell", FilterTypeId = (int)(ENT.FilterType.InventoryStatus), BatchID = 0 });
                db.Insert(new FilterValue { Value = "D", ValueDescription = "Discontinued", FilterTypeId = (int)(ENT.FilterType.InventoryStatus), BatchID = 0 });
                db.Insert(new FilterValue { Value = "N", ValueDescription = "Not Yet Available", FilterTypeId = (int)(ENT.FilterType.InventoryStatus), BatchID = 0 });
                db.Insert(new FilterValue { Value = "T", ValueDescription = "Not Available", FilterTypeId = (int)(ENT.FilterType.InventoryStatus), BatchID = 0 });
                db.Insert(new FilterValue { Value = "U", ValueDescription = "Unlimited Supply", FilterTypeId = (int)(ENT.FilterType.InventoryStatus), BatchID = 0 });
                db.Insert(new FilterValue { Value = "V", ValueDescription = "Drop Ship", FilterTypeId = (int)(ENT.FilterType.InventoryStatus), BatchID = 0 });
                db.Insert(new FilterValue { Value = "00", ValueDescription = "MISSING", FilterTypeId = (int)(ENT.FilterType.InventoryStatus), BatchID = 0 });

                //Product Introduction Date
                db.Insert(new FilterValue { Value = "D07", ValueDescription = "From 0 to 7 days", FilterTypeId = (int)(ENT.FilterType.ProductIntroduced), BatchID = 0 });
                db.Insert(new FilterValue { Value = "D30", ValueDescription = "From 08 to 30 days", FilterTypeId = (int)(ENT.FilterType.ProductIntroduced), BatchID = 0 });
                db.Insert(new FilterValue { Value = "D60", ValueDescription = "From 31 to 60 days", FilterTypeId = (int)(ENT.FilterType.ProductIntroduced), BatchID = 0 });
                db.Insert(new FilterValue { Value = "D90", ValueDescription = "From 61 to 90 days", FilterTypeId = (int)(ENT.FilterType.ProductIntroduced), BatchID = 0 });
                db.Insert(new FilterValue { Value = "D90+", ValueDescription = "Older than 90 days", FilterTypeId = (int)(ENT.FilterType.ProductIntroduced), BatchID = 0 });
                db.Insert(new FilterValue { Value = "D00", ValueDescription = "MISSING", FilterTypeId = (int)(ENT.FilterType.ProductIntroduced), BatchID = 0 });

                //Product Type
                db.Insert(new FilterValue { Value = "1 ", ValueDescription = "resto-parts", FilterTypeId = (int)(ENT.FilterType.ProductType), BatchID = 0 });
                db.Insert(new FilterValue { Value = "1A", ValueDescription = "restokits-disc=yes", FilterTypeId = (int)(ENT.FilterType.ProductType), BatchID = 0 });
                db.Insert(new FilterValue { Value = "2 ", ValueDescription = "Hi Performance", FilterTypeId = (int)(ENT.FilterType.ProductType), BatchID = 0 });
                db.Insert(new FilterValue { Value = "3 ", ValueDescription = "Aftermarket", FilterTypeId = (int)(ENT.FilterType.ProductType), BatchID = 0 });
                db.Insert(new FilterValue { Value = "4 ", ValueDescription = "Apparel", FilterTypeId = (int)(ENT.FilterType.ProductType), BatchID = 0 });
                db.Insert(new FilterValue { Value = "5 ", ValueDescription = "Gift Certificates", FilterTypeId = (int)(ENT.FilterType.ProductType), BatchID = 0 });
                db.Insert(new FilterValue { Value = "6 ", ValueDescription = "Manuals &amp; Literature", FilterTypeId = (int)(ENT.FilterType.ProductType), BatchID = 0 });
                db.Insert(new FilterValue { Value = "7 ", ValueDescription = "Video &amp; Software", FilterTypeId = (int)(ENT.FilterType.ProductType), BatchID = 0 });
                db.Insert(new FilterValue { Value = "8 ", ValueDescription = "Loyalty Points", FilterTypeId = (int)(ENT.FilterType.ProductType), BatchID = 0 });
                db.Insert(new FilterValue { Value = "9 ", ValueDescription = "restokits-disc=no", FilterTypeId = (int)(ENT.FilterType.ProductType), BatchID = 0 });
                db.Insert(new FilterValue { Value = "9A", ValueDescription = "Ptof-aKitorSubOnly", FilterTypeId = (int)(ENT.FilterType.ProductType), BatchID = 0 });
                db.Insert(new FilterValue { Value = "10", ValueDescription = "Unknown", FilterTypeId = (int)(ENT.FilterType.ProductType), BatchID = 0 });
                db.Insert(new FilterValue { Value = "11", ValueDescription = "resto-parts-pairs", FilterTypeId = (int)(ENT.FilterType.ProductType), BatchID = 0 });
                db.Insert(new FilterValue { Value = "G ", ValueDescription = "Gift Card", FilterTypeId = (int)(ENT.FilterType.ProductType), BatchID = 0 });
                db.Insert(new FilterValue { Value = "X ", ValueDescription = "Discontinued", FilterTypeId = (int)(ENT.FilterType.ProductType), BatchID = 0 });
                db.Insert(new FilterValue { Value = "Z ", ValueDescription = "Box &amp; Packing Supply", FilterTypeId = (int)(ENT.FilterType.ProductType), BatchID = 0 });
                db.Insert(new FilterValue { Value = "00", ValueDescription = "MISSING", FilterTypeId = (int)(ENT.FilterType.ProductType), BatchID = 0 });

                //Stock Supply Classification
                db.Insert(new FilterValue { Value = "A", ValueDescription = "Bulk (6+ months)", FilterTypeId = (int)(ENT.FilterType.StockSupplyClassification), BatchID = 0 });
                db.Insert(new FilterValue { Value = "B", ValueDescription = "Ext Stock (30-90 days)", FilterTypeId = (int)(ENT.FilterType.StockSupplyClassification), BatchID = 0 });
                db.Insert(new FilterValue { Value = "C", ValueDescription = "Kit or Sub (OPG Mfg)", FilterTypeId = (int)(ENT.FilterType.StockSupplyClassification), BatchID = 0 });
                db.Insert(new FilterValue { Value = "D", ValueDescription = "Kit or Sub (Std)", FilterTypeId = (int)(ENT.FilterType.StockSupplyClassification), BatchID = 0 });
                db.Insert(new FilterValue { Value = "E", ValueDescription = "Min. Stock (7 days MAX)", FilterTypeId = (int)(ENT.FilterType.StockSupplyClassification), BatchID = 0 });
                db.Insert(new FilterValue { Value = "F", ValueDescription = "OPG Manufactured", FilterTypeId = (int)(ENT.FilterType.StockSupplyClassification), BatchID = 0 });
                db.Insert(new FilterValue { Value = "G", ValueDescription = "Std. Stock (15-30 days)", FilterTypeId = (int)(ENT.FilterType.StockSupplyClassification), BatchID = 0 });
                db.Insert(new FilterValue { Value = "00", ValueDescription = "MISSING", FilterTypeId = (int)(ENT.FilterType.StockSupplyClassification), BatchID = 0 });

                # region //Vendor Value
                db.Insert(new FilterValue { Value = "1800 RADI", ValueDescription = "1800 RADIATOR", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "1PC  ", ValueDescription = "ONE PIECE PRODUCTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "3D FASTEN", ValueDescription = "3d Fasteners", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "3S CORPOR", ValueDescription = "3s Corporation", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "4 PALS IN", ValueDescription = "4 Pals Inc. Plumbin", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "5STAR", ValueDescription = "FIVE STAR GAS AND GEAR", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "67LOV", ValueDescription = "67LOV", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "A PARTS  ", ValueDescription = "A Parts", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "A&amp;A CONTR", ValueDescription = "A &amp; A Contract Custom Brokers", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AAB  ", ValueDescription = "ANTIQUE AUTO BATTERY", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AAC  ", ValueDescription = "ALUMINUM AIR CLEANERS INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AARI     ", ValueDescription = "ANTIQUE AUTOMOBILE RADIO INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ABILITY F", ValueDescription = "Ability Fire Equip., Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ABS B", ValueDescription = "ABS POWER BRAKES INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ABS BRAKE", ValueDescription = "Abs Power Brakes Inc", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AC   ", ValueDescription = "AUTO CUSTOM CARPETS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AC GLOBAL", ValueDescription = "AC GLOBAL MANUFACTURING", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AC2  ", ValueDescription = "AUTO CUSTOM CARPET", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ACCU ", ValueDescription = "DASHTOP", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ACE  ", ValueDescription = "ACE AUTO", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ACME ", ValueDescription = "DEAD VENDOR", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ACME DISP", ValueDescription = "Acme Display Fixture Company", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ACME-", ValueDescription = "DEAD VENDOR", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ACME-AP  ", ValueDescription = "Dead Vendor", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ACS  ", ValueDescription = "DEAD ACCT.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ACS2 ", ValueDescription = "DEAD ACCT.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ACTION WH", ValueDescription = "Action Wholesale Products, Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AD   ", ValueDescription = "AMERICAN DESIGNERS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ADC TECHN", ValueDescription = "Adc Technologies", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ADDCO", ValueDescription = "ADDCO MFG.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ADEPT", ValueDescription = "ADEPT INTERNATIONAL", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ADEPT INT", ValueDescription = "ADEPT INTERNATIONAL", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ADT SECUR", ValueDescription = "Adt Security Services", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ADVAN", ValueDescription = "TRU-FORM IND", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ADVN2", ValueDescription = "ADVANTAGE STAMPINGS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AE   ", ValueDescription = "ACME AUTO HEADLINING", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AE2  ", ValueDescription = "DEAD ACCT.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AFORM", ValueDescription = "ACCU-FORM PLASTICS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AFR  ", ValueDescription = "AIR FLOW RESEARCH", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AG   ", ValueDescription = "AUBURN GEAR", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AGS  ", ValueDescription = "AGS/PRO-TUBE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AH   ", ValueDescription = "AH", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AIR  ", ValueDescription = "AIR LIFT AIR SPRINGS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ALARM PER", ValueDescription = "Huntington Beach Police Dept", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ALCO ", ValueDescription = "ALCO METAL FAB", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ALFA     ", ValueDescription = "ALFA DIRECT INTERNATIONAL", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ALL CLASS", ValueDescription = "Acp All Classic Parts, Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ALLIANT I", ValueDescription = "Alliant Insurance Services", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AM   ", ValueDescription = "SOFFSEAL", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AMD  ", ValueDescription = "AUTO METAL DIRECT", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AMERI    ", ValueDescription = "American Graffiti", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AMERICAN ", ValueDescription = "AMERICAN AUTOWIRE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AMES ", ValueDescription = "AMES AUTOMOTIVE ENTERPRISES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AMOS PRES", ValueDescription = "Amos Press, Inc", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AMSTERDAM", ValueDescription = "Amsterdam Printing &amp; Litho", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ANDYS", ValueDescription = "ANDY'S TEE SHIRTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "APG - (DR", ValueDescription = "Cfw Media Llc (Apg)", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "APG - (MU", ValueDescription = "Yv Media Llc (Apg)", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "API  ", ValueDescription = "API", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AQ   ", ValueDescription = "AQ", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AR   ", ValueDescription = "WHEEL PROS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ARIDE", ValueDescription = "AIR RIDE TECHNOLOGIES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ARIZONA S", ValueDescription = "Az Saguaro Manufacturing, Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ARMOR", ValueDescription = "ARMOR PROTECTIVE PACKAGING", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ARP  ", ValueDescription = "AUTOMOTIVE RACING PRODUCTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ARROW", ValueDescription = "ARROWTRACK GPS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ARROW TRK", ValueDescription = "ARROWTRACK GPS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ART  ", ValueDescription = "CALIFORNIA PERFORMANCE TRANS.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ARTIS", ValueDescription = "DAVID KIZZIAR", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ASALE", ValueDescription = "ALL SALES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ASKEW", ValueDescription = "ASKEW HARDWARE PRODUCTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ASL  ", ValueDescription = "ASL", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ASM  ", ValueDescription = "MEXTRADE &amp; MANUFACTURING INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ASP  ", ValueDescription = "ASP", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AT&amp;T MOBI", ValueDescription = "At&amp;T Mobility", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ATLAS FOR", ValueDescription = "Atlas Forklift", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AUS  ", ValueDescription = "AUSLEY'S CHEVELLE PARTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AUTO     ", ValueDescription = "AUTOMETER", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AUTO CITY", ValueDescription = "AUTO CITY CLASSICS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AUTO METE", ValueDescription = "Auto Meter", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AUTO PRO ", ValueDescription = "AUTO PRO USA", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AUTO VEHI", ValueDescription = "Auveco Products", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AUTOSTAR ", ValueDescription = "Autostar Productions, Inc", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AUTOTRONI", ValueDescription = "Autotronic Controls Corporatio", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AVC  ", ValueDescription = "AU-VE-CO", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AWM  ", ValueDescription = "AWM", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "AZGM ", ValueDescription = "ARIZONA SAGUARO", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "B&amp;B  ", ValueDescription = "STEF'S PERFORMANCE PRODUCTS,INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "B&amp;M  ", ValueDescription = "B&amp;M RACING &amp; PERFORMANCE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "B&amp;M RACIN", ValueDescription = "B &amp; M Racing And Performance", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "B.E.S.T H", ValueDescription = "B.E.S.T", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BA   ", ValueDescription = "BOB'S ANTIQUES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BAER ", ValueDescription = "BAER BRAKES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BALLS", ValueDescription = "BALL'S ROD AND CUSTOM", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BB   ", ValueDescription = "BILLS BIRD RESTORATION PARTS CTR", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BC   ", ValueDescription = "BUTLER CLASSICS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BCC SOFTW", ValueDescription = "Bcc Software Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BCOOL", ValueDescription = "BE-COOL INCORPORATED", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BD   ", ValueDescription = "BUCKLE DOWN", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BE COOL  ", ValueDescription = "Be Cool", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BEAMS    ", ValueDescription = "BEAM'S INDUSTRIES INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BEAMS-AP ", ValueDescription = "Beam's Industries Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BED  ", ValueDescription = "BED", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BELL ", ValueDescription = "BELLTECH INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BELTS", ValueDescription = "MORRIS CLASSIC CONCEPTS LLC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BENCHMARK", ValueDescription = "Benchmark Staffing", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BEND ", ValueDescription = "BEND TEK", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BEND TEK ", ValueDescription = "Bend-Tek, Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BENTLEY P", ValueDescription = "Bentley Publishers", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BEST LIFE", ValueDescription = "B.E.S.T.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BETTER BU", ValueDescription = "Better Business Bureau", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BFM  ", ValueDescription = "BFM", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BG   ", ValueDescription = "BARRY GRANT INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BH   ", ValueDescription = "GENERAL STORE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BHI  ", ValueDescription = "BAER HOLDINGS, INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BILL'S BI", ValueDescription = "Bill's Birds", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BISHK", ValueDescription = "BISHKO AUTOMOBILE BOOKS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BISHKO   ", ValueDescription = "BISHKO AUTOMOBILE BOOKS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BKMNP", ValueDescription = "BKMNP", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BLUE ", ValueDescription = "BLUEPRINT ENGINES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BLUEWOOD ", ValueDescription = "Bluewood Pallets", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BM   ", ValueDescription = "THE BATTERY MAT", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BOB BAKER", ValueDescription = "Bob Baker Ltd", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BOBIT    ", ValueDescription = "Bobit Business Media", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BOLSA BUS", ValueDescription = "Triton Business Park, Llc", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BONN ", ValueDescription = "BONNEVILLE SPORTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BORLA", ValueDescription = "BORLA", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BOURT", ValueDescription = "BOURET DESIGN", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BPH  ", ValueDescription = "BEN PHILLIPS ValueDescription PLATES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BRAD ", ValueDescription = "BRAD DEHAVEN &amp; ASSOCIATES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BRIGGEMAN", ValueDescription = "Briggeman Disposal", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BRNDX", ValueDescription = "GEARHEAD PLANET", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BRUCE LOG", ValueDescription = "Bruce Logan Gto", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BRUNNWORT", ValueDescription = "Brunnworth Lein Sales", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BUCK ", ValueDescription = "FINE LINES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BUCKAROO ", ValueDescription = "Buckaroo", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BUTLR", ValueDescription = "BUTLER PERFORMANCE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "BW   ", ValueDescription = "BARRY WHITES STREET RODS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "C COR", ValueDescription = "CLARK'S CORVAIR PARTS INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CA DMV   ", ValueDescription = "California Department Of Motor Vehi", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CAD KING ", ValueDescription = "CADILLAC KING", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CADDA", ValueDescription = "CADDY DADDY", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CADDADDY ", ValueDescription = "CADDY DADDY", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CAL  ", ValueDescription = "CLASSIC AUTO LOCK", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CAL STATE", ValueDescription = "Cal State Auto Parts", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CAL VAN T", ValueDescription = "Horizon Tool, Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CALBR", ValueDescription = "CALBR", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CALIFORNI", ValueDescription = "California Chamber Of Commerce", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CALST", ValueDescription = "CAL-STATE AUTO PARTS, INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CALUR", ValueDescription = "CALUR", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CANTN", ValueDescription = "CANTON RACING PRODUCTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CAP  ", ValueDescription = "CAP", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CAP KEEPE", ValueDescription = "Cap Keepers", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CAR COLLE", ValueDescription = "Car Collector", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CAR COVER", ValueDescription = "Car Cover Company", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CAR PAK  ", ValueDescription = "Car Pak", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CARB ", ValueDescription = "CARB", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CARDONE  ", ValueDescription = "CARDONE INDUSTRIES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CARLTON,D", ValueDescription = "Carlton,Disante &amp; Freudenberge", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CARMO", ValueDescription = "CAR MOTORSPORTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CARPK", ValueDescription = "CAR-PAK MFG.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CARR ", ValueDescription = "CARRAND (CAROL)", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CARS ", ValueDescription = "C.A.R.S. INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CARS2", ValueDescription = "CARS INC. BUICK", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CAS  ", ValueDescription = "CUSTOM AUTO SOUND", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CAT  ", ValueDescription = "ARO 2000", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CC   ", ValueDescription = "SOUTHWEST REPRODUCTION", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CCCP ", ValueDescription = "CALIFORNIA CLASSIC CHEVY PARTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CCF  ", ValueDescription = "PROUD ROAD INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CCM  ", ValueDescription = "CCM", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CCP  ", ValueDescription = "CLASSIC INDUSTRIES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CCS  ", ValueDescription = "CCS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CD   ", ValueDescription = "CD", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CDW  ", ValueDescription = "VP SALES CO", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CEMPI", ValueDescription = "CEMPI INDUSTRIES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CENTR", ValueDescription = "CENTRIC PARTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CENTRIC  ", ValueDescription = "CENTRIC PARTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CENTURY W", ValueDescription = "Century Wheel &amp; Rim", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CF   ", ValueDescription = "CARBON COMPONENTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CFF  ", ValueDescription = "CFF", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CHAR ", ValueDescription = "HONEST CHARLEY INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CHECK", ValueDescription = "CHECK CORPORATION", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CHECKCORP", ValueDescription = "Check Corporation", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CHEVY", ValueDescription = "CHEVY", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CHQ  ", ValueDescription = "CHQ REPRODUCTIONS, INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CHRIS JAC", ValueDescription = "Chris Jacobs", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CHUBB    ", ValueDescription = "Chubb Group Of Insurance Companies", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CI   ", ValueDescription = "MONTCO", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CISCO SYS", ValueDescription = "Cisco Systems Capital Corporation", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CITY OF H", ValueDescription = "City Of Huntington Beach", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CITY OF S", ValueDescription = "City Of Seal Beach Finance Dept", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CK   ", ValueDescription = "CAP-KEEPER", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CLARK IND", ValueDescription = "Crestmark Commercial Capital Lendin", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CLASSY CA", ValueDescription = "Classy Cars", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CLASY", ValueDescription = "CLASY", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CLAY ", ValueDescription = "CLAY SMITH ENGINEERING,INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CLNDR", ValueDescription = "TWITCHY DIGIT PRODUCTIONS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CLOCK", ValueDescription = "CLOCK", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CLPPR", ValueDescription = "CLIPPER INDUSTRIAL CO. LTD.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CLPR2", ValueDescription = "CLIPPER INDUSTRIAL CO. LTD", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CLSC ", ValueDescription = "CLASSIC 2 CURRENT FABRICATION  P", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CLSC 2 CR", ValueDescription = "CLASSIC 2 CURRENT FABRICATION", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CLSC INST", ValueDescription = "CLASSIC INSTRUMENTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CLSSC", ValueDescription = "CLASSIC FABRICATION", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CLSSC DSH", ValueDescription = "CLASSIC DASH/THUNDER ROAD", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CLSSC FAB", ValueDescription = "Classic Fabrication", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CLUB ", ValueDescription = "WINTER INTERNATIONAL", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CMC  ", ValueDescription = "CHICAGO MUSCLE CAR PARTS INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CMD  ", ValueDescription = "TRIM PARTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CMD2 ", ValueDescription = "TRIM PARTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CMD3 ", ValueDescription = "TRIM PARTS- CARPET ACCOUNT", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CMD4 ", ValueDescription = "TRIM PARTS-FLOORMATS &amp; ACCESS.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CNTPT", ValueDescription = "CNTPT", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CNTRL", ValueDescription = "CNTRL", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "COAST TO ", ValueDescription = "Coast To Coast Technology", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "COASTAL P", ValueDescription = "Coastal Press, Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "COKER", ValueDescription = "COKER", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "COLLECTOR", ValueDescription = "Collectorcar Traderonline.Com", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "COLO ", ValueDescription = "COLORADO CUSTOMS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "COLOR ILL", ValueDescription = "Color Illusion Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "COMMUNICA", ValueDescription = "Communication Arts", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "COMP ", ValueDescription = "COMP CAMS/POWER HOUSE TOOLS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "COMP CAMS", ValueDescription = "Competition Cams Inc", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "COMP-", ValueDescription = "COMP CAMS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "COMP-AP  ", ValueDescription = "Comp Cams", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CONN ", ValueDescription = "CONN", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CONSOLES ", ValueDescription = "Classic Consoles Inc", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CONVE", ValueDescription = "CONVERTIBLE SPECIALISTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CONVERT  ", ValueDescription = "CONVERTIBLE SPECIALISTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "COOL ", ValueDescription = "COOL IT THERMO TEC HIGH PERFORMA", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "COSTCO   ", ValueDescription = "Costco Wholesale Membership", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "COVER", ValueDescription = "COVERCRAFT", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "COX      ", ValueDescription = "Cox Communications 401", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "COX 1    ", ValueDescription = "Cox Communications", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "COX 101  ", ValueDescription = "Cox Communications", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "COX 1301 ", ValueDescription = "Cox Communications", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "COX BUSIN", ValueDescription = "Cox Communications", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "COX501   ", ValueDescription = "Cox Communications O.C.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "COX5512  ", ValueDescription = "Cox Communications 401", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "COYMN", ValueDescription = "COEYMAN ENTERPRISES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CP   ", ValueDescription = "CP", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CP AUTOMO", ValueDescription = "C.P. Automotive", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CPA  ", ValueDescription = "CP AUTO PRODUCTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CPP  ", ValueDescription = "CLASSIC PERFORMANCE PRODUCTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CPR      ", ValueDescription = "CALIFORNIA PONTIAC RESTORATION", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CPS  ", ValueDescription = "HYDRO ELECTRIC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CR   ", ValueDescription = "CLASSIC REPRODUCTIONS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CRAFTEC  ", ValueDescription = "CRAFTEC INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CRAGR", ValueDescription = "CRAGR", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CRANK", ValueDescription = "CRANK", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CRCVR", ValueDescription = "LICENSE FRAMES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CRESTMARK", ValueDescription = "A/C Clark Industries", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CRID ", ValueDescription = "CREATIVE IDEAS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CRK  ", ValueDescription = "CRK AND PARTS CORP.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CRM  ", ValueDescription = "MOD ROTO", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CROSS", ValueDescription = "CROSS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CROW ", ValueDescription = "CROW ENTERPRIZES INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CROWN", ValueDescription = "CROWN", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CROWN MAI", ValueDescription = "Crown Maintenance, Inc", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CSTMM", ValueDescription = "CUSTOM MONTE SS PARTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CSTMMONTE", ValueDescription = "CUSTOM MONTE SS PARTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CURT     ", ValueDescription = "CURT ROEHM BOOKS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CUST ", ValueDescription = "CUSTOM MACHINE COMPONENTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CUST MCHN", ValueDescription = "CUSTOM MACHINE COMPONENTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CVR  ", ValueDescription = "GLOBAL ACCESSORIES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CW   ", ValueDescription = "CHEVELLE WORLD", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CWD  ", ValueDescription = "CWD", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CYBEX SEC", ValueDescription = "Cybex Security Systems", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "CYLINDERS", ValueDescription = "CONVERTIBLE CYLINDERS DIRECT", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "D&amp;P  ", ValueDescription = "D&amp;P CLASSIC CHEVY", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "D.M. STEE", ValueDescription = "D.M. Steele Company", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DASH ", ValueDescription = "DASH DESIGN INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DASH-", ValueDescription = "DASH DESIGNS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DASH-AP  ", ValueDescription = "Dash Designs", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DATS TRUC", ValueDescription = "Dats Trucking, Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DAYLIGHT ", ValueDescription = "Daylight Transport, Llc", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DAYTO", ValueDescription = "DAYTONA SENSORS LLC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DAYTONA  ", ValueDescription = "MODOTEK PERFORMANCE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DBOIS", ValueDescription = "DUBOIS MARKETING INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DCM MANUF", ValueDescription = "Dcm Manufacturing, Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DD   ", ValueDescription = "DAKOTA DIGITAL", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DEA  ", ValueDescription = "DEA PRODUCTS INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DEALS ON ", ValueDescription = "Deals On Wheels Publications", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DECCO", ValueDescription = "DECCOFELT CORP", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DELTA", ValueDescription = "DELTA TECH INDUSTRIES, LLC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DER  ", ValueDescription = "DER SHINEY STUFF", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DER REAL ", ValueDescription = "Der Real Stuff", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DERALE   ", ValueDescription = "Derale Cooling Systems", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DEREVERE ", ValueDescription = "Derevere &amp; Associates", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DET  ", ValueDescription = "DETAILS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DG   ", ValueDescription = "DAVE GRAHAM", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DH   ", ValueDescription = "DH", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DHL EXPRE", ValueDescription = "Dhl Express (Usa) Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DHL GLOBA", ValueDescription = "Dhl Globalmail", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DI   ", ValueDescription = "DISTINCTIVE INDUSTRIES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DI2  ", ValueDescription = "DISTINCTIVE IND.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DICE ", ValueDescription = "DICE ELECTRONICS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DICK ", ValueDescription = "DICK", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DICK EYTC", ValueDescription = "Dick Eytchison Aluminum Air Cleaner", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DIFF ", ValueDescription = "DIFF WORKS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DIRECTV  ", ValueDescription = "Directv", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DISCO", ValueDescription = "DISCO", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DISPLAY  ", ValueDescription = "Displayworks - Mice", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DIXIE", ValueDescription = "CUSTOM AMERICAN AUTO PARTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DIXIEMCD ", ValueDescription = "DIXIE MONTE CARLO DEPOT", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DK   ", ValueDescription = "KIRBAN PERFORMANCE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DK2  ", ValueDescription = "DENNIS KIRBAN GTO'S", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DM   ", ValueDescription = "DYNAMIC MACHINING INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DMV      ", ValueDescription = "Department Of Motor Vehicles", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DMV5536  ", ValueDescription = "Department Of Motor Vehicles", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DON  ", ValueDescription = "DON", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DON Z", ValueDescription = "DON ZIG MAGNETOS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DON ZIG  ", ValueDescription = "DON ZIG MAGNETOS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DONUT", ValueDescription = "DONUT DERELICTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DOUG ", ValueDescription = "DOUG'S HEADERS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DP   ", ValueDescription = "DP", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DPAD2", ValueDescription = "DASHES DIRECT", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DPADS", ValueDescription = "DASHES DIRECT", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DR   ", ValueDescription = "D&amp;R CLASSIC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DRAKE", ValueDescription = "SCOTT DRAKE ", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DREAM", ValueDescription = "DREAM", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DSE  ", ValueDescription = "DETROIT SPEED INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DUBIA,ERI", ValueDescription = "Dubia, Erickson, Tenerelli &amp; Russo", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DUSTR", ValueDescription = "DUSTR", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DYNA ", ValueDescription = "DYNACORN INTERNATIONAL INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "DYNA1", ValueDescription = "DYNACORN INTERNATIONAL INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "EAGLE", ValueDescription = "EAGLE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "EARLY", ValueDescription = "EARLY BIRD ACCESORIES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "EASE-E-WA", ValueDescription = "Ease-E-Waste", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "EASEP", ValueDescription = "EASEPAL ENTERPRISES LTD", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "EASEPAL  ", ValueDescription = "EASEPAL ENTERPRISES LTD", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "EAST ", ValueDescription = "EAST", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "EASTW", ValueDescription = "EASTWOOD", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "EASTWOOD ", ValueDescription = "EASTWOOD", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ECP  ", ValueDescription = "EL CAMINO'S PLUS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "EDDIE", ValueDescription = "EDDIE MOTORSPORTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "EDEL ", ValueDescription = "EDELBROCK", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "EDISON   ", ValueDescription = "Southern California Edison", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "EELCO", ValueDescription = "EELCO MANUFACTURING", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "EFW  ", ValueDescription = "ENTERPRISE FOUNDRY WORKS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "EL CA", ValueDescription = "EL CAMINO MANUFACTURING", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "EL CAM-AP", ValueDescription = "El Camino Manufacturing", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ELCO ", ValueDescription = "EL CAMINO MFG.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ELE  ", ValueDescription = "ELE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ELGIN", ValueDescription = "ELGIN INDUSTRIES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ELITE", ValueDescription = "ELITE AUTOMOTIVE PRODUCTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "EM   ", ValueDescription = "EM", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "EMCO ", ValueDescription = "EMCO SPECIALTIES INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "EMI      ", ValueDescription = "Equity Management Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "EMI5541  ", ValueDescription = "Equity Management Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "EMPIRE SC", ValueDescription = "Empire Scale Company", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "EMT  ", ValueDescription = "RESTORATION TECHNOLOGY INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ENCIRCLE ", ValueDescription = "Encircle Inc", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ENGINEER ", ValueDescription = "ENGINEERED SOURCE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "EPCO ", ValueDescription = "E PARRELLA COMPANY CO", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ERTL ", ValueDescription = "THE ERTL COMPANY INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ESPOSITO ", ValueDescription = "DAVE ESPOSITO", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "EVER ", ValueDescription = "EVERGREEN MUSCLE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "EXTERIOR ", ValueDescription = "Exterior Products, Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "F&amp;W BOOKS", ValueDescription = "F&amp;W Publications, Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "F&amp;W PUBLI", ValueDescription = "F&amp;W Publications-Krause", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "FAB  ", ValueDescription = "FAB", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "FAIR ", ValueDescription = "FAIRCHILD INDUSTRIES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "FARGO", ValueDescription = "FARGO AUTOMOTIVE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "FARMERS F", ValueDescription = "Farmers Fire Insurance Exchang", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "FARMERS I", ValueDescription = "Farmers Ins Group Of Cos", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "FEDEX    ", ValueDescription = "Federal Express Corporation", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "FEDEX 303", ValueDescription = "Fedex", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "FEDEX FRE", ValueDescription = "Fedex Freight West", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "FEDEX NAT", ValueDescription = "Fedex National Ltl", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "FINE ", ValueDescription = "FINE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "FINELINES", ValueDescription = "Finelines", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "FIT  ", ValueDescription = "FIT", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "FM   ", ValueDescription = "FLOWMASTER", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "FOWF1", ValueDescription = "FOWF1", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "FR   ", ValueDescription = "FLAMING RIVER INDUSTRIES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "FRAN     ", ValueDescription = "INTERIOR PARTS, INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "FRAN ", ValueDescription = "DEAD ACCT.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "FRAN2", ValueDescription = "DEAD ACCT", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "FRANCHISE", ValueDescription = "Franchise Tax Board", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "FRANK", ValueDescription = "FRANK", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "FREE ", ValueDescription = "FREEFLOW", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "FRY COMMU", ValueDescription = "Fry Communications", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "FULLR", ValueDescription = "FULLER HOT RODS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "FUSES", ValueDescription = "CATALINA PERFORMANCE ACCESSORIES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "FUSICK AU", ValueDescription = "Fusick Automotive Products", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "FUTRX", ValueDescription = "FUTRX", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "FX   ", ValueDescription = "LAUREN ENGINEERING", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "G FOR", ValueDescription = "G FORCE PERFORMANCE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "G FORCE  ", ValueDescription = "G FORCE PERFORMANCE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "GALEN", ValueDescription = "GALEN PRODUCTS INT", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "GANAHL LU", ValueDescription = "Ganahl Lumber Co", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "GARY B RO", ValueDescription = "Gary B Ross, A Law Corporation", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "GAY  ", ValueDescription = "JOE BEDARD", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "GEARHEADC", ValueDescription = "Gearheadcafe.Com/Detroit Iron", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "GFE  ", ValueDescription = "GFE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "GLASS", ValueDescription = "GLASSTEK", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "GLIDE", ValueDescription = "GLIDE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "GLOBA", ValueDescription = "GLOBAL  ACCESSORIES, INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "GLOBAL-AP", ValueDescription = "Global  Accessories, Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "GM   ", ValueDescription = "GUARANTY CHEVROLET", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "GM OB", ValueDescription = "GM OBSOLETE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "GM OBSOLE", ValueDescription = "GM OBSOLETE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "GMB  ", ValueDescription = "GMB NORTH AMERICA", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "GMI  ", ValueDescription = "GOODMARK", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "GMM  ", ValueDescription = "GMM", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "GMPP ", ValueDescription = "GMPP", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "GOAT ", ValueDescription = "GOAT HILL CLASSICS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "GOLD STAN", ValueDescription = "Gold Standard Service Corp.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "GRAFX", ValueDescription = "GRAFIX SYSTEMS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "GRAND", ValueDescription = "GRAND", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "GRANT", ValueDescription = "GRANT", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "GRIFF", ValueDescription = "GRIFFIN RADIATOR", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "GROOVE CO", ValueDescription = "Groove Construction Inc", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "GROUND UP", ValueDescription = "GROUND UP INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "GSE  ", ValueDescription = "IMAGE APPAREL", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "GSPP ", ValueDescription = "GSPP", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "GSTAR", ValueDescription = "GOLDEN STAR AUTO", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "GT   ", ValueDescription = "GT MUSCLE CAR", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "GTO  ", ValueDescription = "GTO", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "GTPERF   ", ValueDescription = "GT PERFORMANCE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "GW   ", ValueDescription = "GLOBAL WEST SUSPENSION SYSTEMS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "GWC  ", ValueDescription = "GARDNER WESTCOTT COMPANY", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "GWFO2", ValueDescription = "GWFO2", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "H&amp;H  ", ValueDescription = "H&amp;H CLASSIC AUTO RESTORATIONS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HAMPT", ValueDescription = "BARRY HAMPTONS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HANE ", ValueDescription = "HANE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HARM ", ValueDescription = "HARMONS CHEVY PARTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HARRIET B", ValueDescription = "Harriet B Alexson,  A Prof. Law Cor", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HASLER   ", ValueDescription = "Hasler, Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HATS ", ValueDescription = "HATS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HB STAFFI", ValueDescription = "Hb Staffing", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HBC  ", ValueDescription = "HEARTBEAT CITY", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HCM  ", ValueDescription = "HUBCAP MIKE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HD   ", ValueDescription = "H&amp;D MOLDING", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HEART OF ", ValueDescription = "Heart Of Dixie Cehvelle Club", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HEDMN", ValueDescription = "HEDMAN HEDDERS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HEF  ", ValueDescription = "HEF", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HELM ", ValueDescription = "HELM", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HEMMINGS ", ValueDescription = "HEMMINGS MOTOR NEWS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HGP  ", ValueDescription = "HIGHLAND GLEN MFG INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HILD ", ValueDescription = "HILD", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HLLY2", ValueDescription = "HLLY2", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HLNDR", ValueDescription = "ADP HOLLANDER", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HLWIG", ValueDescription = "HELLWIG PRODUCTS COMPANY INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HOEPT", ValueDescription = "HOEPTNERS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HOLLANDER", ValueDescription = "Hollander", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HOLLE", ValueDescription = "HOLLEY PERFORMANCE PRODUCTS, I P", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HOLLEY-AP", ValueDescription = "Holley Performance Products, Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HOLLY", ValueDescription = "HOLLEY PERFORMANCE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HOME DEPO", ValueDescription = "Home Depot Credit Services", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HOOK ", ValueDescription = "HOOKER HEADERS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HOT  ", ValueDescription = "HOTCHKIS PERFORMANCE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HOT ROD &amp;", ValueDescription = "Hot Rod &amp; Restoration", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HP   ", ValueDescription = "HP BOOKS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HPP  ", ValueDescription = "HPP", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HRH  ", ValueDescription = "PERFORMANCE PLUS/TWE INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HRP  ", ValueDescription = "HOT RODS PLUS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HS   ", ValueDescription = "JAMES SHIELDS-BURNOUT", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HSM      ", ValueDescription = "Hsm Electronic Protection Services", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HSM5576  ", ValueDescription = "Hsm Electronic Protection Services", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HTF P", ValueDescription = "HTF", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HTF PART ", ValueDescription = "ALPHONSO SANCHEZ", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HUBBARDS ", ValueDescription = "HUBBARDS IMPALA PARTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HUNTER   ", ValueDescription = "Hunter Fiberglass", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HUNTINGTO", ValueDescription = "Huntington Beach High School", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "HYBRIDGE ", ValueDescription = "HYBRIDGE IMP. &amp; EXP. CO. LTD", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ICON ", ValueDescription = "ICONOGRAFIX, INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ICP  ", ValueDescription = "ICP", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "IDEARC   ", ValueDescription = "Idearc Media Corp. 140", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "IDEARC 13", ValueDescription = "Idearc Media Corp. 139", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "IDID ", ValueDescription = "IDIDIT INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "IG   ", ValueDescription = "IOWA GLASS DEPOT", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "IMPERIAL ", ValueDescription = "Imperial Auto Body", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "IND  ", ValueDescription = "INDIAN ADVENTURES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "INDUSTRIA", ValueDescription = "Industrial Distribution", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "INLNE", ValueDescription = "INLINE TUBE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "INNOV", ValueDescription = "INNOVATIVE WAREHOUSE PRODUCTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "INSTANT J", ValueDescription = "Instant Jungle International", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "INSUL", ValueDescription = "INSUL", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "IRON ", ValueDescription = "DETROIT IRON", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "IRS      ", ValueDescription = "Irs", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "IRS5583  ", ValueDescription = "Irs", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "IRWIN BUI", ValueDescription = "Irwin Buisness Fin", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "IS   ", ValueDescription = "INSTRUMENT SERVICE INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ISS  ", ValueDescription = "INSTRUMENT SALES AND SERVICE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "J A DUNCA", ValueDescription = "J A Duncan", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "J&amp;M      ", ValueDescription = "ST. LOUIS SPRING CO.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "J&amp;M5586  ", ValueDescription = "ST. LOUIS SPRING CO.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "J&amp;S      ", ValueDescription = "J &amp; S Gear", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "J&amp;S5587  ", ValueDescription = "J &amp; S Gear", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "J'LEY &amp; C", ValueDescription = "J'Ley &amp; Company", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "J. COEYMA", ValueDescription = "J. Coeyman Ent", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "JACKSON N", ValueDescription = "Jackson National Life Insuranc", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "JAE  ", ValueDescription = "JAE ENTERPRISES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "JAMCO", ValueDescription = "COIL SPRING SPECIALTIES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "JANTEK EL", ValueDescription = "Jantek Electronics Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "JAY'S CAT", ValueDescription = "Jay's Catering", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "JB   ", ValueDescription = "JACKSON BROS VIDEO", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "JC   ", ValueDescription = "JC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "JCBL ", ValueDescription = "JCBL", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "JCP  ", ValueDescription = "JC PUBLISHING", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "JESSE", ValueDescription = "JESSE LAI INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "JET  ", ValueDescription = "JET PERFORMANCE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "JH   ", ValueDescription = "COVER ALL CAR COVERS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "JH RESTOR", ValueDescription = "Jh Restoration And Custom", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "JO   ", ValueDescription = "JIM OSBORNE REPRODUCTION", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "JOE P RIT", ValueDescription = "Joe P Rittel", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "JOOLTOOL ", ValueDescription = "JOOLTOOL", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "JORDN", ValueDescription = "JORDN", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "JR   ", ValueDescription = "JR DISTRIBUTORS INTERNATIONAL", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "JS   ", ValueDescription = "J + S GEAR", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "JS ENT   ", ValueDescription = "JS ENTERPRISES SOUTHEAST, INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "JUMBO    ", ValueDescription = "JUMBOSOLENOID MFG. CO. LTD.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "JUMBO", ValueDescription = "JUMBO", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "JUNK ", ValueDescription = "JUNK", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "KAISER PE", ValueDescription = "Kaiser Permanante", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "KANTR", ValueDescription = "KANTER AUTO PRODUCTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "KARL ", ValueDescription = "KARL LARSEN", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "KARL LARS", ValueDescription = "Larsen Engineering", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "KD CL", ValueDescription = "KD CLASSICS INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "KD CLSSCS", ValueDescription = "KD CLASSICS INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "KEEN ", ValueDescription = "KEEN PARTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "KEEPR", ValueDescription = "KEEPR", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "KEG  ", ValueDescription = "KEG SUPPLY", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "KEG GRAPH", ValueDescription = "Keg Graphics", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "KEN  ", ValueDescription = "KEN", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "KEY  ", ValueDescription = "PERFECT COOLING", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "KEYST", ValueDescription = "KEYSTONE AUTOMOTIVE INDUSTRIES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "KHE      ", ValueDescription = "VINTAGE CAR AUDIO INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "KHE  ", ValueDescription = "K&amp;C HARRISON, INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "KINKO'S  ", ValueDescription = "Kinko's", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "KLEEN", ValueDescription = "KLEEN WHEELS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "KLSSC KEY", ValueDescription = "KLASSIC KEYLESS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "KNOCH", ValueDescription = "AL KNOCH", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "KOOL ", ValueDescription = "KOOLMAT INSULATION", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "KP   ", ValueDescription = "KRAUSE PUBLICATIONS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "KPART", ValueDescription = "KPART", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "KUGEL", ValueDescription = "KUGEL KOMPONENTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "KY   ", ValueDescription = "GOODMARK", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "KYB  ", ValueDescription = "MONARCH", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "L&amp;N UNIFO", ValueDescription = "L &amp; N Uniform Supply", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "LAMM ", ValueDescription = "LAMM", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "LARRYHAAS", ValueDescription = "LARRY HAAS AUTO PARTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "LAURE", ValueDescription = "CR LAURENCE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "LAURENCE ", ValueDescription = "Cr Laurence", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "LAWSN", ValueDescription = "DOUG LAWSON", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "LEG  ", ValueDescription = "LEGENDARY AUTO INTERIORS LTD", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "LEGCY", ValueDescription = "LEGCY", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "LEGENDARY", ValueDescription = "Legendary Gm Magazine", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "LEGIN", ValueDescription = "GOLDEN LEGION AUTOMOTIVE CORP.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "LESCO    ", ValueDescription = "LESCO DISTRIBUTING", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "LESS ", ValueDescription = "LESS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "LET'S GO ", ValueDescription = "Lets Go Cruisin Magazine", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "LIEGE COR", ValueDescription = "Liege Corp", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "LINDLEY F", ValueDescription = "Lindley Fire Protection Co, Inc", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "LMC  ", ValueDescription = "LEGENDARY MOTORCAR COMPANY LTD", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "LOKAR", ValueDescription = "LOKAR PERFORMANCE PRODUCTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "LOS ANGEL", ValueDescription = "Los Angeles Times", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "LUCKY", ValueDescription = "LUCKY THIRTEEN APPAREL", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "M GRE", ValueDescription = "MIKE GREENE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "M GREENE ", ValueDescription = "MIKE GREENE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "M&amp;M  ", ValueDescription = "M&amp;M ENTERPRISES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "M&amp;M SPEED", ValueDescription = "M&amp;M Speed &amp; Custom", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MALRY", ValueDescription = "MALLORY IGNITION", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MANLY", ValueDescription = "MANLEY PERFORMANCE PRODUCTS INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MARA ", ValueDescription = "MARADYNE HIGH PERFORMANCE FANS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MARCH", ValueDescription = "MARCH PERFORMANCE PULLEYS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MARSH", ValueDescription = "MARSHALL INSTRUMENTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MARSHALL ", ValueDescription = "MARSHALL INSTRUMENTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MASTERS E", ValueDescription = "Masters Entertainment Group", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MASTERS T", ValueDescription = "Masters Televison Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MASUNE CO", ValueDescription = "Masune Company - Medco", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MAT  ", ValueDescription = "DYNAMIC CONTROL", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MATERIAL ", ValueDescription = "Material Handling Supply Inc", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MATRO", ValueDescription = "MATRO", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MAVERICK ", ValueDescription = "MAVERICK UNDERCAR PERFORMANCE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MB       ", ValueDescription = "QUARTO PUBLISHING GROUP", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MB   ", ValueDescription = "MOTORBOOKS INTERNATIONAL", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MB0       ", ValueDescription = "HACHETTE BOOK GROUP USA", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MB1     ", ValueDescription = "HACHETTE BOOK GROUP USA", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MB2  ", ValueDescription = "MOTORBOOKS INTERNATIONAL", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MB3  ", ValueDescription = "MOTORBOOKS INTERNATIONAL", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MB4  ", ValueDescription = "DEAD ACCT", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MBM  ", ValueDescription = "MB MARKETING &amp; MANUFACTURING", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MC   ", ValueDescription = "MC PRODUCTION", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MC PRODUC", ValueDescription = "Mc Productions", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MC-AP", ValueDescription = "MC PRODUCTIONS, INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MCCLE", ValueDescription = "BRAD MCCLEARY", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MCCLEARY ", ValueDescription = "BRAD MCCLEARY", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MCDEP", ValueDescription = "MUSCLE CAR DEPOT", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MCDEPOT  ", ValueDescription = "MUSCLE CAR DEPOT", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MCGARD IN", ValueDescription = "Mcgard Inc", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MCGRD", ValueDescription = "BALLARD AND ALLEN MARKETING", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MCLOONE  ", ValueDescription = "Mcloone", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MCM  ", ValueDescription = "MUSCLE CAR MIKE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MCV  ", ValueDescription = "MCV", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MDCT ", ValueDescription = "MDCT INCORPORATED", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MEDCO    ", ValueDescription = "Medco Supply Company", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MEGS ", ValueDescription = "CONE ENGINEERING INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MESA ", ValueDescription = "MESA", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "METAL", ValueDescription = "DEAD ACCT", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "METRO    ", ValueDescription = "METRO MOULDED PARTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "METRO DST", ValueDescription = "METRO AUTO PARTS DISTRIBUTORS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MEV  ", ValueDescription = "MEV", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MF   ", ValueDescription = "MUSCLE FACTORY", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MFO  ", ValueDescription = "MIKE FUSICK AUTOMOTIVE PRODUCTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MG   ", ValueDescription = "MOUNTAIN GOATS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MH   ", ValueDescription = "M&amp;H FABRICATORS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MH-G ", ValueDescription = "MH-G", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MH2  ", ValueDescription = "M&amp;H FABRICATORS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MIKE ", ValueDescription = "MUSCLE CAR MIKE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MIKE2", ValueDescription = "MIKE2", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MIKES    ", ValueDescription = "MIKE'S AUTO PARTS &amp; ACCESSORIES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MILO ", ValueDescription = "MILODON", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MITY ", ValueDescription = "DYNATECH ENGINEERING-MITY MOUNTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MLUBE", ValueDescription = "MASTERLUBE INDUSTRIES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MMI      ", ValueDescription = "Mmi - Mini Mailers", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MMI5622  ", ValueDescription = "Mmi - Mini Mailers", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MMP  ", ValueDescription = "METRO PAINTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MNLND", ValueDescription = "MAINLAND", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MODEL", ValueDescription = "MODEL", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MODERN-AI", ValueDescription = "Modern-Air", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MODIN", ValueDescription = "MODINE RADIATOR", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MON  ", ValueDescription = "MON", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MORSE DAT", ValueDescription = "Morse Data Corp.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MOTHE", ValueDescription = "MOTHERS WAX", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MOTIV", ValueDescription = "MOTIVE PRODUCTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MOTOR", ValueDescription = "MOTORSPORT DIRECT", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MOUNT", ValueDescription = "KAY AUTOMOTIVE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MP       ", ValueDescription = "Mp", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MP   ", ValueDescription = "M P ENTERPRISES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MPE  ", ValueDescription = "M P ENTERPRISES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MRG  ", ValueDescription = "MR G'S FASTENERS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MRGAS", ValueDescription = "MR GASKET CO. PERFORMANCE GROUP", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MSA1 ", ValueDescription = "MSA-1 BOOKS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MSD  ", ValueDescription = "MSD IGNITION", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MSI FREIG", ValueDescription = "Msi Frieght, Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "MUDGE", ValueDescription = "MUDGE FASTENERS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "NCOA ", ValueDescription = "NATIONAL CHEVELLE OWNERS ASSC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "NEO LOGIC", ValueDescription = "Neo Logic Networks Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "NERGY", ValueDescription = "ENERGY SUSPENSION", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "NEWNA", ValueDescription = "NEWNAK'S, INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "NEWNAK-AP", ValueDescription = "Newnak's, Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "NEWPORT B", ValueDescription = "Newport Business Interiors", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "NFIB     ", ValueDescription = "Nfib", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "NG   ", ValueDescription = "NG", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "NINGB", ValueDescription = "NINGB", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "NMC GROUP", ValueDescription = "Nmc Group", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "NO-VE", ValueDescription = "NO-VENDOR", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "NO-VENDOR", ValueDescription = "NO-VENDOR", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "NOAH ", ValueDescription = "NOAH PERFORMANCE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "NORAM", ValueDescription = "NOR/AM AUTO BODY PARTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "NORAM AUT", ValueDescription = "Noram Auto Body Parts", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "NORM WILS", ValueDescription = "Norm Wilson &amp; Sons, Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "NORTH AME", ValueDescription = "North American Glass Tinting", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "NOS  ", ValueDescription = "NITROUS OXIDE SYSTEMS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "NRG  ", ValueDescription = "DEE ENGINEERING INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "NSTAR", ValueDescription = "NSTAR", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "NUNAK", ValueDescription = "NEWNAK'S INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "NUWAVE CO", ValueDescription = "Nuwave Container", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "NWCSL", ValueDescription = "NEW CASTLE BATTERY MFG CO", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "NWM      ", ValueDescription = "CLASSIC LEDS LLC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "NWM  ", ValueDescription = "NORTHWEST MUSTANG", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "OA   ", ValueDescription = "OLD AIR PRODUCTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "OEC GROUP", ValueDescription = "Oec Logistics", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "OFFICE TE", ValueDescription = "Office Team", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "OIG      ", ValueDescription = "Original Investment Group, Llc", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "OIG5640  ", ValueDescription = "Original Investment Group, Llc", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "OLD DOMIN", ValueDescription = "Old Dominion Freight Line, Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "OLDSMOBIL", ValueDescription = "Oldsmobile Club Of America", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "OLYMC2    ", ValueDescription = "MEG TECHNOLOGIES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "OLYMC", ValueDescription = "NYLON MOLDING CORPORATION", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "OMNI ", ValueDescription = "OMNI", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ONS      ", ValueDescription = "KEBBUCK LLC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ONS  ", ValueDescription = "ONS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "OPFER", ValueDescription = "OPFER", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "OPGI ", ValueDescription = "OPGI", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ORMOLU EN", ValueDescription = "Ormolu Enterprises", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "OTE  ", ValueDescription = "ON THE EDGE MARKETING", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PACIFIC G", ValueDescription = "Abc Property Owners Association", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PACKARD I", ValueDescription = "Packard Industries", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PAD  ", ValueDescription = "PAD", FilterTypeId = (int)((int)(ENT.FilterType.VendorCode)) });
                db.Insert(new FilterValue { Value = "PAH  ", ValueDescription = "PAH PUBLISHING", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PAINT", ValueDescription = "SEYMOUR OF SYCAMORE INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PALCO IND", ValueDescription = "Palco Industries", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PAPER REC", ValueDescription = "Paper Recycling &amp; Shredding Special", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PARA ", ValueDescription = "PARAGON REPRODUCTIONS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PAT  ", ValueDescription = "PATRIOT EXHAUST PRODUCTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PAYBK", ValueDescription = "TOMMY TEES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PB   ", ValueDescription = "MUSCLE CAR MIKE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PBJ  ", ValueDescription = "BLUE JAY J BRA UPHOLSTERY", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PBM IT SO", ValueDescription = "Pbm It Solutions", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PC   ", ValueDescription = "PLASTICOLOR", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PD   ", ValueDescription = "PARTS DUPLICATORS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PE   ", ValueDescription = "P &amp; E RUBBER MFG.CO.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PENGUIN P", ValueDescription = "Penguin Putnam, Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PERF ", ValueDescription = "PERFORMANCE ONLINE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PERMANENT", ValueDescription = "Permanent Impression", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PERRY", ValueDescription = "ERNEST PAPER PRODUCTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PERT ", ValueDescription = "PERTRONIX", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PERTRONEX", ValueDescription = "Pertronix Exhaust", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PETE ", ValueDescription = "PETE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PFT  ", ValueDescription = "POSI FILLER TAG", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PG   ", ValueDescription = "PHOENIX GRAPHIX", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PG CL", ValueDescription = "PG CLASSICS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PG CLSCS ", ValueDescription = "PG CLASSICS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PHOEN", ValueDescription = "PHOENIX GRAPHIX", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PHOENI-AP", ValueDescription = "Phoenix Graphix", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PICK ", ValueDescription = "PICK", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PITNEY BO", ValueDescription = "Pitney Bowes Purchase Power", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PJH BRAND", ValueDescription = "Pjh Brands", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PLAS ", ValueDescription = "PLASTIKOTE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PLAST", ValueDescription = "PLASTIC PARTS INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PLASTIC  ", ValueDescription = "PLASTIC PARTS INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PM   ", ValueDescription = "PM", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PMCC ", ValueDescription = "PETE MCCARTHY AUTHOR/PUBLISHER", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PML  ", ValueDescription = "PML INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PMSTR", ValueDescription = "POWERMASTER", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PNLSS", ValueDescription = "PAINLESS PERFORMANCE PRODUCTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PNT2 ", ValueDescription = "BRYNDANA INTERNATIONAL/COLORBOND", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "POLY ", ValueDescription = "POLY", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PONT ", ValueDescription = "PONTIAC PARADISE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PONT PARA", ValueDescription = "PONTIAC PARADISE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PONTIAC O", ValueDescription = "Pontiac Oakland Club Internati", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PONTIACRE", ValueDescription = "Pontiac Registry.Com", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "POR15", ValueDescription = "POR-15 PRODUCTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "POST OFFI", ValueDescription = "United States Postal Service", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "POSTMASTE", ValueDescription = "Us Postmaster", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "POWER    ", ValueDescription = "Richmond Gears", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "POWERM-AP", ValueDescription = "Powermaster Performance", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PP   ", ValueDescription = "PERFORMANCE YEARS GTOS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PPD  ", ValueDescription = "PPD INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PPD, INC.", ValueDescription = "Ppd, Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PPR      ", ValueDescription = "TOMAHAWK PERFORMANCE PRODUCTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PPR  ", ValueDescription = "PACIFIC PERFORMANCE RACING", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PREFERRED", ValueDescription = "Preferred Paving Company, Inc", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PREST", ValueDescription = "PERFORMANCE RESTORATIONS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PRESTOLIT", ValueDescription = "Prestolite Performance", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PRIMA", ValueDescription = "PRIMA AUTOCRAFT CO LTD", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PRIORITY ", ValueDescription = "Priority Mailing Systems, Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PRO  ", ValueDescription = "PRO", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PROCR", ValueDescription = "SCAT ENTERPRISES INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PROFESSIO", ValueDescription = "Professional Plastics", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PROLIANCE", ValueDescription = "Proliance", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PROMEDIA ", ValueDescription = "Promedia, Llc", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PROPL", ValueDescription = "PRO PLASTICS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PROPR", ValueDescription = "PROFESSIONAL PRODUCTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PROPROD  ", ValueDescription = "PROFESSIONAL PRODUCTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PROTO", ValueDescription = "PROTO BLANK,INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PRP  ", ValueDescription = "PRECISION REPLACEMENT PARTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PSST ", ValueDescription = "PERFORMANCE STAINLESS STEEL INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PTE  ", ValueDescription = "PTE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PUBLICATI", ValueDescription = "Publication Printers Corp.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PUI  ", ValueDescription = "PARTS UNLIMITED INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PUI2 ", ValueDescription = "PARTS UNLIMITED", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PUI3 ", ValueDescription = "PARTS UNLIMITED INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PUI4 ", ValueDescription = "PARTS UNLIMITED", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PUI5     ", ValueDescription = "NOT A VALID VENDOR", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PUI5 ", ValueDescription = "NOT A VALID ACCOUNT", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PUI5A     ", ValueDescription = ".", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PUI6 ", ValueDescription = "PARTS UNLIMITED INTERIORS, INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PUI7 ", ValueDescription = "PARTS UNLIMITED INTERIORS, INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PUI8     ", ValueDescription = "PARTS UNLIMITED INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PUI9     ", ValueDescription = "PARTS UNLIMITED INTERIORS INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PW   ", ValueDescription = "PW", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PWR  ", ValueDescription = "POWER GRAPHICS WORLDWIDE CORP", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PWRLD    ", ValueDescription = "PW DIST. (PONTIAC WORLD)", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PWRLD", ValueDescription = "PWRLD", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PWRSTEER ", ValueDescription = "HUICHANG ELECTROMECHANICAL CO. LTD.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PYCL ", ValueDescription = "PERFORMANCE YEARS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "PYPES", ValueDescription = "PYPES PERFORMANCE EXHAUST", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "QA1  ", ValueDescription = "QA1 PRECISION PRODUCTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "QHS  ", ValueDescription = "QUALITY HEAT SHIELD", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "QUIET", ValueDescription = "QUIET RIDE SOLUTIONS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "QUIETRIDE", ValueDescription = "QUIET RIDE SOLUTIONS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "QUILL COR", ValueDescription = "Quill Corporation", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "QUNTA", ValueDescription = "QUANTA RESTO &amp; PERFORMANCE PRO", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "QVC  ", ValueDescription = "OVC INDUSTRIES INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "R MILLER ", ValueDescription = "RON MILLER", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "R&amp;R SANDB", ValueDescription = "R&amp;R Sandblasting Co.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "R.R. DONN", ValueDescription = "R.R. Donnelley Receivables, Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RACE CAR ", ValueDescription = "Race Car Dynamics", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RACE-", ValueDescription = "RACE CAR DYNAMICS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RACE-AP  ", ValueDescription = "Race Car Dynamics", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RADI ", ValueDescription = "RADI'S CUSTOM UPHOLSTERY", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RAINBOW D", ValueDescription = "Rainbow Disposal", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RARE ", ValueDescription = "RARE PARTS INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RAZO ", ValueDescription = "RAZO", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RAZOR", ValueDescription = "WICKED WILLYS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RB   ", ValueDescription = "ROBERT BENTLY INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RCD  ", ValueDescription = "RACECAR DYNAMICS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RED LINE ", ValueDescription = "Red Line", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RELIABLE ", ValueDescription = "Reliable Carriers Inc", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "REM  ", ValueDescription = "REM AUTOMOTIVE INV", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "REMY ", ValueDescription = "REMY RACING &amp; PERFORMANCE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "REPOP", ValueDescription = "RE-POPS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "REPOPS   ", ValueDescription = "Re-Pops", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RESTO", ValueDescription = "RESTO PERFECT LLC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RESTOPERF", ValueDescription = "RESTO PERFECT LLC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RETRO", ValueDescription = "AUDIO RETRO", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "REVOLUTIO", ValueDescription = "REVOLUTION ELECTRONICS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RIDES", ValueDescription = "BRENTWOOD COMMUNICATIONS INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RIGO'S   ", ValueDescription = "Rigo's Fence Company", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RJ   ", ValueDescription = "DAVIDSON AND CHUNG SIGN COMPANY", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RJL FAST ", ValueDescription = "RJ&amp;L FASTENERS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RK   ", ValueDescription = "RK", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RL   ", ValueDescription = "REDLINE SYNTHETIC OIL CORP", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RM   ", ValueDescription = "RETRO MANUFACTURING", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RMI  ", ValueDescription = "ROTONICS MANUFACTURING INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ROAD ", ValueDescription = "ROAD", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ROADRUNNE", ValueDescription = "Roadrunner Dawes", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ROADWAY E", ValueDescription = "Roadway Express Inc", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RON  ", ValueDescription = "CUSTOM AUTO INTERIORS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RON MANGU", ValueDescription = "Ron Mangus Hot Rod Interiors", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RONMN", ValueDescription = "RONMAN PRODUCTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RONS     ", ValueDescription = "Rons Transmission Service", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RONS OLD ", ValueDescription = "SEE MESSAGE TAB", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RONS OLD2 ", ValueDescription = "RON'S OLDE CAR FIX-IT SHOPPE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ROSS ", ValueDescription = "ROSS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ROUNDBRIX", ValueDescription = "Roundbrix", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RP       ", ValueDescription = "RACING POWER COMPANY", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RPD  ", ValueDescription = "REPLICA PLASTICS OF DOTHAN", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RPM  ", ValueDescription = "RPM", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RS   ", ValueDescription = "RS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RSSI ", ValueDescription = "RESTORATION SPECIALTIES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RT COPIER", ValueDescription = "Reproduction Technology Systems", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RUBBE", ValueDescription = "RUBBER THE RIGHT WAY", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RUBBER   ", ValueDescription = "RUBBER THE RIGHT WAY", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RUCKUS RO", ValueDescription = "Rukus Rod And Kustom", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RUKUS", ValueDescription = "RUCKUS ROD AND KUSTOM", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RUSSL", ValueDescription = "RUSSELL PERFORMANCE PRODUCTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RW   ", ValueDescription = "RALPH WHITE MERCHANDISING", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RYMAL", ValueDescription = "RYMAL'S RESTORATIONS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "RZ   ", ValueDescription = "A-PARTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SA   ", ValueDescription = "CAR TECH", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SAFE ", ValueDescription = "SAFE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SAIA FREI", ValueDescription = "720524060", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SB   ", ValueDescription = "SB", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SCAL ", ValueDescription = "SO-CAL SPEED SHOP", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SCCCC    ", ValueDescription = "So. Ca. Chevelle Camino Club", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SCE  ", ValueDescription = "SCE GASKETS INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SCG  ", ValueDescription = "SURF CITY GARAGE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SCHUM", ValueDescription = "SCHUM", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SCMH     ", ValueDescription = "Southern California Material Handli", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SCP  ", ValueDescription = "SPECIALTY REPRODUCTIONS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SEALED AI", ValueDescription = "Sealed Air Corporation", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SEATB", ValueDescription = "SEATBELT SOLUTIONS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SEATBELT ", ValueDescription = "SEATBELT SOLUTIONS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SEI  ", ValueDescription = "SEI", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SEMA ", ValueDescription = "SEMA/AI HEADQUARTERS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SHAF ", ValueDescription = "SHAFER'S CLASSIC REPRODUCTION", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SHERM", ValueDescription = "SHERMAN &amp; ASSOCIATES INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SHIFT", ValueDescription = "SHIFTWORKS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SHIN ", ValueDescription = "SHIN SHAN IDUSTRIAL CORP", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SHMAR", ValueDescription = "SHEE-MAR", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SIERRA SP", ValueDescription = "Sierra Springs Water", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SIGMA INT", ValueDescription = "Sigma International Business Machin", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SILK ", ValueDescription = "STRAND ART CO", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SKATE", ValueDescription = "MERRICK MACHINE COMPANY", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SLADDEN E", ValueDescription = "Sladden Engineering", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SLOT ", ValueDescription = "POWER SLOT", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SMART AND", ValueDescription = "Smart And Final", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SMI  ", ValueDescription = "SEAN MURPHY INDUCTION", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SMOKM", ValueDescription = "STREET IMAGE RACEWEAR", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SNAKE", ValueDescription = "SSNAKE-OYL", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SO CAL EX", ValueDescription = "Southern California Exterminators", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SO-CAL SA", ValueDescription = "So-Cal Sales &amp; Marketing", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SOA  ", ValueDescription = "GRANT", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SOCAL", ValueDescription = "SDS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SONIC", ValueDescription = "SONIC MOTORS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SOURCE IN", ValueDescription = "Source Interlink Co.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SOUTH COA", ValueDescription = "South Coast Air Quailty Mgt. Dst.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SPECI", ValueDescription = "SPECIALTY PRODUCTS CO.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SPECIALTY", ValueDescription = "SPECIALTY PRODUCTS CO.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SPECTRA P", ValueDescription = "Spectra Premium", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SPECTRUM ", ValueDescription = "Spectrum Information Services", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SPLAT", ValueDescription = "JOHN BERLAGE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SPORT", ValueDescription = "SPORT", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SPOTS", ValueDescription = "SPOTS PERFORMANCE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SPOTTS PE", ValueDescription = "Spotts Performance", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SPRAD", ValueDescription = "PINNACLE SALES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SPW  ", ValueDescription = "SPECIALTY PARTS WAREHOUSE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SSB  ", ValueDescription = "SSB", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SSBC ", ValueDescription = "STAINLESS STEEL BRAKES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SSC  ", ValueDescription = "SSC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SSI SYSTE", ValueDescription = "Ssi Systems, Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SSU  ", ValueDescription = "SSU", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "STAFF PRO", ValueDescription = "Staff Pro Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "STAPLES  ", ValueDescription = "Staples Business Advantage", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "STATE BOA", ValueDescription = "State Board Of Equalization", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "STEDY", ValueDescription = "STEADY CLOTHING", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "STEEL", ValueDescription = "STEELE RUBBER PRODUCTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "STEERING ", ValueDescription = "Steering Superstore", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "STETINA B", ValueDescription = "Stetina Brunda Garred &amp; Brucker", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "STEWART M", ValueDescription = "Stewart Media Inc", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "STOOL", ValueDescription = "SMART INCENTIVES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "STULL", ValueDescription = "STULL INDUSTRIES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SUEDE", ValueDescription = "SUEDE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SUNFIRE  ", ValueDescription = "SUN FIRE INDUSTRIES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SUNSHINE ", ValueDescription = "Sunshine Windows", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SUPERIOR ", ValueDescription = "Superior Press", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SW   ", ValueDescription = "SPECIALTY WHEEL", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SW SPEED ", ValueDescription = "SOUTHWEST SPEED", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "SWISS", ValueDescription = "SWISS TECH PRODUCTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TA SECURI", ValueDescription = "Ta Security", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TABCO", ValueDescription = "TABCO", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TACH ", ValueDescription = "TACH TECH", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TAIT &amp; AS", ValueDescription = "Tait &amp; Associates, Inc", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TALIMAR S", ValueDescription = "Talimar Systems", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TAP  ", ValueDescription = "TA PERFORMANCE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TAX  ", ValueDescription = "TAXOR INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TAYLR", ValueDescription = "TAYLOR CABLE PRODUCTS, INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TBP  ", ValueDescription = "TBP", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TC   ", ValueDescription = "TC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TCI  ", ValueDescription = "TCI AUTOMOTIVE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TD   ", ValueDescription = "TD PERFORMANCE PRODUCTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TEAM ", ValueDescription = "TEAM", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TECH ", ValueDescription = "TECHNOSTALGIA", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TECHN", ValueDescription = "TECHNOSTALGIA", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TECHNO-AP", ValueDescription = "Technostalgia", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TEEZE", ValueDescription = "ACE LEATHER", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TELECHECK", ValueDescription = "Telecheck Services, Inc", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TELREX   ", ValueDescription = "Telrex", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TEST ", ValueDescription = "TEST", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TF   ", ValueDescription = "TRU-FORM", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "THE CIT G", ValueDescription = "Cit Technology Fin Serv, Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "THE FINIS", ValueDescription = "The Finished Look", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "THE GAS C", ValueDescription = "The Gas Company", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "THE GENER", ValueDescription = "The General's Store", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "THE TRANS", ValueDescription = "The Transportation Book Ser", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "THERMO-TE", ValueDescription = "Thermo-Tec Automotive Inc", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "THOMS", ValueDescription = "THOMSON PERFORMANCE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "THOMSON  ", ValueDescription = "THOMSON PERFORMANCE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "THORN", ValueDescription = "THORNTON REPRODUCTIONS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "THRN2", ValueDescription = "THORNTON REPRODUCTIONS/FAN BELTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TILT ", ValueDescription = "TILT", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TITO ", ValueDescription = "TITO", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TLK  ", ValueDescription = "BAD ACCT", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TLM  ", ValueDescription = "TIM BENKO", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TMA  ", ValueDescription = "THOMAS,MCKENNA &amp; ASSOC.INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TMC  ", ValueDescription = "THE MOTOR COMPANY", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TMI      ", ValueDescription = "TMI PRODUCTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TOMT ", ValueDescription = "MR. WILLIAM MORENO", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TOOL ", ValueDescription = "CAL VAN TOOLS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TOWER", ValueDescription = "APS / TOWER PAINT", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TPP  ", ValueDescription = "THE PARTS PLACE INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TRADINGBE", ValueDescription = "Tradingbell, Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TRANS-DAP", ValueDescription = "Trans-Dapt Performance", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TRANS-GLO", ValueDescription = "Trans-Global International", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TRICO", ValueDescription = "TRICO", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TRIMPEX  ", ValueDescription = "T.R. IMPEX", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TRIO ", ValueDescription = "TRIO METAL STAMPING", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TRITON BU", ValueDescription = "Triton Business Park, Llc", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TRUSTWAVE", ValueDescription = "Trustwave", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TUBE ", ValueDescription = "TUBE TAINER", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TUBE-TAIN", ValueDescription = "Tube-Tainer", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TW   ", ValueDescription = "TW", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "TWCP ", ValueDescription = "TED WILLIAMS ENTERPRISES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "U.S. WHEE", ValueDescription = "U.S. Wheel Corporation", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "UBI  ", ValueDescription = "UNIVERSAL BRASS CO.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ULTIMATE ", ValueDescription = "Ultimate Landscape Management Co.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "UNI  ", ValueDescription = "A-UNICORN T-SHIRTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "UNITED PA", ValueDescription = "United Parcel Service", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "UNIV ", ValueDescription = "UNIVERSAL MOLDING COMPANY INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "UNIVE", ValueDescription = "UNIVERSAL URETHANE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "UNIVER-AP", ValueDescription = "Universal Urethane", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "UP   ", ValueDescription = "UP", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "UPS FREIG", ValueDescription = "Ups Freight", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "UPS SUPPL", ValueDescription = "Ups Supply Chain Solutions", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "UR   ", ValueDescription = "US RADIATOR", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "US BANK C", ValueDescription = "Us Bank California Indirect", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "USA  ", ValueDescription = "USA PARTS SUPPLY,LTD.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "USA SNAKE", ValueDescription = "Usa Snake Oyl Products, Inc", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "USA1 ", ValueDescription = "USA1", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "USWHL", ValueDescription = "US WHEEL CORPORATION", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "VA   ", ValueDescription = "VINTAGE AIR", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "VAN KAMPE", ValueDescription = "Van Kampen Trust Company", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "VELOCITY ", ValueDescription = "Velocity Magazine L.L.C.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "VERIZON 0", ValueDescription = "Verizon", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "VERIZON 1", ValueDescription = "Verizon California", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "VERIZON 6", ValueDescription = "Verizon California", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "VERIZON 7", ValueDescription = "Verizon California", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "VERIZON W", ValueDescription = "Verizon Wireless", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "VET  ", ValueDescription = "VET", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "VHT  ", ValueDescription = "SHERWIN WILLIAMS CO.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "VHT AEROS", ValueDescription = "The Sherwin-Williams  Company", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "VIC  ", ValueDescription = "VICAR", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "VP SALES ", ValueDescription = "Vp Sales", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "VSFAB", ValueDescription = "VERSAFAB", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "VSR ENTER", ValueDescription = "Vsr Enterprises", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "VW   ", ValueDescription = "VW", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "WAGNR", ValueDescription = "WAGNR", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "WANG ", ValueDescription = "WANG INTERNATIONAL INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "WANG'S IN", ValueDescription = "Wang' International", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "WARRE", ValueDescription = "WARREN DISTRIBUTING INC PLAY P P", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "WARREN-AP", ValueDescription = "Warren Distributing Inc", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "WARRN", ValueDescription = "WARREN DIST.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "WCAST", ValueDescription = "WCAST", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "WCG  ", ValueDescription = "WEST COAST GASKET", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "WDGRN", ValueDescription = "GREG SETTER", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "WEFCO", ValueDescription = "WEFCO RUBBER MANUFACTURING CO", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "WEN  ", ValueDescription = "WEN", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "WESLEY AL", ValueDescription = "Wesley Allison Photography", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "WFO1 ", ValueDescription = "WFO1", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "WFO2 ", ValueDescription = "WFO2", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "WHEEL", ValueDescription = "WHEEL", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "WHEEL PRO", ValueDescription = "Wheel Pros, Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "WHG  ", ValueDescription = "WHITE HOUSE GRAPHICS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "WHITE", ValueDescription = "WHITESIDE MANUFACTURING CO, INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "WILEY", ValueDescription = "JAMES WILEY CO", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "WILWOOD  ", ValueDescription = "WILWOOD ENGINEERING", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "WINDO", ValueDescription = "NU-RELICS POWER WINDOWS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "WINTERS &amp;", ValueDescription = "L/O Winters &amp; Banks", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "WIRES", ValueDescription = "LECTRIC LIMITED INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "WIX  ", ValueDescription = "WIX TOOLS/WESTERN INDUSTRIAL INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "WIZ  ", ValueDescription = "WIZARD INDUSTRIES, INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "WLK  ", ValueDescription = "WLK", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "WOLVE", ValueDescription = "WOLVE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "WRP  ", ValueDescription = "WARPATH RESTORATION PARTS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "WV   ", ValueDescription = "WHEEL VINTIQUES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "WWA  ", ValueDescription = "WORLD WIDE AUTO", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "WYSCO", ValueDescription = "WYSCO INC", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "X-TREME M", ValueDescription = "X-TREME METAL WORKS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "YEAR ONE ", ValueDescription = "YEAR ONE INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "YELLOW   ", ValueDescription = "Yellow Transportation Inc.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "YM   ", ValueDescription = "YM", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "YOKES", ValueDescription = "POWERTRAIN INDUSTRIES", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "YRONE    ", ValueDescription = "YEAR ONE INC.", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "YRONE", ValueDescription = "YEAR ONE", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ZONES    ", ValueDescription = "Zones Business Solutions", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "ZOOPS", ValueDescription = "ZOOPS", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                db.Insert(new FilterValue { Value = "00", ValueDescription = "MISSING", FilterTypeId = (int)(ENT.FilterType.VendorCode), BatchID = 0 });
                #endregion

                //Location
                db.Insert(new FilterValue { Value = "OPGI01", ValueDescription = "Original Parts Group, Seal Beach CA", FilterTypeId = (int)(ENT.FilterType.Location), BatchID = 0 });

                //Pricing Type
                db.Insert(new FilterValue { Value = "1101", ValueDescription = "On Sale", FilterTypeId = (int)(ENT.FilterType.PricingType), BatchID = 0 });
                db.Insert(new FilterValue { Value = "1102", ValueDescription = "Regular price", FilterTypeId = (int)(ENT.FilterType.PricingType), BatchID = 0 });

                //Package Type
                db.Insert(new FilterValue { Value = "1201", ValueDescription = "Kit or bundle", FilterTypeId = (int)(ENT.FilterType.PackageType), BatchID = 0 });
                db.Insert(new FilterValue { Value = "1202", ValueDescription = "Single item", FilterTypeId = (int)(ENT.FilterType.PackageType), BatchID = 0 });







            }
        }



        [TestMethod]
        [TestCategory("MigrateSql")]
        public void Step_SeedPriceLists()
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                db.DeleteAll<PriceList>();
                db.Insert(new PriceList { PriceListType = "R", Code = "0", Name = "Cost", Sort = 1, BatchID = 0 });
                db.Insert(new PriceList { PriceListType = "R", Code = "L", Name = "Retail List price", Sort = 2, BatchID = 0 });
                db.Insert(new PriceList { PriceListType = "R", Code = "LF", Name = "Retail List price *FUTURE PRICE*", Sort = 3, BatchID = 0 });
                db.Insert(new PriceList { PriceListType = "R", Code = "R", Name = "Retail Sale price", Sort = 4, BatchID = 0 });
                db.Insert(new PriceList { PriceListType = "R", Code = "RF", Name = "Retail Sale price *FUTURE PRICE*", Sort = 5, BatchID = 0 });
                db.Insert(new PriceList { PriceListType = "R", Code = "C", Name = "Dealer Sugg Retail", Sort = 6, BatchID = 0 });
                db.Insert(new PriceList { PriceListType = "R", Code = "J", Name = "Jobber - Trim Shop", Sort = 7, BatchID = 0 });

                db.Insert(new PriceList { PriceListType = "R", Code = "D", Name = "Dealer Std Price", Sort = 8, BatchID = 0 });
                db.Insert(new PriceList { PriceListType = "R", Code = "1", Name = "Dealer 1", Sort = 9, BatchID = 0 });
                db.Insert(new PriceList { PriceListType = "R", Code = "2", Name = "Dealer 2", Sort = 10, BatchID = 0 });
                db.Insert(new PriceList { PriceListType = "R", Code = "3", Name = "Dealer 3", Sort = 11, BatchID = 0 });
                db.Insert(new PriceList { PriceListType = "R", Code = "4", Name = "Dealer 4", Sort = 12, BatchID = 0 });
            }
        }


        [TestMethod, Ignore]
        [TestCategory("MigrateSql")]
        public void Step1a_DropPricing()
        {

            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                //if (db.TableExists<DB.PricingResultEditType>()) db.DropTable<DB.PricingResultEditType>();
                //if (db.TableExists<DB.PricingResultWarningType>()) db.DropTable<DB.PricingResultWarningType>();
                //if (db.TableExists<DB.PricingResult>()) db.DropTable<DB.PricingResult>();
                //if (db.TableExists<DB.PricingFilter>()) db.DropTable<DB.PricingFilter>();
                //if (db.TableExists<DB.PricingDriver>()) db.DropTable<DB.PricingDriver>();
                //if (db.TableExists<DB.PricingPriceList>()) db.DropTable<DB.PricingPriceList>();
                //if (db.TableExists<DB.PricingResult>()) db.DropTable<DB.PricingResult>();

                //if (db.TableExists<DB.Pricing>()) db.DropTable<DB.Pricing>();

            }
        }
        [TestMethod]
        [TestCategory("MigrateSql")]
        public void Step7_SeedPricing()
        {

            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                db.DeleteAll<PricingResultEditType>();
                //db.CreateTable<PricingResultEditType>();
                db.InsertAll(new[]
                {                        
                    new PricingResultEditType { PricingResultEditTypeId = 1, Name = "ExcludeUpdate"},
                    new PricingResultEditType { PricingResultEditTypeId = 2, Name = "DefaultPrice"},
                    new PricingResultEditType { PricingResultEditTypeId = 3, Name = "DefaultToCurrentPrice"},
                    new PricingResultEditType { PricingResultEditTypeId = 4, Name = "DefaultToMaxMarkup"},
                    new PricingResultEditType { PricingResultEditTypeId = 5, Name = "DefaultToMinMarkup"},
                    new PricingResultEditType { PricingResultEditTypeId = 6, Name = "EditNewPrice"},
                    new PricingResultEditType { PricingResultEditTypeId = 7, Name = "EditNewMarkup"},
                    new PricingResultEditType { PricingResultEditTypeId = 8, Name = "EditRemoveRounding"},
                    new PricingResultEditType { PricingResultEditTypeId = 9, Name = "EditApplyRounding"}

                });

                db.DeleteAll<PricingResultWarningType>();
                //db.CreateTable<PricingResultWarningType>();
                db.InsertAll(new[]
                {                        
                    new PricingResultWarningType { Id = 1, Name = "MarkupPassed"},
                    new PricingResultWarningType { Id = 2, Name = "MarkupBelow"},
                    new PricingResultWarningType { Id = 3, Name = "MarkupAbove"},
                    new PricingResultWarningType { Id = 4, Name = "DefaultToMaxMarkup"}

                });


                db.DeleteAll<PricingMode>();

                db.DeleteAll<PricingDriver>();
                db.DeleteAll<PricingPriceList>();
                db.DeleteAll<PricingResult>();
                //db.CreateTable<PricingDriver>();
                db.DeleteAll<Pricing>();



            }
        }


        [TestMethod, Ignore]
        [TestCategory("MigrateSql")]
        public void Step9_SeedPricingResults()
        {

            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                db.CreateTable<Product>();
                db.CreateTable<ProductPrice>();
                db.CreateTable<PricingResult>();

            }
        }


        [TestMethod, Ignore]
        [TestCategory("MigrateSql")]
        public void Step9_SeedProductDetail()
        {

            var analyticProducts = Enumerable.Range(1, 100000).Select(i => new ProductDetail
            {
                Filters = Enumerable.Repeat(0, r.Next(minFilterCount, maxFilterCount)).Select(j => r.Next(minFilter, maxFilter)).ToArray(),
                SkuId = string.Format("Sku{0}", i),
                DaysOnHand = r.Next(minDays, maxDays),
                Markup = r.Next(minMarkup, maxMarkup),
                Movement = r.Next(minMovement, maxMovement),
                SalesValue = r.Next(minSales, maxSales)
            });
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                if (db.TableExists<ProductDetail>()) db.DropTable<ProductDetail>();

                db.CreateTable<ProductDetail>();
                db.InsertAll<ProductDetail>(analyticProducts);
            }

        }


        [TestMethod, Ignore]
        [TestCategory("MigrateSql")]
        public void Step9_QueryProductDetail()
        {

            var filters = new[] { 1991, 1099, 1594, 1311 };

            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                //var skus = db.Select<AnalyticProduct>(f => f.Filters.Any(j => filters.Contains(j)));

            }

        }

        [TestMethod]
        [TestCategory("MigrateSql")]
        public void Cleanup()
        {
            //TODO: deleteall records
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {

                //db.DeleteAll<DB.AnalyticPriceList>();
                //db.DeleteAll<DB.AnalyticFilter>();
                db.DeleteAll<DB.AnalyticDriver>();
                db.DeleteAll<DB.Analytic>();

                //if (db.TableExists<DB.PricingResultEditType>()) db.DropTable<DB.PricingResultEditType>();
                //if (db.TableExists<DB.PricingResultWarningType>()) db.DropTable<DB.PricingResultWarningType>();
                //if (db.TableExists<DB.PricingResult>()) db.DropTable<DB.PricingResult>();
                //if (db.TableExists<DB.PricingFilter>()) db.DropTable<DB.PricingFilter>();
                //if (db.TableExists<DB.PricingDriver>()) db.DropTable<DB.PricingDriver>();
                //if (db.TableExists<DB.PricingPriceList>()) db.DropTable<DB.PricingPriceList>();
                //if (db.TableExists<DB.PricingResult>()) db.DropTable<DB.PricingResult>();


                //if (db.TableExists<Filter>()) db.DropTable<Filter>();

                //if (db.TableExists<PriceList>()) db.DropTable<PriceList>();
                //if (db.TableExists<FilterType>()) db.DropTable<FilterType>();
                //if (db.TableExists<DriverType>()) db.DropTable<DriverType>();

                //if (db.TableExists<DB.User>()) db.DropTable<DB.User>();
                //if (db.TableExists<DB.Role>()) db.DropTable<DB.UserRole>();
                //if (db.TableExists<DB.ModuleFeature>()) db.DropTable<DB.ModuleFeature>();
                //if (db.TableExists<DB.Module>()) db.DropTable<DB.Module>();


                //if (db.TableExists<DB.RoundingTemplate>()) db.DropTable<DB.RoundingTemplate>();
                //if (db.TableExists<DB.OptimizationTemplate>()) db.DropTable<DB.OptimizationTemplate>();
                //if (db.TableExists<DB.MarkupTemplate>()) db.DropTable<DB.MarkupTemplate>();
                ////if (db.TableExists<DB.Template>()) db.DropTable<DB.Template>();
                //if (db.TableExists<DB.TemplateType>()) db.DropTable<DB.TemplateType>();
                //if (db.TableExists<DB.RoundingType>()) db.DropTable<DB.RoundingType>();

            }
        }
    }
}
