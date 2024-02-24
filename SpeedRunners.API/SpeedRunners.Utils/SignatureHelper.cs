using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

public class SignatureHelper
{
    public static string GenerateSignature(string token, string userId, string parameters, long timestamp)
    {
        string dataToSign = $"{token}params{parameters}ts{timestamp}user_id{userId}";
        return CalculateMD5Hash(dataToSign);
    }

    private static string CalculateMD5Hash(string input)
    {
        using MD5 md5 = MD5.Create();
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = md5.ComputeHash(inputBytes);

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < hashBytes.Length; i++)
        {
            sb.Append(hashBytes[i].ToString("x2"));
        }

        return sb.ToString();
    }
}
