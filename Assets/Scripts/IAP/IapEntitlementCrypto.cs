using System;
using System.Security.Cryptography;
using System.Text;

public static class IapEntitlementCrypto
{
    private const string Prefix = "enc_v1:";
    private const string EntitlementSeed = "ArtnetFixtureApp.IAP.Entitlements.v1";

    private static readonly byte[] KeyBytes;

    static IapEntitlementCrypto()
    {
        using (SHA256 sha = SHA256.Create())
        {
            KeyBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(EntitlementSeed));
        }
    }

    public static string Encrypt(string plainText)
    {
        if (plainText == null)
        {
            plainText = string.Empty;
        }

        using Aes aes = Aes.Create();
        aes.Key = KeyBytes;
        aes.GenerateIV();

        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        using ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        string payload = Convert.ToBase64String(aes.IV) + "." + Convert.ToBase64String(cipherBytes);
        return Prefix + payload;
    }

    public static bool TryDecrypt(string storedValue, out string plainText)
    {
        plainText = string.Empty;

        if (string.IsNullOrWhiteSpace(storedValue) || !storedValue.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return false;
        }

        string payload = storedValue.Substring(Prefix.Length);
        string[] parts = payload.Split('.');
        if (parts.Length != 2)
        {
            return false;
        }

        try
        {
            byte[] iv = Convert.FromBase64String(parts[0]);
            byte[] cipherBytes = Convert.FromBase64String(parts[1]);

            using Aes aes = Aes.Create();
            aes.Key = KeyBytes;
            aes.IV = iv;

            using ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            plainText = Encoding.UTF8.GetString(plainBytes);
            return true;
        }
        catch (Exception)
        {
            plainText = string.Empty;
            return false;
        }
    }
}
