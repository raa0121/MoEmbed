using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace MoEmbed.Models.Metadata
{
    /// <summary>
    /// Represents the <see cref="Metadata"/> for the unknown URL.
    /// </summary>
    [Serializable]
    public class UnknownMetadata : Metadata
    {
        public UnknownMetadata()
        {
        }

        public UnknownMetadata(string uri)
        {
            Uri = uri;
        }

        public UnknownMetadata(Uri uri)
        {
            Uri = uri.ToString();
        }

        /// <summary>
        /// Gets or sets the requested URL.
        /// </summary>
        [DefaultValue(null)]
        public string Uri { get; set; }

        /// <summary>
        /// Gets or sets the URL the <see cref="Uri" /> moved to.
        /// </summary>
        [DefaultValue(null)]
        public string MovedTo { get; set; }

        /// <summary>
        /// Gets or sets the resolved data.
        /// </summary>
        [DefaultValue(null)]
        public EmbedData Data { get; set; }

        [NonSerialized]
        private Task<EmbedData> _FetchTask;

        /// <inheritdoc />
        public override Task<EmbedData> FetchAsync()
        {
            lock (this)
            {
                if (_FetchTask == null)
                {
                    if (Data != null)
                    {
                        _FetchTask = Task.FromResult<EmbedData>(Data);
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

        private async Task<EmbedData> FetchAsyncCore()
        {
            using (var hh = new HttpClientHandler()
            {
                AllowAutoRedirect = false
            })
            using (var hc = new HttpClient(hh))
            {
                // TODO: share HttpClient in service

                var res = await GetResponseAsync(hc).ConfigureAwait(false);
                if (!res.IsSuccessStatusCode)
                {
                    return null;
                }

                var mediaType = res.Content.Headers.ContentType.MediaType;

                if (Regex.IsMatch(mediaType, @"^text\/html$"))
                {
                    LoadHtml(await res.Content.ReadAsStringAsync());
                }
                else if (Regex.IsMatch(mediaType, @"^image\/"))
                {
                }
                else if (Regex.IsMatch(mediaType, @"^video\/"))
                {
                }
                else
                {
                }
            }
            return Data;
        }

        private async Task<HttpResponseMessage> GetResponseAsync(HttpClient hc)
        {
            var u = MovedTo ?? Uri;
            for (;;)
            {
                var res = await hc.GetAsync(u).ConfigureAwait(false);

                switch (res.StatusCode)
                {
                    case HttpStatusCode.Moved:
                        if (u == (MovedTo ?? Uri))
                        {
                            MovedTo = res.Headers.Location.ToString();
                        }
                        u = res.Headers.Location.ToString();
                        continue;
                    case HttpStatusCode.Ambiguous:
                    case HttpStatusCode.Found:
                    case HttpStatusCode.RedirectMethod:
                        u = res.Headers.Location.ToString();
                        continue;
                }

                return res;
            }
        }

        private void LoadHtml(string html)
        {
            var hd = new HtmlDocument();
            hd.LoadHtml(html);

            var nav = hd.CreateNavigator();
            Data = new EmbedData()
            {
                Title = nav.SelectSingleNode("//html/head/title/text()")?.Value
            };
        }
    }
}
