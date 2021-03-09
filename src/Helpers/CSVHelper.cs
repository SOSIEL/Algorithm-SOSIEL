/// Name: CSVHelper.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace SOSIEL.Helpers
{
    class EnumerableConverter<T> : DefaultTypeConverter
    {
        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            return string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, value as IEnumerable<T>);
        }
    }

    /// <summary>
    /// Helper class for working with CSV files.
    /// </summary>
    public static class CSVHelper
    {
        private static readonly EnumerableConverter<string> _enumerableConverter = new EnumerableConverter<string>();

        /// <summary>
        /// Reads all records in file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath">The file path.</param>
        /// <param name="headerRecortExist">if set to <c>true</c> [header recort exist].</param>
        /// <param name="mappingClass">The mapping class.</param>
        /// <returns></returns>
        public static List<T> ReadAllRecords<T>(string filePath, bool hasHeader = false, Type mappingClass = null)
        {
            var configuration = createConfiguration();
            if (mappingClass != null)
                configuration.RegisterClassMap(mappingClass);

            using (StreamReader sr = new StreamReader(filePath))
            using (CsvReader csv = new CsvReader(sr, configuration))
            {
                if (hasHeader) csv.Read(); // manually skip header line
                return csv.GetRecords<T>().ToList();
            }
        }

        /// <summary>
        /// Appends record to the file end or creates new file and writes record there.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <param name="record"></param>
        public static void AppendTo<T>(string filePath, T record)
        {
            var isFileExist = File.Exists(filePath);
            var configuration = createConfiguration();

            using (FileStream fs = File.Open(filePath, isFileExist ? FileMode.Append : FileMode.Create))
            using (StreamWriter sw = new StreamWriter(fs))
            using (CsvWriter csv = new CsvWriter(sw, configuration))
            {
                if (!isFileExist)
                {
                    configuration.HasHeaderRecord = true;
                    csv.WriteHeader<T>();
                    csv.NextRecord();
                }
                csv.WriteRecord(record);
                csv.NextRecord();
            }
        }

        /// <summary>
        /// Appends records to the file end or creates new file and writes records there.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <param name="records"></param>
        public static void AppendTo<T>(string filePath, IEnumerable<T> records)
        {
            var isFileExist = File.Exists(filePath);
            var configuration = createConfiguration();
            configuration.HasHeaderRecord = !isFileExist;

            using (FileStream fs = File.Open(filePath, isFileExist ? FileMode.Append : FileMode.Create))
            using (StreamWriter sw = new StreamWriter(fs))
            using (CsvWriter csv = new CsvWriter(sw, configuration))
            {
                csv.WriteRecords(records);
            }
        }

        private static CsvHelper.Configuration.Configuration createConfiguration()
        {
            var config = new CsvHelper.Configuration.Configuration(CultureInfo.InvariantCulture);
            config.HasHeaderRecord = false;
            config.TypeConverterCache.AddConverter<string[]>(_enumerableConverter);
            return config;
        }
    }
}
