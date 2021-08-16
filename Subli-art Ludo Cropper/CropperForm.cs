#region Using
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Resources;
using System.Globalization;
#endregion

namespace Subli_art_Ludo_Cropper
{
    public enum PanelType { First, Last }
    enum Language { English, Italian }
    public partial class CropperForm : Form
    {
        #region Members
        bool m_nextPanel;
        int m_panelsIndex;
        string m_currFilename;
        float[] m_slotRatios;
        const int WM_KEYDOWN = 0x100;
        const int WM_SYSKEYDOWN = 0x104;
        public MenuStrip m_menuStrip;
        public ImagesLibraryClass m_imagesLibrary;
        CropPanelClass[] m_cropPanels;
        SlotsPanelClass[] m_slotsPanels;
        public Dictionary<string, string> m_langDictionary;
        Timer m_panelsTimer;
        PictureBox m_multiPanelPB;
        #endregion 

        #region Initialize
        public CropperForm()
        {
            InitializeComponent();

            // Set the program's language
            LoadLanguage(Language.English);

            // Set the cropper form
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            Icon = Properties.Resources.Icon;
            BackColor = Color.Black;
            Text = Name + " - " + m_langDictionary["Untitled"];
            DoubleBuffered = true;
            FormClosing += new FormClosingEventHandler(CropperForm_FormClosing);
        }
                        
        private void Initialize(object sender, EventArgs e)
        {
            // Set the file name
            m_currFilename = m_langDictionary["Untitled"];

            // MENU STRIP
            m_menuStrip = new MenuStrip();
            m_menuStrip.Name = "menuStrip";
            m_menuStrip.ForeColor = Color.FromArgb(255, 220, 208, 192);
            m_menuStrip.Renderer = new ToolStripProfessionalRenderer(new CustomMenuStripColorTable());
            this.Controls.Add(m_menuStrip);

            // Initialize the slot ratios array
            m_slotRatios = new float[2];
            m_slotRatios[0] = 1.0f;
            m_slotRatios[1] = 35.0f / 54.97f; // width / height
                        
            m_panelsTimer = new Timer();
            m_panelsTimer.Interval = 10;
            m_panelsTimer.Tick += new EventHandler(m_panelsTimer_Tick);

            InitializePanels();
            
            InitializeMenuStripItems();
        }

