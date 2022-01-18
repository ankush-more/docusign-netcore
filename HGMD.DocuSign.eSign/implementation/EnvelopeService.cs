using DocuSign.eSign.Model;
using HGMD.DocuSign.eSign.contracts;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
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
            string envelopeDocument = _configuration["DocuSign:SigningDocument"];

            byte[] buffer = System.IO.File.ReadAllBytes(envelopeDocument);

            EnvelopeDefinition envelopeDefinition = new EnvelopeDefinition();
            envelopeDefinition.EmailSubject = "Please sign this document";
            envelopeDefinition.BrandId = "044e4164-9ca5-4f43-ab01-399bf289fbc0";
            envelopeDefinition.EnableWetSign = "false";

            Document document = new Document();
            document.DocumentBase64 = Convert.ToBase64String(buffer);
            document.Name = "Terms and Conditions";
            document.FileExtension = "docx";
            document.DocumentId = "1";

            envelopeDefinition.Documents = new List<Document> { document };
            envelopeDefinition.Status = "sent";

            return envelopeDefinition;
        }
    }
}
