using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB
{
    public class WhatsApp
    {
        public string SendWhastAppApi(string number, string message)
        {
            try
            {
                string url = "https://api.whatsapp.com/send?phone=" + number + "&text=" + message;
                System.Diagnostics.Process.Start(url);
                return "Success: Send()";
            }
            catch (Exception e)
            {
                return "Error: Send() " + e.Message;
            }
        }


        public string SendUltraMessage(string url, string instance, string token,  string number, string message) 
        {
            try
            {
                RestTools rest = new RestTools(url);
                rest.Request(url, Method.Post);
                rest.AddHeader("content-type", "application/x-www-form-urlencoded");
                rest.AddParameter("token", token);
                rest.AddParameter("to", number);
                rest.AddParameter("body", message);
                return rest.Execute();
            }
            catch (Exception e)
            {
                return "Error: SendUltraMessage() " + e.Message;
            }
        }

        public async Task<string> SendWhastAppApiAsync(string url, string instance, string token, string number, string message)
        {
            try
            {
                RestTools rest = new RestTools(url);
                rest.Request(url, Method.Post);
                rest.AddHeader("content-type", "application/x-www-form-urlencoded");
                rest.AddParameter("token", token);
                rest.AddParameter("to", number);
                rest.AddParameter("body", message);
                return await rest.ExecuteAsync();
            }
            catch (Exception e)
            {
                return "Error: SendWhastAppApiAsync() " + e.Message;
            }
        }

    }
}
