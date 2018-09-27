using System;
using System.Collections.Generic;

using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;


namespace Common.Helpers
{
    class EnumerableConverter<T> : DefaultTypeConverter
    {
        public override string ConvertToString(TypeConverterOptions options, object value)
        {
            IEnumerable<T> values = value as IEnumerable<T>;

            return string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, values);
        }
    }

    /// <summary>
    /// Helper class for working with CSV files.
    /// </summary>
    public static class CSVHelper
    {
        private static CsvConfiguration GetDefaultConfiguration()
        {
            return new CsvConfiguration() { CultureInfo = CultureInfo.InvariantCulture, HasHeaderRecord = false };
        }

        static CSVHelper()
        {
            TypeConverterFactory.AddConverter<string[]>(new EnumerableConverter<string>());
        }

        /// <summary>
        /// Reads all records in file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath">The file path.</param>
        /// <param name="headerRecortExist">if set to <c>true</c> [header recort exist].</param>
        /// <param name="mappingClass">The mapping class.</param>
        /// <returns></returns>
        public static List<T> ReadAllRecords<T>(string filePath, bool headerRecortExist = false, Type mappingClass = null)
        {
            var configuration = GetDefaultConfiguration();

            if (mappingClass != null)
            {
                configuration.RegisterClassMap(mappingClass);
            }

            using (FileStream fs = File.Open(filePath, FileMode.Open))
            using (StreamReader sr = new StreamReader(fs))
            using (CsvReader csv = new CsvReader(sr, configuration))
            {
                csv.Read();

                var records = csv.GetRecords<T>();
                return records.ToList();
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
            var configuration = GetDefaultConfiguration();
            var isFileExist = File.Exists(filePath);

            using (FileStream fs = File.Open(filePath, isFileExist ? FileMode.Append : FileMode.Create))
            using (StreamWriter sw = new StreamWriter(fs))
            using (CsvWriter csv = new CsvWriter(sw, configuration))
            {
                if (!isFileExist)
                {
                    configuration.HasHeaderRecord = true;
                    csv.WriteHeader<T>();
                    configuration.HasHeaderRecord = false;
                }

                csv.WriteRecord(record);
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
            var configuration = GetDefaultConfiguration();
            var isFileExist = File.Exists(filePath);

            using (FileStream fs = File.Open(filePath, isFileExist ? FileMode.Append : FileMode.Create))
            using (StreamWriter sw = new StreamWriter(fs))
            using (CsvWriter csv = new CsvWriter(sw, configuration))
            {
                //it writes header by default
                if (!isFileExist)
                {
                    configuration.HasHeaderRecord = true;
                }
                else
                {
                    configuration.HasHeaderRecord = false;
                }

                csv.WriteRecords(records);
            }
        }


    }
}
