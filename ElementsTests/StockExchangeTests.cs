using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Elements.Tests
{
    [TestClass()]
    public class StockExchangeTests
    {
        StockExchange exchange = new StockExchange();
        DateTime isNotHolidayAt3pm = new DateTime(2022, 05, 26, 15, 0, 0);
        DateTime isholidayAt3pm = new DateTime(2022, 05, 30, 15, 0, 0);
        DateTime sundayAfternoon = new DateTime(2022, 05, 15, 12, 0, 0);
        DateTime monday = new DateTime(2022, 05, 16);

        [TestMethod()]
        public void TimeLeftUntilOpenTest()
        {
            DateTime monday5amLocal = monday.AddHours(5);
            DateTime monday10amLocal = monday.AddHours(10);
            DateTime monday3pmLocal = monday.AddHours(15);

            Assert.AreEqual(exchange.TimeLeftUntilOpen(monday5amLocal), new TimeSpan(9, 30, 0), "Should be 9 hours 30 minutes");
            Assert.AreEqual(exchange.TimeLeftUntilOpen(monday10amLocal), new TimeSpan(4, 30, 0), "Should be 4 hours 30 minutes");
            Assert.AreEqual(exchange.TimeLeftUntilOpen(monday3pmLocal), new TimeSpan(0, 0, 0), "Should be 0 hours 0 minutes, currently open");
        }

        [TestMethod()]
        public void TimeLeftUntilCloseTest()
        {
            DateTime monday3pmLocal = monday.AddHours(15);

            Assert.AreEqual(exchange.TimeLeftUntilClose(monday3pmLocal), new TimeSpan(6, 0, 0), "Should be 6 hours");
        }

        [TestMethod()]
        public void GetDateTimeAtStockExchangeTest()
        {
            DateTime monday3pmLocal = monday.AddHours(15);


            Assert.AreEqual(exchange.GetDateTimeAtStockExchange(monday3pmLocal), monday3pmLocal.AddHours(-5), "Should be 10am on Monday 16th May");
        }

        [TestMethod()]
        public void GetDayOfWeekAtStockExchangeTest()
        {
            DateTime monday3pmLocal = monday.AddHours(15);

            Assert.AreEqual(exchange.GetDayOfWeekAtStockExchange(monday3pmLocal), DayOfWeek.Monday, "Should be Monday");
            Assert.AreEqual(exchange.GetDayOfWeekAtStockExchange(sundayAfternoon), DayOfWeek.Sunday, "Should be Sunday");
        }

        [TestMethod()]
        public void GetTimeAtStockExchangeTest()
        {
            DateTime monday3pmLocal = monday.AddHours(15);

            Assert.AreEqual(exchange.GetTimeAtStockExchange(monday3pmLocal), monday3pmLocal.AddHours(-5).TimeOfDay, "Should be 10am");
        }

        [TestMethod()]
        public void CalculateNextOpenDateTimeTest()
        {
            DateTime monday10amLocal = monday.AddHours(10);
            DateTime monday11pmLocal = monday.AddHours(23);

            Assert.AreEqual(exchange.CalculateNextOpenDateTime(monday10amLocal), monday10amLocal.AddHours(4).AddMinutes(30), "Should be 2:30pm on Monday 16th May");
            Assert.AreEqual(exchange.CalculateNextOpenDateTime(monday11pmLocal), monday11pmLocal.AddHours(15).AddMinutes(30), "Should be 2:30pm on Tuesday 17th May");
        }

        [TestMethod()]
        public void IsOpenNowTest()
        {
            DateTime monday5amLocal = monday.AddHours(5);
            DateTime monday10amLocal = monday.AddHours(10);
            DateTime monday3pmLocal = monday.AddHours(15);
            DateTime monday11pmLocal = monday.AddHours(23);

            Assert.IsFalse(exchange.IsOpenNow(monday5amLocal), "Early in the morning, should be closed");
            Assert.IsFalse(exchange.IsOpenNow(monday10amLocal), "Early in the morning, should be closed");
            Assert.IsTrue(exchange.IsOpenNow(monday3pmLocal), "Should be open");
            Assert.IsFalse(exchange.IsOpenNow(monday11pmLocal), "Late in the evening, should be closed");
        }

        [TestMethod()]
        public void IsOpenTodayTest()
        {
            DateTime monday3pmLocal = monday.AddHours(15);

            Assert.AreEqual(ExchangeState.OPEN, exchange.IsOpen(monday3pmLocal), "Should be open");
            Assert.AreEqual(ExchangeState.WEEKEND, exchange.IsOpen(sundayAfternoon), "It's the weekend, should be closed");
            Assert.AreEqual(ExchangeState.HOLIDAY, exchange.IsOpen(isholidayAt3pm), "It's a holiday, should be closed");
        }

        [TestMethod()]
        public void IsOpenTest()
        {
            DateTime monday5amLocal = monday.AddHours(5);
            DateTime monday10amLocal = monday.AddHours(10);
            DateTime monday3pmLocal = monday.AddHours(15);
            DateTime monday11pmLocal = monday.AddHours(23);

            Assert.AreEqual(ExchangeState.CLOSED, exchange.IsOpen(monday5amLocal), "Early in the morning, should be closed");
            Assert.AreEqual(ExchangeState.CLOSED, exchange.IsOpen(monday10amLocal), "Early in the morning, should be closed");
            Assert.AreEqual(ExchangeState.CLOSED, exchange.IsOpen(monday11pmLocal), "Late in the evening, should be closed");
            Assert.AreEqual(ExchangeState.OPEN, exchange.IsOpen(monday3pmLocal), "Should be open");
            Assert.AreEqual(ExchangeState.WEEKEND, exchange.IsOpen(sundayAfternoon), "It's the weekend, should be closed");
            Assert.AreEqual(ExchangeState.HOLIDAY, exchange.IsOpen(isholidayAt3pm), "It's a holiday, should be closed");
        }

        [TestMethod()]
        public void IsHolidayTest()
        {
            Assert.IsTrue(exchange.IsHoliday(isholidayAt3pm), "This day is a holiday");
            Assert.IsFalse(exchange.IsHoliday(isNotHolidayAt3pm), "This day is NOT a holiday");
        }
    }
}