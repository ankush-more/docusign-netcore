using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using DocuSign.ESignature.Demo.Models;
using HGMD.DocuSign.eSign.contracts;
using HGMD.DocuSign.eSign.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static DocuSign.eSign.Client.Auth.OAuth;
using static DocuSign.eSign.Client.Auth.OAuth.UserInfo;

namespace DocuSign.ESignature.Demo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ISignatureService _signatureService;
        private readonly ApiClient _apiClient;

        public HomeController(ILogger<HomeController> logger, ISignatureService signatureService)
        {
            _apiClient = new ApiClient();
            _apiClient.SetOAuthBasePath($"{ConfigData.AuthServer}");
            _logger = logger;
            _signatureService = signatureService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.IsPostBack = "0";
            ViewBag.Signing = "incomplete";
            if (Request.Query["event"] == "completed")
            {
                ViewBag.IsPostBack = "1";
                ViewBag.Signing = "completed";
            }

            ViewUrl signingDocumentURL = await _signatureService
                .GenerateEnvelopeRedirectURL(new DocumentSigner { EmailId = "hgmddummy@gmail.com", Name = "Dr Tracy Mc'Coy", ClientId = Guid.NewGuid().ToString() });
            ViewBag.SigningURL = signingDocumentURL.Url;

            return View();
        }

        public IActionResult AddUser()
        {
            return View();
        }

        public IActionResult SignatureCallback(string envelopeId)
        {
            OAuthToken authToken = _apiClient.RequestJWTUserToken(
                ConfigData.IntegrationKey,
                ConfigData.UserId,
                ConfigData.AuthServer,
                System.IO.File.ReadAllBytes(ConfigData.PrivateKey),
                ConfigData.JWTLifeTime,
                new List<string> { "signature" });

            Account _account = GetAccountInfo(authToken);

            var apiClient = new ApiClient($"{_account.BaseUri}/restapi");
            apiClient.Configuration.DefaultHeader.Add("Authorization", "Bearer " + authToken.access_token);

            var esignEnvelopesApi = new EnvelopesApi(apiClient);
            Envelope envelope = esignEnvelopesApi.GetEnvelope(_account.AccountId, envelopeId);
            if(envelope.Status.ToLower() == "completed")
            {
                Stream _signedDocumentStream = esignEnvelopesApi.GetDocument(_account.AccountId, envelope.EnvelopeId, "1");
                if (_signedDocumentStream != null)
                    return File(_signedDocumentStream, "application/pdf");
            }
            return Redirect($"https://localhost:44360/Home/SCB?event={envelope.Status}");
        }

        public IActionResult SCB()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            #region Downloading EnvelopeDocument 
            // To Do => Move to Service
            //OAuthToken authToken = _apiClient.RequestJWTUserToken(
            //        ConfigData.IntegrationKey,
            //        ConfigData.UserId,
            //        ConfigData.AuthServer,
            //        System.IO.File.ReadAllBytes(ConfigData.PrivateKey),
            //        ConfigData.JWTLifeTime,
            //        new List<string> { "signature" });

            //Account _account = GetAccountInfo(authToken);

            //var apiClient = new ApiClient($"{_account.BaseUri}/restapi");
            //apiClient.Configuration.DefaultHeader.Add("Authorization", "Bearer " + authToken.access_token);

            //var esignEnvelopesApi = new EnvelopesApi(apiClient);

            //string _envelopeId = "";
            //Stream _signedDocumentStream = esignEnvelopesApi.GetDocument(_account.AccountId, _envelopeId, "1");

            //Envelope envelope = esignEnvelopesApi.GetEnvelope(_account.AccountId, _envelopeId);
            //EnvelopeDocumentsResult re = esignEnvelopesApi.ListDocuments(_account.AccountId, _envelopeId);

            //string filePath = @"C:\Ankush\Learning\docusign\src-demo\temp";

            //DirectoryInfo info = new DirectoryInfo(filePath);
            //if (!info.Exists)
            //{
            //    info.Create();
            //}

            //string path = Path.Combine(filePath, "result.pdf");
            //using (FileStream outputFileStream = new FileStream(path, FileMode.Create))
            //{
            //    _signedDocumentStream.CopyTo(outputFileStream);
            //}

            #endregion

            return View();
        }

        private Account GetAccountInfo(OAuthToken authToken)
        {
            _apiClient.SetOAuthBasePath(ConfigData.AuthServer);
            eSign.Client.Auth.OAuth.UserInfo userInfo = _apiClient.GetUserInfo(authToken.access_token);
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
