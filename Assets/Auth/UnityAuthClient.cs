using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IdentityModel.Client;
using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;
using IdentityModel.OidcClient.Infrastructure;
using IdentityModel.OidcClient.Results;
using UnityEngine;

namespace Assets
{
    public class UnityAuthClient
    {
        private OidcClient _client;
        private LoginResult _result;

        public UnityAuthClient()
        {
            // We must disable the IdentityModel log serializer to avoid Json serialize exceptions on IOS.
#if UNITY_IOS
            LogSerializer.Enabled = false;
#endif

            // On Android, we use Chrome custom tabs to achieve single-sign on.
            // On Ios, we use SFSafariViewController to achieve single-sign-on.
            // See: https://www.youtube.com/watch?v=DdQTXrk6YTk
            // And for unity integration, see: https://qiita.com/lucifuges/items/b17d602417a9a249689f (Google translate to English!)
#if UNITY_EDITOR
            Browser = new PCBrowser();
#elif UNITY_ANDROID
            Browser = new AndroidChromeCustomTabBrowser();
#elif UNITY_IOS
            Browser = new SFSafariViewBrowser();
#endif
            CertificateHandler.Initialize();
        }

        // Instead of using AppAuth, which is not available for Unity apps, we are using
        // this library: https://github.com/IdentityModel/IdentityModel.OidcClient2
        // .Net 4.5.2 binaries have been built from the above project and included in
        // /Assets/Plugins folder.
        private OidcClient CreateAuthClient()
        {
            var options = new OidcClientOptions()
            {
                //Authority = "https://demo.identityserver.io/",
                //ClientId = "interactive.public",
                //Scope = "openid profile email",

                Authority = "http://connect.omoon.top:8000/",
                ClientId = "sanguoapp",
                ClientSecret = "3e198d192532488d8cb14978248af0eb",
                Scope = "openid profile offline_access",
                FilterClaims = false,

                // Redirect (reply) uri is specified in the AndroidManifest and code for handling
                // it is in the associated AndroidUnityPlugin project, and OAuthUnityAppController.mm.
                RedirectUri = "com.unclegames.sanguoapp://callback",
                PostLogoutRedirectUri = "com.unclegames.sanguoapp://callback",
                ResponseMode = OidcClientOptions.AuthorizeResponseMode.Redirect,
                Flow = OidcClientOptions.AuthenticationFlow.AuthorizationCode,
                Browser = Browser,
            };
            options.Policy.Discovery.RequireHttps = false;

#if UNITY_EDITOR
            var pcBrower = Browser as PCBrowser;
            options.RedirectUri = pcBrower.Path + ":" + pcBrower.Port;
            options.PostLogoutRedirectUri = pcBrower.Path + ":" + pcBrower.Port;
#endif


            options.LoggerFactory.AddProvider(new UnityAuthLoggerProvider());
            return new OidcClient(options);
        }

        public async Task<String> LoginAsync()
        {
            String errCode = "";

            do
            {
                _client = CreateAuthClient();

                try
                {
                    _result = await _client.LoginAsync(new LoginRequest());
                }
                catch (Exception e)
                {
                    Debug.Log("UnityAuthClient::Exception during login: " + e.Message);
                    errCode = "UnityAuthClient::Exception during login: " + e.Message;
                }
                finally
                {
                    Debug.Log("UnityAuthClient::Dismissing sign-in browser.");
                    Browser.Dismiss();
                }

                if (_result.IsError)
                {
                    Debug.Log("UnityAuthClient::Error authenticating: " + _result.Error);
                    errCode = "UnityAuthClient::Error authenticating: " + _result.Error;
                }
                else
                {
                    Debug.Log("UnityAuthClient::AccessToken: " + _result.AccessToken);
                    Debug.Log("UnityAuthClient::RefreshToken: " + _result.RefreshToken);
                    Debug.Log("UnityAuthClient::IdentityToken: " + _result.IdentityToken);
                    Debug.Log("UnityAuthClient::Signed in.");
                }

            } while (false);
            return errCode;
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                await _client.LogoutAsync(new LogoutRequest() {
                    BrowserDisplayMode = DisplayMode.Hidden,
                    IdTokenHint = _result.IdentityToken });
                Debug.Log("UnityAuthClient::Signed out successfully.");
                return true;
            }
            catch (Exception e)
            {
                Debug.Log("UnityAuthClient::Failed to sign out: " + e.Message);
            }
            finally
            {
                Debug.Log("UnityAuthClient::Dismissing sign-out browser.");
                Browser.Dismiss();
                _client = null;
            }

            return false;
        }

        public string GetUserName()
        {
            return _result == null ? "" : _result.User.Identity.Name;
        }

        public String GetIdentityToken()
        {
            return _result == null ? "" : _result.IdentityToken;
        }

        public MobileBrowser Browser { get; }
    }
}
