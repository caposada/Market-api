namespace Elements
{

    public class StockExchange
    {
        public static List<DateTime> holidays = new List<DateTime>()
        {
            new DateTime(2022, 04, 15), //Good Friday                 April 15, 2022      Closed
            new DateTime(2022, 05, 30), //Memorial Day                May 30, 2022        Closed
            new DateTime(2022, 06, 20), //Juneteenth Holiday          June 20, 2022       Closed
            new DateTime(2022, 07, 04), //Independence Day            July 4, 2022        Closed
            new DateTime(2022, 09, 05), //Labor Day                   September 5, 2022   Closed
            new DateTime(2022, 11, 24), //Thanksgiving Day            November 24, 2022   Closed
            new DateTime(2022, 11, 25), //Early Close                 November 25, 2022   1:00pm
            new DateTime(2022, 12, 26), //Christmas Holiday           December 26, 2022   Closed
        };
        public static TimeSpan OpeningTime = new TimeSpan(9, 30, 0); // 09:30am
        public static TimeSpan ClosingTime = new TimeSpan(16, 0, 0); // 04:00pm
        private static TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

        public TimeSpan TimeLeftUntilOpen(DateTime? thisTime = null)
        {
            if (!IsOpenNow(thisTime))
            {
                TimeSpan timeOverThere = GetTimeAtStockExchange(thisTime);
                return OpeningTime - timeOverThere;
            }
            else
            {
                return TimeSpan.Zero;
            }
        }

        public TimeSpan TimeLeftUntilClose(DateTime? thisTime = null)
        {
            if (IsOpenNow(thisTime))
            {
                TimeSpan timeOverThere = GetTimeAtStockExchange(thisTime);
                return ClosingTime - timeOverThere;
            }
            else
            {
                return TimeSpan.Zero;
            }
        }

        public DateTime GetDateTimeAtStockExchange(DateTime? thisTime = null)
        {
            thisTime = thisTime ?? DateTime.Now;
            //bool isDaylightSavingTime =
            //    TimeZoneInfo.Local.IsDaylightSavingTime(thisTime.Value);
            DateTime easternStandardTimeTime =
                TimeZoneInfo.ConvertTime(thisTime.Value, TimeZoneInfo.Local, easternZone);
            return easternStandardTimeTime;
        }

        public DayOfWeek GetDayOfWeekAtStockExchange(DateTime? thisTime = null)
        {
            return GetDateTimeAtStockExchange(thisTime).DayOfWeek;
        }

        public TimeSpan GetTimeAtStockExchange(DateTime? thisTime = null)
        {
            return GetDateTimeAtStockExchange(thisTime).TimeOfDay;
        }

        public DateTime CalculateNextOpenDateTime(DateTime? thisTime = null)
        {
            thisTime = thisTime ?? DateTime.Now;
            DateTime nextOpenDateTime = GetDateTimeAtStockExchange(thisTime);

            if (IsOpenNow(thisTime) || nextOpenDateTime.TimeOfDay > ClosingTime)
            {
                nextOpenDateTime = nextOpenDateTime.AddDays(1);
            }

            // Check if tomorrow a weekend or holiday, and if so go to next date
            DayOfWeek dow = nextOpenDateTime.DayOfWeek;
            bool isWeekend = dow == DayOfWeek.Saturday || dow == DayOfWeek.Sunday;
            bool isHoliday = IsHoliday(nextOpenDateTime);
            while (isWeekend || isHoliday)
            {
                nextOpenDateTime = nextOpenDateTime.AddDays(1);
                dow = nextOpenDateTime.DayOfWeek;
                isWeekend = dow == DayOfWeek.Saturday || dow == DayOfWeek.Sunday;
                isHoliday = IsHoliday(nextOpenDateTime);
            };
            // Found our next open day

            // Set the current opening time
            nextOpenDateTime = new DateTime(
                nextOpenDateTime.Year,
                nextOpenDateTime.Month,
                nextOpenDateTime.Day,
                OpeningTime.Hours,
                OpeningTime.Minutes,
                0);

            nextOpenDateTime = TimeZoneInfo.ConvertTime(nextOpenDateTime, easternZone, TimeZoneInfo.Local);

            return nextOpenDateTime;
        }

        public bool IsOpenNow(DateTime? thisTime = null)
        {
            TimeSpan timeOverThere = GetTimeAtStockExchange(thisTime);
            return timeOverThere >= OpeningTime && timeOverThere <= ClosingTime;
        }

        public ExchangeState IsOpenToday(DateTime? thisTime = null)
        {
            DateTime dateTime = GetDateTimeAtStockExchange(thisTime);

            // Check if weekend
            if (dateTime.DayOfWeek == DayOfWeek.Saturday && dateTime.DayOfWeek == DayOfWeek.Sunday)
                return ExchangeState.WEEKEND; // Weekend!

            // Check if holiday
            bool foundHoliday = holidays.Any(x => x.Date == dateTime.Date);
            if (foundHoliday)
                return ExchangeState.HOLIDAY;

            // It is open
            return ExchangeState.OPEN;
        }

        public ExchangeState IsOpen(DateTime? thisTime = null)
        {
            DateTime dateTimeOverThere = GetDateTimeAtStockExchange(thisTime);

            // Check if weekend
            if (dateTimeOverThere.DayOfWeek == DayOfWeek.Saturday || dateTimeOverThere.DayOfWeek == DayOfWeek.Sunday)
                return ExchangeState.WEEKEND; // Weekend!

            // Check if holiday
            bool foundHoliday = holidays.Any(x => x.Date == dateTimeOverThere.Date);
            if (foundHoliday)
                return ExchangeState.HOLIDAY;

            // Check out of hours time
            TimeSpan timeOfDay = dateTimeOverThere.TimeOfDay;
            if (timeOfDay < OpeningTime || timeOfDay > ClosingTime)
                return ExchangeState.CLOSED;

            // It is open
            return ExchangeState.OPEN;
        }

        public bool IsHoliday(DateTime checkDateTime)
        {
            DateTime foundDateTime = holidays.Find(x => x.Date == checkDateTime.Date);
            return foundDateTime != DateTime.MinValue;
        }

    }

}
