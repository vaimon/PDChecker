namespace PDChecker.Models;

public class User
{
    public int Id { get; set; }
    public string Role { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }
    public string Login { get; set; }

    public List<Grade> Grades { get; set; } = default!;

    public User(string role, string name, string login)
    {
        Role = role;
        Name = name;
        Password = "";
        Login = login;
    }
}