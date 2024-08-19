using Amazon.S3;
using Amazon.S3.Model;
using System.Text;

class YandexStorage{
    private readonly AmazonS3Client s3Client;

    public YandexStorage () {
        s3Client = new AmazonS3Client(
            Environment.GetEnvironmentVariable("YandexAccessId"),
            Environment.GetEnvironmentVariable("YandexSecretKey"),
            new AmazonS3Config
            {
                ServiceURL = "https://storage.yandexcloud.net",
                ForcePathStyle = true
            }
        );
    }

    public async Task PlaceText(string text, string hash) {

        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
        {
            var putObjectRequest = new PutObjectRequest
            {
                BucketName = Environment.GetEnvironmentVariable("BucketName"),
                Key = $"{hash}.txt",
                InputStream = stream,
                ContentType = "text/plain"
            };

            await s3Client.PutObjectAsync(putObjectRequest);
        }
    }

    public async Task<string> GetText(string hash) {
        var getObjectRequest = new GetObjectRequest
        {
            BucketName = Environment.GetEnvironmentVariable("BucketName"),
            Key = $"{hash}.txt"
        };

        using (var response = await s3Client.GetObjectAsync(getObjectRequest))
        using (var responseStream = response.ResponseStream)
        using (var reader = new StreamReader(responseStream))
        {
            return await reader.ReadToEndAsync();
        }
    }
};
