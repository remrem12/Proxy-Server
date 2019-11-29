using System;
using System.IO;
using System.Net;
using System.Text;


namespace ProjectSocket
{
    public class Process
    {
        private readonly HttpListenerContext ClientContext;

        public Process(HttpListenerContext ClientContext) //Contructor
        {
            this.ClientContext = ClientContext;
        }

        public void ProcessRequest()
        {
            string Url = ClientContext.Request.RawUrl; //Lấy url from client request

            bool checkBlacklist = false; // Biến kiểm tra url có trong Blacklist hay không

            using (StreamReader streamR = new StreamReader("Blacklist.conf"))
            {
                string BlacklistUrl = "";
                while ((BlacklistUrl = streamR.ReadLine()) != null) // Kiểm tra xem tới cuối dòng hay chưa
                {
                    if (Url.Contains(BlacklistUrl))
                    {
                        checkBlacklist = true; //Nếu nó trong blacklist thì biến kiểm tra = true
                        break;
                    }
                }
            }

            if (checkBlacklist) //Nếu url có trong blacklist thì ta thực hiện hàm BlacklistCallbackResponse
                BlacklistCallbackResponse((HttpListenerResponse)ClientContext.Response);

            else //Ngược lại send request tới web sever rồi tiến hành hàm CallbackResponse
            {
                //Đổi màu để phân biệt request
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Received a REQUEST for url: " + Url);
                Console.ResetColor();
                //-----------------------------------

                HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(Url); //Tạo biến httpwebrequest để send request từ url
                
                //Định dạng loại request
                Request.Proxy.Credentials = CredentialCache.DefaultCredentials;
                Request.UserAgent = ClientContext.Request.UserAgent;
                Request.Method = ClientContext.Request.HttpMethod; 
                Request.ContentType = "APPLICATION/XML; CHARSET=UTF-8";

                RequestSt requestData = new RequestSt(Request, ClientContext);
                Request.BeginGetResponse(CallbackResponse, requestData);
            }
        }

        private static void CallbackResponse(IAsyncResult asynchronousResult)
        {
            RequestSt requestData = (RequestSt)asynchronousResult.AsyncState;
            //Đổi màu để phân biệt response
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine("Received a RESPONSE from url: " + requestData.context.Request.RawUrl);
            Console.ResetColor();
            //-----------------------------------

            using (HttpWebResponse GetWebResponse = (HttpWebResponse)requestData.webRequest.EndGetResponse(asynchronousResult))
            {
                using (Stream GetWebResponseStream = GetWebResponse.GetResponseStream())
                {
                    HttpListenerResponse ClientResponse = requestData.context.Response;

                    if (GetWebResponse.ContentType.Contains("text/html"))
                    {
                        var reader = new StreamReader(GetWebResponseStream);
                        string html = reader.ReadToEnd();

                        byte[] byteArray = Encoding.UTF8.GetBytes(html);
                        var stream = new MemoryStream(byteArray);
                        stream.CopyTo(ClientResponse.OutputStream);
                    }
                    else
                    {
                        GetWebResponseStream.CopyTo(ClientResponse.OutputStream);
                    }

                    
                    //----------------------------------
                    ClientResponse.OutputStream.Close();
                }
            }
        }

        private static void BlacklistCallbackResponse(HttpListenerResponse ClientResponse)
        {
            Console.WriteLine("Can not access to this site.");
            ClientResponse.StatusCode = (int)HttpStatusCode.Forbidden;
            ClientResponse.StatusDescription = "403 (Forbidden) HTTP response.";

            byte[] byteArray = Encoding.Default.GetBytes(ClientResponse.StatusDescription);
            MemoryStream stream = new MemoryStream(byteArray);
            stream.CopyTo(ClientResponse.OutputStream);
            //----------------------------------
            ClientResponse.OutputStream.Close();
        }
    //------------------------------------------
    }
}
