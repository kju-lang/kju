namespace KJU.Tests.Input
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.Diagnostics;
    using KJU.Core.Input;
    using KJU.Tests.Util;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class PreprocessorTests
    {
        private Preprocessor preprocessor;

        [TestInitialize]
        public void Initialize()
        {
            this.preprocessor = new Preprocessor();
        }

        [TestMethod]
        public void TestPreprocessInputEmptyString()
        {
            string s = string.Empty;
            var output = this.preprocessor.PreprocessInput(ToInput(s), null);
            var actual = FromPreprocessor(output);
            Assert.AreEqual(s, actual);
        }

        [TestMethod]
        public void TestPreprocessInputNoComments()
        {
            string s = "i am the walrus";
            var output = this.preprocessor.PreprocessInput(ToInput(s), null);
            var actual = FromPreprocessor(output);
            Assert.AreEqual(s, actual);
        }

        [TestMethod]
        public void TestPreprocessInputEmptyComment()
        {
            string s = "/**/";
            var output = this.preprocessor.PreprocessInput(ToInput(s), null);
            var actual = FromPreprocessor(output);
            Assert.AreEqual(" ", actual);
        }

        [TestMethod]
        public void TestPreprocessInputNonEmptyComment()
        {
            string s = "/* I am the walrus! */";
            var output = this.preprocessor.PreprocessInput(ToInput(s), null);
            var actual = FromPreprocessor(output);
            Assert.AreEqual(" ", actual);
        }

        [TestMethod]
        public void TestPreprocessInputNonEmpty()
        {
            string s = "i/**/am/**/the/**/walrus";
            var output = this.preprocessor.PreprocessInput(ToInput(s), null);
            var actual = FromPreprocessor(output);
            Assert.AreEqual("i am the walrus", actual);
        }

        [TestMethod]
        public void TestPreprocessInputAmbiguities()
        {
            string s = "/*/ What a beautiful way to write comments! Only one stray asterisk at the end :( */*";
            var output = this.preprocessor.PreprocessInput(ToInput(s), null);
            var actual = FromPreprocessor(output);
            Assert.AreEqual(" *", actual);
        }

        [TestMethod]
        public void TestPreprocessInputMultiline()
        {
            string s = "We have l o o/*\n" +
                       "Litwo! Ojczyzno moja! Ty jestes jak zdrowie,\n" +
                       "Ile cie trzeba cenic, ten tylko sie dowie,\n" +
                       "Kto cie stracil. Dzis pieknosc twa w calej ozdobie\n" +
                       "Widze i opisuje, bo tesknie po tobie*/o o o ng comment somewhere";
            var output = this.preprocessor.PreprocessInput(ToInput(s), null);
            var actual = FromPreprocessor(output);
            Assert.AreEqual("We have l o o o o o ng comment somewhere", actual);
        }

        [TestMethod]
        public void TestPreprocessInputNestedComments()
        {
            string s = "who/* whom/* whomst /* whomst'd /* whomst'd've */*/*/*/";
            var output = this.preprocessor.PreprocessInput(ToInput(s), null);
            var actual = FromPreprocessor(output);
            Assert.AreEqual("who ", actual);
        }

        [TestMethod]
        public void TestPreprocessInputInfiniteComment()
        {
            string s = "/*/* No problem here :^) */*";
            var diag = new Mock<IDiagnostics>();
            Assert.ThrowsException<PreprocessorException>(() =>
                FromPreprocessor(this.preprocessor.PreprocessInput(ToInput(s), diag.Object)));
            MockDiagnostics.Verify(diag, Preprocessor.UnterminatedCommentDiagnosticType);
        }

        [TestMethod]
        public void TestPreprocessInputNotMatchedCommentEnd()
        {
            string s = "/*/ OK, fixed now */*/";
            var diag = new Mock<IDiagnostics>();
            Assert.ThrowsException<PreprocessorException>(() =>
                FromPreprocessor(this.preprocessor.PreprocessInput(ToInput(s), diag.Object)));
            MockDiagnostics.Verify(diag, Preprocessor.CommentEndDiagnosticType);
        }

        private static IEnumerable<KeyValuePair<ILocation, char>> ToInput(IEnumerable<char> s)
        {
            return s.Select(c => new KeyValuePair<ILocation, char>(null, c));
        }

        private static string FromPreprocessor(IEnumerable<KeyValuePair<ILocation, char>> output)
        {
            return string.Concat(output.Select(t => t.Value));
        }
    }
}
