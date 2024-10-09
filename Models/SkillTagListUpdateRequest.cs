// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
using System.Text.Json.Serialization;

namespace SkillManagementApi.Models
{
    public record class SkillTagListUpdateRequest
    {
        [JsonPropertyName("tagIds")]
        [JsonRequired]
        public required List<string> TagIds { get; set; }
    }
}
