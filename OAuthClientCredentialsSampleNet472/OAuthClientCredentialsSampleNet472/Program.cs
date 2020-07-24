using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.Caching;
using Newtonsoft.Json;
using RestSharp;

namespace OAuthClientCredentialsSampleNet472
{
    class Program
    {
        static void Main(string[] args)
        {
            string accessToken = string.Empty;
            string accessToken2 = string.Empty;

            // Get New Access Token
            accessToken = GetAccessToken();
            Console.WriteLine(accessToken);

            // Just to make sure that we are getting same access token for the consecutive calls
            for (int i=0; i<3; i++)
            {
                accessToken2  = GetAccessToken();

                if (accessToken2 == accessToken)
                {
                    Console.WriteLine("Same Access token");
                }
                else
                {
                    Console.WriteLine("Different Access token");
                }
            }
            Console.ReadKey();
           
        }

        /// <summary>
        /// This method returns an Access Token. If token exists in cache then it returns from cache 
        /// otherwise it makea a call to Token Endpoint to get a new access token.
        /// It uses client credentials grant.
        /// </summary>
        /// <returns>Access token</returns>
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

    }
}
