using System;
using System.Security.Cryptography;

/* Stolen Shamelessly from Janak's Blog
 * 
 * https://janaks.com.np/password-hashing-in-csharp/
 * 
 * and this code was located at his git
 * 
 * https://github.com/janaks09/dotnet-samples/tree/master/PasswordHashing
 * 
 * 
 * 
 * for class/professor purposes:
 * 
 * Hash and Salt are two highly randomized results based on an algorithm. taking  the users pw, whatever it may be,
 * and adding a highly randomized byte (24 in this case) to the end of it ( so password1XXXXXXXX- 24) then processing
 * that through the hashing function we get unique hashes, even if a user has the same password as another.
 * 
 * we then store the salt and the hash as plain text. The salts are all unique - and have nothing they break down too
 * except some high quality random number. This means that breaking attempts have to break both the salt and the hash for 
 * each password, exponnetially increasing the time required to crack a database.
 */

namespace CcnSession
{
    class PasswordHash
    {
        // 24 = 192 bits
        private const int SaltByteSize = 24;
        private const int HashByteSize = 24;
        private const int HasingIterationsCount = 10101;


        public static byte[] ComputeHash(string password, byte[] salt, int iterations = HasingIterationsCount, int hashByteSize = HashByteSize)
        {
            using (Rfc2898DeriveBytes hashGenerator = new Rfc2898DeriveBytes(password, salt))
            {
                hashGenerator.IterationCount = iterations;
                return hashGenerator.GetBytes(hashByteSize);
            }
        }

        public static byte[] GenerateSalt(int saltByteSize = SaltByteSize)
        {
            using (RNGCryptoServiceProvider saltGenerator = new RNGCryptoServiceProvider())
            {
                byte[] salt = new byte[saltByteSize];
                saltGenerator.GetBytes(salt);
                return salt;
            }
        }

        public static bool VerifyPassword(String password, byte[] passwordSalt, byte[] passwordHash)
        {
            byte[] computedHash = ComputeHash(password, passwordSalt);
            return AreHashesEqual(computedHash, passwordHash);
        }

        //Length constant verification - prevents timing attack
        private static bool AreHashesEqual(byte[] firstHash, byte[] secondHash)
        {
            int minHashLenght = firstHash.Length <= secondHash.Length ? firstHash.Length : secondHash.Length;
            var xor = firstHash.Length ^ secondHash.Length;
            for (int i = 0; i < minHashLenght; i++)
                xor |= firstHash[i] ^ secondHash[i];
            return 0 == xor;
        }
    }
}
