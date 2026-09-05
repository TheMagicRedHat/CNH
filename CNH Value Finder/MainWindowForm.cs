namespace CNH;

public partial class MainWindowForm : Form
{

    class Field : FlowLayoutPanel
    {
        public Label label;
        public TextBox textBox;

        public Field(string labelText) : base()
        {
            FlowDirection = FlowDirection.TopDown;
            AutoSize = true;

            textBox = new TextBox();
            textBox.AutoSize = true;
            textBox.Anchor = AnchorStyles.Top;

            label = new Label();
            label.Text = labelText;
            label.AutoSize = true;
            label.Anchor = AnchorStyles.Bottom;

            Controls.Add(textBox);
            Controls.Add(label);
        }
    }

    public MainWindowForm()
    {
        // Text at top that explains usage
        Label usageText = new Label()
        {
            Text = "Welcome to the CNH Value Finder! This app lets you find a variety of useful info for CNH GM game design. Fill in the high-level info and then select one of the tabs to get started.",
            Dock = DockStyle.Top,
            AutoSize = true,
        };

        // Fields for common variables (type dice, num dice, etc)
        FlowLayoutPanel commonVariables = new FlowLayoutPanel()
        {
            FlowDirection = FlowDirection.LeftToRight,
            Text = "High level dice info",
            Dock = DockStyle.Top,
            AutoSize = true,
        };

        Field typeDiceField = new Field("Type of Dice (D6, D20, etc)")
        {
            
        };

        Field numDiceField = new Field("Number of Dice (1D6, 2D6, etc)")
        {
            
        };

        Field lowestSuccessValueField = new Field("Lowest Success Value (default of '4' for D6)")
        {
            
        };

        GroupBox advantageStateGroupBox = new GroupBox()
        {
            Text = "Advantage State",
            Dock = DockStyle.None,
            AutoSize = true,
        };

        RadioButton advantageRadioButton = new RadioButton()
        {
            Text = "Advantage",
            Anchor = AnchorStyles.Top,
            AutoSize = true,
        };

        RadioButton neutralRadioButton = new RadioButton()
        {
            Text = "Neutral",
            Anchor = AnchorStyles.None,
            AutoSize = true,
        };

        RadioButton disadvantageRadioButton = new RadioButton()
        {
            Text = "Disadvantage",
            Anchor = AnchorStyles.Bottom,
            AutoSize = true,
        };

        // Run button at the bottom
        Button runButton = new Button()
        {
            Text = "Run",
            Dock = DockStyle.Bottom,
            AutoSize = true,
        };

        // Tabs for different features
        TabControl tabs = new TabControl()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
        };

        // The different tab pages
        TabPage diceRollerTabPage = new TabPage()
        {
            Text = "Dice Roller",
            AutoSize = true,
            TabIndex = 0,
        };

        TabPage probabilityTablesTabPage = new TabPage()
        {
            Text = "Probability Tables",
            AutoSize = true,
            TabIndex = 1,
        };

        TabPage statFinderTabPage = new TabPage()
        {
            Text = "Stat Finder",
            AutoSize = true,
            TabIndex = 2,
        };

        // Features for the Dice Roller tab page

        // Features for the Probability Tables tab page

        // Features for the Stat Finder tab page

        // Add all of the controls and then initialize
        tabs.Controls.Add(diceRollerTabPage);
        tabs.Controls.Add(probabilityTablesTabPage);
        tabs.Controls.Add(statFinderTabPage);

        advantageStateGroupBox.Controls.Add(disadvantageRadioButton);
        advantageStateGroupBox.Controls.Add(neutralRadioButton);
        advantageStateGroupBox.Controls.Add(advantageRadioButton);
        neutralRadioButton.Checked = true;

        commonVariables.Controls.Add(typeDiceField);
        commonVariables.Controls.Add(numDiceField);
        commonVariables.Controls.Add(lowestSuccessValueField);
        commonVariables.Controls.Add(advantageStateGroupBox);

        Controls.Add(tabs);
        Controls.Add(commonVariables);
        Controls.Add(usageText);
        Controls.Add(runButton);
        AcceptButton = runButton;

        InitializeComponent();
    }
}
