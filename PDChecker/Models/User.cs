﻿using System.Text.Json.Serialization;

namespace PDChecker.Models;

public class User
{
    public int Id { get; set; }
    public string Role { get; set; }
    public string Name { get; set; }
    [JsonIgnore]
    public string Password { get; set; }
    public string Login { get; set; }

    [JsonIgnore]
    public ICollection<Grade> Grades { get; set; } = default!;

    public User(string role, string name, string login)
    {
        Role = role;
        Name = name;
        Password = "";
        Login = login;
    }
}