using DocuSign.eSign.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGMD.DocuSign.eSign.contracts
{
    public interface IEnvelopeService
    {
        public EnvelopeDefinition _envDefinitionTemplate { get; set; }
    }
}
