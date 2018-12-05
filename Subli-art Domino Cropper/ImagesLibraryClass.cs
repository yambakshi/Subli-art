#region Using
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
#endregion

namespace Subli_art_Domino_Cropper
{
    public class ImagesLibraryClass
    {
        // This delegate enables asynchronous calls for setting 
        // the text property on a TextBox control. 
        delegate void SetImagePanelCallback(PictureBox pb);

        #region Members
        public bool
            m_ctrl,
            m_shift;
        int
            m_imgSize,
            m_imgGap,
            m_slideSpeed;
        string[] m_paths;
        CropperForm m_cropperForm;
        OpenFileDialog m_fileDialog;
        Panel
            m_imagesLibrary,
            m_buttonsPanel,
            m_imagesPanel,
            m_instPanel;
        Button
            m_clearLibraryButt,
            m_importImagesButt,
            m_hideInstButt;
        List<PictureBox> m_images;
        BackgroundWorker m_backgroundWorker;
        ProgressBar m_progressBar;
        Timer m_toggleInst;
        #endregion

        #region Initialize
        public ImagesLibraryClass(CropperForm cropperForm)
        {
            m_cropperForm = cropperForm;

            InitializeFileDialog();

            InitializeImagesLibrary();
        }

        private void InitializeFileDialog()
        {
            m_fileDialog = new OpenFileDialog();

            // Set filter options and filter index.
            m_fileDialog.Filter =
                "All Formats (*.*)|*.*|" +
                "BMP (*.bmp; *.rle; *.dib)|*.bmp; *.rle; *.dib;|" +
                "JPEG (*.jpg; *.jpeg; *.jpe)|*.jpg; *.jpeg; *.jpe;|" +
                "PNG (*.png; *.pns)|*.png; *.pns;";
            m_fileDialog.FilterIndex = 1;
            m_fileDialog.Multiselect = true;
        }

