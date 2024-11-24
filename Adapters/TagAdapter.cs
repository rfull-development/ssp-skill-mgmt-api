// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
using SkillManagementApi.Databases.Interfaces;
using SkillManagementApi.Models;

namespace SkillManagementApi.Adapters
{
    public class TagAdapter(ITagDatabaseClient client)
    {
        private readonly ITagDatabaseClient _client = client;

        public async Task<string> CreateAsync(string name)
        {
            long dbId = await _client.CreateAsync(name);
            string id = dbId.ToString();
            return id;
        }

        public async Task<Tag?> GetAsync(string id)
        {
            if (!long.TryParse(id, out long dbId))
            {
                throw new ArgumentException(null, nameof(id));
            }

            var dbTag = await _client.GetAsync(dbId);
            if (dbTag is null)
            {
                return null;
            }
            if (dbTag.Id is not long ||
                string.IsNullOrEmpty(dbTag.Name))
            {
                throw new InvalidDataException();
            }

            Tag tag = new()
            {
                Id = id,
                Name = dbTag.Name
            };
            return tag;
        }

        public async Task<bool> SetAsync(Tag tag)
        {
            ArgumentNullException.ThrowIfNull(tag);
            if (!long.TryParse(tag.Id, out long dbId))
            {
                throw new ArgumentNullException(tag.Id);
            }

            var dbTag = await _client.GetAsync(dbId);
            if (dbTag is null)
            {
                return false;
            }
            dbTag.Name = tag.Name;

            int rows = await _client.SetAsync(dbTag);
            bool success = rows > 0;
            return success;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            if (!long.TryParse(id, out long dbId))
            {
                throw new ArgumentException(null, nameof(id));
            }

            int rows = await _client.DeleteAsync(dbId);
            bool success = rows > 0;
            return success;
        }

        public async Task<List<Tag>> GetListAsync(string? id, int limit)
        {
            long dbId;
            if (id is not null)
            {
                if (!long.TryParse(id, out dbId))
                {
                    throw new ArgumentException(null, nameof(id));
                }
            }
            else
            {
                dbId = default;
            }
            var dbTags = await _client.GetListAsync(dbId, limit);
            List<Tag> tags = [];
            foreach (var dbTag in dbTags)
            {
                if (dbTag.Id is not long dbItemId ||
                    string.IsNullOrEmpty(dbTag.Name))
                {
                    throw new InvalidDataException();
                }

                Tag tag = new()
                {
                    Id = dbItemId.ToString(),
                    Name = dbTag.Name
                };
                tags.Add(tag);
            }
            return tags;
        }

        public async Task<long> GetTotalCountAsync()
        {
            long count = await _client.GetTotalCount();
            return count;
        }
    }
}
