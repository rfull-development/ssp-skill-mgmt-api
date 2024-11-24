// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillManagementApi.Databases.Models
{
    [Table("tag_alloc")]
    public record class TagAlloc
    {
        [Column("item_id")]
        public long? ItemId { get; init; }
        [Column("tag_id")]
        public long? TagId { get; init; }
    }
}
