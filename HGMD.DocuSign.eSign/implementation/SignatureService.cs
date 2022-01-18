using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using HGMD.DocuSign.eSign.contracts;
using HGMD.DocuSign.eSign.Entities;
using HGMD.DocuSign.eSign.Helper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using static DocuSign.eSign.Client.Auth.OAuth;
using static DocuSign.eSign.Client.Auth.OAuth.UserInfo;

namespace HGMD.DocuSign.eSign.implementation
{
    public class SignatureService : ISignatureService
    {
        private readonly IConfiguration _configuration;
        private readonly IAccountService _accountService;
        private IEnvelopeService _envelopeService;
        private readonly OAuthToken _authToken;
        private Account _account;

        private readonly EnvelopeDefinition envelopeDefinition;

        public SignatureService(IAccountService accountService, IEnvelopeService envelopeService , IConfiguration configuration)
        {
            _accountService = accountService;
            _configuration = configuration;
            _envelopeService = envelopeService;
            (_authToken, _account) = accountService.Initialize();
            envelopeDefinition = _envelopeService._envDefinitionTemplate;
        }

        public string GenerateEnvelopeRedirectURL(DocumentSigner docSigner)
        {

            var apiClient = new ApiClient($"{_account.BaseUri}/restapi");
            apiClient.Configuration.DefaultHeader.Add("Authorization", "Bearer " + _authToken.access_token);

            var esignEnvelopesApi = new EnvelopesApi(apiClient);

            AddSignerToEnvelopeTemplate(docSigner);

            EnvelopeSummary envelopeSummary = esignEnvelopesApi
                .CreateEnvelope(_account.AccountId, envelopeDefinition);

            string callbackUrl = $"https://localhost:44360/Home/SignatureCallback?envelopeId={envelopeSummary.EnvelopeId}";

            ViewUrl redirectUrl = esignEnvelopesApi
                .CreateRecipientView(
                    _account.AccountId,
                    envelopeSummary.EnvelopeId,
                    EnvelopeHelper.MakeRecipientViewRequest(docSigner, callbackUrl, "https://localhost:44392/api/Info/ping")
                    );

            return redirectUrl.Url;
        }

        private void AddSignerToEnvelopeTemplate(DocumentSigner documentSigner)
        {
            Signer signer = new Signer
            {
                Email = documentSigner.EmailId,
                Name = documentSigner.Name,
                ClientUserId = documentSigner.ClientId,
                RecipientId = "1"
            };

            SignHere signHere = new SignHere
            {
                //AnchorIgnoreIfNotPresent = "true",
                //AnchorCaseSensitive = "true",
                //AnchorString = "/PLEASE_SIGN_HERE/",
                //AnchorUnits = "pixels",
                //AnchorXOffset = "20",
                //AnchorYOffset = "-20",
                XPosition = "500",
                YPosition = "750",
                Optional = "false",
                StampType = "signature",
                DocumentId = "1",
                PageNumber = "9",
                HandDrawRequired = "true",
                RecipientId = "1"
            };

            Tabs signerTabs = new Tabs
            {
                SignHereTabs = new List<SignHere>
                {
                    signHere
                }
            };
            signer.Tabs = signerTabs;

            Recipients recipients = new Recipients
            {
                Signers = new List<Signer> { signer }
            };
            envelopeDefinition.Recipients = recipients;
        }
    }
}
