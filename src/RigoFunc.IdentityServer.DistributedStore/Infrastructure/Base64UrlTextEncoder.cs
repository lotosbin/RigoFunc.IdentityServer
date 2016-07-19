using System;

namespace RigoFunc.IdentityServer.DistributedStore {
    public static class Base64UrlTextEncoder {
        public static string Encode(byte[] data) => Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        public static byte[] Decode(string text) => Convert.FromBase64String(Pad(text.Replace('-', '+').Replace('_', '/')));

        private static string Pad(string text) {
            var padding = 3 - ((text.Length + 3) % 4);
            if (padding == 0) {
                return text;
            }
            return text + new string('=', padding);
        }
    }
}
