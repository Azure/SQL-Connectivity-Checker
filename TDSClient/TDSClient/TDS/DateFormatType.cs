namespace TDSClient.TDS
{
    /// <summary>
    /// Format of the date
    /// </summary>
    public enum DateFormatType
    {
        /// <summary>
        /// y/m/d
        /// </summary>
        YearMonthDay,

        /// <summary>
        /// y/d/m
        /// </summary>
        YearDayMonth,

        /// <summary>
        /// m/d/y
        /// </summary>
        MonthDayYear,

        /// <summary>
        /// m/y/d
        /// </summary>
        MonthYearDay,

        /// <summary>
        /// d/m/y
        /// </summary>
        DayMonthYear,

        /// <summary>
        /// d/y/m
        /// </summary>
        DayYearMonth
    }
}