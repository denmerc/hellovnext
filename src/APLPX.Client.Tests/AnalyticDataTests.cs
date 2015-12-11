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
    [TestClass]
    public class AnalyticDataTests
    {
        public AnalyticData AnalyticRepo { get; set; }
        int minInt = 100; int maxInt = 100000;
        int minDriver = 1, maxDriver = 3;
        int minGroup = 1; int maxGroup = 15;
        int minDecimal = 1500; int maxDecimal = 500000;
        int minFilter = 1000; int maxFilter = 1500;
        int minFilterType = 1; int maxFilterType = 11;
        int minPriceList = 1; int maxPriceList = 12;
        Random r = new Random();

        public OrmLiteConnectionFactory DBConnectionFactory { get; set; }
        public int AnalyticIdToTest { get; set; }
        [TestInitialize]
        public void Initialize()
        {

            DBConnectionFactory = new OrmLiteConnectionFactory(ConfigurationManager.AppSettings["localConnectionString"], SqlServerDialect.Provider);
            OrmLiteConfig.DialectProvider.NamingStrategy = new OrmLiteNamingStrategyBase();

            AnalyticRepo = new AnalyticData();
            
            //using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            //{

            //    //db.Delete<DB.AnalyticFilter>(af => af.AnalyticId == AnalyticIdToTest);
            //    //db.Delete<DB.AnalyticDriver>();

            //    //db.Delete<DB.AnalyticPriceList>(af => af.AnalyticId == AnalyticIdToTest);
            //    //db.Delete<DB.AnalBytic>(u => u.Name == "test");
            //    //db.Delete<DB.Analytic>(u => u.Name == "testInsert");

            //    var filters = db.Select<DB.FilterValue>().Where(a => a.FilterTypeId == r.Next(minFilterType, maxFilterType));
            //    var filterGroups = (from p in filters
            //                        group p by p.FilterTypeId into g
            //                        select new DB.FilterGroup
            //                        {
            //                            Name = ((ENT.FilterType)g.Key).ToString(),
            //                            Filters = g.Select(fv => new DB.Filter
            //                            {
            //                                Value = fv.Value
            //                            }).ToArray()
            //                        }).ToArray();


            //    db.Insert(new DB.Analytic
            //    {
            //        //Id = 3,
            //        ClusterType = "D",
            //        Name = "test",
            //        Description = "Admin - Everyday - All Filters - Movement Only",
            //        FolderID = 76,
            //        Notes = "Optional notes...",
            //        CreateTS = DateTime.UtcNow,
            //        OwnerId = 2,
            //        ProductFilters = filterGroups,
            //        PriceLists = Enumerable.Repeat(0, 2).Select(i => r.Next(minPriceList, maxPriceList)).ToArray()
            //    });

            //    AnalyticIdToTest = db.Single<DB.Analytic>(u => u.Name == "test").AnalyticId;

            //}

        }

        [TestCleanup]
        public void CleanUp()
        {
            //db.DeleteAll<DB.AnalyticPriceList>();
            //db.DeleteAll<DB.AnalyticFilter>();
            //db.DeleteAll<DB.AnalyticDriver>();
            //db.DeleteAll<DB.Analytic>();

            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {

                db.Delete<DB.AnalyticDriver>(af => af.AnalyticId == AnalyticIdToTest);

                db.Delete<DB.Analytic>(u => u.Name == "test");
                db.Delete<DB.Analytic>(u => u.Name == "testInsert");
                

            }
        }

        public void ArrangeTestAnalytics()
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                var filters = db.Select<DB.FilterValue>().Where(a => a.FilterTypeId == r.Next(minFilterType, maxFilterType));
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
                    //Id = 3,
                    ClusterType = "D",
                    Name = "test",
                    Description = "Admin - Everyday - All Filters - Movement Only",
                    FolderID = 76,
                    Notes = "Optional notes...",
                    CreateTS = DateTime.UtcNow,
                    OwnerId = 2,
                    ProductFilters = filterGroups,
                    PriceLists = Enumerable.Repeat(0, 2).Select(i => r.Next(minPriceList, maxPriceList)).ToArray()
                });

                AnalyticIdToTest = db.Single<DB.Analytic>(u => u.Name == "test").AnalyticId;
            }
        }

        public void ArrangeDrivers()
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                AnalyticIdToTest = db.Single<DB.Analytic>(u => u.Name == "test").AnalyticId;
                var drivers = Enumerable.Range(1, r.Next(minDriver, maxDriver))
                    .Select(i => new DB.AnalyticDriver
                    {
                        AnalyticId = AnalyticIdToTest,
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

            }
        }

        [TestMethod]
        public void Analytic_Load_Existing()
        {
            ArrangeTestAnalytics();

            var response = AnalyticRepo.Load(AnalyticIdToTest);
            Assert.IsNotNull(response);
        }

        [TestMethod]
        public void Analytic_Load_NonExisting()
        {
            ArrangeTestAnalytics();

            var response = AnalyticRepo.Load(999999);
            Assert.IsNull(response);
        }

        [TestMethod]
        public void Analytic_Load_Id_as_0()
        {
            ArrangeTestAnalytics();

            var response = AnalyticRepo.Load(0);
            Assert.IsNull(response);
        }
           
        [TestMethod]
        public void Analytic_SaveNew()
        {
            var a = new Analytic
            {
                Id = 0,
                Name = "testInsert",
                Description = "DescriptionNew",
                Notes = "Notes",
                OwnerName = "admin",
                IsActive = true,
                //IsShared = false,
                FolderId = 76
            };

            var response = AnalyticRepo.Save(a);
            Assert.AreNotEqual(0, response);

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Analytic_SaveWithNoOwner()
        {
            var a = new Analytic
            {
                Id = 0,
                Name = "testInsert",
                Description = "DescriptionNew",
                Notes = "Notes",
                OwnerName = "",
                IsActive = true,
                //IsShared = false,
                FolderId = 76
            };

            var response = AnalyticRepo.Save(a);
            Assert.AreNotEqual(0, response);

        }

        [TestMethod]
        public void Analytic_SaveNew_ButAlreadyExists() //TODO: EXPECT  FALSE
        {
            ArrangeTestAnalytics();
            var a = new Analytic
            {
                Id = 0,
                Name = "test",
                Description = "DescriptionNew",
                Notes = "Notes",
                OwnerName = "admin",
                IsActive = true,
                //IsShared = false,
                FolderId = 75
            };

            var response = AnalyticRepo.Save(a);
            Assert.AreNotEqual(0, response);

        }

        [TestMethod]
        public void Analytic_SaveExisting()
        {
            //arrange
            ArrangeTestAnalytics();
            var a = new Analytic
            {
                Id = AnalyticIdToTest,
                Name = "test",
                Description = "Description for Existing Analytic Updated",
                Notes = "Notes",
                OwnerName = "admin",
                IsActive = true,
                //IsShared = false,
                FolderId = 76
            };

            //act
            var response = AnalyticRepo.Save(a);
            Assert.AreNotEqual(0, response);

            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                DB.Analytic updated = db.Single<DB.Analytic>(up => up.Name == "test");
                Assert.AreEqual("Description for Existing Analytic Updated", updated.Description);
            }

        }

        [TestMethod]
        public void Analytic_LoadFilters()
        {
            ArrangeTestAnalytics();

            var response = AnalyticRepo.LoadFilters(AnalyticIdToTest);
            Assert.IsNotNull(response);
        }


        [TestMethod]
        public void Analytic_LoadPriceLists()
        {
            ArrangeTestAnalytics();

            var response = AnalyticRepo.LoadPriceLists(AnalyticIdToTest);
            Assert.IsNotNull(response);

        }

        [TestMethod]
        public void Analytic_SavePriceListsAsArray()
        {
            ArrangeTestAnalytics();

            var pl = Enumerable.Repeat(0, 2).Select( i => r.Next(minPriceList, maxPriceList)).ToArray();
            var response = AnalyticRepo.SavePriceLists(AnalyticIdToTest, pl);
            Assert.IsTrue(response);
        }

        [TestMethod]
        public void Analytic_SavePriceLists()
        {
            ArrangeTestAnalytics();

            var priceLists = Enumerable.Range(1, 12).Select(i => i).ToArray();
            //var filtersDist = priceLists.Distinct().ToArray();
            var response = AnalyticRepo.SavePriceLists(AnalyticIdToTest, priceLists);
            Assert.IsTrue(response);

        }


        [TestMethod]
        public void Analytic_SaveFiltersAsArray()
        {
            //TODO: select filters from test analytic and modify

            ArrangeTestAnalytics();

            
            var filters = Enumerable.Range(minFilterType,maxFilterType)
                    .Select(i => new FilterGroup
                    {
                        FilterType = i,
                        Filters = 
                                    Enumerable.Repeat(r.Next(minFilter,maxFilter), r.Next(minGroup, maxGroup))
                                    
                                    .Select(fid => new ENT.Filter
                                    {
                                        FilterType = i,
                                        Code = string.Format("Code{0}", fid),
                                        Name = string.Format("Name{0}", fid)
                                    }).ToList()
                                    //Enumerable.Range(minFilter, maxFilter)
                    }).ToArray();

            var response = AnalyticRepo.SaveFilters(AnalyticIdToTest, filters);
            Assert.IsTrue(response);

            //TODO: Assert count of filters

        }

        [TestMethod, Ignore]
        public void Analytic_SaveFilters()
        {
            var filters = Enumerable.Range(1053, 750).Select(i => i).ToArray();
            var filtersDist = filters.Distinct().ToArray();
            //TODO make filter group
            //var response = AnalyticRepo.SaveFilters(AnalyticIdToTest, filtersDist);
            //Assert.IsTrue(response);

        }
        
        [TestMethod]
        public void Analytic_LoadDrivers()
        {
            ArrangeTestAnalytics();            
            ArrangeDrivers();
            var response = AnalyticRepo.LoadDrivers(AnalyticIdToTest);
            Assert.IsNotNull(response);
        }
        
        [TestMethod]
        public void Analytic_SaveDrivers()
        {
            ArrangeTestAnalytics();

            var drivers = Enumerable.Range(1, r.Next(minDriver, maxDriver)).Select( i => new AnalyticValueDriver
                                {

                                    DriverType = (DriverType) i,
                                    Groups = Enumerable.Range(1, r.Next(minGroup, maxGroup))
                                                .Select( j => new ValueDriverGroup 
                                                { 
                                                    GroupNumber = j, 
                                                    MinOutlier = r.Next(minDecimal,maxDecimal),
                                                    MaxOutlier = r.Next(minDecimal, maxDecimal),

                                                }).ToList()
                                }).ToList();
            var response = AnalyticRepo.SaveDrivers( AnalyticIdToTest, drivers);
            Assert.IsTrue(response);
        }
       




        [TestMethod]
        public void LoadModulesForAdmin()
        {
            using(IDbConnection db = DBConnectionFactory.OpenDbConnection())
	        {
                var feats = db.Select<DB.ModuleFeature>().Where(r => r.Roles.Contains(((int)UserRoleType.AplUserRoleAdministrator)));
                var mods = db.Select<DB.Module>();

                var modsandFeats = mods.Select(m => new Module
                    {
                        Name = m.Name,
                        Features = feats
                                .Where(f => f.ModuleId == m.Id)
                                .Select(mf => new ModuleFeature 
                                { 
                                    Name = mf.Name,
                                    Description = mf.Description
                                    //,
                                    //SearchGroups = mf.SearchGroups,
                                }).ToList()
                    }
                    );
	        }
        }

        [TestMethod]
        public void LoadModuleLookup()
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                var mods = db.Select<DB.Module>();
            }
        }

        [TestMethod, Ignore]
        public void LoadTemplatesAsJson()
        {
            using( IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                //var t = db.Select<DB.Template<RoundingTemplateRule>();
                //var items = JsonConvert.SerializeObject(t.First().Rules);
            };
        }


        [TestMethod, Ignore]
        public void LoadRulesFromTemplatesAsDapperJSON()
        {
            //JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            //{
            //    Formatting = Formatting.Indented,
            //    TypeNameHandling = TypeNameHandling.Objects,
            //    ContractResolver = new CamelCasePropertyNamesContractResolver()
            //};


            //var connstring = ConfigurationManager.AppSettings["localConnectionString"];
            //using (var connection = new SqlConnection(connstring))
            //{
            //    var items = connection.Query("Select Rules From RoundingTemplate");
            //    List<DB.RoundingTemplateRule> json = JsonConvert.DeserializeObject<List<DB.RoundingTemplateRule>>(items.First().Rules);

            //    var temps = connection.Query<DB.RoundingTemplate>("Select Name, Description from RoundingTemplate");

            //}
        }

        public bool SaveFiltersAsTable(int analyticId, int[] filtersToInsert)
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                //method 2 - delete all and insert in one atomic transaction
                //using (IDbTransaction dbTrans = db.OpenTransaction(IsolationLevel.ReadCommitted))
                //{
                //db.Delete<DB.AnalyticFilter>(af => af.AnalyticId == analyticId);
                //    db.InsertAll<DB.AnalyticFilter>(filtersToInsert.Select(i => new DB.AnalyticFilter
                //    {
                //        FilterId = i,
                //        AnalyticId = analyticId
                //    }
                //        ));
                return true;

                //}
            }


        }


    }

}
