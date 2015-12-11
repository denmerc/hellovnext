using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using APLPX.Client.Contracts;
using APLPX.Entity;
using Newtonsoft.Json;
using Npgsql;
using ServiceStack.OrmLite;
using DB = APLPX.Client.Postgres.Models;
using ENT = APLPX.Entity;

namespace APLPX.Client.Postgres
{
    public class AnalyticDataService : IAnalyticDataService
    {
        private OrmLiteConnectionFactory DBConnectionFactory { get; set; }

        public AnalyticDataService()
        {
            DBConnectionFactory = new OrmLiteConnectionFactory(
                ConfigurationManager.AppSettings["localConnectionString"],
                    PostgreSqlDialect.Provider
                );
            OrmLiteConfig.DialectProvider.NamingStrategy = new OrmLiteNamingStrategyBase();
            OrmLiteConfig.CommandTimeout = 0;
            //ServiceStack.OrmLite.PostgreSQL.PostgreSQLDialectProvider.Instance.UseReturningForLastInsertId = true;

        }

        public ENT.Analytic Load(int analyticId, bool includeChildren)
        {
            if (analyticId == 0)
            {
                throw new ArgumentOutOfRangeException("analyticId");
            }

            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                var analytic = db.Single<DB.Analytic>(a => a.AnalyticId == analyticId);
                if (analytic == null)
                {
                    throw new KeyNotFoundException("Analytic not found.");
                }
                var user = db.Single<DB.User>(u => u.UserId == analytic.OwnerId);
                if (user == null)
                {
                    throw new ArgumentOutOfRangeException("Owner/User not found.");
                }

                ENT.Analytic result = null;

                if (analytic.ProductFilters == null)
                {
                    analytic.ProductFilters = new DB.ProductFilterGroup[0];
                }
                if (analytic.PriceLists == null)
                {
                    analytic.PriceLists = new int[0];
                }

                var dbRows = db.Select<DB.AnalyticDriverExtended>(db.From<DB.DriverType>()
                                    .LeftJoin<DB.AnalyticDriver>((dt, ad) => ad.DriverId == dt.DriverId && ad.AnalyticId == analyticId));

                var counts = db.Single<DB.AnalyticSummary>(string.Format("SELECT count (*) SkuCount, Sum(\"ActualSalesAmount\") TotalSalesValue FROM \"PX_Main\".\"AnalyticProduct\" WHERE \"AnalyticId\" = {0}", analyticId));

                if (includeChildren)
                {
                    result = new Analytic
                    {
                        Id = analytic.AnalyticId,
                        Name = analytic.Name,
                        Description = analytic.Description,
                        FolderId = analytic.FolderID,
                        Notes = analytic.Notes,
                        OwnerId = user.UserId,
                        OwnerName = GetUserFullName(user),
                        CreatedDate = analytic.CreateTS,
                        ModifiedDate = analytic.ModifyTS,
                        AggregationStartDate = analytic.AggStartDate,
                        AggregationEndDate = analytic.AggEndDate,
                        Filters = (from p in analytic.ProductFilters
                                   group p by p.FilterTypeId into g
                                   select new FilterGroup
                                   {
                                       FilterType = g.Key,

                                       Filters = g.SelectMany(h => h.Values)
                                       .Select(fv => new Filter { Code = fv, FilterType = g.Key }).ToList()
                                   }).ToArray() ?? new FilterGroup[0],

                        PriceLists = analytic.PriceLists,

                        ValueDrivers = dbRows.Select(dbRow => MapToDto(dbRow, analyticId)).ToList(),
                        SkuCount = (int)counts.SkuCount,
                        TotalSales = (decimal)counts.TotalSalesValue

                    };
                }
                else
                {
                    result = new Analytic
                    {
                        Id = analytic.AnalyticId,
                        Name = analytic.Name,
                        Description = analytic.Description,
                        FolderId = analytic.FolderID,
                        Notes = analytic.Notes,
                        //IsActive = analytic.IsActive,
                        //IsShared = analytic.IsShared,
                        OwnerName = GetUserFullName(user),
                        CreatedDate = analytic.CreateTS,
                        ModifiedDate = analytic.ModifyTS,
                        AggregationStartDate = analytic.AggStartDate,
                        AggregationEndDate = analytic.AggEndDate,
                        SkuCount = (int)counts.SkuCount,
                        TotalSales = (decimal)counts.TotalSalesValue
                    };
                }
                return result;
            }
        }

