// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
namespace SkillManagementApi.Databases.Interfaces
{
    public interface IDatabaseClientFactory
    {
        ISkillDatabaseClient CreateSkillDatabaseClient();
        ITagDatabaseClient CreateTagDatabaseClient();
    }
}
