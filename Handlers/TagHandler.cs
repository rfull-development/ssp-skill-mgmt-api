// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
using Microsoft.AspNetCore.Mvc;
using SkillManagementApi.Adapters;
using SkillManagementApi.Databases.Clients.Postgres;
using SkillManagementApi.Databases.Exceptions;
using SkillManagementApi.Models;

namespace SkillManagementApi.Handlers
{
    public static class TagHandler
    {
        public static void AddTagHandler(this WebApplication app)
        {
            var tags = app.MapGroup("/tags");
            tags.MapGet("/", GetTagListAsync);
            tags.MapPost("/", CreateTagAsync);
            tags.MapGet("/{id}", GetTagAsync);
            tags.MapPatch("/{id}", UpdateTagAsync);
            tags.MapDelete("/{id}", DeleteTagAsync);
        }

        private static async Task<IResult> GetTagListAsync(
            [FromQuery(Name = "start-id")] string? startId,
            [FromQuery(Name = "limit")] int limit = 20
            )
        {
            List<Tag>? tags;
            long totalCount;
            try
            {
                DatabaseClientFactory databaseClientFactory = new();
                using var databaseClient = databaseClientFactory.CreateTagDatabaseClient();
                TagAdapter adapter = new(databaseClient);
                tags = await adapter.GetListAsync(startId, limit);
                totalCount = await adapter.GetTotalCountAsync();
            }
            catch (ArgumentException)
            {
                return Results.BadRequest();
            }
            catch (DatabaseParameterException)
            {
                return Results.BadRequest();
            }
            TagListGetResponse response = new()
            {
                TotalCount = totalCount,
                Count = tags.Count,
                Tags = tags
            };
            return Results.Ok(response);
        }

        private static async Task<IResult> CreateTagAsync(
            [FromBody] TagCreateRequest request
            )
        {
            string id;
            try
            {
                DatabaseClientFactory databaseClientFactory = new();
                using var databaseClient = databaseClientFactory.CreateTagDatabaseClient();
                TagAdapter adapter = new(databaseClient);
                id = await adapter.CreateAsync(request.Name);
            }
            catch (ArgumentException)
            {
                return Results.BadRequest();
            }
            catch (DatabaseParameterException)
            {
                return Results.BadRequest();
            }
            catch (DatabaseConflictException)
            {
                return Results.Conflict();
            }
            TagCreateResponse response = new()
            {
                Id = id
            };
            return Results.Created($"/tags/{response.Id}", response);
        }

        private static async Task<IResult> GetTagAsync(
            [FromRoute(Name = "id")] string id
            )
        {
            Tag? tag;
            try
            {
                DatabaseClientFactory databaseClientFactory = new();
                using var databaseClient = databaseClientFactory.CreateTagDatabaseClient();
                TagAdapter adapter = new(databaseClient);
                tag = await adapter.GetAsync(id);
            }
            catch (ArgumentException)
            {
                return Results.BadRequest();
            }
            catch (DatabaseParameterException)
            {
                return Results.BadRequest();
            }
            if (tag is null)
            {
                return Results.NotFound();
            }
            TagGetResponse response = new()
            {
                Id = tag.Id,
                Name = tag.Name
            };
            return Results.Ok(response);
        }

        private static async Task<IResult> UpdateTagAsync(
            [FromRoute(Name = "id")] string id,
            [FromBody] TagUpdateRequest request
            )
        {
            bool success;
            try
            {
                DatabaseClientFactory databaseClientFactory = new();
                using var databaseClient = databaseClientFactory.CreateTagDatabaseClient();
                TagAdapter adapter = new(databaseClient);
                Tag user = new()
                {
                    Id = id,
                    Name = request.Name
                };
                success = await adapter.SetAsync(user);
            }
            catch (ArgumentException)
            {
                return Results.BadRequest();
            }
            catch (DatabaseParameterException)
            {
                return Results.BadRequest();
            }
            catch (DatabaseConflictException)
            {
                return Results.Conflict();
            }
            if (!success)
            {
                return Results.NotFound();
            }
            return Results.Accepted();
        }

        private static async Task<IResult> DeleteTagAsync(
            [FromRoute(Name = "id")] string id
            )
        {
            bool success;
            try
            {
                DatabaseClientFactory databaseClientFactory = new();
                using var databaseClient = databaseClientFactory.CreateTagDatabaseClient();
                TagAdapter adapter = new(databaseClient);
                success = await adapter.DeleteAsync(id);
            }
            catch (ArgumentException)
            {
                return Results.BadRequest();
            }
            catch (DatabaseParameterException)
            {
                return Results.BadRequest();
            }
            if (!success)
            {
                return Results.NotFound();
            }
            return Results.Accepted();
        }
    }
}
