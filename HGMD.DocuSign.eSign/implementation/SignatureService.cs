using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using HGMD.DocuSign.eSign.contracts;
using HGMD.DocuSign.eSign.Entities;
using HGMD.DocuSign.eSign.Helper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static DocuSign.eSign.Client.Auth.OAuth;
using static DocuSign.eSign.Client.Auth.OAuth.UserInfo;

namespace HGMD.DocuSign.eSign.implementation
{
    public class SignatureService : ISignatureService
    {
        private readonly IConfiguration _configuration;
        private IEnvelopeService _envelopeService;
        private readonly OAuthToken _authToken;
        private Account _account;

        private readonly EnvelopeDefinition _envelopeDefinition;
        private readonly Tabs _tabs;

        public SignatureService(IAccountService accountService, IEnvelopeService envelopeService, IConfiguration configuration)
        {
            _configuration = configuration;
            _envelopeService = envelopeService;
            (_authToken, _account) = accountService.Initialize();
            _envelopeDefinition = _envelopeService._envDefinitionTemplate;
            _tabs = GenerateCustomTabsFromConfiguration();
        }

        public async Task<ViewUrl> GenerateEnvelopeRedirectURL(DocumentSigner docSigner)
        {

            var apiClient = new ApiClient($"{_account.BaseUri}/restapi");
            apiClient.Configuration.DefaultHeader.Add("Authorization", "Bearer " + _authToken.access_token);

            var esignEnvelopesApi = new EnvelopesApi(apiClient);

            AddSignerToEnvelopeTemplate(docSigner);

            EnvelopeSummary envelopeSummary = esignEnvelopesApi
                .CreateEnvelope(_account.AccountId, _envelopeDefinition);

            string callbackUrl = $"https://localhost:44360/Home/SignatureCallback?envelopeId={envelopeSummary.EnvelopeId}";

            ViewUrl redirectUrl = await esignEnvelopesApi
                .CreateRecipientViewAsync(
                    _account.AccountId,
                    envelopeSummary.EnvelopeId,
                    EnvelopeHelper.MakeRecipientViewRequest(docSigner, callbackUrl, "https://localhost:44392/api/Info/ping")
                    );

            return redirectUrl;
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

            #region CustomTabs => Moved to resource/DocusSignConfiguration.json

            //Tabs signerTabs = new Tabs
            //{
            //    TextTabs = new List<Text>
            //    {
            //        new Text
            //        {
            //            CustomTabId = "423ba386-702d-4632-a3b4-478fcff76aab",
            //            DocumentId = "1",
            //            RecipientId = "1",
            //            AnchorIgnoreIfNotPresent = "true",
            //            AnchorCaseSensitive = "true",
            //            AnchorString = "##COVERED_ENTITY##",
            //            AnchorXOffset = "-5",
            //            AnchorYOffset = "-2",
            //        },
            //        new Text
            //        {
            //            CustomTabId = "176d5ad4-68de-4fd6-b6d3-388b196008bb",
            //            DocumentId = "1",
            //            RecipientId = "1",
            //            AnchorIgnoreIfNotPresent = "true",
            //            AnchorCaseSensitive = "true",
            //            AnchorString = "##COVERED_ENTITY_ADDRESS##",
            //            AnchorXOffset = "-5",
            //            AnchorYOffset = "-2",
            //            Height = "5"
            //        },
            //        new Text
            //        {
            //            CustomTabId = "d9bed91c-e53c-461b-8d3b-745b72007d9f",
            //            DocumentId = "1",
            //            RecipientId = "1",
            //            AnchorIgnoreIfNotPresent = "true",
            //            AnchorCaseSensitive = "true",
            //            AnchorString = "##COVERED_ENTITY_LEGAL_NAME##",
            //            AnchorXOffset = "-5",
            //            AnchorYOffset = "-2",
            //            Height = "5"
            //        },
            //        new Text
            //        {
            //            CustomTabId = "2f337d09-a616-476b-aa18-7aa509c4b0d6",
            //            DocumentId = "1",
            //            RecipientId = "1",
            //            AnchorIgnoreIfNotPresent = "true",
            //            AnchorCaseSensitive = "true",
            //            AnchorString = "##NAME##",
            //            AnchorXOffset = "-5",
            //            AnchorYOffset = "-2",
            //            Height = "5"
            //        },
            //    },
            //    DateTabs = new List<Date>
            //    {
            //        new Date
            //        {
            //            //CustomTabId = "789b7353-242e-44ed-90b5-cb418698bb8e",
            //            TabLabel = "Effective Date",
            //            TabType = "date",
            //            DocumentId = "1",
            //            RecipientId = "1",
            //            AnchorIgnoreIfNotPresent = "true",
            //            AnchorCaseSensitive = "true",
            //            AnchorString = "##EFFECTIVE_DATE##",
            //            AnchorYOffset = "-2",
            //            Height = "5",
            //            Width = "100"
            //        }
            //    },
            //    SignHereTabs = new List<SignHere>
            //    {
            //        new SignHere
            //        {
            //            CustomTabId = "9dabbd42-6d79-444f-8753-3010790fbda1",
            //            DocumentId = "1",
            //            RecipientId = "1",
            //            HandDrawRequired = "true",
            //            AnchorIgnoreIfNotPresent = "true",
            //            AnchorCaseSensitive = "true",
            //            AnchorString = "##SIGNHERE##",
            //            AnchorXOffset = "10",
            //            AnchorYOffset = "10"
            //        }
            //    },
            //    DateSignedTabs = new List<DateSigned>
            //    {
            //        new DateSigned
            //        {
            //            CustomTabId= "2f3493f8-eb8a-40f1-99e3-50f7cff52369",
            //            DocumentId = "1",
            //            RecipientId = "1",
            //            AnchorIgnoreIfNotPresent = "true",
            //            AnchorCaseSensitive = "true",
            //            AnchorString = "##DATESIGNED##",
            //            AnchorYOffset = "-2",
            //            Height = "5"
            //        }
            //    },
            //    ListTabs = new List<List>
            //    {
            //        new List
            //        {
            //            CustomTabId = "346baba1-8772-44ce-9385-83f33485b619",
            //            DocumentId = "1",
            //            RecipientId = "1",
            //            AnchorIgnoreIfNotPresent = "true",
            //            AnchorCaseSensitive = "true",
            //            AnchorString = "##TITLE##",
            //            AnchorXOffset = "-5",
            //            AnchorYOffset = "5",
            //            Height = "5"
            //        }
            //    }
            //};

            //signer.Tabs = signerTabs;
            #endregion

            signer.Tabs = _tabs;

            Recipients recipients = new Recipients
            {
                Signers = new List<Signer> { signer }
            };
            _envelopeDefinition.Recipients = recipients;
        }

        private Tabs GenerateCustomTabsFromConfiguration()
        {
            try
            {
                string tabsJsonFile = _configuration["DocuSign:TabsConfigurationFile"];
                string _configurationTemplatePath = Path.Combine(AppContext.BaseDirectory, "resource", tabsJsonFile);
                if (File.Exists(_configurationTemplatePath))
                {
                    using var reader = new StreamReader(_configurationTemplatePath);
                    return JsonConvert.DeserializeObject<Tabs>(reader.ReadToEnd());
                }
            }
            catch (Exception)
            {

                throw;
            }
            return null;
        }
    }
}
