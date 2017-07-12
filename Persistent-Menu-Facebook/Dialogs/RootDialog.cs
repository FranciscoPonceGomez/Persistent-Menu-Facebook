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

namespace Persistent_Menu_Facebook.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public const string BASE_URI = "https://graph.facebook.com/v2.6/me/messenger_profile?";

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
            string PAGE_ACCESS_TOKEN = WebConfigurationManager.AppSettings["FacebookAccessToken"];
            HttpWebRequest request;
            //string option = activity.Text;
            Object _result = null;
            string Data = string.Empty;
            string Uri = BASE_URI + "access_token=" + PAGE_ACCESS_TOKEN;
            Options option;
            string optionsString = string.Empty;
            foreach (Options op in EnumUtil.GetValues<Options>()) { optionsString += op + ", "; };
            if (Enum.TryParse(activity.Text, out option))
            {
                switch (option)
                {
                    case Options.ActivateMenu:

                        Data = "{" +
                         "'persistent_menu':[" +
                           "{" +
                             "'locale':'default'," +
                             "'composer_input_disabled':true," +
                             "'call_to_actions':[" +
                               "{" +
                                 "'title':'My Account', " +
                                 "'type':'nested', " +
                                 "'call_to_actions':[ " +
                                   "{" +
                                     "'title':'Pay Bill', " +
                                     "'type':'postback'," +
                                     "'payload':'PAYBILL_PAYLOAD'" +
                                   "}," +
                                   "{" +
                                     "'title':'History'," +
                                     "'type':'postback'," +
                                     "'payload':'HISTORY_PAYLOAD'" +
                                   "}," +
                                   "{" +
                                     "'title':'Contact Info'," +
                                     "'type':'postback'," +
                                     "'payload':'CONTACT_INFO_PAYLOAD'" +
                                   "}" +
                                 "]" +
                               "}," +
                               "{" +
                                 "'type':'web_url'," +
                                 "'title':'Latest News'," +
                                 "'url':'http://petershats.parseapp.com/hat-news'," +
                                 "'webview_height_ratio':'full'" +
                               "}" +
                             "]" +
                           "}," +
                           "{" +
                             "'locale':'zh_CN'," +
                             "'composer_input_disabled':false" +
                           "}" +
                         "]" +
                       "}";

                        request = (HttpWebRequest)HttpWebRequest.Create("https://graph.facebook.com/v2.6/me/messenger_profile?access_token=" + PAGE_ACCESS_TOKEN);
                        request.ContentType = "application/json; charset=utf-8";
                        request.Method = "POST";

                        using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
                        {
                            streamWriter.Write(Data);
                        }

                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            Stream dataStream = response.GetResponseStream();
                            StreamReader reader = new StreamReader(dataStream);
                            Data = reader.ReadToEnd();
                            reader.Close();
                            dataStream.Close();
                        }
                        _result = Newtonsoft.Json.JsonConvert.DeserializeObject(Data);
                        await context.PostAsync($"{_result}");
                        break;

                    case Options.ShowMenu:

                        _result = HttpRequestHelper(BASE_URI + "fields=persistent_menu&access_token=" + PAGE_ACCESS_TOKEN, "GET", null);
                        await context.PostAsync($"{_result}");

                        break;

                    case Options.DeleteMenu:
                        request = (HttpWebRequest)HttpWebRequest.Create("https://graph.facebook.com/v2.6/me/messenger_profile?access_token=" + PAGE_ACCESS_TOKEN);
                        request.ContentType = "application/json; charset=utf-8";
                        request.Method = "DELETE";

                        using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
                        {
                            Data = "{'fields':['persistent_menu']}";
                            streamWriter.Write(Data);
                        }

                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            Stream dataStream = response.GetResponseStream();
                            StreamReader reader = new StreamReader(dataStream);
                            Data = reader.ReadToEnd();
                            reader.Close();
                            dataStream.Close();
                        }
                        _result = Newtonsoft.Json.JsonConvert.DeserializeObject(Data);
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
                        // return our reply to the user
                        await context.PostAsync($"You sent {activity.Text}. The available options are: {optionsString}");
                        break;
                }
            }
            
            await context.PostAsync($"You sent {activity.Text}. The available options are:  {optionsString}" );
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