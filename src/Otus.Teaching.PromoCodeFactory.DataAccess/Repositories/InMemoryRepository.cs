using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Otus.Teaching.PromoCodeFactory.Core.Abstractions.Repositories;
using Otus.Teaching.PromoCodeFactory.Core.Application.Common.Exceptions;
using Otus.Teaching.PromoCodeFactory.Core.Domain;

namespace Otus.Teaching.PromoCodeFactory.DataAccess.Repositories
{
    public class InMemoryRepository<T>
        : IRepository<T>
        where T: BaseEntity
    {
        protected IEnumerable<T> Data { get; set; }

        public InMemoryRepository(IEnumerable<T> data)
        {
            Data = data;
        }
        
        public Task<IEnumerable<T>> GetAllAsync()
        {
            return Task.FromResult(Data);
        }

        public Task<T> GetByIdAsync(Guid id)
        {
            var entity = Data.FirstOrDefault(e => e.Id == id);

            if (entity == null)
                throw new NotFoundException(typeof(T).Name, id);

            return Task.FromResult(entity);
        }

        public Task<Guid> CreateAsync(T entity)
        {
            entity.Id = Guid.NewGuid();
            Data = Data.Concat(new[] { entity });

            return Task.FromResult(entity.Id);
        }

        public Task UpdateAsync(T entity)
        {
            if (Data.FirstOrDefault(e => e.Id == entity.Id) == null)
                throw new NotFoundException(typeof(T).Name, entity.Id);

            Data = Data.Where(e => e.Id != entity.Id);
            Data = Data.Concat(new[] { entity });
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id)
        {
            if (Data.FirstOrDefault(e => e.Id == id) == null)
                throw new NotFoundException(typeof(T).Name, id);

            Data = Data.Where(e => e.Id != id);
            return Task.CompletedTask;
        }
    }
}