using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Net;
using System.Dynamic;
using System.IO;
using System.Web.Configuration;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Persistent_Menu_Facebook.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public const string BASE_URI = "https://graph.facebook.com/v2.6/me/messenger_profile?";
        public static string PAGE_ACCESS_TOKEN = WebConfigurationManager.AppSettings["FacebookAccessToken"];


        public enum Options
        {
            GetStarted, ActivateMenu, ShowMenu, DeleteMenu
        };

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            Object _result = null;
            string Data = string.Empty;
            string Uri = BASE_URI + "access_token=" + PAGE_ACCESS_TOKEN;
            Options option;
            if (Enum.TryParse(activity.Text, out option))
            {
                switch (option)
                {
                    case Options.ActivateMenu:
                        TextReader tr = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "PersistentMenu.json");
                        Data = tr.ReadToEnd();
                        _result = HttpRequestHelper(BASE_URI + "access_token=" + PAGE_ACCESS_TOKEN, "POST", Data);
                        await context.PostAsync($"{_result}");
                        break;

                    case Options.ShowMenu:
                        _result = HttpRequestHelper(BASE_URI + "fields=persistent_menu&access_token=" + PAGE_ACCESS_TOKEN, "GET", null);
                        await context.PostAsync($"{_result}");
                        break;

                    case Options.DeleteMenu:
                        Data = "{'fields':['persistent_menu']}";
                        _result = HttpRequestHelper(BASE_URI + "access_token=" + PAGE_ACCESS_TOKEN, "DELETE", Data);
                        await context.PostAsync($"{_result}");
                        break;

                    case Options.GetStarted:
                        dynamic JsonData = new ExpandoObject();
                        JsonData.get_started = new ExpandoObject();
                        JsonData.get_started.payload = "GET_STARTED_PAYLOAD";
                        _result = HttpRequestHelper(BASE_URI + "access_token=" + PAGE_ACCESS_TOKEN, "POST", JsonData);
                        await context.PostAsync($"{_result}");
                        break;

                    default:
                        break;
                }
            }
            else
            {
                string optionsString = string.Empty;
                foreach (Options op in EnumUtil.GetValues<Options>()) { optionsString += op + ", "; };
                await context.PostAsync($"You sent {activity.Text}. The available options are:  {optionsString}");
            }
            context.Wait(MessageReceivedAsync);
        }

        private static Object HttpRequestHelper(string Uri, string Method, dynamic JsonData)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(Uri);
            request.ContentType = "application/json; charset=utf-8";
            request.Method = Method;
            string Data = string.Empty;

            if (!Method.Equals("GET"))
            {
                using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(JsonData);
                }
            }

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                Data = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject(Data);
        }
        public static class EnumUtil
        {
            public static IEnumerable<T> GetValues<T>()
            {
                return Enum.GetValues(typeof(T)).Cast<T>();
            }
        }
    }
}