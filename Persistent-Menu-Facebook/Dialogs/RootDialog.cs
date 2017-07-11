using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Net;
using System.Dynamic;
using System.IO;
using System.Web.Configuration;

namespace Persistent_Menu_Facebook.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {

            var activity = await result as Activity;
            string PAGE_ACCESS_TOKEN = WebConfigurationManager.AppSettings["FacebookAccessToken"];
            //String PAGE_ACCESS_TOKEN = "EAAJvLGRGsFwBAJbFtgq7w6e3YlXVXZBZA7uGZBZAJ7hD7T06lLTOapTzJOPZAAwZALiPMu9pIbDbiP4GXysHOvhilLlFIm1HDXZCrAqfBP3cmM2GzVhatt3Px9qYiRAHYLZBdaygoADXKhSFyFKPIlXvsZBoJNZAVSArcd6kwQCqTnfwZDZD"; //your facebook app token
            HttpWebRequest request;
            string option = activity.Text;
            Object _result = null;
            string Data = string.Empty;
            switch (option)
            {
                case "ActivateMenu":

                    //Build JSON required - Or use a string , copy+paste
                    //dynamic Data = new ExpandoObject();
                    //Data.get_started = new ExpandoObject();
                    //Data.get_started.payload = "GET_STARTED_PAYLOAD";

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

                    using (WebClient client = new WebClient())
                    {
                        client.Headers[HttpRequestHeader.ContentType] = "application/json";
                        _result = client.UploadString(
                            "https://graph.facebook.com/v2.6/me/messenger_profile?access_token=" + PAGE_ACCESS_TOKEN,
                            Newtonsoft.Json.JsonConvert.SerializeObject(Data)
                        );
                        await context.PostAsync($"{_result}");
                    }
                    break;

                case "ShowMenu":
                    request = (HttpWebRequest)HttpWebRequest.Create("https://graph.facebook.com/v2.6/me/messenger_profile?fields=persistent_menu&access_token=" + PAGE_ACCESS_TOKEN);
                    request.Method = "GET";
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

                case "DeleteMenu":
                    request = (HttpWebRequest)HttpWebRequest.Create("https://graph.facebook.com/v2.6/me/messenger_profile?access_token=" + PAGE_ACCESS_TOKEN);
                    request.Method = "DELETE";
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

                default:
                    int length = (activity.Text ?? string.Empty).Length;

                    // return our reply to the user
                    await context.PostAsync($"You sent {activity.Text}. The available options are: \n\t1: ActivateMenu \n\t2: ShowMenu \n\t3: DeleteMenu \n ");
                    break;
            }

        context.Wait(MessageReceivedAsync);
        }
    }
}