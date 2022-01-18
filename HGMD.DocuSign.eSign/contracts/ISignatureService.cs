using HGMD.DocuSign.eSign.Entities;

namespace HGMD.DocuSign.eSign.contracts
{
    public interface ISignatureService
    {
        public string GenerateEnvelopeRedirectURL(DocumentSigner documentSigner);
    }
}
