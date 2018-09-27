using CsvHelper.Configuration;

namespace SOSIEL_EX1.Models
{
    /// <summary>
    /// Model for parsing probability table file.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ProbabilityRecord<T>
    {
        public T Value { get; set; }

        public double Probability { get; set; }
    }

    public sealed class ProbabilityRecordMap<T> : CsvClassMap<ProbabilityRecord<T>>
    {
        public ProbabilityRecordMap()
        {
            Map(m => m.Value).Index(0);
            Map(m => m.Probability).Index(1);
        }
    }
}