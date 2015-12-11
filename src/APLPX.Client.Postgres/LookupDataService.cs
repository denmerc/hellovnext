using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using APLPX.Client.Contracts;
using APLPX.Entity;
using ServiceStack.OrmLite;
using DB = APLPX.Client.Postgres.Models;
using ENT = APLPX.Entity;



namespace APLPX.Client.Postgres
{
    public class LookupDataService : ILookupDataService
    {
        public OrmLiteConnectionFactory DBConnectionFactory { get; private set; }

        public LookupDataService()
        {
            DBConnectionFactory = new OrmLiteConnectionFactory(
                ConfigurationManager.AppSettings["localConnectionString"],
                    PostgreSqlDialect.Provider
                );
            OrmLiteConfig.DialectProvider.NamingStrategy = new OrmLiteNamingStrategyBase();
        }

        public List<ENT.FilterGroup> LoadFilters()
        {
            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                var filters = db.LoadSelect(db.From<DB.FilterGroup>());

                var filterGroupDtos = filters.Select(fg => new ENT.FilterGroup
                {
                    Name = fg.Name,
                    FilterType = fg.FilterTypeId,
                    Filters = fg.Filters.Select(igf => new ENT.Filter
                    {
                        FilterType = igf.FilterTypeId,
                        Name = igf.ValueDescription,
                        Code = igf.Value
                    }).ToList()
                }).ToList();

                return filterGroupDtos;
            };
        }


        public List<ENT.PriceList> LoadPriceLists(PriceRoutineType type)
        {
            if (type != PriceRoutineType.Everyday && type != PriceRoutineType.Promotion)
            {
                throw new ArgumentException("type must be Everyday or Promotion.");
            }

            string priceListType = type == PriceRoutineType.Everyday ? "N" : "P";

            using (IDbConnection db = DBConnectionFactory.OpenDbConnection())
            {
                List<ENT.PriceList> priceListDtos = db.Select<DB.PriceList>()
                                               .Where(t => t.PriceListType == priceListType)
                                               .Select(dbEntity => new ENT.PriceList
                                               {
                                                   Id = dbEntity.PriceListId,
                                                   PriceListType = dbEntity.PriceListType,
                                                   Code = dbEntity.Code,
                                                   Name = dbEntity.Name,
                                                   Description = dbEntity.Description,
                                                   EffectiveDate = dbEntity.EffectiveDate,
                                                   EndDate = dbEntity.EndDate,
                                                   Sort = dbEntity.Sort
                                               }).ToList();
                return priceListDtos;
            };

        }
    }
}
