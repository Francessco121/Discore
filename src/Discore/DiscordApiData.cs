﻿using Newtonsoft.Json;
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
    enum DiscordApiDataType
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
    /// A piece of data in the Discord API.
    /// </summary>
    class DiscordApiData
    {
        /// <summary>
        /// The type of data this DiscordApiData represents.
        /// </summary>
        public DiscordApiDataType Type { get; }

        /// <summary>
        /// If a container type, contains all properties in this DiscordApiData container.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this data is not a container.</exception>
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
        /// <exception cref="InvalidOperationException">Thrown if this data is not a value.</exception>
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
        /// <exception cref="InvalidOperationException">Thrown if this data is not an array.</exception>
        public IList<DiscordApiData> Values
        {
            get
            {
                AssertArray();
                return values;
            }
        }

        /// <summary>
        /// If a container type, gets all stored entries.
        /// </summary>
        public IEnumerable<KeyValuePair<string, DiscordApiData>> Entries
        {
            get
            {
                AssertContainer();
                return data;
            }
        }

        /// <summary>
        /// Gets whether the stored data is null.
        /// </summary>
        public bool IsNull
        {
            get { return Type == DiscordApiDataType.Value ? ReferenceEquals(value, null) : false; }
        }

        Dictionary<string, DiscordApiData> data;
        object value;
        IList<DiscordApiData> values;

        /// <summary>
        /// Creates a new DiscordApiData object of the specified type.
        /// </summary>
        /// <param name="type">The type of api data to store.</param>
        public DiscordApiData(DiscordApiDataType type)
        {
            Type = type;

            if (type == DiscordApiDataType.Array)
                values = new List<DiscordApiData>();
            else if (type == DiscordApiDataType.Container)
                data = new Dictionary<string, DiscordApiData>();
        }

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
        /// Creates a new value type DiscordApiData object.
        /// </summary>
        public DiscordApiData(Snowflake value)
        {
            this.value = value.ToString(); // Snowflakes are stored as strings in transit.
            Type = DiscordApiDataType.Value;
        }

        /// <summary>
        /// Creates a new array type DiscordApiData object.
        /// </summary>
        public DiscordApiData(IList<DiscordApiData> values)
        {
            this.values = values is Array ? new List<DiscordApiData>(values) : values;
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
        /// If a value type, returns this data as an uint64.
        /// </summary>
        public ulong? ToUInt64()
        {
            return value as ulong?;
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
        /// If a value type, returns this data as a unsigned 64-bit snowflake.
        /// </summary>
        /// <remarks>
        /// This is not the same as .ToUInt64 as the snowflake is originally
        /// stored as a string by the Discord Api.
        /// </remarks>
        public Snowflake? ToSnowflake()
        {
            string str = value as string;

            ulong snowflake;
            if (str != null && ulong.TryParse(str, out snowflake))
                return new Snowflake(snowflake);

            return null;
        }

        /// <summary>
        /// If a value type, returns this data as a string.
        /// Otherwise defaults to object.ToString().
        /// </summary>
        public override string ToString()
        {
            if (Type == DiscordApiDataType.Array)
                return $"DiscordApiData[{values.Count}]";
            else if (Type == DiscordApiDataType.Value && value == null)
                return "null";
            else
                return value?.ToString() ?? base.ToString();
        }
        #endregion

        /// <summary>
        /// If a container type, returns whether this container contains the given key.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this data is not a container.</exception>
        public bool ContainsKey(string key)
        {
            AssertContainer();
            return data.ContainsKey(key);
        }

        #region Get*
        /// <summary>
        /// If a container type, gets the api data at the given key.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this data is not a container.</exception>
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
        /// <exception cref="InvalidOperationException">Thrown if this data is not a container.</exception>
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
        /// <exception cref="InvalidOperationException">Thrown if this data is not a container.</exception>
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
        /// <exception cref="InvalidOperationException">Thrown if this data is not a container.</exception>
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
        /// <exception cref="InvalidOperationException">Thrown if this data is not a container.</exception>
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
        /// <exception cref="InvalidOperationException">Thrown if this data is not a container.</exception>
        public long? GetInt64(string key)
        {
            AssertContainer();

            DiscordApiData nestedData;
            if (data.TryGetValue(key, out nestedData))
                return nestedData.ToInt64();

            return null;
        }

        /// <summary>
        /// If a container type, gets the uint64 at the given key.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this data is not a container.</exception>
        public ulong? GetUInt64(string key)
        {
            AssertContainer();

            DiscordApiData nestedData;
            if (data.TryGetValue(key, out nestedData))
                return nestedData.ToUInt64();

            return null;
        }

        /// <summary>
        /// If a container type, gets the double floating-point number at the given key.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this data is not a container.</exception>
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
        /// <exception cref="InvalidOperationException">Thrown if this data is not a container.</exception>
        public DateTime? GetDateTime(string key)
        {
            AssertContainer();

            DiscordApiData nestedData;
            if (data.TryGetValue(key, out nestedData))
                return nestedData.ToDateTime();

            return null;
        }

        /// <summary>
        /// If a container type, gets the snowflake at the given key.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this data is not a container.</exception>
        public Snowflake? GetSnowflake(string key)
        {
            AssertContainer();

            DiscordApiData nestedData;
            if (data.TryGetValue(key, out nestedData))
                return nestedData.ToSnowflake();

            return null;
        }

        /// <summary>
        /// If a container type, gets the array at the given key.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this data is not a container.</exception>
        public IList<DiscordApiData> GetArray(string key)
        {
            AssertContainer();

            DiscordApiData nestedData;
            if (data.TryGetValue(key, out nestedData))
                return nestedData.values;

            return null;
        }

        /// <summary>
        /// If a container type, gets the DiscordColor at the given key.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this data is not a container.</exception>
        public DiscordColor? GetColor(string key)
        {
            AssertContainer();

            int? nestedData = GetInteger(key);
            if (nestedData.HasValue)
                return DiscordColor.FromHexadecimal(nestedData.Value);

            return null;
        }

        public DiscordImageData GetAvatar(string key)
        {
            AssertContainer();

            string nestedData = GetString(key);
            if (!string.IsNullOrWhiteSpace(nestedData))
                return DiscordImageData.FromDataUriScheme(nestedData);

            return null;
        }
        #endregion

        #region Set*
        /// <summary>
        /// Sets a value in this api data container.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this data is not a container.</exception>
        public DiscordApiData Set(string key, object value)
        {
            AssertContainer();

            DiscordApiData apiValue = new DiscordApiData(value);
            data[key] = apiValue;
            return apiValue;
        }

        [Obsolete("Use SetSnowflake instead.", true)]
        public DiscordApiData Set(string key, Snowflake snowflake)
        {
            throw new NotSupportedException();
        }

        [Obsolete("Use SetSnowflake instead.", true)]
        public DiscordApiData Set(string key, Snowflake? snowflake)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Sets a snowflake value in this api data container.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this data is not a container.</exception>
        public DiscordApiData SetSnowflake(string key, Snowflake snowflake)
        {
            AssertContainer();

            DiscordApiData apiValue = new DiscordApiData(snowflake);
            data[key] = apiValue;
            return apiValue;
        }

        /// <summary>
        /// Sets a snowflake value in this api data container.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this data is not a container.</exception>
        public DiscordApiData SetSnowflake(string key, Snowflake? snowflake)
        {
            AssertContainer();

            DiscordApiData apiValue = snowflake.HasValue ? new DiscordApiData(snowflake.Value) : new DiscordApiData(value: null);
            data[key] = apiValue;
            return apiValue;
        }

        /// <summary>
        /// Sets a value in this api data container.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this data is not a container.</exception>
        public DiscordApiData Set(string key, DiscordApiData apiData)
        {
            AssertContainer();

            data[key] = apiData ?? new DiscordApiData(apiData);
            return apiData;
        }

        /// <summary>
        /// Sets a DiscordColor value in this api data container.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this data is not a container.</exception>
        public DiscordApiData Set(string key, DiscordColor color)
        {
            AssertContainer();

            DiscordApiData apiData = new DiscordApiData(color.ToHexadecimal());
            data[key] = apiData;
            return apiData;
        }

        /// <summary>
        /// Sets a DiscordColor value in this api data container.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this data is not a container.</exception>
        public DiscordApiData Set(string key, DiscordColor? color)
        {
            AssertContainer();

            DiscordApiData apiData = color.HasValue ? new DiscordApiData(color.Value) : new DiscordApiData(value: null);
            data[key] = apiData;
            return apiData;
        }

        public DiscordApiData Set(string key, DateTime dateTime)
        {
            AssertContainer();

            DiscordApiData apiData = new DiscordApiData(dateTime);
            data[key] = apiData;
            return apiData;
        }

        public DiscordApiData Set(string key, DiscordImageData avatar)
        {
            AssertContainer();

            DiscordApiData apiData = new DiscordApiData(avatar.ToDataUriScheme());
            data[key] = apiData;
            return apiData;
        }

        /// <summary>
        /// Sets a value in this api data container.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this data is not a container.</exception>
        public DiscordApiData Set<T>(string key, IList<T> array)
        {
            AssertContainer();

            List<DiscordApiData> dataList = new List<DiscordApiData>();
            for (int i = 0; i < array.Count; i++)
            {
                // Small trick to choose the correct constructor
                T value = array[i];
                if (value is Snowflake snowflake)
                    dataList.Add(new DiscordApiData(snowflake));
                else
                    dataList.Add(new DiscordApiData(value));
            }

            DiscordApiData arrayValue = new DiscordApiData(dataList);
            data[key] = arrayValue;
            return arrayValue;
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
        /// attempts to locate the uint64 at the end of the given path.
        /// </summary>
        /// <see cref="Locate(string)"/>
        /// <param name="path">A dot seperated path to the uint64.</param>
        public ulong? LocateUInt64(string path)
        {
            DiscordApiData data = Locate(path);
            return data?.ToUInt64();
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
        /// attempts to locate the snowflake at the end of the given path.
        /// </summary>
        /// <see cref="Locate(string)"/>
        /// <param name="path">A dot seperated path to the snowflake.</param>
        public Snowflake? LocateSnowflake(string path)
        {
            DiscordApiData data = Locate(path);
            return data?.ToSnowflake();
        }

        /// <summary>
        /// If a container type,
        /// attempts to locate the array at the end of the given path.
        /// </summary>
        /// <see cref="Locate(string)"/>
        /// <param name="path">A dot seperated path to the array.</param>
        public IList<DiscordApiData> LocateArray(string path)
        {
            DiscordApiData data = Locate(path);
            return data != null ? data.values : null;
        }
        #endregion

        /// <summary>
        /// Updates this data with the specified data, overwriting any conflicts with the new data.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this data or the given data is not a container.</exception>
        public void OverwriteUpdate(DiscordApiData data)
        {
            AssertContainer();
            data.AssertContainer();

            foreach (KeyValuePair<string, DiscordApiData> pair in data.Entries)
                this.data[pair.Key] = pair.Value;
        }

        public DiscordApiData Clone()
        {
            return ParseJson(SerializeToJson());
        }

        /// <summary>
        /// Creates a new value-type <see cref="DiscordApiData"/>.
        /// </summary>
        public static DiscordApiData CreateValue()
        {
            return new DiscordApiData(DiscordApiDataType.Value);
        }

        /// <summary>
        /// Creates a new container-type <see cref="DiscordApiData"/>.
        /// </summary>
        public static DiscordApiData CreateContainer()
        {
            return new DiscordApiData(DiscordApiDataType.Container);
        }

        /// <summary>
        /// Creates a new array-type <see cref="DiscordApiData"/>.
        /// </summary>
        public static DiscordApiData CreateArray()
        {
            return new DiscordApiData(DiscordApiDataType.Array);
        }

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

        void ApiDataArrayToJson(IList<DiscordApiData> apiDataArray, JsonTextWriter writer)
        {
            writer.WriteStartArray();

            for (int i = 0; i < apiDataArray.Count; i++)
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
        /// Attempts to create a DiscordApiData object from a JSON string.
        /// </summary>
        /// <returns>Returns whether the conversion was successful.</returns>
        public static bool TryParseJson(string json, out DiscordApiData data)
        {
            try
            {
                data = ParseJson(json);
                return true;
            }
            catch (Exception)
            {
                data = null;
                return false;
            }
        }

        /// <summary>
        /// Creates a DiscordApiData object from a JSON string.
        /// </summary>
        public static DiscordApiData ParseJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new DiscordApiData(value: null);

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

                    JArrayToApiDataArray(dataArray, array);
                    apiData.data[key] = new DiscordApiData(dataArray);
                }
                else
                    apiData.data[key] = new DiscordApiData(token.ToObject<object>());
            }
        }

        static void JArrayToApiDataArray(IList<DiscordApiData> apiDataArray, JArray array)
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
