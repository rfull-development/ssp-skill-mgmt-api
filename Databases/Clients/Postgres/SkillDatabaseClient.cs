// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
using Dapper;
using Npgsql;
using SkillManagementApi.Databases.Exceptions;
using SkillManagementApi.Databases.Extensions;
using SkillManagementApi.Databases.Interfaces;
using SkillManagementApi.Databases.Models;
using System.Text;

namespace SkillManagementApi.Databases.Clients.Postgres
{
    public class SkillDatabaseClient(string connectionString, string schema) : DatabaseClient(schema), ISkillDatabaseClient, IDisposable
    {
        private readonly NpgsqlConnection _connection = new(connectionString);
        private readonly string _schema = schema;

        static SkillDatabaseClient()
        {
            SqlMapper.SetTypeMap(typeof(Alias), new CustomPropertyTypeMap(typeof(Alias), CustomMap));
            SqlMapper.SetTypeMap(typeof(AliasListItem), new CustomPropertyTypeMap(typeof(AliasListItem), CustomMap));
            SqlMapper.SetTypeMap(typeof(AliasSegment), new CustomPropertyTypeMap(typeof(AliasSegment), CustomMap));
            SqlMapper.SetTypeMap(typeof(Item), new CustomPropertyTypeMap(typeof(Item), CustomMap));
            SqlMapper.SetTypeMap(typeof(Link), new CustomPropertyTypeMap(typeof(Link), CustomMap));
            SqlMapper.SetTypeMap(typeof(LinkSegment), new CustomPropertyTypeMap(typeof(LinkSegment), CustomMap));
            SqlMapper.SetTypeMap(typeof(ListItem), new CustomPropertyTypeMap(typeof(ListItem), CustomMap));
            SqlMapper.SetTypeMap(typeof(TagAlloc), new CustomPropertyTypeMap(typeof(TagAlloc), CustomMap));
            SqlMapper.SetTypeMap(typeof(TagAllocListItem), new CustomPropertyTypeMap(typeof(TagAllocListItem), CustomMap));
            SqlMapper.SetTypeMap(typeof(TagAllocSegment), new CustomPropertyTypeMap(typeof(TagAllocSegment), CustomMap));
            SqlMapper.SetTypeMap(typeof(Tag), new CustomPropertyTypeMap(typeof(Tag), CustomMap));

        }

        public async Task<Guid> CreateAsync(string name)
        {
            await _connection.OpenIfClosedAsync();
            using var transaction = await _connection.BeginTransactionAsync();

            Guid guid;
            try
            {
                string table = GetTableName<Item>();
                string query = $"""
                INSERT INTO
                    {table} ("name")
                VALUES
                    (@Name)
                ON CONFLICT DO NOTHING
                RETURNING
                    "guid";
                """;
                DynamicParameters parameters = new();
                parameters.Add("Name", name);
                guid = await _connection.ExecuteScalarAsync<Guid?>(query, parameters, transaction: transaction) ?? Guid.Empty;
                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw new DatabaseException("Failed to create item.", e);
            }
            if (guid == Guid.Empty)
            {
                throw new DatabaseConflictException();
            }
            return guid;
        }

        public async Task<Item?> GetAsync(Guid guid)
        {
            await _connection.OpenIfClosedAsync();

            Item? item;
            try
            {
                string table = GetTableName<Item>();
                string columns = GenerateColumnListQuery<Item>();
                string query = $"""
                SELECT
                    {columns}
                FROM
                    {table}
                WHERE
                    "guid" = @Guid;
                """;
                DynamicParameters parameters = new();
                parameters.Add("Guid", guid);
                item = await _connection.QueryFirstOrDefaultAsync<Item>(query, parameters);
            }
            catch (Exception e)
            {
                throw new DatabaseException("Failed to get item.", e);
            }
            return item;
        }

