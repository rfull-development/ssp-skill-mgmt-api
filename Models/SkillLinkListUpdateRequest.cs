// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
using System.Text.Json.Serialization;

namespace SkillManagementApi.Models
{
    public record class SkillLinkListUpdateRequest
    {
        [JsonPropertyName("links")]
        [JsonRequired]
        public required List<SkillLink> Links { get; set; }
    }
}
