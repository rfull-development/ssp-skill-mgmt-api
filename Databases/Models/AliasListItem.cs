// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillManagementApi.Databases.Models
{
    [Table("alias_list")]
    public record class AliasListItem
    {
        [Column("item_id")]
        public long? ItemId { get; init; }
        [Column("guid")]
        public string? Guid { get; init; }
        [Column("name")]
        public string? Name { get; set; }
    }
}
