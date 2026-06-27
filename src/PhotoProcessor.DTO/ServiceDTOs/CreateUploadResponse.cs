namespace PhotoProcessor.DTO.ServiceDTOs
{  
    public class CreateUploadResponse(Guid mediaId, string uploadUrl)
    {
        public Guid MediaId { get; set; } = mediaId;
        public string UploadUrl { get; set; } = uploadUrl;
    }
}

