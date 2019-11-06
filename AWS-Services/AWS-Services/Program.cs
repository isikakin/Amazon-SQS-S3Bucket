using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AWS_Services
{
    class Program
    {
        private static IAmazonS3 s3Client;
        private static readonly RegionEndpoint defaultRegion = RegionEndpoint.EUCentral1;
        private static readonly BasicAWSCredentials awsCredentials = new BasicAWSCredentials("yourAccessKey", "yourSecretKey");
        private static readonly string bucketName = "bucketName";
        private static readonly string queueName = "qeueName";

        static void Main(string[] args)
        {
            s3Client = new AmazonS3Client(awsCredentials, defaultRegion);

            ItemModel model = new ItemModel();
            model.Name = "Test";
            model.Description = "TEST";

            string bucketKey = Guid.NewGuid().ToString();

            AddBucket(bucketName, bucketKey, JsonConvert.SerializeObject(model)).Wait();

            SendToSqs(bucketKey, queueName).Wait();

            ReceiveFromBucket(bucketName, bucketKey).Wait();
        }

        public static async Task AddBucket(string bucketName, string keyName, string body)
        {
            try
            {
                PutObjectRequest request = new PutObjectRequest();
                request.BucketName = bucketName;
                request.Key = string.Concat(keyName, ".json");
                request.ContentType = "application/json";
                request.ContentBody = body;
                request.CannedACL = S3CannedACL.Private;

                await s3Client.PutObjectAsync(request);
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }

        }

        public static async Task<bool> SendToSqs(string messageBody, string queueName)
        {
            try
            {
                IAmazonSQS amazonSQS = new AmazonSQSClient(awsCredentials, defaultRegion);
                var sqsMessageRequest = new SendMessageRequest
                {
                    QueueUrl = amazonSQS.GetQueueUrlAsync(queueName).Result.QueueUrl,
                    MessageBody = messageBody
                };
                await amazonSQS.SendMessageAsync(sqsMessageRequest);
                return true;
            }
            catch (Exception exception)
            {
                Console.WriteLine("Excetpion: {0}", exception.Message);
                return false;
            }
        }

        public static async Task ReceiveFromBucket(string bucketName, string keyName)
        {
            string responseBody = "";
            try
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName + ".json"
                };
                using (GetObjectResponse response = await s3Client.GetObjectAsync(request))
                using (Stream responseStream = response.ResponseStream)
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    string title = response.Metadata["x-amz-meta-title"];
                    string contentType = response.Headers["Content-Type"];
                    Console.WriteLine("Object metadata, Title: {0}", title);
                    Console.WriteLine("Content type: {0}", contentType);

                    responseBody = reader.ReadToEnd();
                }

                Console.WriteLine("Data from S3 : {0}", responseBody);
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered ***. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }
        }
    }
}
