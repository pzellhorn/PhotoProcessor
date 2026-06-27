namespace PhotoProcessor.DTO.ServiceDTOs
{
    public class CreateUploadRequest(string fileName)
    {
        public string FileName { get; set; } = fileName;
    }
}