        private void LoadLanguage(Language lang)
        {
            CultureInfo cultureInfo = null;
            ResourceManager resourceManager = null;

            switch (lang)
            {
                case Language.Italian:
                {
                    cultureInfo = new CultureInfo("it-IT");
                    resourceManager = new ResourceManager(
                        "Subli_art_Ludo_Cropper.Languages.Lang-Italian", typeof(CropperForm).Assembly);
                    break;
                }
                default:
                {
                    cultureInfo = new CultureInfo("en-US");
                    resourceManager = new ResourceManager(
                        "Subli_art_Ludo_Cropper.Languages.Lang-English", typeof(CropperForm).Assembly);
                    break;
                }
            }

            // Initialize the dictionary
            m_langDictionary = new Dictionary<string, string>();

            // Alerts
            m_langDictionary["All slots must be full and approved"] = resourceManager.GetString("All_slots_must_be_full_and_approved", cultureInfo);
            m_langDictionary["Save changes to document"] = resourceManager.GetString("Save_changes_to_document", cultureInfo);
            m_langDictionary["before closing?"] = resourceManager.GetString("before_closing", cultureInfo);
            m_langDictionary["Some images could not be found"] = resourceManager.GetString("Some_images_could_not_be_found", cultureInfo);

            // File Menu
            m_langDictionary["File"] = resourceManager.GetString("File", cultureInfo);
            m_langDictionary["New"] = resourceManager.GetString("New", cultureInfo);
            m_langDictionary["Open"] = resourceManager.GetString("Open", cultureInfo);
            m_langDictionary["Save As"] = resourceManager.GetString("Save_As", cultureInfo);
            m_langDictionary["Save"] = resourceManager.GetString("Save", cultureInfo);
            m_langDictionary["Import Images"] = resourceManager.GetString("Import_Images", cultureInfo);
            m_langDictionary["Create PDF"] = resourceManager.GetString("Create_PDF", cultureInfo);
            m_langDictionary["Exit"] = resourceManager.GetString("Exit", cultureInfo);

            // Options menu
            m_langDictionary["Options"] = resourceManager.GetString("Options", cultureInfo);
            m_langDictionary["Next Slot"] = resourceManager.GetString("Next_Slot", cultureInfo);
            m_langDictionary["Previous Slot"] = resourceManager.GetString("Previous_Slot", cultureInfo);
            m_langDictionary["Clear Slot"] = resourceManager.GetString("Clear_Slot", cultureInfo);
            m_langDictionary["Approve Crop"] = resourceManager.GetString("Approve_Crop", cultureInfo);
            m_langDictionary["Clear Library"] = resourceManager.GetString("Clear_Library", cultureInfo);
            
            // Other
            m_langDictionary["Close"] = resourceManager.GetString("Close", cultureInfo);
            m_langDictionary["Untitled"] = resourceManager.GetString("Untitled", cultureInfo);
            m_langDictionary["Cancel"] = resourceManager.GetString("Cancel", cultureInfo);

            // Progress Bar Form
            m_langDictionary["Creating PDF..."] = resourceManager.GetString("Creating_PDF", cultureInfo);
            m_langDictionary["Finished"] = resourceManager.GetString("Finished", cultureInfo);
            m_langDictionary["Open PDF"] = resourceManager.GetString("Open_PDF", cultureInfo);
            m_langDictionary["Open PDF Folder"] = resourceManager.GetString("Open_PDF_Folder", cultureInfo);

            // Slots Panel
            m_langDictionary["Arrow Text Create"] = resourceManager.GetString("Arrow_Text_Create", cultureInfo);
            m_langDictionary["Arrow Text Exit"] = resourceManager.GetString("Arrow_Text_Exit", cultureInfo);
            m_langDictionary["Arrow Text Next"] = resourceManager.GetString("Arrow_Text_Next", cultureInfo);
            m_langDictionary["Slot"] = resourceManager.GetString("Slot", cultureInfo);
            m_langDictionary["Next"] = resourceManager.GetString("Next", cultureInfo);
            m_langDictionary["Back"] = resourceManager.GetString("Back", cultureInfo);
            
            // Crop Panel
            m_langDictionary["Board"] = resourceManager.GetString("Board", cultureInfo);
            m_langDictionary["Players"] = resourceManager.GetString("Players", cultureInfo);

            // User's Guide
            m_langDictionary["User Guide"] = resourceManager.GetString("User_Guide", cultureInfo);
        }

        private void InitializePanels()
        {
            // IMAGE LIBRARY
            m_imagesLibrary = new ImagesLibraryClass(this);

            // SLOTS PANEL
            m_slotsPanels = new SlotsPanelClass[2];

            // CROP PANELS
            m_cropPanels = new CropPanelClass[2];

            // Initialize panels
            for (int i = 0; i < m_cropPanels.Length; i++)
            {
                // Change the panels index for the
                // panels constructors
                m_panelsIndex = i;

                // If it's the first panel
                if (i == 0)
                {
                    // Initialize the panels
                    m_slotsPanels[i] = new SlotsPanelClass(this, m_slotRatios[i], PanelType.First);
                    m_cropPanels[i] = new CropPanelClass(this, m_slotRatios[i], PanelType.First);
                }
                // If it's the last panel
                else if (i == m_cropPanels.Length - 1)
                {
                    m_slotsPanels[i] = new SlotsPanelClass(this, m_slotRatios[i], PanelType.Last);
                    m_cropPanels[i] = new CropPanelClass(this, m_slotRatios[i], PanelType.Last);
                }
            }

            // Set the crop panels names
            m_cropPanels[0].Name = m_langDictionary["Board"];
            m_cropPanels[1].Name = m_langDictionary["Players"];

            // Set the panels bitmap
            m_cropPanels[1].Bitmap = Properties.Resources.Player;
            m_slotsPanels[1].Bitmap = Properties.Resources.Player;

            // Position the panel according 
            // to the index passed as argument
            m_panelsIndex = 0;
            PositionPanels();

            // MULTI PANEL PB
            m_multiPanelPB = new PictureBox();
            m_multiPanelPB.Size = new Size(
                m_cropPanels[0].Width + m_cropPanels[1].Width,
                m_cropPanels[0].Height + m_slotsPanels[0].Height);
            m_multiPanelPB.Location = new Point(m_cropPanels[0].Location.X, m_cropPanels[0].Location.Y);
            m_multiPanelPB.Visible = false;
            Controls.Add(m_multiPanelPB);
            m_multiPanelPB.BringToFront();
            m_imagesLibrary.BringToFront();
        }

