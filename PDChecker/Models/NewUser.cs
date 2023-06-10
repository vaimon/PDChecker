namespace PDChecker.Models;

public class NewUser
{
    public string Login { get; set; }
    public string Password { get; set; }
    public string Name { get; set; }
    public string Role { get; set; }


    public NewUser(string login, string password, string name, string role)
    {
        Login = login;
        Password = password;
        Name = name;
        Role = role;
    }
}