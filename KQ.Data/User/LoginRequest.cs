namespace KQ.DataDto.User
{
    public class LoginRequest
    {
        public string LoginName { get; set; }
        public string Password { get; set; }
        public string? Imei { get; set; }
    }
}