        private void InitializeMenuStripItems()
        {
            // FILE
            ToolStripMenuItem fileMenu = new ToolStripMenuItem(m_langDictionary["File"]);

            ToolStripMenuItem newSubmenu = new ToolStripMenuItem(m_langDictionary["New"]);
            newSubmenu.ShortcutKeys = Keys.Control | Keys.N;
            newSubmenu.ForeColor = Color.FromArgb(255, 220, 208, 192);
            newSubmenu.Click += new EventHandler(newSubmenu_Click);
            fileMenu.DropDownItems.Add(newSubmenu);

            ToolStripMenuItem openSubmenu = new ToolStripMenuItem(m_langDictionary["Open"] + "...");
            openSubmenu.ShortcutKeys = Keys.Control | Keys.O;
            openSubmenu.ForeColor = Color.FromArgb(255, 220, 208, 192);
            openSubmenu.Click += new EventHandler(openSubmenu_Click);
            fileMenu.DropDownItems.Add(openSubmenu);

            ToolStripMenuItem saveAsSubmenu = new ToolStripMenuItem(m_langDictionary["Save As"] + "...");
            saveAsSubmenu.ShortcutKeys = Keys.Control | Keys.Shift | Keys.S;
            saveAsSubmenu.ForeColor = Color.FromArgb(255, 220, 208, 192);
            saveAsSubmenu.Click += new EventHandler(saveAsSubmenu_Click);
            fileMenu.DropDownItems.Add(saveAsSubmenu);

            ToolStripMenuItem saveSubmenu = new ToolStripMenuItem(m_langDictionary["Save"]);
            saveSubmenu.ShortcutKeys = Keys.Control | Keys.S;
            saveSubmenu.ForeColor = Color.FromArgb(255, 220, 208, 192);
            saveSubmenu.Click += new EventHandler(saveSubmenu_Click);
            fileMenu.DropDownItems.Add(saveSubmenu);

            // SEPERATING LINE
            fileMenu.DropDownItems.Add(new ToolStripSeparator());

            ToolStripMenuItem importImagesSubmenu = new ToolStripMenuItem(m_langDictionary["Import Images"] + "...");
            importImagesSubmenu.ForeColor = Color.FromArgb(255, 220, 208, 192);
            importImagesSubmenu.Click += new EventHandler(m_imagesLibrary.OpenFileDialog);
            fileMenu.DropDownItems.Add(importImagesSubmenu);

            ToolStripMenuItem createPDFSubmenu = new ToolStripMenuItem(m_langDictionary["Create PDF"] + "...");
            createPDFSubmenu.ShortcutKeys = Keys.Control | Keys.P;
            createPDFSubmenu.ForeColor = Color.FromArgb(255, 220, 208, 192);
            createPDFSubmenu.Click += new EventHandler(createPDFSubmenu_Click);
            fileMenu.DropDownItems.Add(createPDFSubmenu);

            // SEPERATING LINE
            fileMenu.DropDownItems.Add(new ToolStripSeparator());

            ToolStripMenuItem exitSubmenu = new ToolStripMenuItem(m_langDictionary["Exit"]);
            exitSubmenu.ShortcutKeys = Keys.Alt | Keys.F4;
            exitSubmenu.ForeColor = Color.FromArgb(255, 220, 208, 192);
            exitSubmenu.Click += new EventHandler(exitSubmenu_Click);
            fileMenu.DropDownItems.Add(exitSubmenu);

            m_menuStrip.Items.Add(fileMenu);            
            
            // OPTIONS
            ToolStripMenuItem optionsMenu = new ToolStripMenuItem(m_langDictionary["Options"]);

            ToolStripMenuItem nextSlotSubmenu = new ToolStripMenuItem(m_langDictionary["Next Slot"]);
            nextSlotSubmenu.ShortcutKeys = Keys.Control | Keys.Right;
            nextSlotSubmenu.ForeColor = Color.FromArgb(255, 220, 208, 192);
            nextSlotSubmenu.Click += new EventHandler(nextSlotSubmenu_Click);
            optionsMenu.DropDownItems.Add(nextSlotSubmenu);

            ToolStripMenuItem prevSlotSubmenu = new ToolStripMenuItem(m_langDictionary["Previous Slot"]);
            prevSlotSubmenu.ShortcutKeys = Keys.Control | Keys.Left;
            prevSlotSubmenu.ForeColor = Color.FromArgb(255, 220, 208, 192);
            prevSlotSubmenu.Click += new EventHandler(prevSlotSubmenu_Click);
            optionsMenu.DropDownItems.Add(prevSlotSubmenu);

            ToolStripMenuItem emptySlotSubmenu = new ToolStripMenuItem(m_langDictionary["Clear Slot"]);
            emptySlotSubmenu.ShortcutKeys = Keys.Delete;
            emptySlotSubmenu.ForeColor = Color.FromArgb(255, 220, 208, 192);
            emptySlotSubmenu.Click += new EventHandler(emptySlotSubmenu_Click);
            optionsMenu.DropDownItems.Add(emptySlotSubmenu);

            ToolStripMenuItem approveCropSubmenu = new ToolStripMenuItem(m_langDictionary["Approve Crop"]);
            approveCropSubmenu.ShortcutKeys = Keys.Control | Keys.Enter;
            approveCropSubmenu.ForeColor = Color.FromArgb(255, 220, 208, 192);
            approveCropSubmenu.Click += new EventHandler(approveCropSubmenu_Click);
            optionsMenu.DropDownItems.Add(approveCropSubmenu);
            
            // SEPERATING LINE
            optionsMenu.DropDownItems.Add(new ToolStripSeparator());

            ToolStripMenuItem clearAllSubmenu = new ToolStripMenuItem(m_langDictionary["Clear Library"]);
            clearAllSubmenu.ForeColor = Color.FromArgb(255, 220, 208, 192);
            clearAllSubmenu.Click += new EventHandler(clearAllSubmenu_Click);
            optionsMenu.DropDownItems.Add(clearAllSubmenu);

            m_menuStrip.Items.Add(optionsMenu);

            return;
        }
        #endregion

