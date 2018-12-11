#region Using
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Reflection;

using iTextSharp.text;
using iTextSharp.text.pdf;
#endregion

namespace Subli_art_Ludo_Cropper
{
    class LudoPDFClass
    {
        #region Members
        const float BOARD_IMAGE_SIZE = 139.7f, PLAYER_IMAGE_WIDTH = 99.3f;
        string m_filename;
        iTextSharp.text.Image[] m_images;
        CropperForm m_cropperForm;
        Form m_progressBarForm;
        BackgroundWorker m_backgroundWorker;
        ProgressBar m_progressBar;
        Label m_creatingPDFLabel;
        Button 
            m_openPDFButt,
            m_openPDFFolderButt,
            m_cancelButt;
        #endregion

        #region Initialize
        public LudoPDFClass(CropperForm cropperForm)
        {
            m_cropperForm = cropperForm;            

            // Set the filename
            m_filename = "";

            InitializeProgressBarForm();
        }

        private void InitializeProgressBarForm()
        {
            // PROGRESS BAR FORM
            m_progressBarForm = new Form();
            m_progressBarForm.Text = m_cropperForm.m_langDictionary["Create PDF"];
            m_progressBarForm.StartPosition = FormStartPosition.CenterScreen;
            m_progressBarForm.Icon = m_cropperForm.Icon;
            m_progressBarForm.FormBorderStyle = FormBorderStyle.Fixed3D;
            m_progressBarForm.MinimizeBox = false;
            m_progressBarForm.MaximizeBox = false;
            m_progressBarForm.BackColor = Color.FromArgb(255, 78, 78, 78);
            m_progressBarForm.Width = (int)(m_cropperForm.Width * 0.3f);
            m_progressBarForm.Height = (int)(m_progressBarForm.Width * 0.3f);
            m_progressBarForm.Shown += new EventHandler(m_progressBarForm_Shown);
            m_progressBarForm.FormClosing += new FormClosingEventHandler(m_progressBarForm_FormClosing);
                       
            // CREATING PDF LABEL
            m_creatingPDFLabel = new Label();
            m_creatingPDFLabel.Font = new System.Drawing.Font("Arial", m_progressBarForm.ClientSize.Height * 0.07f);
            m_creatingPDFLabel.Width = (int)(m_progressBarForm.ClientSize.Width * 0.9f);
            m_creatingPDFLabel.Height = (int)(m_creatingPDFLabel.Font.Height * 1.1f);
            m_creatingPDFLabel.Location = new Point(
                (int)((m_progressBarForm.ClientSize.Width - m_creatingPDFLabel.Width) / 2.0f),
                (int)(m_progressBarForm.ClientSize.Height * 0.1f));
            m_creatingPDFLabel.ForeColor = Color.FromArgb(255, 220, 208, 192);
            m_progressBarForm.Controls.Add(m_creatingPDFLabel);

            // PROGRESS BAR
            m_progressBar = new ProgressBar();
            m_progressBar.Width = (int)(m_progressBarForm.ClientSize.Width * 0.9f);
            m_progressBar.Height = (int)(m_progressBarForm.ClientSize.Height * 0.15f);
            m_progressBar.Location = new Point(
                (int)((m_progressBarForm.ClientSize.Width - m_progressBar.Width) / 2.0f),
                (int)(m_creatingPDFLabel.Location.Y + m_creatingPDFLabel.Height + (m_progressBarForm.ClientSize.Height * 0.05f)));
            m_progressBar.Minimum = 0;
            m_progressBar.Maximum = 100;
            m_progressBar.Value = 0;
            m_progressBar.Step = 0;
            m_progressBarForm.Controls.Add(m_progressBar);
            
            // OPEN PDF BUTTON
            m_openPDFButt = new Button();
            m_openPDFButt.Text = m_cropperForm.m_langDictionary["Open PDF"];
            m_openPDFButt.Font = new System.Drawing.Font("Arial", m_progressBarForm.ClientSize.Height * 0.08f);
            m_openPDFButt.Width = (int)(m_progressBar.Width * 0.3f);
            m_openPDFButt.Height = (int)(m_openPDFButt.Font.Height * 1.6f);
            m_openPDFButt.Location = new Point(
                m_progressBar.Location.X,
                (int)(m_progressBarForm.ClientSize.Height * 0.9f) - m_openPDFButt.Height);
            m_openPDFButt.Enabled = false;
            m_openPDFButt.BackColor = Color.FromArgb(255, 220, 208, 192);
            m_openPDFButt.FlatStyle = FlatStyle.Flat;
            m_openPDFButt.FlatAppearance.BorderSize = 0;
            m_openPDFButt.Cursor = Cursors.Hand;
            m_openPDFButt.Click += new EventHandler(m_openPDFButt_Click);
            m_progressBarForm.Controls.Add(m_openPDFButt);

            // OPEN PDF FOLDER BUTTON
            m_openPDFFolderButt = new Button();
            m_openPDFFolderButt.Text = m_cropperForm.m_langDictionary["Open PDF Folder"];
            m_openPDFFolderButt.Font = m_openPDFButt.Font;
            m_openPDFFolderButt.Width = (int)(m_progressBar.Width * 0.46f);
            m_openPDFFolderButt.Height = m_openPDFButt.Height;
            m_openPDFFolderButt.Location = new Point(
                m_progressBar.Location.X + m_openPDFButt.Width + (int)(m_progressBar.Width * 0.02f),
                (int)(m_progressBarForm.ClientSize.Height * 0.9f) - m_openPDFFolderButt.Height);
            m_openPDFFolderButt.Enabled = false;
            m_openPDFFolderButt.BackColor = Color.FromArgb(255, 220, 208, 192);
            m_openPDFFolderButt.FlatStyle = FlatStyle.Flat;
            m_openPDFFolderButt.FlatAppearance.BorderSize = 0;
            m_openPDFFolderButt.Cursor = Cursors.Hand;
            m_openPDFFolderButt.Click += new EventHandler(m_openPDFFolderButt_Click);
            m_progressBarForm.Controls.Add(m_openPDFFolderButt);

            // CANCEL BUTTON
            m_cancelButt = new Button();
            m_cancelButt.Text = m_cropperForm.m_langDictionary["Cancel"];
            m_cancelButt.Font = m_openPDFFolderButt.Font;
            m_cancelButt.Width = (int)(m_progressBar.Width * 0.2f);
            m_cancelButt.Height = m_openPDFFolderButt.Height;
            m_cancelButt.Location = new Point(
                m_progressBar.Location.X + m_openPDFButt.Width + m_openPDFFolderButt.Width + (2 * (int)(m_progressBar.Width * 0.02f)),
                (int)(m_progressBarForm.ClientSize.Height * 0.9f) - m_cancelButt.Height);
            m_cancelButt.BackColor = Color.FromArgb(255, 220, 208, 192);
            m_cancelButt.FlatStyle = FlatStyle.Flat;
            m_cancelButt.FlatAppearance.BorderSize = 0;
            m_cancelButt.Cursor = Cursors.Hand;
            m_cancelButt.Click += new EventHandler(cancelButt_Click);
            m_progressBarForm.Controls.Add(m_cancelButt);

            // LOAD IMAGE THREAD
            m_backgroundWorker = new BackgroundWorker();
            m_backgroundWorker.WorkerSupportsCancellation = true;
            m_backgroundWorker.WorkerReportsProgress = true;
            m_backgroundWorker.DoWork += new DoWorkEventHandler(CreatePDF);
            m_backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(UpdateProgressBar);
            m_backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(PDFCreationCompleted);
        }
        #endregion