        private void InitializeImagesLibrary()
        {
            // IMAGES LIBRARY
            m_imagesLibrary = new Panel();
            m_imagesLibrary.Size = new Size(
                (int)(m_cropperForm.ClientSize.Width * 0.3f),
                m_cropperForm.ClientSize.Height - m_cropperForm.Controls["menuStrip"].Height);
            m_imagesLibrary.Location = new Point(0, m_cropperForm.Controls["menuStrip"].Height);
            m_imagesLibrary.BackColor = Color.Black;
            m_imagesLibrary.AllowDrop = true;
            m_imagesLibrary.DragEnter += new DragEventHandler(DragEnter);
            m_imagesLibrary.DragDrop += new DragEventHandler(DragDrop);

            // Initialize the images library variables
            m_images = new List<PictureBox>();
            m_imgGap = (int)(m_imagesLibrary.Size.Width * 0.008f);
            m_imgSize = (int)((m_imagesLibrary.Width - (m_imgGap * 4) - SystemInformation.VerticalScrollBarWidth) / 3.0f);
            m_slideSpeed = 30;

            // IMAGE BAR
            m_buttonsPanel = new Panel();
            m_buttonsPanel.Size = new Size(m_imagesLibrary.Width, (int)(m_imagesLibrary.Height * 0.04f));
            m_buttonsPanel.BackColor = Color.FromArgb(255, 78, 78, 78);
            m_buttonsPanel.Paint += new PaintEventHandler(m_buttonsPanel_Paint);

            // LOAD IMAGE BUTTON
            m_importImagesButt = new Button();
            m_importImagesButt.Text = m_cropperForm.m_langDictionary["Import Images"];
            m_importImagesButt.FlatStyle = FlatStyle.Flat;
            m_importImagesButt.FlatAppearance.BorderSize = 0;
            m_importImagesButt.Cursor = Cursors.Hand;
            m_importImagesButt.BackColor = Color.FromArgb(255, 220, 208, 192);
            m_importImagesButt.ForeColor = Color.Black;
            m_importImagesButt.Font = new Font("Arial", m_buttonsPanel.Width * 0.023f);
            m_importImagesButt.Size = new Size(
                (int)((m_buttonsPanel.Width - (4 * m_imgGap) - SystemInformation.VerticalScrollBarWidth) / 3.0f),
                (int)(m_buttonsPanel.Height * 0.8f));
            m_importImagesButt.Location = new Point(m_imgGap, (int)((m_buttonsPanel.Height - m_importImagesButt.Height) / 2.0f));
            m_importImagesButt.Click += new EventHandler(OpenFileDialog);

            // CLEAR ALL BUTTON
            m_clearLibraryButt = new Button();
            m_clearLibraryButt.Text = m_cropperForm.m_langDictionary["Clear Library"];
            m_clearLibraryButt.FlatStyle = FlatStyle.Flat;
            m_clearLibraryButt.FlatAppearance.BorderSize = 0;
            m_clearLibraryButt.Cursor = Cursors.Hand;
            m_clearLibraryButt.BackColor = Color.FromArgb(255, 220, 208, 192);
            m_clearLibraryButt.ForeColor = Color.Black;
            m_clearLibraryButt.Font = m_importImagesButt.Font;
            m_clearLibraryButt.Size = new Size(m_importImagesButt.Width, m_importImagesButt.Height);
            m_clearLibraryButt.Location = new Point(2 * m_imgGap + m_importImagesButt.Width, (int)((m_buttonsPanel.Height - m_clearLibraryButt.Height) / 2.0f));
            m_clearLibraryButt.Enabled = false;
            m_clearLibraryButt.Click += new EventHandler(ClearAll);

            // PROGRESS BAR
            m_progressBar = new ProgressBar();
            m_progressBar.Visible = false;
            m_progressBar.Size = new Size(m_buttonsPanel.Width - m_importImagesButt.Width - m_clearLibraryButt.Width - (4 * m_imgGap), (int)m_clearLibraryButt.Font.Height + 8);
            m_progressBar.Location = new Point(3 * m_imgGap + m_importImagesButt.Width + m_clearLibraryButt.Width, (int)((m_buttonsPanel.Height - m_progressBar.Height) / 2.0f));
            m_progressBar.Minimum = 0;
            m_progressBar.Maximum = 100;
            m_progressBar.Value = 0;
            m_progressBar.Step = 0;

            // IMAGE PANEL
            m_imagesPanel = new Panel();
            m_imagesPanel.AutoScroll = true;
            m_imagesPanel.Size = new Size(
                m_imagesLibrary.Width,
                (3 * m_imgSize) + (4 * m_imgGap));
            m_imagesPanel.Location = new Point(0, m_buttonsPanel.Height);
            m_imagesPanel.BackColor = Color.FromArgb(255, 31, 31, 31);
            m_imagesPanel.Paint += new PaintEventHandler(m_imagesPanel_Paint);

            // INSTRUCTIONS PANEL
            m_instPanel = new Panel();
            m_instPanel.Size = new Size(
                m_imagesLibrary.Width,
                m_imagesLibrary.Height - m_imagesPanel.Height - m_buttonsPanel.Height);
            m_instPanel.Location = new Point(0, m_imagesPanel.Height + m_buttonsPanel.Height);

            // HIDE BUTTON
            m_hideInstButt = new Button();
            m_hideInstButt.Size = new Size(
                m_imagesLibrary.Width,
                (int)(m_imagesLibrary.Height * 0.03f));
            m_hideInstButt.FlatStyle = FlatStyle.Flat;
            m_hideInstButt.FlatAppearance.BorderSize = 0;
            m_hideInstButt.Cursor = Cursors.Hand;
            m_hideInstButt.BackColor = Color.FromArgb(255, 220, 208, 192);
            m_hideInstButt.ForeColor = Color.Black;
            m_hideInstButt.Font = new Font("Arial", m_buttonsPanel.Height * 0.4f);
            m_hideInstButt.Text = "\u25BC";
            m_hideInstButt.Click += new EventHandler(hideInst_Click);

            // INSTRUCTIONS
            Label inst = new Label();
            inst.Padding = new Padding(2 * m_imgGap);
            inst.Size = new Size(m_instPanel.Width, m_instPanel.Height - m_hideInstButt.Height);
            inst.Location = new Point(0, m_hideInstButt.Height);
            inst.BackgroundImage = Image.FromFile(Application.StartupPath + "//Data//Images//Instructions.png");
            inst.BackgroundImageLayout = ImageLayout.Stretch;
            inst.ForeColor = Color.Black;
            inst.Font = new Font("Arial", m_buttonsPanel.Height * 0.38f);
            inst.Text = m_cropperForm.m_langDictionary["User Guide"];
            m_toggleInst = new Timer();
            m_toggleInst.Interval = 1;
            m_toggleInst.Tick += new EventHandler(m_toggleInst_Tick);

            // LOAD IMAGE THREAD
            m_backgroundWorker = new BackgroundWorker();
            m_backgroundWorker.WorkerSupportsCancellation = true;
            m_backgroundWorker.WorkerReportsProgress = true;
            m_backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(UpdateProgressBar);
            m_backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ImportCompleted);
            m_backgroundWorker.DoWork += new DoWorkEventHandler(ImportImages);

