using System.Collections.Generic;
using System.IO;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Extensions.Configuration;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;

namespace Microsoft.BotBuilderSamples
{
    public class ViewToDoDialog : ComponentDialog
    {
        public ViewToDoDialog(IConfiguration configuration)
            : base(nameof(ViewToDoDialog))
        {
            string[] paths = { ".", "Dialogs", "ViewToDoDialog", "ViewToDoDialog.lg" };
            string fullPath = Path.Combine(paths);
            // Create instance of adaptive dialog.
            var ViewToDoDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Generator = new TemplateEngineLanguageGenerator(Templates.ParseFile(fullPath)),
                Recognizer = CreateLuisRecognizer(configuration),
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            // See if any list has any items.
                            new IfCondition()
                            {
                                Condition = "count(user.lists.timber) != 0 || count(user.lists.paint) != 0 || count(user.lists.tiles) != 0",
                                Actions = new List<Dialog>()
                                {
                                    // Get list type
                                    new TextInput()
                                    {
                                        Property = "dialog.listType",
                                        Prompt = new ActivityTemplate("${GetListType()}"),
                                        Value = "=@listType",
                                        AllowInterruptions = "!@listType && turn.recognized.score >= 0.7",
                                        Validations = new List<BoolExpression>()
                                        {
                                            // Verify using expressions that the value is one of timber or tiles or paint
                                            "contains(createArray('timber', 'tiles', 'paint', 'all'), toLower(this.value))",
                                        },
                                        OutputFormat = "=toLower(this.value)",
                                        InvalidPrompt = new ActivityTemplate("${GetListType.Invalid()}"),
                                        MaxTurnCount = 2,
                                        DefaultValue = "timber",
                                        DefaultValueResponse = new ActivityTemplate("${GetListType.DefaultValueResponse()}")
                                    },
                                    new SendActivity("${ShowList()}")
                                },
                                ElseActions = new List<Dialog>()
                                {
                                    new SendActivity("${NoItemsInLists()}")
                                }
                            },
                        }
                    }
                }
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(ViewToDoDialog);

            // The initial child Dialog to run.
            InitialDialogId = nameof(AdaptiveDialog);
        }

        private static Recognizer CreateLuisRecognizer(IConfiguration Configuration)
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
    }
}