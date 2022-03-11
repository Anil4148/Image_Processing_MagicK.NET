using Amazon.S3;
using Amazon.S3.Model;
using ImageMagick;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Magick
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("start processing");
            await ProcessImages();

        }

        static async Task<string> ProcessImages()
        {
            const int adjustment = 30;
            const int imageQuality = 70;
            var width = 1366;
            var height = 768;
            

            var request = new PutObjectRequest
            {
                BucketName = "bucketname",
                InputStream = new MemoryStream(),
                Key = $"{Guid.NewGuid()}.jpeg",
                TagSet = new List<Tag>()
            };

            // Input Image 
            using (var client = new HttpClient())
            {
                using (var response = await client.GetAsync("https://singleton-dev-media.s3-us-west-2.amazonaws.com/04380167-70ca-4d78-99dc-993128f07005.png"))
                {
                    byte[] imageBytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    using (var image = new MagickImage(imageBytes))
                    {
                        if (image.Width > width + adjustment || image.Height > height + adjustment)
                        {
                            image.Quality = imageQuality;
                            image.Resize(width + adjustment, height + adjustment);
                        }

                        using var finalImage = new MagickImage(new MagickColor("#151515"), width + adjustment, height + adjustment);
                        finalImage.Composite(image, Gravity.Center, CompositeOperator.Over);
                        finalImage.Quality = 100;
                        finalImage.Write(request.InputStream, MagickFormat.Jpeg);
                        var _s3Client = new AmazonS3Client("AccesskeyId", "AccessKeySecret");
                        await _s3Client.PutObjectAsync(request);
                        return request.Key;
                    }
                }
            }
        }
    }
}