            // Add the controls in the right order
            m_cropperForm.Controls.Add(m_imagesLibrary);
            m_imagesLibrary.Controls.Add(m_buttonsPanel);
            m_buttonsPanel.Controls.Add(m_importImagesButt);
            m_buttonsPanel.Controls.Add(m_clearLibraryButt);
            m_buttonsPanel.Controls.Add(m_progressBar);
            m_imagesLibrary.Controls.Add(m_imagesPanel);
            m_instPanel.Controls.Add(m_hideInstButt);
            m_instPanel.Controls.Add(inst);
            m_imagesLibrary.Controls.Add(m_instPanel);

            return;
        }
        #endregion

        #region Events
        private void m_imagesPanel_Paint(object sender, PaintEventArgs e)
        {
            LinearGradientBrush brush = new LinearGradientBrush(
                new PointF(m_imagesPanel.Width - SystemInformation.VerticalScrollBarWidth, m_imagesPanel.Height / 2.0f),
                new PointF(m_imagesPanel.Width, m_imagesPanel.Height / 2.0f),
                Color.FromArgb(255, 78, 78, 78), Color.FromArgb(255, 31, 31, 31));

            e.Graphics.FillRectangle(brush, new Rectangle(
                m_imagesPanel.Width - SystemInformation.VerticalScrollBarWidth, 0,
                m_imagesPanel.Width, m_imagesPanel.Height));
        }

        private void m_buttonsPanel_Paint(object sender, PaintEventArgs e)
        {
            SolidBrush strokeBrush = new SolidBrush(Color.FromArgb(255, 31, 31, 31));

            e.Graphics.DrawRectangle(new Pen(strokeBrush, 2f), new Rectangle(0, 0, m_buttonsPanel.Width, m_buttonsPanel.Height + 1));
        }

        private void DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void DragDrop(object sender, DragEventArgs e)
        {
            // Get the image paths
            string[] imagePaths = (string[])e.Data.GetData(DataFormats.FileDrop);

            // Start importing images to library
            StartImporting(imagePaths, false);
        }

        private void ClearAll(object sender, EventArgs e)
        {
            ClearAll();
        }

        private void DeleteImage(object sender, EventArgs e)
        {
            PictureBox parent = (PictureBox)((Button)sender).Parent;

            for (int i = m_images.IndexOf(parent); i < m_images.Count; i++)
            {
                if (i + 1 < m_images.Count)
                {
                    m_images[i].Image = m_images[i + 1].Image;
                    m_images[i].Name = m_images[i + 1].Name;
                }
            }

            m_imagesPanel.Controls.Remove(m_images[m_images.Count - 1]);
            m_images.RemoveAt(m_images.Count - 1);
            if (m_images.Count == 0)
                m_clearLibraryButt.Enabled = false;
        }

