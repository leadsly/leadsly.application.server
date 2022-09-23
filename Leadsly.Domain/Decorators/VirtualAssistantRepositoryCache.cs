using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Decorators
{
    public class VirtualAssistantRepositoryCache : IVirtualAssistantRepository
    {
        public VirtualAssistantRepositoryCache(
            ILogger<VirtualAssistantRepositoryCache> logger,
            IMemoryCache memoryCache,
            IVirtualAssistantRepository repository)
        {
            _logger = logger;
            _memoryCache = memoryCache;
            _repository = repository;
        }

        private readonly ILogger<VirtualAssistantRepositoryCache> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly IVirtualAssistantRepository _repository;

        public const string CacheToken_GetAll = $"{nameof(GetAllByUserIdAsync)}";
        public const string CacheToken_Get = "Get";

        public async Task<VirtualAssistant> CreateAsync(VirtualAssistant newVirtualAssistant, CancellationToken ct = default)
        {
            return await _repository.CreateAsync(newVirtualAssistant, ct);
        }

        public async Task<bool> DeleteAsync(string virtualAssistantId, CancellationToken ct = default)
        {
            return await _repository.DeleteAsync(virtualAssistantId, ct);
        }

        public async Task<IList<VirtualAssistant>> GetAllByUserIdAsync(string userId, CancellationToken ct = default)
        {
            string cacheToken = $"{CacheToken_GetAll}-{userId}";
            if (_memoryCache.TryGetValue(cacheToken, out IList<VirtualAssistant> virtualAssistants) == false)
            {
                virtualAssistants = await _repository.GetAllByUserIdAsync(userId, ct);
                _memoryCache.Set(cacheToken, virtualAssistants, TimeSpan.FromMinutes(3));
            }

            return virtualAssistants;
        }

        public async Task<VirtualAssistant> GetByHalIdAsync(string halId, CancellationToken ct = default)
        {
            string cacheToken = $"{CacheToken_Get}-{halId}";
            if (_memoryCache.TryGetValue(cacheToken, out VirtualAssistant virtualAssistant) == false)
            {
                virtualAssistant = await _repository.GetByHalIdAsync(halId, ct);
                _memoryCache.Set(cacheToken, virtualAssistant, TimeSpan.FromMinutes(3));
            }

            return virtualAssistant;
        }

        public async Task<VirtualAssistant> UpdateAsync(VirtualAssistant updated, CancellationToken ct = default)
        {
            updated = await _repository.UpdateAsync(updated, ct);
            if (updated != null)
            {
                string halId = updated.HalId;
                string cacheToken = $"{CacheToken_Get}-{halId}";
                if (_memoryCache.TryGetValue(cacheToken, out VirtualAssistant virtualAssistant) == true)
                {
                    _memoryCache.Set(cacheToken, updated, TimeSpan.FromMinutes(3));
                }
            }

            return updated;
        }
    }
}
