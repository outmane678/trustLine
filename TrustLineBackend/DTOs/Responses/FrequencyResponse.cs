namespace AnonymousComplaintsAPI.DTOs.Responses
{
    public class FrequencyResponse
    {
        public int FrequencyID { get; set; }
        public string Label { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? Archived { get; set; }
    }
}
