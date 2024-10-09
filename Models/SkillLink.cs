// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
using System.Text.Json.Serialization;

namespace SkillManagementApi.Models
{
    public record class SkillLink
    {
        [JsonPropertyName("title")]
        [JsonRequired]
        public required string Title { get; set; }

        [JsonPropertyName("url")]
        [JsonRequired]
        public required string Url { get; set; }
    }
}
