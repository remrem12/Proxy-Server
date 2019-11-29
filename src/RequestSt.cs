using System.Net;

namespace ProjectSocket
{
    //Chứa request client vào request chuyển tiếp
    public class RequestSt
    {
        public readonly HttpWebRequest webRequest; //Chứa request chuyển tiếp
        public readonly HttpListenerContext context; //Chứa request ban đầu

        public RequestSt(HttpWebRequest webRequest, HttpListenerContext context) //Constructor
        {
            this.webRequest = webRequest;
            this.context = context;
        }
    }
}