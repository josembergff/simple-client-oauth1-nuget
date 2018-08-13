# Simple Client Oauth1

Package that performs simple requests for an Oauth1 service.

## Install

Package Manager

```bash
PM> Install-Package simple-client-oauth1
```

or .NET CLI

```bash
> dotnet add package simple-client-oauth1
```

or Paket CLI

```bash
> paket add simple-client-oauth1
```

## Example Usage

```bash
using simple_client_oauth1;
using simple_client_oauth1.Enums;

namespace ExampleConnectOauth
{
    public class ExampleConnectOauth
    {
        private readonly GenerateValuesRequest aouth1;
        public ExampleConnectOauth(){}
            aouth1 = new GenerateValuesRequest("{comsumerKey}", "{comsumerKeySecret}", "{token}", "{tokenKey}", SignatureTypes.HMAC_SHA1, true, "1.0");
        }

        public Dictionary<string, string> GetValuesRequestOauth(string url, string method){
            return aouth1.GetParametersRequest(url, method);
        }

        public string GetUrlCompleteOauth(string url, string method){
            return aouth1.GetHttpClienteUrl(url, method);
        }
    }
 }
```

### Other language

> Package NPM: https://github.com/josembergff/simple-client-oauth1-npm
