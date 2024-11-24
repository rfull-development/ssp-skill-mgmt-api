// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
using System.Text.Json.Serialization;

namespace SkillManagementApi.Models
{
    public record class SkillAliasListGetResponse
    {
        [JsonPropertyName("count")]
        [JsonRequired]
        public required long Count { get; set; }
        [JsonPropertyName("aliases")]
        [JsonRequired]
        public required List<string> Aliases { get; set; }
    }
}