        public async Task<int> SetAsync(Item item)
        {
            await _connection.OpenIfClosedAsync();
            using var transaction = await _connection.BeginTransactionAsync();

            int rows;
            try
            {
                string table = GetTableName<Item>();
                string updateSet = GenerateUpdateSetListQuery(item, [
                    nameof(Item.Id),
                    nameof(Item.Guid),
                    nameof(Item.Version)
                    ]);
                string query = $"""
                UPDATE {table}
                SET
                    {updateSet},
                    "version" = "version" + 1
                WHERE
                    "guid" = @Guid
                    AND "version" = @Version;
                """;
                DynamicParameters parameters = new();
                parameters.AddDynamicParams(item);
                rows = await _connection.ExecuteAsync(query, item, transaction: transaction);
                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw new DatabaseException("Failed to set item.", e);
            }
            if (rows < 1)
            {
                throw new DatabaseConflictException();
            }
            return rows;
        }

        public async Task<int> DeleteAsync(Guid guid)
        {
            await _connection.OpenIfClosedAsync();
            var transaction = await _connection.BeginTransactionAsync();

            int rows;
            try
            {
                string table = GetTableName<Item>();
                string query = $"""
                DELETE FROM {table}
                WHERE
                    "guid" = @Guid;
                """;
                DynamicParameters parameters = new();
                parameters.Add("Guid", guid);
                rows = await _connection.ExecuteAsync(query, parameters, transaction: transaction);
                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw new DatabaseException("Failed to delete item.", e);
            }
            return rows;
        }

        public async Task<List<ListItem>> GetListAsync(Guid guid, int limit)
        {
            limit = Math.Min(Math.Max(limit, 1), 128);

            await _connection.OpenIfClosedAsync();

            List<ListItem> items;
            try
            {
                string cte = GetTableName<Item>();
                string table = GetTableName<ListItem>();
                string columns = GenerateColumnListQuery<ListItem>();
                string condition;
                if (guid != Guid.Empty)
                {
                    condition = """
                        "id" >= (
                            SELECT
                                "id"
                            FROM
                                id_cte
                        )
                    """;
                }
                else
                {
                    condition = "TRUE";
                }
                string query = $"""
                WITH
                    id_cte AS (
                        SELECT
                            "id"
                        FROM
                            {cte}
                        WHERE
                            "guid" = @Guid
                    )
                SELECT
                    {columns}
                FROM
                    {table}
                WHERE
                    {condition}
                LIMIT
                    @Limit;
                """;
                DynamicParameters parameters = new();
                parameters.Add("Guid", guid);
                parameters.Add("Limit", limit);
                IEnumerable<ListItem> results = await _connection.QueryAsync<ListItem>(query, parameters);
                items = results.ToList();
            }
            catch (Exception e)
            {
                throw new DatabaseException("Failed to get skills.", e);
            }
            return items;
        }

        public async Task<List<Item>> GetListAsync(List<Guid> guids)
        {
            await _connection.OpenIfClosedAsync();

            List<Item> items;
            try
            {
                string table = GetTableName<Item>();
                string columns = GenerateColumnListQuery<Item>();
                StringBuilder builder = new();
                for (int index = 0; index < guids.Count; index++)
                {
                    builder.Append($"@Guid_{index},");
                }
                string values = builder.ToString().TrimEnd(',');
                string query = $"""
                SELECT
                    {columns}
                FROM
                    {table}
                WHERE
                    "guid" IN ({values});
                """;
                DynamicParameters parameters = new();
                for (int index = 0; index < guids.Count; index++)
                {
                    parameters.Add($"Guid_{index}", guids[index]);
                }
                IEnumerable<Item> results = await _connection.QueryAsync<Item>(query, parameters);
                items = results.ToList();
            }
            catch (Exception e)
            {
                throw new DatabaseException("Failed to get items.", e);
            }
            return items;
        }

        public async Task<long> GetTotalCount()
        {
            await _connection.OpenIfClosedAsync();

            long count;
            try
            {
                string query = $"""
                SELECT
                    n_live_tup
                FROM
                    pg_catalog.pg_stat_user_tables
                WHERE
                    relname = 'item';
                """;
                count = await _connection.ExecuteScalarAsync<long?>(query) ?? 0;
            }
            catch (Exception e)
            {
                throw new DatabaseException("Failed to get total count.", e);
            }
            return count;
        }

