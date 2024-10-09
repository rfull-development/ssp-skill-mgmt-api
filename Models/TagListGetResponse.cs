// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
using System.Text.Json.Serialization;

namespace SkillManagementApi.Models
{
    public record class TagListGetResponse
    {
        [JsonPropertyName("totalCount")]
        [JsonRequired]
        public required long TotalCount { get; set; }
        [JsonPropertyName("count")]
        [JsonRequired]
        public required long Count { get; set; }
        [JsonPropertyName("tags")]
        [JsonRequired]
        public required List<Tag> Tags { get; set; }
    }
}
