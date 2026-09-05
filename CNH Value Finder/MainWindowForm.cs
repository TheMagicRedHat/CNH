namespace CNH;

public partial class MainWindowForm : Form
{
    public MainWindowForm()
    {
        Label testLabel = new Label()
        {
            Text = "TEMP",
            Location = new Point(0, 0),
            TabIndex = 10
        };

        TextBox testTextBox = new TextBox()
        {
            Location = new Point(testLabel.Location.X, testLabel.Bounds.Bottom + Padding.Top),
            TabIndex = 11
        };

        Controls.Add(testLabel);
        Controls.Add(testTextBox);

        InitializeComponent();
    }
}