        private void ThumbnailClick(object sender, EventArgs e)
        {
            // Send the image path to the crop panel
            m_cropperForm.m_cropPanel.DisplayImage(((PictureBox)sender).Name);

            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                {
                    m_shift = true;
                }
                m_ctrl = true;
            }
        }

        private void hideInst_Click(object sender, EventArgs e)
        {
            m_toggleInst.Start();
        }

        private void m_toggleInst_Tick(object sender, EventArgs e)
        {
            Point newInstPos;

            // If the user want to hide the instructions panel
            if (m_hideInstButt.Text.Equals("\u25BC"))
            {
                newInstPos = new Point(m_instPanel.Location.X, m_instPanel.Location.Y + m_slideSpeed);
                if (newInstPos.Y + m_hideInstButt.Height > m_imagesLibrary.Height)
                {
                    newInstPos.Y = m_imagesLibrary.Height - m_hideInstButt.Height;
                    m_hideInstButt.Text = "\u25B2";
                    m_toggleInst.Stop();
                }
            }
            // If the user want to show the instructions panel
            else
            {
                newInstPos = new Point(m_instPanel.Location.X, m_instPanel.Location.Y - m_slideSpeed);
                if (newInstPos.Y < m_buttonsPanel.Height + (3 * m_imgSize) + (4 * m_imgGap))
                {
                    newInstPos.Y = m_buttonsPanel.Height + (3 * m_imgSize) + (4 * m_imgGap);
                    m_hideInstButt.Text = "\u25BC";
                    m_toggleInst.Stop();
                }
            }

            m_instPanel.Location = newInstPos;
            m_imagesPanel.Height = m_instPanel.Location.Y - m_buttonsPanel.Height;
        }

        public void OpenFileDialog(object sender, EventArgs e)
        {
            DialogResult result = m_fileDialog.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                // Start importing images to library
                StartImporting(m_fileDialog.FileNames, false);
            }


            return;
        }

        private void ImportImages(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            List<string> imgPaths;
            ImageList imageList;
            int progressValue = 0;


            try
            {
                // Initialize the image paths list
                imgPaths = new List<string>(m_paths);

                // Remove images that already exist in the panel
                for (int i = 0; i < m_images.Count; i++)
                    for (int j = 0; j < imgPaths.Count; j++)
                        if (imgPaths[j].Equals(m_images[i].Name))
                            imgPaths.RemoveAt(j);

                // Create the images list
                imageList = new ImageList();
                imageList.ImageSize = new Size(m_imgSize, m_imgSize);
                imageList.ColorDepth = ColorDepth.Depth32Bit;

                // Create a thumbnail from every image
                for (int i = 0; i < imgPaths.Count; i++)
                {
                    if ((worker.CancellationPending == true))
                    {
                        e.Cancel = true;
                        return;
                    }

                    // Add thumbnail to image list
                    imageList.Images.Add(GetThumbnailImage(imageList.ImageSize.Width, Image.FromFile(imgPaths[i])));

                    // Calculate the progress bar value
                    progressValue = (int)(((i + 1) / (float)imgPaths.Count) * 50);
                    worker.ReportProgress(progressValue);
                }

                // Fill the picture box list
                for (int i = 0; i < imageList.Images.Count; i++)
                {
                    if ((worker.CancellationPending == true))
                    {
                        e.Cancel = true;
                        return;
                    }

                    // Create the delete button
                    Button delete = new Button();
                    delete.BackgroundImageLayout = ImageLayout.Stretch;
                    delete.BackColor = Color.Black;
                    delete.BackgroundImage = new Bitmap(Application.StartupPath + "//Data//Images//X.png");
                    delete.Size = new Size((int)(m_imgSize * 0.2f), (int)(m_imgSize * 0.2f));
                    delete.Click += new EventHandler(DeleteImage);

                    // Set the picture box
                    m_images.Add(new PictureBox());
                    m_images[m_images.Count - 1].Name = imgPaths[i];
                    m_images[m_images.Count - 1].Image = imageList.Images[i];
                    m_images[m_images.Count - 1].BackColor = Color.Black;
                    m_images[m_images.Count - 1].Cursor = Cursors.Hand;
                    m_images[m_images.Count - 1].Size = new Size(m_imgSize, m_imgSize);
                    m_images[m_images.Count - 1].Enabled = false;
                    m_images[m_images.Count - 1].Location = new Point(
                        m_imgGap + ((m_images.Count - 1) % 3) * (m_images[m_images.Count - 1].Width + m_imgGap),
                        m_imgGap + ((m_images.Count - 1) / 3) * (m_images[m_images.Count - 1].Height + m_imgGap));
                    m_images[m_images.Count - 1].Controls.Add(delete);
                    m_images[m_images.Count - 1].Click += new EventHandler(ThumbnailClick);
                    UpdateImagePanel(m_images[m_images.Count - 1]);

                    // Send the progress bar value
                    progressValue = 50 + (int)(((i + 1) / (float)imageList.Images.Count) * 50);
                    worker.ReportProgress(progressValue);
                }

                // Create the result
                object[] result = { 
                                      (bool)e.Argument, // Pass the argument indicating
                                                        // whether the images were imported
                                                        // from a saved file
                                      imgPaths.ToArray() }; // Pass the image paths array
                e.Result = result;

                System.Threading.Thread.Sleep(1000);

            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, m_cropperForm.m_langDictionary["Import Images"], MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                e.Cancel = true;
                return;
            }
        }

        private void UpdateProgressBar(object sender, ProgressChangedEventArgs e)
        {
            m_progressBar.Value = e.ProgressPercentage;
        }

        private void ImportCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Remove progress bar
            m_progressBar.Value = 0;
            m_progressBar.Visible = false;

            // Enable import images button
            m_importImagesButt.Enabled = true;
            m_importImagesButt.Focus();

            // If the image library is not empty
            if (m_imagesPanel.Controls.Count > 0)
            {
                // Enable the clear all button
                m_clearLibraryButt.Enabled = true;

                // Enable all images
                for (int i = 0; i < m_imagesPanel.Controls.Count; i++)
                    m_imagesPanel.Controls[i].Enabled = true;
            }

            // Handle new images:
            // If import was cancelled
            if (e.Cancelled)
                return;
            // If import was not cancelled
            else
            {
                // Store the result variable
                object[] result = (object[])e.Result;

                // If new images we're loaded to library
                if (((string[])result[1]).Length > 0)
                    // If the session is not loaded from a saved file
                    if (!((bool)result[0]))
                        // If all the slots are empty
                        if (m_cropperForm.m_slotsPanel.AllEmpty)
                            // Load the first image from the image list to the current slot
                            m_cropperForm.m_cropPanel.DisplayImage(((string[])result[1])[0]);
            }
        }
        #endregion

        #region Functions
        public void ImportFromFile(string[] paths)
        {
            List<string> existingImages = new List<string>(paths);
            List<string> missingImages = new List<string>();

            // Go through the images paths array
            for (int i = 0; i < existingImages.Count; i++)
            {
                // If image doesnt exist
                if (!File.Exists(existingImages[i]))
                {
                    // Add the path to the missing images array
                    missingImages.Add(existingImages[i]);

                    // Remove the path from the existing images list
                    existingImages.Remove(existingImages[i]);

                    i--;
                }
            }

            // If there are missing images
            if (missingImages.Count > 0)
            {
                // Display error with missing paths
                //string error = "Some images could not be found:";
                string error = m_cropperForm.m_langDictionary["Some images could not be found"] + ":";
                for (int i = 0; i < missingImages.Count; i++)
                    error += "\n" + missingImages[i];

                MessageBox.Show(error, m_cropperForm.m_langDictionary["Import Images"], MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            // Fill an array with the existing images
            string[] newPaths = new string[existingImages.Count];
            for (int i = 0; i < newPaths.Length; i++)
                newPaths[i] = existingImages[i];

            // Clear all previous pictureboxes and paths
            m_imagesPanel.Controls.Clear();
            m_images.Clear();

            // Start importing images to library
            StartImporting(newPaths, true);
        }

        private void StartImporting(string[] paths, bool fromFile)
        {
            m_paths = paths;
            m_progressBar.Visible = true;
            m_importImagesButt.Enabled = false;
            m_clearLibraryButt.Enabled = false;

            m_backgroundWorker.RunWorkerAsync(fromFile);
        }

        private Image GetThumbnailImage(int width, Image img)
        {
            Image thumb = new Bitmap(width, width);
            Image tmp = null;

            if (img.Width < width && img.Height < width)
            {
                using (Graphics g = Graphics.FromImage(thumb))
                {
                    int xoffset = (int)((width - img.Width) / 2);
                    int yoffset = (int)((width - img.Height) / 2);
                    g.DrawImage(img, xoffset, yoffset, img.Width, img.Height);
                }
            }
            else
            {
                Image.GetThumbnailImageAbort myCallback = new
                    Image.GetThumbnailImageAbort(ThumbnailCallback);

                if (img.Width == img.Height)
                {
                    thumb = img.GetThumbnailImage(
                             width, width,
                             myCallback, IntPtr.Zero);
                }
                else
                {
                    int k = 0;
                    int xoffset = 0;
                    int yoffset = 0;

                    if (img.Width < img.Height)
                    {
                        k = (int)(width * img.Width / img.Height);
                        tmp = img.GetThumbnailImage(k, width, myCallback, IntPtr.Zero);
                        xoffset = (int)((width - k) / 2);

                    }

                    if (img.Width > img.Height)
                    {
                        k = (int)(width * img.Height / img.Width);
                        tmp = img.GetThumbnailImage(width, k, myCallback, IntPtr.Zero);
                        yoffset = (int)((width - k) / 2);
                    }

                    using (Graphics g = Graphics.FromImage(thumb))
                    {
                        g.DrawImage(tmp, xoffset, yoffset, tmp.Width, tmp.Height);
                    }
                }
            }

            using (Graphics g = Graphics.FromImage(thumb))
            {
                g.DrawRectangle(Pens.Beige, 0, 0, thumb.Width - 1, thumb.Height - 1);
            }

            return thumb;
        }

        public bool ThumbnailCallback()
        {
            return true;
        }

        private void UpdateImagePanel(PictureBox pb)
        {
            // InvokeRequired required compares the thread ID of the 
            // calling thread to the thread ID of the creating thread. 
            // If these threads are different, it returns true. 
            if (m_imagesPanel.InvokeRequired)
            {
                SetImagePanelCallback d = new SetImagePanelCallback(UpdateImagePanel);

                try
                {
                    m_cropperForm.Invoke(d, new object[] { pb });
                }
                catch
                {
                    return;
                }
            }
            else
            {
                m_imagesPanel.Controls.Add(m_images[m_images.Count - 1]);
            }
        }

        public void ClearAll()
        {
            m_imagesPanel.Controls.Clear();
            m_clearLibraryButt.Enabled = false;
            m_images.Clear();
        }
        #endregion

        #region Properties
        public int Width
        {
            get { return m_imagesLibrary.Width; }
        }

        public string[] ImagesPaths
        {
            get
            {
                m_paths = new string[m_images.Count];
                for (int i = 0; i < m_images.Count; i++)
                    m_paths[i] = m_images[i].Name;
                return m_paths;
            }
        }
        #endregion
    }
}
