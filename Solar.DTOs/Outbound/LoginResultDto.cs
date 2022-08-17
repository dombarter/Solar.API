namespace Solar.DTOs.Outbound
{
    public class LoginResultDto
    {
        public LoginResultDto(string email, IEnumerable<string> roles)
        {
            Email = email;
            Roles = roles;
        }

        public string Email { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }
}
