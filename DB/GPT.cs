using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenAI_API;
using OpenAI_API.Chat;

namespace AngelDB
{
    public class OpenAIChatbot
    {
        private string ApiKey = "tu_clave_api_aquí";
        private string Endpoint = "https://api.openai.com/v1/engines/davinci-codex/completions";
        private int maxTokens = 4000;
        private string model = "text-davinci-003";
        private static HttpClient _httpClient;
        private DbLanguage lex =null;
        private OpenAIAPI api = null;

        public Conversation chat = null;
        public AngelDB.DB db = null;

        public OpenAIChatbot(AngelDB.DB db)
        {
            lex = new DbLanguage();
            lex.SetCommands(GPTCommands.Commands());
            _httpClient = new HttpClient();
            this.db = db;
        }


        public string ProcessCommand(string command) 
        {
            Dictionary<string, string> d = lex.Interpreter(command);

            if (!string.IsNullOrEmpty(lex.errorString)) return lex.errorString;
            string commandkey = d.First().Key;

            switch (commandkey) 
            {
                case "set_api_key":
                    return this.SetApiKey(d["set_api_key"]);
                case "set_end_point":
                    return this.SetEndpoint(d["set_end_point"]);
                case "set_max_tokens":
                    return this.SetMaxTokens(int.Parse(d["set_max_tokens"]));
                case "save_account_to":
                    return SaveAccountsTo(d["save_account_to"], d["password"]);
                case "restore_account_from":
                    return RestorAccount(d["save_account_to"], d["password"]);
                case "start_chat":
                    return StartChat(d);
                case "add_user_input":
                    return AddUserInput(d);
                case "add_example_output":
                    return AddExampleOutput(d);
                case "get_models":
                    return GetAvailableModels().Result;
                case "prompt_preview":
                    return PromptPreview(d["prompt_preview"]);
                case "prompt":
                    return Prompt(d[commandkey]).Result;
                default:
                    return "Error: Command not found.";
            }
        }

        public string SetApiKey(string apiKey)
        {
            ApiKey = apiKey;
            return "Ok.";
        }

        public string SetEndpoint(string endpoint)
        {
            Endpoint = endpoint;
            return "Ok.";
        }

        public string SetMaxTokens(int maxTokens)
        {
            this.maxTokens = maxTokens;
            return "Ok.";
        }


        public string SaveAccountsTo(string file, string password)
        {
            try
            {

                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("api_key", ApiKey);
                //dic.Add("endpoint", Endpoint);
                //dic.Add("max_tokens", maxTokens.ToString());

                string json = JsonConvert.SerializeObject(dic);
                return AngelDBTools.StringFunctions.SaveEncriptedFile(file, json, password);
            }
            catch (Exception e)
            {
                return $"Error: {e}";
            }
        }

        public string RestorAccount(string file, string password)
        {
            try
            {

                if (!File.Exists(file))
                {
                    return $"Error: The file does not exists {file}";
                }

                string json = AngelDBTools.StringFunctions.RestoreEncriptedFile(file, password);

                Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                this.ApiKey = dic["api_key"];
                //this.Endpoint = dic["endpoint"];
                //this.maxTokens = int.Parse(dic["max_tokens"]);
                return "Ok.";

            }
            catch (Exception e)
            {
                return $"Error: {e}";
            }
        }


        public string StartChat(Dictionary<string, string> d)
        {
            try
            {

                if (string.IsNullOrEmpty(this.ApiKey)) 
                {
                    return "Error: StartChat(): ApiKey is empty.";
                }

                api = new OpenAIAPI(ApiKey);
                ChatRequest chatRequest = new ChatRequest();
                //chatRequest.MaxTokens = maxTokens;
                //chatRequest.Model = "davinci-codex";

                chat = api.Chat.CreateConversation();

                if (string.IsNullOrEmpty(d["start_chat"])) 
                {
                    chat.AppendSystemMessage(d["start_chat"]);
                }

                return "Ok.";
            }
            catch (Exception e)
            {
                return $"Error: StartChat() {e}";
            }
        }


        public string AddUserInput(Dictionary<string, string> d)
        {
            try
            {

                if (string.IsNullOrEmpty(this.ApiKey))
                {
                    return "Error: StartChat(): ApiKey is empty.";
                }

                if( chat == null)
                {
                    return "Error: AddUserInput(): Chat is null. Use START CHAT command";
                }

                chat.AppendUserInput(d["add_user_input"]);

                return "Ok.";
            }
            catch (Exception e)
            {
                return $"Error: AddUserInput() {e}";
            }
        }

        public string AddExampleOutput(Dictionary<string, string> d)
        {
            try
            {

                if (string.IsNullOrEmpty(this.ApiKey))
                {
                    return "Error: AddExampleOutput(): ApiKey is empty.";
                }

                if (chat == null)
                {
                    return "Error: AddExampleOutput(): Chat is null. Use START CHAT command";
                }

                chat.AppendExampleChatbotOutput(d["add_example_output"]);

                return "Ok.";
            }
            catch (Exception e)
            {
                return $"Error: AddExampleOutput() {e}";
            }
        }


        public string GetChat(Dictionary<string, string> d)
        {
            try
            {

                if (string.IsNullOrEmpty(this.ApiKey))
                {
                    return "Error: GetChat(): ApiKey is empty.";
                }

                if (chat == null)
                {
                    return "Error: GetChat(): Chat is null. Use START CHAT command";
                }


                return "Ok.";
            }
            catch (Exception e)
            {
                return $"Error: AddExampleOutput() {e}";
            }
        }


        public async Task<string> GetAvailableModels()
        {
            try
            {
                WebHeaderCollection headers = new WebHeaderCollection();
                headers.Add("Authorization", $"Bearer {this.ApiKey}");
                return await AngelDB.Curl.Get(" https://api.openai.com/v1/models", headers);

            }
            catch (Exception e)
            {
                return $"Error: GetAvailableModels() {e}";
            }

        }

        public string PromptPreview(string prompt)
        {
            try
            {

                List<string> tokens = Tokenizer.Tokenize(prompt, "<&", "&>");

                StringBuilder sb = new StringBuilder();

                foreach (var item in tokens)
                {
                    if (item.StartsWith("^"))
                    {
                        string key = item.Substring(1);
                        string value = this.db.Prompt(key);
                        sb.AppendLine(value.Trim());
                        sb.AppendLine();
                    }
                    else 
                    {
                        sb.AppendLine(item.Trim());
                        sb.AppendLine();
                    }                    
                }

                return sb.ToString();

            }
            catch (Exception e)
            {
                return $"Error: Prompt() {e}";
            }
        }


        public async Task<string> Prompt(string prompt)
        {
            try
            {

                if (string.IsNullOrEmpty(this.ApiKey))
                {
                    return "Error: Prompt(): ApiKey is empty.";
                }

                if (chat == null)
                {
                    return "Error: Prompt(): Chat is null. Use START CHAT command";
                }

                prompt = PromptPreview(prompt);
                chat.AppendUserInput(prompt);
                return await chat.GetResponseFromChatbotAsync();

            }
            catch (Exception e)
            {
                return $"Error: Prompt() {e}";
            }
        }
    }
}
