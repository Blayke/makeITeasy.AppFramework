﻿using AutoMapper;
using makeITeasy.AppFramework.Core.Infrastructure.Persistence;
using makeITeasy.AppFramework.Models;
using Microsoft.EntityFrameworkCore;

namespace makeITeasy.CarCatalog.Infrastructure.Persistence
{
    public class EfRepository<T> : BaseEfRepository<T, DbContext> where T : class, IBaseEntity
    {
        public EfRepository(DbContext dbContext, IMapper mapper) : base(dbContext, mapper)
        {
        }
    }
}
