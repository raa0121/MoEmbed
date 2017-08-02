﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json.Linq;

namespace MoEmbed.Models
{
    /// <summary>
    /// Represents the <see cref="Metadata"/> fetching from remote oEmbed providers.
    /// </summary>
    [Serializable]
    public class RemoteMetadata : Metadata
    {
        /// <summary>
        /// Gets or sets the requested URL.
        /// </summary>
        [DefaultValue(null)]
        public string Uri { get; set; }

        /// <summary>
        /// Gets or sets the oEmbed servide URL.
        /// </summary>
        [DefaultValue(null)]
        public string OEmbedUrl { get; set; }

        [DefaultValue(null)]
        public DictionaryEmbedData Data { get; set; }

        [NonSerialized]
        private Task<IEmbedData> _FetchTask;

        /// <inheritdoc />
        public override Task<IEmbedData> FetchAsync()
        {
            lock (this)
            {
                if (_FetchTask == null)
                {
                    if (Data != null)
                    {
                        _FetchTask = Task.FromResult<IEmbedData>(Data);
                    }
                    else
                    {
                        _FetchTask = FetchAsyncCore();
                        _FetchTask.ConfigureAwait(false);
                    }
                }
                return _FetchTask;
            }
        }

        private async Task<IEmbedData> FetchAsyncCore()
        {
            using (var hc = new HttpClient())
            {
                // TODO: share HttpClient in service

                var r = await hc.GetAsync(OEmbedUrl).ConfigureAwait(false);

                r.EnsureSuccessStatusCode();

                var txt = await r.Content.ReadAsStringAsync().ConfigureAwait(false);

                Dictionary<string, object> values;
                if (r.Content.Headers.ContentType.MediaType == "text/xml")
                {
                    values = new Dictionary<string, object>();
                    var d = new XmlDocument();
                    d.LoadXml(txt);

                    foreach (XmlNode xn in d.DocumentElement.ChildNodes)
                    {
                        if (xn.NodeType == XmlNodeType.Element)
                        {
                            var e = (XmlElement)xn;
                            // TODO: parse number
                            values[e.LocalName] = d.Value;
                        }
                    }
                }
                else
                {
                    var jo = JObject.Parse(txt);
                    values = jo.ToObject<Dictionary<string, object>>();
                }

                return Data = new DictionaryEmbedData(values);
            }
        }
    }
}