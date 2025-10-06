using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Classes
{
    class Security
    {
        public static string jwt_secret_key = BitConverter.ToString(RandomNumberGenerator.GetBytes(256)); // Generating a secure random key for the JW tokens

        public static string GenerateToken(string username)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt_secret_key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "crazydomain.com",
                audience: "crazydomain.com",
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public static string HashArgon2(string s)
        {
            Argon2id argon_hash = new Argon2id(Encoding.UTF8.GetBytes(s));
            argon_hash.Iterations = 6;
            argon_hash.MemorySize = 65535;
            argon_hash.DegreeOfParallelism = 2;
            byte[] argon_bytes = argon_hash.GetBytes(32);
            return Convert.ToHexString(argon_bytes);
        }

        static string HashSha256(string s) // ! Not used ! 
        {
            byte[] hashbytes = SHA256.HashData(Encoding.UTF8.GetBytes(s));
            return BitConverter.ToString(hashbytes);
        }

    }
}