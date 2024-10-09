// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
using System.Text.Json.Serialization;

namespace SkillManagementApi.Models
{
    public record class SkillLinkListGetResponse
    {
        [JsonPropertyName("count")]
        [JsonRequired]
        public required long Count { get; set; }
        [JsonPropertyName("links")]
        [JsonRequired]
        public required List<SkillLink> Links { get; set; }
    }
}