        public long Save(APLPX.Entity.Analytic analytic)
        {

            if (String.IsNullOrWhiteSpace(analytic.OwnerName))
            {
                throw new ArgumentOutOfRangeException("Invalid parameter : analytic.OwnerName");
            }
            try
            {
                using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
                {
                    var aggDays = db.Single<DB.GlobalParameter>(gp => gp.ParamName == "DefaultAggregationDays").ParamValue;
                    if (aggDays == null) { throw new KeyNotFoundException("Analytic - Load : DefaultAggregationDays not found."); }

                    if (analytic.Id == 0) //insert
                    {

                        var id = db.Insert(new DB.Analytic
                        {
                            Name = analytic.Name,
                            Description = analytic.Description,
                            FolderID = analytic.FolderId,
                            Notes = analytic.Notes,
                            ClusterType = "D",
                            AggStartDate = analytic.AggregationStartDate,
                            AggEndDate = analytic.AggregationEndDate,
                            //IsActive = analytic.IsActive,
                            //IsShared = analytic.IsShared,
                            OwnerId = analytic.OwnerId,
                            ModifyTS = DateTime.Now,
                            CreateTS = DateTime.Now


                        }
                            //, selectIdentity: true
                        );

                        var i = db.LastInsertId();

                        SavePriceLists((int)i, analytic.PriceLists);
                        //var id = db.SelectFmt<Analytic>("SELECT curval() from PX_Main.Analytic_AnalyticId_seq");
                        return db.LastInsertId();

                    }
                    else
                    {
                        db.UpdateOnly(new DB.Analytic
                        {
                            Name = analytic.Name,
                            Description = analytic.Description,
                            FolderID = analytic.FolderId,
                            Notes = analytic.Notes,
                            AggStartDate = analytic.AggregationStartDate,
                            AggEndDate = analytic.AggregationEndDate,
                            //IsActive = analytic.IsActive,
                            //IsShared = analytic.IsShared,
                            OwnerId = analytic.OwnerId,
                            ModifyTS = DateTime.Now
                        }
                        , onlyFields: p => new { p.Name, p.Description, p.FolderID, p.Notes, p.AggStartDate, p.AggEndDate, p.OwnerId, p.ModifyTS }
                        , where: p => p.AnalyticId == analytic.Id);
                        return analytic.Id;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        public FilterGroup[] LoadFilters(int analyticId)
        {
            if (analyticId == 0)
            {
                throw new ArgumentOutOfRangeException("Invalid parameter : analyticId");
            }
            try
            {
                using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
                {
                    var productFilters = db.Scalar<DB.ProductFilterGroup[]>(db.From<DB.Analytic>().Where(q => q.AnalyticId == analyticId).Select(x => x.ProductFilters));
                    if (productFilters == null) { return new FilterGroup[0]; }


                    var filtergroups = (from p in productFilters
                                        group p by p.FilterTypeId into g
                                        select new FilterGroup
                                        {
                                            FilterType = g.Key,

                                            Filters = g.SelectMany(h => h.Values)
                                            .Select(fv => new Filter { Code = fv, FilterType = g.Key }).ToList()
                                        }).ToArray();
                    return filtergroups.ToArray() ?? new FilterGroup[] { };
                    //return JsonConvert.DeserializeObject<FilterGroup[]> (analytic.ProductFilters) ?? new FilterGroup[0];
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool SaveFilters(int analyticId, FilterGroup[] filtersToSave)
        {
            if (analyticId == 0)
            {
                throw new ArgumentOutOfRangeException("Invalid parameter : analyticId");
            }

            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                db.UpdateOnly(new DB.Analytic
                {
                    ProductFilters = (from fg in filtersToSave
                                      select new DB.ProductFilterGroup
                                      {
                                          Name = fg.Name,
                                          FilterTypeId = fg.FilterType,
                                          Values = (from f in fg.Filters
                                                    select f.Code).ToArray()
                                      }).ToArray()
                }
                , onlyFields: p => new { p.ProductFilters }
                , where: p => p.AnalyticId == analyticId
                );
            }

            ApplyChangesToPriceRoutines(analyticId);

            return true;
        }


        public int[] LoadPriceLists(int analyticId)
        {
            if (analyticId == 0)
            {
                throw new ArgumentOutOfRangeException("Invalid parameter : analyticId");
            }
            try
            {
                using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
                {
                    var an = db.Single<DB.Analytic>(a => a.AnalyticId == analyticId);
                    return an.PriceLists ?? new int[0];
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool SavePriceLists(int analyticId, int[] priceListsToSave)
        {
            if (analyticId == 0)
            {
                throw new ArgumentOutOfRangeException("Invalid parameter : analyticId");
            }

            if (priceListsToSave == null)
            {
                throw new ArgumentNullException("Invalid parameter : priceListsToSave");
            }
            try
            {
                using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
                {
                    var analytic = db.Single<DB.Analytic>(a => a.AnalyticId == analyticId);

                    if (analytic != null)
                    {
                        analytic.PriceLists = priceListsToSave;
                        db.Save(analytic);
                    }

                    //below doesnt work int[] to jsonb
                    //db.UpdateOnly(new DB.Analytic
                    //{
                    //    AnalyticId = analyticId,
                    //    PriceLists = priceListsToSave
                    //}
                    //, onlyFields: p => new { p.PriceLists }
                    //, where: p => p.AnalyticId == analyticId

                    //);
                    return true;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<ENT.AnalyticValueDriver> LoadDrivers(int analyticId)
        {
            if (analyticId == 0)
            {
                throw new ArgumentOutOfRangeException("Invalid parameter : analyticId");
            }

            var result = new List<ENT.AnalyticValueDriver>();

            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                var dbRows = db.Select<DB.AnalyticDriverExtended>(db.From<DB.DriverType>()
                                .LeftJoin<DB.AnalyticDriver>((dt, ad) => ad.DriverId == dt.DriverId && ad.AnalyticId == analyticId));

                if (dbRows != null)
                {
                    var dtos = dbRows.Select(dbRow => MapToDto(dbRow, analyticId));
                    result = dtos.ToList();
                }
                return result;
            }
        }

        public bool SaveDrivers(int analyticId, List<APLPX.Entity.AnalyticValueDriver> valueDriversToSave)
        {
            if (analyticId == 0)
            {
                throw new ArgumentOutOfRangeException("analyticId");
            }
            if (valueDriversToSave == null)
            {
                throw new ArgumentNullException("valueDriversToSave");
            }

            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                db.Delete<DB.AnalyticDriver>(d => d.AnalyticId == analyticId);

                var vds = valueDriversToSave.Select(vd => new DB.AnalyticDriver
                            {
                                AnalyticId = analyticId,
                                ModifyTS = DateTime.Now,
                                DriverId = (int)vd.DriverType,
                                Encoding = new DB.DriverEncoding
                                {
                                    MinDriverOutlier = vd.MinDriverOutlier,
                                    MaxDriverOutlier = vd.MaxDriverOutlier,
                                    Groups = vd.Groups.Select(g => new DB.DriverGroup
                                    {
                                        GroupNumber = g.GroupNumber,
                                        MinOutlier = g.MinOutlier,
                                        MaxOutlier = g.MaxOutlier,
                                        SkuCount = g.SkuCount,
                                        SalesValue = g.SalesValue

                                    }).ToArray()

                                }

                            });

                db.InsertAll(vds);
            }

            BuildClusters(analyticId);
            ApplyChangesToPriceRoutines(analyticId);

            return true;
        }

        public List<APLPX.Entity.Analytic> LoadList(int ownerId)
        {
            List<APLPX.Entity.Analytic> result = new List<Analytic>();

            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                var analytics = db.Select<DB.Analytic>().Where(a => a.OwnerId == ownerId);
                if (analytics == null)
                {
                    analytics = new List<DB.Analytic>();
                }
                foreach (DB.Analytic dbEntity in analytics)
                {
                    //Get the user so we can construct the user name.
                    var user = db.Single<DB.User>(u => u.UserId == dbEntity.OwnerId);

                    Entity.Analytic dto = new Analytic
                    {
                        Id = dbEntity.AnalyticId,
                        FolderId = dbEntity.FolderID,
                        Name = dbEntity.Name,
                        Description = dbEntity.Description,
                        Notes = dbEntity.Notes,
                        OwnerId = dbEntity.OwnerId,
                        OwnerName = GetUserFullName(user),
                        CreatedDate = dbEntity.CreateTS,
                        ModifiedDate = dbEntity.ModifyTS,
                        AggregationStartDate = dbEntity.AggStartDate,
                        AggregationEndDate = dbEntity.AggEndDate,
                        SkuCount = (int)GetSummary(dbEntity.AnalyticId).SkuCount,
                        DriverCount = (int)GetDriverCount(dbEntity.AnalyticId)
                    };
                    result.Add(dto);

                }
            }
            return result;

        }

        public APLPX.Entity.Analytic Load(int analyticId) //TODO: dm - not using strict rest resource spec
        {
            if (analyticId == 0)
            {
                throw new ArgumentOutOfRangeException("AnalyticId cannot be zero");
            }
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                var a = db.SingleById<DB.Analytic>(analyticId);
                if (a == null)
                {
                    throw new KeyNotFoundException("Analytic not found.");
                }

                var userDbEntity = db.Single<DB.User>(u => u.UserId == a.OwnerId);
                if (userDbEntity == null)
                {
                    throw new KeyNotFoundException("Owner/User not found.");
                }

                return new ENT.Analytic
                {
                    Id = a.AnalyticId,
                    Name = a.Name,
                    Description = a.Description,
                    Notes = a.Notes,
                    FolderId = a.FolderID,
                    //IsActive = a.IsActive,
                    OwnerId = a.OwnerId,
                    OwnerName = GetUserFullName(userDbEntity),
                    //IsShared = a.IsShared,
                    CreatedDate = a.CreateTS,
                    Filters = (from fg in a.ProductFilters
                               select new FilterGroup
                               {
                                   FilterType = fg.FilterTypeId,
                                   Name = fg.Name,
                                   Filters = (from f in fg.Values
                                              select new Filter
                                              {
                                                  FilterType = fg.FilterTypeId,
                                                  Code = f,
                                              }).ToList()
                               }).ToArray(),
                    PriceLists = a.PriceLists.ToArray()
                };
            }
        }



        //base rest contract
        public APLPX.Entity.Analytic Get(int id)
        {

            try
            {
                using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
                {
                    var a = db.SingleById<DB.Analytic>(id);
                    if (a == null) { return null; };

                    return new ENT.Analytic
                    {
                        Id = a.AnalyticId,
                        Name = a.Name,
                        Description = a.Description,
                        Notes = a.Notes,
                        FolderId = a.FolderID,
                        //IsActive = a.IsActive,
                        OwnerName = db.Single<DB.User>(u => u.UserId == a.OwnerId).Login,
                        //IsShared = a.IsShared,
                        CreatedDate = a.CreateTS,
                        Filters = (from fg in a.ProductFilters
                                   select new FilterGroup
                                   {
                                       Name = fg.Name,
                                       Filters = (from f in fg.Values
                                                  select new Filter
                                                  {
                                                      Code = f,
                                                      FilterType = fg.FilterTypeId
                                                  }).ToList()
                                   }).ToArray(),
                        PriceLists = a.PriceLists.ToArray()
                    };
                }
            }
            catch (Exception)
            {
                return null;
            }

        }

        public bool Insert(APLPX.Entity.Analytic entity)
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                try
                {
                    db.Insert(new DB.Analytic
                    {
                        Description = entity.Description,
                        Name = entity.Name,
                        Notes = entity.Notes,
                        FolderID = entity.FolderId,
                    });
                    return true;

                }
                catch (Exception)
                {

                    return false;
                }
            }
        }

        public bool Update(APLPX.Entity.Analytic entity)
        {
            if (entity == null)
            {
                throw new ArgumentException("entity");
            }
            //if entity exists
            if (!Exists()) //placeholder
            {
                return false; //return resource NotFound at controller level
            }
            //Delete(entity.Id);
            //Insert(entity);

            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {

                db.Update<Analytic>(
                    new DB.Analytic
                    {
                        Name = entity.Name,
                        Description = entity.Description,
                        //SearchGroupId = entity.SearchGroupId,
                        FolderID = entity.FolderId,
                        Notes = entity.Notes

                    }
                );
            }
            return true;

        }

        public bool Delete(int id)
        {
            throw new NotImplementedException();
        }

        public bool Exists()
        {
            return true;
        }


        public Analytic Create(int folderId, int userId)
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                var aggDays = db.Single<DB.GlobalParameter>(gp => gp.ParamName == "DefaultAggregationDays").ParamValue;
                if (aggDays == null)
                {
                    throw new KeyNotFoundException("Analytic - Load : DefaultAggregationDays not found.");
                }

                var user = db.Single<DB.User>(u => u.UserId == userId);
                if (user == null)
                {
                    throw new KeyNotFoundException("Owner/User not found.");
                }

                var drivers = db.Select<DB.DriverType>();
                if (drivers == null)
                {
                    throw new KeyNotFoundException("Driver Types not found.");
                }

                var priceLists = db.Select<DB.PriceList>()
                                   .Where(t => t.PriceListType == "N")
                                   .Select(pl => pl.PriceListId).ToArray();

                var dto = new ENT.Analytic
                {
                    Name = string.Empty,
                    Description = string.Empty,
                    Notes = string.Empty,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    FolderId = folderId,
                    OwnerId = user.UserId,
                    OwnerName = user.Login,
                    Filters = new FilterGroup[0],
                    PriceLists = priceLists ?? new int[0],
                    AggregationStartDate = DateTime.Now.Subtract(TimeSpan.FromDays(Int32.Parse(aggDays))).AddDays(-1),
                    AggregationEndDate = DateTime.Now.AddDays(-1),
                    ValueDrivers = (from d in drivers
                                    select new AnalyticValueDriver
                                    {
                                        Id = d.DriverId,
                                        DriverType = (DriverType)d.DriverId,
                                        Name = d.Name,
                                        Description = d.Description,
                                        IsInverted = d.IsInverted,
                                        UnitOfMeasure = d.UnitOfMeasure,
                                        Groups = new List<ValueDriverGroup>()
                                    }).ToList()
                };

                return dto;
            }
        }

        public Analytic Copy(int analyticId)
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                var source = db.SingleById<DB.Analytic>(analyticId);
                if (source == null)
                {
                    throw new KeyNotFoundException("Source Analytic not found.");
                }
                var user = db.Single<DB.User>(u => u.UserId == source.OwnerId);
                if (user == null)
                {
                    throw new KeyNotFoundException("Owner/User not found.");
                }

                if (source.ProductFilters == null)
                {
                    source.ProductFilters = new DB.ProductFilterGroup[0];
                }
                if (source.PriceLists == null)
                {
                    source.PriceLists = new int[0];
                }
                var drivers = db.Select<DB.AnalyticDriverExtended>(db.From<DB.DriverType>()
                            .LeftJoin<DB.AnalyticDriver>((dt, ad) => ad.DriverId == dt.DriverId && ad.AnalyticId == analyticId));

                var newAnalytic = new Analytic
                    {
                        Name = string.Format("{0}-Copy", source.Name),
                        Description = source.Description,
                        Notes = string.Empty,
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now,
                        FolderId = source.FolderID,
                        OwnerId = user.UserId,
                        OwnerName = user.Login,
                        AggregationStartDate = source.AggStartDate,
                        AggregationEndDate = source.AggEndDate,
                        Filters = (from p in source.ProductFilters
                                   group p by p.FilterTypeId into g
                                   select new FilterGroup
                                   {
                                       //FilterType = (FilterType)g.Key,
                                       FilterType = g.Key,
                                       Filters = g.SelectMany(h => h.Values)
                                       .Select(fv => new Filter { Code = fv, FilterType = g.Key }).ToList()
                                   }).ToArray() ?? new FilterGroup[0],
                        PriceLists = source.PriceLists,
                        ValueDrivers = drivers.Select(dbRow => MapToDto(dbRow, analyticId)).ToList(),
                    };

                return newAnalytic;
            }
        }


        public int CacheAnalyticProductSet(int analyticId, IProgress<String> progress)
        {
            if (analyticId == 0)
            {
                throw new ArgumentOutOfRangeException("analyticId");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }

            using (var db = DBConnectionFactory.OpenDbConnection())
            {
                NpgsqlConnection conn = (NpgsqlConnection)db.ToDbConnection();

                //Set up the progress reporting mechanism.                
                conn.Notice += (sender, args) =>
                {
                    if (progress != null)
                    {
                        progress.Report(args.Notice.Message);
                    }
                };

                progress.Report("Updating product-level data...");
                int result = db.SqlScalar<int>(string.Format("select \"PX_Main\".\"SP_AnalyticCacheProductSet\"({0})", analyticId));

                //The previous call deletes any existing driver clusters. 
                // Rebuild them for the current driver configuration.
                progress.Report("Rebuilding Value Drivers...");
                db.SqlScalar<bool>(string.Format("SELECT \"PX_Main\".\"SP_AnalyticBuildClusters\"({0})", analyticId));

                return result;
            }
        }

        public ValueDriverGroup[] RecalculateAggregates(int analyticId, int driverId, ValueDriverGroup[] driverGroups)
        {
            if (analyticId == 0) throw new ArgumentOutOfRangeException("Invalid parameter : analyticId.");
            if (driverGroups == null) throw new ArgumentOutOfRangeException("Invalid parameter : driverGroups.");

            var jsonGroups = JsonConvert.SerializeObject(driverGroups);
            using (var db = DBConnectionFactory.OpenDbConnection())
            {
                var json = db.SqlScalar<string>(string.Format("select \"PX_Main\".\"SP_AnalyticRecalculateAggregates\"({0}, {1}, '{2}')", analyticId, driverId, jsonGroups));
                // return DriverCombinationAggregates[]
                var aggregates = JsonConvert.DeserializeObject<DB.DriverGroup[]>(json);
                return aggregates.Select(agg => new ValueDriverGroup
                    {
                        GroupNumber = agg.GroupNumber,
                        MinOutlier = agg.MinOutlier,
                        MaxOutlier = agg.MaxOutlier,
                        SkuCount = agg.SkuCount,
                        SalesValue = agg.SalesValue
                    }).ToArray();
            }
        }

        public ValueDriverGroup[] AutoCalculate(int analyticId, int driverId, int groupCount, decimal minOutlier, decimal maxOutlier)
        {
            if (analyticId == 0)
            {
                throw new ArgumentOutOfRangeException("analyticId.");
            }
            if (driverId == 0)
            {
                throw new ArgumentOutOfRangeException("driverId.");
            }
            if (groupCount == 0)
            {
                throw new ArgumentOutOfRangeException("groupCount");
            }

            string sql = String.Format("SELECT \"PX_Main\".\"SP_AnalyticAutoCalculate\"({0},{1}, {2}, {3}, {4})", analyticId, driverId, groupCount, minOutlier, maxOutlier);
            ValueDriverGroup[] result = AutoCalculateDriverGroups(sql);

            return result;
        }

        public ValueDriverGroup[] AutoCalculate(int analyticId, int driverId, int groupCount)
        {
            if (analyticId == 0)
            {
                throw new ArgumentOutOfRangeException("analyticId.");
            }
            if (driverId == 0)
            {
                throw new ArgumentOutOfRangeException("driverId.");
            }
            if (groupCount == 0)
            {
                throw new ArgumentOutOfRangeException("groupCount");
            }

            string sql = String.Format("select \"PX_Main\".\"SP_AnalyticAutoCalculate\"({0},{1}, {2})", analyticId, driverId, groupCount);
            ValueDriverGroup[] result = AutoCalculateDriverGroups(sql);

            return result;
        }

        private ValueDriverGroup[] AutoCalculateDriverGroups(string sql)
        {
            using (var db = DBConnectionFactory.OpenDbConnection())
            {
                string json = db.SqlScalar<string>(sql);
                if (String.IsNullOrEmpty(json))
                {
                    json = "[]";
                }

                DB.DriverGroup[] dbGroups = JsonConvert.DeserializeObject<DB.DriverGroup[]>(json);

                var dtoList = dbGroups.Select(dbEntity => MapToDto(dbEntity));

                return dtoList.ToArray();
            }
        }

        public AnalyticAggregates GetSummary(int analyticId)
        {
            if (analyticId == 0) throw new ArgumentOutOfRangeException("Invalid parameter : analyticId.");

            using (var db = DBConnectionFactory.OpenDbConnection())
            {
                var summary = db.Single<DB.AnalyticSummary>(string.Format("SELECT \"SkuCount\", \"TotalSalesValue\" FROM \"PX_Main\".\"SP_AnalyticGetSummary\"({0})", analyticId));
                return new AnalyticAggregates { SkuCount = summary.SkuCount, TotalSalesValue = summary.TotalSalesValue };
            }
        }

        public List<DriverCombinationAggregate> GetDriverCombinationAggregates(int analyticId)
        {
            using (var db = DBConnectionFactory.OpenDbConnection())
            {
                var combos = db.SelectFmt<DB.DriverCombinationAggregate>(string.Format("SELECT \"ClusterId\", \"Cluster\", \"SkuCount\", \"SalesValue\" FROM \"PX_Main\".\"SP_AnalyticGetDriverCombinationAggregates\"({0})", analyticId));
                return combos.Select(c => new DriverCombinationAggregate
                    {
                        SkuCount = c.SkuCount,
                        TotalSalesValue = c.SalesValue,
                        ClusterId = c.ClusterId,
                        Cluster = c.Cluster.Select(gd => new DriverGroupCombination
                                {
                                    GroupNum = (int)gd.DriverValue,
                                    DriverId = gd.DriverId
                                }).ToArray()
                    }).ToList();
            }

        }

        /// <summary>
        /// Gets the number of SKUs included for the specified collection of filters.
        /// </summary>
        /// <param name="filters">The included collection of filter groups</param>
        /// <returns>The result, as an int.</returns>
        public int GetFilterSkuCount(IEnumerable<FilterGroup> filters)
        {
            int result = 0;

            var dbf = (from fg in filters
                       select new DB.ProductFilterGroup
                       {
                           Name = fg.Name,
                           FilterTypeId = fg.FilterType,
                           Values = (from f in fg.Filters
                                     select f.Code).ToArray()
                       }).ToArray();

            using (var db = DBConnectionFactory.OpenDbConnection())
            {
                string json = JsonConvert.SerializeObject(dbf);
                result = db.SqlScalar<int>(string.Format("select \"PX_Main\".\"SP_ProductFilterGetCount\"('{0}')", json));
            }

            return result;
        }

        /// <summary>
        /// Gets the number of SKUs included for the specified analytic and collection of price lists.
        /// </summary>
        /// <param name="analyticId">The ID of the analytic.</param>
        /// <param name="priceLists">The IDs of the included price lists.</param>
        /// <returns>The result, as an int.</returns>
        public int GetPriceListSkuCount(int analyticId, IEnumerable<int> priceLists)
        {
            int result = 0;
            var priceListKeys = priceLists != null ? JsonConvert.SerializeObject(priceLists) : "[]";

            using (var db = DBConnectionFactory.OpenDbConnection())
            {
                //"PX_Main"."SP_PriceListFilterGetCount"("pAnalyticId" integer,"pPriceLists" jsonb)
                result = db.SqlScalar<int>(string.Format("select \"PX_Main\".\"SP_PriceListFilterGetCount\"({0},'{1}')", analyticId, priceListKeys));
            }

            return result;
        }

        public bool BuildClusters(int analyticId)
        {
            if (analyticId == 0)
            {
                throw new ArgumentOutOfRangeException("analyticId");
            }

            using (var db = DBConnectionFactory.OpenDbConnection())
            {
                return db.SqlScalar<bool>(string.Format("SELECT \"PX_Main\".\"SP_AnalyticBuildClusters\"({0})", analyticId));
            }
        }

        public long GetDriverCount(int analyticId)
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                return db.Count<DB.AnalyticDriver>((dt => dt.AnalyticId == analyticId));
            }
        }
        public MinMax GetDriverSummary(int analyticId, int driverId)
        {
            if (analyticId == 0)
            {
                throw new ArgumentOutOfRangeException("analyticId");
            }
            if (driverId == 0)
            {
                throw new ArgumentOutOfRangeException("driverId");
            }

            using (var db = DBConnectionFactory.OpenDbConnection())
            {
                string sql = String.Format("SELECT \"MinValue\", \"MaxValue\" FROM \"PX_Main\".\"SP_AnalyticGetDriverSummary\"({0}, {1})", analyticId, driverId);
                DB.MinMax dbEntity = db.Single<DB.MinMax>(sql);

                ENT.MinMax result = new MinMax
                {
                    Min = dbEntity.MinValue,
                    Max = dbEntity.MaxValue
                };
                return result;
            }
        }


