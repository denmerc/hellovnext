using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using APLPX.Client.Contracts;
using APLPX.Entity;
using Newtonsoft.Json;
using Npgsql;
using ServiceStack;
using ServiceStack.OrmLite;
using DB = APLPX.Client.Postgres.Models;
using ENT = APLPX.Entity;


namespace APLPX.Client.Postgres
{
    public class PricingDataService : IPricingDataService
    {
        private OrmLiteConnectionFactory DBConnectionFactory { get; set; }

        public PricingDataService()
        {
            DBConnectionFactory = new OrmLiteConnectionFactory(
                ConfigurationManager.AppSettings["localConnectionString"],
                    PostgreSqlDialect.Provider
                );
            OrmLiteConfig.DialectProvider.NamingStrategy = new OrmLiteNamingStrategyBase();
            OrmLiteConfig.CommandTimeout = 0;

        }

        public List<PricingEveryday> LoadList(int ownerId, ENT.PriceRoutineType priceRoutineType)
        {
            if (priceRoutineType != PriceRoutineType.Everyday && priceRoutineType != PriceRoutineType.Promotion)
            {
                throw new ArgumentException("Price Routine type must be either Everyday or Promotion", "priceRoutineType");
            }

            var result = new List<ENT.PricingEveryday>();

            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                //Filter by type.
                IEnumerable<DB.Pricing> priceRoutines = Enumerable.Empty<DB.Pricing>();
                if (priceRoutineType == PriceRoutineType.Promotion)
                {
                    priceRoutines = db.Select<DB.Pricing>().Where(p => p.OwnerId == ownerId &&
                                                                  p.PricingModeId == (int)PricingMode.Promotion);
                }
                else
                {
                    priceRoutines = db.Select<DB.Pricing>().Where(p => p.OwnerId == ownerId &&
                                                                  p.PricingModeId != (int)PricingMode.Promotion);
                }

                foreach (DB.Pricing pricingDbEntity in priceRoutines)
                {
                    DB.User userDbEntity = db.Single<DB.User>(u => u.UserId == pricingDbEntity.OwnerId);
                    pricingDbEntity.AnalyticName = db.SingleById<DB.Analytic>(pricingDbEntity.AnalyticId).Name;

                    ENT.PricingEveryday dto = ToPricingEverydayDto(pricingDbEntity, userDbEntity);

                    result.Add(dto);
                }
            }
            return result;
        }

