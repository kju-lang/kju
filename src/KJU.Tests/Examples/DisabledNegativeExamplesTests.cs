namespace KJU.Tests.Examples
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using KJU.Core;
    using KJU.Core.Diagnostics;
    using KJU.Core.Input;
    using KJU.Core.Parser;
    using KJU.Tests.Examples;
    using KJU.Tests.Util;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DisabledNegativeExamplesTests
    {
        public static IEnumerable<object[]> Data
        {
            get
            {
                var examplesGetter = new KjuExamplesGetter();
                return examplesGetter.Examples
                    .Where((example) => example.IsDisabled)
                    .Where((example) => !example.IsPositive)
                    .Select((example) => new object[] { example });
            }
        }

        [DataTestMethod]
        [DynamicData(nameof(Data), DynamicDataSourceType.Property)]
        public void TestExamplesSpecification(KjuExample example)
        {
            try
            {
                new NegativeExamplesTests().TestExamplesSpecification(example);
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"{ex}");
            }
        }
    }
}
