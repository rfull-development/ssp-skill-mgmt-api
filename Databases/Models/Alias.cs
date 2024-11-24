// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillManagementApi.Databases.Models
{
    [Table("alias")]
    public record class Alias
    {
        [Column("item_id")]
        public long? ItemId { get; init; }
        [Column("alias_item_id")]
        public long? AliasItemId { get; init; }
    }
}
