using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Evie.Chatbot.Recognizers
{
    public class CustomerRegexRecognizer
    {
        public RegexRecognizer CreateRecognizer()
        {
            return new RegexRecognizer()
            {
                Id = "CustomerRegexRecognizerId",
                Intents = new List<IntentPattern>
                {
                    new IntentPattern("Greeting","(?i)hi there"),
                    new IntentPattern("Greeting","(?i)hello"),
                    new IntentPattern("Greeting","(?i)hey"),
                    new IntentPattern("Greeting","(?i)hi there"),
                    new IntentPattern("Greeting","(?i)hi"),

                    new IntentPattern("BuyProduct","(?i)buy"),
                    new IntentPattern("DeleteItem","(?i)remove"),
                    new IntentPattern("DeleteItem","(?i)delete"),
                    new IntentPattern("DeleteItem","(?i)erase"),
                    new IntentPattern("DeleteItem","(?i)DeleteItem"),
                    new IntentPattern("ViewCart","(?i)view"),
                    new IntentPattern("GetUserProfile","(?i)profile"),
                    new IntentPattern("BookFlight","(?i)book"),
                    new IntentPattern("BookFlight","(?i)book a flight"),
                    new IntentPattern("BookFlight","(?i)flight booking"),
                    new IntentPattern("BookFlight","(?i)catch a flight"),
                    new IntentPattern("BookFlight","(?i)fly to"),
                    new IntentPattern("BookFlight","(?i)fly from"),
                    new IntentPattern("BookFlight","(?i)flight to"),
                    new IntentPattern("BookFlight","(?i)fly from"),
                    new IntentPattern("BookFlight","(?i)flying from"),
                    new IntentPattern("BookFlight","(?i)BookFlight"),
                    new IntentPattern("BookFlight","(?i)book"),
                    new IntentPattern("BookFlight","(?i)travel"),
                    new IntentPattern("BookFlight","(?i)fly"),
                    new IntentPattern("BookFlight","(?i)flight"),

                    new IntentPattern("Help","(?i)help"),
                    new IntentPattern("Help","(?i)query | question | q\\?estion | q\\?esti\\?n"),
                    new IntentPattern("Cancel","(?i)cancel"),
                    new IntentPattern("Exit","(?i)exit"),
                    new IntentPattern("Exit","(?i)bye"),
                    new IntentPattern("Cancel","(?i)nope"),
                    new IntentPattern("Cancel","(?i)no thanks"),
                    new IntentPattern("BuyProduct","(?i)add"),
                    new IntentPattern("BuyProduct","(?i)buy"),
                    new IntentPattern("Cart","(?i)cart"),
                    new IntentPattern("Help","(?i)help"),
                    new IntentPattern("GetWeather","(?i)weather"),
                    new IntentPattern()
                    {
                        Intent = "AddIntent",
                        Pattern = "(?i)(?:add|create) .*(?:to-do|todo|task)(?: )?(?:named (?<title>.*))?"
                    },
                    new IntentPattern()
                    {
                        Intent = "HelpIntent",
                        Pattern = "(?i)help"
                    },
                    new IntentPattern()
                    {
                        Intent = "CancelIntent",
                        Pattern = "(?i)cancel|never mind"
                    }
                },
                Entities = new List<EntityRecognizer>()
                {
                    new ConfirmationEntityRecognizer(),
                    new DateTimeEntityRecognizer(),
                    new NumberEntityRecognizer()
                }
            };
        }
    }
}