namespace PlantCare.Data.DbModels;

public class User
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string CreatedAt { get; set; }
}