using simple_client_oauth1.Enums;
using simple_client_oauth1.Models;
using simple_client_oauth1.Utils;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace simple_client_oauth1
{
    public class GenerateValuesRequest
    {
        private readonly string OAuthVersion;
        protected const string OAuthParameterPrefix = "oauth_";
        protected const string OAuthConsumerKeyKey = "oauth_consumer_key";
        protected const string OAuthCallbackKey = "oauth_callback";
        protected const string OAuthVersionKey = "oauth_version";
        protected const string OAuthSignatureMethodKey = "oauth_signature_method";
        protected const string OAuthSignatureKey = "oauth_signature";
        protected const string OAuthTimestampKey = "oauth_timestamp";
        protected const string OAuthNonceKey = "oauth_nonce";
        protected const string OAuthTokenKey = "oauth_token";
        protected const string OAuthTokenSecretKey = "oauth_token_secret";
        protected const string HMACSHA1SignatureType = "HMAC-SHA1";
        private readonly string ConsumerKey;
        private readonly string ConsumerSecretKey;
        private readonly string TokenKey;
        private readonly string TokenSecretKey;
        private readonly bool IncludeVersion;
        private readonly SignatureTypes SignatureType;

        public GenerateValuesRequest(string consumerKey, string consumerSecretKey, string tokenKey, string tokenSecretKey, SignatureTypes signatureType, bool includeVersion, string version = "1.0")
        {
            OAuthVersion = version;
            ConsumerKey = consumerKey;
            ConsumerSecretKey = consumerSecretKey;
            TokenKey = tokenKey;
            TokenSecretKey = tokenSecretKey;
            IncludeVersion = includeVersion;
            SignatureType = signatureType;
        }

        public Dictionary<string, string> GetParametersRequest(string url, string requestMethod)
        {
            Dictionary<string, string> retorno;

            Uri requesturl = new Uri(url);
            string TimeInSecondsSince1970 = ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString();
            string Nonce = Convert.ToBase64String(Encoding.UTF8.GetBytes(TimeInSecondsSince1970
            + TimeInSecondsSince1970 + TimeInSecondsSince1970));

            string signatureHash = GenerateSignature(requesturl, ConsumerKey, ConsumerSecretKey, TokenKey, TokenSecretKey, SignatureType, requestMethod.ToUpper(), TimeInSecondsSince1970, Nonce);

            retorno = new Dictionary<string, string> {
                { OAuthConsumerKeyKey, ConsumerKey },
                { OAuthNonceKey, Nonce },
                { OAuthTokenKey, TokenKey },
                { OAuthSignatureMethodKey, SignatureType.GetDescription() },
                { OAuthSignatureKey, signatureHash },
                { OAuthTimestampKey, TimeInSecondsSince1970 },
                { OAuthVersionKey, OAuthVersion }
            };

            return retorno;
        }

        private string GenerateSignature(Uri url, string consumerKey, string consumerSecret, string token, string tokenSecret, SignatureTypes signatureType, string httpMethod, string timeStamp, string nonce)
        {

            switch (signatureType)
            {
                case SignatureTypes.PLAINTEXT:
                    return HttpUtility.UrlEncode(string.Format("{0}&{1}", consumerSecret, tokenSecret));
                case SignatureTypes.HMAC_SHA1:
                    string signatureBase = GenerateSignatureBase(url, consumerKey, token, tokenSecret, httpMethod, timeStamp, nonce, signatureType);

                    HMACSHA1 hmacsha1 = new HMACSHA1();
                    hmacsha1.Key = Encoding.ASCII.GetBytes(string.Format("{0}&{1}", Converter.UrlEncode(consumerSecret), string.IsNullOrEmpty(tokenSecret) ? "" : Converter.UrlEncode(tokenSecret)));

                    return GenerateSignatureUsingHash(signatureBase, hmacsha1);
                case SignatureTypes.RSA_SHA1:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentException("Unknown signature type", "signatureType");
            }
        }

        private string GenerateSignatureUsingHash(string signatureBase, HashAlgorithm hash)
        {
            return ComputeHash(hash, signatureBase);
        }

        private string ComputeHash(HashAlgorithm hashAlgorithm, string data)
        {
            if (hashAlgorithm == null)
            {
                throw new ArgumentNullException("hashAlgorithm");
            }

            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentNullException("data");
            }

            byte[] dataBuffer = System.Text.Encoding.ASCII.GetBytes(data);
            byte[] hashBytes = hashAlgorithm.ComputeHash(dataBuffer);

            return Convert.ToBase64String(hashBytes);
        }

        private string GenerateSignatureBase(Uri url, string consumerKey, string token, string tokenSecret, string httpMethod, string timeStamp, string nonce, SignatureTypes signatureType)
        {
            if (token == null)
            {
                token = string.Empty;
            }

            if (tokenSecret == null)
            {
                tokenSecret = string.Empty;
            }

            if (string.IsNullOrEmpty(consumerKey))
            {
                throw new ArgumentNullException("consumerKey");
            }

            if (string.IsNullOrEmpty(httpMethod))
            {
                throw new ArgumentNullException("httpMethod");
            }

            List<QueryParameter> parameters = GetQueryParameters(url.Query);
            if (IncludeVersion)
            {
                parameters.Add(new QueryParameter(OAuthVersionKey, OAuthVersion));
            }
            parameters.Add(new QueryParameter(OAuthNonceKey, nonce));
            parameters.Add(new QueryParameter(OAuthTimestampKey, timeStamp));
            parameters.Add(new QueryParameter(OAuthSignatureMethodKey, signatureType.GetDescription()));
            parameters.Add(new QueryParameter(OAuthConsumerKeyKey, consumerKey));

            if (!string.IsNullOrEmpty(token))
            {
                parameters.Add(new QueryParameter(OAuthTokenKey, token));
            }

            parameters.Sort(new QueryParameterComparer());

            var normalizedUrl = string.Format("{0}://{1}", url.Scheme, url.Host);
            if (!((url.Scheme == "http" && url.Port == 80) || (url.Scheme == "https" && url.Port == 443)))
            {
                normalizedUrl += ":" + url.Port;
            }
            normalizedUrl += url.AbsolutePath;
            var normalizedRequestParameters = NormalizeRequestParameters(parameters);

            StringBuilder signatureBase = new StringBuilder();
            signatureBase.AppendFormat("{0}&", httpMethod.ToUpper());
            signatureBase.AppendFormat("{0}&", Converter.UrlEncode(normalizedUrl));
            signatureBase.AppendFormat("{0}", Converter.UrlEncode(normalizedRequestParameters));

            return signatureBase.ToString();
        }

        private List<QueryParameter> GetQueryParameters(string parameters)
        {
            if (parameters.StartsWith("?"))
            {
                parameters = parameters.Remove(0, 1);
            }

            List<QueryParameter> result = new List<QueryParameter>();

            if (!string.IsNullOrEmpty(parameters))
            {
                string[] p = parameters.Split('&');
                foreach (string s in p)
                {
                    if (!string.IsNullOrEmpty(s) && !s.StartsWith(OAuthParameterPrefix))
                    {
                        if (s.IndexOf('=') > -1)
                        {
                            string[] temp = s.Split('=');
                            result.Add(new QueryParameter(temp[0], temp[1]));
                        }
                        else
                        {
                            result.Add(new QueryParameter(s, string.Empty));
                        }
                    }
                }
            }

            return result;
        }

        private string NormalizeRequestParameters(IList<QueryParameter> parameters)
        {
            StringBuilder sb = new StringBuilder();
            QueryParameter p = null;
            for (int i = 0; i < parameters.Count; i++)
            {
                p = parameters[i];
                sb.AppendFormat("{0}={1}", p.Name, p.Value);

                if (i < parameters.Count - 1)
                {
                    sb.Append("&");
                }
            }

            return sb.ToString();
        }
        
        
    }
}
