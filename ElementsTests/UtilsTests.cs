using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Elements.Tests
{
    [TestClass()]
    public class UtilsTests
    {
        [TestMethod()]
        public void IsAllUppercaseTest()
        {
            Assert.IsFalse(Utils.IsAllUppercase("abc"), "No uppercase characters");
            Assert.IsFalse(Utils.IsAllUppercase("aBC"), "One character is lowercase");
            Assert.IsTrue(Utils.IsAllUppercase("ABC"), "All characters are uppercase");
        }

        [TestMethod()]
        public void AcronymsTest()
        {
            Assert.AreEqual(0, Utils.Acronyms("Thames is a river").Length, "No acronyms here");
            Assert.AreEqual(1, Utils.Acronyms("ABC has won").Length, "A single acronym here");
            Assert.AreEqual("ABC", Utils.Acronyms("ABC has won")[0], "The 'ABC' acronym here");
            Assert.AreEqual(2, Utils.Acronyms("ABC has won and XYZ has as well").Length, "Two acronyms here");
        }

        [TestMethod()]
        public void SplitTextTest()
        {
            Assert.AreEqual(1, Utils.SplitText("Thames").Length, "One word here");
            Assert.AreEqual(4, Utils.SplitText("Thames is a river").Length, "Four words here");
            Assert.AreEqual(1, Utils.SplitText("Thames-is-a-river").Length, "One word here");
        }

        [TestMethod()]
        public void CleanTest()
        {
            Assert.AreEqual("Fruits are good", Utils.Clean("Fruits (like apples) are good"), "Should remove text in brackets and the brackets and any extra spaces left behind");
            Assert.AreEqual("One", Utils.Clean("One - Two - Three"), "Should remove hyphens and anything following");
            Assert.AreEqual("I hate it when there are big gaps", Utils.Clean("I hate it when there are big    gaps"), "Should remove extra spaces left behind");
        }

        [TestMethod()]
        public void ReduceTest()
        {
            Assert.AreEqual("Fruits are doing well", Utils.Reduce("Fruits Ltd are doing well"), "Should remove words like Ltd, Limited, plc, etc. and any extra spaces left behind");
            Assert.AreEqual("Sanfilippo & Son Spice", Utils.Reduce("Sanfilippo & Son @Spice £ "), "Should remove odd characters (keeping &) and any extra spaces left behind and trim");
        }

        [TestMethod()]
        public void RemoveMultipleSpacesTest()
        {
            Assert.AreEqual("I hate it when there are big gaps", Utils.Reduce("I hate it when there are big    gaps"), "Should remove extra spaces");
        }

        [TestMethod()]
        public void GetWhenTest()
        {
            DateTime sunday12pm = new DateTime(2022, 05, 15, 12, 0, 0);
            DateTime sunday1pm = new DateTime(2022, 05, 15, 13, 0, 0);
            int expected = 1 * 60 * 60 * 1000; // 1 hour * 60 minutes * 60 seconds * 1000 milliseconds in a second
            Assert.AreEqual(expected, Utils.GetWhen(sunday1pm, sunday12pm), "Should be the number of milliseconds in an hour");
        }
    }
}