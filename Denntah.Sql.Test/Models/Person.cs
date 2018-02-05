using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Denntah.Sql.Test.Models
{
    [Table("persons")]
    public class Person
    {
        [Key]
        [Generated]
        public virtual long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public virtual string FullName { get { return FirstName + " " + LastName; } }
        public int? Age { get; set; }
        public Gender Gender { get; set; }
        [Generated]
        public DateTime? DateCreated { get; set; }
    }
}
