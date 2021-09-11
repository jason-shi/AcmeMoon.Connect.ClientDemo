using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Net;
using System.Threading;
using IdentityModel.OidcClient.Browser;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Assets
{
    public class PCBrowser : MobileBrowser
    {
        public int Port { get; set; }
        public string Path { get; set; } = "http://127.0.0.1";

        public PCBrowser()
        {
            Port = GetRandomUnusedPort();
        }

        private int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        protected override void Launch(string url)
        {
            StartHttpListener();
            // open url
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        protected void StartHttpListener()
        {
            mListener = new LoopbackHttpListener(Port) { Callback = OnHttpCallback };
        }
        protected LoopbackHttpListener mListener = null;

        protected void OnHttpCallback(String result)
        {
            OnAuthReply(result);
        }

    }

    public class LoopbackHttpListener
    {
        const int DefaultTimeout = 60 * 5; // 5 mins (in seconds)

        HttpListener _listener;
        Thread _listenerThread;

        TaskCompletionSource<string> _source = new TaskCompletionSource<string>();
        string _url;

        public string Url => _url;

        public LoopbackHttpListener(int port, String path = null)
        {
            path = path ?? String.Empty;
            if (path.StartsWith("/")) path = path.Substring(1);
            _url = $"http://127.0.0.1:{port}/{path}";

            _listener = new HttpListener();
            _listener.Prefixes.Add(_url);
            _listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            _listener.Start();

            _listenerThread = new Thread(startListener);
            _listenerThread.Start();
            UnityEngine.Debug.Log("Server Started");
        }

        public Action<String> Callback { get; set; }

        private void startListener()
        {
            //while (true)
            {
                var result = _listener.BeginGetContext(ListenerCallback, _listener);
                result.AsyncWaitHandle.WaitOne();
            }
        }

        private void ListenerCallback(IAsyncResult result)
        {
            var context = _listener.EndGetContext(result);

            UnityEngine.Debug.Log("Method: " + context.Request.HttpMethod);
            UnityEngine.Debug.Log("LocalUrl: " + context.Request.Url.LocalPath);
            UnityEngine.Debug.Log("Callback: " + context.Request.RawUrl);

            {
                context.Response.StatusCode = 200;
                context.Response.ContentType = "text/html";
                var respContent = System.Text.Encoding.UTF8.GetBytes("<h1>You can now return to the application.</h1>");
                context.Response.OutputStream.Write(respContent, 0, respContent.Length);
                context.Response.OutputStream.Flush();
            }
            context.Response.Close();

            if(null != Callback)
                Callback(context.Request.RawUrl);
        }

    }
}