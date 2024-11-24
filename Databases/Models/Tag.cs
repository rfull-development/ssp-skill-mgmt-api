// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillManagementApi.Databases.Models
{
    [Table("tag")]
    public record class Tag
    {
        [Column("id")]
        public long? Id { get; init; }
        [Column("version")]
        public int? Version { get; init; }
        [Column("name")]
        public string? Name { get; set; }
    }
}
