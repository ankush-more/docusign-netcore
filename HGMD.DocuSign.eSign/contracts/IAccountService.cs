using Microsoft.Extensions.Configuration;
using static DocuSign.eSign.Client.Auth.OAuth;
using static DocuSign.eSign.Client.Auth.OAuth.UserInfo;

namespace HGMD.DocuSign.eSign.contracts
{
    public interface IAccountService
    {
        public (OAuthToken, Account) Initialize();
    }
}