        public async Task<List<Tag>> GetTagListAsync(long itemId)
        {
            await _connection.OpenIfClosedAsync();

            List<Tag> tags;
            try
            {
                string table = GetTableName<TagAllocListItem>();
                string columns = GenerateColumnListQuery<Tag>([
                    nameof(Tag.Version)
                    ]);
                string query = $"""
                SELECT
                    {columns}
                FROM
                    {table}
                WHERE
                    "item_id" = @ItemId
                ORDER BY
                    "id" ASC;
                """;
                DynamicParameters parameters = new();
                parameters.Add("ItemId", itemId);
                IEnumerable<Tag> results = await _connection.QueryAsync<Tag>(query, parameters);
                tags = results.ToList();
            }
            catch (Exception e)
            {
                throw new DatabaseException("Failed to get tags.", e);
            }
            return tags;
        }

        public async Task<int> SetTagList(long itemId, List<long> tagIds)
        {
            await _connection.OpenIfClosedAsync();
            using var transaction = await _connection.BeginTransactionAsync();

            TagAllocSegment? segment;
            try
            {
                segment = await CreateTagAllocSegmentIfNotExistsAsync(itemId, transaction);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw new DatabaseException("Failed to get tag alloc segment.", e);
            }

            try
            {
                await DeleteTagAllocListAsync(itemId, transaction);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw new DatabaseException("Failed to delete old tags.", e);
            }

            int rows;
            if (tagIds.Count > 0)
            {
                try
                {
                    string table = GetTableName<TagAlloc>();
                    string segmentTable = GetTableName<TagAllocSegment>();
                    string columns = GenerateColumnListQuery<TagAlloc>();
                    StringBuilder builder = new();
                    for (int index = 0; index < tagIds.Count; index++)
                    {
                        builder.Append($"(@ItemId,@TagId_{index}),");
                    }
                    string values = builder.ToString().TrimEnd(',');
                    string query = $"""
                    INSERT INTO
                        {table} ({columns})
                    SELECT
                        ta.item_id,
                        ta.tag_id
                    FROM
                        (
                            VALUES
                                {values}
                        ) AS ta (item_id, tag_id)
                        LEFT JOIN {segmentTable} AS tas ON ta.item_id = tas.item_id
                    WHERE
                        tas.version = @Version;
                    """;
                    DynamicParameters parameters = new();
                    parameters.AddDynamicParams(segment);
                    for (int index = 0; index < tagIds.Count; index++)
                    {
                        parameters.Add($"TagId_{index}", tagIds[index]);
                    }
                    rows = await _connection.ExecuteAsync(query, parameters, transaction: transaction);
                    if (rows < 1)
                    {
                        throw new DatabaseConflictException();
                    }
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync();
                    throw new DatabaseException("Failed to set tags.", e);
                }
            }
            else
            {
                rows = 0;
            }

            try
            {
                int segmentRows = await UpdateTagAllocSegment(segment, transaction);
                if (segmentRows < 1)
                {
                    throw new DatabaseConflictException();
                }

                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw new DatabaseException("Failed to update tag alloc segment.", e);
            }
            return rows;
        }

        public async Task<int> DeleteTagListAsync(long itemId)
        {
            await _connection.OpenIfClosedAsync();
            using var transaction = await _connection.BeginTransactionAsync();

            TagAllocSegment? segment;
            try
            {
                segment = await GetTagAllocSegmentAsync(itemId, transaction);
                if (segment == null)
                {
                    return 0;
                }
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw new DatabaseException("Failed to get tag alloc segment.", e);
            }

            int rows;
            try
            {
                rows = await DeleteTagAllocListAsync(itemId, transaction);
                if (rows < 1)
                {
                    throw new DatabaseConflictException();
                }
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw new DatabaseException("Failed to delete tag allocs.", e);
            }

            try
            {
                int segmentRows = await UpdateTagAllocSegment(segment, transaction);
                if (segmentRows < 1)
                {
                    throw new DatabaseConflictException();
                }

                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw new DatabaseException("Failed to update tag alloc segment.", e);
            }
            return rows;
        }

