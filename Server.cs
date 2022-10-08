using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace printServer
{
    class Server : UAP.Net.Http.HttpServer
    {
        private readonly string contentStr = "PrintServer";
        public Server(int port)
            : base(port)
        {

            this.AddService("/", new UAP.Net.Http.Service.BaseService
            {
                CreateResponse = context =>
                {
                    var resp = new UAP.Net.Http.Base.HttpResponse
                    {
                        Content = contentStr
                    };
                    resp.Headers.Add("Access-Control-Allow-Origin", "*");
                    return resp;
                }
            });

            this.AddService(new Regex("/print/(.*)", RegexOptions.IgnoreCase), new UAP.Net.Http.Service.BaseService
            {
                CreateResponse = c =>
                {
                    Func<string, List<PrintParam>, object> _ = (k, v) =>
                    {
                        try
                        {
                            switch (k)
                            {
                                case "tspl":
                                    if (v!=null&&v.Count>0)
                                        return Program.SendCMD(v);
                                    return "parameter is null";
                                case "zpl":
                                    if (v != null && v.Count > 0) {
                                        return Program.SendZPL(v);
                                    }
                                    return "print zpl success";
                                case "demo":
                                    {
                                        List<PrintParam> ps = new List<PrintParam>();
                                        ps.Add(Program.GetParam("0000000001"));
                                        ps.Add(Program.GetParam("0000000002"));
                                        return Program.SendCMD(ps);
                                    }
                                    
                            }
                        }
                        catch (Exception e)
                        {
                            return "error：" + e.Message;
                        }
                        return contentStr;
                    };

                    var resp = new UAP.Net.Http.Base.HttpResponse();
                    resp.Headers.Add("Access-Control-Allow-Origin", "*");
                    resp.Headers.Add("Access-Control-Allow-Headers", "*");
                    if (c.Method.Equals("OPTIONS"))
                        return resp;
                    List<PrintParam> plist = new List<PrintParam>();
                    if (c.PostStream != null)
                    {
                        var strReader = new StreamReader(c.PostStream,Encoding.GetEncoding("utf-8"));
                        var OriginPostString = strReader.ReadToEnd();
                        plist = JsonConvert.DeserializeObject<List<PrintParam>>(OriginPostString);
                    }
                    resp.Content = _(c.Match.Result("$1"), plist).ToString();
                    return resp;
                }
            });

            this.AddService<UAP.Net.Http.Service.FileService>("/*");
        }

        protected override void OnHttpError(System.Net.Sockets.NetworkStream stream, Exception e)
        {
            base.OnHttpError(stream, e);
        }

        protected override void OnTcpError(System.Net.Sockets.TcpClient client, Exception e)
        {
            base.OnTcpError(client, e);
        }
    }
}
