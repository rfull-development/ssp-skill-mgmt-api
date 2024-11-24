// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
using SkillManagementApi.Databases.Interfaces;
using SkillManagementApi.Models;

namespace SkillManagementApi.Adapters
{
    public class SkillAdapter(ISkillDatabaseClient client)
    {
        private readonly ISkillDatabaseClient _client = client;

        public async Task<string> CreateAsync(string name)
        {
            Guid dbGuid = await _client.CreateAsync(name);
            string id = dbGuid.ToString();
            return id;
        }

        public async Task<Skill?> GetAsync(string id)
        {
            if (!Guid.TryParse(id, out Guid dbGuid))
            {
                throw new ArgumentException(null, nameof(id));
            }

            var dbItem = await _client.GetAsync(dbGuid);
            if (dbItem is null)
            {
                return null;
            }
            if ((dbItem.Id is not long dbId) ||
                (dbItem.Guid is null) ||
                string.IsNullOrEmpty(dbItem.Name))
            {
                throw new InvalidDataException();
            }

            var tags = await GetTagListAsync(dbId);
            var aliases = await GetAliasListAsync(dbId);
            var links = await GetLinkListAsync(dbId);
            Skill skill = new()
            {
                Id = id,
                Name = dbItem.Name,
                Tags = tags,
                Aliases = aliases,
                Links = links
            };
            return skill;
        }

        public async Task<bool> SetAsync(Skill skill)
        {
            ArgumentNullException.ThrowIfNull(skill);
            if (!Guid.TryParse(skill.Id, out Guid dbGuid))
            {
                throw new ArgumentNullException(skill.Id);
            }

            var dbItem = await _client.GetAsync(dbGuid);
            if (dbItem is null)
            {
                return false;
            }
            dbItem.Name = skill.Name;

            int rows = await _client.SetAsync(dbItem);
            bool success = rows > 0;
            return success;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            if (!Guid.TryParse(id, out Guid dbGuid))
            {
                throw new ArgumentException(null, nameof(id));
            }

            int result = await _client.DeleteAsync(dbGuid);
            bool success = result > 0;
            return success;
        }

        public async Task<List<Skill>> GetListAsync(string? id, int limit)
        {
            Guid dbGuid;
            if (id is not null)
            {
                if (!Guid.TryParse(id, out dbGuid))
                {
                    throw new ArgumentException(null, nameof(id));
                }
            }
            else
            {
                dbGuid = Guid.Empty;
            }

            var dbItems = await _client.GetListAsync(dbGuid, limit);
            List<Skill> skills = [];
            foreach (var dbItem in dbItems)
            {
                if ((dbItem.Id is not long dbItemId) ||
                    (dbItem.Guid is not Guid dbItemGuid) ||
                    string.IsNullOrEmpty(dbItem.Name))
                {
                    throw new InvalidDataException();
                }

                Skill skill = new()
                {
                    Id = dbItemGuid.ToString(),
                    Name = dbItem.Name
                };
                skills.Add(skill);
            }
            return skills;
        }

        public async Task<long> GetTotalCountAsync()
        {
            long count = await _client.GetTotalCount();
            return count;
        }

        public async Task<List<SkillTag>> GetTagListAsync(string id)
        {
            if (!Guid.TryParse(id, out Guid dbGuid))
            {
                throw new ArgumentException(null, nameof(id));
            }

            var dbItem = await _client.GetAsync(dbGuid);
            if ((dbItem is null) ||
                (dbItem.Id is not long dbId))
            {
                return [];
            }

            var tags = await GetTagListAsync(dbId);
            return tags;
        }

        public async Task<bool> SetTagListAsync(string id, List<string> tagIds)
        {
            if (!Guid.TryParse(id, out Guid dbGuid))
            {
                throw new ArgumentException(null, nameof(id));
            }

            var dbItem = await _client.GetAsync(dbGuid);
            if ((dbItem is null) ||
                (dbItem.Id is not long dbId))
            {
                return false;
            }

            List<long> dbTagIds = [];
            foreach (var tagId in tagIds)
            {
                if (!long.TryParse(tagId, out long dbTagId))
                {
                    throw new ArgumentException(nameof(tagId));
                }
                dbTagIds.Add(dbTagId);
            }

            int rows = await _client.SetTagList(dbId, dbTagIds);
            bool success = rows > 0;
            return success;
        }

        public async Task<bool> DeleteTagListAsync(string id)
        {
            if (!Guid.TryParse(id, out Guid dbGuid))
            {
                throw new ArgumentException(null, nameof(id));
            }

            var dbItem = await _client.GetAsync(dbGuid);
            if ((dbItem is null) ||
                (dbItem.Id is not long dbId))
            {
                return false;
            }

            int result = await _client.DeleteTagListAsync(dbId);
            bool success = result > 0;
            return success;
        }

        public async Task<List<string>> GetAliasListAsync(string id)
        {
            if (!Guid.TryParse(id, out Guid dbGuid))
            {
                throw new ArgumentException(null, nameof(id));
            }

            var dbItem = await _client.GetAsync(dbGuid);
            if ((dbItem is null) ||
                (dbItem.Id is not long dbId))
            {
                return [];
            }

            var aliases = await GetAliasListAsync(dbId);
            return aliases;
        }

