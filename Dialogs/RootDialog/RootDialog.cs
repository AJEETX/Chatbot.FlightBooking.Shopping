using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Extensions.Configuration;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using System.IO;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;

namespace Evie.Chatbot.Dialogs
{
    public class RootDialog : ComponentDialog
    {
        private static IConfiguration Configuration;

        public RootDialog(IConfiguration configuration) : base(nameof(RootDialog))
        {
            Configuration = configuration;
            string[] paths = { ".", "Dialogs", "RootDialog", "RootDialog.lg" };
            string fullPath = Path.Combine(paths);
            // Create instance of adaptive dialog.
            var rootDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                // Add a generator. This is how all Language Generation constructs specified for this dialog are resolved.
                Generator = new TemplateEngineLanguageGenerator(Templates.ParseFile(fullPath)),
                // Create a LUIS recognizer.
                // The recognizer is built using the intents, utterances, patterns and entities defined in ./RootDialog.lu file
                Recognizer = CreateRecognizer(),
                Triggers = new List<OnCondition>()
                {
                    // Add a rule to welcome user
                    new OnConversationUpdateActivity()
                    {
                        Actions = WelcomeUserSteps()
                    },
                    // Intent rules for the LUIS model. Each intent here corresponds to an intent defined in ./RootDialog.lu file
                    new OnIntent("Greeting")
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("${HelpRootDialog()}")
                            }
                    },
                    new OnIntent("BuyProduct")
                    {
                        // LUIS returns a confidence score with intent classification.
                        // Conditions are expressions.
                        // This expression ensures that this trigger only fires if the confidence score for the
                        // AddToDoDialog intent classification is at least 0.7
                        Condition = "#BuyProduct.Score >= 0.5",
                        Actions = new List<Dialog>()
                        {
                            new BeginDialog(nameof(AddToDoDialog))
                        }
                    },
                    new OnIntent("GetWeather")
                    {
                        Intent = "GetWeather",
                        Actions = new List<Dialog> ()
                        {
                            new SetProperties()
                            {
                                Assignments = new List<PropertyAssignment>()
                                {
                                    new PropertyAssignment()
                                    {
                                        Property = "conversation.weather",
                                        Value = "={}"
                                    },

                                    // We will only save any geography city entities that explicitly have been classified as either fromCity or toCity.
                                    new PropertyAssignment()
                                    {
                                        Property = "conversation.weather.city",
                                        // Value is an expression. @entityName is shorthand to refer to the value of an entity recognized.
                                        // @xxx is same as turn.recognized.entities.xxx
                                        Value = "=@toCity.location"
                                    }
                                }
                            },
                            new TextInput()
                            {
                                Property = "conversation.weather.city",
                                Prompt = new ActivityTemplate("${PromptForCityWeatherResponse()}"),
                                AllowInterruptions = "!@toCity  || !@geographyV2",
                                // Value is an expression. Take any recognized city name as fromCity
                                Value = "=@toCity.location"
                            },
                            new ConfirmInput()
                            {
                                Property = "turn.weatherConfirmation",
                                Prompt = new ActivityTemplate("${ConfirmWeather()}"),
                                // You can use this flag to control when a specific input participates in consultation bubbling and can be interrupted.
                                // 'false' means interruption is not allowed when this input is active.
                                AllowInterruptions = "false"
                            },
                            new IfCondition()
                            {
                                // All conditions are expressed using adaptive expressions.
                                // See https://aka.ms/adaptive-expressions to learn more
                                Condition = "turn.weatherConfirmation == true",
                                Actions = new List<Dialog>()
                                {
                                    // TODO: book flight.
                                    new SendActivity("${CityWeatherResponse()}")
                                },
                                ElseActions = new List<Dialog>()
                                {
                                    new SendActivity("Can't provide weather. Thank you.")
                                }
                            }
                        }
                    },
                    new OnIntent("BookFlight")
                    {
                        Condition = "#BookFlight.Score >= 0.5",
                        Actions = new List<Dialog>()
                        {
                            new BeginDialog(nameof(BookFlightDialog))
                        }
                    },
                    new OnIntent("DeleteItem")
                    {
                        Condition = "#DeleteItem.Score >= 0.5",
                        Actions = new List<Dialog>()
                        {
                            new BeginDialog(nameof(DeleteToDoDialog))
                        }
                    },
                    new OnIntent("GetUserProfile")
                    {
                        Condition = "#GetUserProfile.Score >= 0.5",
                        Actions = new List<Dialog>()
                        {
                             new BeginDialog(nameof(GetUserProfileDialog))
                        }
                    },
                    // Come back with LG template based readback for global help
                    new OnIntent("Help")
                    {
                        Condition = "#Help.Score >= 0.8",
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("${HelpRootDialog()}")
                        }
                    },
                    new OnIntent("Cancel")
                    {
                        Condition = "#Cancel.Score >= 0.8",
                        Actions = new List<Dialog>()
                        {
                            // Ask user for confirmation.
                            // This input will still use the recognizer and specifically the confirm list entity extraction.
                            new ConfirmInput()
                            {
                                Prompt = new ActivityTemplate("${Cancel.prompt()}"),
                                Property = "turn.confirm",
                                Value = "=@confirmation",
                                // Allow user to intrrupt this only if we did not get a value for confirmation.
                                AllowInterruptions = "!@confirmation"
                            },
                            new IfCondition()
                            {
                                Condition = "turn.confirm == true",
                                Actions = new List<Dialog>()
                                {
                                    // This is the global cancel in case a child dialog did not explicit handle cancel.
                                    new SendActivity("Cancelling all dialogs.."),
                                    new SendActivity("${SigninCard()}"),
                                    new CancelAllDialogs(),
                                },
                                ElseActions = new List<Dialog>()
                                {
                                    new SendActivity("${CancelCancelled()}"),
                                    new SendActivity("${SigninCard()}")
                                }
                            }
                        }
                    }
                }
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(rootDialog);

            // Add all child dialogS
            AddDialog(new AddToDoDialog(configuration));
            AddDialog(new DeleteToDoDialog(configuration));
            AddDialog(new ViewToDoDialog(configuration));
            AddDialog(new GetUserProfileDialog(configuration));
            AddDialog(new BookFlightDialog(configuration));
            // The initial child Dialog to run.
            InitialDialogId = nameof(AdaptiveDialog);
        }

        private static List<Dialog> WelcomeUserSteps()
        {
            return new List<Dialog>()
            {
                // Iterate through membersAdded list and greet user added to the conversation.
                new Foreach()
                {
                    ItemsProperty = "turn.activity.membersAdded",
                    Actions = new List<Dialog>()
                    {
                        // Note: Some channels send two conversation update events - one for the Bot added to the conversation and another for user.
                        // Filter cases where the bot itself is the recipient of the message.
                        new IfCondition()
                        {
                            Condition = "$foreach.value.name != turn.activity.recipient.name",
                            Actions = new List<Dialog>()
                            {
                                new SendActivity("${AdaptiveCard()}"),
                                new SendActivity("${IntroMessage()}"),
                                // Initialize global properties for the user.
                                new SetProperty()
                                {
                                    Property = "user.lists",
                                    Value = "={timber : [], paint : [], tiles : []}"
                                }
                            }
                        }
                    }
                }
            };
        }

        public static Recognizer CreateRecognizer()
        {
            return new RegexRecognizer
            {
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
                    new IntentPattern("BookFlight","(?i)catch  a flight"),
                    new IntentPattern("BookFlight","(?i)fly to"),
                    new IntentPattern("BookFlight","(?i)fly from"),
                    new IntentPattern("BookFlight","(?i)flight to"),
                    new IntentPattern("BookFlight","(?i)fly from"),
                    new IntentPattern("BookFlight","(?i)flying from"),

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
    }
}