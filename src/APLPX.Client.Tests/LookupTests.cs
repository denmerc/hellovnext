using APLPX.Entity;
using ServiceStack.OrmLite;
using System;
using System.Data;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using DB = APLPX.Client.Sql.Models;
using ENT = APLPX.Entity;
using System.Configuration;
using Newtonsoft.Json;
using System.Data.SqlClient;
using Dapper;
using APLPX.Client.Sql;


namespace APLPX.Client.Tests
{
    [TestClass, Ignore]
    public class LookupTests
    {
        public LookupData LookupRepo { get; set; }
        public OrmLiteConnectionFactory DBConnectionFactory { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            DBConnectionFactory = new OrmLiteConnectionFactory(ConfigurationManager.AppSettings["localConnectionString"], SqlServerDialect.Provider);
            LookupRepo = new LookupData();
        }

        [TestCleanup]
        public void Cleanup()
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {

            }
        }

        [TestMethod, Ignore]
        public void LoadLookupsAsDictionary()
        {


            //TODO: add description for tooltips
            //SearchGroups,FilterType,Filters,PriceLists,UserRoleType,DriverType,RoundingType,ModeType,TemplateType
            Dictionary<DB.LookupType, dynamic> lookups = new Dictionary<DB.LookupType, dynamic>();


            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {

                List<DB.FilterValue> filters = db.Select<DB.FilterValue>();
                lookups.Add(DB.LookupType.Filters, filters);
                //TODO: change to list filtertype

                var priceLists = db.Select<PriceList>();
                lookups.Add(DB.LookupType.PriceLists, priceLists);

                Assert.IsTrue(lookups.Count == 2);

                //temps.Add("FilterTypes", db.Select<DB.Filter>());

                //var ft = (Enum.GetValues(typeof(FilterType))
                //                .Cast<int>()
                //                .Select(e => new DB.FilterType() { Name = Enum.GetName(typeof(FilterType), e), Id = e }))
                //                .ToList();
                //lookups.Add(DB.LookupType.FilterType, ft);


                //var driverTypes = db.Select<DB.DriverType>();
                //temps.Add(DB.LookupType.DriverType, driverTypes);

                //var driverTypes = (Enum.GetValues(typeof(DriverType))
                //                .Cast<int>()
                //                .Select( e => new DB.DriverType() { Name = Enum.GetName(typeof(DriverType), e), Id = e}))
                //                .ToList();


                //var roles = (Enum.GetValues(typeof(UserRoleType))
                //                .Cast<int>()
                //                .Select(e => new DB.UserRole() { Name = Enum.GetName(typeof(UserRoleType), e), Id = e }))
                //                .ToList();
                //lookups.Add(DB.LookupType.UserRoleType, roles);

                //var roundingTypes = (Enum.GetValues(typeof(RoundingType))
                //                .Cast<int>()
                //                .Select(e => new DB.RoundingType() { Name = Enum.GetName(typeof(RoundingType), e), Id = e }))
                //                .ToList();
                //lookups.Add(DB.LookupType.RoundingType, roundingTypes);

                //var modeTypes = (Enum.GetValues(typeof(DriverGroupMode))
                //                .Cast<int>()
                //                .Select(e => new DB.DriverGroupMode() { Name = Enum.GetName(typeof(DriverGroupMode), e), Id = e }))
                //                .ToList();
                //lookups.Add(DB.LookupType.DriverGroupMode, modeTypes);


                //var pricingModeTypes = (Enum.GetValues(typeof(PricingMode))
                //                .Cast<int>()
                //                .Select(e => new DB.PricingMode() { Name = Enum.GetName(typeof(PricingMode), e), Id = e }))
                //                .ToList();
                //lookups.Add(DB.LookupType.PricingMode, pricingModeTypes);

                //var templateTypes = (Enum.GetValues(typeof(TemplateType))
                //                .Cast<int>()
                //                .Select(e => new DB.TemplateType() { Name = Enum.GetName(typeof(TemplateType), e), Id = e }))
                //                .ToList();
                //lookups.Add(DB.LookupType.TemplateType, templateTypes);

            }



        }

        [TestMethod, Ignore]
        public void LoadFilterGroupLookup()
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {

                var filterGroups = db.Select<DB.FilterValue>()
                    .GroupBy(f => f.FilterTypeId).Select(ig =>
                                new FilterGroup
                                {
                                    FilterType = ig.Key,
                                    Name = Enum.GetName(typeof(ENT.FilterType), ig.Key),
                                    Filters = ig.Select(igf => new ENT.Filter
                                    {
                                        FilterType = igf.FilterTypeId,
                                        Code = igf.Value,
                                        Name = igf.ValueDescription

                                    }).ToList()
                                }).ToList();
                Assert.IsNotNull(filterGroups);
            };
        }

        [TestMethod, Ignore]
        public void LoadPriceListLookup()
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {

                var priceLists = db.Select<DB.PriceList>().Select(pl => new PriceList
                {
                    Id = pl.PriceListId,
                    Name = pl.Name,
                    Description = pl.Description,
                    Sort = pl.Sort
                });


                Assert.IsNotNull(priceLists);
            };
        }

        [TestMethod]
        public void LoadFiltersByDAL()
        {
            var filters = LookupRepo.LoadFilters();
            Assert.IsNotNull(filters);
        }

        [TestMethod]
        public void LoadPriceListsByDAL()
        {
            var pls = LookupRepo.LoadPriceLists();
            Assert.IsNotNull(pls);
        }

    }

}
