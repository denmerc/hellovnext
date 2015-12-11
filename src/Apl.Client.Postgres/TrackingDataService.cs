using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using APLPX.Client.Contracts;
using Newtonsoft.Json;
using Npgsql;
using ServiceStack;
using ServiceStack.OrmLite;
using DB = APLPX.Client.Postgres.Models;
using DTO = APLPX.Entity;

namespace APLPX.Client.Postgres
{
    /// <summary>
    /// Data access provider for Tracking.
    /// </summary>
    public class TrackingDataService : ITrackingDataService
    {
        private OrmLiteConnectionFactory _dBConnectionFactory;

        public TrackingDataService()
        {
            _dBConnectionFactory = new OrmLiteConnectionFactory(ConfigurationManager.AppSettings["localConnectionString"], PostgreSqlDialect.Provider);

            OrmLiteConfig.DialectProvider.NamingStrategy = new OrmLiteNamingStrategyBase();
            OrmLiteConfig.CommandTimeout = 0;
        }

        public DTO.TrackingSummary GetTrackingSummary(DateTime startDate, DateTime endDate)
        {
            DTO.TrackingSummary result = null;
            using (IDbConnection db = _dBConnectionFactory.OpenDbConnection())
            {
                string sql = String.Format("SELECT \"ImplementedPricing\",\"Series\" FROM \"PX_Track\".\"SP_TrackingGetSummaryData\"('{0:d}', '{1:d}')", startDate.Date, endDate.Date);


                DB.TrackingSummary dbSummary = db.Single<DB.TrackingSummary>(sql);

                if (dbSummary != null)
                {
                    result = new DTO.TrackingSummary();
                    if (dbSummary.Series != null)
                    {
                        foreach (DB.TrackingDataPointCollection dbPointSeries in dbSummary.Series)
                        {
                            DTO.TrackingDataPointSeries dataPointSet = MapToDtoList(dbPointSeries);
                            result.DataPointSets.Add(dataPointSet);
                        }
                    }
                    if (dbSummary.ImplementedPricing != null)
                    {
                        foreach (DB.ImplementedPricing dbPricing in dbSummary.ImplementedPricing)
                        {
                            DTO.ImplementedPricing implementedPricing = MapToDto(dbPricing);
                            result.ImplementedPriceRoutines.Add(implementedPricing);
                        }
                    }
                }
            }

            return result;
        }

        #region Entity-DTO Mapping Helpers

        private DTO.ImplementedPricing MapToDto(DB.ImplementedPricing dbEntity)
        {
            var dto = new DTO.ImplementedPricing();
            dto = dbEntity.ConvertTo<DTO.ImplementedPricing>();
            dto.UpdateTypeId = (DTO.PricingUpdateType)dbEntity.UpdateTypeId;
            return dto;
        }

        private DTO.TrackingDataPointSeries MapToDtoList(DB.TrackingDataPointCollection dbEntity)
        {
            var series = new DTO.TrackingDataPointSeries();
            series.PeriodGranularityId = (DTO.CalculationInterval)dbEntity.PeriodGranularityId;


            if (dbEntity.Points == null) { dbEntity.Points = new DB.TrackingDataPoint[0]; }
            foreach (DB.TrackingDataPoint dbPoint in dbEntity.Points)
            {
                var dto = new DTO.TrackingDataPoint();
                dto.DataPointDate = dbPoint.DataPointDate;
                dto.Label = dbPoint.Label;
                dto.UnitSales = dbPoint.UnitSales;
                dto.Revenue = dbPoint.Revenue;
                dto.Cost = dbPoint.Cost;
                dto.Profit = dbPoint.Profit;
                dto.Markup = dbPoint.Markup;

                series.Points.Add(dto);
            }

            return series;
        }

        //private DTO.PerformanceResult MapToDtoList(DB.PerformanceResult dbEntity)
        //{
        //    var series = new DTO.PerformanceResult();


