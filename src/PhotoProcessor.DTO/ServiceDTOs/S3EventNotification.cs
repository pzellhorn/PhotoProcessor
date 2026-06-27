namespace PhotoProcessor.DTO.ServiceDTOs
{
   
    public class S3EventNotification
    {
        public List<S3EventRecord> Records { get; set; } = [];
    }

    public class S3EventRecord
    {
        public S3Entity S3 { get; set; } = new();
    }

    public class S3Entity
    {
        public S3Object Object { get; set; } = new();
    }

    public class S3Object
    { 
        public string Key { get; set; } = string.Empty;
    }
}
