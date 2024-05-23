using System.Security.Cryptography;
using System.Text;

namespace dotnet_primer;

public class Login
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string base64Credentials { get; set; }
    public string hash { get; set; }

    public Login(string username, string password)
    {
        this.Id = System.Security.Cryptography.RandomNumberGenerator.GetInt32(1000);
        Username = username;
        Password = password;
        this.base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password));
        var byteCredentials = Convert.FromBase64String(this.base64Credentials);
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(byteCredentials);
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            this.hash = builder.ToString();
        }
    }
}