        #region Events
        private void newSubmenu_Click(object sender, EventArgs e)
        {
            // Show save document alert
            DialogResult dialogResult = MessageBox.Show(
                m_langDictionary["Save changes to document"] + " '" + Path.GetFileName(m_currFilename) + "' " + m_langDictionary["before closing?"],
                m_langDictionary["New"],
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);

            if (dialogResult == DialogResult.Yes)
            {
                saveSubmenu_Click(sender, e);

                // If the document was not saved
                if (Path.GetFileName(m_currFilename).Equals(m_langDictionary["Untitled"]))
                    dialogResult = DialogResult.Cancel;
            }

            if (dialogResult != DialogResult.Cancel)
                ResetSession();
        }

        private void openSubmenu_Click(object sender, EventArgs e)
        {
            // Initialize the open file dialog
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "SAL (*.sal)|*.sal";

            // Get the open file dialog result
            DialogResult result = fileDialog.ShowDialog();
            
            if (result == DialogResult.OK)
            {
                m_currFilename = fileDialog.FileName;
                Text = Name + " - " + Path.GetFileName(m_currFilename);
                OpenFile();
            }            
        }

        private void saveAsSubmenu_Click(object sender, EventArgs e)
        { 
            // Initialize the save file dialog
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "SAL (*.sal)|*.sal";
            saveDialog.FileName = "LudoFile";                                    
            
            // Get the save file dialog result
            DialogResult result = saveDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                m_currFilename = saveDialog.FileName;
                Text = Name + " - " + Path.GetFileName(m_currFilename);
                SaveFile();
            }            
        }

        private void saveSubmenu_Click(object sender, EventArgs e)
        {
            // If its not an already saved file
            if (m_currFilename.Equals(m_langDictionary["Untitled"]))
                saveAsSubmenu_Click(sender, e);
            else
                SaveFile();
        }

        private void clearAllSubmenu_Click(object sender, EventArgs e)
        {
            m_imagesLibrary.ClearAll();
        }

        private void createPDFSubmenu_Click(object sender, EventArgs e)
        {
            if (CurrSlotsPanel.RightButtonMode == RightButtonModes.Create)
                m_slotsPanels[m_panelsIndex].createPDF_Click(sender, e);
        }

        private void emptySlotSubmenu_Click(object sender, EventArgs e)
        {
            m_cropPanels[m_panelsIndex].DeleteImage();
        }

        private void prevSlotSubmenu_Click(object sender, EventArgs e)
        {
            m_slotsPanels[m_panelsIndex].PrevSlot();
        }

        private void nextSlotSubmenu_Click(object sender, EventArgs e)
        {
            m_slotsPanels[m_panelsIndex].NextSlot();
        }

        private void approveCropSubmenu_Click(object sender, EventArgs e)
        {
            // If the slot being approved is not empty
            if (!m_slotsPanels[m_panelsIndex].IsSlotEmpty(m_slotsPanels[m_panelsIndex].SlotIndex))
                m_cropPanels[m_panelsIndex].ApproveCrop();
        }

