// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
using Microsoft.AspNetCore.Mvc;
using SkillManagementApi.Adapters;
using SkillManagementApi.Databases.Clients.Postgres;
using SkillManagementApi.Databases.Exceptions;
using SkillManagementApi.Models;

namespace SkillManagementApi.Handlers
{
    public static class SkillHandler
    {
        public static void AddSkillHandler(this WebApplication app)
        {
            var skills = app.MapGroup("/skills");
            skills.MapGet("/", GetSkillListAsync);
            skills.MapPost("/", CreateSkillAsync);
            skills.MapGet("/{id}", GetSkillAsync);
            skills.MapPatch("/{id}", UpdateSkillAsync);
            skills.MapDelete("/{id}", DeleteSkillAsync);

            var tags = app.MapGroup("/skills/{id}/tags");
            tags.MapGet("/", GetSkillTagListAsync);
            tags.MapPut("/", UpdateSkillTagListAsync);
            tags.MapDelete("/", DeleteSkillTagListAsync);

            var aliases = app.MapGroup("/skills/{id}/aliases");
            aliases.MapGet("/", GetSkillAliasListAsync);
            aliases.MapPut("/", UpdateSkillAliasListAsync);
            aliases.MapDelete("/", DeleteSkillAliasListAsync);

            var links = app.MapGroup("/skills/{id}/links");
            links.MapGet("/", GetSkillLinkListAsync);
            links.MapPut("/", UpdateSkillLinkListAsync);
            links.MapDelete("/", DeleteSkillLinkListAsync);
        }

