using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Denntah.Sql.Test.Models
{
    [Table("parent_children")]
    public class ParentChild
    {
        [Key]
        public long ParentId { get; set; }
        [Key]
        public long ChildId { get; set; }
    }
}
