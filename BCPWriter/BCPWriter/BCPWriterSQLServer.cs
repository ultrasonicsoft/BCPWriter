﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Data;
using System.Data.SqlClient;

namespace BCPWriter
{
    class BCPWriterSQLServer
    {
        public BCPWriterSQLServer(BinaryWriter writer, List<IBCPSerialization> columns, IEnumerable<object> rows)
        {
            if (columns.Count() == 0)
            {
                throw new ArgumentException("No columns");
            }

            string createTableString = GetCreateTableString(columns);
            string insertIntoString = GetInsertIntoString(columns, rows);

            SendSQLRequests(createTableString, insertIntoString);
            GenerateBCPFileFromSQLServer(writer);
        }

        static private string GetCreateTableString(IEnumerable<IBCPSerialization> columns)
        {
            StringBuilder createTableString = new StringBuilder();
            createTableString.AppendLine("CREATE TABLE BCPTest (");

            int columnNumber = 0;
            foreach (IBCPSerialization column in columns)
            {
                createTableString.AppendFormat("col{0} ", columnNumber++);

                //FIXME Is there a better way than casting every type?
                //Don't forget to add new SQL types here
                //and to modify the unit tests accordingly
                if (column is SQLBinary)
                {
                    SQLBinary sql = (SQLBinary)column;
                    createTableString.AppendFormat("binary({0})", sql.Length);
                }
                else if (column is SQLChar)
                {
                    SQLChar sql = (SQLChar)column;
                    createTableString.AppendFormat("char({0})", sql.Length);
                }
                else if (column is SQLInt)
                {
                    SQLInt sql = (SQLInt)column;
                    createTableString.Append("int");
                }
                else if (column is SQLNChar)
                {
                    SQLNChar sql = (SQLNChar)column;
                    createTableString.AppendFormat("nchar({0})", sql.Length);
                }
                else if (column is SQLNVarChar)
                {
                    SQLNVarChar sql = (SQLNVarChar)column;
                    if (sql.Length == SQLNVarChar.MAX)
                    {
                        createTableString.Append("nvarchar(max)");
                    }
                    else
                    {
                        createTableString.AppendFormat("nvarchar({0})", sql.Length);
                    }
                }
                else if (column is SQLVarBinary)
                {
                    SQLVarBinary sql = (SQLVarBinary)column;
                    if (sql.Length == SQLVarBinary.MAX)
                    {
                        createTableString.Append("varbinary(max)");
                    }
                    else
                    {
                        createTableString.AppendFormat("varbinary({0})", sql.Length);
                    }
                }
                else if (column is SQLVarChar)
                {
                    SQLVarChar sql = (SQLVarChar)column;
                    if (sql.Length == SQLVarChar.MAX)
                    {
                        createTableString.Append("varchar(max)");
                    }
                    else
                    {
                        createTableString.AppendFormat("varchar({0})", sql.Length);
                    }
                }
                else if (column is SQLNText)
                {
                    SQLNText sql = (SQLNText)column;
                    createTableString.Append("ntext");
                }
                else if (column is SQLText)
                {
                    SQLText sql = (SQLText)column;
                    createTableString.Append("text");
                }
                else if (column is SQLXML)
                {
                    SQLXML sql = (SQLXML)column;
                    createTableString.Append("xml");
                }
                else if (column is SQLFloat)
                {
                    SQLFloat sql = (SQLFloat)column;
                    createTableString.Append("float");
                }
                else if (column is SQLReal)
                {
                    SQLReal sql = (SQLReal)column;
                    createTableString.Append("real");
                }
                else if (column is SQLUniqueIdentifier)
                {
                    SQLUniqueIdentifier sql = (SQLUniqueIdentifier)column;
                    createTableString.Append("uniqueidentifier");
                }
                else if (column is SQLBigInt)
                {
                    SQLBigInt sql = (SQLBigInt)column;
                    createTableString.Append("bigint");
                }
                else if (column is SQLDateTime)
                {
                    SQLDateTime sql = (SQLDateTime)column;
                    createTableString.Append("datetime");
                }
                else if (column is SQLDateTime2)
                {
                    SQLDateTime2 sql = (SQLDateTime2)column;
                    createTableString.Append("datetime2");
                }
                else if (column is SQLDate)
                {
                    SQLDate sql = (SQLDate)column;
                    createTableString.Append("date");
                }
                else if (column is SQLTime)
                {
                    SQLTime sql = (SQLTime)column;
                    createTableString.Append("time");
                }
                else
                {
                    System.Diagnostics.Trace.Assert(false);
                }

                if (columnNumber < columns.Count())
                {
                    createTableString.AppendLine(",");
                }
            }

            createTableString.Append(")");

            return createTableString.ToString();
        }

