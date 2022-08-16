namespace Solar.DTOs.Outbound
{
    public class LoginResultDto
    {
        public LoginResultDto(string email, string accessToken, IEnumerable<string> roles)
        {
            Email = email;
            AccessToken = accessToken;
            Roles = roles;
        }

        public string Email { get; set; }
        public string AccessToken { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }
}
