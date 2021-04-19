using System.Collections.Generic;
using System.IO;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Extensions.Configuration;

namespace Evie.Chatbot.Dialogs
{
    public class BookFlightDialog : ComponentDialog
    {
        private static IConfiguration Configuration;

        public BookFlightDialog(IConfiguration configuration) : base(nameof(BookFlightDialog))
        {
            Configuration = configuration;
            string[] paths = { ".", "Dialogs", "BookFlightDialog", "BookFlightDialog.lg" };
            string fullPath = Path.Combine(paths);
            var bookFlightDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Generator = new TemplateEngineLanguageGenerator(Templates.ParseFile(fullPath)),
                // Create and use a LUIS recognizer on the child
                // Each child adaptive dialog can have its own recognizer.
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            //welcome message to the booking
                            new SendActivity("${BookingWelcome()}"),
                            // Save any entities returned by LUIS.
                           new BeginDialog(nameof(GetUserProfileDialog)),
                            new SetProperties()
                            {
                                Assignments = new List<PropertyAssignment>()
                                {
                                    new PropertyAssignment()
                                    {
                                        Property = "conversation.flightBooking",
                                        Value = "={}"
                                    },

                                    // We will only save any geography city entities that explicitly have been classified as either fromCity or toCity.
                                    new PropertyAssignment()
                                    {
                                        Property = "conversation.flightBooking.departureCity",
                                        // Value is an expression. @entityName is shorthand to refer to the value of an entity recognized.
                                        // @xxx is same as turn.recognized.entities.xxx
                                        Value = "=@fromCity.location"
                                    },
                                    new PropertyAssignment()
                                    {
                                        Property = "conversation.flightBooking.destinationCity",
                                        Value = "=@toCity.location"
                                    },
                                    new PropertyAssignment()
                                    {
                                        Property = "conversation.flightBooking.departureDate",
                                        Value = "=@datetime.timex[0]"
                                    }
                                }
                            },
                            // Steps to book flight
                            // Help and Cancel intents are always available since TextInput will always initiate
                            // Consultation up the parent dialog chain to see if anyone else wants to take the user input.
                            new TextInput()
                            {
                                Property = "conversation.flightBooking.departureCity",
                                // Prompt property supports full language generation resolution.
                                // See here to learn more about language generation
                                // https://aka.ms/language-generation
                                Prompt = new ActivityTemplate("${PromptForMissingInformation()}"),
                                // We will allow interruptions as long as the user did not explicitly answer the question
                                // This property supports an expression so you can examine presence of an intent via #intentName,
                                //    detect presence of an entity via @entityName etc. Interruption is allowed if the expression
                                //    evaluates to `true`. This property defaults to `true`.
                                AllowInterruptions = "!@fromCity || !@geographyV2",
                                // Value is an expression. Take first non null value.
                                Value = "=coalesce(@fromCity.location, @geographyV2.location)"
                            },
                            // delete entity so it is not over consumed as destination as well
                            new DeleteProperty()
                            {
                                Property = "turn.recognized.entities.geographyV2"
                            },
                            new TextInput()
                            {
                                Property = "conversation.flightBooking.destinationCity",
                                Prompt = new ActivityTemplate("${PromptForMissingInformation()}"),
                                AllowInterruptions = "!@toCity || !@geographyV2 || @toCity == @fromCity",
                                // Value is an expression. Take any recognized city name as fromCity
                                Value = "=coalesce(@toCity.location, @geographyV2.location)"
                            },
                            new DateTimeInput()
                            {
                                Property = "conversation.flightBooking.departureDate",
                                Prompt = new ActivityTemplate("${PromptForMissingInformation()}"),
                                AllowInterruptions = "!@datetime",
                                Validations = new List<BoolExpression>()
                                {
                                    // Prebuilt function that returns boolean true if we have a full, valid date.
                                    "isDefinite(this.value[0].timex)"
                                },
                                InvalidPrompt = new ActivityTemplate("${InvalidDateReprompt()}"),
                                // Value is an expression. Take any date time entity recognition as deparature date.
                                Value = "=@datetime.timex[0]"
                            },
                            new ConfirmInput()
                            {
                                Property = "turn.bookingConfirmation",
                                Prompt = new ActivityTemplate("${ConfirmBooking()}"),
                                // You can use this flag to control when a specific input participates in consultation bubbling and can be interrupted.
                                // 'false' means interruption is not allowed when this input is active.
                                AllowInterruptions = "false"
                            },
                            new IfCondition()
                            {
                                // All conditions are expressed using adaptive expressions.
                                // See https://aka.ms/adaptive-expressions to learn more
                                Condition = "turn.bookingConfirmation == true",
                                Actions = new List<Dialog>()
                                {
                                    // TODO: book flight.
                                    new SendActivity("${BookingConfirmation()}"),
                                    new SendActivity("${BookingReceiptCard()}"),
                                    new SendActivity("${BotOverviewRestart()}")
                                },
                                ElseActions = new List<Dialog>()
                                {
                                    new SendActivity("booking Cancelled. Thank you."),
                                    new SendActivity("${BotOverviewRestart()}")
                                }
                            }
                        }
                    }
                }
            };
            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(bookFlightDialog);

            // The initial child Dialog to run.
            InitialDialogId = nameof(AdaptiveDialog);
        }
    }
}