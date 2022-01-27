using DocuSign.eSign.Model;
using HGMD.DocuSign.eSign.contracts;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGMD.DocuSign.eSign.implementation
{
    public class EnvelopeService : IEnvelopeService
    {
        private readonly IConfiguration _configuration;
        public EnvelopeDefinition _envDefinitionTemplate { get; set; }

        public EnvelopeService(IConfiguration configuration)
        {
            _configuration = configuration;
            _envDefinitionTemplate = GenerateEnvelopeTemplate();
        }

        private EnvelopeDefinition GenerateEnvelopeTemplate()
        {
            EnvelopeDefinition envelopeDefinition = new EnvelopeDefinition();

            string signingDocument = _configuration["DocuSign:SigningDocument"];
            string documentTitle = _configuration["DocuSign:DocumentTitle"];
            string documentPath = Path.Combine(AppContext.BaseDirectory, "resource", signingDocument);

            if(File.Exists(documentPath))
            {
                Document document = new Document();
                byte[] buffer = System.IO.File.ReadAllBytes(documentPath);
                document.DocumentBase64 = Convert.ToBase64String(buffer);
                document.Name = documentTitle ?? signingDocument.Split(".")[0];
                document.FileExtension = signingDocument.Split(".")[1] ?? "docx";
                document.DocumentId = "1";

                envelopeDefinition.EmailSubject = _configuration["DocuSign:EmailSubject"];
                envelopeDefinition.BrandId = _configuration["DocuSign:BrandId"];
                envelopeDefinition.EnableWetSign = _configuration["DocuSign:EnableWetSign"];

                envelopeDefinition.Documents = new List<Document> { document };
                envelopeDefinition.Status = "sent";
            }

            return envelopeDefinition;
        }
    }
}
