using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Converters;

namespace SOSIEL.Entities
{
    /// <summary>
    /// Extended model for probability table.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ExtentedProbabilityRecord<T>
    {
        /// <summary>
        /// Gets or sets the specific value or batch number.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public T Value { get; set; }

        /// <summary>
        /// Gets or sets the probability.
        /// </summary>
        /// <value>
        /// The probability.
        /// </value>
        public double Probability { get; set; }

        /// <summary>
        /// Gets or sets the normalized probability.
        /// </summary>
        /// <value>
        /// The normalized probability.
        /// </value>
        public double NormalizedProbability { get; set; }

        /// <summary>
        /// Gets or sets the cumulative probability.
        /// </summary>
        /// <value>
        /// The cumulative probability.
        /// </value>
        public double CumulativeProbability { get; set; }


        public ExtentedProbabilityRecord(T value, double probability)
        {
            Value = value;
            Probability = probability;
        }
    }

    /// <summary>
    /// Extended probability table
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ExtendedProbabilityTable<T>
    {
        private Dictionary<T, ExtentedProbabilityRecord<T>> _table = new Dictionary<T, ExtentedProbabilityRecord<T>>();

        public ExtendedProbabilityTable(ProbabilityTable<T> prototype)
        {
            double sum = 0;

            foreach (T key in prototype.Keys.OrderBy(k => k))
            {
                double probability = prototype.GetProbability(key);

                sum += probability;

                ExtentedProbabilityRecord<T> newRecord = new ExtentedProbabilityRecord<T>(key, probability);
                _table[key] = newRecord;
            }

            double cumulative = 0;

            foreach (var tableKey in _table.Keys)
            {
                var record = _table[tableKey];

                record.NormalizedProbability = record.Probability / sum;

                cumulative += record.NormalizedProbability;
                record.CumulativeProbability = cumulative;
            }
        }

        /// <summary>
        /// Gets the value count.
        /// </summary>
        /// <value>
        /// The value count.
        /// </value>
        public int ValueCount
        {
            get { return _table.Count; }
        }

        /// <summary>
        /// Gets the probability for the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public double GetProbability(T value)
        {
            var record = GetExtentedProbabilityRecord(value);

            return record.Probability;
        }

        /// <summary>
        /// Gets the normalized probability for the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public double GetNormalizedProbability(T value)
        {
            var record = GetExtentedProbabilityRecord(value);

            return record.NormalizedProbability;
        }

        /// <summary>
        /// Gets the cumulative probability for the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public double GetCumulativeProbability(T value)
        {
            var record = GetExtentedProbabilityRecord(value);

            return record.CumulativeProbability;
        }

        /// <summary>
        /// Gets the value by cumulative probability.
        /// </summary>
        /// <param name="comulative">The comulative.</param>
        /// <returns></returns>
        public T GetValueByCumulative(double comulative)
        {
            foreach (var tableKey in _table.Keys)
            {
                var record = _table[tableKey];

                if (comulative <= record.CumulativeProbability)
                    return tableKey;
            }

            return default(T);
        }

        private ExtentedProbabilityRecord<T> GetExtentedProbabilityRecord(T value)
        {
            ExtentedProbabilityRecord<T> record;
            if (!_table.TryGetValue(value, out record))
                throw new ArgumentOutOfRangeException("Cannot find a probability for the value: " + value);

            return record;
        }
    }
}