        private static async Task<IResult> GetSkillListAsync(
            [FromQuery(Name = "start-id")] string? startId,
            [FromQuery(Name = "limit")] int limit = 20
            )
        {
            List<Skill>? users;
            long totalCount;
            try
            {
                DatabaseClientFactory databaseClientFactory = new();
                using var databaseClient = databaseClientFactory.CreateSkillDatabaseClient();
                SkillAdapter adapter = new(databaseClient);
                users = await adapter.GetListAsync(startId, limit);
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
            SkillListGetResponse response = new()
            {
                TotalCount = totalCount,
                Count = users.Count,
                Skills = users
            };
            return Results.Ok(response);
        }

        private static async Task<IResult> CreateSkillAsync(
            [FromBody] SkillCreateRequest requestBody
            )
        {
            string id;
            try
            {
                DatabaseClientFactory databaseClientFactory = new();
                using var databaseClient = databaseClientFactory.CreateSkillDatabaseClient();
                SkillAdapter adapter = new(databaseClient);
                id = await adapter.CreateAsync(requestBody.Name);
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
            SkillCreateResponse response = new()
            {
                Id = id
            };
            return Results.Created($"/skills/{response.Id}", response);
        }

        private static async Task<IResult> GetSkillAsync(
            [FromRoute(Name = "id")] string id
            )
        {
            Skill? skill;
            try
            {
                DatabaseClientFactory databaseClientFactory = new();
                using var databaseClient = databaseClientFactory.CreateSkillDatabaseClient();
                SkillAdapter adapter = new(databaseClient);
                skill = await adapter.GetAsync(id);
            }
            catch (ArgumentException)
            {
                return Results.BadRequest();
            }
            catch (DatabaseParameterException)
            {
                return Results.BadRequest();
            }
            if (skill is null)
            {
                return Results.NotFound();
            }
            SkillGetResponse response = new()
            {
                Id = skill.Id,
                Name = skill.Name,
                Tags = skill.Tags,
                Aliases = skill.Aliases,
                Links = skill.Links
            };
            return Results.Ok(response);
        }

        private static async Task<IResult> UpdateSkillAsync(
            [FromRoute(Name = "id")] string id,
            [FromBody] SkillUpdateRequest requestBody
            )
        {
            bool success;
            try
            {
                DatabaseClientFactory databaseClientFactory = new();
                using var databaseClient = databaseClientFactory.CreateSkillDatabaseClient();
                SkillAdapter adapter = new(databaseClient);
                Skill? user = await adapter.GetAsync(id);
                if (user is null)
                {
                    return Results.NotFound();
                }

                Skill updatedUser = user with
                {
                    Name = requestBody.Name ?? user.Name,
                    Description = requestBody.Description ?? user.Description
                };
                success = await adapter.SetAsync(updatedUser);
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

        private static async Task<IResult> DeleteSkillAsync(
            [FromRoute(Name = "id")] string id
            )
        {
            bool success;
            try
            {
                DatabaseClientFactory databaseClientFactory = new();
                using var databaseClient = databaseClientFactory.CreateSkillDatabaseClient();
                SkillAdapter adapter = new(databaseClient);
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

        private static async Task<IResult> GetSkillTagListAsync(
            [FromRoute(Name = "id")] string id
            )
        {
            List<SkillTag>? tags;
            try
            {
                DatabaseClientFactory databaseClientFactory = new();
                using var databaseClient = databaseClientFactory.CreateSkillDatabaseClient();
                SkillAdapter adapter = new(databaseClient);
                tags = await adapter.GetTagListAsync(id);
            }
            catch (ArgumentException)
            {
                return Results.BadRequest();
            }
            catch (DatabaseParameterException)
            {
                return Results.BadRequest();
            }
            if (tags is null)
            {
                return Results.NotFound();
            }
            SkillTagListGetResponse response = new()
            {
                Count = tags.Count,
                Tags = tags
            };
            return Results.Ok(response);
        }

        private static async Task<IResult> UpdateSkillTagListAsync(
            [FromRoute(Name = "id")] string id,
            [FromBody] SkillTagListUpdateRequest requestBody
            )
        {
            bool success;
            try
            {
                DatabaseClientFactory databaseClientFactory = new();
                using var databaseClient = databaseClientFactory.CreateSkillDatabaseClient();
                SkillAdapter adapter = new(databaseClient);
                success = await adapter.SetTagListAsync(id, requestBody.TagIds);
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

        private static async Task<IResult> DeleteSkillTagListAsync(
            [FromRoute(Name = "id")] string id
            )
        {
            bool success;
            try
            {
                DatabaseClientFactory databaseClientFactory = new();
                using var databaseClient = databaseClientFactory.CreateSkillDatabaseClient();
                SkillAdapter adapter = new(databaseClient);
                success = await adapter.DeleteTagListAsync(id);
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

        private static async Task<IResult> GetSkillAliasListAsync(
            [FromRoute(Name = "id")] string id
            )
        {
            List<string>? aliases;
            try
            {
                DatabaseClientFactory databaseClientFactory = new();
                using var databaseClient = databaseClientFactory.CreateSkillDatabaseClient();
                SkillAdapter adapter = new(databaseClient);
                aliases = await adapter.GetAliasListAsync(id);
            }
            catch (ArgumentException)
            {
                return Results.BadRequest();
            }
            catch (DatabaseParameterException)
            {
                return Results.BadRequest();
            }
            if (aliases is null)
            {
                return Results.NotFound();
            }
            SkillAliasListGetResponse response = new()
            {
                Count = aliases.Count,
                Aliases = aliases
            };
            return Results.Ok(response);
        }

        private static async Task<IResult> UpdateSkillAliasListAsync(
            [FromRoute(Name = "id")] string id,
            [FromBody] SkillAliasListUpdateRequest requestBody
            )
        {
            bool success;
            try
            {
                DatabaseClientFactory databaseClientFactory = new();
                using var databaseClient = databaseClientFactory.CreateSkillDatabaseClient();
                SkillAdapter adapter = new(databaseClient);
                success = await adapter.SetAliasListAsync(id, requestBody.Aliases);
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

        private static async Task<IResult> DeleteSkillAliasListAsync(
            [FromRoute(Name = "id")] string id
            )
        {
            bool success;
            try
            {
                DatabaseClientFactory databaseClientFactory = new();
                using var databaseClient = databaseClientFactory.CreateSkillDatabaseClient();
                SkillAdapter adapter = new(databaseClient);
                success = await adapter.DeleteAliasListAsync(id);
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

        private static async Task<IResult> GetSkillLinkListAsync(
            [FromRoute(Name = "id")] string id
            )
        {
            List<SkillLink>? links;
            try
            {
                DatabaseClientFactory databaseClientFactory = new();
                using var databaseClient = databaseClientFactory.CreateSkillDatabaseClient();
                SkillAdapter adapter = new(databaseClient);
                links = await adapter.GetLinkListAsync(id);
            }
            catch (ArgumentException)
            {
                return Results.BadRequest();
            }
            catch (DatabaseParameterException)
            {
                return Results.BadRequest();
            }
            if (links is null)
            {
                return Results.NotFound();
            }
            SkillLinkListGetResponse response = new()
            {
                Count = links.Count,
                Links = links
            };
            return Results.Ok(response);
        }

        private static async Task<IResult> UpdateSkillLinkListAsync(
            [FromRoute(Name = "id")] string id,
            [FromBody] SkillLinkListUpdateRequest requestBody
            )
        {
            bool success;
            try
            {
                DatabaseClientFactory databaseClientFactory = new();
                using var databaseClient = databaseClientFactory.CreateSkillDatabaseClient();
                SkillAdapter adapter = new(databaseClient);
                success = await adapter.SetLinkListAsync(id, requestBody.Links);
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

        private static async Task<IResult> DeleteSkillLinkListAsync(
            [FromRoute(Name = "id")] string id
            )
        {
            bool success;
            try
            {
                DatabaseClientFactory databaseClientFactory = new();
                using var databaseClient = databaseClientFactory.CreateSkillDatabaseClient();
                SkillAdapter adapter = new(databaseClient);
                success = await adapter.DeleteLinkListAsync(id);
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
