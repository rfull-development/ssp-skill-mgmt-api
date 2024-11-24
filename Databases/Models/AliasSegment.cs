// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillManagementApi.Databases.Models
{
    [Table("alias_seg")]
    public record class AliasSegment
    {
        [Column("item_id")]
        public long? ItemId { get; init; }
        [Column("version")]
        public int? Version { get; init; }
    }
}
