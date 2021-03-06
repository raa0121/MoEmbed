﻿using System;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Portable.Xaml.Markup;

namespace MoEmbed.Models.Metadata
{
    /// <summary>
    /// Represents the <see cref="Metadata"/> for the <see href="https://ani.tv"/>.
    /// </summary>
    [Serializable]
    [ContentProperty(nameof(Data))]
    public class AnitvMetadata : UnknownMetadata
    {
        /// <inheritdoc />
        protected override void LoadHtml(HtmlDocument htmlDocument)
        {
            base.LoadHtml(htmlDocument);

            var nav = htmlDocument.CreateNavigator();

            Data.Type = EmbedDataTypes.MixedContent;

            var noText = nav.SelectSingleNode("//*[@class='movie-content-note']/h3/text()")?.Value?.Trim();
            if (noText != null)
            {
                var m = Regex.Match(noText, @"第\s*([0-9]{1,8})\s*話");

                if (m.Success)
                {
                    var no = int.Parse(m.Groups[1].Value);

                    var title = nav.SelectSingleNode("//*[@class='movie-content-note']/h2/text()")?.Value?.Trim();

                    if (title != null)
                    {
                        Data.Title = $"第{no}話 {title}";
                    }
                }
            }

            var desc = nav.SelectSingleNode("//*[@class='mcd-text']/text()")?.Value?.Trim();

            if (!string.IsNullOrEmpty(desc))
            {
                Data.Description = desc;
            }

            Data.MetadataImage.Type = MediaTypes.Video;
        }
    }
}