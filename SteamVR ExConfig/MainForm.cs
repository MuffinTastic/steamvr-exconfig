using System.Diagnostics;
using System.Windows.Forms;

namespace SteamVR_ExConfig;

public partial class MainForm : Form
{
    private List<VRAppSetting> vrApps;
    private SteamVRConfig vrConfig;

    public MainForm( List<VRAppSetting> vrApps, SteamVRConfig vrConfig )
    {
        InitializeComponent();

        this.vrApps = vrApps;
        this.vrConfig = vrConfig;

        SuspendLayout();
        InitializeSettingList( autolaunchGroupBox, vrApps );
        InitializeSettingList( driverGroupBox, vrConfig.DriverSettings! );
        ResumeLayout( false );
    }

    private Label CreateSeparator()
    {
        var separator = new Label();
        separator.Anchor = AnchorStyles.None;
        separator.AutoSize = false;
        separator.Name = "separator";
        separator.Width = 300;
        separator.Height = 2;
        separator.BorderStyle = BorderStyle.Fixed3D;
        separator.Text = "";
        return separator;
    }

    private void InitializeSettingList( GroupBox parent, IEnumerable<IVRSetting> settingsList )
    {
        parent.SuspendLayout();

        var settingsTable = new TableLayoutPanel();
        settingsTable.SuspendLayout();

        settingsTable.Anchor = AnchorStyles.Top;
        settingsTable.AutoScroll = false;
        settingsTable.ColumnCount = 2;
        settingsTable.ColumnStyles.Add( new ColumnStyle( SizeType.Percent, 85F ) );
        settingsTable.ColumnStyles.Add( new ColumnStyle( SizeType.Percent, 15F ) );
        settingsTable.Location = new Point( 0, 15 );
        settingsTable.Name = $"{parent.Name}_settingsTable";
        settingsTable.Dock = DockStyle.Fill;
        settingsTable.AutoSize = true;
        settingsTable.TabIndex = 0;

        int itemCount = 0;
        int maxCount = settingsList.Count();
        settingsTable.RowCount = 0;
        settingsTable.HorizontalScroll.Maximum = 0;
        settingsTable.AutoScroll = true;

        foreach ( var setting in settingsList )
        {
            itemCount++;

            var settingLabel = new Label();
            settingLabel.Anchor = AnchorStyles.None;
            settingLabel.AutoSize = true;
            settingLabel.Location = new Point( 121, 7 );
            settingLabel.Name = $"{settingsTable.Name}_{setting.ReadableName}_label";
            settingLabel.TabIndex = 1;
            settingLabel.Text = setting.ReadableName;
            settingsTable.Controls.Add( settingLabel, 0, settingsTable.RowCount );

            var settingToggle = new CheckBox();
            settingToggle.Anchor = AnchorStyles.None;
            settingToggle.AutoSize = true;
            settingToggle.Location = new Point( 0, 0 );
            settingToggle.Name = $"{settingsTable.Name}_{setting.ReadableName}_toggle";
            settingToggle.TabIndex = 0;
            settingToggle.UseVisualStyleBackColor = true;
            settingToggle.Checked = setting.Enabled;
            settingToggle.CheckedChanged += ( _, _ ) => setting.SetEnabled( settingToggle.Checked );
            settingsTable.Controls.Add( settingToggle, 1, settingsTable.RowCount );
            
            settingsTable.RowCount++;
            settingsTable.RowStyles.Add( new RowStyle( SizeType.Absolute, 30F ) );

            if ( itemCount != maxCount )
            {
                settingsTable.Controls.Add( CreateSeparator(), 0, settingsTable.RowCount );

                settingsTable.RowCount++;
                settingsTable.RowStyles.Add( new RowStyle( SizeType.Absolute, 2F ) );
            }
        }

        settingsTable.RowCount++;
        settingsTable.RowStyles.Add( new RowStyle( SizeType.Absolute, 5F ) );

        settingsTable.ResumeLayout( false );
        settingsTable.PerformLayout();

        parent.Controls.Add( settingsTable );
        parent.ResumeLayout( false );
        parent.PerformLayout();
    }

    private void MainForm_Load( object sender, EventArgs e )
    {

    }

    private async void saveButton_Click( object sender, EventArgs e )
    {
        var button = sender as Button;
        if ( button is null )
            return;

        // Save changed apps
        foreach ( var vrApp in vrApps )
        {
            vrApp.SaveToFile();
        }

        // Save drivers
        vrConfig.SaveToFile();

        button.Text = "Saved...";
        button.Enabled = false;
        await Task.Delay( 500 );
        button.Text = "Save";
        button.Enabled = true;
    }
}
