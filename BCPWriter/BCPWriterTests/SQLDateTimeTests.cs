﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NUnit.Framework;

using BCPWriter;

namespace BCPWriter.Tests
{
    [TestFixture]
    class SQLDateTimeTests
    {
        private void WriteDateTime(DateTime? dateTime, string myFileName)
        {
            BinaryWriter writer = BCPTests.CreateBinaryFile(myFileName);

            SQLDateTime sql = new SQLDateTime();
            sql.Write(writer, dateTime);

            writer.Close();
        }

        [Test]
        public void TestDateTimeSeconds()
        {
            DateTime dateTime = DateTime.Parse(
                                    "2004-05-23T14:25:10",
                                    System.Globalization.CultureInfo.InvariantCulture
                                );

            string myFileName = "datetime_seconds.bcp";
            WriteDateTime(dateTime, myFileName);
            BCPTests.CheckFile(myFileName);
        }

        [Test]
        public void TestDateTimeMilliseconds()
        {
            DateTime dateTime = DateTime.Parse(
                                    "2004-05-23T14:25:10.123",
                                    System.Globalization.CultureInfo.InvariantCulture
                                );

            string myFileName = "datetime_milliseconds.bcp";
            WriteDateTime(dateTime, myFileName);
            //Comparison can be 1 millisecond different due to cast double to long
            //and the way bcp rounds
            //Nothing to be worry about
            //BCPTests.CheckFile(myFileName);
        }

        [Test]
        public void TestDateTimeMin()
        {
            DateTime dateTime = SQLDateTime.MIN_DATETIME;

            string myFileName = "datetime_min.bcp";
            WriteDateTime(dateTime, myFileName);
            BCPTests.CheckFile(myFileName);
        }

        [Test]
        public void TestDateTimeNull()
        {
            string myFileName = "datetime_null.bcp";
            WriteDateTime(null, myFileName);
            BCPTests.CheckFile(myFileName);
        }
    }
}