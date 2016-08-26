using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Discore.Secrets
{
    public class SecretsManager : IDisposable
    {
        const string FILE_NAME = "secrets.json";

        Secret[] tokens;

        /// <exception cref="SecretException"/>
        public SecretsManager()
        {
            if (!File.Exists(FILE_NAME))
            { // create a template of the file
                File.WriteAllText(FILE_NAME, JsonConvert.SerializeObject(new SecretToken(), Formatting.Indented));
            }
            tokens = JsonConvert.DeserializeObject<Secret[]>(File.ReadAllText(FILE_NAME));
            if (tokens == null || tokens.Length == 0)
                throw new SecretException($"No Secrets defined in \"{FILE_NAME}\"");
        }

        /// <summary>
        /// Get the default <see cref="SecretToken"/>
        /// </summary>
        /// <param name="release">Disabled auto detection of debugger, used to fake a particular environment</param>
        /// <returns>The default bot's <see cref="SecretToken"/></returns>
        /// <exception cref="SecretNotFoundException"/>
        public SecretToken? GetDefault(bool? release = null)
        {
            bool condition = CheckOrAutoFill(release);


            var p = tokens.Where(i => { return i.Default; }).ToList();
            for (int i = 0; i < p.Count; i++)
            {
                for (int ii = 0; ii < p[i].Config.Tokens.Length; ii++)
                {
                    var configBlob = p[i].Config.Tokens[ii];
                    if (configBlob.Debug == condition)
                    {
                        return configBlob;
                    }
                }
            }
            throw new SecretNotFoundException("default");
        }
        
        /// <summary>
        /// Get <see cref="SecretsJson"/> of a bot defined in "secrets.json"<para/>
        /// <example>
        /// Usage of <see cref="GetSecret(string, bool?)"/><para/>
        /// <c>
        /// var botConfig = GetSecret("MyBot"); //for auto detection<para/>
        /// var botConfig = GetSecret("MyBot", true); //faking a release build<para/>
        /// var botConfig = GetSecret("MyBot", false); //faking a debug build<para/>
        /// </c>
        /// </example>
        /// </summary>
        /// <param name="name">Bot Name</param>
        /// <param name="release">Disabled auto detection of debugger, used to fake a particular environment</param>
        /// <returns>The requested bot's <see cref="SecretToken"/></returns>
        /// <exception cref="SecretNotFoundException"/>
        public SecretToken? GetSecret(string name, bool? release = null)
        {
            bool condition = CheckOrAutoFill(release);


            var p = tokens.Where(i => { return i.Name == name; }).ToArray();


            for (int i = 0; i < p.Length; i++)
            {
                for (int ii = 0; ii < p[i].Config.Tokens.Length; ii++)
                {
                    var configBlob = p[i].Config.Tokens[ii];
                    if (configBlob.Debug == condition)
                    {
                        return configBlob;
                    }
                }
            }

            if (p.Length == 0)
                throw new SecretNotFoundException(name);
            return null;
        }

        /// <summary>
        /// Try to get a bot's <see cref="SecretToken"/>
        /// </summary>
        /// <param name="name">Bot's Name</param>
        /// <param name="secret">Resulting <see cref="SecretToken"/></param>
        /// <param name="release">Disabled auto detection of debugger, used to fake a particular environment</param>
        /// <returns>Returns True if bot was found, else false</returns>
        public bool TryGetSecret(string name, out SecretToken? secret, bool? release = null)
        {
            bool condition = CheckOrAutoFill(release);

            secret = null;

            try
            {
                secret = GetSecret(name, condition);
            } catch (SecretNotFoundException) { }

            return (secret != null);
        }

        private bool CheckOrAutoFill(bool? value) {
            return value ?? Debugger.IsAttached;
        }

        public void Dispose()
        {
            tokens = null;
        }
    }
}