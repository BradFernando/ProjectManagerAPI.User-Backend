using System.ComponentModel.DataAnnotations;

namespace ProjectManager.Api.User.Models
{
    public class Users
    {
        [Key]
        public int id { get; set; }
        
        public string firstName { get; set; }
        
        public string lastName { get; set; }
        
        public string email { get; set; }
        
        public string userName { get; set; }
        
        public string password { get; set; }
        
        public int type { get; set; }
        
        public string avatar { get; set; }
        
        public int status { get; set; } = 0; // Default value set to 0
    }
}