        static private string GetInsertIntoString(List<IBCPSerialization> columns, IEnumerable<object> rows)
        {
            StringBuilder insertIntoString = new StringBuilder();

            insertIntoString.AppendLine("INSERT INTO BCPTest VALUES");

            for (int i = 0; i < rows.Count(); i++)
            {
                int modulo = i % columns.Count();
                if (modulo == 0 && i > 0)
                {
                    insertIntoString.AppendLine("),");
                }
                if (modulo > 0)
                {
                    insertIntoString.AppendLine(",");
                }
                if (modulo == 0)
                {
                    insertIntoString.Append("(");
                }

                IBCPSerialization column = columns[modulo];
                object row = rows.ElementAt(i);

                //FIXME Is there a better way than casting every type?
                //Don't forget to add new SQL types here
                //and to modify the unit tests accordingly
                if (column is SQLBinary)
                {
                    SQLBinary sql = (SQLBinary)column;
                    byte[] value = (byte[])row;
                    if (value == null)
                    {
                        insertIntoString.Append("NULL");
                    }
                    else
                    {
                        insertIntoString.AppendFormat(
                            "CAST('{0}' AS binary({1}))",
                            Encoding.Default.GetString(value), sql.Length);
                    }
                }
                else if (column is SQLChar)
                {
                    string value = (string)row;
                    if (value == null)
                    {
                        insertIntoString.Append("NULL");
                    }
                    else
                    {
                        insertIntoString.AppendFormat("'{0}'", value);
                    }
                }
                else if (column is SQLInt)
                {
                    int? value = (int?)row;
                    if (!value.HasValue)
                    {
                        insertIntoString.Append("NULL");
                    }
                    else
                    {
                        insertIntoString.AppendFormat("{0}", value.Value);
                    }
                }
                else if (column is SQLNChar)
                {
                    string value = (string)row;
                    if (value == null)
                    {
                        insertIntoString.Append("NULL");
                    }
                    else
                    {
                        insertIntoString.AppendFormat("'{0}'", value);
                    }
                }
                else if (column is SQLNVarChar)
                {
                    string value = (string)row;
                    if (value == null)
                    {
                        insertIntoString.Append("NULL");
                    }
                    else
                    {
                        insertIntoString.AppendFormat("'{0}'", value);
                    }
                }
                else if (column is SQLVarBinary)
                {
                    SQLVarBinary sql = (SQLVarBinary)column;
                    byte[] value = (byte[])row;

                    if (value == null)
                    {
                        insertIntoString.Append("NULL");
                    }
                    else
                    {
                        if (sql.Length == SQLVarBinary.MAX)
                        {
                            insertIntoString.AppendFormat(
                                "CAST('{0}' AS varbinary(max))",
                                Encoding.Default.GetString(value));
                        }
                        else
                        {
                            insertIntoString.AppendFormat(
                                "CAST('{0}' AS varbinary({1}))",
                                Encoding.Default.GetString(value), sql.Length);
                        }
                    }
                }
                else if (column is SQLVarChar)
                {
                    string value = (string)row;
                    if (value == null)
                    {
                        insertIntoString.Append("NULL");
                    }
                    else
                    {
                        insertIntoString.AppendFormat("'{0}'", value);
                    }
                }
                else if (column is SQLNText)
                {
                    string value = (string)row;
                    if (value == null)
                    {
                        insertIntoString.Append("NULL");
                    }
                    else
                    {
                        insertIntoString.AppendFormat("'{0}'", value);
                    }
                }
                else if (column is SQLText)
                {
                    string value = (string)row;
                    if (value == null)
                    {
                        insertIntoString.Append("NULL");
                    }
                    else
                    {
                        insertIntoString.AppendFormat("'{0}'", value);
                    }
                }
                else if (column is SQLXML)
                {
                    XmlDocument value = (XmlDocument)row;
                    if (value == null)
                    {
                        insertIntoString.Append("NULL");
                    }
                    else
                    {
                        insertIntoString.AppendFormat("'{0}'", value.DocumentElement.OuterXml);
                    }
                }
                else if (column is SQLFloat)
                {
                    if (row is float)
                    {
                        float? value = (float?)row;
                        if (!value.HasValue)
                        {
                            insertIntoString.Append("NULL");
                        }
                        else
                        {
                            insertIntoString.AppendFormat("{0}", value.Value);
                        }
                    }
                    else
                    {
                        //If we don't know, let's cast it to double
                        double? value = (double?)row;
                        if (!value.HasValue)
                        {
                            insertIntoString.Append("NULL");
                        }
                        else
                        {
                            insertIntoString.AppendFormat("{0}", value.Value);
                        }
                    }
                }
                else if (column is SQLReal)
                {
                    float? value = (float?)row;
                    if (!value.HasValue)
                    {
                        insertIntoString.Append("NULL");
                    }
                    else
                    {
                        insertIntoString.AppendFormat("{0}", value.Value);
                    }
                }
                else if (column is SQLUniqueIdentifier)
                {
                    Guid? value = (Guid?)row;
                    if (!value.HasValue)
                    {
                        insertIntoString.Append("NULL");
                    }
                    else
                    {
                        insertIntoString.AppendFormat("'{0}'", value.Value);
                    }
                }
                else if (column is SQLBigInt)
                {
                    long? value = (long?)row;
                    if (!value.HasValue)
                    {
                        insertIntoString.Append("NULL");
                    }
                    else
                    {
                        insertIntoString.AppendFormat("{0}", value.Value);
                    }
                }
                else if (column is SQLDateTime)
                {
                    DateTime? value = (DateTime?)row;
                    if (!value.HasValue)
                    {
                        insertIntoString.Append("NULL");
                    }
                    else
                    {
                        insertIntoString.AppendFormat("'{0}'", value.Value);
                    }
                }
                else if (column is SQLDateTime2)
                {
                    DateTime? value = (DateTime?)row;
                    if (!value.HasValue)
                    {
                        insertIntoString.Append("NULL");
                    }
                    else
                    {
                        insertIntoString.AppendFormat("'{0}'", value.Value);
                    }
                }
                else if (column is SQLDate)
                {
                    DateTime? value = (DateTime?)row;
                    if (!value.HasValue)
                    {
                        insertIntoString.Append("NULL");
                    }
                    else
                    {
                        insertIntoString.AppendFormat("'{0}'", value.Value);
                    }
                }
                else if (column is SQLTime)
                {
                    DateTime? value = (DateTime?)row;
                    if (!value.HasValue)
                    {
                        insertIntoString.Append("NULL");
                    }
                    else
                    {
                        insertIntoString.AppendFormat("'{0}'", value.Value);
                    }
                }
                else
                {
                    System.Diagnostics.Trace.Assert(false);
                }
            }

            insertIntoString.Append(")");

            return insertIntoString.ToString();
        }

