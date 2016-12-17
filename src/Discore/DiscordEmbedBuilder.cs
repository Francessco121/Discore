using System;

namespace Discore
{
    public class DiscordEmbedBuilder
    {
        DiscordApiData product;

        public DiscordEmbedBuilder()
        {
            product = DiscordApiData.CreateContainer();
        }

        public DiscordEmbedBuilder CreateEmbed()
        {
            return this;
        }

        public DiscordEmbedBuilder SetTitle(string title)
        {
            product.Set("title", title);
            return this;
        }

        public DiscordEmbedBuilder SetType(string type)
        {
            product.Set("type", type);
            return this;
        }

        public DiscordEmbedBuilder SetDescription(string description)
        {
            product.Set("description", description);
            return this;
        }

        public DiscordEmbedBuilder SetUrl(string url)
        {
            product.Set("url", url);
            return this;
        }

        public DiscordEmbedBuilder SetTimestamp(DateTime time)
        {
            product.Set("timestamp", time);
            return this;
        }

        bool colorSet;
        public DiscordEmbedBuilder SetColor(DiscordColor color)
        {
            colorSet = true;
            product.Set("color", color);
            return this;
        }

        public DiscordEmbedBuilder SetFooter(string text, string icon = null)
        {
            DiscordApiData apiData = DiscordApiData.CreateContainer();

            apiData.Set("text", text);
            if (!string.IsNullOrWhiteSpace(icon)) apiData.Set("icon", icon);

            product.Set("footer", apiData);

            return this;
        }

        public DiscordEmbedBuilder SetImage(string url)
        {
            DiscordApiData apiData = DiscordApiData.CreateContainer();

            apiData.Set("url", url);

            product.Set("image", apiData);

            return this;
        }

        public DiscordEmbedBuilder SetVideo(string url)
        {
            DiscordApiData apiData = DiscordApiData.CreateContainer();

            apiData.Set("url", url);

            product.Set("video", apiData);

            return this;
        }

        public DiscordEmbedBuilder SetThumbnail(string url)
        {
            DiscordApiData apiData = DiscordApiData.CreateContainer();

            apiData.Set("url", url);

            product.Set("thumbnail", apiData);

            return this;
        }

        public DiscordEmbedBuilder SetProvider(string name, string url = null)
        {
            DiscordApiData apiData = DiscordApiData.CreateContainer();

            apiData.Set("name", name);
            if (!string.IsNullOrWhiteSpace(url)) apiData.Set("url", url);

            product.Set("provider", apiData);

            return this;
        }

        public DiscordEmbedBuilder AddField(string name, string value, bool inline)
        {

            DiscordApiData fieldArray = product.Get("fields");

            // if the field array doesn't exist then we create it
            if (fieldArray.IsNull)
                fieldArray = DiscordApiData.CreateArray();


            DiscordApiData apiData = DiscordApiData.CreateContainer();

            apiData.Set("name", name);
            apiData.Set("value", value);
            apiData.Set("inline", inline);

            fieldArray.Values.Add(apiData);

            product.Set("fields", fieldArray);
            return this;
        }

        internal DiscordApiData Build()
        {
            if (!colorSet)
                product.Set("color", DiscordColor.DefaultEmbed);

            return product;
        }
    }
}
