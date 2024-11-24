// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
using SkillManagementApi.Databases.Models;

namespace SkillManagementApi.Databases.Interfaces
{
    public interface ISkillDatabaseClient : IDisposable
    {
        public Task<Guid> CreateAsync(string name);
        public Task<Item?> GetAsync(Guid guid);
        public Task<int> SetAsync(Item item);
        public Task<int> DeleteAsync(Guid guid);
        public Task<List<ListItem>> GetListAsync(Guid guid, int limit);
        public Task<List<Item>> GetListAsync(List<Guid> guids);
        public Task<long> GetTotalCount();
        public Task<List<Tag>> GetTagListAsync(long itemId);
        public Task<int> SetTagList(long itemId, List<long> tagIds);
        public Task<int> DeleteTagListAsync(long itemId);
        public Task<List<Item>> GetAliasListAsync(long itemId);
        public Task<int> SetAliasListAsync(long itemId, List<Item> items);
        public Task<int> DeleteAliasListAsync(long itemId);
        public Task<List<Link>> GetLinkListAsync(long itemId);
        public Task<int> SetLinkListAsync(long itemId, List<Link> links);
        public Task<int> DeleteLinkListAsync(long itemId);
    }
}