        //    foreach (DB.PerformanceResult dbResult in dbEntity.)
        //    {
        //        var dto = new DTO.TrackingDataPoint();
        //        dto.DataPointDate = dbPoint.DataPointDate;
        //        dto.Label = dbPoint.Label;
        //        dto.UnitSales = dbPoint.UnitSales;
        //        dto.Revenue = dbPoint.Revenue;
        //        dto.Cost = dbPoint.Cost;
        //        dto.Profit = dbPoint.Profit;
        //        dto.Markup = dbPoint.Markup;

        //        series.Points.Add(dto);
        //    }

        //    return series;
        //}


        #endregion

        #region Mock Database Entity Generator

        private static DB.TrackingSummary GetSampleTrackingSummaryDbEntity()
        {
            var summary = new DB.TrackingSummary();

            //Populate the 4 series of data points (1 per PeriodGranularityId).
            var seriesCollection = new List<DB.TrackingDataPointCollection>();
            for (int i = 1; i <= 4; i++)
            {
                var series = new DB.TrackingDataPointCollection();
                series.PeriodGranularityId = i;
                var points = GetDataPoints(i);
                series.Points = points.ToArray();

                seriesCollection.Add(series);
            }
            summary.Series = seriesCollection.ToArray();

            //Populate the implemented price routines.
            var priceRoutines = GetImplementedPriceRoutines();
            summary.ImplementedPricing = priceRoutines.ToArray();

            return summary;
        }

        private static List<DB.TrackingDataPoint> GetDataPoints(int periodGranularityId)
        {
            var points = new List<DB.TrackingDataPoint>();

            //Simulate variation by PeriodGranularityId.
            var random = new Random(periodGranularityId);
            int dataPointCount = 10;

            for (int i = 0; i < dataPointCount; i++)
            {
                DB.TrackingDataPoint point = new DB.TrackingDataPoint();
                point.DataPointDate = DateTime.Today.AddDays(i * 14);
                point.Label = String.Format("Label {0}", i);
                point.UnitSales = random.Next(1000, 3000);
                point.Revenue = random.Next(1000, 5000);
                point.Cost = point.Revenue * random.Next(500, 1200) / 1000M;
                point.Profit = point.Revenue - point.Cost;
                if (point.Cost != 0)
                {
                    point.Markup = (point.Revenue / point.Cost) - 1;
                }
                points.Add(point);
            }
            return points;
        }

        private static List<DB.ImplementedPricing> GetImplementedPriceRoutines()
        {
            var result = new List<DB.ImplementedPricing>();

            int priceRoutineCount = 10;
            for (int i = 0; i < priceRoutineCount; i++)
            {
                var pricing = new DB.ImplementedPricing();
                pricing.TrackPricingId = 101 + i;
                pricing.Name = String.Format("Sample Price Routine {0}", 101 + i);

                if (i % 3 == 0)
                {
                    pricing.PricingType = "Promotion";
                    pricing.PromotionStart = DateTime.Today.AddDays(i * 14);
                    pricing.PromotionEnd = pricing.PromotionStart.Value.AddDays(21 + i);
                    pricing.ImplementationDate = pricing.PromotionStart.Value.AddDays(-2 - i);
                }
                else
                {
                    pricing.PricingType = "Everyday";
                    pricing.ImplementationDate = DateTime.Today.AddDays(i * 14);
                }

                pricing.AvgPriceIncrease = 12.3M;
                pricing.AvgPriceReduction = 2.1M;
                pricing.HigherPriceCount = 1056;
                pricing.LowerPriceCount = 2321;
                pricing.SKUCount = 11835;
                pricing.UpdateTypeId = 1;

                result.Add(pricing);
            }
            return result;
        }

        #endregion

