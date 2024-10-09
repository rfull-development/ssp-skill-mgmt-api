// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
using System.Text.Json.Serialization;

namespace SkillManagementApi.Models
{
    public record class TagUpdateRequest
    {
        [JsonPropertyName("name")]
        [JsonRequired]
        public required string Name { get; set; }
    }
}
