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

        public IActionResult Index()
        {
            ViewBag.IsPostBack = "0";
            ViewBag.Signing = "incomplete";
            if (Request.Query["event"] == "completed")
            {
                ViewBag.IsPostBack = "1";
                ViewBag.Signing = "completed";
            }

            string signingDocumentURL = _signatureService
                .GenerateEnvelopeRedirectURL(new DocumentSigner { EmailId = "hgmddummy@gmail.com", Name = "Dr Tracy Mc'Coy", ClientId = Guid.NewGuid().ToString() });
            ViewBag.SigningURL = signingDocumentURL;
            return View();
        }

        public IActionResult AddUser()
        {
            return View();
        }

        //public string Docusign()
        //{
        //    string signingDocumentURL = _signatureService
        //        .GenerateEnvelopeRedirectURL(new DocumentSigner { EmailId = "hgmddummy@gmail.com", Name = "Dr Tracy Mc'Coy", ClientId = Guid.NewGuid().ToString() });
            
        //    return signingDocumentURL;
        //}

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
            //return envelope.Status == "completed" ? "RESULT A" : "RESULT B";
            //return View();
            return Redirect($"https://localhost:44360/Home/SCB?event={envelope.Status}");
        }

        public IActionResult SCB()
        {
            return View();
        }

        public IActionResult Privacy()
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

            string _envelopeId = "";
            Stream _signedDocumentStream = esignEnvelopesApi.GetDocument(_account.AccountId, _envelopeId, "1");

            Envelope envelope = esignEnvelopesApi.GetEnvelope(_account.AccountId, _envelopeId);
            EnvelopeDocumentsResult re = esignEnvelopesApi.ListDocuments(_account.AccountId, _envelopeId);

            string filePath = @"C:\Ankush\Learning\docusign\src-demo\temp";

            DirectoryInfo info = new DirectoryInfo(filePath);
            if (!info.Exists)
            {
                info.Create();
            }

            string path = Path.Combine(filePath, "result.pdf");
            using (FileStream outputFileStream = new FileStream(path, FileMode.Create))
            {
                _signedDocumentStream.CopyTo(outputFileStream);
            }


            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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

        private static RecipientViewRequest MakeRecipientViewRequest(string signerEmail, string signerName, string returnUrl, string signerClientId, string pingUrl = null)
        {
            // Data for this method
            // signerEmail 
            // signerName
            // dsPingUrl -- class global
            // signerClientId -- class global
            // dsReturnUrl -- class global


            RecipientViewRequest viewRequest = new RecipientViewRequest();
            // Set the url where you want the recipient to go once they are done signing
            // should typically be a callback route somewhere in your app.
            // The query parameter is included as an example of how
            // to save/recover state information during the redirect to
            // the DocuSign signing ceremony. It's usually better to use
            // the session mechanism of your web framework. Query parameters
            // can be changed/spoofed very easily.
            viewRequest.ReturnUrl = returnUrl;

            // How has your app authenticated the user? In addition to your app's
            // authentication, you can include authenticate steps from DocuSign.
            // Eg, SMS authentication
            viewRequest.AuthenticationMethod = "none";

            // Recipient information must match embedded recipient info
            // we used to create the envelope.
            viewRequest.Email = signerEmail;
            viewRequest.UserName = signerName;
            viewRequest.ClientUserId = signerClientId;

            // DocuSign recommends that you redirect to DocuSign for the
            // Signing Ceremony. There are multiple ways to save state.
            // To maintain your application's session, use the pingUrl
            // parameter. It causes the DocuSign Signing Ceremony web page
            // (not the DocuSign server) to send pings via AJAX to your
            // app,
            // NOTE: The pings will only be sent if the pingUrl is an https address
            if (pingUrl != null)
            {
                viewRequest.PingFrequency = "600"; // seconds
                viewRequest.PingUrl = pingUrl; // optional setting
            }

            return viewRequest;
        }

        ///// Embedded Signing Ceremony
        ///
        private static EnvelopeDefinition MakeEnvelope(string signerEmail, string signerName, string signerClientId, string docPdf)
        {
            // Data for this method
            // signerEmail 
            // signerName
            // signerClientId -- class global
            // Config.docPdf


            byte[] buffer = System.IO.File.ReadAllBytes(docPdf);

            EnvelopeDefinition envelopeDefinition = new EnvelopeDefinition();
            envelopeDefinition.EmailSubject = "Please sign this document";

            envelopeDefinition.BrandId = "044e4164-9ca5-4f43-ab01-399bf289fbc0";

            Document doc1 = new Document();

            String doc1b64 = Convert.ToBase64String(buffer);

            doc1.DocumentBase64 = doc1b64;
            doc1.Name = "Terms and Conditions"; // can be different from actual file name
            doc1.FileExtension = "docx";
            doc1.DocumentId = "1";

            // The order in the docs array determines the order in the envelope
            envelopeDefinition.Documents = new List<Document> { doc1 };

            // Create a signer recipient to sign the document, identified by name and email
            // We set the clientUserId to enable embedded signing for the recipient
            // We're setting the parameters via the object creation
            Signer signer1 = new Signer
            {
                Email = signerEmail,
                Name = signerName,
                ClientUserId = signerClientId,
                RecipientId = "1"
            };

            // Create signHere fields (also known as tabs) on the documents,
            // We're using anchor (autoPlace) positioning
            //
            // The DocuSign platform seaches throughout your envelope's
            // documents for matching anchor strings.
            SignHere signHere1 = new SignHere
            {
                AnchorIgnoreIfNotPresent = "true",
                AnchorCaseSensitive = "true",
                AnchorString = "/PLEASE_SIGN_HERE/",
                AnchorUnits = "pixels",
                AnchorXOffset = "20",
                AnchorYOffset = "-20"
            };

            // Tabs are set per recipient / signer
            Tabs signer1Tabs = new Tabs
            {
                SignHereTabs = new List<SignHere> 
                {
                    signHere1
                }
            };
            signer1.Tabs = signer1Tabs;

            // Add the recipient to the envelope object
            Recipients recipients = new Recipients
            {
                Signers = new List<Signer> { signer1 }
            };
            envelopeDefinition.Recipients = recipients;

            // Request that the envelope be sent by setting |status| to "sent".
            // To request that the envelope be created as a draft, set to "created"
            envelopeDefinition.Status = "sent";

            return envelopeDefinition;
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
