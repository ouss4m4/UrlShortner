using System;
using Xunit;
using System.Reflection;

namespace Test
{
    public class ModelExistenceTests
    {
        [Theory]
        [InlineData("User")]
        [InlineData("Url")]
        [InlineData("Visit")]
        [InlineData("Analytics")]
        public void ModelClass_ShouldExist(string className)
        {
            var apiAssembly = Assembly.Load("UrlShortner");
            var type = apiAssembly.GetType($"UrlShortner.API.Models.{className}");
            Assert.True(type != null, $"Model class '{className}' should exist in UrlShortner.API.Models namespace");
        }
    }
}