        public PricingEveryday Load(int pricingId, bool includeChildren = false)
        {
            if (pricingId == 0)
            {
                throw new ArgumentOutOfRangeException("pricingId");
            }

            PricingEveryday result = null;

            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                var pricingDbEntity = db.LoadSingleById<DB.Pricing>(pricingId);
                if (pricingDbEntity == null)
                {
                    throw new KeyNotFoundException("Price Routine not found.");
                }
                var userDbEntity = db.Single<DB.User>(u => u.UserId == pricingDbEntity.OwnerId);
                if (userDbEntity == null)
                {
                    throw new KeyNotFoundException("Owner/User not found.");
                }
                pricingDbEntity.AnalyticName = db.SingleById<DB.Analytic>(pricingDbEntity.AnalyticId).Name;

                result = ToPricingEverydayDto(pricingDbEntity, userDbEntity);

                var counts = db.Single<ENT.PricingSummary>(string.Format("SELECT count(distinct \"ProductId\") \"SkuCount\", count(*) \"PriceCount\" FROM \"PX_Main\".\"PricingRecommendation\" WHERE \"PricingId\"={0}", pricingId));

                result.SkuCount = (int)counts.SkuCount;
                result.PriceCount = (int)counts.PriceCount;

                if (includeChildren)
                {
                    result.Filters = LoadFilters(pricingId).ToList();
                    result.PriceLists = LoadPriceLists(pricingId);
                    result.ValueDrivers = LoadDrivers(pricingId);
                }

                return result;
            }
        }


        public long Save(PricingEveryday priceRoutine)
        {
            if (priceRoutine.OwnerName == null)
            {
                throw new ArgumentOutOfRangeException("Invalid parameter : Pricing.OwnerName");
            }

            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                if (priceRoutine.Id == 0) //insert
                {
                    //Set the default pricing mode unless it is already set on the price routine, e.g., for a Promotion.
                    int pricingModeId = (int)priceRoutine.PricingMode;
                    if (priceRoutine.PricingMode == PricingMode.NotSet)
                    {
                        pricingModeId = (int)PricingMode.Single;
                    }

                    db.Insert(new DB.Pricing
                    {
                        Name = priceRoutine.Name,
                        Description = priceRoutine.Description,
                        FolderId = priceRoutine.FolderId,
                        Notes = priceRoutine.Notes,
                        OwnerId = priceRoutine.OwnerId,
                        AnalyticId = priceRoutine.AnalyticId,
                        PricingModeId = pricingModeId,
                        PricingStateId = (int)PricingState.InProgress,
                        ModifyTS = DateTime.Now,
                        CreateTS = DateTime.Now,
                    });

                    long id = db.LastInsertId();
                    bool response = db.SqlScalar<bool>(string.Format("select \"PX_Main\".\"SP_PricingInitialize\"({0})", id));

                    return id;
                }
                else
                {

                    db.UpdateOnly(new DB.Pricing
                    {
                        Name = priceRoutine.Name,
                        Description = priceRoutine.Description,
                        FolderId = priceRoutine.FolderId,
                        Notes = priceRoutine.Notes,
                        //IsActive = analytic.IsActive,
                        //IsShared = analytic.IsShared,
                        OwnerId = priceRoutine.OwnerId,
                        ModifyTS = DateTime.Now,
                        AnalyticId = priceRoutine.AnalyticId

                    }
                    , onlyFields: p => new { p.Name, p.Description, p.FolderId, p.Notes, p.OwnerId, p.ModifyTS, p.AnalyticId }
                    , where: p => p.PricingId == priceRoutine.Id);

                    bool response = db.SqlScalar<bool>(string.Format("select \"PX_Main\".\"SP_PricingApplyAnalytic\"({0}, {1})", priceRoutine.Id, priceRoutine.AnalyticId));

                    return priceRoutine.Id;
                }
            }
        }

        public List<FilterGroup> LoadFilters(int pricingId)
        {
            List<FilterGroup> result = new List<FilterGroup>();

            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                var filterGroups = db.SqlScalar<DB.PricingProductFilterGroup[]>(string.Format("select \"PX_Main\".\"FN_PricingLoadFilters\"({0})", pricingId));
                if (filterGroups != null)
                {
                    for (int i = 0; i < filterGroups.Length; i++)
                    {
                        if (filterGroups[i].Values == null) { filterGroups[i].Values = new DB.PricingFilterValue[0]; }
                    }
                    var filterGroupDtos = (from pf in filterGroups
                                           group pf by pf.FilterTypeId into g
                                           select new FilterGroup
                                           {
                                               FilterType = g.Key,
                                               Filters = g.SelectMany(h => h.Values)
                                               .Select(fv => new Filter
                                               {
                                                   Code = fv.Value,
                                                   FilterType = g.Key,
                                                   IsSelectedInAnalytic = fv.AnalyticSelected,
                                                   IsSelectedInPricing = fv.PricingSelected
                                               }).ToList()
                                           });
                    result = filterGroupDtos.ToList();
                }
            }

            return result;
        }


        public bool SaveFilters(int pricingId, FilterGroup[] filtersToSave)
        {
            if (pricingId == 0)
            {
                throw new ArgumentOutOfRangeException("Invalid parameter : pricingId");
            }

            try
            {
                using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
                {
                    var routine = new DB.Pricing
                    {
                        ProductFilters = (from fg in filtersToSave
                                          select new DB.ProductFilterGroup
                                          {
                                              Name = fg.Name,
                                              FilterTypeId = fg.FilterType,
                                              Values = (from f in fg.Filters
                                                        select f.Code).ToArray()
                                          }).ToArray(),
                        ChangeProductFiltersFlag = false
                    };
                    db.UpdateOnly(
                        routine
                    , onlyFields: p => new { p.ProductFilters, p.ChangeProductFiltersFlag }
                    , where: p => p.PricingId == pricingId
                    );

                    SetRecalculateFlag(pricingId);
                    return true;

                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<PricingPriceList> LoadPriceLists(int pricingId)
        {
            var priceLists = new List<PricingPriceList>();

            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                string sql = String.Format("SELECT * FROM \"PX_Main\".\"V_PricingPriceListExtended\" WHERE \"PricingId\"= {0}", pricingId);
                var priceListDbEntities = db.SqlList<DB.PricingPriceListExtended>(sql);
                if (priceListDbEntities != null)
                {
                    priceLists = ToPriceListDtos(priceListDbEntities);
                }
            }

            return priceLists;
        }

        public bool SavePriceLists(int pricingId, PricingMode mode, List<PricingPriceList> priceListsToSave)
        {
            if (pricingId == 0)
            {
                throw new ArgumentOutOfRangeException("Invalid parameter : pricingId");
            }

            if (priceListsToSave == null)
            {
                throw new ArgumentNullException("Invalid parameter : priceListsToSave");
            }
            try
            {
                using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
                {
                    db.Delete<DB.PricingPriceList>(pp => pp.PricingId == pricingId);
                    var plists = priceListsToSave.Select(pl => new DB.PricingPriceList
                                {
                                    PricingId = pricingId,
                                    PriceListId = pl.PriceListId,
                                    IsKey = pl.IsKey,
                                    MinValue = pl.MinValue,
                                    MaxValue = pl.MaxValue,
                                    PercentChange = pl.PercentChange,
                                    RoundingRules = pl.RoundingRules == null ? new List<DB.PriceRoundingRule>() :
                                                            pl.RoundingRules.Select(rr => new DB.PriceRoundingRule
                                                            {
                                                                MinValue = rr.DollarRangeLower,
                                                                MaxValue = rr.DollarRangeUpper,
                                                                RoundingType = (int)rr.RoundingType,
                                                                ValueChange = rr.ValueChange
                                                            }).ToList(),
                                    MarkupRules = pl.MarkupRules == null ? new List<DB.PriceMarkupRule>() :
                                                            pl.MarkupRules.Select(mr => new DB.PriceMarkupRule
                                                            {
                                                                DollarRangeLower = mr.DollarRangeLower,
                                                                DollarRangeUpper = mr.DollarRangeUpper,
                                                                PercentLimitLower = mr.PercentLimitLower,
                                                                PercentLimitUpper = mr.PercentLimitUpper
                                                            }).ToList(),
                                    ApplyRounding = pl.ApplyRounding,
                                    ApplyMarkup = pl.ApplyMarkup

                                });
                    db.InsertAll<DB.PricingPriceList>(plists);

                    //update related pricing to mode
                    db.UpdateOnly(new DB.Pricing
                    {
                        PricingModeId = (int)mode
                    }
                    , onlyFields: p => new { p.PricingModeId }
                    , where: p => p.PricingId == pricingId
                    );
                    SetRecalculateFlag(pricingId);
                    return true;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<PricingValueDriver> LoadDrivers(int pricingId)
        {
            var result = new List<PricingValueDriver>();

            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                //TEMP load the analytic so we can get at the configured drivers. 
                //TODO: Add DriverMinimum and DriverMaximum retrieval to FN_PricingLoadDrivers so this is not necessary.
                var pricing = db.Single<DB.Pricing>(p => p.PricingId == pricingId);
                var analytic = db.Single<DB.Analytic>(a => a.AnalyticId == pricing.AnalyticId);

                string sql = String.Format("SELECT * FROM \"PX_Main\".\"FN_PricingLoadDrivers\"({0})", pricingId);
                var driverDbEntities = db.Select<DB.PricingDriverExtended>(sql);
                if (driverDbEntities != null)
                {
                    foreach (DB.PricingDriverExtended dbEntity in driverDbEntities)
                    {
                        ENT.PricingValueDriver driver = ToValueDriverDto(dbEntity);

                        //Populate the global limits for each driver.
                        //TODO: Add DriverMinimum and DriverMaximum retrieval to FN_PricingLoadDrivers.
                        sql = String.Format("SELECT \"MinValue\", \"MaxValue\" FROM \"PX_Main\".\"SP_AnalyticGetDriverSummary\"({0}, {1})", analytic.AnalyticId, driver.Id);
                        DB.MinMax aggregates = db.Single<DB.MinMax>(sql);
                        driver.DriverMinimum = aggregates.MinValue;
                        driver.DriverMaximum = aggregates.MaxValue;

                        result.Add(driver);
                    }
                }
            }
            return result;
        }


        public bool SaveDrivers(int pricingId, List<PricingValueDriver> valueDriversToSave)
        {
            if (pricingId == 0) { throw new ArgumentOutOfRangeException("Invalid parameter: pricingId"); }
            if (valueDriversToSave == null) { throw new ArgumentOutOfRangeException("Invalid parameter : valueDriversToSave"); }

            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                db.Delete<DB.PricingDriver>(d => d.PricingId == pricingId);
                var driverDbos = valueDriversToSave.Where(i => i.IsSelected || i.IsKey).Select(dts => new DB.PricingDriver
                        {
                            PricingId = pricingId,
                            DriverId = (int)dts.DriverType,
                            IsKey = dts.IsKey,
                            ModifyTS = DateTime.Now,
                            CreateTS = DateTime.Now,
                            ChangeDriverFlag = dts.ChangeDriverFlag,
                            OptimizationRules = dts.Groups != null ? dts.Groups.Select(g => new DB.PriceOptimizationRule
                            {
                                GroupNum = g.GroupNumber,
                                InfluencerPercentChange = g.InfluencerPercentChange,
                                ChangeGroupFlag = g.ChangeGroupFlag,
                                PriceRanges = g.OptimizationRules != null ? g.OptimizationRules.Select(r => new DB.PriceOptimizationRange
                                {
                                    DollarRangeLower = r.DollarRangeLower,
                                    DollarRangeUpper = r.DollarRangeUpper,
                                    PercentChange = r.PercentChange
                                }).ToArray() : new DB.PriceOptimizationRange[0]
                            }).ToArray() : new DB.PriceOptimizationRule[0]

                        }
                    );
                db.InsertAll(driverDbos);
                SetRecalculateFlag(pricingId);
            }
            return true;
        }

        public PricingEveryday Create(int folderId, int userId, PriceRoutineType priceRoutineType)
        {
            if (priceRoutineType != PriceRoutineType.Everyday && priceRoutineType != PriceRoutineType.Promotion)
            {
                throw new ArgumentException("Price Routine type must be either Everyday or Promotion", "priceRoutineType");
            }

            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                var user = db.Single<DB.User>(u => u.UserId == userId);

                if (user == null)
                {
                    throw new KeyNotFoundException("Owner Name not found.");
                }

                var priceLists = db.Column<int>(db.From<DB.PriceList>().Select(q => q.PriceListId).Where(pl => pl.PriceListType == "N"));
                if (priceLists == null)
                {
                    throw new KeyNotFoundException("Pricelists not found.");
                }

                var pricingPriceLists = priceLists.ToList().Select(pl => new PricingPriceList
                                {
                                    PriceListId = pl,
                                    MarkupRules = new List<PriceMarkupRule>(),
                                    RoundingRules = new List<PriceRoundingRule>(),
                                    CreatedDate = DateTime.Now,
                                    ModifiedDate = DateTime.Now
                                }).ToList();

                PricingMode mode = priceRoutineType == PriceRoutineType.Promotion ? PricingMode.Promotion : PricingMode.NotSet;

                var p = new PricingEveryday
                {
                    Name = string.Empty,
                    Description = string.Empty,
                    Notes = string.Empty,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    FolderId = folderId,
                    OwnerId = user.UserId,
                    OwnerName = user.Login,
                    PricingMode = mode,
                    Filters = new List<FilterGroup>(),
                    PriceLists = pricingPriceLists ?? new List<PricingPriceList>(),
                    ValueDrivers = new List<PricingValueDriver>()
                };

                return p;
            }
        }

        public PricingEveryday Copy(int pricingId)
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                var copy = db.LoadSingleById<DB.Pricing>(pricingId);
                if (copy == null) { throw new KeyNotFoundException("Pricing being copied cannot be found."); }
                if (copy.ProductFilters == null) { copy.ProductFilters = new DB.ProductFilterGroup[0]; }
                if (copy.PriceLists == null) { copy.PriceLists = new List<DB.PricingPriceList>(); }
                if (copy.Drivers == null) { copy.Drivers = new List<DB.PricingDriver>(); }
                var pricingPriceLists = copy.PriceLists.ToList().Select(pl => new PricingPriceList
                {
                    PriceListId = pl.PriceListId,
                    MarkupRules = new List<PriceMarkupRule>(),
                    RoundingRules = new List<PriceRoundingRule>(),
                    ApplyMarkup = pl.ApplyMarkup,
                    ApplyRounding = pl.ApplyRounding,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                }).ToList();



                var user = db.Single<DB.User>(u => u.UserId == copy.OwnerId);
                if (user == null) { throw new KeyNotFoundException("Owner/User cannot be found."); }

                var newPricing = new PricingEveryday
                {
                    Name = string.Format("{0}-Copy", copy.Name),
                    Description = copy.Description,
                    AnalyticId = copy.AnalyticId,
                    Notes = string.Empty,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    FolderId = copy.FolderId,
                    OwnerId = copy.OwnerId,
                    OwnerName = user.Login,
                    PricingMode = (ENT.PricingMode)copy.PricingModeId,

                    PriceLists = copy.PriceLists.Select(pl => new PricingPriceList
                    {
                        PricingId = pl.PricingId,
                        PriceListId = pl.PriceListId,
                        IsKey = pl.IsKey,
                        MinValue = pl.MinValue,
                        MaxValue = pl.MaxValue,
                        PercentChange = pl.PercentChange,
                        ApplyMarkup = pl.ApplyMarkup,
                        ApplyRounding = pl.ApplyRounding,
                        MarkupRules = pl.MarkupRules.Select(mr => new PriceMarkupRule { DollarRangeLower = mr.DollarRangeLower, DollarRangeUpper = mr.DollarRangeUpper, PercentLimitLower = mr.PercentLimitLower, PercentLimitUpper = mr.PercentLimitUpper }).ToList(),
                        RoundingRules = pl.RoundingRules.Select(rr => new PriceRoundingRule { DollarRangeLower = rr.MinValue, DollarRangeUpper = rr.MaxValue, ValueChange = rr.ValueChange, RoundingType = (RoundingType)rr.RoundingType }).ToList()
                    }).ToList(),
                    ValueDrivers = copy.Drivers.Select(d => new PricingValueDriver
                    {
                        DriverType = (DriverType)d.DriverId,
                        IsKey = d.IsKey.GetValueOrDefault(),
                        IsSelected = (d.IsKey != null),
                        Groups = d.OptimizationRules.Select(or => new PricingValueDriverGroup
                        {
                            GroupNumber = or.GroupNum,
                            OptimizationRules = or.PriceRanges.Select(pr => new PriceOptimizationRule
                            {
                                DollarRangeLower = pr.DollarRangeLower,
                                DollarRangeUpper = pr.DollarRangeUpper,
                                PercentChange = pr.PercentChange
                            }).ToList(),
                            InfluencerPercentChange = or.InfluencerPercentChange


                        }).ToList()
                    }).ToList()
                };

                //Fix for populating the filters on copy - needs to go FN_PricingLoadFilters through FN_ IsSelectedInAnalytic to populate IsSelectedInPricing.
                newPricing.Filters = LoadFilters(pricingId);

                ENT.PricingPriceList keyPriceList = newPricing.PriceLists.FirstOrDefault(item => item.IsKey);
                if (keyPriceList != null)
                {
                    newPricing.KeyPriceListId = keyPriceList.PriceListId;
                }

                return newPricing;
            }
        }


        public List<PricingResult> LoadResults(int pricingId, PricingResultFilter[] filters, IProgress<String> progress)
        {
            if (pricingId == 0) throw new ArgumentOutOfRangeException("Invalid parameter : pricingId.");

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

                var filterKeys = filters != null ? JsonConvert.SerializeObject(filters) : "[]";
                var sql = string.Format("SELECT * FROM \"PX_Main\".\"SP_PricingLoadResults\"({0},'{1}'::jsonb) ", pricingId, filterKeys);
                var results = new List<PricingResult>();
                using (IDbCommand command = db.CreateCommand())
                {
                    command.CommandText = sql;
                    using (IDataReader dr = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                    {
                        while (dr.Read())
                        {
                            results.Add(new PricingResult
                            {
                                SkuId = (int)dr["ProductId"],
                                SkuCode = dr["Sku"].ToString(),
                                SkuDescription = dr["ProductName"].ToString(),
                                Cost = (decimal)dr["Cost"],
                                Inventory = (decimal)dr["Inventory"],
                                ClusterId = (int)dr["ClusterId"],
                                PricingResultDetail = JsonConvert.DeserializeObject<List<PricingResultDetail>>(dr["PricingResultDetail"].ToString())
                                        .Select(prd => new PricingResultDetail
                                        {
                                            ProductId = prd.ProductId,
                                            PriceListId = prd.PriceListId,
                                            PriceListName = prd.PriceListName,
                                            CurrentPrice = prd.CurrentPrice,
                                            RecommendedPrice = prd.RecommendedPrice,
                                            FinalPrice = prd.FinalPrice,
                                            PricingResultEditTypeId = (PricingResultsEditType)prd.PricingResultEditTypeId,
                                            Alerts = prd.Alerts != null ? prd.Alerts.Select(a => new Alert
                                            {
                                                AlertId = a.AlertId,
                                                AlertMessage = a.AlertMessage,
                                                Severity = a.Severity
                                            }).ToList() : new List<Alert>(),
                                            HasCompetitionData = prd.HasCompetitionData,
                                            CompetitionMinPrice = prd.CompetitionMinPrice,
                                            CompetitionMaxPrice = prd.CompetitionMaxPrice,
                                            CompetitivePosition = prd.CompetitivePosition
                                        }).ToList(),
                                HasCompetitionData = (bool)dr["HasCompetitionData"]

                            });
                        }
                        dr.Close();
                    }

                }
                return results;


            }

        }

        public PricingResult LoadResults(int pricingId, int productId)
        {
            if (pricingId == 0) throw new ArgumentOutOfRangeException("Invalid parameter : pricingId.");

            using (var db = DBConnectionFactory.OpenDbConnection())
            {
                var sql = string.Format("SELECT * FROM \"PX_Main\".\"SP_PricingLoadResults\"({0},'{1}') ", pricingId, productId);
                var result = db.Single<DB.PricingResult>(sql);

                if (result == null) { return new PricingResult(); }
                return new PricingResult
                {
                    SkuId = result.ProductId,
                    SkuCode = result.Sku,
                    SkuDescription = result.ProductName,
                    Cost = result.Cost,
                    ClusterId = result.ClusterId,
                    HasCompetitionData = result.HasCompetitionData,
                    Inventory = result.Inventory,
                    PricingResultDetail = result.PricingResultDetail != null ? result.PricingResultDetail.Select(prd => new PricingResultDetail
                    {
                        ProductId = prd.ProductId,
                        PriceListId = prd.PriceListId,
                        PriceListName = prd.PriceListName,
                        CurrentPrice = prd.CurrentPrice,
                        RecommendedPrice = prd.RecommendedPrice,
                        FinalPrice = prd.FinalPrice,
                        PricingResultEditTypeId = (PricingResultsEditType)prd.PricingResultEditType,
                        Alerts = prd.Alerts != null ? prd.Alerts.Select(a => new Alert
                        {
                            AlertId = a.AlertId,
                            AlertMessage = a.AlertMessage,
                            Severity = a.Severity
                        }).ToList() : new List<Alert>(),
                        HasCompetitionData = prd.HasCompetitionData,
                        CompetitionMinPrice = prd.CompetitionMinPrice,
                        CompetitionMaxPrice = prd.CompetitionMaxPrice,
                        CompetitivePosition = prd.CompetitivePosition
                    }).ToList() : new List<PricingResultDetail>()
                };
            }

        }


        public PricingResultValueDriverDetail GetResultDetail(int pricingId, int productId)
        {
            //if (pricingId == 0) throw new ArgumentOutOfRangeException("Invalid parameter : pricingId.");

            using (var db = DBConnectionFactory.OpenDbConnection())
            {
                var sql = string.Format("SELECT \"PriceListId\", \"PriceDrivers\" FROM \"PX_Main\".\"FN_PricingGetResultDetail\"({0},{1}) ", pricingId, productId);
                var results = db.Single<DB.PricingResultValueDriverDetail>(sql);

                if (results == null) { return new PricingResultValueDriverDetail(); }
                return new PricingResultValueDriverDetail()
                {
                    PriceListId = results.PriceListId,
                    DriverDetail = results.PriceDrivers.Select(r => new PricingValueDriverDetail
                    {
                        DriverId = r.DriverId,
                        DriverName = r.DriverName,
                        IsKey = r.IsKey,
                        PriceChange = r.PriceChange,
                        DriverGroup = r.DriverGroup,
                        DriverPercent = r.DriverPercent
                    }).ToList()
                };
            }

        }


        public List<PricingResult> SaveResults(int pricingId, PricingResultFilter[] filters, List<PricingResult> resultsToSave)
        {
            if (pricingId == 0) throw new ArgumentOutOfRangeException("Invalid parameter : pricingId.");

            using (var db = DBConnectionFactory.OpenDbConnection())
            {
                var filterKeys = filters != null ? JsonConvert.SerializeObject(filters) : "[]";

                var jsonResults = JsonConvert.SerializeObject(resultsToSave.SelectMany(r => r.PricingResultDetail).ToList());
                var results = db.SqlList<DB.PricingResult>(string.Format("SELECT * FROM \"PX_Main\".\"SP_PricingSaveResults\"({0},'{1}','{2}') ", pricingId, filterKeys, jsonResults));
                if (results == null) { return new List<PricingResult>(); }
                return results.Select(r => new PricingResult
                {
                    SkuId = r.ProductId,
                    SkuCode = r.Sku,
                    SkuDescription = r.ProductName,
                    Cost = r.Cost,
                    ClusterId = r.ClusterId,
                    PricingResultDetail = r.PricingResultDetail != null ? r.PricingResultDetail.Select(prd => new PricingResultDetail
                    {
                        ProductId = prd.ProductId,
                        PriceListId = prd.PriceListId,
                        PriceListName = prd.PriceListName,
                        CurrentPrice = prd.CurrentPrice,
                        RecommendedPrice = prd.RecommendedPrice,
                        FinalPrice = prd.FinalPrice,
                        PricingResultEditTypeId = (PricingResultsEditType)prd.PricingResultEditType,
                        Alerts = prd.Alerts != null ? prd.Alerts.Select(a => new Alert
                        {
                            AlertId = a.AlertId,
                            AlertMessage = a.AlertMessage,
                            Severity = a.Severity
                        }).ToList() : new List<Alert>()
                    }).ToList() : new List<PricingResultDetail>()
                }).ToList();
            }
        }


        public List<PricingResult> RecalculateResults(int pricingId, PricingResultFilter[] filters, IProgress<String> progress)
        {
            if (pricingId == 0) throw new ArgumentOutOfRangeException("Invalid parameter : pricingId.");

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

                // recalculate results
                db.ExecuteNonQuery(string.Format("SELECT \"PX_Main\".\"SP_PricingRecalculateResults\"({0})", pricingId));


                return LoadResults(pricingId, filters, progress);

                ////load results
                //var filterKeys = filters != null ? JsonConvert.SerializeObject(filters) : "[]";
                //var results = db.SqlList<DB.PricingResult>(string.Format("SELECT * FROM \"PX_Main\".\"SP_PricingLoadResults\"   ({0}, '{1}')", pricingId, filterKeys));
                //if (results == null) { return new List<PricingResult>(); }
                //return results.Select(r => new PricingResult
                //{
                //    SkuId = r.ProductId,
                //    SkuCode = r.Sku,
                //    SkuDescription = r.ProductName,
                //    Cost = r.Cost,
                //    Inventory = r.Inventory,
                //    ClusterId = r.ClusterId,
                //    PricingResultDetail = r.PricingResultDetail != null ? r.PricingResultDetail.Select(prd => new PricingResultDetail
                //    {
                //        ProductId = prd.ProductId,
                //        PriceListId = prd.PriceListId,
                //        PriceListName = prd.PriceListName,
                //        CurrentPrice = prd.CurrentPrice,
                //        RecommendedPrice = prd.RecommendedPrice,
                //        FinalPrice = prd.FinalPrice,
                //        PricingResultEditTypeId = (PricingResultsEditType)prd.PricingResultEditType,
                //        Alerts = prd.Alerts != null ? prd.Alerts.Select(a => new Alert
                //        {
                //            AlertId = a.AlertId,
                //            AlertMessage = a.AlertMessage,
                //            Severity = a.Severity
                //        }).ToList() : new List<Alert>()
                //    }).ToList() : new List<PricingResultDetail>(),
                //    HasCompetitionData = r.HasCompetitionData
                //}).ToList();
            }
        }

        public ResultSummary GetResultSummary(int pricingId)
        {
            if (pricingId == 0) throw new ArgumentOutOfRangeException("Invalid parameter : pricingId.");

            using (var db = DBConnectionFactory.OpenDbConnection())
            {
                var r = db.Single<DB.ResultSummary>(string.Format("SELECT \"Name\",\"Description\", \"CreatedDate\", \"ModifiedDate\", \"SkuCount\", \"PriceCount\", \"EditedCount\",\"ExcludedCount\",\"WarningCount\" FROM \"PX_Main\".\"SP_PricingResultSummary\"({0})", pricingId));
                return new ResultSummary
                {
                    Name = r.Name,
                    Description = r.Description,
                    CreatedDate = r.CreatedDate,
                    ModifiedDate = r.ModifiedDate,
                    SkuCount = r.SkuCount,
                    PriceCount = r.PriceCount,
                    EditedCount = r.EditedCount,
                    ExcludedCount = r.ExcludedCount,
                    WarningCount = r.WarningCount
                };
            }
        }

        public AnalyticAggregates GetAnalyticAggregates(int analyticId)
        {
            if (analyticId == 0) throw new ArgumentOutOfRangeException("Invalid parameter : analyticId.");

            using (var db = DBConnectionFactory.OpenDbConnection())
            {
                var summary = db.Single<DB.AnalyticSummary>(string.Format("SELECT \"SkuCount\", \"TotalSalesValue\" FROM \"PX_Main\".\"SP_AnalyticGetSummary\"({0})", analyticId));
                return new AnalyticAggregates { SkuCount = summary.SkuCount, TotalSalesValue = summary.TotalSalesValue };
            }
        }


        public bool SaveFolderAssignment(int pricingId, int folderId)
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                db.UpdateOnly(new DB.Pricing
                {
                    FolderId = folderId
                }

                            , onlyFields: p => new { p.FolderId }
                            , where: p => p.PricingId == pricingId);
                return true;
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
                result = db.SqlScalar<int>(string.Format("select \"PX_Main\".\"SP_ProductFilterGetCount\"('{0}')", JsonConvert.SerializeObject(dbf)));
            }

            return result;
        }

        /// <summary>
        /// Gets the number of SKUs included for the specified price routine and collection of price lists.
        /// </summary>
        /// <param name="pricingId">The ID of the price routine.</param>
        /// <param name="priceListIds">The IDs of the included price lists.</param>
        /// <returns>The result, as an int.</returns>
        public int GetPriceListSkuCount(int pricingId, IEnumerable<int> priceLists)
        {
            int result = 0;
            var priceListKeys = priceLists != null ? JsonConvert.SerializeObject(priceLists) : "[]";

            using (var db = DBConnectionFactory.OpenDbConnection())
            {
                result = db.SqlScalar<int>(string.Format("select \"PX_Main\".\"SP_PricingPriceListFilterGetCount\"({0},'{1}')", pricingId, priceListKeys));
            }

            return result;
        }

        #region Impact Analysis

        public ENT.ImpactAnalysis CreateImpactAnalysis(int pricingId)
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                var pricing = db.Single<DB.Pricing>(p => p.PricingId == pricingId);
                if (pricing == null)
                {
                    throw new KeyNotFoundException("Pricing not found.");
                }

                var analytic = db.Single<DB.Analytic>(a => a.AnalyticId == pricing.AnalyticId);
                if (analytic == null)
                {
                    throw new KeyNotFoundException("Analytic not found.");
                }

                return new ENT.ImpactAnalysis
                {
                    Name = string.Empty,
                    Notes = string.Empty,
                    PricingId = pricing.PricingId,
                    AnalyticStartDate = analytic.AggStartDate,
                    AnalyticEndDate = analytic.AggEndDate,
                    ProjectedStartDate = analytic.AggStartDate.AddYears(1),
                    ProjectedEndDate = analytic.AggEndDate.AddYears(1),
                    ElasticityRegulator = 1M, //sensitivity factor
                    DefaultPriceElasticity = -1M,
                    TrendCalcLength = 365,
                    Results = new ENT.ImpactAnalysisResultSet[0],
                    Forecast = new ENT.ImpactAnalysisDataPointSet[0]
                };
            }
        }

        public ImpactAnalysis LoadImpactAnalysis(int impactAnalysisId)
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                DB.ImpactScenario dbRow = db.Single<DB.ImpactScenario>(item => item.ImpactId == impactAnalysisId);

                var pricing = db.Single<DB.Pricing>(p => p.PricingId == dbRow.PricingId);
                if (pricing == null)
                {
                    throw new KeyNotFoundException("Pricing not found.");
                }
                var analytic = db.Single<DB.Analytic>(a => a.AnalyticId == pricing.AnalyticId);
                if (analytic == null)
                {
                    throw new KeyNotFoundException("Analytic not found.");
                }

                ENT.ImpactAnalysis dto = MapToDto(dbRow, analytic);

                return dto;
            }
        }

        public List<ImpactAnalysis> LoadImpactAnalyses(int pricingId)
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                var dbPricing = db.Single<DB.Pricing>(p => p.PricingId == pricingId);
                if (dbPricing == null)
                {
                    throw new KeyNotFoundException("Pricing not found.");
                }
                var dbAnalytic = db.Single<DB.Analytic>(a => a.AnalyticId == dbPricing.AnalyticId);
                if (dbAnalytic == null)
                {
                    throw new KeyNotFoundException("Analytic not found.");
                }

                List<DB.ImpactScenario> dbRows = db.Select<DB.ImpactScenario>(item => item.PricingId == pricingId);
                List<ENT.ImpactAnalysis> dtoList = dbRows.Select(row => MapToDto(row, dbAnalytic)).ToList();

                return dtoList;
            }
        }

        public bool DeleteImpactAnalysis(int impactAnalysisId)
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                db.Delete<DB.ImpactScenario>(i => i.ImpactId == impactAnalysisId);
            }
            return true;
        }

        public long SaveImpactAnalysis(ImpactAnalysis analysis)
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                long impactId = analysis.ImpactId;

                var dbRow = MapToDbEntity(analysis);

                if (impactId == 0)
                {
                    db.Insert(dbRow);
                    impactId = db.LastInsertId();
                }
                else
                {
                    db.UpdateOnly(dbRow,
                         onlyFields: p => new
                         {
                             ImpactId = p.ImpactId,
                             Name = p.Name,
                             Notes = p.Notes,
                             PricingId = p.PricingId,
                             ProjectedStartDate = p.ProjectedStartDate,
                             ProjectedEndDate = p.ProjectedEndDate,
                             ElasticityRegulator = p.ElasticityRegulator,
                             DefaultPriceElasticity = p.DefaultPriceElasticity,
                             TrendCalcLength = p.TrendCalcDays,
                             Results = p.Results,
                             Forecast = p.Forecast
                         },
                         where: p => p.ImpactId == analysis.ImpactId);
                }

                return impactId;
            }
        }

        public bool SaveImpactAnalyses(IEnumerable<ImpactAnalysis> impactAnalyses)
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                foreach (ENT.ImpactAnalysis item in impactAnalyses)
                {
                    SaveImpactAnalysis(item);
                }
            }

            return true;
        }

        public ImpactAnalysis RunImpactAnalysis(ImpactAnalysis analysis, IProgress<String> progress)
        {
            // attach event handler to connection to report progress
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                //Set up the progress reporting mechanism.
                NpgsqlConnection conn = (NpgsqlConnection)db.ToDbConnection();
                conn.Notice += (sender, args) =>
                {
                    if (progress != null)
                    {
                        progress.Report(args.Notice.Message);
                    }
                };

                // build SQL for forecast prep function call
                string sql = string.Format("SELECT \"PX_Track\".\"SP_PricingForecastPrepare\"({0},{1},{2},{3},'{4}','{5}') ",
                         analysis.PricingId,
                         analysis.TrendCalcLength,
                         analysis.ElasticityRegulator,
                         analysis.DefaultPriceElasticity,
                         analysis.ProjectedStartDate,
                         analysis.ProjectedEndDate);
                int pid = db.Scalar<int>(sql);

                // build SQL for function call
                sql = string.Format("SELECT \"Results\"::jsonb,\"Forecast\"::jsonb FROM \"PX_Track\".\"SP_PricingForecastImpact\"({0},{1},{2},{3},{4},'{5}','{6}') ",
                                     pid,
                                     analysis.PricingId,
                                     analysis.TrendCalcLength,
                                     analysis.ElasticityRegulator,
                                     analysis.DefaultPriceElasticity,
                                     analysis.ProjectedStartDate,
                                     analysis.ProjectedEndDate);


                // run analysis
                DB.ImpactAnalysisCalculation dbRow = db.Single<DB.ImpactAnalysisCalculation>(sql);

                //put results and forecast back into analysis and return it
                analysis.Results = MapToDtos(dbRow.Results);
                analysis.Forecast = MapToDtos(dbRow.Forecast);

                conn.Notice -= (sender, args) =>
                {
                    if (progress != null)
                    {
                        progress.Report(args.Notice.Message);
                    }
                };
            }

            return analysis;
        }

        #region Impact Analysis Entity-DTO Mapping helpers

        /// <summary>
        /// Maps an <see cref="ImpactAnalysis"/> DTO to a database-specific <see cref="ImpactScenario"/> entity.
        /// </summary>
        private DB.ImpactScenario MapToDbEntity(ENT.ImpactAnalysis dto)
        {
            DB.ImpactScenario dbRow = new DB.ImpactScenario
            {
                PricingId = dto.PricingId,
                Name = dto.Name,
                Notes = dto.Notes,
                ProjectedStartDate = dto.ProjectedStartDate,
                ProjectedEndDate = dto.ProjectedEndDate,
                TrendCalcDays = dto.TrendCalcLength,
                DefaultPriceElasticity = dto.DefaultPriceElasticity,
                ElasticityRegulator = dto.ElasticityRegulator,

                //Populate contained entities.
                Results = MapToDbEntities(dto.Results),
                Forecast = MapToDbEntities(dto.Forecast)
            };

            return dbRow;
        }

        /// <summary>
        /// Maps an <see cref="ImpactScenario"/> database entity to an <see cref="ImpactAnalysis"/> DTO.
        /// </summary>       
        private ENT.ImpactAnalysis MapToDto(DB.ImpactScenario dbRow, DB.Analytic analyticDbEntity)
        {
            var dto = new ENT.ImpactAnalysis
            {
                ImpactId = dbRow.ImpactId,
                Name = dbRow.Name,
                Notes = dbRow.Notes,
                PricingId = dbRow.PricingId,
                AnalyticStartDate = analyticDbEntity.AggStartDate,
                AnalyticEndDate = analyticDbEntity.AggEndDate,
                ProjectedStartDate = dbRow.ProjectedStartDate,
                ProjectedEndDate = dbRow.ProjectedEndDate,
                ElasticityRegulator = dbRow.ElasticityRegulator,
                DefaultPriceElasticity = dbRow.DefaultPriceElasticity,
                TrendCalcLength = dbRow.TrendCalcDays,

                //Populate child entites:
                Results = MapToDtos(dbRow.Results),
                Forecast = MapToDtos(dbRow.Forecast)
            };

            return dto;
        }

        /// <summary>
        ///  Maps a collection of <see cref="ImpactAnalysisDataPointSet"/> DTOs to a collection of <see cref="ImpactAnalysisDataPointSet"/> database entities.
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        private DB.ImpactAnalysisDataPointSet[] MapToDbEntities(IEnumerable<ENT.ImpactAnalysisDataPointSet> dtos)
        {
            var dbRows = dtos.Select(rs => new DB.ImpactAnalysisDataPointSet
            {
                PeriodGranularityId = (int)rs.PeriodGranularityId,
                Points = rs.Points.Select(dp => new DB.DataPoint
                {
                    ActualQty = dp.ActualQty,
                    ImpactQty = dp.ImpactQty,
                    ProjectedQty = dp.ProjectedQty,
                    TrendQty = dp.TrendQty,
                    ActualAmount = dp.ActualAmount,
                    ImpactAmount = dp.ImpactAmount,
                    ProjectedAmount = dp.ProjectedAmount,
                    TrendAmount = dp.TrendAmount,
                    DataPointDate = dp.DataPointDate,
                    Label = dp.Label
                }).ToArray()
            });

            return dbRows.ToArray();
        }

        /// <summary>
        /// Maps a collection of <see cref="ImpactAnalysisResultSet"/> DTOs to a collection of <see cref="ImpactAnalysisResultSet"/> database entities.
        /// </summary>      
        private DB.ImpactAnalysisResultSet[] MapToDbEntities(IEnumerable<ENT.ImpactAnalysisResultSet> dtos)
        {
            var dbRows = dtos.Select(rs => new DB.ImpactAnalysisResultSet
            {
                PeriodGranularityId = (int)rs.PeriodGranularityId,
                Items = rs.Items.Select(r => new DB.ImpactAnalysisResult
                {
                    Name = r.Name,
                    UnitOfMeasureId = (int)r.UnitOfMeasureType,
                    ActualAmount = r.ActualAmount,
                    ImpactAmount = r.ImpactAmount,
                    ProjectedAmount = r.ProjectedAmount
                }).ToArray()
            });

            return dbRows.ToArray();
        }

        /// <summary>
        /// Maps a collection of <see cref="ImpactAnalysisResultSet"/> database entities to a collection of <see cref="ImpactAnalysisResultSet"/> DTOs.
        /// </summary>
        private ENT.ImpactAnalysisResultSet[] MapToDtos(IEnumerable<DB.ImpactAnalysisResultSet> dbResultRows)
        {
            ENT.ImpactAnalysisResultSet[] result = new ImpactAnalysisResultSet[0];

            if (dbResultRows != null)
            {
                var dtoRows = dbResultRows.Select(rs => new ENT.ImpactAnalysisResultSet
                {
                    PeriodGranularityId = (ENT.CalculationInterval)rs.PeriodGranularityId,
                    Items = rs.Items.Select(r => new ENT.ImpactAnalysisResult
                    {
                        Name = r.Name,
                        UnitOfMeasureType = (UnitOfMeasureType)r.UnitOfMeasureId,
                        ActualAmount = r.ActualAmount,
                        ImpactAmount = r.ImpactAmount,
                        ProjectedAmount = r.ProjectedAmount
                    }).ToArray()
                });
                result = dtoRows.ToArray();
            }

            return result;
        }

        /// <summary>
        /// Maps a collection of <see cref="ImpactAnalysisDataPointSet"/> database entities to a collection of <see cref="ImpactAnalysisDataPointSet"/> DTOs.
        /// </summary>
        /// <param name="dbImpactForecastRows"></param>
        /// <returns></returns>
        private ENT.ImpactAnalysisDataPointSet[] MapToDtos(IEnumerable<DB.ImpactAnalysisDataPointSet> dbImpactForecastRows)
        {
            ENT.ImpactAnalysisDataPointSet[] result = new ImpactAnalysisDataPointSet[0];

            if (dbImpactForecastRows != null)
            {
                var dtoRows = dbImpactForecastRows.Select(rs => new ENT.ImpactAnalysisDataPointSet
                {
                    PeriodGranularityId = (ENT.CalculationInterval)rs.PeriodGranularityId,
                    Points = rs.Points.Select(dp => new ENT.DataPoint
                    {
                        ActualQty = dp.ActualQty,
                        ImpactQty = dp.ImpactQty,
                        ProjectedQty = dp.ProjectedQty,
                        TrendQty = dp.TrendQty,
                        ActualAmount = dp.ActualAmount,
                        ImpactAmount = dp.ImpactAmount,
                        ProjectedAmount = dp.ProjectedAmount,
                        TrendAmount = dp.TrendAmount,
                        DataPointDate = dp.DataPointDate,
                        Label = dp.Label
                    }).ToArray()
                });
                result = dtoRows.ToArray();
            }

            return result;
        }

        #endregion

        #endregion

        public List<APLPX.Entity.CompetitionData> LoadCompetitionData(int productId)
        {
            if (productId == 0) throw new ArgumentOutOfRangeException("Invalid parameter : productId.");

            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {

                //var competitionData = db.Select<DB.CompetitionView>().Where(q => q.ProductId == productId);
                var competitionData = db.SqlList<DB.CompetitionView>(string.Format("SELECT \"CompetitorName\", \"CompetitorSKU\", \"CompetitorProductName\", \"EffectiveDate\", \"CompetitorPrice\", \"ShippingCost\", \"OutOfStockFlag\", \"Attributes\"  FROM \"PX_Main\".\"SP_PricingGetCompetitionView\"({0})", productId));

                if (competitionData == null) { return new List<APLPX.Entity.CompetitionData>(); }

                foreach (DB.CompetitionView competitor in competitionData)
                {
                    if (competitor.Attributes == null)
                        competitor.Attributes = new DB.CompetitorAttribute[0];
                }

                return competitionData.Select(cd => new APLPX.Entity.CompetitionData
                {
                    ProductId = productId,
                    CompetitorName = cd.CompetitorName,
                    CompetitorSKU = cd.CompetitorSku,
                    //SalesChannel = cd.SalesChannel,
                    CompetitorProductName = cd.CompetitorProductName,
                    EffectiveDate = cd.EffectiveDate,
                    CompetitorPrice = cd.CompetitorPrice,
                    ShippingCost = cd.ShippingCost,
                    OutOfStockFlag = cd.OutOfStockFlag,
                    Attributes = cd.Attributes.Select(dataPoint => new CompetitorAttribute
                        {
                            Attribute = dataPoint.Attribute,
                            Value = dataPoint.Value
                        }).ToList()
                }).ToList();

            }

        }

        public List<APLPX.Entity.CompetitorSeries> LoadCompetitionSeries(int productId, int pricingId)
        {
            if (productId == 0) throw new ArgumentOutOfRangeException("Invalid parameter : productId.");

            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                //var competitorSeries = db.Select<DB.CompetitorSeries>().Where(q => q.PricingId == pricingId);
                var competitorSeries = db.SqlList<DB.CompetitorSeries>(string.Format("SELECT * FROM \"PX_Main\".\"FN_PricingGetCompetitionSeries\"({0},{1})", productId, pricingId));

                if (competitorSeries == null) { return new List<CompetitorSeries>(); }

                foreach (DB.CompetitorSeries series in competitorSeries)
                {
                    if (series.DataPoints == null)
                        series.DataPoints = new DB.CompetitorSeriesAttribute[0];
                }

                return competitorSeries.Select(cs => new CompetitorSeries
                {
                    MetricName = cs.MetricName,
                    CompetitorName = cs.CompetitorName,
                    CompetitorSku = cs.CompetitorSku,
                    //SalesChannel = cs.SalesChannel,
                    CompetitorProductName = cs.CompetitorProductName,
                    IsCompetitor = cs.IsCompetitor,
                    Order = cs.Order,
                    DataPoints = cs.DataPoints.Select(dataPoint => new CompetitorSeriesAttribute
                        {
                            DataPointDate = dataPoint.Date,
                            Value = dataPoint.Value
                        }).ToList()
                }).ToList();

            }

        }


        public List<ENT.PricingChartSummary> GetResultsChartSummary(int pricingId)
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                string sql = string.Format("SELECT * FROM \"PX_Main\".\"FN_PricingGetChartSummary\"({0})", pricingId);

                var chartSummary = db.SqlList<DB.PricingChartSummary>(sql);

                return chartSummary.ConvertAll<ENT.PricingChartSummary>(y => y.ToDto());

            }

        }


        public long GetResultCount(int pricingId)
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                string sql = string.Format("SELECT \"PX_Main\".\"FN_PricingGetResultCount\"({0})", pricingId);

                long numResults = db.Scalar<long>(sql);

                return numResults;
            }
        }

        public void SetRecalculateFlag(int pricingId)
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                string sql = string.Format("SELECT \"PX_Main\".\"SP_PricingSetRecalculateFlag\"({0},{1})", pricingId, true);

                db.ExecuteNonQuery(sql);

            }
        }
        public void DeleteCompetitionProduct(int productId, string competitorName, string competitorSku)
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                string sql = string.Format("SELECT \"PX_Main\".\"FN_PricingDeleteCompetitionProduct\"({0},'{1}','{2}')", productId, competitorName, competitorSku);

                db.ExecuteNonQuery(sql);

            }

        }

        #region Price Routine Entity-DTO Mapping helpers



        /// <summary>
        /// Maps a <see cref="Pricing" database entity to a <see cref="PricingEveryday"/> DTO./>
        /// </summary>
        /// <param name="pricingEntity"></param>
        /// <param name="userEntity"></param>
        /// <returns></returns>
        private ENT.PricingEveryday ToPricingEverydayDto(DB.Pricing pricingEntity, DB.User userEntity)
        {
            var result = new ENT.PricingEveryday
            {
                Id = pricingEntity.PricingId,
                Name = pricingEntity.Name,
                Description = pricingEntity.Description,
                Notes = pricingEntity.Notes,
                OwnerId = pricingEntity.OwnerId,
                OwnerName = string.Format("{0} {1}", userEntity.FirstName, userEntity.LastName),
                PricingMode = (PricingMode)pricingEntity.PricingModeId,
                FolderId = pricingEntity.FolderId,
                AnalyticId = pricingEntity.AnalyticId,
                AnalyticName = pricingEntity.AnalyticName,
                ApproverId = pricingEntity.ApproverId,
                PricingStateId = pricingEntity.PricingStateId,
                ChangeProductFiltersFlag = pricingEntity.ChangeProductFiltersFlag,
                CreatedDate = pricingEntity.CreateTS,
                ModifiedDate = pricingEntity.ModifyTS,
                NeedsRecalculation = pricingEntity.NeedsRecalculation
            };

            return result;
        }

        /// <summary>
        /// Maps a collection of <see cref="ProductFilterGroup" database entities to a collection of <see cref="FilterGroup"/> DTOs./>
        /// </summary>
        /// <param name="dbEntity"></param>
        /// <returns></returns>
        private List<ENT.FilterGroup> ToFilterGroupDtos(DB.Pricing dbEntity)
        {
            var result = new List<ENT.FilterGroup>();

            if (dbEntity.ProductFilters != null)
            {
                var dtos = from filterGroup in dbEntity.ProductFilters
                           group filterGroup by filterGroup.FilterTypeId into grp
                           select new ENT.FilterGroup
                           {
                               FilterType = grp.Key,
                               Filters = grp.SelectMany(h => h.Values)
                                          .Select(fv => new Filter { Code = fv, FilterType = grp.Key }).ToList()
                           };

                if (dtos != null)
                {
                    result = dtos.ToList();
                }
            }
            return result;
        }

        /// <summary>
        /// Maps a collection of <see cref="PricingPriceListExtended" database entities to a collection of <see cref="PricingPriceList"/> DTOs./>
        /// </summary>
        /// <param name="dbEntities"></param>
        /// <returns></returns>
        private List<ENT.PricingPriceList> ToPriceListDtos(IEnumerable<DB.PricingPriceListExtended> dbEntities)
        {
            var result = new List<PricingPriceList>();

            var dtos = dbEntities.Select(dbEntity => new ENT.PricingPriceList
            {
                PricingId = dbEntity.PricingId,
                PriceListId = dbEntity.PriceListId,
                IsKey = dbEntity.IsKey,
                MinValue = dbEntity.MinValue,
                MaxValue = dbEntity.MaxValue,
                PercentChange = dbEntity.PercentChange,
                ApplyMarkup = dbEntity.ApplyMarkup,
                ApplyRounding = dbEntity.ApplyRounding,
                MarkupRules = ToMarkupRuleDtos(dbEntity),
                RoundingRules = ToRoundingRuleDtos(dbEntity),

                Name = dbEntity.PriceListName,
                PriceListType = dbEntity.PriceListType,
                IsPromotion = (dbEntity.PriceListType == "P"),
                Code = dbEntity.PriceListCode,
                Description = dbEntity.PriceListDescription,
                Sort = dbEntity.Sort,
                EffectiveDate = dbEntity.EffectiveDate,
                EndDate = dbEntity.EndDate
            });

            if (dtos != null)
            {
                result = dtos.ToList();
            }

            return result;
        }

        private List<ENT.PriceMarkupRule> ToMarkupRuleDtos(DB.PricingPriceList dbEntity)
        {
            var dtos = dbEntity.MarkupRules.Select(dbRule => new ENT.PriceMarkupRule
            {
                DollarRangeLower = dbRule.DollarRangeLower,
                DollarRangeUpper = dbRule.DollarRangeUpper,
                PercentLimitLower = dbRule.PercentLimitLower,
                PercentLimitUpper = dbRule.PercentLimitUpper
            });
            return dtos.ToList();
        }

        private List<ENT.PriceRoundingRule> ToRoundingRuleDtos(DB.PricingPriceList dbEntity)
        {
            var dtos = dbEntity.RoundingRules.Select(dbRule => new ENT.PriceRoundingRule
            {
                DollarRangeLower = dbRule.MinValue,
                DollarRangeUpper = dbRule.MaxValue,
                ValueChange = dbRule.ValueChange,
                RoundingType = (RoundingType)dbRule.RoundingType
            });
            return dtos.ToList();
        }

        /// <summary>
        /// Maps a collection of <see cref="PricingDriverExtended" database entities to a collection of <see cref="PricingValueDriver"/> DTOs./>
        /// </summary>
        /// <param name="dbEntities"></param>
        /// <returns></returns>
        private List<ENT.PricingValueDriver> ToValueDriverDtos(IEnumerable<DB.PricingDriverExtended> dbEntities)
        {
            var result = new List<ENT.PricingValueDriver>();

            var dtos = dbEntities.Select(dbEntity => ToValueDriverDto(dbEntity));

            if (dtos != null)
            {
                result = dtos.ToList();
            }

            return result;
        }

        /// <summary>
        /// Maps a <see cref="PricingDriverExtended" database entity to a <see cref="PricingValueDriver"/> DTO./>
        /// </summary>
        /// <param name="dbEntity"></param>
        /// <returns></returns>
        private ENT.PricingValueDriver ToValueDriverDto(DB.PricingDriverExtended dbEntity)
        {
            var result = new ENT.PricingValueDriver
            {
                Id = dbEntity.DriverId,
                Name = dbEntity.Name,
                Description = dbEntity.Description,
                DriverType = (DriverType)dbEntity.DriverId,
                UnitOfMeasure = dbEntity.UnitOfMeasure,
                ChangeDriverFlag = dbEntity.ChangeDriverFlag,
                IsKey = dbEntity.IsKey.GetValueOrDefault(),
                IsSelected = (dbEntity.IsKey != null),

                Groups = ToDriverGroupDtos(dbEntity)
            };
            return result;
        }

        /// <summary>
        /// Maps a collection of <see cref="PricingDriverExtended" database entities to a collection of <see cref="PricingValueDriverGroup"/> DTOs./>
        /// </summary>
        /// <param name="dbEntity"></param>
        /// <returns></returns>
        private List<ENT.PricingValueDriverGroup> ToDriverGroupDtos(DB.PricingDriverExtended dbEntity)
        {
            var result = new List<ENT.PricingValueDriverGroup>();

            //Create the driver groups based on the optimization rules.
            foreach (DB.PriceOptimizationRule dbRule in dbEntity.OptimizationRules)
            {
                var driverGroupDto = new ENT.PricingValueDriverGroup
                {
                    GroupNumber = dbRule.GroupNum,
                    SkuCount = dbEntity.Encoding.Single(g => g.GroupNumber == dbRule.GroupNum).SkuCount,
                    SalesValue = dbEntity.Encoding.Single(g => g.GroupNumber == dbRule.GroupNum).SalesValue,
                    OptimizationRules = dbRule.PriceRanges.Select(pr => new ENT.PriceOptimizationRule
                    {
                        DollarRangeLower = pr.DollarRangeLower,
                        DollarRangeUpper = pr.DollarRangeUpper,
                        PercentChange = pr.PercentChange
                    }).ToList(),

                    InfluencerPercentChange = dbRule.InfluencerPercentChange,
                    ChangeGroupFlag = dbRule.ChangeGroupFlag
                };

                result.Add(driverGroupDto);
            }

            //Set the outlier range from the corresponding driver group.
            for (int i = 0; i < dbEntity.Encoding.Length; i++)
            {
                DB.DriverGroup encoding = dbEntity.Encoding[i];

                ENT.PricingValueDriverGroup destGroup = result.Where(item => item.GroupNumber == encoding.GroupNumber).FirstOrDefault();
                if (destGroup != null)
                {
                    destGroup.MinOutlier = encoding.MinOutlier;
                    destGroup.MaxOutlier = encoding.MaxOutlier;
                }
            }
            return result;
        }


        #endregion


    }

    public static class PricingConvertExtensions
    {

        public static ENT.PricingChartSummary ToDto(this DB.PricingChartSummary from)
        {
            return from.ConvertTo<ENT.PricingChartSummary>();
        }

    }
}