        public DTO.TrackingPerformance GetTrackingPerformance(long trackingPricingId,
                                                              DateTime baseStartDate, DateTime baseEndDate,
                                                              DateTime trackStartDate, DateTime trackEndDate,
                                                              IEnumerable<int> clusters, IEnumerable<int> priceLists, IEnumerable<int> products,
                                                              int? impactAnalysisId = null)
        {
            DTO.TrackingPerformance result = null;
            using (var db = _dBConnectionFactory.OpenDbConnection())
            {
                string impactId = impactAnalysisId == null ? "null" : impactAnalysisId.Value.ToString();

                object[] values = { trackingPricingId, baseStartDate, baseEndDate, trackStartDate, trackEndDate, 
                                    ConvertListToJsonString(clusters), 
                                    ConvertListToJsonString(priceLists), 
                                    ConvertListToJsonString(products), 
                                    impactId};

                string sql = String.Format("SELECT \"Series\", \"Results\" FROM \"PX_Track\".\"SP_TrackingGetPerformanceData\" ({0}, '{1:d}', '{2:d}', '{3:d}', '{4:d}', {5}, {6}, {7}, {8})", values);

                DB.TrackingPerformance dbEntity = db.Single<DB.TrackingPerformance>(sql);

                result = dbEntity.ToDto();
            }

            return result;
        }

        private string ConvertListToJsonString(IEnumerable<int> list)
        {
            string result = "null";
            if (list != null && list.Count() > 0)
            {
                result = String.Format("'{0}'", JsonConvert.SerializeObject(list));
            }

            return result;
        }

        public List<DTO.TrackingCluster> GetPerformanceClusters(long trackingPricingId, DateTime trackStartDate, DateTime trackEndDate)
        {
            using (var db = _dBConnectionFactory.OpenDbConnection())
            {
                string sql = String.Format("SELECT * FROM \"PX_Track\".\"SP_TrackingGetPerformanceClusters\" ({0}, '{1:d}', '{2:d}')", trackingPricingId, trackStartDate, trackEndDate);
                var dbEntity = db.Select<DB.TrackingCluster>(sql);
                return dbEntity.Select(e => e.ToDto()).ToList();

            }
        }

        public List<DTO.TrackingProduct> GetPerformanceProducts(long trackingPricingId, IEnumerable<int> priceListIds, DateTime trackStartDate, DateTime trackEndDate)
        {
            using (var db = _dBConnectionFactory.OpenDbConnection())
            {
                string priceListIdsJson = ConvertListToJsonString(priceListIds);

                object[] values = { trackingPricingId, priceListIdsJson, trackStartDate, trackEndDate };
                string sql = String.Format("SELECT * FROM \"PX_Track\".\"SP_TrackingGetPerformanceProducts\" ({0}, {1},'{2:d}', '{3:d}')", values);

                var dbEntity = db.Select<DB.TrackingProduct>(sql);
                return dbEntity.Select(e => e.ToDto()).ToList();
            }
        }

        public List<DTO.TrackingPriceList> GetPerformancePriceLists(long trackingPricingId, DateTime trackStartDate, DateTime trackEndDate)
        {
            using (var db = _dBConnectionFactory.OpenDbConnection())
            {
                string sql = String.Format("SELECT * FROM \"PX_Track\".\"SP_TrackingGetPerformancePriceLists\" ({0}, '{1:d}', '{2:d}')", trackingPricingId, trackStartDate, trackEndDate);
                var dbEntity = db.Select<DB.TrackingPriceList>(sql);
                return dbEntity.Select(e => e.ToDto()).ToList();
            }
        }

        public DTO.TrackingTimeFrame GetComparisonSyncTimeFrame(IEnumerable<int> trackPricingIds)
        {
            using (var db = _dBConnectionFactory.OpenDbConnection())
            {
                string sql = String.Format("select \"MinStartDate\", \"MaxEndDate\" FROM \"PX_Track\".\"SP_TrackingGetSyncTimeFrame\" ('{0}')", JsonConvert.SerializeObject(trackPricingIds));
                DB.TrackingTimeFrame dbEntity = db.Single<DB.TrackingTimeFrame>(sql);
                return dbEntity.ConvertTo<DTO.TrackingTimeFrame>();
            }
        }

