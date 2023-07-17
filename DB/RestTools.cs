using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace DB
{
    public class RestTools
    {
        RestClient client;
        RestRequest request;
        string url;

        public RestTools(string url)
        {
            this.client = new RestClient(url);
            this.url = url;
        }

        public void Request(string resource, Method method)
        {
            this.request = new RestRequest(url, method);
        }

        public void AddHeader(string name, string value)
        {
            this.request.AddHeader(name, value);
        }

        public void AddParameter(string name, string value)
        {
            this.request.AddParameter(name, value);
        }

        public string Execute()
        {
            try
            {
                RestResponse response = this.client.Execute(this.request);
                return response.Content;
            }
            catch (Exception e)
            {
                return "Error: Execute() " + e.Message;
            }
        }

        public async Task<string> ExecuteAsync()
        {
            try
            {
                RestResponse response = await this.client.ExecuteAsync(this.request);
                return response.Content;
            }
            catch (Exception e)
            {
                return "Error: ExecuteAsync() " + e.Message;
            }
        }

    }
}