        static private void SendSQLRequests(string createTableString, string insertIntoString)
        {
            string server = "localhost";
            string username = "sa";
            string password = "Password01";
            string database = "BCPTest";

            string connectionString = string.Format(
                "Data Source={0};User ID={1};Password={2};Initial Catalog={3}",
                server, username, password, database
            );

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand();
                command.Connection = connection;
                command.CommandText = "SELECT * FROM INFORMATION_SCHEMA.TABLES";
                ArrayList result = new ArrayList();
                IDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(reader.GetString(0));
                }
                reader.Close();
                reader.Dispose();

                command.CommandText = "IF OBJECT_ID('BCPTest','U') IS NOT NULL DROP TABLE BCPTest";
                command.ExecuteNonQuery();

                command.CommandText = createTableString;
                command.ExecuteNonQuery();

                command.CommandText = insertIntoString;
                command.ExecuteNonQuery();

                connection.Close();
            }
        }

        static private void GenerateBCPFileFromSQLServer(BinaryWriter writer)
        {
            //Default file name
            string baseFileName = "BCPTest.bcp";

            Stream stream = writer.BaseStream;
            if (stream is FileStream)
            {
                FileStream fileStream = (FileStream)stream;
                baseFileName = fileStream.Name;
            }

            string bcpFileName = string.Format("{0}.{1}", baseFileName, "BCPTest");

            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardError = true;
            startInfo.FileName = "bcp";
            startInfo.Arguments = "[BCPTest].[dbo].[BCPTest] out " + bcpFileName + " -S localhost -U sa -P Password01 -n";

            try
            {
                //Start the process with the info we specified
                //Call WaitForExit and then the using statement will close
                using (System.Diagnostics.Process process = System.Diagnostics.Process.Start(startInfo))
                {
                    string errorMessage = process.StandardError.ReadToEnd();
                    string outputMessage = process.StandardOutput.ReadToEnd();

                    process.WaitForExit();
                }
            }
            catch
            {
                //Log error
            }
        }
    }
}