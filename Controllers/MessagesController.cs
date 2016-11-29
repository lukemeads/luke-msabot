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

                // State service 
                StateClient stateClient = activity.GetStateClient();
                BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);

                Activity reply;

                string name = userData.GetProperty<string>("UserName");

                if (name == null)
                {
                    userData.SetProperty<string>("UserName", "sir");
                }

                RateLUIS LUIS = await GetEntityFromLUIS(activity.Text);
                if (LUIS.intents.Count() > 0)
                {
                    switch (LUIS.intents[0].intent)
                    {
                        case "Greeting":
                            if (!userData.GetProperty<bool>("SentGreeting"))
                            {
                                //reply = activity.CreateReply($"G'day, {name}.");
                                await connector.Conversations.SendToConversationAsync(await getLogo(activity));
                                return Request.CreateResponse(HttpStatusCode.OK);
                                userData.SetProperty<bool>("SentGreeting", true);
                                await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                            } else
                            {
                                reply = activity.CreateReply($"You've already greeted me, {name}.");
                            }
                            break;
                        case "GetExchangeRate":
                            reply = activity.CreateReply(await GetRate(LUIS.entities[0].entity.ToUpper()));
                            break;
                        case "GetBalance":
                            string requestUser = "";
                            if (LUIS.entities.Count() == 0 && name != "sir")
                            {
                                requestUser = name;
                            } else if (LUIS.entities.Count == 0) {
                                reply = activity.CreateReply($"I don't know who you are, sir.");
                                break;
                            } else
                            {
                                requestUser = LUIS.entities[0].entity;
                            }
                            Users user = new Users();
                            var usersList = await AzureManager.AzureManagerInstance.GetUser(requestUser);
                            foreach (Users u in usersList)
                            {
                                user.Name = u.Name;
                                user.Balance = u.Balance;
                                user.Currency = u.Currency;
                            }

                            reply = activity.CreateReply($"The current balance for {user.Name} is {user.Balance} {user.Currency}.");
                            break;
                        case "CreateAccount":
                            Users newAccount = new Users()
                            {
                                Name = LUIS.entities[2].entity,
                                Balance = Convert.ToDouble(LUIS.entities[0].entity),
                                Currency = LUIS.entities[1].entity
                            };
                            await AzureManager.AzureManagerInstance.AddUser(newAccount);
                            reply = activity.CreateReply($"I have created {newAccount.Name} an account, {name}.");
                            break;
                        case "WithdrawCash":
                            if (name == "sir")
                             {
                                 reply = activity.CreateReply($"I don't know who you are.");
                                 break;
                             }
                            var userNames = await AzureManager.AzureManagerInstance.GetUser(name);
                            foreach (Users u in userNames)
                            {
                                u.Name = name;
                                u.Balance = u.Balance - Convert.ToDouble(LUIS.entities[0].entity);
                                u.Currency = u.Currency;
                                await AzureManager.AzureManagerInstance.UpdateUser(u);
                            }
                            reply = activity.CreateReply($"Okay. You have successfully withdrawn {LUIS.entities[0].entity} cash from your account, {name}.");
                            break;
                        case "DepositCash":
                            if (name == "sir")
                            {
                                reply = activity.CreateReply($"I don't know who you are.");
                                break;
                            }
                            var userNames2 = await AzureManager.AzureManagerInstance.GetUser(name);
                            foreach (Users u in userNames2)
                            {
                                u.Name = name;
                                u.Balance = u.Balance + Convert.ToDouble(LUIS.entities[0].entity);
                                u.Currency = u.Currency;
                                await AzureManager.AzureManagerInstance.UpdateUser(u);
                            }
                            reply = activity.CreateReply($"Okay. You have successfully deposited {LUIS.entities[0].entity} cash into your account, {name}.");
                            break;
                        case "DeclareSpeaker":
                            userData.SetProperty<string>("UserName", LUIS.entities[0].entity);
                            await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                            reply = activity.CreateReply($"Okay, I'll call you {name}.");
                            break;
                        default:
                            reply = activity.CreateReply($"Hmm...I'm not getting you, {name}.");
                            break;
                   } 
                }
                else
                {
                    reply = activity.CreateReply($"Hmm...I'm not getting you, {name}.");
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

        private static async Task<Activity> getLogo(Activity activity)
        {
            Activity replyToConversation = activity.CreateReply("G'day sir, welcome to Contonzo Bank.");
            replyToConversation.Recipient = activity.From;
            replyToConversation.Type = "message";
            replyToConversation.Attachments = new List<Attachment>();
            List<CardImage> cardImages = new List<CardImage>();
            cardImages.Add(new CardImage(url: "https://lh5.googleusercontent.com/ZDsZ-VQigco2FFkrl0xGsZIgyknFQeE0aarAiBMQAwj5ZCCL4tC3xcfP6DYO_jaHmlHtm9tH3es-7i4=w1920-h950"));
            List<CardAction> cardButtons = new List<CardAction>();
            /*
            CardAction plButton = new CardAction()
            {
                Value = "http://msa.ms",
                Type = "openUrl",
                Title = "MSA Website"
            };
            cardButtons.Add(plButton);*/
            ThumbnailCard plCard = new ThumbnailCard()
            {
                Title = "Best money, best bots",
                Subtitle = "Bank with us today",
                Images = cardImages,
                Buttons = cardButtons
            };
            Attachment plAttachment = plCard.ToAttachment();
            replyToConversation.Attachments.Add(plAttachment);
            return replyToConversation;
        }

    }
}