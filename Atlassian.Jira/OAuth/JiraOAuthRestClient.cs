﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Atlassian.Jira.Remote;
using RestSharp;
using RestSharp.Authenticators;

namespace Atlassian.Jira.OAuth
{
    /// <summary>
    /// Reimplements the <see cref="JiraRestClient"/> to use the OAuth protocol.
    /// </summary>
    public class JiraOAuthRestClient : JiraRestClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JiraOAuthRestClient"/> class.
        /// </summary>
        /// <param name="url">The url of the Jira instance to request to.</param>
        /// <param name="consumerKey">The consumer key provided by the Jira application link.</param>
        /// <param name="consumerSecret">The consumer private key in XML format.</param>
        /// <param name="oAuthAccessToken">The OAuth access token obtained from Jira.</param>
        /// <param name="oAuthTokenSecret">The OAuth token secret generated by Jira.</param>
        /// <param name="oAuthSignatureMethod">The signature method used to sign the request.</param>
        /// <param name="settings">The settings used to configure the rest client.</param>
        public JiraOAuthRestClient(
            string url,
            string consumerKey,
            string consumerSecret,
            string oAuthAccessToken,
            string oAuthTokenSecret,
            JiraOAuthSignatureMethod oAuthSignatureMethod = JiraOAuthSignatureMethod.RsaSha1,
            JiraRestClientSettings settings = null)
            : base(
                url,
                OAuth1Authenticator.ForProtectedResource(
                    consumerKey,
                    consumerSecret,
                    oAuthAccessToken,
                    oAuthTokenSecret,
                    oAuthSignatureMethod.ToOAuthSignatureMethod()),
                settings)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Replace the request query with a collection of parameters.
        /// </summary>
        protected override Task<RestResponse> ExecuteRawResquestAsync(RestRequest request, CancellationToken token)
        {
            Uri fullPath = new Uri(new Uri(Url), new Uri(request.Resource));

            // Move the query parameters to the request parameters.
            if (!string.IsNullOrEmpty(fullPath.Query))
            {
                foreach (var parameter in QueryParametersHelper.GetParametersFromPath(fullPath.Query))
                {
                    request.AddParameter(name: parameter.Name, value: parameter.Value, type: parameter.Type);
                }

                request.Resource = request.Resource.Replace(fullPath.Query, string.Empty);
            }

            return base.ExecuteRawResquestAsync(request, token);
        }
    }
}