        public bool SaveFolderAssignment(int analyticId, int folderId)
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                db.UpdateOnly(new DB.Analytic
                {
                    FolderID = folderId
                }

                            , onlyFields: p => new { p.FolderID }
                            , where: p => p.AnalyticId == analyticId);
                return true;
            }
        }

        private bool ApplyChangesToPriceRoutines(int analyticId)
        {
            if (analyticId == 0)
            {
                throw new ArgumentOutOfRangeException("analyticId");
            }

            bool result = true;

            string sql = String.Format("select \"PX_Main\".\"SP_AnalyticApplyToPricings\"({0})", analyticId);
            using (var db = DBConnectionFactory.OpenDbConnection())
            {
                result = db.SqlScalar<bool>(sql);
            }

            return result;
        }

        private string GetUserFullName(DB.User userEntity)
        {
            string result = String.Format("{0} {1}", userEntity.FirstName, userEntity.LastName);
            return result;
        }

        #region Entity-DTO Mapping helpers

        // <summary>
        /// Maps an <see cref="AnalyticDriverExtended"/> database entity to an <see cref="AnalyticValueDriver"/> DTO.
        /// </summary>
        private ENT.AnalyticValueDriver MapToDto(DB.AnalyticDriverExtended dbEntity, int analyticId)
        {
            if (dbEntity.Encoding == null || dbEntity.Encoding.Groups.FirstOrDefault() == null)
            {
                dbEntity.Encoding = new DB.DriverEncoding { Groups = new DB.DriverGroup[0] };
            }

            MinMax driverLimits = GetDriverSummary(analyticId, dbEntity.DriverId);

            var dto = new ENT.AnalyticValueDriver
            {
                Id = dbEntity.DriverId,
                IsInverted = dbEntity.IsInverted,
                UnitOfMeasure = dbEntity.UnitOfMeasure,
                DriverType = (DriverType)dbEntity.DriverId,
                Name = dbEntity.Name,
                Description = dbEntity.Description,
                IsSelected = (dbEntity.Encoding.Groups.FirstOrDefault() != null),
                DriverMinimum = driverLimits.Min,
                DriverMaximum = driverLimits.Max,
                Groups = dbEntity.Encoding.Groups.Select(dbGroup => MapToDto(dbGroup)).ToList(),
                MinDriverOutlier = dbEntity.Encoding.MinDriverOutlier,
                MaxDriverOutlier = dbEntity.Encoding.MaxDriverOutlier
            };

            return dto;
        }

        private ENT.ValueDriverGroup MapToDto(DB.DriverGroup dbEntity)
        {
            ENT.ValueDriverGroup dto = new ENT.ValueDriverGroup
            {
                GroupNumber = dbEntity.GroupNumber,
                SkuCount = dbEntity.SkuCount,
                SalesValue = dbEntity.SalesValue,
                MinOutlier = dbEntity.MinOutlier,
                MaxOutlier = dbEntity.MaxOutlier
            };

            return dto;
        }

        #endregion

    }
}
