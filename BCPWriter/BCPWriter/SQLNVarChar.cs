﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BCPWriter
{
    /// <summary>
    /// SQL nvarchar.
    /// </summary>
    /// 
    /// <remarks>
    /// <see cref="SQLNChar"/>
    /// <a href="http://msdn.microsoft.com/en-us/library/ms176089.aspx">char and varchar (Transact-SQL)</a><br/>
    /// </remarks>
    public class SQLNVarChar : IBCPSerialization
    {
        /// <summary>
        /// Minimum length allowed for SQL nvarchar.
        /// </summary>
        public const ushort MIN_LENGTH = SQLNChar.MIN_LENGTH;

        /// <summary>
        /// Maximum length allowed for SQL nvarchar.
        /// </summary>
        public const ushort MAX_LENGTH = SQLNChar.MAX_LENGTH;

        /// <summary>
        /// Maximum length allowed for SQL nvarchar
        /// </summary>
        /// <remarks>
        /// Use this in order to get a SQL nvarchar(max)
        /// </remarks>
        public const uint MAX = SQLInt.MAX_VALUE;

        /// <summary>
        /// Constructs a SQL nvarchar.
        /// </summary>
        /// <param name="length">
        /// length of n bytes, where n is a value from 1 through 4,000.
        /// The storage size is two times n bytes.
        /// length can also be MAX.
        /// </param>
        public SQLNVarChar(uint length)
        {
            if (length != MAX)
            {
                if (length < MIN_LENGTH || length > MAX_LENGTH)
                {
                    throw new ArgumentException("length should be between 1 and 4,000");
                }
            }

            Length = length;
        }

        /// <summary>
        /// SQL nvarchar length.
        /// </summary>
        public uint Length
        {
            get;
            private set;
        }

        public void Write(BinaryWriter writer, object value)
        {
            Write(writer, (string)value);
        }

        public void Write(BinaryWriter writer, string text)
        {
            if (text == null)
            {
                if (Length == MAX)
                {
                    //8 bytes long
                    byte[] nullBytes = { 255, 255, 255, 255, 255, 255, 255, 255 };
                    writer.Write(nullBytes);
                }
                else
                {
                    //2 bytes long
                    byte[] nullBytes = { 255, 255 };
                    writer.Write(nullBytes);
                }
                return;
            }

            if (text.Length > Length)
            {
                throw new ArgumentException("text is longer than the length declared inside the constructor");
            }

            if (Length == MAX)
            {
                //ulong is 8 bytes long
                //* 2 because we are in UTF-16, thus 1 char is 2 bytes long
                writer.Write((ulong)(text.Length * 2));
            }
            else
            {
                //ushort is 2 bytes long
                //* 2 because we are in UTF-16, thus 1 char is 2 bytes long
                writer.Write((ushort)(text.Length * 2));
            }

            //Text should be in unicode UTF-16
            writer.Write(Encoding.Unicode.GetBytes(text));
        }
    }
}
