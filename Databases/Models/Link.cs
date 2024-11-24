// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillManagementApi.Databases.Models
{
    [Table("link")]
    public record class Link
    {
        [Column("item_id")]
        public long? ItemId { get; init; }
        [Column("title")]
        public string? Title { get; set; }
        [Column("url")]
        public string? Url { get; set; }
    }
}
