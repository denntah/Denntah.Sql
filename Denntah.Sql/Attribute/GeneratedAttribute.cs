using System;
using System.Collections.Generic;
using System.Text;

namespace Denntah.Sql
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class GeneratedAttribute : Attribute
    {
        public GeneratedAttribute() { }
    }
}
