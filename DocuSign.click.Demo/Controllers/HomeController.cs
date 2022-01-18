using DocuSign.Click.Client;
using DocuSign.ESign.Demo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using DocuSign.Click.Api;
using static DocuSign.Click.Client.Auth.OAuth.UserInfo;
using static DocuSign.Click.Client.Auth.OAuth;
using System.Linq;
using System;
using DocuSign.Click.Model;
using DocuSign.ClickWrap.Demo.Models;

namespace DocuSign.ESign.Demo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApiClient _apiClient;

        public HomeController(ILogger<HomeController> logger)
        {
            _apiClient = new ApiClient();
            _apiClient.SetOAuthBasePath($"{ConfigData.AuthServer}");
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RegisterUser()
        {
            return View();
        }

        public IActionResult ClickWrapResponse()
        {
            OAuthToken authToken = _apiClient.RequestJWTUserToken(
                    ConfigData.IntegrationKey,
                    ConfigData.UserId,
                    ConfigData.AuthServer,
                    System.IO.File.ReadAllBytes(ConfigData.PrivateKey),
                    ConfigData.JWTLifeTime,
                    new List<string> { "click.manage" });

            Account _account = GetAccountInfo(authToken);

            var apiClient = new ApiClient($"{_account.BaseUri}/clickapi");
            apiClient.Configuration.DefaultHeader.Add("Authorization", "Bearer " + authToken.access_token);

            var clickAccountApi = new AccountsApi(apiClient);
            ClickwrapAgreementsResponse clickwrapAgreementsResponse = clickAccountApi.GetClickwrapAgreements
                                                                            (
                                                                                _account.AccountId,
                                                                                "5ae8d3e0-ca82-447f-ab8a-f4340ad70767"
                                                                             );
            List<ClickWrapResponse> clickWrapResponses = new List<ClickWrapResponse>();
            clickwrapAgreementsResponse.UserAgreements.ForEach(c =>
               clickWrapResponses
               .Add(new ClickWrapResponse
               {
                   Identifier = c.ClientUserId,
                   DocumentName = c.Settings.DisplayName,
                   Version = c.VersionNumber.ToString(),
                   AgreedOn = Convert.ToDateTime(c.AgreedOn).ToLocalTime(),
                   Status = c.Status
               }
               ));

            return View(clickWrapResponses);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private Account GetAccountInfo(OAuthToken authToken)
        {
            _apiClient.SetOAuthBasePath(ConfigData.AuthServer);
            UserInfo userInfo = _apiClient.GetUserInfo(authToken.access_token);
            Account acct = userInfo.Accounts.FirstOrDefault();
            if (acct == null)
            {
                throw new Exception("The user does not have access to account");
            }

            return acct;
        }
    }

    public static class ConfigData
    {
        public static string IntegrationKey { get; } = "94148ec7-24c5-4fc3-8197-801c355fedf0";
        public static string UserId { get; } = "330cfe09-b02c-4804-8a2b-898addca87e7";
        public static string AuthServer { get; } = "account-d.docusign.com";
        public static string PrivateKey { get; } = "private.key";
        public static int JWTLifeTime { get; } = 1;
    }
}
