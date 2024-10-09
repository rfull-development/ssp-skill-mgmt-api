// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
using System.Text.Json.Serialization;

namespace SkillManagementApi.Models
{
    public record class Skill
    {
        [JsonPropertyName("id")]
        [JsonRequired]
        public required string Id { get; set; }

        [JsonPropertyName("name")]
        [JsonRequired]
        public required string Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
        [JsonPropertyName("tags")]
        public List<SkillTag>? Tags { get; set; }
        [JsonPropertyName("aliases")]
        public List<string>? Aliases { get; set; }
        [JsonPropertyName("links")]
        public List<SkillLink>? Links { get; set; }
    }
}
