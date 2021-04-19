<<<<<<< HEAD
﻿using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using System.Collections.Generic;

namespace Evie.Chatbot.Recognizers
{
    public class CustomRegexRecognizer
    {
        public static Recognizer CreateRootRecognizer()
        {
            return new RegexRecognizer
            {
=======
﻿using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
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
>>>>>>> Integrated Lui, with regex
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
<<<<<<< HEAD
                    new IntentPattern("BookFlight","(?i)catch  a flight"),
=======
                    new IntentPattern("BookFlight","(?i)catch a flight"),
>>>>>>> Integrated Lui, with regex
                    new IntentPattern("BookFlight","(?i)fly to"),
                    new IntentPattern("BookFlight","(?i)fly from"),
                    new IntentPattern("BookFlight","(?i)flight to"),
                    new IntentPattern("BookFlight","(?i)fly from"),
                    new IntentPattern("BookFlight","(?i)flying from"),
<<<<<<< HEAD

                    new IntentPattern("Help","(?i)help"),
                    new IntentPattern("Help","(?i)query | question | q\\?estion | q\\?esti\\?n"),
                    new IntentPattern("Cancel","(?i)cancel"),
                    new IntentPattern("Exit","(?i)exit"),
                    new IntentPattern("Exit","(?i)bye"),
                    new IntentPattern("Cancel","(?i)nope"),
                    new IntentPattern("Cancel","(?i)no thanks"),
                    new IntentPattern("BuyProduct","(?i)add"),
                    new IntentPattern("BuyProduct","(?i)buy"),
                    new IntentPattern("GetWeather","(?i)weather")
                }
            };
        }

        public static Recognizer CreateAddToDoDialogRecognizer()
        {
            return new RegexRecognizer
            {
                Intents = new List<IntentPattern>
                {
                    new IntentPattern("Cart","(?i)cart"),
                    new IntentPattern("Help","(?i)help")
                }
            };
        }

        public static Recognizer CreateBookingRecognizer()
        {
            return new RegexRecognizer
            {
                Intents = new List<IntentPattern>
                {
=======
>>>>>>> Integrated Lui, with regex
                    new IntentPattern("BookFlight","(?i)BookFlight"),
                    new IntentPattern("BookFlight","(?i)book"),
                    new IntentPattern("BookFlight","(?i)travel"),
                    new IntentPattern("BookFlight","(?i)fly"),
<<<<<<< HEAD
                    new IntentPattern("BookFlight","(?i)flight")
                }
            };
        }

        public static Recognizer CreateDeleteRecognizer()
        {
            return new RegexRecognizer
            {
                Intents = new List<IntentPattern>
                {
                    new IntentPattern("BookFlight","(?i)book"),
                    new IntentPattern("BookFlight","(?i)travel"),
                    new IntentPattern("BookFlight","(?i)fly"),
                    new IntentPattern("BookFlight","(?i)flight"),
                    new IntentPattern("Greeting","(?i)hi"),
                    new IntentPattern("Greeting","(?i)hi there"),
                    new IntentPattern("Greeting","(?i)hello"),
                    new IntentPattern("Greeting","(?i)hey"),
                    new IntentPattern("Greeting","(?i)hi there"),
                    new IntentPattern("Help","(?i)help"),
                    new IntentPattern("Help","(?i)query"),
                    new IntentPattern("Cancel","(?i)cancel"),
                    new IntentPattern("Exit","(?i)exit"),
                    new IntentPattern("Exit","(?i)bye"),
                    new IntentPattern("Cancel","(?i)no"),
                    new IntentPattern("Cancel","(?i)nope"),
                    new IntentPattern("Cancel","(?i)no thanks"),
                    new IntentPattern("BuyProduct","(?i)add"),
                    new IntentPattern("GetWeather","(?i)weather")
                }
            };
        }

        public static Recognizer CreateProfileRecognizer()
        {
            return new RegexRecognizer
            {
                Intents = new List<IntentPattern>
                {
                    new IntentPattern("BookFlight","(?i)book"),
                    new IntentPattern("BookFlight","(?i)travel"),
                    new IntentPattern("BookFlight","(?i)fly"),
                    new IntentPattern("BookFlight","(?i)flight"),
                    new IntentPattern("Greeting","(?i)hi"),
                    new IntentPattern("Greeting","(?i)hi there"),
                    new IntentPattern("Greeting","(?i)hello"),
                    new IntentPattern("Greeting","(?i)hey"),
                    new IntentPattern("Greeting","(?i)hi there"),
                    new IntentPattern("Help","(?i)help"),
                    new IntentPattern("Help","(?i)query"),
                    new IntentPattern("Cancel","(?i)cancel"),
                    new IntentPattern("Exit","(?i)exit"),
                    new IntentPattern("Exit","(?i)bye"),
                    new IntentPattern("Cancel","(?i)no"),
                    new IntentPattern("Cancel","(?i)nope"),
                    new IntentPattern("Cancel","(?i)no thanks"),
                    new IntentPattern("BuyProduct","(?i)add"),
                    new IntentPattern("GetWeather","(?i)weather")
                }
            };
        }

        public static Recognizer CreateViewRecognizer()
        {
            return new RegexRecognizer
            {
                Intents = new List<IntentPattern>
                {
                    new IntentPattern("BookFlight","(?i)book"),
                    new IntentPattern("BookFlight","(?i)travel"),
                    new IntentPattern("BookFlight","(?i)fly"),
                    new IntentPattern("BookFlight","(?i)flight"),
                    new IntentPattern("Greeting","(?i)hi"),
                    new IntentPattern("Greeting","(?i)hi there"),
                    new IntentPattern("Greeting","(?i)hello"),
                    new IntentPattern("Greeting","(?i)hey"),
                    new IntentPattern("Greeting","(?i)hi there"),
                    new IntentPattern("Help","(?i)help"),
                    new IntentPattern("Help","(?i)query"),
                    new IntentPattern("Cancel","(?i)cancel"),
                    new IntentPattern("Exit","(?i)exit"),
                    new IntentPattern("Exit","(?i)bye"),
                    new IntentPattern("Cancel","(?i)no"),
                    new IntentPattern("Cancel","(?i)nope"),
                    new IntentPattern("Cancel","(?i)no thanks"),
                    new IntentPattern("BuyProduct","(?i)add"),
                    new IntentPattern("GetWeather","(?i)weather")
=======
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
>>>>>>> Integrated Lui, with regex
                }
            };
        }
    }
}