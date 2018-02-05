using Denntah.Sql.Reflection;
using Denntah.Sql.Test.Models;
using System;
using System.Linq;
using Xunit;

namespace Denntah.Sql.Test.Reflection
{
    public class UtilTest
    {
        [Theory]
        [InlineData("visualStudio")]
        [InlineData("VisualStudio")]
        [InlineData(" VisualStudio ")]
        public void ToUnderscore(string input)
        {
            Assert.Equal("visual_studio", Util.ToUnderscore(input));
        }
    }
}