        public async Task<List<Item>> GetAliasListAsync(long itemId)
        {
            await _connection.OpenIfClosedAsync();

            List<Item> items;
            try
            {
                string table = GetTableName<AliasListItem>();
                string query = $"""
                SELECT
                    "guid",
                    "name"
                FROM
                    {table}
                WHERE
                    "item_id" = @ItemId;
                """;
                DynamicParameters parameters = new();
                parameters.Add("ItemId", itemId);
                IEnumerable<Item> results = await _connection.QueryAsync<Item>(query, parameters);
                items = results.ToList();
            }
            catch (Exception e)
            {
                throw new DatabaseException("Failed to get aliases.", e);
            }
            return items;
        }

        public async Task<int> SetAliasListAsync(long itemId, List<Item> items)
        {
            await _connection.OpenIfClosedAsync();
            using var transaction = await _connection.BeginTransactionAsync();

            AliasSegment? segment;
            try
            {
                segment = await CreateAliasSegmentIfNotExistsAsync(itemId, transaction);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw new DatabaseException("Failed to get alias segment.", e);
            }

            try
            {
                await DeleteAliasListAsync(itemId, transaction);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw new DatabaseException("Failed to delete old aliases.", e);
            }

            int rows;
            if (items.Count > 0)
            {
                try
                {
                    string table = GetTableName<Alias>();
                    string segmentTable = GetTableName<AliasSegment>();
                    string columns = GenerateColumnListQuery<Alias>();
                    StringBuilder builder = new();
                    for (int index = 0; index < items.Count; index++)
                    {
                        builder.Append($"(@ItemId,@AliasItemId_{index}),");
                    }
                    string values = builder.ToString().TrimEnd(',');
                    string query = $"""
                    INSERT INTO
                        {table} ({columns})
                    SELECT
                        a.item_id,
                        a.alias_item_id
                    FROM
                        (
                            VALUES
                            {values}
                        ) AS a (item_id, alias_item_id)
                    LEFT JOIN {segmentTable} AS asg ON a.item_id = asg.item_id
                    WHERE
                        asg.version = @Version;
                    """;

                    DynamicParameters parameters = new();
                    parameters.AddDynamicParams(segment);
                    for (int index = 0; index < items.Count; index++)
                    {
                        var item = items[index];
                        if (item?.Id is not long)
                        {
                            throw new DatabaseParameterException();
                        }
                        parameters.Add($"AliasItemId_{index}", item.Id);
                    }
                    rows = await _connection.ExecuteAsync(query, parameters, transaction: transaction);
                    if (rows < 1)
                    {
                        throw new DatabaseConflictException();
                    }
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync();
                    throw new DatabaseException("Failed to set aliases.", e);
                }
            }
            else
            {
                rows = 0;
            }

            try
            {
                int segmentRows = await UpdateAliasSegment(segment, transaction);
                if (segmentRows < 1)
                {
                    throw new DatabaseConflictException();
                }

                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw new DatabaseException("Failed to update alias segment.", e);
            }
            return rows;
        }

        public async Task<int> DeleteAliasListAsync(long itemId)
        {
            await _connection.OpenIfClosedAsync();
            using var transaction = await _connection.BeginTransactionAsync();

            AliasSegment? segment;
            try
            {
                segment = await GetAliasSegmentAsync(itemId, transaction);
                if (segment == null)
                {
                    return 0;
                }
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw new DatabaseException("Failed to get alias segment.", e);
            }

            int rows;
            try
            {
                rows = await DeleteAliasListAsync(itemId, transaction);
                if (rows < 1)
                {
                    throw new DatabaseConflictException();
                }
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw new DatabaseException("Failed to delete aliases.", e);
            }

            try
            {
                int segmentRows = await UpdateAliasSegment(segment, transaction);
                if (segmentRows < 1)
                {
                    throw new DatabaseConflictException();
                }

                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw new DatabaseException("Failed to update alias segment.", e);
            }
            return rows;
        }

        public async Task<List<Link>> GetLinkListAsync(long itemId)
        {
            await _connection.OpenIfClosedAsync();

            List<Link> links;
            try
            {
                string table = GetTableName<Link>();
                string columns = GenerateColumnListQuery<Link>();
                string query = $"""
                SELECT
                    {columns}
                FROM
                    {table}
                WHERE
                    "item_id" = @ItemId;
                """;
                DynamicParameters parameters = new();
                parameters.Add("ItemId", itemId);
                IEnumerable<Link> results = await _connection.QueryAsync<Link>(query, parameters);
                links = results.ToList();
            }
            catch (Exception e)
            {
                throw new DatabaseException("Failed to get links.", e);
            }
            return links;
        }

