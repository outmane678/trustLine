using System.ComponentModel.DataAnnotations;

namespace AnonymousComplaintsAPI.DTOs.Requests
{
    public class UserLoginRequest
    {
        [Required]
        public string UserName
        {
            get;
            set;
        }
        [Required]
        public string Password
        {
            get;
            set;
        }
        public UserLoginRequest() { }
    }

}
