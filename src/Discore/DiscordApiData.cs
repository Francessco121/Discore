using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace Discore
{
    /// <summary>
    /// A value representing the type of value a DiscordApiData object represents.
    /// </summary>
    public enum DiscordApiDataType
    {
        /// <summary>
        /// The DiscordApiData represents a single value.
        /// </summary>
        Value,
        /// <summary>
        /// The DiscordApiData represents an array of DiscordApiData objects.
        /// </summary>
        Array,
        /// <summary>
        /// The DiscordApiData represents a container for nested DiscordApiData objects.
        /// </summary>
        Container
    }

    /// <summary>
    /// A piece of data in the discord api.
    /// </summary>
    public class DiscordApiData
    {
        /// <summary>
        /// The type of data this DiscordApiData represents.
        /// </summary>
        public DiscordApiDataType Type { get; }

        /// <summary>
        /// If a container type, contains all properties in this DiscordApiData container.
        /// </summary>
        public IReadOnlyDictionary<string, DiscordApiData> Properties
        {
            get
            {
                AssertContainer();
                return new ReadOnlyDictionary<string, DiscordApiData>(data);
            }
        }

        /// <summary>
        /// If a value type, contains the stored value of this DiscordApiData.
        /// </summary>
        public object Value
        {
            get
            {
                AssertValue();
                return value;
            }
        }

        /// <summary>
        /// If an array type, contains the stored list of DiscordApiData objects.
        /// </summary>
        public IReadOnlyList<DiscordApiData> Values
        {
            get
            {
                AssertArray();
                return values;
            }
        }

        Dictionary<string, DiscordApiData> data;
        object value;
        DiscordApiData[] values;

        /// <summary>
        /// Creates a new container type DiscordApiData object.
        /// </summary>
        public DiscordApiData()
        {
            data = new Dictionary<string, DiscordApiData>();
            Type = DiscordApiDataType.Container;
        }

        /// <summary>
        /// Creates a new value type DiscordApiData object.
        /// </summary>
        public DiscordApiData(object value)
        {
            this.value = value;
            Type = DiscordApiDataType.Value;
        }

        /// <summary>
        /// Creates a new array type DiscordApiData object.
        /// </summary>
        public DiscordApiData(DiscordApiData[] values)
        {
            this.values = values;
            Type = DiscordApiDataType.Array;
        }

        void AssertContainer()
        {
            if (Type != DiscordApiDataType.Container)
                throw new InvalidOperationException("This DiscordApiData is not a container!");
        }

        void AssertValue()
        {
            if (Type != DiscordApiDataType.Value)
                throw new InvalidOperationException("This DiscordApiData is not a value!");
        }

        void AssertArray()
        {
            if (Type != DiscordApiDataType.Array)
                throw new InvalidOperationException("This DiscordApiData is not an array!");
        }

        #region To*
        /// <summary>
        /// If a value type, returns this data as a boolean.
        /// </summary>
        public bool? ToBoolean()
        {
            return value as bool?;
        }

        /// <summary>
        /// If a value type, returns this data as an integer.
        /// </summary>
        public int? ToInteger()
        {
            long? v = value as long?;
            if (v.HasValue)
                return (int)v.Value;
            else
                return null;
        }

        /// <summary>
        /// If a value type, returns this data as an int64.
        /// </summary>
        public long? ToInt64()
        {
            return value as long?;
        }

        /// <summary>
        /// If a value type, returns this data as a double floating-point number.
        /// </summary>
        public double? ToDouble()
        {
            return value as double?;
        }

        /// <summary>
        /// If a value type, returns this data as a datetime object.
        /// </summary>
        public DateTime? ToDateTime()
        {
            return value as DateTime?;
        }

        /// <summary>
        /// If a value type, returns this data as a string.
        /// Otherwise defaults to object.ToString().
        /// </summary>
        public override string ToString()
        {
            return value?.ToString() ?? base.ToString();
        }
        #endregion

        /// <summary>
        /// If a container type, returns whether this container contains the given key.
        /// </summary>
        public bool ContainsKey(string key)
        {
            AssertContainer();
            return data.ContainsKey(key);
        }

        #region Get*
        /// <summary>
        /// If a container type, gets the api data at the given key.
        /// </summary>
        public DiscordApiData Get(string key)
        {
            AssertContainer();

            DiscordApiData nestedData;
            if (data.TryGetValue(key, out nestedData))
                return nestedData;

            return null;
        }

        /// <summary>
        /// If a container type, gets the object at the given key.
        /// </summary>
        public object GetObject(string key)
        {
            AssertContainer();

            DiscordApiData nestedData;
            if (data.TryGetValue(key, out nestedData))
                return nestedData.value;

            return null;
        }

        /// <summary>
        /// If a container type, gets the string at the given key.
        /// </summary>
        public string GetString(string key)
        {
            AssertContainer();

            DiscordApiData nestedData;
            if (data.TryGetValue(key, out nestedData))
                return nestedData.value as string;

            return null;
        }

        /// <summary>
        /// If a container type, gets the boolean at the given key.
        /// </summary>
        public bool? GetBoolean(string key)
        {
            AssertContainer();

            DiscordApiData nestedData;
            if (data.TryGetValue(key, out nestedData))
                return nestedData.ToBoolean();

            return null;
        }

        /// <summary>
        /// If a container type, gets the integer at the given key.
        /// </summary>
        public int? GetInteger(string key)
        {
            AssertContainer();

            DiscordApiData nestedData;
            if (data.TryGetValue(key, out nestedData))
                return nestedData.ToInteger();

            return null;
        }

        /// <summary>
        /// If a container type, gets the int64 at the given key.
        /// </summary>
        public long? GetInt64(string key)
        {
            AssertContainer();

            DiscordApiData nestedData;
            if (data.TryGetValue(key, out nestedData))
                return nestedData.ToInt64();

            return null;
        }

        /// <summary>
        /// If a container type, gets the double floating-point number at the given key.
        /// </summary>
        public double? GetDouble(string key)
        {
            AssertContainer();

            DiscordApiData nestedData;
            if (data.TryGetValue(key, out nestedData))
                return nestedData.ToDouble();

            return null;
        }

        /// <summary>
        /// If a container type, gets the datetime at the given key.
        /// </summary>
        public DateTime? GetDateTime(string key)
        {
            AssertContainer();

            DiscordApiData nestedData;
            if (data.TryGetValue(key, out nestedData))
                return nestedData.ToDateTime();

            return null;
        }

        /// <summary>
        /// If a container type, gets the array at the given key.
        /// </summary>
        public IReadOnlyList<DiscordApiData> GetArray(string key)
        {
            AssertContainer();

            DiscordApiData nestedData;
            if (data.TryGetValue(key, out nestedData))
                return nestedData.values;

            return null;
        }
        #endregion

        #region Set*
        /// <summary>
        /// Sets a value in this api data container.
        /// </summary>
        public DiscordApiData Set(string key, object value)
        {
            if (Type != DiscordApiDataType.Container)
                throw new InvalidOperationException("This DiscordApiData is not a container!");

            DiscordApiData apiValue = new DiscordApiData(value);
            data[key] = apiValue;
            return apiValue;
        }

        /// <summary>
        /// Sets a value in this api data container.
        /// </summary>
        public DiscordApiData Set(string key, DiscordApiData value)
        {
            if (Type != DiscordApiDataType.Container)
                throw new InvalidOperationException("This DiscordApiData is not a container!");

            data[key] = value ?? new DiscordApiData(value);
            return value;
        }

        /// <summary>
        /// Sets a value in this api data container.
        /// </summary>
        public DiscordApiData Set<T>(string key, T[] array)
        {
            if (Type != DiscordApiDataType.Container)
                throw new InvalidOperationException("This DiscordApiData is not a container!");

            DiscordApiData[] dataArray = new DiscordApiData[array.Length];
            for (int i = 0; i < array.Length; i++)
                dataArray[i] = new DiscordApiData(array[i]);

            DiscordApiData arrayValue = new DiscordApiData(dataArray);
            data[key] = arrayValue;
            return arrayValue;
        }

        /// <summary>
        /// Creates a nested DiscordApiData that can hold its own data.
        /// </summary>
        public DiscordApiData CreateNestedContainer(string key)
        {
            if (Type != DiscordApiDataType.Container)
                throw new InvalidOperationException("This DiscordApiData is not a container!");

            DiscordApiData nestedData = new DiscordApiData();
            data[key] = nestedData;
            return nestedData;
        }

        /// <summary>
        /// Sets a value at the end of the given path.
        /// The path will be created if it does not exist.
        /// </summary>
        public void SetAt(string path, object value)
        {
            string[] parts = path.Split('.');

            DiscordApiData node = this;
            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];

                if (i == parts.Length - 1)
                    node.data[part] = new DiscordApiData(value);
                else
                {
                    DiscordApiData nestedData = null;
                    if (node.data.TryGetValue(part, out nestedData))
                    {
                        if (nestedData.Type == DiscordApiDataType.Container)
                        {
                            node = nestedData;
                            continue;
                        }
                    }

                    DiscordApiData newNode = new DiscordApiData(new Dictionary<string, object>());
                    node.data[part] = newNode;
                    node = newNode;
                }
            }
        }
        #endregion

        #region Locate*
        /// <summary>
        /// If a container type,
        /// attempts to locate the api data at the end of the given path.
        /// <para>
        /// For example:
        /// Locate("someContainer.someOtherContainer.someValue")
        /// would retrieve the DiscordApiData at "someValue" inside of the container "someOtherContainer"
        /// if it exists.
        /// </para>
        /// </summary>
        /// <param name="path">A dot seperated path to the value.</param>
        public DiscordApiData Locate(string path)
        {
            string[] parts = path.Split('.');

            DiscordApiData node = this;
            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];

                DiscordApiData nestedData = null;
                if (node.data.TryGetValue(part, out nestedData))
                {
                    if (i == parts.Length - 1)
                        return nestedData;

                    if (node.Type != DiscordApiDataType.Container)
                        break;

                    node = nestedData;
                }
                else
                    break;
            }

            return null;
        }

        /// <summary>
        /// If a container type,
        /// attempts to locate the string at the end of the given path.
        /// </summary>
        /// <see cref="Locate(string)"/>
        /// <param name="path">A dot seperated path to the string.</param>
        public string LocateString(string path)
        {
            DiscordApiData data = Locate(path);
            return data?.ToString();
        }

        /// <summary>
        /// If a container type,
        /// attempts to locate the boolean at the end of the given path.
        /// </summary>
        /// <see cref="Locate(string)"/>
        /// <param name="path">A dot seperated path to the boolean.</param>
        public bool? LocateBoolean(string path)
        {
            DiscordApiData data = Locate(path);
            return data?.ToBoolean();
        }

        /// <summary>
        /// If a container type,
        /// attempts to locate the integer at the end of the given path.
        /// </summary>
        /// <see cref="Locate(string)"/>
        /// <param name="path">A dot seperated path to the integer.</param>
        public int? LocateInteger(string path)
        {
            DiscordApiData data = Locate(path);
            return data?.ToInteger();
        }

        /// <summary>
        /// If a container type,
        /// attempts to locate the int64 at the end of the given path.
        /// </summary>
        /// <see cref="Locate(string)"/>
        /// <param name="path">A dot seperated path to the int64.</param>
        public long? LocateInt64(string path)
        {
            DiscordApiData data = Locate(path);
            return data?.ToInt64();
        }

        /// <summary>
        /// If a container type,
        /// attempts to locate the double floating-point number at the end of the given path.
        /// </summary>
        /// <see cref="Locate(string)"/>
        /// <param name="path">A dot seperated path to the double floating-point number.</param>
        public double? LocateDouble(string path)
        {
            DiscordApiData data = Locate(path);
            return data?.ToDouble();
        }

        /// <summary>
        /// If a container type,
        /// attempts to locate the datetime at the end of the given path.
        /// </summary>
        /// <see cref="Locate(string)"/>
        /// <param name="path">A dot seperated path to the datetime.</param>
        public DateTime? LocateDateTime(string path)
        {
            DiscordApiData data = Locate(path);
            return data?.ToDateTime();
        }

        /// <summary>
        /// If a container type,
        /// attempts to locate the array at the end of the given path.
        /// </summary>
        /// <see cref="Locate(string)"/>
        /// <param name="path">A dot seperated path to the array.</param>
        public DiscordApiData[] LocateArray(string path)
        {
            DiscordApiData data = Locate(path);
            return data != null ? data.values : null;
        }
        #endregion

        /// <summary>
        /// Serializes this api data object to a JSON string.
        /// </summary>
        public string SerializeToJson()
        {
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            using (JsonTextWriter writer = new JsonTextWriter(sw))
            {
                if (Type == DiscordApiDataType.Container)
                    ApiDataContainerToJson(this, writer);
                else if (Type == DiscordApiDataType.Array)
                    ApiDataArrayToJson(values, writer);
                else
                    ApiDataToJson(this, writer);
            }

            return sb.ToString();
        }

        void ApiDataContainerToJson(DiscordApiData apiData, JsonTextWriter writer)
        {
            writer.WriteStartObject();

            foreach (KeyValuePair<string, DiscordApiData> pair in apiData.data)
            {
                string key = pair.Key;
                DiscordApiData data = pair.Value;

                writer.WritePropertyName(key);
                ApiDataToJson(data, writer);
            }

            writer.WriteEndObject();
        }

        void ApiDataArrayToJson(DiscordApiData[] apiDataArray, JsonTextWriter writer)
        {
            writer.WriteStartArray();

            for (int i = 0; i < apiDataArray.Length; i++)
            {
                DiscordApiData apiData = apiDataArray[i];
                ApiDataToJson(apiData, writer);
            }

            writer.WriteEndArray();
        }

        void ApiDataToJson(DiscordApiData data, JsonTextWriter writer)
        {
            if (data.Type == DiscordApiDataType.Container)
                ApiDataContainerToJson(data, writer);
            else if (data.Type == DiscordApiDataType.Array)
                ApiDataArrayToJson(data.values, writer);
            else
                writer.WriteValue(data.value);
        }

        #region Conversions
        /// <summary>
        /// Creates a DiscordApiData object from a JSON string.
        /// </summary>
        public static DiscordApiData FromJson(string json)
        {
            JToken jToken = JsonConvert.DeserializeObject<JToken>(json);

            DiscordApiData data;
            if (jToken.Type == JTokenType.Object)
            {
                data = new DiscordApiData();
                JObjectToApiData(data, (JObject)jToken);
            }
            else if (jToken.Type == JTokenType.Array)
            {
                JArray array = (JArray)jToken;
                DiscordApiData[] dataArray = new DiscordApiData[array.Count];
                JArrayToApiDataArray(dataArray, array);

                data = new DiscordApiData(dataArray);
            }
            else
                data = new DiscordApiData(jToken.ToObject<object>());

            return data;
        }

        static void JObjectToApiData(DiscordApiData apiData, JObject obj)
        {
            foreach (KeyValuePair<string, JToken> pair in obj)
            {
                string key = pair.Key;
                JToken token = pair.Value;

                if (token.Type == JTokenType.None || token.Type == JTokenType.Comment)
                    continue;

                if (token.Type == JTokenType.Object)
                {
                    DiscordApiData newData = new DiscordApiData();
                    apiData.data[key] = newData;

                    JObjectToApiData(newData, (JObject)token);
                }
                else if (token.Type == JTokenType.Array)
                {
                    JArray array = (JArray)token;
                    DiscordApiData[] dataArray = new DiscordApiData[array.Count];

                    apiData.data[key] = new DiscordApiData(dataArray);
                    JArrayToApiDataArray(dataArray, array);
                }
                else
                    apiData.data[key] = new DiscordApiData(token.ToObject<object>());
            }
        }

        static void JArrayToApiDataArray(DiscordApiData[] apiDataArray, JArray array)
        {
            for (int i = 0; i < array.Count; i++)
            {
                JToken token = array[i];

                if (token.Type == JTokenType.None || token.Type == JTokenType.Comment)
                    continue;

                if (token.Type == JTokenType.Object)
                {
                    DiscordApiData newData = new DiscordApiData();
                    apiDataArray[i] = newData;

                    JObjectToApiData(newData, (JObject)token);
                }
                else if (token.Type == JTokenType.Array)
                {
                    JArray nestedArray = (JArray)token;
                    DiscordApiData[] dataArray = new DiscordApiData[nestedArray.Count];

                    JArrayToApiDataArray(dataArray, nestedArray);
                    apiDataArray[i] = new DiscordApiData(dataArray);
                }
                else
                    apiDataArray[i] = new DiscordApiData(token.ToObject<object>());
            }
        }
        #endregion
    }
}
