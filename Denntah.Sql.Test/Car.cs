using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Denntah.Sql.Test.Models
{
    [Table("cars")]
    public class Car
    {
        [Key]
        public string Id { get; set; }
        public string Make { get; set; }
        [Generated]
        public DateTime DateRegistered { get; set; }
    }
}
