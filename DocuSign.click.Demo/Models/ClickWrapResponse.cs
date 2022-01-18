using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocuSign.ClickWrap.Demo.Models
{
    public class ClickWrapResponse
    {
        public string Identifier { get; set; }
        public string DocumentName { get; set; }
        public string Version { get; set; }
        public string Status { get; set; }
        public DateTime AgreedOn { get; set; }
    }
}
