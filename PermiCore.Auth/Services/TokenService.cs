using Microsoft.IdentityModel.Tokens;
using PermiCore.Auth.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PermiCore.Auth.Services
{
    public class TokenService
    {
        readonly List<UserModel> _userModels;

        public TokenService()
        {
            _userModels = new List<UserModel>
            {
                new UserModel{ Email="test@gmail.com", Password="test123",Age=25,Gender = true },
                new UserModel{ Email="test2@gmail.com", Password="test1234", Age=26, Gender=false }
            };
        }
        public string LoginToken(LoginRequestModel loginRequestModel)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(loginRequestModel.ClientSecret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            UserModel? userModel = _userModels.FirstOrDefault(y => y.Email == loginRequestModel.Username && y.Password == loginRequestModel.Password);
            if (userModel == null)
                throw new Exception("Invalid username or password");

            var claims = new List<Claim>();

            var token = new JwtSecurityToken(
                issuer: "http://localhost:5127",
                audience: "http://localhost:5160",
                claims: claims,
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
