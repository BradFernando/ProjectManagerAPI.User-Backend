namespace ProjectManager.Api.User.Models
{
    public class EmailSend
    {
        public string FromEmail { get; set; }
        public string FromPassword { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}