        public async Task<int> SetLinkListAsync(long itemId, List<Link> links)
        {
            await _connection.OpenIfClosedAsync();
            using var transaction = await _connection.BeginTransactionAsync();

            LinkSegment? segment;
            try
            {
                segment = await CreateLinkSegmentIfNotExistsAsync(itemId, transaction);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw new DatabaseException("Failed to get link segment.", e);
            }

            try
            {
                await DeleteLinkListAsync(itemId, transaction);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw new DatabaseException("Failed to delete old links.", e);
            }

            int rows;
            if (links.Count > 0)
            {
                try
                {
                    string table = GetTableName<Link>();
                    string segmentTable = GetTableName<LinkSegment>();
                    string columns = GenerateColumnListQuery<Link>();
                    StringBuilder builder = new();
                    for (int index = 0; index < links.Count; index++)
                    {
                        builder.Append($"(@ItemId,@Title_{index},@Url_{index}),");
                    }
                    string values = builder.ToString().TrimEnd(',');
                    string query = $"""
                    INSERT INTO
                        {table} ({columns})
                    SELECT
                        l.item_id,
                        l.title,
                        l.url
                    FROM
                        (
                            VALUES
                                {values}
                        ) AS l (item_id, title, url)
                        LEFT JOIN {segmentTable} AS ls ON l.item_id = ls.item_id
                    WHERE
                        ls.version = @Version;
                    """;
                    DynamicParameters parameters = new();
                    parameters.AddDynamicParams(segment);
                    for (int index = 0; index < links.Count; index++)
                    {
                        var link = links[index];
                        if ((string.IsNullOrEmpty(link?.Title)) ||
                            (string.IsNullOrEmpty(link?.Url)))
                        {
                            throw new DatabaseParameterException();
                        }
                        parameters.Add($"Title_{index}", link.Title);
                        parameters.Add($"Url_{index}", link.Url);
                    }
                    rows = await _connection.ExecuteAsync(query, parameters, transaction: transaction);
                    if (rows < 1)
                    {
                        throw new DatabaseConflictException();
                    }
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync();
                    throw new DatabaseException("Failed to set links.", e);
                }
            }
            else
            {
                rows = 0;
            }

            try
            {
                int segmentRows = await UpdateLinkSegment(segment, transaction);
                if (segmentRows < 1)
                {
                    throw new DatabaseConflictException();
                }

                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw new DatabaseException("Failed to update link segment.", e);
            }
            return rows;
        }

        public async Task<int> DeleteLinkListAsync(long itemId)
        {
            await _connection.OpenIfClosedAsync();
            using var transaction = await _connection.BeginTransactionAsync();

            LinkSegment? segment;
            try
            {
                segment = await GetLinkSegmentAsync(itemId, transaction);
                if (segment == null)
                {
                    return 0;
                }
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw new DatabaseException("Failed to get link segment.", e);
            }

            int rows;
            try
            {
                rows = await DeleteLinkListAsync(itemId, transaction);
                if (rows < 1)
                {
                    throw new DatabaseConflictException();
                }
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw new DatabaseException("Failed to delete links.", e);
            }

            try
            {
                int segmentRows = await UpdateLinkSegment(segment, transaction);
                if (segmentRows < 1)
                {
                    throw new DatabaseConflictException();
                }

                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw new DatabaseException("Failed to update link segment.", e);
            }
            return rows;
        }

        private async Task<int> DeleteTagAllocListAsync(long itemId, NpgsqlTransaction transaction)
        {
            int rows;
            try
            {
                string table = GetTableName<TagAlloc>();
                string query = $"""
                DELETE FROM {table}
                WHERE
                    "item_id" = @ItemId;
                """;
                DynamicParameters parameters = new();
                parameters.Add("ItemId", itemId);
                rows = await _connection.ExecuteAsync(query, parameters, transaction: transaction);
            }
            catch (Exception e)
            {
                throw new DatabaseException("Failed to delete tag allocs.", e);
            }
            return rows;
        }