        public DTO.ImpactAnalysis LoadImpactAnalysis(long impactAnalysisId)
        {
            using (IDbConnection db = _dBConnectionFactory.OpenDbConnection())
            {
                DB.TrackImpactScenario dbRow = db.Single<DB.TrackImpactScenario>(item => item.TrackImpactId == impactAnalysisId);

                var pricing = db.Single<DB.TrackPricing>(p => p.TrackPricingId == dbRow.TrackPricingId);
                if (pricing == null)
                {
                    throw new KeyNotFoundException("Pricing not found.");
                }
                var analytic = db.Single<DB.TrackAnalytic>(a => a.TrackAnalyticId == pricing.TrackAnalyticId);
                if (analytic == null)
                {
                    throw new KeyNotFoundException("Analytic not found.");
                }

                DTO.ImpactAnalysis dto = MapToDto(dbRow, analytic);

                return dto;
            }
        }

        public DTO.AnalyticAggregates GetAnalyticAggregates(int analyticId)
        {
            if (analyticId == 0) throw new ArgumentOutOfRangeException("Invalid parameter : analyticId.");

            using (var db = _dBConnectionFactory.OpenDbConnection())
            {
                var summary = db.Single<DB.AnalyticSummary>(string.Format("SELECT \"SkuCount\", \"TotalSalesValue\" FROM \"PX_Main\".\"SP_AnalyticGetSummary\"({0})", analyticId));
                return new DTO.AnalyticAggregates { SkuCount = summary.SkuCount, TotalSalesValue = summary.TotalSalesValue };
            }
        }

        public List<DTO.PricingResult> LoadResults(int trackPricingId, DTO.PricingResultFilter[] filters, IProgress<String> progress)
        {
            if (trackPricingId == 0) throw new ArgumentOutOfRangeException("Invalid parameter : pricingId.");

            using (var db = _dBConnectionFactory.OpenDbConnection())
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

                var sql = string.Format("SELECT \"ProductId\", \"SKU\", \"ProductName\", \"Cost\", \"ClusterId\", \"PricingResultDetail\" FROM \"PX_Track\".\"SP_TrackingLoadResults\"({0})", trackPricingId);
                var results = db.SqlList<DB.PricingResult>(sql);

                if (results == null) { return new List<DTO.PricingResult>(); }
                return results.Select(r => new DTO.PricingResult
                {
                    SkuId = r.ProductId,
                    SkuCode = r.Sku,
                    SkuDescription = r.ProductName,
                    Cost = r.Cost,
                    ClusterId = r.ClusterId,
                    PricingResultDetail = r.PricingResultDetail != null ? r.PricingResultDetail.Select(prd => new DTO.PricingResultDetail
                    {
                        ProductId = prd.ProductId,
                        PriceListId = prd.PriceListId,
                        PriceListName = prd.PriceListName,
                        CurrentPrice = prd.CurrentPrice,
                        RecommendedPrice = prd.RecommendedPrice,
                        FinalPrice = prd.FinalPrice,
                        PricingResultEditTypeId = (DTO.PricingResultsEditType)prd.PricingResultEditType,
                        Alerts = prd.Alerts != null ? prd.Alerts.Select(a => new DTO.Alert
                        {
                            AlertId = a.AlertId,
                            AlertMessage = a.AlertMessage,
                            Severity = a.Severity
                        }).ToList() : new List<DTO.Alert>()
                    }).ToList() : new List<DTO.PricingResultDetail>()
                }).ToList();
            }

        }

        public DTO.PricingResultValueDriverDetail GetResultDetail(int trackPricingId, int productId)
        {
            using (var db = _dBConnectionFactory.OpenDbConnection())
            {
                var sql = string.Format("SELECT \"PriceListId\", \"PriceDrivers\" FROM \"PX_Track\".\"FN_TrackingGetResultDetail\"({0},{1}) ", trackPricingId, productId);
                var results = db.Single<DB.PricingResultValueDriverDetail>(sql);

                if (results == null) { return new DTO.PricingResultValueDriverDetail(); }
                return new DTO.PricingResultValueDriverDetail()
                {
                    PriceListId = results.PriceListId,
                    DriverDetail = results.PriceDrivers.Select(r => new DTO.PricingValueDriverDetail
                    {
                        DriverId = r.DriverId,
                        DriverName = r.DriverName,
                        IsKey = r.IsKey,
                        PriceChange = r.PriceChange
                    }).ToList()
                };
            }

        }

