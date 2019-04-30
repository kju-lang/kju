#pragma warning disable SA1118 // The parameter spans multiple lines ???
namespace KJU.Tests.Intermediate
{
    using System.Collections.Generic;
    using KJU.Core.AST;
    using KJU.Core.AST.BuiltinTypes;
    using KJU.Core.Intermediate;
    using KJU.Core.Intermediate.NameMangler;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class NameManglerTest
    {
        private readonly NameMangler nameMangler = new NameMangler();

        [TestMethod]
        public void Test()
        {
            // tested with c++filt
            FunctionDeclaration function1 = new FunctionDeclaration(
                identifier: "foo",
                returnType: null,
                parameters: new List<VariableDeclaration>()
                {
                    new VariableDeclaration(IntType.Instance, null, null),
                    new VariableDeclaration(IntType.Instance, null, null),
                    new VariableDeclaration(BoolType.Instance, null, null),
                },
                body: null);

            Assert.AreEqual(
                actual: this.nameMangler.GetMangledName(function1, null),
                expected: "_ZN3KJU3fooExxb");

            Assert.AreEqual(
                actual: this.nameMangler.GetMangledName(function1, "_ZN3KJU3barEv"),
                expected: "_ZZN3KJU3barEvEN3fooExxb");

            Assert.AreEqual(
                actual: this.nameMangler.GetMangledName(function1, "_ZZN3KJU3fooEvEN3fooExxb"),
                expected: "_ZZZN3KJU3fooEvEN3fooExxbEN3fooExxb");

            FunctionDeclaration function2 = new FunctionDeclaration(
                identifier: "bar",
                returnType: null,
                parameters: new List<VariableDeclaration>() { },
                body: null);

            Assert.AreEqual(
                actual: this.nameMangler.GetMangledName(function2, null),
                expected: "_ZN3KJU3barEv");
        }
    }
}