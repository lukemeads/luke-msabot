using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Bot_Application1.Controllers;
using Bot_Application1.Models;
using System.Collections.Generic;

namespace Bot_Application1
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                // calculate something for us to return
                //int length = (activity.Text ?? string.Empty).Length;
                Activity reply;

                RateLUIS LUIS = await GetEntityFromLUIS(activity.Text);
                if (LUIS.intents.Count() > 0)
                {
                    switch (LUIS.intents[0].intent)
                    {
                        case "Greeting":
                            reply = activity.CreateReply($"G'day, good sir.");
                            break;
                        case "GetExchangeRate":
                            reply = activity.CreateReply(await GetRate(LUIS.entities[0].entity.ToUpper()));
                            break;
                        case "GetBalance":
                            /*List<User> users = await AzureManager.AzureManagerInstance.GetUser(LUIS.entities[0].entity);
                            string output = "";
                            foreach(User u in users)
                            {
                                output = "The current balance for " + u.Name + " is " + u.Balance + " " + u.Currency +".";
                            }*/
                            User user = new User();
                            user.Balance = 999;
                            user.Name = "Shama";
                            user.Currency = "AUD";
                            await AzureManager.AzureManagerInstance.AddUser(user);
                            reply = activity.CreateReply($"Done");
                            break;
                        default:
                            reply = activity.CreateReply($"Hmm...I'm not getting you, sir.");
                            break;
                    }
                }
                else
                {
                    reply = activity.CreateReply($"Hmm...I'm not getting you, sir.");
                }

                await connector.Conversations.ReplyToActivityAsync(reply);


                // return our reply to the user
                //Activity reply = activity.CreateReply($"You sent {activity.Text} which was {length} characters");
                //await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }

        private async Task<string> GetRate(string RateSymbol)
        {
            double? dblStockValue = await RatesBot.GetExchangeRateAsync(RateSymbol);
            if (dblStockValue == -1)
            {
                return string.Format("This \"{0}\" is not an valid rate symbol.", RateSymbol);
            }
            else
            {
                return string.Format("The current exchange rate for {0} is {1}, sir.", RateSymbol, dblStockValue);
            }
        }

        private static async Task<RateLUIS> GetEntityFromLUIS(string Query)
        {
            Query = Uri.EscapeDataString(Query);
            RateLUIS Data = new RateLUIS();
            using (HttpClient client = new HttpClient())
            {
                string RequestURI = "https://api.projectoxford.ai/luis/v2.0/apps/c8701615-f586-48db-868d-ed62613a97b0?subscription-key=12c93f29f8074992a09cade3d9a53a41&q=" + Query + "&verbose=true";
                HttpResponseMessage msg = await client.GetAsync(RequestURI);

                if (msg.IsSuccessStatusCode)
                {
                    var JsonDataResponse = await msg.Content.ReadAsStringAsync();
                    Data = JsonConvert.DeserializeObject<RateLUIS>(JsonDataResponse);
                }
            }
            return Data;
        }

    }
}