        #region Events
        private void CreatePDF(object sender, DoWorkEventArgs e)
        {
            int progressValue = 0;
            string 
                outputPDFPath,
                pdfType;
            float prec;
            string[] arguments;
            Bitmap bmpCrop;
            Bitmap[] bitmaps;
            RectangleF[] srcRects;
            BackgroundWorker worker = sender as BackgroundWorker;

                        
            // Save all cropped images
            try
            {
                // Get the arguments
                arguments = (string[])e.Argument;

                // Fill the bitmaps array
                bitmaps = new Bitmap[m_cropperForm.CurrSlotsPanel.Bitmaps.Length];
                for (int i = 0; i < bitmaps.Length; i++)
                    bitmaps[i] = new Bitmap(m_cropperForm.CurrSlotsPanel.Bitmaps[i]);

                // Fill the source rects array
                srcRects = new RectangleF[m_cropperForm.CurrSlotsPanel.SourceRectangles.Length];
                for (int i = 0; i < srcRects.Length; i++)
                {
                    srcRects[i] = new RectangleF(
                        m_cropperForm.CurrSlotsPanel.SourceRectangles[i].X,
                        m_cropperForm.CurrSlotsPanel.SourceRectangles[i].Y,
                        m_cropperForm.CurrSlotsPanel.SourceRectangles[i].Width,
                        m_cropperForm.CurrSlotsPanel.SourceRectangles[i].Height);
                }

                // Get the filename
                outputPDFPath = arguments[0];
                pdfType = arguments[1];

                // Save the bitmaps as jpg files
                for (int i = 0; i < bitmaps.Length; i++)
                {
                    // If the pdf creation is cancelled
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                    bmpCrop = bitmaps[i].Clone(srcRects[i], bitmaps[i].PixelFormat);
                    bmpCrop.Save(Application.StartupPath + "//tmp//slot" + (i + 1) + ".jpg");

                    // Calculate the progress bar value 0% - 33%
                    progressValue = (int)(((i + 1) / (float)bitmaps.Length) * 33);
                    worker.ReportProgress(progressValue);
                }


                // Fill the images array
                m_images = new iTextSharp.text.Image[bitmaps.Length];
                for (int i = 0; i < m_images.Length; i++)
                {
                    // If the pdf creation is cancelled
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                    m_images[i] = iTextSharp.text.Image.GetInstance(Application.StartupPath + "//tmp//slot" + (i + 1) + ".jpg");

                    prec = BOARD_IMAGE_SIZE / m_images[i].Width;
                    m_images[i].ScalePercent(prec * 100);

                    // Calculate the progress bar value 33% - 45%
                    progressValue = 33 + (int)(((i + 1) / (float)m_images.Length) * 12);
                    worker.ReportProgress(progressValue);
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, m_cropperForm.m_langDictionary["Create PDF"], MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                e.Cancel = true;
                return;
            }

            // Create the BOARD pdf
            if (pdfType.Equals(m_cropperForm.m_langDictionary["Board"]))
            {
                if (!CreateBoardPDF(outputPDFPath))
                {
                    e.Cancel = true;
                    return;
                }
            }
            // Create the PLAYERS pdf
            else
            {
                if (!CreatePlayersPDF(outputPDFPath))
                {
                    e.Cancel = true;
                    return;
                }
            }

            // Calculate the progress bar value 33% - 45%
            progressValue = 90;
            worker.ReportProgress(progressValue);

            try
            {
                // Delete all cropped images
                for (int i = 0; i < bitmaps.Length; i++)
                {
                    // If the pdf creation is cancelled
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                    string filePath = Application.StartupPath + "//tmp//slot" + (i + 1) + ".jpg";
                    if (File.Exists(filePath))
                        File.Delete(filePath);

                    // Calculate the progress bar value 90% - 100%
                    progressValue = 90 + (int)(((i + 1) / (float)bitmaps.Length) * 10);
                    worker.ReportProgress(progressValue);
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, m_cropperForm.m_langDictionary["Create PDF"], MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                e.Cancel = true;
                return;
            }
        }

        private void UpdateProgressBar(object sender, ProgressChangedEventArgs e)
        {
            // Update the progress bar value
            m_progressBar.Value = e.ProgressPercentage;

            // Update the label above the progress bar
            m_creatingPDFLabel.Text = m_cropperForm.m_langDictionary["Creating PDF..."] + m_progressBar.Value + "%";
        }

        private void PDFCreationCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                CleanPDFCreation();
            else
            {
                // Change form's buttons and label
                m_creatingPDFLabel.Text = m_cropperForm.m_langDictionary["Finished"];
                m_openPDFButt.Enabled = true;
                m_openPDFFolderButt.Enabled = true;
                m_cancelButt.Enabled = false;

                // If its the first panel
                if (m_cropperForm.CurrSlotsPanel.PanelType == PanelType.First)
                {
                    m_cropperForm.CurrSlotsPanel.ChangeRightButtonMode(RightButtonModes.Next);
                    m_cropperForm.CurrCropPanel.ChangeArrowMode(RightButtonModes.Next);
                }
                else if (m_cropperForm.CurrSlotsPanel.PanelType == PanelType.Last)
                {
                    m_cropperForm.CurrSlotsPanel.ChangeRightButtonMode(RightButtonModes.Exit);
                    m_cropperForm.CurrCropPanel.ChangeArrowMode(RightButtonModes.Exit);
                }
            }
        }

        private void m_openPDFFolderButt_Click(object sender, EventArgs e)
        {
            try
            {
                string pdfDirectory = Path.GetDirectoryName(m_filename);
                if (Directory.Exists(pdfDirectory))
                {
                    System.Diagnostics.Process prc = new System.Diagnostics.Process();
                    prc.StartInfo.FileName = pdfDirectory;
                    prc.Start();
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, m_cropperForm.m_langDictionary["Create PDF"], MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void m_openPDFButt_Click(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists(m_filename))
                {
                    System.Diagnostics.Process prc = new System.Diagnostics.Process();
                    prc.StartInfo.FileName = m_filename;
                    prc.Start();
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, m_cropperForm.m_langDictionary["Create PDF"], MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void cancelButt_Click(object sender, EventArgs e)
        {
            m_progressBarForm.Close();
        }

        private void m_progressBarForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Cancel pdf creation thread
            m_backgroundWorker.CancelAsync();
        }

        private void m_progressBarForm_Shown(object sender, EventArgs e)
        {
            string[] arguments = { m_filename, m_cropperForm.CurrCropPanel.Name };

            // Start the creating PDF thread
            m_backgroundWorker.RunWorkerAsync(arguments);
        }
        #endregion

        #region Methods
        public void StartCreatingPDF(string filename)
        {
            // Save the filename
            m_filename = filename;

            // Change form's buttons and label
            m_creatingPDFLabel.Text = m_cropperForm.m_langDictionary["Creating PDF..."];
            m_openPDFButt.Enabled = false;
            m_openPDFFolderButt.Enabled = false;
            m_cancelButt.Enabled = true;

            // Zero the progress bar value
            m_progressBar.Value = 0;

            // Show the progress bar dialog
            m_progressBarForm.ShowDialog();
        }   

        private void InvokeFunction(string methodName)
        {
            // Invoke the method by name
            MethodInfo method = this.GetType().GetMethod(methodName);
            method.Invoke(this, null);
        }

        private void CleanPDFCreation()
        {
            try
            {
                // Delete the bitmaps saved for the PDF
                for (int i = 0; i < 7; i++)
                {
                    string filePath = Application.StartupPath + "//tmp//slot" + (i + 1) + ".jpg";
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }

                // Delete the PDF created
                if (File.Exists(m_filename))
                    File.Delete(m_filename);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, m_cropperForm.m_langDictionary["Create PDF"], MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        
        private bool CreateBoardPDF(string outputPDFPath)
        {
            float prec;
            iTextSharp.text.Image boardR, boardL;
            Document outputDocument = null;
            PdfWriter pdfWriter = null;
            PdfReader templatePDF = null;
            PdfContentByte contentByte = null;

            try
            {
                templatePDF = new PdfReader(Properties.Resources.Ludo_Board);

                // Create the RIGHT board image
                boardR = iTextSharp.text.Image.GetInstance(Properties.Resources.Ludo_Board_R, ImageFormat.Png);
                boardR.SetAbsolutePosition(0, 0);
                prec = templatePDF.GetPageSize(1).Width / boardR.Width;
                boardR.ScalePercent(prec * 100);

                // Create the LEFT board image
                boardL = iTextSharp.text.Image.GetInstance(Properties.Resources.Ludo_Board_L, ImageFormat.Png);
                boardL.SetAbsolutePosition(0, 0);
                prec = templatePDF.GetPageSize(1).Width / boardL.Width;
                boardL.ScalePercent(prec * 100);

                outputDocument = new iTextSharp.text.Document(templatePDF.GetPageSize(1));
                outputDocument.SetMargins(0f, 0f, 0f, 0f);

                // Create the pdf writer
                pdfWriter = PdfWriter.GetInstance(outputDocument, new FileStream(outputPDFPath, FileMode.Create, FileAccess.Write));

                // Open the pdf document
                outputDocument.Open();

                // Draw the corner dots
                contentByte = pdfWriter.DirectContent;

                // Add the right board image
                contentByte.AddImage(boardR);

                // GREEN CORNER
                // Rotate image
                m_images[0].RotationDegrees = 135;
                m_images[0].Rotate();

                // Add image
                m_images[0].SetAbsolutePosition(208.6f, 572.9f);
                contentByte.AddImage(m_images[0]);

                // YELLOW CORNER
                // Scale image
                prec = BOARD_IMAGE_SIZE / m_images[1].Width;
                m_images[1].ScalePercent(prec * 100);

                // Rotate image before horizontal drawing
                m_images[1].RotationDegrees = 45;
                m_images[1].Rotate();

                // Add image
                m_images[1].SetAbsolutePosition(208.6f, 68.5f);
                contentByte.AddImage(m_images[1]);

                // PAGE 2
                // Add new page
                outputDocument.NewPage();

                // Add the right board image
                contentByte.AddImage(boardL);

                // RED CORNER
                // Scale image
                prec = BOARD_IMAGE_SIZE / m_images[2].Width;
                m_images[2].ScalePercent(prec * 100);

                // Rotate image before horizontal drawing
                m_images[2].RotationDegrees = 225;
                m_images[2].Rotate();

                // Add image
                m_images[2].SetAbsolutePosition(223.0f, 572.9f);
                contentByte.AddImage(m_images[2]);

                // BLUE CORNER
                // Scale image
                prec = BOARD_IMAGE_SIZE / m_images[3].Width;
                m_images[3].ScalePercent(prec * 100);

                // Rotate image before horizontal drawing
                m_images[3].RotationDegrees = 315;
                m_images[3].Rotate();

                // Add image
                m_images[3].SetAbsolutePosition(223.0f, 68.5f);
                contentByte.AddImage(m_images[3]);

                // Close the document
                outputDocument.Close();
            }
            catch (iTextSharp.text.DocumentException err)
            {
                MessageBox.Show(err.Message, m_cropperForm.m_langDictionary["Create PDF"], MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
            finally
            {
                // Clean up 
                outputDocument = null;
            }


            return true;
        }

        private bool CreatePlayersPDF(string outputPDFPath)
        {
            int bmpIndex;
            float prec, radius;
            Document outputDocument = null;
            PdfWriter pdfWriter = null;
            iTextSharp.text.Image players;
            PdfReader templatePDF = null;
            PdfContentByte contentByte = null;

            try
            {
                templatePDF = new PdfReader(Properties.Resources.Ludo_Players);

                players = iTextSharp.text.Image.GetInstance(Properties.Resources.Ludo_Players_PNG, ImageFormat.Png);
                players.SetAbsolutePosition(0, 0);
                prec = templatePDF.GetPageSize(1).Width / players.Width;
                players.ScalePercent(prec * 100);

                outputDocument = new iTextSharp.text.Document(templatePDF.GetPageSize(1));
                outputDocument.SetMargins(0f, 0f, 0f, 0f);

                // Create the pdf writer
                pdfWriter = PdfWriter.GetInstance(outputDocument, new FileStream(outputPDFPath, FileMode.Create, FileAccess.Write));

                // Open the pdf document
                outputDocument.Open();

                // Draw the corner dots
                contentByte = pdfWriter.DirectContent;

                // Add the right board image
                //contentByte.AddImage(players);

                for (int i = 0; i < 2; i++)
                {
                    // Place players images
                    for (int j = 0; j < 4 * m_images.Length; j++)
                    {
                        bmpIndex = j % m_images.Length;

                        // Scale image
                        prec = PLAYER_IMAGE_WIDTH / m_images[bmpIndex].Width;
                        m_images[bmpIndex].ScalePercent(prec * 100);

                        // Add image
                        m_images[bmpIndex].SetAbsolutePosition(
                            150.8f + (bmpIndex * (m_images[bmpIndex].ScaledWidth)),
                            593.8f - ((j / m_images.Length) * (m_images[bmpIndex].ScaledHeight)));
                        contentByte.AddImage(m_images[bmpIndex]);

                    }

                    // Set the dots radius
                    radius = 5.5f / 2.0f;

                    // TOP LEFT
                    contentByte.Circle(197.8f + radius, 749.5f + radius, radius);
                    contentByte.Fill();

                    // TOP RIGHT
                    contentByte.Circle(495.5f + radius, 749.5f + radius, radius);
                    contentByte.Fill();

                    // BOTTOM LEFT
                    contentByte.Circle(150.6f + radius, 121.2f + radius, radius);
                    contentByte.Fill();

                    // BOTTOM RIGHT
                    contentByte.Circle(542.5f + radius, 121.2f + radius, radius);
                    contentByte.Fill();

                    if (i + 1 < 2)
                        outputDocument.NewPage();
                }
                
                // Close the document
                outputDocument.Close();
            }
            catch (iTextSharp.text.DocumentException err)
            {
                MessageBox.Show(err.Message, m_cropperForm.m_langDictionary["Create PDF"], MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
            finally
            {
                // Clean up 
                outputDocument = null;
            }


            return true;
        }
        #endregion
    }
}