        public List<DTO.ImpactAnalysis> LoadImpactAnalyses(long trackPricingId)
        {
            using (IDbConnection db = _dBConnectionFactory.OpenDbConnection())
            {
                var dbPricing = db.Single<DB.TrackPricing>(p => p.TrackPricingId == trackPricingId);
                if (dbPricing == null)
                {
                    throw new KeyNotFoundException("Pricing not found.");
                }
                var dbAnalytic = db.Single<DB.TrackAnalytic>(a => a.TrackAnalyticId == dbPricing.TrackAnalyticId);
                if (dbAnalytic == null)
                {
                    throw new KeyNotFoundException("Analytic not found.");
                }

                List<DB.TrackImpactScenario> dbRows = db.Select<DB.TrackImpactScenario>(item => item.TrackPricingId == trackPricingId);
                List<DTO.ImpactAnalysis> dtoList = dbRows.Select(row => MapToDto(row, dbAnalytic)).ToList();

                return dtoList;
            }
        }

        public DTO.Analytic LoadAnalytic(int trackAnalyticId)
        {
            if (trackAnalyticId == 0)
            {
                throw new ArgumentOutOfRangeException("analyticId");
            }

            using (IDbConnection db = _dBConnectionFactory.OpenDbConnection())
            {
                var analytic = db.Single<DB.TrackAnalytic>(a => a.TrackAnalyticId == trackAnalyticId);
                if (analytic == null)
                {
                    throw new KeyNotFoundException("Analytic not found.");
                }

                DTO.Analytic result = null;

                if (analytic.ProductFilters == null)
                {
                    analytic.ProductFilters = new DB.ProductFilterGroup[0];
                }
                if (analytic.PriceLists == null)
                {
                    analytic.PriceLists = new int[0];
                }

                var counts = db.Single<DB.AnalyticSummary>(string.Format("SELECT count (*) SkuCount, Sum(\"ActualSalesAmount\") TotalSalesValue FROM \"PX_Track\".\"TrackAnalyticProduct\" WHERE \"TrackAnalyticId\" = {0}", trackAnalyticId));

                result = new DTO.Analytic
                {
                    Id = (int)analytic.TrackAnalyticId,
                    Name = analytic.Name,
                    Description = analytic.Description,
                    FolderId = analytic.FolderID,
                    Notes = analytic.Notes,
                    AggregationStartDate = analytic.AggStartDate,
                    AggregationEndDate = analytic.AggEndDate,
                    SkuCount = (int)counts.SkuCount,
                    TotalSales = (decimal)counts.TotalSalesValue,
                    Filters = (from p in analytic.ProductFilters
                               group p by p.FilterTypeId into g
                               select new DTO.FilterGroup
                               {
                                   FilterType = g.Key,
                                   Filters = g.SelectMany(h => h.Values)
                                   .Select(fv => new DTO.Filter { Code = fv, FilterType = g.Key }).ToList()
                               }).ToArray(),
                    PriceLists = analytic.PriceLists.ToArray()
                };

                return result;
            }
        }

