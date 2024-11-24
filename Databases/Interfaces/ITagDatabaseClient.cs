// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
using SkillManagementApi.Databases.Models;

namespace SkillManagementApi.Databases.Interfaces
{
    public interface ITagDatabaseClient : IDisposable
    {
        public Task<long> CreateAsync(string name);
        public Task<Tag?> GetAsync(long id);
        public Task<int> SetAsync(Tag tag);
        public Task<int> DeleteAsync(long id);
        public Task<List<TagListItem>> GetListAsync(long id, int limit);
        public Task<long> GetTotalCount();
    }
}
