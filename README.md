# How to implement the Client Credentials Grant
The Client Credentials grant is used when application request an access token to access their own resources, not on behalf of a user. The access token will be for the “application”, an application context not a user context.

## Caching Access Token
It's relatively expensive to get an OAuth access token, because it requires an HTTP request to the token endpoint. Therefore, an application should always cache access tokens. 
It does not make sense to fetch new access token for each API request. 

Access tokens has an expiration and must be cached by the application within 5 minutes of token expiration.

## How to cache an Access token
Below is the sample .NET code to cache an access token. For details, see .NET sample application folder

```
 private static string GetAccessToken()
        {
            ObjectCache cache = MemoryCache.Default;
            int tokenTime;
            if (ConfigurationManager.AppSettings["ida:TokenTime"] != null)
                tokenTime = int.Parse(ConfigurationManager.AppSettings["ida:TokenTime"]);
            else
                tokenTime = 50;

            // If access token exists in cache then return it from there
            var securityToken = cache.Get("SecurityToken") as string;
            if (securityToken != null)
                return securityToken;

            // If access token does not exist in cache then make a call to Token Endpoint
            // to get a new Access Token
            var oauthClient = new RestClient(ConfigurationManager.AppSettings["ida:TokenEndPoint"]);
            var request = new RestRequest()
            {
                Method = Method.POST
            };
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("scope", ConfigurationManager.AppSettings["ida:Scope"]);
            request.AddParameter("client_id", ConfigurationManager.AppSettings["ida:ClientId"]);
            request.AddParameter("client_secret", ConfigurationManager.AppSettings["ida:ClientSecret"]);
            request.AddParameter("grant_type", "client_credentials");
            var tResponse = oauthClient.Execute(request);
            var responseJson = tResponse.Content;
            securityToken = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseJson)["access_token"].ToString();
            CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(tokenTime) };
            cache.Add("SecurityToken", securityToken, policy);
            return securityToken;
        }
```

