using System.Collections.Generic;
using System.IO;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Extensions.Configuration;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;

namespace Microsoft.BotBuilderSamples
{
    public class AddToDoDialog : ComponentDialog
    {
        private static IConfiguration Configuration;

        public AddToDoDialog(IConfiguration configuration) : base(nameof(AddToDoDialog))
        {
            Configuration = configuration;
            string[] paths = { ".", "Dialogs", "AddToDoDialog", "AddToDoDialog.lg" };
            string fullPath = Path.Combine(paths);
            var AddToDoDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Generator = new TemplateEngineLanguageGenerator(Templates.ParseFile(fullPath)),
                Recognizer = CreateLuisRecognizer(),
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SetProperties()
                            {
                                Assignments = new List<PropertyAssignment>()
                                {
                                    new PropertyAssignment()
                                    {
                                        Property = "dialog.listType",
                                        Value = "=@listType"
                                    },
                                    new PropertyAssignment()
                                    {
                                        Property = "dialog.itemTitle",
                                        Value = "=@itemTitle"
                                    }
                                }
                            },
                              // Get list type
                            new TextInput()
                            {
                                Property = "dialog.listType",
                                Prompt = new ActivityTemplate("${GetListType()}"),
                                Value = "=@listType",
                                AllowInterruptions = "!@listType && turn.recognized.score >= 0.7",
                                Validations = new List<BoolExpression>()
                                {
                                    "contains(createArray('timber', 'tiles', 'paint'), toLower(this.value))",
                                },
                                OutputFormat = "=toLower(this.value)",
                                InvalidPrompt = new ActivityTemplate("${GetListType.Invalid()}"),
                                MaxTurnCount = 50,
                                DefaultValue = "timber",
                                DefaultValueResponse = new ActivityTemplate("${GetListType.DefaultValueResponse()}")
                            },
                            new TextInput()
                            {
                                Property = "dialog.itemTitle",
                                Prompt = new ActivityTemplate("${GetItemTitle()}"),
                                Value = "=@itemTitle",
                                AllowInterruptions = "!@itemTitle && turn.recognized.score >= 0.7"
                            },

                            // Add the new product to the timber category. Keep the cart in the user scope.
                            new EditArray()
                            {
                                ItemsProperty = "user.lists[dialog.listType]",
                                ChangeType = EditArray.ArrayChangeType.Push,
                                Value = "=dialog.itemTitle"
                            },
                            new SendActivity("${AddItemReadBack()}")
                            // All child dialogs will automatically end if there are no additional steps to execute.
                            // If you wish for a child dialog to not end automatically, you can set
                            // AutoEndDialog property on the Adaptive Dialog to 'false'
                        }
                    },
                    // Although root dialog can handle this, this will match locally because this dialog's .lu has local definition for this intent.
                    new OnIntent("Help")
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("${HelpAddItem()}")
                        }
                    },
                    // Shows how to use dialog event to capture intent recognition event for more than one intent.
                    // Alternate to this would be to add two separate OnIntent events.
                    // This ensures we set any entities recognized by these two intents.
                    new OnDialogEvent()
                    {
                        Event = AdaptiveEvents.RecognizedIntent,
                        Condition = "#GetItemTitle || #GetListType",
                        Actions = new List<Dialog>()
                        {
                            new SetProperties()
                            {
                                Assignments = new List<PropertyAssignment>()
                                {
                                    new PropertyAssignment()
                                    {
                                        Property = "dialog.itemTitle",
                                        Value = "=@itemTitle"
                                    },
                                    new PropertyAssignment()
                                    {
                                        Property = "dialog.listType",
                                        Value = "=@listType"
                                    }
                                }
                            }
                        }
                    },
                    new OnIntent("Cart")
                    {
                        Condition = "#Cart.Score >= 0.5",
                        Actions = new List<Dialog>()
                        {
                            new BeginDialog(nameof(ViewToDoDialog))
                        }
                    },
                }
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(AddToDoDialog);

            // The initial child Dialog to run.
            InitialDialogId = nameof(AdaptiveDialog);
        }

        private static Recognizer CreateLuisRecognizer()
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
    }
}