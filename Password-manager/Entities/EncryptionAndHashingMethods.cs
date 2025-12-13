using System;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace Password_manager.Entities
{
    class EncryptionAndHashingMethods
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int DegreeOfParallelism = 4;
        private const int Iterations = 2;
        private const int MemorySize = 65536;

        public string HashString(string password)
        {
            // 1. Generate random salt
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // 2. Hash the password using that salt
            byte[] hash = HashString(password, salt);

            // 3. Combine Salt + Hash for storage
            var combinedBytes = new byte[salt.Length + hash.Length];
            Array.Copy(salt, 0, combinedBytes, 0, salt.Length);
            Array.Copy(hash, 0, combinedBytes, salt.Length, hash.Length);

            // 4. Return as Base64
            return Convert.ToBase64String(combinedBytes);
        }
        public byte[] GenerateKeys(int keySize)
        {
            byte[] key = new byte[keySize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }
            return key;
        }
        public byte[] HashString(string password, byte[] salt)
        {
            using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)))
            {
                argon2.Salt = salt;
                argon2.DegreeOfParallelism = DegreeOfParallelism;
                argon2.Iterations = Iterations;
                argon2.MemorySize = MemorySize;

                return argon2.GetBytes(HashSize);
            }
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            byte[] combinedBytes = Convert.FromBase64String(hashedPassword);

            byte[] salt = new byte[SaltSize];
            byte[] existingHash = new byte[HashSize];

            Array.Copy(combinedBytes, 0, salt, 0, SaltSize);
            Array.Copy(combinedBytes, SaltSize, existingHash, 0, HashSize);

            byte[] newHash = HashString(password, salt);

            return CryptographicOperations.FixedTimeEquals(existingHash, newHash);
        }

        public string Encrypt(string plain, byte[] _key)
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plain);

            int nonceSize = AesGcm.NonceByteSizes.MaxSize;
            int tagSize = AesGcm.TagByteSizes.MaxSize; 
            int cipherSize = plainBytes.Length;


            int encryptedDataLength = 4 + nonceSize + 4 + tagSize + cipherSize;
            Span<byte> encryptedData = encryptedDataLength < 1024
                                     ? stackalloc byte[encryptedDataLength]
                                     : new byte[encryptedDataLength].AsSpan();

            // 1. Write Header Info
            BinaryPrimitives.WriteInt32LittleEndian(encryptedData.Slice(0, 4), nonceSize);

            // 2. Define Slices
            var nonce = encryptedData.Slice(4, nonceSize);
            BinaryPrimitives.WriteInt32LittleEndian(encryptedData.Slice(4 + nonceSize, 4), tagSize);
            var tag = encryptedData.Slice(4 + nonceSize + 4, tagSize);
            var cipherBytes = encryptedData.Slice(4 + nonceSize + 4 + tagSize, cipherSize);

            // 3. Generate Nonce & Encrypt
            RandomNumberGenerator.Fill(nonce);

            using (var aes = new AesGcm(_key))
            {
                aes.Encrypt(nonce, plainBytes.AsSpan(), cipherBytes, tag);
            }

            return Convert.ToBase64String(encryptedData);
        }

        public string Decrypt(string cipher, byte[] _key)
        {
            Span<byte> encryptedData = Convert.FromBase64String(cipher).AsSpan();

            // 1. Read Header Info
            int nonceSize = BinaryPrimitives.ReadInt32LittleEndian(encryptedData.Slice(0, 4));

            // 2. Read Tag Length (located after the Nonce)
            int tagSize = BinaryPrimitives.ReadInt32LittleEndian(encryptedData.Slice(4 + nonceSize, 4));

            int cipherSize = encryptedData.Length - 4 - nonceSize - 4 - tagSize;

            // 3. Extract Slices
            var nonce = encryptedData.Slice(4, nonceSize);
            var tag = encryptedData.Slice(4 + nonceSize + 4, tagSize);
            var cipherBytes = encryptedData.Slice(4 + nonceSize + 4 + tagSize, cipherSize);

            // 4. Decrypt
            Span<byte> plainBytes = cipherSize < 1024
                                  ? stackalloc byte[cipherSize]
                                  : new byte[cipherSize];

            using (var aes = new AesGcm(_key))
            {
                aes.Decrypt(nonce, cipherBytes, tag, plainBytes);
            }

            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}