using DocuSign.eSign.Model;
using HGMD.DocuSign.eSign.Entities;
using System;
using System.Collections.Generic;

namespace HGMD.DocuSign.eSign.Helper
{
    public class EnvelopeHelper
    {
        public static RecipientViewRequest MakeRecipientViewRequest(DocumentSigner documentSigner, string returnUrl, string pingUrl = null)
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
            viewRequest.Email = documentSigner.EmailId;
            viewRequest.UserName = documentSigner.Name;
            viewRequest.ClientUserId = documentSigner.ClientId;

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
    }
}
