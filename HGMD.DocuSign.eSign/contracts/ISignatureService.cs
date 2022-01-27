using DocuSign.eSign.Model;
using HGMD.DocuSign.eSign.Entities;
using System.Threading.Tasks;

namespace HGMD.DocuSign.eSign.contracts
{
    public interface ISignatureService
    {
        public Task<ViewUrl> GenerateEnvelopeRedirectURL(DocumentSigner documentSigner);
    }
}
