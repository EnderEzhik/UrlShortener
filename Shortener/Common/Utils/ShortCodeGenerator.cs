using System.Security.Cryptography;

namespace Shortener.Common.Utils;

public class ShortCodeGenerator
{
    private static readonly char[] URL_SAFE_ALPHABET = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();

    // Сгенерировать код для короткой ссылки
    public static string GenerateCode(int length)
    {
        var buffer = new char[length];
        for (var i = 0; i < length; i++)
        {
            var index = RandomNumberGenerator.GetInt32(URL_SAFE_ALPHABET.Length);
            buffer[i] = URL_SAFE_ALPHABET[index];
        }
        return new string(buffer);
    }
}