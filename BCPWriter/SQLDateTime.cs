﻿namespace BCPWriter
{
    using System;
    using System.IO;

    /// <summary>
    /// Obsolete! SQL datetime.
    /// </summary>
    /// 
    /// <remarks>
    /// <a href="http://msdn.microsoft.com/en-us/library/ms187819.aspx">datetime (Transact-SQL)</a><br/>
    /// <br/>
    /// From SQL Server 2008 Books Online:<br/>
    /// <br/>
    /// Use the time, date, datetime2 and datetimeoffset data types for new work.
    /// These types align with the SQL Standard.
    /// They are more portable.
    /// time, datetime2 and datetimeoffset provide more seconds precision.
    /// datetimeoffset provides time zone support for globally deployed applications.
    /// </remarks>
    [Obsolete]
    public class SQLDateTime : IBCPSerialization
    {
        /// <summary>
        /// Minimum value allowed for SQL datetime.
        /// </summary>
        public static DateTime MIN_DATETIME = new DateTime(1753, 01, 01, 00, 00, 00);

        public void Write(BinaryWriter writer, object value)
        {
            Write(writer, (DateTime?)value);
        }

        public static void Write(BinaryWriter writer, DateTime? dateTime)
        {
            if (!dateTime.HasValue)
            {
                // 8 bytes long
                byte[] nullBytes = { 255, 255, 255, 255, 255, 255, 255, 255 };
                writer.Write(nullBytes);
                return;
            }

            if (dateTime.Value < MIN_DATETIME)
            {
                throw new ArgumentException("dateTime cannot be inferior than 1753-01-01T00:00:00");
            }

            // byte is 1 byte long :)
            const byte size = 8;
            writer.Write(size);

            // Format:
            // 1753-01-01T00:00:00 gives 08 46 2E FF FF 00 00 00 00
            // 1900-01-01T00:00:00 gives 08 00 00 00 00 00 00 00 00
            // 1900-01-02T00:00:00 gives 08 01 00 00 00 00 00 00 00 -> 00 00 00 01 = 1 day
            // 1900-02-01T00:00:00 gives 08 1F 00 00 00 00 00 00 00 -> 00 00 00 1F = 31 days
            // 1900-02-02T00:00:00 gives 08 20 00 00 00 00 00 00 00 -> 00 00 00 20 = 32 days
            // 1900-09-30T00:00:00 gives 08 10 01 00 00 00 00 00 00 -> 00 00 01 10 = 272 days
            // 2000-01-01T00:00:00 gives 08 AC 8E 00 00 00 00 00 00 -> 00 00 8E AC = 36524 days

            // 1900-01-01T00:00:00 gives 08 00 00 00 00 00 00 00 00
            // 1900-01-01T00:00:01 gives 08 00 00 00 00 2C 01 00 00 -> 00 00 01 2C = 300
            // 1900-01-01T00:01:00 gives 08 00 00 00 00 50 46 00 00 -> 00 00 46 50 = 18000
            // 1900-01-01T01:00:00 gives 08 00 00 00 00 C0 7A 10 00 -> 00 10 7A C0 = 1080000
            // 1900-01-01T01:01:01 gives 08 00 00 00 00 3C C2 10 00 -> 00 10 C2 3C = 1098300

            // 1900-01-01T00:00:00.001 gives 08 00 00 00 00 00 00 00 00
            // 1900-01-01T00:00:00.002 gives 08 00 00 00 00 01 00 00 00 = 1
            // 1900-01-01T00:00:00.003 gives 08 00 00 00 00 01 00 00 00 = 1
            // 1900-01-01T00:00:00.004 gives 08 00 00 00 00 01 00 00 00 = 1
            // 1900-01-01T00:00:00.005 gives 08 00 00 00 00 02 00 00 00 = 2
            // 1900-01-01T00:00:00.010 gives 08 00 00 00 00 03 00 00 00 = 3
            // 1900-01-01T00:00:00.100 gives 08 00 00 00 00 1E 00 00 00 = 30
            // 1900-01-01T00:00:00.200 gives 08 00 00 00 00 3C 00 00 00 = 60
            // 1900-01-01T00:00:00.500 gives 08 00 00 00 00 96 00 00 00 = 150
            // 1900-01-01T00:00:00.999 gives 08 00 00 00 00 2C 01 00 00 -> 00 00 01 2C = 300

            // 1900-01-01T14:25:10 gives 08 00 00 00 00 08 A0 ED 00 -> 00 ED A0 08 = 15573000
            // 2004-05-23T14:25:10 gives 08 F0 94 00 00 08 A0 ED 00 -> 00 94 F0 08 = 9760776 days
            //                                                      -> 00 ED A0 08 = 15573000

            // 2004-05-23T14:25:10.123 gives 08 F0 94 00 00 2D A0 ED 00 -> 00 ED A0 2D = 15573037

            // Date, 4 bytes long
            DateTime date = dateTime.Value.Date;
            DateTime initDate = new DateTime(1900, 01, 01, 00, 00, 00);
            TimeSpan span = date - initDate;
            writer.Write((int)span.TotalDays);

            // Time, 4 bytes long
            DateTime time = DateTime.Parse(dateTime.Value.ToString("HH:mm:ss.fffffff"));
            DateTime initTime = new DateTime(time.Year, time.Month, time.Day, 00, 00, 00);
            span = time - initTime;
            int bcpTimeFormat = (int)(span.TotalMilliseconds * 0.3);
            writer.Write(bcpTimeFormat);
        }
    }
}
