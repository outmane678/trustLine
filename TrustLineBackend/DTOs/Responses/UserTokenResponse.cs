
namespace AnonymousComplaintsAPI.DTOs.Responses
{
    public class UserTokenResponse
    {
        public string Token
        {
            get;
            set;
        }
        public string UserName
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public TimeSpan Validaty
        {
            get;
            set;
        }
        public string RefreshToken
        {
            get;
            set;
        }
        public string avatar
        {
            get;
            set;
        }
        public string Id
        {
            get;
            set;
        }
        public string EmailId
        {
            get;
            set;
        }
        public Guid GuidId
        {
            get;
            set;
        }
        public DateTime ExpiredTime
        {
            get;
            set;
        }

        //public string permissionsList { get; set; }

        public List<object> permissions { get; set; }
    }
}
