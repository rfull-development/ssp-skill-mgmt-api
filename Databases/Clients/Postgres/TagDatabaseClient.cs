// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
using Dapper;
using Npgsql;
using SkillManagementApi.Databases.Exceptions;
using SkillManagementApi.Databases.Extensions;
using SkillManagementApi.Databases.Interfaces;
using SkillManagementApi.Databases.Models;

namespace SkillManagementApi.Databases.Clients.Postgres
{
    public class TagDatabaseClient(string connectionString, string schema) : DatabaseClient(schema), ITagDatabaseClient, IDisposable
    {
        private readonly NpgsqlConnection _connection = new(connectionString);
        private readonly string _schema = schema;

        static TagDatabaseClient()
        {
            SqlMapper.SetTypeMap(typeof(Tag), new CustomPropertyTypeMap(typeof(Tag), CustomMap));
            SqlMapper.SetTypeMap(typeof(TagListItem), new CustomPropertyTypeMap(typeof(TagListItem), CustomMap));
        }

        public async Task<long> CreateAsync(string name)
        {
            await _connection.OpenIfClosedAsync();
            using var transaction = await _connection.BeginTransactionAsync();

            long id;
            try
            {
                string table = GetTableName<Tag>();
                string query = $"""
                INSERT INTO
                    {table} ("name")
                VALUES
                    (@Name)
                ON CONFLICT DO NOTHING
                RETURNING
                    "id";
                """;
                DynamicParameters parameters = new();
                parameters.Add("@Name", name);
                id = await _connection.ExecuteScalarAsync<long?>(query, parameters, transaction: transaction) ?? default;
                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw new DatabaseException("Failed to create tag.", e);
            }
            if (id == default)
            {
                throw new DatabaseConflictException();
            }
            return id;
        }

        public async Task<Tag?> GetAsync(long id)
        {
            await _connection.OpenIfClosedAsync();

            Tag? tag;
            try
            {
                string table = GetTableName<Tag>();
                string columns = GenerateColumnListQuery<Tag>();
                string query = $"""
                SELECT
                    {columns}
                FROM
                    {table}
                WHERE
                    "id" = @Id;
                """;
                DynamicParameters parameters = new();
                parameters.Add("@Id", id);
                tag = await _connection.QueryFirstOrDefaultAsync<Tag>(query, parameters);
            }
            catch (Exception e)
            {
                throw new DatabaseException("Failed to get tag.", e);
            }
            return tag;
        }

        public async Task<int> SetAsync(Tag tag)
        {
            await _connection.OpenIfClosedAsync();
            using var transaction = await _connection.BeginTransactionAsync();

            int rows;
            try
            {
                string table = GetTableName<Tag>();
                string updateSet = GenerateUpdateSetListQuery(tag, [
                    nameof(Tag.Id),
                    nameof(Tag.Version)
                    ]);
                string query = $"""
                UPDATE {table}
                SET
                    {updateSet},
                    "version" = "version" + 1
                WHERE
                    "id" = @Id
                    AND "version" = @Version;
                """;
                DynamicParameters parameters = new();
                parameters.AddDynamicParams(tag);
                rows = await _connection.ExecuteAsync(query, parameters, transaction: transaction);
                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw new DatabaseException("Failed to set tag.", e);
            }
            if (rows < 1)
            {
                throw new DatabaseConflictException();
            }
            return rows;
        }

        public async Task<int> DeleteAsync(long id)
        {
            await _connection.OpenIfClosedAsync();
            var transaction = await _connection.BeginTransactionAsync();

            int rows;
            try
            {
                string table = GetTableName<Tag>();
                string query = $"""
                DELETE FROM {table}
                WHERE
                    "id" = @Id;
                """;
                DynamicParameters parameters = new();
                parameters.Add("@Id", id);
                rows = await _connection.ExecuteAsync(query, parameters, transaction: transaction);
                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw new DatabaseException("Failed to delete tag.", e);
            }
            return rows;
        }

        public async Task<List<TagListItem>> GetListAsync(long id, int limit)
        {
            limit = Math.Min(Math.Max(limit, 1), 128);

            await _connection.OpenIfClosedAsync();

            List<TagListItem> items;
            try
            {
                string cte = GetTableName<Tag>();
                string table = GetTableName<TagListItem>();
                string columns = GenerateColumnListQuery<TagListItem>();
                string condition;
                if (id != default)
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
                            "id" = @Id
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
                parameters.Add("@Id", id);
                parameters.Add("@Limit", limit);
                IEnumerable<TagListItem> results = await _connection.QueryAsync<TagListItem>(query, parameters);
                items = results.ToList();
            }
            catch (Exception e)
            {
                throw new DatabaseException("Failed to get skill list.", e);
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
                    relname = 'tag';
                """;
                count = await _connection.ExecuteScalarAsync<long?>(query) ?? 0;
            }
            catch (Exception e)
            {
                throw new DatabaseException("Failed to get total count.", e);
            }
            return count;
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
