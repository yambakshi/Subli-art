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

namespace Subli_art_Domino_Cropper
{
    enum Language { English, Italian }
    public partial class CropperForm : Form
    {
        #region Members
        public Dictionary<string, string> m_langDictionary;
        const int WM_KEYDOWN = 0x100;
        const int WM_SYSKEYDOWN = 0x104;

        const float SLOT_RATIO = 297.5f / 360f; // width / height
        string m_currFilename;
        public MenuStrip m_menuStrip;
        public ImagesLibraryClass m_imagesLibrary;
        public CropPanelClass m_cropPanel;
        public SlotsPanelClass m_slotsPanel;
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
            m_currFilename = m_langDictionary["Untitled"];

            m_menuStrip = new MenuStrip();
            //m_menuStrip.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            m_menuStrip.Name = "menuStrip";
            m_menuStrip.ForeColor = Color.FromArgb(255, 220, 208, 192);
            m_menuStrip.Renderer = new ToolStripProfessionalRenderer(new CustomMenuStripColorTable());
            this.Controls.Add(m_menuStrip);

            m_imagesLibrary = new ImagesLibraryClass(this);

            m_slotsPanel = new SlotsPanelClass(this);

            m_cropPanel = new CropPanelClass(this);

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
                        "Subli_art_Domino_Cropper.Languages.Lang-Italian", typeof(CropperForm).Assembly);
                    break;
                }
                default:
                {
                    cultureInfo = new CultureInfo("en-US");
                    resourceManager = new ResourceManager(
                        "Subli_art_Domino_Cropper.Languages.Lang-English", typeof(CropperForm).Assembly);
                    break;
                }
            }

            // Initialize the dictionary
            m_langDictionary = new Dictionary<string, string>();

            // Alerts
            m_langDictionary["All slots must be approved"] = resourceManager.GetString("All_slots_must_be_approved", cultureInfo);
            m_langDictionary["All slots must be full"] = resourceManager.GetString("All_slots_must_be_full", cultureInfo);
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
            m_langDictionary["Arrow Text"] = resourceManager.GetString("Arrow_Text", cultureInfo);
            m_langDictionary["Slot"] = resourceManager.GetString("Slot", cultureInfo);

            // User's Guide
            m_langDictionary["User Guide"] = resourceManager.GetString("User_Guide", cultureInfo);
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
            fileDialog.Filter = "SAD (*.sad)|*.sad";

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
            saveDialog.Filter = "SAD (*.sad)|*.sad";
            saveDialog.FileName = "DominoFile";                                    
            
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
            m_slotsPanel.createPDF_Click(sender, e);
        }

        private void emptySlotSubmenu_Click(object sender, EventArgs e)
        {
            m_cropPanel.DeleteImage();
        }

        private void prevSlotSubmenu_Click(object sender, EventArgs e)
        {
            m_slotsPanel.PrevSlot();
        }

        private void nextSlotSubmenu_Click(object sender, EventArgs e)
        {
            m_slotsPanel.NextSlot();
        }

        private void approveCropSubmenu_Click(object sender, EventArgs e)
        {
            // If the slot being approved is not empty
            if (!m_slotsPanel.IsSlotEmpty(m_slotsPanel.SlotIndex))
                m_cropPanel.ApproveCrop();
        }

        private void exitSubmenu_Click(object sender, EventArgs e)
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
        #endregion
        
        #region Functions
        private void SaveFile()
        {
            try
            {
                // Save the necessary data for the serialization
                DominoFileClass dominoFile = new DominoFileClass();
                dominoFile.m_imagesPaths = m_imagesLibrary.ImagesPaths;
                dominoFile.m_sourceRectangles = m_slotsPanel.SourceRectangles;
                dominoFile.m_rectangleColors = m_slotsPanel.RectanglesColors;
                dominoFile.m_bitmaps = m_slotsPanel.Bitmaps;
                dominoFile.m_slotInd = m_slotsPanel.SlotIndex;
                dominoFile.m_approvedSlots = m_slotsPanel.ApprovedSlots;

                // Serialize
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(m_currFilename, FileMode.Create, FileAccess.Write, FileShare.None);
                formatter.Serialize(stream, dominoFile);
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
                DominoFileClass dominoFile = (DominoFileClass)formatter.Deserialize(stream);
                stream.Close();

                // Pass the data to the side bar
                m_imagesLibrary.ImportFromFile(dominoFile.m_imagesPaths);

                // Pass the data to the slots panel
                m_slotsPanel.ImportFromFile(
                    dominoFile.m_sourceRectangles,
                    dominoFile.m_rectangleColors,
                    dominoFile.m_bitmaps,
                    dominoFile.m_slotInd,
                    dominoFile.m_approvedSlots);

                // Pass the data to the crop panel
                m_cropPanel.DisplayImage(
                    dominoFile.m_bitmaps[dominoFile.m_slotInd],
                    dominoFile.m_sourceRectangles[dominoFile.m_slotInd]);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, m_langDictionary["Open"], MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void ResetSession()
        {
            // Reset the domino form
            m_currFilename = m_langDictionary["Untitled"];
            Text = Name + " - " + m_currFilename;

            // Reset side bar
            m_imagesLibrary.ImportFromFile(new string[0]);

            // Reset slots panel
            m_slotsPanel.Reset();

            // Reset crop panel
            m_cropPanel.DeleteImage();
        }
        #endregion

        #region Properties
        public float SlotRatio
        {
            get { return SLOT_RATIO; }
        }
        #endregion
    }

    #region DominoFileClass
    [Serializable]
    public class DominoFileClass
    {
        public int m_slotInd;
        public string[] m_imagesPaths;
        public Color[] m_rectangleColors;
        public Bitmap[] m_bitmaps;
        public RectangleF[] m_sourceRectangles;
        public bool[] m_approvedSlots;

        public DominoFileClass()
        {
            m_slotInd = 0;
            m_imagesPaths = null;
            m_rectangleColors = null;
            m_sourceRectangles = null;
            m_bitmaps = null;
            m_approvedSlots = null;
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