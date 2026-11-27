using BasketService.Api.Core.Application.Repository;
using StackExchange.Redis;

namespace BasketService.Api.Infrastructure.Repository
{
    //product ids kept in a redis set per user
    public class RedisWishlistRepository : IWishlistRepository
    {
        private readonly IDatabase _database;

        public RedisWishlistRepository(ConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        private static RedisKey Key(string userName) => $"wishlist:{userName}";

        public async Task<List<int>> GetProductIdsAsync(string userName)
        {
            var members = await _database.SetMembersAsync(Key(userName));

            return members.Select(m => int.TryParse(m.ToString(), out var id) ? id : 0)
                .Where(id => id > 0)
                .OrderBy(id => id)
                .ToList();
        }

        public Task AddAsync(string userName, int productId) => _database.SetAddAsync(Key(userName), productId);

        public Task RemoveAsync(string userName, int productId) => _database.SetRemoveAsync(Key(userName), productId);
    }
}
