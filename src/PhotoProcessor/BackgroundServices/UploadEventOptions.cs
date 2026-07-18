using PhotoProcessor.DTO.enums;

namespace PhotoProcessor.API.BackgroundServices
{
  
    public class UploadEventOptions
    {
        public required string Queue { get; set; }
        public required string Exchange { get; set; }
        public required string ExchangeType { get; set; }
        public required string RoutingKey { get; set; }
         
        public required string KeyPrefix { get; set; }
         
        public required List<JobTypes> JobTypes { get; set; }
    }
}
