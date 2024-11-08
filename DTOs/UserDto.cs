public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public DateTime Expiration { get; set; }
}
