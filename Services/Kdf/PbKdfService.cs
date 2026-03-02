using System;
using System.Collections.Generic;
using System.Text;

namespace SharpKnP321.Services.Kdf
{
    internal class PbKdfService : IKdfService
    {
        public string Dk(string salt, string password)
        {
            String t = Hash(salt + password);
            for (int i = 0; i < 100000; i++)
            {
                t = Hash(t);
            }
            return t;
        }

        private String Hash(String input) => Convert.ToHexString(
            System.Security.Cryptography.MD5.HashData(
                System.Text.Encoding.UTF8.GetBytes(input)
            )
        );

    }
}
