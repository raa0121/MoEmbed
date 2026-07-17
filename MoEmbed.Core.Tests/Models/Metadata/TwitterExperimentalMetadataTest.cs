using MoEmbed.Models.TwitterExperimental;
using MoEmbed.Providers;

using System;
using System.Collections.Generic;

namespace MoEmbed.Models.Metadata
{
    public class TwitterExperimentalMetadataTest
    {
        [Test]
        [Arguments(
            463440424141459456L,
            "US Department of the Interior (@Interior)",
            "https://pbs.twimg.com/profile_images/432081479/DOI_LOGO_normal.jpg",
            "Sunsets don't get much better than this one over @GrandTetonNPS. #nature #sunset http://t.co/YuKy2rcjyU"
            )]
        public async Task FetchAsyncTest(long tweetId, string expectedDisplayName, string expectedProfileImageUrl, string expectedDescription)
        {
            var uri = $"https://twitter.com/Interior/status/{tweetId}";
            var target = new TwitterExperimentalMetadataProvider().GetMetadata(
                new ConsumerRequest(new Uri(uri)));

            var data = await target.FetchAsync(
                            new RequestContext(
                                new MetadataService(),
                                new ConsumerRequest(new Uri(uri))));

            await Assert.That(data.Title).IsEqualTo(expectedDisplayName);
            await Assert.That(data.MetadataImage.Thumbnail.Url).IsEqualTo(expectedProfileImageUrl);
            await Assert.That(data.Description).IsEqualTo(expectedDescription);
        }

        [Test]
        [Arguments(
            1608046425149177856L,
            "https://x.com/supermomonga/status/1608046425149177856/photo/1",
            "https://pbs.twimg.com/media/FlDtq9fakAIHeKj.jpg"
            )]
        public async Task SingleImageTest(long tweetId, string expectedLocation, string expectedRawUrl)
        {
            var uri = $"https://twitter.com/supermomonga/status/{tweetId}";
            var target = new TwitterExperimentalMetadataProvider().GetMetadata(
                new ConsumerRequest(new Uri(uri)));

            var data = await target.FetchAsync(
                            new RequestContext(
                                new MetadataService(),
                                new ConsumerRequest(new Uri(uri))));

            await Assert.That(data.Medias.Count).IsEqualTo(1);
            await Assert.That(data.Medias[0].Location).IsEqualTo(expectedLocation);
            await Assert.That(data.Medias[0].RawUrl).IsEqualTo(expectedRawUrl);
        }

        [Test]
        [Arguments(
                463440424141459456L,
                "US Department of the Interior (@Interior)",
                "https://pbs.twimg.com/profile_images/432081479/DOI_LOGO_normal.jpg",
                "Sunsets don't get much better than this one over @GrandTetonNPS. #nature #sunset http://t.co/YuKy2rcjyU"
                )]
        public async Task XComFetchAsyncTest(long tweetId, string expectedDisplayName, string expectedProfileImageUrl, string expectedDescription)
        {
            var uri = $"https://x.com/Interior/status/{tweetId}";
            var target = new TwitterExperimentalMetadataProvider().GetMetadata(
                new ConsumerRequest(new Uri(uri)));

            var data = await target.FetchAsync(
                            new RequestContext(
                                new MetadataService(),
                                new ConsumerRequest(new Uri(uri))));

            await Assert.That(data.Title).IsEqualTo(expectedDisplayName);
            await Assert.That(data.MetadataImage.Thumbnail.Url).IsEqualTo(expectedProfileImageUrl);
            await Assert.That(data.Description).IsEqualTo(expectedDescription);
        }

        [Test]
        public async Task GetRawUrl_VideoPicksHighestBitrateMp4Test()
        {
            var m = new MediaDetail
            {
                Type = "video",
                MediaUrlHttps = "https://pbs.twimg.com/ext_tw_video_thumb/1/pu/img/thumb.jpg",
                VideoInfo = new VideoInfo
                {
                    Variants = new List<VideoVariant>
                    {
                        new() { Bitrate = 632000, ContentType = "video/mp4", Url = "https://video.twimg.com/ext_tw_video/1/pu/vid/480x270/low.mp4" },
                        new() { Bitrate = 2176000, ContentType = "video/mp4", Url = "https://video.twimg.com/ext_tw_video/1/pu/vid/1280x720/high.mp4" },
                        new() { ContentType = "application/x-mpegURL", Url = "https://video.twimg.com/ext_tw_video/1/pu/pl/playlist.m3u8" },
                    },
                },
            };

            await Assert.That(TwitterExperimentalMetadata.GetRawUrl(m)).IsEqualTo("https://video.twimg.com/ext_tw_video/1/pu/vid/1280x720/high.mp4");
        }

        [Test]
        public async Task GetRawUrl_VideoFallsBackToThumbnailWhenNoMp4VariantTest()
        {
            var m = new MediaDetail
            {
                Type = "video",
                MediaUrlHttps = "https://pbs.twimg.com/ext_tw_video_thumb/1/pu/img/thumb.jpg",
                VideoInfo = new VideoInfo
                {
                    Variants = new List<VideoVariant>
                    {
                        new() { ContentType = "application/x-mpegURL", Url = "https://video.twimg.com/ext_tw_video/1/pu/pl/playlist.m3u8" },
                    },
                },
            };

            await Assert.That(TwitterExperimentalMetadata.GetRawUrl(m)).IsEqualTo(m.MediaUrlHttps);
        }

        [Test]
        public async Task GetRawUrl_PhotoUsesMediaUrlHttpsTest()
        {
            var m = new MediaDetail
            {
                Type = "photo",
                MediaUrlHttps = "https://pbs.twimg.com/media/photo.jpg",
            };

            await Assert.That(TwitterExperimentalMetadata.GetRawUrl(m)).IsEqualTo(m.MediaUrlHttps);
        }
    }
}