        public void exitSubmenu_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void CropperForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Show save document alert
            DialogResult dialogResult = MessageBox.Show(
                m_langDictionary["Save changes to document"] + " '" + Path.GetFileName(m_currFilename) + "' " + m_langDictionary["before closing?"],
                m_langDictionary["Close"],
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);

            if (dialogResult == DialogResult.Yes)
            {
                saveSubmenu_Click(sender, e);

                // If the document was not saved
                if (Path.GetFileName(m_currFilename).Equals(m_langDictionary["Untitled"]))
                    e.Cancel = true;
            }
            else if (dialogResult == DialogResult.Cancel)
                e.Cancel = true;
        }
        
        private void m_panelsTimer_Tick2(object sender, EventArgs e)
        {
            int speed = (int)(m_slotsPanels[0].Width * 0.1f);
            Point prevSlotsPanelPos = new Point(0, 0);
            Point prevCropPanelPos = new Point(0, 0);
            Point nextSlotsPanelPos = new Point(0, m_slotsPanels[m_panelsIndex].Location.Y);
            Point nextCropPanelPos = new Point(0, m_cropPanels[m_panelsIndex].Location.Y);

            
            // NEXT PANEL
            if (m_nextPanel)
            {
                prevSlotsPanelPos.Y = m_slotsPanels[m_panelsIndex - 1].Location.Y;
                prevCropPanelPos.Y = m_cropPanels[m_panelsIndex - 1].Location.Y;
                if (m_slotsPanels[m_panelsIndex].Location.X - speed > m_imagesLibrary.Width)
                {
                    // Previous slots panel
                    prevSlotsPanelPos.X = m_slotsPanels[m_panelsIndex - 1].Location.X - speed;

                    // Previous crop panel
                    prevCropPanelPos.X = m_cropPanels[m_panelsIndex - 1].Location.X - speed;

                    // Next slots panel
                    nextSlotsPanelPos.X = m_slotsPanels[m_panelsIndex].Location.X - speed;

                    // Next crop panel
                    nextCropPanelPos.X = m_cropPanels[m_panelsIndex].Location.X - speed;
                }
                else
                {
                    // Previous slots panel
                    prevSlotsPanelPos.X = m_imagesLibrary.Width - m_slotsPanels[m_panelsIndex - 1].Width;

                    // Previous crop panel
                    prevCropPanelPos.X = m_imagesLibrary.Width - m_cropPanels[m_panelsIndex - 1].Width;

                    // Next slots panel
                    nextSlotsPanelPos.X = m_imagesLibrary.Width;

                    // Next crop panel
                    nextCropPanelPos.X = m_imagesLibrary.Width;

                    ((Timer)sender).Stop();
                    m_cropPanels[m_panelsIndex].PlayArrow();
                }

                // Previous slots panel
                m_slotsPanels[m_panelsIndex - 1].Location = prevSlotsPanelPos;

                // Previous crop panel
                m_cropPanels[m_panelsIndex - 1].Location = prevCropPanelPos;
            }
            // PREVIOUS PANEL
            else
            {
                prevSlotsPanelPos.Y = m_slotsPanels[m_panelsIndex + 1].Location.Y;
                prevCropPanelPos.Y = m_cropPanels[m_panelsIndex + 1].Location.Y;
                if (m_slotsPanels[m_panelsIndex].Location.X + speed < m_imagesLibrary.Width)
                {
                    // Previous slots panel
                    prevSlotsPanelPos.X = m_slotsPanels[m_panelsIndex + 1].Location.X + speed;

                    // Previous crop panel
                    prevCropPanelPos.X = m_cropPanels[m_panelsIndex + 1].Location.X + speed;

                    // Next slots panel
                    nextSlotsPanelPos.X = m_slotsPanels[m_panelsIndex].Location.X + speed;

                    // Next crop panel
                    nextCropPanelPos.X = m_cropPanels[m_panelsIndex].Location.X + speed;
                }
                else
                {
                    // Previous slots panel
                    prevSlotsPanelPos.X = m_imagesLibrary.Width + m_slotsPanels[m_panelsIndex + 1].Width;

                    // Previous crop panel
                    prevCropPanelPos.X = m_imagesLibrary.Width + m_cropPanels[m_panelsIndex + 1].Width;

                    // Next slots panel
                    nextSlotsPanelPos.X = m_imagesLibrary.Width;

                    // Next crop panel
                    nextCropPanelPos.X = m_imagesLibrary.Width;

                    ((Timer)sender).Stop();
                    m_cropPanels[m_panelsIndex].PlayArrow();
                }

                // Previous slots panel
                m_slotsPanels[m_panelsIndex + 1].Location = prevSlotsPanelPos;

                // Previous crop panel
                m_cropPanels[m_panelsIndex + 1].Location = prevCropPanelPos;
            }
            
            // Next slots panel
            m_slotsPanels[m_panelsIndex].Location = nextSlotsPanelPos;

            // Next crop panel
            m_cropPanels[m_panelsIndex].Location = nextCropPanelPos;
        }

        private void m_panelsTimer_Tick(object sender, EventArgs e)
        {
            bool done = false;
            int speed = 70;
            Point newPos = m_multiPanelPB.Location;

            if (m_nextPanel)
            {
                if (newPos.X - speed > m_imagesLibrary.Width - m_multiPanelPB.Width / 2.0f)
                    newPos.X -= speed;
                else
                {
                    newPos.X = (int)(m_imagesLibrary.Width - (m_multiPanelPB.Width / 2.0f));
                    done = true;
                }
            }
            else
            {
                if (newPos.X + speed < m_imagesLibrary.Width)
                    newPos.X += speed;
                else
                {
                    newPos.X = m_imagesLibrary.Width;
                    done = true;
                }
            }

            // Set the multi bitmap pb position
            m_multiPanelPB.Location = newPos;

            if (done)
            {
                m_panelsTimer.Stop();
                m_multiPanelPB.Visible = false;

                // Play the arrow
                m_cropPanels[m_panelsIndex].PlayArrow();
            }
        }
        #endregion
        
        #region Methods
        private void SaveFile()
        {
            try
            {
                // Save the necessary data for the serialization
                LudoFileClass ludoFile = new LudoFileClass(m_slotsPanels.Length);

                ludoFile.m_panelIndex = m_panelsIndex;

                // IMAGE LIBRARY
                ludoFile.m_imagesPaths = m_imagesLibrary.ImagesPaths;

                // PANELS
                for (int i = 0; i < m_slotsPanels.Length; i++)
                {
                    // SLOTS PANELS
                    ludoFile.m_sourceRectangles[i] = m_slotsPanels[i].SourceRectangles;
                    ludoFile.m_rectangleColors[i] = m_slotsPanels[i].RectanglesColors;
                    ludoFile.m_bitmaps[i] = m_slotsPanels[i].Bitmaps;
                    ludoFile.m_slotInd[i] = m_slotsPanels[i].SlotIndex;
                    ludoFile.m_approvedSlots[i] = m_slotsPanels[i].ApprovedSlots;
                    ludoFile.m_rightButtonMode[i] = m_slotsPanels[i].RightButtonMode;
                }

                // Serialize
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(m_currFilename, FileMode.Create, FileAccess.Write, FileShare.None);
                formatter.Serialize(stream, ludoFile);
                stream.Close();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, m_langDictionary["Save"], MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void OpenFile()
        {
            try
            {
                // Get data from the saved file
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(m_currFilename, FileMode.Open, FileAccess.Read, FileShare.Read);
                LudoFileClass ludoFile = (LudoFileClass)formatter.Deserialize(stream);
                stream.Close();

                // Pass the index of the focused panel
                m_panelsIndex = ludoFile.m_panelIndex;
                PositionPanels();

                // Pass the data to the side bar
                m_imagesLibrary.ImportFromFile(ludoFile.m_imagesPaths);
                
                for (int i = 0; i < m_slotsPanels.Length; i++)
                {
                    // SLOTS PANEL
                    m_slotsPanels[i].ImportFromFile(
                        ludoFile.m_sourceRectangles[i],
                        ludoFile.m_rectangleColors[i],
                        ludoFile.m_bitmaps[i],
                        ludoFile.m_slotInd[i],
                        ludoFile.m_approvedSlots[i], i);

                    m_slotsPanels[i].ChangeRightButtonMode(ludoFile.m_rightButtonMode[i]);

                    // CROP PANELS
                    m_cropPanels[i].DisplayImage(
                        ludoFile.m_bitmaps[i][ludoFile.m_slotInd[i]],
                        ludoFile.m_sourceRectangles[i][ludoFile.m_slotInd[i]]);

                    m_cropPanels[i].ChangeArrowMode(ludoFile.m_rightButtonMode[i]);
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, m_langDictionary["Open"], MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void ResetSession()
        {
            // Reset the ludo form
            m_currFilename = m_langDictionary["Untitled"];
            Text = Name + " - " + m_currFilename;

            // Reset side bar
            m_imagesLibrary.ImportFromFile(new string[0]);

            for (int i = 0; i < m_slotsPanels.Length; i++)
            {
                // Reset slots panel
                m_slotsPanels[i].Reset();

                // Reset crop panel
                m_cropPanels[i].DeleteImage();
            }

            // Reset the panels index
            m_panelsIndex = 0;

            PositionPanels();
        }

        public void NextPanels()
        {
            if (m_panelsIndex + 1 < m_slotsPanels.Length)
            {
                m_nextPanel = true;
                
                // Pause all other timers
                m_cropPanels[m_panelsIndex].PauseArrow();
                m_cropPanels[m_panelsIndex + 1].PauseArrow();

                // Place the multi panel bitmap
                // on top of the panels
                SetMultiPanel();

                // Place the panels
                PositionPanels();

                // Start the panels timer
                m_panelsTimer.Start();
            }
        }

        public void BackPanels()
        {
            if (m_panelsIndex - 1 >= 0)
            {
                m_nextPanel = false;

                // Pause all other timers
                m_cropPanels[m_panelsIndex].PauseArrow();
                m_cropPanels[m_panelsIndex - 1].PauseArrow();

                // Place the multi panel bitmap
                // on top of the panels
                SetMultiPanel();

                // Place the panels
                PositionPanels();

                // Start the panels timer
                m_panelsTimer.Start();
            }
        }

        private void SetMultiPanel()
        {
            Bitmap
                finalBmp = null,
                currSlotsPanelBmp = null, nextSlotsPanelBmp = null,
                currCropPanelBmp = null, nextCropPanelBmp = null;



            // Create the panels bitmaps
            currSlotsPanelBmp = m_slotsPanels[m_panelsIndex].PanelBitmap;
            currCropPanelBmp = m_cropPanels[m_panelsIndex].PanelBitmap;

            if (m_nextPanel)
            {
                m_panelsIndex++;
                nextSlotsPanelBmp = m_slotsPanels[m_panelsIndex].PanelBitmap;
                nextCropPanelBmp = m_cropPanels[m_panelsIndex].PanelBitmap;
                m_multiPanelPB.Location = m_cropPanels[m_panelsIndex - 1].Location;


                // Create the multi bitmap
                finalBmp = CreateMultiPanelBMP(
                    currCropPanelBmp, nextCropPanelBmp,
                    currSlotsPanelBmp, nextSlotsPanelBmp);
            }
            else
            {

                m_panelsIndex--;
                nextSlotsPanelBmp = m_slotsPanels[m_panelsIndex].PanelBitmap;
                nextCropPanelBmp = m_cropPanels[m_panelsIndex].PanelBitmap;
                m_multiPanelPB.Location = m_cropPanels[m_panelsIndex].Location;


                // Create the multi bitmap
                finalBmp = CreateMultiPanelBMP(
                    nextCropPanelBmp, currCropPanelBmp,
                    nextSlotsPanelBmp ,currSlotsPanelBmp);
            }


            // Set the multi bitmap to the pb
            m_multiPanelPB.BackgroundImage = finalBmp;
            m_multiPanelPB.Visible = true;
        }

        public void PositionPanels()
        {
            int firstPanelPosX = m_imagesLibrary.Width - (m_panelsIndex * m_slotsPanels[m_panelsIndex].Width);

            for (int i = 0; i < m_slotsPanels.Length; i++)
            {
                // Position the panels
                m_cropPanels[i].Location = new Point(
                    firstPanelPosX + (i * m_cropPanels[i].Width),
                    m_cropPanels[i].Location.Y);

                m_slotsPanels[i].Location = new Point(
                    firstPanelPosX + (i * m_slotsPanels[i].Width),
                    m_cropPanels[i].Location.Y + m_cropPanels[i].Height);
            }
        }

        public Bitmap CreateMultiPanelBMP(
            Bitmap currCropPanelBmp, Bitmap nextCropPanelBmp,
            Bitmap currSlotsPanelBmp, Bitmap nextSlotsPanelBmp)
        {
            int width = 0;
            int height = 0;
            Bitmap finalImage = null;

            try
            {
                width = currCropPanelBmp.Width + nextCropPanelBmp.Width;
                height = currCropPanelBmp.Height + currSlotsPanelBmp.Height;
                
                //create a bitmap to hold the combined image
                finalImage = new Bitmap(width, height);

                //get a graphics object from the image so we can draw on it
                using (Graphics g = Graphics.FromImage(finalImage))
                {
                    //set background color
                    g.Clear(Color.Black);

                    // CURRENT CROP PANEL
                    g.DrawImage(currCropPanelBmp,
                      new Rectangle(0, 0, currCropPanelBmp.Width, currCropPanelBmp.Height));

                    // CURRENT SLOTS PANEL
                    g.DrawImage(currSlotsPanelBmp,
                      new Rectangle(0, currCropPanelBmp.Height, currSlotsPanelBmp.Width, currSlotsPanelBmp.Height));

                    // NEXT CROP PANEL
                    g.DrawImage(nextCropPanelBmp,
                      new Rectangle(currCropPanelBmp.Width, 0, nextCropPanelBmp.Width, nextCropPanelBmp.Height));

                    // NEXT SLOTS PANEL
                    g.DrawImage(nextSlotsPanelBmp,
                      new Rectangle(currSlotsPanelBmp.Width, nextCropPanelBmp.Height, nextSlotsPanelBmp.Width, nextSlotsPanelBmp.Height));
                }

                return finalImage;
            }
            catch (Exception ex)
            {
                if (finalImage != null)
                    finalImage.Dispose();

                throw ex;
            }
        }
        #endregion

        #region Properties
        public float SlotRatio
        {
            get { return m_slotRatios[m_panelsIndex]; }
        }
        
        public SlotsPanelClass CurrSlotsPanel
        {
            get { return m_slotsPanels[m_panelsIndex]; }
        }

        public SlotsPanelClass[] SlotsPanels
        {
            get { return m_slotsPanels; }
        }

        public CropPanelClass CurrCropPanel
        {
            get { return m_cropPanels[m_panelsIndex]; }
        }

        public CropPanelClass[] CropPanels
        {
            get { return m_cropPanels; }
        }
        #endregion
    }

    #region LudoFileClass
    [Serializable]
    public class LudoFileClass
    {
        public int m_panelIndex;
        public string[] m_imagesPaths;
        public int[] m_slotInd;
        public Color[][] m_rectangleColors;
        public Bitmap[][] m_bitmaps;
        public RectangleF[][] m_sourceRectangles;
        public bool[][] m_approvedSlots;
        public RightButtonModes[] m_rightButtonMode;
        
        public LudoFileClass(int panelsCount)
        {
            m_panelIndex = 0;

            // IMAGES LIBRARY
            m_imagesPaths = null;

            // SLOTS PANEL
            m_slotInd = new int[panelsCount];
            m_rectangleColors = new Color[panelsCount][];            
            m_sourceRectangles = new RectangleF[panelsCount][];
            m_bitmaps = new Bitmap[panelsCount][];
            m_approvedSlots = new bool[panelsCount][];
            m_rightButtonMode = new RightButtonModes[panelsCount];
        }
    } 
    #endregion

    #region CustomizedMenuStrip
    // This class defines the gradient colors for 
    // the MenuStrip and the ToolStrip.
    class CustomMenuStripColorTable : ProfessionalColorTable
    {
        public override Color MenuStripGradientBegin
        {
            get { return Color.FromArgb(255, 78, 78, 78); }
        }

        public override Color MenuStripGradientEnd
        {
            get { return Color.FromArgb(255, 78, 78, 78); }
        }

        public override Color MenuItemBorder  //added for changing the menu border
        {
            get { return Color.Transparent; }
        }

        public override Color MenuItemSelectedGradientBegin
        {
            get { return Color.FromArgb(255, 100, 100, 100); }
        }

        public override Color MenuItemSelectedGradientEnd
        {
            get { return Color.FromArgb(255, 100, 100, 100); }
        }

        public override Color MenuItemPressedGradientBegin
        {
            get { return Color.FromArgb(255, 100, 100, 100); }
        }

        public override Color MenuItemPressedGradientEnd
        {
            get { return Color.FromArgb(255, 100, 100, 100); }
        }

        public override Color ToolStripDropDownBackground
        {
            get { return Color.FromArgb(255, 78, 78, 78); }
        }

        public override Color ImageMarginGradientBegin
        {
            get { return Color.FromArgb(255, 100, 100, 100); }
        }

        public override Color ImageMarginGradientMiddle
        {
            get { return Color.FromArgb(255, 100, 100, 100); }
        }

        public override Color ImageMarginGradientEnd
        {
            get { return Color.FromArgb(255, 100, 100, 100); }
        }

        public override Color MenuItemSelected
        {
            get { return Color.FromArgb(255, 100, 100, 100); }
        }
    }
    #endregion
}