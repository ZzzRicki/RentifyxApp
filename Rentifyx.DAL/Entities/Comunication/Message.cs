namespace Rentifyx.DAL.Entities.Comunication
{
    public class Message
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool Answered { get; set; }
    }
}
