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
        private static byte[] key = new byte[0];
        private static byte[] user_key = new byte[0];

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

        public static void DecryptDb(string path)
        {
            Console.WriteLine("Please insert the database password: ");
            string password = "";
            ConsoleKeyInfo keyinfo;
            do
            {
                keyinfo = Console.ReadKey(true);
                if (keyinfo.Key != ConsoleKey.Backspace)
                {
                    password += keyinfo.KeyChar;
                    Console.Write("*");
                }
            }
            while (keyinfo.Key != ConsoleKey.Enter);
            Console.WriteLine("\n");

            user_key = Encoding.UTF8.GetBytes(password);
            byte[] fixed_key = new byte[32];
            int length = Math.Min(user_key.Length, 32);
            Array.Copy(user_key, fixed_key, length);
            Array.Copy(fixed_key, user_key, length);
            
            string key_file = File.ReadAllText("key");
            if (key_file == "")
            {
                GenerateKey();
            }
            else
            {
                key = Convert.FromBase64String(key_file);
                var aes = Aes.Create();
                aes.Key = fixed_key;
                aes.IV = new byte[16];
                aes.Padding = PaddingMode.PKCS7;

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                var ms = new MemoryStream(key);
                var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                var sr = new StreamReader(cs);

                // var a = sr.ReadToEnd();
                // Console.WriteLine(a);
                // key = Encoding.ASCII.GetBytes(a);
                var msOut = new MemoryStream();
                cs.CopyTo(msOut);
                key = msOut.ToArray();
            }

            string db = File.ReadAllText(path);

            var aes2 = Aes.Create();
            aes2.Key = key;
            aes2.IV = new byte[16];
            aes2.Padding = PaddingMode.PKCS7;

            var decryptor2 = aes2.CreateDecryptor(aes2.Key, aes2.IV);
            var ms2 = new MemoryStream(Convert.FromBase64String(db));
            var cs2 = new CryptoStream(ms2, decryptor2, CryptoStreamMode.Read);
            var sr2 = new StreamReader(cs2);

            // string nedb = sr2.ReadToEnd();
            // File.WriteAllText(path, nedb);
            var msOut2 = new MemoryStream();
            cs2.CopyTo(msOut2);
            File.WriteAllBytes(path, msOut2.ToArray());
        }

        public static void CryptDB(string path)
        {
            var user_key_str = Encoding.UTF8.GetString(user_key);

            // if (user_key_str == "" || user_key_str == null )
            if (!user_key.Any())
            {
                Console.WriteLine("Please insert the database password: ");
                ConsoleKeyInfo keyinfo;
                do
                {
                    keyinfo = Console.ReadKey(true);
                    if (keyinfo.Key != ConsoleKey.Backspace)
                    {
                        user_key_str += keyinfo.KeyChar;
                        Console.Write("*");
                    }
                }
                while (keyinfo.Key != ConsoleKey.Enter);
                Console.WriteLine("\n");
                user_key = Encoding.ASCII.GetBytes(user_key_str);
                GenerateKey();
            }

            // crypting db
            string db = File.ReadAllText(path);
            var aes = Aes.Create();
            aes.Key = key;
            aes.IV = new byte[16];
            aes.Padding = PaddingMode.PKCS7;

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            byte[] encryptedDb;
            using (var ms = new MemoryStream())
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(db);
                sw.Flush();
                cs.FlushFinalBlock();
                encryptedDb = ms.ToArray();
            }
            File.WriteAllText(path, Convert.ToBase64String(encryptedDb));

            // Cifrare la chiave con la password dell’utente
            byte[] fixed_key = new byte[32];
            int length = Math.Min(user_key.Length, 32);
            Array.Copy(user_key, fixed_key, length);

            var aes2 = Aes.Create();
            aes2.Key = fixed_key;
            aes2.IV = new byte[16];
            aes2.Padding = PaddingMode.PKCS7;

            byte[] encryptedKey;
            using (var ms = new MemoryStream())
            using (var cs = new CryptoStream(ms, aes2.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(key, 0, key.Length);
                cs.FlushFinalBlock();
                encryptedKey = ms.ToArray();
            }
            File.WriteAllText("key", Convert.ToBase64String(encryptedKey));
        }
    
        public static void GenerateKey()
        {
            key = RandomNumberGenerator.GetBytes(32); // 256 bit key
        }
        
        static string HashSha256(string s) // ! Not used ! 
        {
            byte[] hashbytes = SHA256.HashData(Encoding.UTF8.GetBytes(s));
            return BitConverter.ToString(hashbytes);
        }

    }
}