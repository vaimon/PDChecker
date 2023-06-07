using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace PDChecker.Services;

public class AuthOptions
{
    public const string ISSUER = "Vaimon"; // издатель токена
    public const string AUDIENCE = "Client"; // потребитель токена
    const string KEY = "superpuperSeCrEt_KEY";   // ключ для шифрации
    public const int LIFETIME = 60; // время жизни токена - 1 час
    public static SymmetricSecurityKey GetSymmetricSecurityKey()
    {
        return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
    }
}