using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Discore.Http
{
    /// <summary>
    /// Represents a set of form errors for a Discord HTTP API request.
    /// <para/>
    /// Will be either a <see cref="DiscordHttpErrorObject"/>, <see cref="DiscordHttpErrorArray"/>, 
    /// or <see cref="DiscordHttpErrorField"/>.
    /// </summary>
    public abstract class DiscordHttpError 
    {
        internal DiscordHttpError() { }

        internal static bool TryParse(JsonElement json, [NotNullWhen(true)] out DiscordHttpError? error)
        {
            if (json.ValueKind == JsonValueKind.Object)
            {
                if (json.HasProperty("_errors"))
                {
                    // Field
                    error = new DiscordHttpErrorField(json);
                    return true;
                }
                else
                {
                    // Get first property
                    JsonElement.ObjectEnumerator enumerator = json.EnumerateObject();
                    if (enumerator.MoveNext())
                    {
                        // Check if this is an array error by checking if the property name is an integer index
                        if (int.TryParse(enumerator.Current.Name, out _))
                        {
                            // Array
                            error = new DiscordHttpErrorArray(json);
                            return true;
                        }
                        else
                        {
                            // Object
                            error = new DiscordHttpErrorObject(json);
                            return true;
                        }
                    }
                }
            }

            error = null;
            return false;
        }
    }

    /// <summary>
    /// Contains errors for a form object.
    /// </summary>
    public class DiscordHttpErrorObject : DiscordHttpError
    {
        /// <summary>
        /// Gets the errors for each form field that had an error.
        /// </summary>
        public IReadOnlyDictionary<string, DiscordHttpError> Fields { get; }

        internal DiscordHttpErrorObject(JsonElement json)
        {
            var fields = new Dictionary<string, DiscordHttpError>();

            foreach (JsonProperty property in json.EnumerateObject())
            {
                if (DiscordHttpError.TryParse(property.Value, out DiscordHttpError? error))
                    fields[property.Name] = error;
            }

            Fields = fields;
        }
    }

    /// <summary>
    /// Contains errors for a form array.
    /// </summary>
    public class DiscordHttpErrorArray : DiscordHttpError
    {
        /// <summary>
        /// Gets the errors for each element that had an error.
        /// </summary>
        public IReadOnlyDictionary<int, DiscordHttpError> Elements { get; }

        internal DiscordHttpErrorArray(JsonElement json)
        {
            var elements = new Dictionary<int, DiscordHttpError>();

            foreach (JsonProperty property in json.EnumerateObject())
            {
                if (int.TryParse(property.Name, out int index))
                {
                    if (DiscordHttpError.TryParse(property.Value, out DiscordHttpError? error))
                        elements[index] = error;
                }
            }

            Elements = elements;
        }
    }

    /// <summary>
    /// Contains the error messages and error codes for a specific form field.
    /// </summary>
    public class DiscordHttpErrorField : DiscordHttpError
    {
        /// <summary>
        /// Gets the error messages and error codes for this field.
        /// </summary>
        public IReadOnlyList<DiscordHttpErrorMessage> Errors { get; }

        internal DiscordHttpErrorField(JsonElement json)
        {
            JsonElement? errorsJson = json.GetPropertyOrNull("_errors");

            if (errorsJson != null)
            {
                JsonElement _errorsJson = errorsJson.Value;

                var errors = new DiscordHttpErrorMessage[_errorsJson.GetArrayLength()];
                for (int i = 0; i < errors.Length; i++)
                {
                    errors[i] = new DiscordHttpErrorMessage(_errorsJson[i]);
                }

                Errors = errors;
            }
            else
            {
                Errors = Array.Empty<DiscordHttpErrorMessage>();
            }
        }
    }

    /// <summary>
    /// Contains an error message and error code for a form field. 
    /// </summary>
    public class DiscordHttpErrorMessage
    {
        /// <summary>
        /// A code representing the type of error.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// A message describing the error.
        /// </summary>
        public string Message { get; }

        internal DiscordHttpErrorMessage(JsonElement json)
        {
            Code = json.GetPropertyOrNull("code")?.GetString() ?? "";
            Message = json.GetPropertyOrNull("message")?.GetString() ?? "";
        }
    }
}
