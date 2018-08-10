using simple_client_oauth1.Enums;
using System;
using System.ComponentModel;
using System.Reflection;

namespace simple_client_oauth1.Utils
{
    public static class EnumExtension
    {
        public static string GetDescription(this Enum value)
        {
            FieldInfo fieldInfo = value.GetType().GetField(value.ToString());
            if (fieldInfo == null) return null;
            var attribute = (DescriptionAttribute)fieldInfo.GetCustomAttribute(typeof(DescriptionAttribute));
            return attribute.Description;
        }

        public static string SomeMethod(this SignatureTypes enumValue)
        {
            var retorno = "";
            switch (enumValue)
            {
                case SignatureTypes.HMAC_SHA1:
                    retorno = "HMAC-SHA1";
                    break;
                case SignatureTypes.PLAINTEXT:
                    retorno = "PLAINTEXT";
                    break;
                case SignatureTypes.RSA_SHA1:
                    retorno = "RSA-SHA1";
                    break;
            }

            return retorno;
        }
    }
}
