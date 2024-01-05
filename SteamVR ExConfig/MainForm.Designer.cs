namespace SteamVR_ExConfig;

partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose( bool disposing )
    {
        if ( disposing && ( components != null ) )
        {
            components.Dispose();
        }
        base.Dispose( disposing );
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        saveButton = new Button();
        formLayout = new TableLayoutPanel();
        tableLayoutPanel1 = new TableLayoutPanel();
        themeToggle = new Button();
        splitContainer = new SplitContainer();
        label1 = new Label();
        autolaunchGroupBox = new GroupBox();
        driverGroupBox = new GroupBox();
        formLayout.SuspendLayout();
        tableLayoutPanel1.SuspendLayout();
        ( (System.ComponentModel.ISupportInitialize) splitContainer ).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        SuspendLayout();
        // 
        // saveButton
        // 
        saveButton.Dock = DockStyle.Fill;
        saveButton.FlatStyle = FlatStyle.Flat;
        saveButton.Location = new Point( 38, 3 );
        saveButton.Name = "saveButton";
        saveButton.Size = new Size( 287, 28 );
        saveButton.TabIndex = 1;
        saveButton.Text = "Save";
        saveButton.UseVisualStyleBackColor = false;
        saveButton.Click +=  saveButton_Click ;
        // 
        // formLayout
        // 
        formLayout.ColumnCount = 1;
        formLayout.ColumnStyles.Add( new ColumnStyle() );
        formLayout.Controls.Add( tableLayoutPanel1, 0, 1 );
        formLayout.Controls.Add( splitContainer, 0, 0 );
        formLayout.Dock = DockStyle.Fill;
        formLayout.Location = new Point( 0, 0 );
        formLayout.Name = "formLayout";
        formLayout.RowCount = 2;
        formLayout.RowStyles.Add( new RowStyle( SizeType.Percent, 100F ) );
        formLayout.RowStyles.Add( new RowStyle( SizeType.Absolute, 40F ) );
        formLayout.RowStyles.Add( new RowStyle( SizeType.Absolute, 20F ) );
        formLayout.Size = new Size( 334, 443 );
        formLayout.TabIndex = 5;
        // 
        // tableLayoutPanel1
        // 
        tableLayoutPanel1.ColumnCount = 2;
        tableLayoutPanel1.ColumnStyles.Add( new ColumnStyle( SizeType.Absolute, 35F ) );
        tableLayoutPanel1.ColumnStyles.Add( new ColumnStyle( SizeType.Percent, 100F ) );
        tableLayoutPanel1.Controls.Add( saveButton, 1, 0 );
        tableLayoutPanel1.Controls.Add( themeToggle, 0, 0 );
        tableLayoutPanel1.Dock = DockStyle.Fill;
        tableLayoutPanel1.Location = new Point( 3, 406 );
        tableLayoutPanel1.Name = "tableLayoutPanel1";
        tableLayoutPanel1.RowCount = 1;
        tableLayoutPanel1.RowStyles.Add( new RowStyle( SizeType.Percent, 100F ) );
        tableLayoutPanel1.Size = new Size( 328, 34 );
        tableLayoutPanel1.TabIndex = 0;
        // 
        // themeToggle
        // 
        themeToggle.Dock = DockStyle.Fill;
        themeToggle.FlatStyle = FlatStyle.Flat;
        themeToggle.Location = new Point( 3, 3 );
        themeToggle.Name = "themeToggle";
        themeToggle.Size = new Size( 29, 28 );
        themeToggle.TabIndex = 2;
        themeToggle.Text = "☼";
        themeToggle.UseVisualStyleBackColor = false;
        themeToggle.Click +=  themeToggle_Click ;
        // 
        // splitContainer
        // 
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.Location = new Point( 3, 3 );
        splitContainer.Name = "splitContainer";
        splitContainer.Orientation = Orientation.Horizontal;
        // 
        // splitContainer.Panel1
        // 
        splitContainer.Panel1.Controls.Add( label1 );
        splitContainer.Panel1.Controls.Add( autolaunchGroupBox );
        // 
        // splitContainer.Panel2
        // 
        splitContainer.Panel2.Controls.Add( driverGroupBox );
        splitContainer.Size = new Size( 328, 397 );
        splitContainer.SplitterDistance = 198;
        splitContainer.TabIndex = 2;
        // 
        // label1
        // 
        label1.BorderStyle = BorderStyle.Fixed3D;
        label1.Dock = DockStyle.Bottom;
        label1.Location = new Point( 0, 196 );
        label1.Name = "label1";
        label1.Size = new Size( 328, 2 );
        label1.TabIndex = 1;
        // 
        // autolaunchGroupBox
        // 
        autolaunchGroupBox.Dock = DockStyle.Fill;
        autolaunchGroupBox.Location = new Point( 0, 0 );
        autolaunchGroupBox.Name = "autolaunchGroupBox";
        autolaunchGroupBox.Size = new Size( 328, 198 );
        autolaunchGroupBox.TabIndex = 0;
        autolaunchGroupBox.TabStop = false;
        autolaunchGroupBox.Text = "Autolaunch";
        // 
        // driverGroupBox
        // 
        driverGroupBox.Dock = DockStyle.Fill;
        driverGroupBox.Location = new Point( 0, 0 );
        driverGroupBox.Name = "driverGroupBox";
        driverGroupBox.Size = new Size( 328, 195 );
        driverGroupBox.TabIndex = 0;
        driverGroupBox.TabStop = false;
        driverGroupBox.Text = "Drivers";
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF( 7F, 15F );
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size( 334, 443 );
        Controls.Add( formLayout );
        MaximizeBox = false;
        MaximumSize = new Size( 350, 800 );
        MinimumSize = new Size( 350, 400 );
        Name = "MainForm";
        Text = "SteamVR External Configurator";
        Load +=  MainForm_Load ;
        formLayout.ResumeLayout( false );
        tableLayoutPanel1.ResumeLayout( false );
        splitContainer.Panel1.ResumeLayout( false );
        splitContainer.Panel2.ResumeLayout( false );
        ( (System.ComponentModel.ISupportInitialize) splitContainer ).EndInit();
        splitContainer.ResumeLayout( false );
        ResumeLayout( false );
    }

    #endregion
    private Button saveButton;
    private TableLayoutPanel formLayout;
    private SplitContainer splitContainer;
    private GroupBox autolaunchGroupBox;
    private GroupBox driverGroupBox;
    private Label label1;
    private TableLayoutPanel tableLayoutPanel1;
    private Button themeToggle;
}