        public DTO.TrackedPriceRoutine LoadPricing(int trackPricingId)
        {
            if (trackPricingId == 0)
            {
                throw new ArgumentOutOfRangeException("pricingId");
            }

            DTO.TrackedPriceRoutine result = null;

            using (IDbConnection db = _dBConnectionFactory.OpenDbConnection())
            {
                var pricingDbEntity = db.LoadSingleById<DB.TrackPricing>(trackPricingId);
                if (pricingDbEntity == null)
                {
                    throw new KeyNotFoundException("Price Routine not found.");
                }
                var userDbEntity = db.Single<DB.User>(u => u.UserId == pricingDbEntity.OwnerId);
                if (userDbEntity == null)
                {
                    throw new KeyNotFoundException("Owner/User not found.");
                }

                var approverDbEntity = db.Single<DB.User>(u => u.UserId == pricingDbEntity.ApproverId);
                if (approverDbEntity == null)
                {
                    throw new KeyNotFoundException("Owner/User not found.");
                }

                result = ToPricingEverydayDto(pricingDbEntity, userDbEntity, approverDbEntity);

                var counts = db.Single<DTO.PricingSummary>(string.Format("SELECT count(distinct \"ProductId\") \"SkuCount\", count(*) \"PriceCount\" FROM \"PX_Track\".\"TrackPricingRecommendation\" WHERE \"TrackPricingId\"={0}", trackPricingId));

                result.SkuCount = (int)counts.SkuCount;
                result.PriceCount = (int)counts.PriceCount;

                return result;
            }
        }


        public List<DTO.PricingValueDriver> LoadDrivers(long trackPricingId)
        {
            var result = new List<DTO.PricingValueDriver>();

            using (IDbConnection db = _dBConnectionFactory.OpenDbConnection())
            {
                //TEMP load the analytic so we can get at the configured drivers. 
                //TODO: Add DriverMinimum and DriverMaximum retrieval to FN_PricingLoadDrivers so this is not necessary.
                var pricing = db.Single<DB.TrackPricing>(p => p.TrackPricingId == trackPricingId);
                var analytic = db.Single<DB.TrackAnalytic>(a => a.TrackAnalyticId == pricing.TrackAnalyticId);

                string sql = String.Format("SELECT * FROM \"PX_Track\".\"FN_TrackingLoadDrivers\"({0})", trackPricingId);
                var driverDbEntities = db.Select<DB.TrackPricingDriverExtended>(sql);
                if (driverDbEntities != null)
                {
                    foreach (DB.TrackPricingDriverExtended dbEntity in driverDbEntities)
                    {
                        DTO.PricingValueDriver driver = dbEntity.ToDto();
                        result.Add(driver);
                    }
                }
            }
            return result;
        }

        public List<DTO.PricingPriceList> LoadPriceLists(long trackPricingId)
        {
            var priceLists = new List<DTO.PricingPriceList>();

            using (IDbConnection db = _dBConnectionFactory.OpenDbConnection())
            {
                string sql = String.Format("SELECT * FROM \"PX_Track\".\"V_TrackPricingPriceListExtended\" WHERE \"TrackPricingId\"= {0}", trackPricingId);
                var priceListDbEntities = db.SqlList<DB.TrackPricingPriceListExtended>(sql);
                if (priceListDbEntities != null)
                {
                    //priceLists = ToPriceListDtos(priceListDbEntities);
                    priceLists = priceListDbEntities.ConvertAll(ppl => ppl.ToDto());
                }
            }

            return priceLists;
        }

