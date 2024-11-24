// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillManagementApi.Databases.Models
{
    [Table("item")]
    public record class Item
    {
        [Column("id")]
        public long? Id { get; init; }
        [Column("guid")]
        public Guid? Guid { get; init; }
        [Column("version")]
        public int? Version { get; init; }
        [Column("name")]
        public string? Name { get; set; }
        [Column("description")]
        public string? Description { get; set; }
    }
}
