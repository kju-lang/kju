// <copyright file="HelloWorldTests.cs" company="KJU Supreme Language Development Team">
// Copyright (c) KJU Supreme Language Development Team. All rights reserved.
// Licensed under the BSD License 2.0. See LICENSE file in the project root for
// full license information.
// </copyright>

namespace KJU.Tests
{
    using KJU.Core;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Example tests class.
    /// </summary>
    [TestClass]
    public class HelloWorldTests
    {
        /// <summary>
        /// Verify whether example class returns good value.
        /// </summary>
        [TestMethod]
        public void TestHelloWorld()
        {
            KJU.Core.HelloWorld helloWorld = new KJU.Core.HelloWorld();
            Assert.AreEqual("Hello World!", helloWorld.Hello());
        }
    }
}