        private DTO.ImpactAnalysis MapToDto(DB.TrackImpactScenario dbRow, DB.TrackAnalytic analytic)
        {
            var dto = new DTO.ImpactAnalysis
            {
                ImpactId = (int)dbRow.TrackImpactId,
                Name = dbRow.Name,
                Notes = dbRow.Notes,
                PricingId = (int)dbRow.TrackPricingId,
                AnalyticStartDate = analytic.AggStartDate,
                AnalyticEndDate = analytic.AggEndDate,
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

        private DTO.ImpactAnalysisResultSet[] MapToDtos(IEnumerable<DB.ImpactAnalysisResultSet> dbResultRows)
        {
            var dtos = dbResultRows.Select(rs => new DTO.ImpactAnalysisResultSet
            {
                PeriodGranularityId = (DTO.CalculationInterval)rs.PeriodGranularityId,
                Items = rs.Items.Select(r => new DTO.ImpactAnalysisResult
                {
                    Name = r.Name,
                    UnitOfMeasureType = (DTO.UnitOfMeasureType)r.UnitOfMeasureId,
                    ActualAmount = r.ActualAmount,
                    ImpactAmount = r.ImpactAmount,
                    ProjectedAmount = r.ProjectedAmount
                }).ToArray()
            });

            return dtos.ToArray();
        }

        /// <summary>
        /// Maps a collection of <see cref="ImpactAnalysisDataPointSet"/> database entities to a collection of <see cref="ImpactAnalysisDataPointSet"/> DTOs.
        /// </summary>
        /// <param name="dbImpactForecastRows"></param>
        /// <returns></returns>
        private DTO.ImpactAnalysisDataPointSet[] MapToDtos(IEnumerable<DB.ImpactAnalysisDataPointSet> dbImpactForecastRows)
        {
            var result = dbImpactForecastRows.Select(rs => new DTO.ImpactAnalysisDataPointSet
            {
                PeriodGranularityId = (DTO.CalculationInterval)rs.PeriodGranularityId,
                Points = rs.Points.Select(dp => new DTO.DataPoint
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

            return result.ToArray();
        }

        private DTO.TrackedPriceRoutine ToPricingEverydayDto(DB.TrackPricing pricingDbEntity, DB.User userDbEntity, DB.User approverDbEntity)
        {
            var dto = pricingDbEntity.ConvertTo<DTO.TrackedPriceRoutine>();

            var filtergroups = (from p in pricingDbEntity.ProductFilters
                                group p by p.FilterTypeId into g
                                select new DTO.FilterGroup
                                {
                                    Name = g.First().Name,
                                    FilterType = g.Key,
                                    Filters = g.SelectMany(h => h.Values)
                                    .Select(fv => new DTO.Filter { Code = fv, FilterType = g.Key }).ToList()
                                }).ToArray();
            dto.Filters = filtergroups.ToList();
            dto.Id = pricingDbEntity.PricingId;
            dto.ValueDrivers = LoadDrivers(pricingDbEntity.TrackPricingId);
            dto.PriceLists = LoadPriceLists(pricingDbEntity.TrackPricingId);
            dto.TransmitDate = pricingDbEntity.TransmitTS;
            dto.PricingMode = (DTO.PricingMode)pricingDbEntity.PricingModeId;
            dto.PricingState = (DTO.PricingState)pricingDbEntity.PricingStateId;
            dto.PriceRoutineType = pricingDbEntity.PricingModeId == 5 ? "Promotion" : "Everyday";
            dto.AnalystName = GetUserFullName(userDbEntity);
            dto.ApproverName = GetUserFullName(approverDbEntity);

            return dto;
        }

        private string GetUserFullName(DB.User userEntity)
        {
            string result = String.Format("{0} {1}", userEntity.FirstName, userEntity.LastName);
            return result;
        }
    }

    public static class ConvertExtensions
    {
        public static DTO.PricingValueDriver ToDto(this DB.TrackPricingDriverExtended from)
        {
            var result = new DTO.PricingValueDriver
            {
                Id = from.DriverId,
                Name = from.Name,
                Description = from.Description,
                DriverType = (DTO.DriverType)from.DriverId,
                UnitOfMeasure = from.UnitOfMeasure,
                ChangeDriverFlag = from.ChangeDriverFlag,
                IsKey = from.IsKey,
                IsSelected = true,
                Groups = ToDriverGroups(from)
            };
            return result;
        }

        public static List<DTO.PricingValueDriverGroup> ToDriverGroups(DB.TrackPricingDriverExtended from)
        {
            var result = new List<DTO.PricingValueDriverGroup>();

            //Create the driver groups based on the optimization rules.
            foreach (DB.PriceOptimizationRule dbRule in from.OptimizationRules)
            {
                var driverGroupDto = new DTO.PricingValueDriverGroup
                {
                    GroupNumber = dbRule.GroupNum,

                    OptimizationRules = dbRule.PriceRanges.Select(pr => new DTO.PriceOptimizationRule
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
            for (int i = 0; i < from.Encoding.Length; i++)
            {
                DB.DriverGroup encoding = from.Encoding[i];

                DTO.PricingValueDriverGroup destGroup = result.Where(item => item.GroupNumber == encoding.GroupNumber).FirstOrDefault();
                if (destGroup != null)
                {
                    destGroup.MinOutlier = encoding.MinOutlier;
                    destGroup.MaxOutlier = encoding.MaxOutlier;
                }
            }
            return result;
        }

        public static DTO.PricingPriceList ToDto(this DB.TrackPricingPriceListExtended from)
        {
            return new DTO.PricingPriceList
            {
                PricingId = from.TrackPricingId,
                PriceListId = from.PriceListId,
                IsKey = from.IsKey,
                MinValue = from.MinValue,
                MaxValue = from.MaxValue,
                PercentChange = from.PercentChange,
                ApplyMarkup = from.ApplyMarkup,
                ApplyRounding = from.ApplyRounding,
                MarkupRules = from.MarkupRules.ConvertAll(mr => mr.ToDto()),
                RoundingRules = from.RoundingRules.ConvertAll(rr => rr.ToDto()),

                Name = from.PriceListName,
                PriceListType = from.PriceListType,
                IsPromotion = (from.PriceListType == "P"),
                Code = from.PriceListCode,
                Description = from.PriceListDescription,
                Sort = from.Sort,
                EffectiveDate = from.EffectiveDate,
                EndDate = from.EndDate
            };


        }

        public static DTO.PriceMarkupRule ToDto(this DB.PriceMarkupRule from)
        {
            return from.ConvertTo<DTO.PriceMarkupRule>();
        }

        public static DTO.PriceRoundingRule ToDto(this DB.PriceRoundingRule from)
        {
            return from.ConvertTo<DTO.PriceRoundingRule>();
        }

        public static DTO.TrackingPerformance ToDto(this DB.TrackingPerformance from)
        {
            var to = from.ConvertTo<DTO.TrackingPerformance>();
            to.Series = from.Series.ToList().ConvertAll(s => s.ToDto());
            to.Results = from.Results.ToList().ConvertAll(r => r.ToDto());
            return to;
        }

        public static DTO.PerformanceResult ToDto(this DB.PerformanceResult from)
        {
            return from.ConvertTo<DTO.PerformanceResult>();
        }

        public static DTO.TrackingDataPointSeries ToDto(this DB.TrackingDataPointCollection from)
        {
            return new DTO.TrackingDataPointSeries
            {
                PeriodGranularityId = (DTO.CalculationInterval)from.PeriodGranularityId,
                Points = from.Points != null ? from.Points.ToList().ConvertAll(p => p.ToDto()) : new List<DTO.TrackingDataPoint>()
            };
        }

        public static DTO.TrackingDataPoint ToDto(this DB.TrackingDataPoint from)
        {
            return from.ConvertTo<DTO.TrackingDataPoint>();
        }


        public static DTO.TrackingCluster ToDto(this DB.TrackingCluster from)
        {
            return new DTO.TrackingCluster
                {
                    ClusterId = from.ClusterId,
                    EncodedSpace = from.EncodedSpace.Select(es => new DTO.DriverGroupCombination
                    {
                        DriverId = es.DriverId,
                        GroupNum = (int)es.DriverValue
                    }).ToList(),
                    Profit = from.Profit,
                    ProfitContributionPerSKU = from.ProfitContributionPerSKU,
                    SkuCount = from.SkuCount
                };
        }

        public static DTO.TrackingProduct ToDto(this DB.TrackingProduct from)
        {
            return from.ConvertTo<DTO.TrackingProduct>();
        }

        public static DTO.TrackingPriceList ToDto(this DB.TrackingPriceList from)
        {
            return from.ConvertTo<DTO.TrackingPriceList>();
        }
    }
}
