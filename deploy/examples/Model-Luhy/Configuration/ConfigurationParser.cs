using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace ModelLuhy.Configuration
{
    static class MemberInfoExtensions
    {
        internal static bool IsPropertyWithSetter(this MemberInfo member)
        {
            var property = member as PropertyInfo;

            return property?.GetSetMethod(true) != null;
        }
    }

    public static class ConfigurationParser
    {
        

        /// <summary>
        /// Contract resolver for setting properties with private set part. 
        /// </summary>
        private class PrivateSetterContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var jProperty = base.CreateProperty(member, memberSerialization);
                if (jProperty.Writable)
                    return jProperty;

                jProperty.Writable = member.IsPropertyWithSetter();

                return jProperty;
            }
        }


        /// <summary>
        /// Converter for casting integer numbers to int instead of decimal.
        /// </summary>
        private class IntConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return (objectType == typeof(int) || objectType == typeof(object));
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Integer)
                {
                    return Convert.ToInt32((object) reader.Value);
                }

                return reader.Value;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }


        static JsonSerializer serializer;

        static ConfigurationParser()
        {
            serializer = new JsonSerializer();

            serializer.Converters.Add(new IntConverter());
            serializer.ContractResolver = new PrivateSetterContractResolver();
        }

        /// <summary>
        /// Parses all configuration file.
        /// </summary>
        /// <param name="jsonPath"></param>
        /// <returns></returns>
        public static ConfigurationModel ParseConfiguration(string jsonPath)
        {
            if (File.Exists(jsonPath) == false)
                throw new FileNotFoundException(string.Format("Configuration file doesn't found at {0}", jsonPath));

            string jsonContent = File.ReadAllText(jsonPath);

            JToken json = JToken.Parse(jsonContent);

            return json.ToObject<ConfigurationModel>(serializer);
        }
    }


}
