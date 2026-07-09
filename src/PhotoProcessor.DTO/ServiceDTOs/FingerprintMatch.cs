namespace PhotoProcessor.DTO.ServiceDTOs
{
    public class FingerprintMatch
    {
        public Guid FingerprintId { get; set; }
        public Guid MediaId { get; set; }
        public Guid? TagId { get; set; }
         
        public double Distance { get; set; }
    }
}
