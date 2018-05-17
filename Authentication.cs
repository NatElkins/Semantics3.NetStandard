/* Copyright (c) 2006 Google Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*     http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.IO;
using System.Collections;
using System.Text;
using System.Net;
using Google.GData.Client;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Google.GData.Client
{
    public interface ICreateHttpRequest
    {
        HttpWebRequest Create(Uri target);
    }

    public class HttpRequestFactory : ICreateHttpRequest
    {
        public HttpWebRequest Create(Uri target)
        {
            return WebRequest.Create(target) as HttpWebRequest;
        }
    }

    /// <summary>
    /// this is the static collection of all google service names
    /// </summary>
    public static class ServiceNames
    {
        public static string YouTube = "youtube";
        public static string Calendar = "cl";
        public static string Documents = "writely";
    }

    /// <summary>
    /// Base authentication class. Takes credentials and applicationname
    /// and is able to create a HttpWebRequest augmented with the right
    /// authentication
    /// </summary>
    /// <returns></returns>
    public abstract class Authenticator
    {
        private string applicationName;
        private string developerKey;
        private ICreateHttpRequest requestFactory;

        /// <summary>
        /// an unauthenticated use case
        /// </summary>
        /// <param name="applicationName"></param>
        /// <returns></returns>
        public Authenticator(string applicationName)
        {
            this.applicationName = applicationName;
            this.requestFactory = new HttpRequestFactory();
        }

        public ICreateHttpRequest RequestFactory
        {
            get
            {
                return this.requestFactory;
            }
            set
            {
                this.requestFactory = value;
            }
        }

        /// <summary>
        /// Creates a HttpWebRequest object that can be used against a given service. 
        /// for a RequestSetting object that is using client login, this might call 
        /// to get an authentication token from the service, if it is not already set.
        /// 
        /// if this uses client login, and you need to use a proxy, set the application wide
        /// proxy first using the GlobalProxySelection
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="httpMethod"></param>
        /// <param name="targetUri"></param>
        /// <returns></returns>
        public HttpWebRequest CreateHttpWebRequest(string httpMethod, Uri targetUri)
        {
            Uri uriResult = ApplyAuthenticationToUri(targetUri);

            if (this.requestFactory != null)
            {
                HttpWebRequest request = this.requestFactory.Create(uriResult);
                // turn off autoredirect
                request.AllowAutoRedirect = false;
                request.Method = httpMethod;
                ApplyAuthenticationToRequest(request);
                return request;
            }
            return null;
        }

        /// <summary>
        /// returns the application name
        /// </summary>
        /// <returns></returns>
        public string Application
        {
            get
            {
                return this.applicationName;
            }
        }

        /// <summary>
        /// primarily for YouTube. allows you to set the developer key used
        /// </summary>
        public string DeveloperKey
        {
            get
            {
                return this.developerKey;
            }
            set
            {
                this.developerKey = value;
            }
        }

        /// <summary>
        /// Takes an existing httpwebrequest and modifies its headers according to 
        /// the authentication system used.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual void ApplyAuthenticationToRequest(HttpWebRequest request)
        {
            // /// adds the developer key if present
            // if (this.DeveloperKey != null)
            // {
            //     string strHeader = GoogleAuthentication.YouTubeDevKey + this.DeveloperKey;
            //     request.Headers.Add(strHeader);
            // }
        }

        /// <summary>
        /// Takes an existing httpwebrequest and modifies its uri according to 
        /// the authentication system used. Only overridden in 2-leggedoauth case
        /// </summary>
        /// <param name="source">the original uri</param>
        /// <returns></returns>
        public virtual Uri ApplyAuthenticationToUri(Uri source)
        {
            return source;
        }
    }

    public abstract class OAuthAuthenticator : Authenticator
    {
        private string consumerKey;
        private string consumerSecret;

        public OAuthAuthenticator(
            string applicationName,
            string consumerKey,
            string consumerSecret)
            : base(applicationName)
        {
            this.consumerKey = consumerKey;
            this.consumerSecret = consumerSecret;
        }

        /// <summary>
        /// returns the ConsumerKey
        /// </summary>
        /// <returns></returns>
        public string ConsumerKey
        {
            get
            {
                return this.consumerKey;
            }
        }

        /// <summary>
        /// returns the ConsumerSecret
        /// </summary>
        /// <returns></returns>
        public string ConsumerSecret
        {
            get
            {
                return this.consumerSecret;
            }
        }
    }
}