        private async Task<TagAllocSegment> CreateTagAllocSegmentIfNotExistsAsync(long itemId, NpgsqlTransaction transaction)
        {
            TagAllocSegment segment;
            try
            {
                string table = GetTableName<TagAllocSegment>();
                string query = $"""
                INSERT INTO
                    {table} ("item_id")
                VALUES
                    (@ItemId)
                ON CONFLICT ("item_id") DO
                UPDATE
                SET
                    item_id = @ItemId
                RETURNING
                    "version";
                """;
                DynamicParameters parameters = new();
                parameters.Add("ItemId", itemId);
                int version = await _connection.ExecuteScalarAsync<int?>(query, parameters, transaction: transaction) ?? throw new DatabaseException();
                segment = new TagAllocSegment
                {
                    ItemId = itemId,
                    Version = version,
                };
            }
            catch (Exception e)
            {
                throw new DatabaseException("Failed to create tag alloc segment.", e);
            }
            return segment;
        }

        private async Task<TagAllocSegment?> GetTagAllocSegmentAsync(long itemId, NpgsqlTransaction transaction)
        {
            await _connection.OpenIfClosedAsync();

            TagAllocSegment? segment;
            try
            {
                string table = GetTableName<TagAllocSegment>();
                string columns = GenerateColumnListQuery<TagAllocSegment>();
                string query = $"""
                SELECT
                    {columns}
                FROM
                    {table}
                WHERE
                    "item_id" = @ItemId;
                """;
                DynamicParameters parameters = new();
                parameters.Add("ItemId", itemId);
                segment = await _connection.QueryFirstOrDefaultAsync<TagAllocSegment>(query, parameters, transaction: transaction);
            }
            catch (Exception e)
            {
                throw new DatabaseException("Failed to get tag alloc segment.", e);
            }
            return segment;
        }

        private async Task<int> UpdateTagAllocSegment(TagAllocSegment segment, NpgsqlTransaction transaction)
        {
            int rows;
            try
            {
                string table = GetTableName<TagAllocSegment>();
                string query = $"""
                UPDATE {table}
                SET
                    "version" = "version" + 1
                WHERE
                    "item_id" = @ItemId
                    AND "version" = @Version;
                """;
                DynamicParameters parameters = new();
                parameters.AddDynamicParams(segment);
                rows = await _connection.ExecuteAsync(query, parameters, transaction: transaction);
            }
            catch (Exception e)
            {
                throw new DatabaseException("Failed to update tag alloc segment.", e);
            }
            return rows;
        }

        private async Task<int> DeleteAliasListAsync(long itemId, NpgsqlTransaction transaction)
        {
            int rows;
            try
            {
                string table = GetTableName<Alias>();
                string query = $"""
                DELETE FROM {table}
                WHERE
                    "item_id" = @ItemId;
                """;
                DynamicParameters parameters = new();
                parameters.Add("ItemId", itemId);
                rows = await _connection.ExecuteAsync(query, parameters, transaction: transaction);
            }
            catch (Exception e)
            {
                throw new DatabaseException("Failed to delete aliases.", e);
            }
            return rows;
        }

        private async Task<AliasSegment> CreateAliasSegmentIfNotExistsAsync(long itemId, NpgsqlTransaction transaction)
        {
            AliasSegment segment;
            try
            {
                string table = GetTableName<AliasSegment>();
                string query = $"""
                INSERT INTO
                    {table} ("item_id")
                VALUES
                    (@ItemId)
                ON CONFLICT ("item_id") DO
                UPDATE
                SET
                    item_id = @ItemId
                RETURNING
                    "version";
                """;
                DynamicParameters parameters = new();
                parameters.Add("ItemId", itemId);
                int version = await _connection.ExecuteScalarAsync<int?>(query, parameters, transaction: transaction) ?? throw new DatabaseException();
                segment = new AliasSegment
                {
                    ItemId = itemId,
                    Version = version,
                };
            }
            catch (Exception e)
            {
                throw new DatabaseException("Failed to create alias segment.", e);
            }
            return segment;
        }

