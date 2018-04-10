namespace gdi_framework
{
    using Microsoft.VisualBasic.CompilerServices;
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    public class PHPC
    {
        private string Password;
        private Uri URL;
        private bool UseAuth;
        private string Username;

        public PHPC(Uri URL, bool UseAuth, string Username = "Anonymous", string Password = "Anonymous")
        {
            this.URL = URL;
            NameValueCollection data = new NameValueCollection();
            data = DataInjector.Foundation(data);
            string str = WebSocket.SendRequest(this.URL, data);
            if (str.ToLower().Contains("request;success"))
            {
                if (UseAuth)
                {
                    throw new Exception("Authorization is not required for this server");
                }
                this.UseAuth = UseAuth;
                this.Username = Username;
                this.Password = Password;
            }
            else
            {
                if (!str.ToLower().Contains("request;failure"))
                {
                    throw new Exception($"Server returned an unknown response {str}");
                }
                if (str.ToLower().Contains("(error0@1) = auth required"))
                {
                    if (!UseAuth)
                    {
                        throw new Exception("Authorization is required for this server");
                    }
                    data = DataInjector.AddAuth(data, Username, Password);
                    str = WebSocket.SendRequest(this.URL, data);
                    if (str.ToLower().Contains("request;failure"))
                    {
                        if (str.ToLower().Contains("(error1@1)"))
                        {
                            throw new Exception("Authorization failure, Incorrect username or password");
                        }
                        throw new Exception($"Server returned an unknown response {str}");
                    }
                    if (str.ToLower().Contains("request;success"))
                    {
                        this.UseAuth = UseAuth;
                        this.Username = Username;
                        this.Password = Password;
                    }
                }
                else
                {
                    if (str.ToLower().Contains("(error 2@2)"))
                    {
                        throw new Exception("The server does not support this client's version");
                    }
                    throw new Exception($"Server returned an unknown response {str}");
                }
            }
        }

        public string SendRequest(Vodka Data)
        {
            NameValueCollection data = new NameValueCollection();
            data = DataInjector.Foundation(data);
            if (this.UseAuth)
            {
                data = DataInjector.AddAuth(data, this.Username, this.Password);
            }
            data.Add("Request-Data", Vodka.Encode(Data));
            return WebSocket.SendRequest(this.URL, data);
        }

        public class DataInjector
        {
            public static NameValueCollection AddAuth(NameValueCollection Data, string Username = "Anonymous", string Password = "Anonymous")
            {
                PHPC.Vodka data = new PHPC.Vodka();
                data.Add("user", Username);
                data.Add("pswd", Password);
                Data.Add("Client-Auth", PHPC.Vodka.Encode(data));
                return Data;
            }

            public static NameValueCollection Foundation(NameValueCollection Data)
            {
                Data.Add("Client", "PHP-C/2 SecuredWebSocket - SecuredVodka");
                Data.Add("Client-Version", "31000");
                return Data;
            }
        }

        public class Vodka
        {
            private string CurrentSyntax = "";

            public void Add(string Key, string Value)
            {
                if (this.CurrentSyntax.Count<char>() > 0)
                {
                    this.CurrentSyntax = this.CurrentSyntax + $","{Key}": "{Value}"";
                }
                else
                {
                    this.CurrentSyntax = this.CurrentSyntax + $""{Key}": "{Value}"";
                }
            }

            public void Clear()
            {
                this.CurrentSyntax = "";
            }

            public static string Encode(PHPC.Vodka Data) => 
                Convert.ToBase64String(Encoding.UTF8.GetBytes("{" + Data.ToString() + "}"));

            public override string ToString() => 
                this.CurrentSyntax;
        }

        public class WebSocket
        {
            public static string SendRequest(Uri URL, NameValueCollection Data)
            {
                IEnumerator enumerator;
                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(URL);
                try
                {
                    enumerator = Data.Keys.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        IEnumerator enumerator2;
                        object objectValue = RuntimeHelpers.GetObjectValue(enumerator.Current);
                        try
                        {
                            object[] objArray;
                            bool[] flagArray;
                            object[] objArray1 = new object[] { objectValue };
                            bool[] flagArray1 = new bool[] { true };
                            if (flagArray[0])
                            {
                                objectValue = RuntimeHelpers.GetObjectValue(objArray[0]);
                            }
                            enumerator2 = ((IEnumerable) NewLateBinding.LateGet(Data, null, "GetValues", objArray = objArray1, null, null, flagArray = flagArray1)).GetEnumerator();
                            while (enumerator2.MoveNext())
                            {
                                string str = Conversions.ToString(enumerator2.Current);
                                object[] objArray2 = new object[] { objectValue, str };
                                bool[] flagArray2 = new bool[] { true, true };
                                NewLateBinding.LateCall(request.Headers, null, "Add", objArray = objArray2, null, null, flagArray = flagArray2, true);
                                if (flagArray[0])
                                {
                                    objectValue = RuntimeHelpers.GetObjectValue(objArray[0]);
                                }
                                if (flagArray[1])
                                {
                                    str = (string) Conversions.ChangeType(RuntimeHelpers.GetObjectValue(objArray[1]), typeof(string));
                                }
                            }
                            continue;
                        }
                        finally
                        {
                            if (enumerator2 is IDisposable)
                            {
                                (enumerator2 as IDisposable).Dispose();
                            }
                        }
                    }
                }
                finally
                {
                    if (enumerator is IDisposable)
                    {
                        (enumerator as IDisposable).Dispose();
                    }
                }
                return new StreamReader(((HttpWebResponse) request.GetResponse()).GetResponseStream(), Encoding.UTF8).ReadToEnd();
            }
        }
    }
}