        public async Task<bool> SetAliasListAsync(string id, List<string> aliases)
        {
            if (!Guid.TryParse(id, out Guid dbGuid))
            {
                throw new ArgumentException(null, nameof(id));
            }

            var dbItem = await _client.GetAsync(dbGuid);
            if ((dbItem is null) ||
                (dbItem.Id is not long dbId))
            {
                return false;
            }

            List<Guid> dbGuids = [];
            foreach (var alias in aliases)
            {
                if (!Guid.TryParse(alias, out Guid dbAlias))
                {
                    throw new ArgumentException(nameof(alias));
                }
                dbGuids.Add(dbAlias);
            }
            List<Databases.Models.Item>? dbItems = await _client.GetListAsync(dbGuids);

            int rows = await _client.SetAliasListAsync(dbId, dbItems);
            bool success = rows > 0;
            return success;
        }

        public async Task<bool> DeleteAliasListAsync(string id)
        {
            if (!Guid.TryParse(id, out Guid dbGuid))
            {
                throw new ArgumentException(null, nameof(id));
            }

            var dbItem = await _client.GetAsync(dbGuid);
            if ((dbItem is null) ||
                (dbItem.Id is not long dbId))
            {
                return false;
            }

            int result = await _client.DeleteAliasListAsync(dbId);
            bool success = result > 0;
            return success;
        }

        public async Task<List<SkillLink>> GetLinkListAsync(string id)
        {
            if (!Guid.TryParse(id, out Guid dbGuid))
            {
                throw new ArgumentException(null, nameof(id));
            }

            var dbItem = await _client.GetAsync(dbGuid);
            if ((dbItem is null) ||
                (dbItem.Id is not long dbId))
            {
                return [];
            }

            var links = await GetLinkListAsync(dbId);
            return links;
        }

        private async Task<List<SkillTag>> GetTagListAsync(long itemId)
        {
            var dbTags = await _client.GetTagListAsync(itemId);
            List<SkillTag> tags = [];
            foreach (var dbTag in dbTags)
            {
                if ((dbTag.Id is not long dbTagId) ||
                    string.IsNullOrEmpty(dbTag.Name))
                {
                    throw new InvalidDataException();
                }

                string id = dbTagId.ToString();
                SkillTag tag = new()
                {
                    Id = id,
                    Name = dbTag.Name
                };
                tags.Add(tag);
            }
            return tags;
        }

        public async Task<bool> SetLinkListAsync(string id, List<SkillLink> links)
        {
            if (!Guid.TryParse(id, out Guid dbGuid))
            {
                throw new ArgumentException(null, nameof(id));
            }

            var dbItem = await _client.GetAsync(dbGuid);
            if ((dbItem is null) ||
                (dbItem.Id is not long dbId))
            {
                return false;
            }

            List<Databases.Models.Link> dbLinks = [];
            foreach (var link in links)
            {
                if ((link is null) ||
                    string.IsNullOrEmpty(link.Title) ||
                    string.IsNullOrEmpty(link.Url))
                {
                    throw new ArgumentException(nameof(link));
                }

                Databases.Models.Link dbLink = new()
                {
                    Title = link.Title,
                    Url = link.Url
                };
                dbLinks.Add(dbLink);
            }

            int rows = await _client.SetLinkListAsync(dbId, dbLinks);
            bool success = rows > 0;
            return success;
        }

        public async Task<bool> DeleteLinkListAsync(string id)
        {
            if (!Guid.TryParse(id, out Guid dbGuid))
            {
                throw new ArgumentException(null, nameof(id));
            }

            var dbItem = await _client.GetAsync(dbGuid);
            if ((dbItem is null) ||
                (dbItem.Id is not long dbId))
            {
                return false;
            }

            int result = await _client.DeleteLinkListAsync(dbId);
            bool success = result > 0;
            return success;
        }

        private async Task<List<string>> GetAliasListAsync(long itemId)
        {
            var dbItems = await _client.GetAliasListAsync(itemId);
            List<string> aliases = [];
            foreach (var dbItem in dbItems)
            {
                if (dbItem.Guid is not Guid guid)
                {
                    throw new InvalidDataException();
                }
                string id = guid.ToString();
                aliases.Add(id);
            }
            return aliases;
        }

        private async Task<List<SkillLink>> GetLinkListAsync(long itemId)
        {
            var dbLinks = await _client.GetLinkListAsync(itemId);
            List<SkillLink> links = [];
            foreach (var dbLink in dbLinks)
            {
                if ((dbLink is null) ||
                    string.IsNullOrEmpty(dbLink.Title) ||
                    string.IsNullOrEmpty(dbLink.Url))
                {
                    throw new InvalidDataException();
                }

                SkillLink link = new()
                {
                    Title = dbLink.Title,
                    Url = dbLink.Url
                };
                links.Add(link);
            }
            return links;
        }
    }
}
