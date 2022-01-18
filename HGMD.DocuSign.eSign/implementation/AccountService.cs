using DocuSign.CodeExamples.Helper;
using DocuSign.eSign.Client;
using HGMD.DocuSign.eSign.contracts;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using static DocuSign.eSign.Client.Auth.OAuth;
using static DocuSign.eSign.Client.Auth.OAuth.UserInfo;

namespace HGMD.DocuSign.eSign.implementation
{
    public class AccountService : IAccountService
    {
        private readonly IConfiguration _configuration;
        private static ApiClient _apiClient;

        public AccountService(IConfiguration configuration)
        {
            _configuration = configuration;
            _apiClient ??= new ApiClient();
        }
        
        public (OAuthToken, Account) Initialize()
        {
            var authToken = _apiClient.RequestJWTUserToken(
                    _configuration["DocuSign:IntegrationKey"],
                    _configuration["DocuSign:UserId"],
                    _configuration["DocuSign:AuthServer"],
                    DSHelper.ReadFileContent(DSHelper.PrepareFullPrivateKeyFilePath(_configuration["DocuSign:PrivateKeyFile"])),
                    int.Parse(_configuration["DocuSign:JWTLifeTime"]),
                    new List<string> { "signature" });

            var account = GetAccountInfo(authToken);
            return (authToken, account);
        }

        private Account GetAccountInfo(OAuthToken authToken)
        {
            _apiClient.SetOAuthBasePath(this._configuration["DocuSign:AuthServer"]);
            UserInfo userInfo = _apiClient.GetUserInfo(authToken.access_token);
            Account acct = userInfo.Accounts.FirstOrDefault();
            if (acct == null)
            {
                throw new Exception("The user does not have access to account");
            }

            return acct;
        }

    }
}
