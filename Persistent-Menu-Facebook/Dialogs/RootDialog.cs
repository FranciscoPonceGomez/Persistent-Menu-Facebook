using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Net;
using System.Dynamic;

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

            if (activity.Text.Equals("ActivateMenu"))
            {
                String PAGE_ACCESS_TOKEN = "EAAJvLGRGsFwBAJbFtgq7w6e3YlXVXZBZA7uGZBZAJ7hD7T06lLTOapTzJOPZAAwZALiPMu9pIbDbiP4GXysHOvhilLlFIm1HDXZCrAqfBP3cmM2GzVhatt3Px9qYiRAHYLZBdaygoADXKhSFyFKPIlXvsZBoJNZAVSArcd6kwQCqTnfwZDZD"; //your facebook app token

                //Build JSON required - Or use a string , copy+paste
                dynamic Data = new ExpandoObject();
                Data.get_started = new ExpandoObject();
                Data.get_started.payload = "GET_STARTED_PAYLOAD";

                Console.WriteLine("About to send : " + Newtonsoft.Json.JsonConvert.SerializeObject(Data));

                using (WebClient client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    var _result = client.UploadString(
                        "https://graph.facebook.com/v2.6/me/messenger_profile?access_token=" + PAGE_ACCESS_TOKEN,
                        Newtonsoft.Json.JsonConvert.SerializeObject(Data)
                    );
                    //Console.WriteLine("Result : " + _result);
                    await context.PostAsync($"Result: {_result}");
                }
            }
            else
            {
                int length = (activity.Text ?? string.Empty).Length;

                // return our reply to the user
                await context.PostAsync($"You sent {activity.Text} which was {length} characters");
            }
            context.Wait(MessageReceivedAsync);
        }
    }
}