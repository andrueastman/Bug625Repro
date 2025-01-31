﻿using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace Bug625Repro.Controllers
{
    public class TestController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        private static GraphServiceClient GetGraphClient()
        {
            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var options = new TokenCredentialOptions { AuthorityHost = AzureAuthorityHosts.AzurePublicCloud };
            var clientSecretCredential = new ClientSecretCredential(string.Empty, string.Empty, string.Empty); // TenantId, ClientId, ClientSecret, options);
            return new GraphServiceClient(clientSecretCredential, scopes);
        }
        public static async Task TestSync(string mailFolderId, string notPrintedFolderId)
        {
            try
            {
                var Client = GetGraphClient();
                var inboxMessagesResponse = await Client
                    .Me
                    .MailFolders[mailFolderId]
                    .Messages
                    .GetAsync(requestConfiguration =>
                    {
                        requestConfiguration.QueryParameters.Select = new string[] { "id", "displayName" };
                        requestConfiguration.QueryParameters.Top = int.MaxValue;
                    });

                var pageIterator = PageIterator<Message, MessageCollectionResponse>
                    .CreatePageIterator(Client, inboxMessagesResponse!,
                        (message) =>
                        {
                            Thread.Sleep(1);
                            return true;
                        });
                await pageIterator.IterateAsync();
            }
            catch (Exception)
            {
                //
            }
        }
        public static async Task TestAsyncSync(string mailFolderId, string notPrintedFolderId)
        {
            try
            {
                var Client = GetGraphClient();
                var inboxMessagesResponse = await Client
                    .Me
                    .MailFolders[mailFolderId]
                    .Messages
                    .GetAsync(requestConfiguration =>
                    {
                        requestConfiguration.QueryParameters.Select = new string[] { "id", "displayName" };
                        requestConfiguration.QueryParameters.Top = int.MaxValue;
                    });

                var pageIterator = PageIterator<Message, MessageCollectionResponse>
                    .CreatePageIterator(Client, inboxMessagesResponse!,
                        async (message) =>
                        {
                            await Task.Delay(1);
                            return true;
                        });
                await pageIterator.IterateAsync();
            }
            catch (Exception)
            {
                //
            }
        }
    }
}
