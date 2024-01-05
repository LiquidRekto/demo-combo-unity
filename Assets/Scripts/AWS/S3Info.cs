using System.Collections;
using System.Collections.Generic;

public class S3Info
{
    public string AwsBucketName {  get; set; }
    public string AwsAccessKey { get; set; }
    public string AwsSecretKey { get; set; }

    public Amazon.RegionEndpoint AwsRegion { get; set; }
}