        private async Task<AliasSegment?> GetAliasSegmentAsync(long itemId, NpgsqlTransaction transaction)
        {
            AliasSegment? segment;
            try
            {
                string table = GetTableName<AliasSegment>();
                string columns = GenerateColumnListQuery<AliasSegment>();
                string query = $"""
                SELECT
                    {columns}
                FROM
                    {table}
                WHERE
                    "item_id" = @ItemId;
                """;
                DynamicParameters parameters = new();
                parameters.Add("ItemId", itemId);
                segment = await _connection.QueryFirstOrDefaultAsync<AliasSegment>(query, parameters, transaction: transaction);
            }
            catch (Exception e)
            {
                throw new DatabaseException("Failed to get alias segment.", e);
            }
            return segment;
        }

        private async Task<int> UpdateAliasSegment(AliasSegment segment, NpgsqlTransaction transaction)
        {
            int rows;
            try
            {
                string table = GetTableName<AliasSegment>();
                string query = $"""
                UPDATE {table}
                SET
                    "version" = "version" + 1
                WHERE
                    "item_id" = @ItemId
                    AND "version" = @Version;
                """;
                DynamicParameters parameters = new();
                parameters.AddDynamicParams(segment);
                rows = await _connection.ExecuteAsync(query, parameters, transaction: transaction);
            }
            catch (Exception e)
            {
                throw new DatabaseException("Failed to update alias segment.", e);
            }
            return rows;
        }

        private async Task<int> DeleteLinkListAsync(long itemId, NpgsqlTransaction transaction)
        {
            int rows;
            try
            {
                string table = GetTableName<Link>();
                string query = $"""
                DELETE FROM {table}
                WHERE
                    "item_id" = @ItemId;
                """;
                DynamicParameters parameters = new();
                parameters.Add("ItemId", itemId);
                rows = await _connection.ExecuteAsync(query, parameters, transaction: transaction);
            }
            catch (Exception e)
            {
                throw new DatabaseException("Failed to delete links.", e);
            }
            return rows;
        }

        private async Task<LinkSegment> CreateLinkSegmentIfNotExistsAsync(long itemId, NpgsqlTransaction transaction)
        {
            LinkSegment segment;
            try
            {
                string table = GetTableName<LinkSegment>();
                string query = $"""
                INSERT INTO
                    {table} ("item_id")
                VALUES
                    (@ItemId)
                ON CONFLICT ("item_id") DO
                UPDATE
                SET
                    item_id = @ItemId
                RETURNING
                    "version";
                """;
                DynamicParameters parameters = new();
                parameters.Add("ItemId", itemId);
                int version = await _connection.ExecuteScalarAsync<int?>(query, parameters, transaction: transaction) ?? throw new DatabaseException();
                segment = new LinkSegment
                {
                    ItemId = itemId,
                    Version = version,
                };
            }
            catch (Exception e)
            {
                throw new DatabaseException("Failed to create link segment.", e);
            }
            return segment;
        }

        private async Task<LinkSegment?> GetLinkSegmentAsync(long itemId, NpgsqlTransaction transaction)
        {
            LinkSegment? segment;
            try
            {
                string table = GetTableName<LinkSegment>();
                string columns = GenerateColumnListQuery<LinkSegment>();
                string query = $"""
                SELECT
                    {columns}
                FROM
                    {table}
                WHERE
                    "item_id" = @ItemId;
                """;
                DynamicParameters parameters = new();
                parameters.Add("ItemId", itemId);
                segment = await _connection.QueryFirstOrDefaultAsync<LinkSegment>(query, parameters, transaction: transaction);
            }
            catch (Exception e)
            {
                throw new DatabaseException("Failed to get link segment.", e);
            }
            return segment;
        }

        private async Task<int> UpdateLinkSegment(LinkSegment segment, NpgsqlTransaction transaction)
        {
            int rows;
            try
            {
                string table = GetTableName<LinkSegment>();
                string query = $"""
                UPDATE {table}
                SET
                    "version" = "version" + 1
                WHERE
                    "item_id" = @ItemId
                    AND "version" = @Version;
                """;
                DynamicParameters parameters = new();
                parameters.AddDynamicParams(segment);
                rows = await _connection.ExecuteAsync(query, parameters, transaction: transaction);
            }
            catch (Exception e)
            {
                throw new DatabaseException("Failed to update link segment.", e);
            }
            return rows;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _connection.Dispose();
            }
        }
    }
}
