namespace CNH;

public partial class MainWindowForm : Form
{
    public MainWindowForm()
    {
        // Text at top that explains usage
        Label usageText = new Label()
        {
            Text = "Welcome to the CNH Value Finder! This app lets you find a variety of useful info for CNH GM game design. Select one of the tabs below to get started.",
            Dock = DockStyle.Top,
            Size = this.PreferredSize,
        };

        // Run button at the bottom
        Button runButton = new Button()
        {
            Text = "Run",
            Dock = DockStyle.Bottom,
            Size = this.PreferredSize,
        };

        // Tabs for different features
        TabControl tabs = new TabControl()
        {
            Dock = DockStyle.Fill,
            Size = this.PreferredSize,
        };

        // The different tab pages
        TabPage diceRollerTabPage = new TabPage()
        {
            Text = "Dice Roller",
            Size = this.PreferredSize,
            TabIndex = 0,
        };

        TabPage probabilityTablesTabPage = new TabPage()
        {
            Text = "Probability Tables",
            Size = this.PreferredSize,
            TabIndex = 1,
        };

        TabPage statFinderTabPage = new TabPage()
        {
            Text = "Stat Finder",
            Size = this.PreferredSize,
            TabIndex = 2,
        };

        // Features for the Dice Roller tab page
        // Next X are for fillable text options (type dice, number dice, what is a success)
        // HERE
        // Next 4 are for Advantage State
        GroupBox diceRollerAdvantageStateGroupBox = new GroupBox()
        {
            Text = "Advantage State",
            Dock = DockStyle.Fill,
            Size = this.PreferredSize,
        };

        RadioButton diceRollerAdvantageRadioButton = new RadioButton()
        {
            Text = "Advantage",
            Dock = DockStyle.Top,
            Size = this.PreferredSize,
        };

        RadioButton diceRollerNeutralRadioButton = new RadioButton()
        {
            Text = "Neutral",
            Dock = DockStyle.Top,
            Size = this.PreferredSize,
        };

        RadioButton diceRollerDisadvantageRadioButton = new RadioButton()
        {
            Text = "Disadvantage",
            Dock = DockStyle.Top,
            Size = this.PreferredSize,
        };

        // Features for the Probability Tables tab page

        // Features for the Stat Finder tab page

        // Add all of the controls and then initialize
        diceRollerAdvantageStateGroupBox.Controls.Add(diceRollerDisadvantageRadioButton);
        diceRollerAdvantageStateGroupBox.Controls.Add(diceRollerNeutralRadioButton);
        diceRollerAdvantageStateGroupBox.Controls.Add(diceRollerAdvantageRadioButton);
        diceRollerNeutralRadioButton.Checked = true;

        diceRollerTabPage.Controls.Add(diceRollerAdvantageStateGroupBox);

        tabs.Controls.Add(diceRollerTabPage);
        tabs.Controls.Add(probabilityTablesTabPage);
        tabs.Controls.Add(statFinderTabPage);

        Controls.Add(tabs);
        Controls.Add(usageText);
        Controls.Add(runButton);
        this.AcceptButton = runButton;

        InitializeComponent();
    }
}
