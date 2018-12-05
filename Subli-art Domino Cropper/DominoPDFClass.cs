#region Using
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Reflection;

using iTextSharp.text.pdf;
#endregion

namespace Subli_art_Domino_Cropper
{
    class DominoPDFClass
    {
        #region Members
        iTextSharp.text.Document m_document;
        iTextSharp.text.Image[] m_images;
        iTextSharp.text.Rectangle m_pageSize;
        PdfContentByte m_cb;
        const float
            imgWidth = 74.4f, imgHeight = 89.5f,
            vertTilesMarginLeft = 25f, horizTilesMarginLeft = 422.9f,
            vertTilesMarginBottom = 775f, horizTilesMarginBottom = 773.8f,
            vertTilesHorizGap = 9.6f, vertTilesVertGap = 3.4f, horizTilesVertGap = 5.5f,
            m_lineWidth = 2f;

        string m_filename;
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
        public DominoPDFClass(CropperForm cropperForm)
        {
            m_cropperForm = cropperForm;
            
            // Create a pdf reader
            PdfReader pdfReader = new PdfReader(Properties.Resources.Domino_Pattern);

            // Get the pattern pdf file dimensions
            m_pageSize = new iTextSharp.text.Rectangle(0, 0);
            m_pageSize = pdfReader.GetPageSize(1);

            // Set the document var to null
            m_document = null;
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
            m_openPDFButt.Width = (int)(m_progressBar.Width * 0.3f);
            m_openPDFButt.Height = (int)(m_progressBar.Height * 1.4f);
            m_openPDFButt.Font = new System.Drawing.Font("Arial", m_openPDFButt.Width * 0.07f);
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
            string filename;
            PdfWriter pdfWriter = null;
            Bitmap bmpCrop;
            Bitmap[] bitmaps;
            RectangleF[] srcRects;
            BackgroundWorker worker = sender as BackgroundWorker;


            // Get the bitmaps from the slots panel
            bitmaps = new Bitmap[m_cropperForm.m_slotsPanel.Bitmaps.Length];
            for (int i = 0; i < bitmaps.Length; i++)
                bitmaps[i] = new Bitmap(m_cropperForm.m_slotsPanel.Bitmaps[i]);

            // Get the rectangles from the slots panel
            srcRects = new RectangleF[m_cropperForm.m_slotsPanel.SourceRectangles.Length];
            for (int i = 0; i < bitmaps.Length; i++)
                srcRects[i] = new RectangleF(
                    m_cropperForm.m_slotsPanel.SourceRectangles[i].X,
                    m_cropperForm.m_slotsPanel.SourceRectangles[i].Y,
                    m_cropperForm.m_slotsPanel.SourceRectangles[i].Width,
                    m_cropperForm.m_slotsPanel.SourceRectangles[i].Height);

            // Get the filename
            filename = (string)e.Argument;

            // Save all cropped images
            try
            {
                for (int i = 0; i < m_cropperForm.m_slotsPanel.SlotCount; i++)
                {
                    bmpCrop = bitmaps[i].Clone(srcRects[i], bitmaps[i].PixelFormat);
                    bmpCrop.Save(Application.StartupPath + "//tmp//PDF//Domino_PDF//Slot" + (i + 1) + ".jpg");

                    // Calculate the progress bar value
                    progressValue = (int)(((i + 1) / 7.0f) * 33);
                    worker.ReportProgress(progressValue);
                }

                // If the pdf creation is cancelled
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, m_cropperForm.m_langDictionary["Create PDF"], MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                e.Cancel = true;
                return;
            }

            // Create the PDF document
            try
            {
                // Initialize the PDF document 
                m_document = new iTextSharp.text.Document(m_pageSize);
                m_document.SetMargins(0f, 0f, 0f, 0f);

                // Create the pdf writer
                pdfWriter = PdfWriter.GetInstance(m_document, new FileStream(filename, FileMode.Create));

                // Open the pdf document
                m_document.Open();

                // Draw the corner dots
                m_cb = pdfWriter.DirectContent;
                m_cb.SetCMYKColorFill(0, 0, 0, 255);

                // BOTTOM LEFT
                float radius = 3.8f / 2.0f;
                m_cb.Circle(18f + radius, 165.1f + radius, radius);
                m_cb.Fill();

                // TOP LEFT
                m_cb.Circle(16.8f + radius, 622.3f + radius, radius);
                m_cb.Fill();

                // TOP RIGHT
                m_cb.Circle(572.2f + radius, 678f + radius, radius);
                m_cb.Fill();

                // BOTTOM RIGHT
                m_cb.Circle(574.6f + radius, 109.4f + radius, radius);
                m_cb.Fill();

                // Fill the images array
                m_images = new iTextSharp.text.Image[m_cropperForm.m_slotsPanel.SlotCount];
                for (int i = 0; i < m_images.Length; i++)
                {
                    // If the pdf creation is cancelled
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                    m_images[i] = iTextSharp.text.Image.GetInstance(Application.StartupPath + "//tmp//PDF//Domino_PDF//Slot" + (i + 1) + ".jpg");

                    float prec = imgWidth / m_images[i].Width;
                    m_images[i].ScalePercent(prec * 100);
                    m_images[i].RotationDegrees = 90;
                    m_images[i].Rotate();

                    // Calculate the progress bar value
                    progressValue = 33 + (int)(((i + 1) / (float)m_images.Length) * 12);
                    worker.ReportProgress(progressValue);
                }

                // Draw the slots to te pdf
                for (int i = 0; i < m_cropperForm.m_slotsPanel.SlotCount; i++)
                {
                    // If the pdf creation is cancelled
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                    InvokeFunction("Slot" + (i + 1));

                    // Calculate the progress bar value
                    progressValue += 3;
                    worker.ReportProgress(progressValue);
                }

                // Tiles 1-20 Lines
                for (int i = 0; i < 20; i++)
                {
                    // If the pdf creation is cancelled
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                    m_cb.Rectangle(
                        vertTilesMarginLeft + (i % 4) * (imgHeight + vertTilesHorizGap),
                        vertTilesMarginBottom - ((2 * (i / 4) + 1) * imgWidth) - ((i / 4) * vertTilesVertGap) - (m_lineWidth / 2.0f),
                        imgHeight + 0.5f, m_lineWidth);
                    m_cb.Fill();

                    // Calculate the progress bar value
                    progressValue = 66 + (int)(((i + 1) / 20.0f) * 12);
                    worker.ReportProgress(progressValue);
                }

                // Tiles 20-28 Lines
                for (int i = 0; i < 8; i++)
                {
                    // If the pdf creation is cancelled
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                    m_cb.Rectangle(
                        horizTilesMarginLeft + imgWidth - (m_lineWidth / 2.0f),
                        horizTilesMarginBottom - ((i + 1) * imgHeight) - (i * horizTilesVertGap),
                        m_lineWidth, imgHeight + 0.5f);
                    m_cb.Fill();

                    // Calculate the progress bar value
                    progressValue = 78 + (int)(((i + 1) / 8.0f) * 12);
                    worker.ReportProgress(progressValue);
                }
            }
            catch (iTextSharp.text.DocumentException err)
            {
                MessageBox.Show(err.Message, m_cropperForm.m_langDictionary["Create PDF"], MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                e.Cancel = true;
                return;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, m_cropperForm.m_langDictionary["Create PDF"], MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                e.Cancel = true;
                return;
            }
            finally
            {
                // Clean up 
                m_document.Close();
                m_document = null;
            }

            // Delete all cropped images
            try
            {
                for (int i = 0; i < m_cropperForm.m_slotsPanel.SlotCount; i++)
                {
                    string filePath = Application.StartupPath + "//tmp//PDF//Domino_PDF//Slot" + (i + 1) + ".jpg";
                    if (File.Exists(filePath))
                        File.Delete(filePath);

                    // Calculate the progress bar value
                    progressValue = 90 + (int)(((i + 1) / 7.0f) * 10);
                    worker.ReportProgress(progressValue);
                }
                
                // If the pdf creation is cancelled
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
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
            // Start the creating PDF thread
            m_backgroundWorker.RunWorkerAsync(m_filename);
        }
        #endregion

        #region Functions
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
                    string filePath = Application.StartupPath + "//tmp//PDF//Domino_PDF//Slot" + (i + 1) + ".jpg";
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

        public void Slot1()
        {
            // Tile 1 R
            m_images[0].SetAbsolutePosition(
                vertTilesMarginLeft, 
                vertTilesMarginBottom - imgWidth);
            m_document.Add(m_images[0]);
            
            // Tiles 1-4 L
            for (int i = 0; i < 4; i++)
            {
                m_images[0].SetAbsolutePosition(
                    vertTilesMarginLeft + (i * vertTilesHorizGap) +(i * imgHeight),
                    vertTilesMarginBottom - (2 * imgWidth));
                m_document.Add(m_images[0]);
            }

            // Tile 5-7 L
            for (int i = 0; i < 3; i++)
            {
                m_images[0].SetAbsolutePosition(
                    vertTilesMarginLeft + (i * vertTilesHorizGap) + (i * imgHeight),
                    vertTilesMarginBottom - (4 * imgWidth) - vertTilesVertGap);
                m_document.Add(m_images[0]);
            }
        }

        public void Slot2()
        {
            // Tile 2 R
            m_images[1].SetAbsolutePosition(vertTilesMarginLeft + imgHeight + vertTilesHorizGap, vertTilesMarginBottom - imgWidth);
            m_document.Add(m_images[1]);

            // Tile 8 R
            m_images[1].SetAbsolutePosition(
                vertTilesMarginLeft + (3 * imgHeight) + (3 * vertTilesHorizGap) ,
                vertTilesMarginBottom - (3 * imgWidth) - vertTilesVertGap);
            m_document.Add(m_images[1]);

            // Tile 8 L
            m_images[1].SetAbsolutePosition(
                vertTilesMarginLeft + (3 * imgHeight) + (3 * vertTilesHorizGap),
                vertTilesMarginBottom - (4 * imgWidth) - vertTilesVertGap);
            m_document.Add(m_images[1]);

            // Tiles 9-12 L
            for (int i = 0; i < 4; i++)
            {
                m_images[1].SetAbsolutePosition(
                    vertTilesMarginLeft + (i * imgHeight) + (i * vertTilesHorizGap),
                    vertTilesMarginBottom - (6 * imgWidth) - (2 * vertTilesVertGap));
                m_document.Add(m_images[1]);
            }

            // Tiles 13 L
            m_images[1].SetAbsolutePosition(
                    vertTilesMarginLeft,
                    vertTilesMarginBottom - (8 * imgWidth) - (3 * vertTilesVertGap));
            m_document.Add(m_images[1]);
        }

        public void Slot3()
        {
            // Tile 3 R
            m_images[2].SetAbsolutePosition(
                vertTilesMarginLeft + (2 * imgHeight) + (2 * vertTilesHorizGap), 
                vertTilesMarginBottom - imgWidth);
            m_document.Add(m_images[2]);

            // Tile 9 R
            m_images[2].SetAbsolutePosition(vertTilesMarginLeft, vertTilesMarginBottom - (5 * imgWidth) - (2 * vertTilesVertGap));
            m_document.Add(m_images[2]);

            // Tile 14 R
            m_images[2].SetAbsolutePosition(
                vertTilesMarginLeft + imgHeight + vertTilesHorizGap, 
                vertTilesMarginBottom - (7 * imgWidth) - (3 * vertTilesVertGap));
            m_document.Add(m_images[2]);

            // Tile 14-16 L
            for (int i = 1; i <= 3; i++)
            {
                m_images[2].SetAbsolutePosition(
                    vertTilesMarginLeft + (i * imgHeight) + (i * vertTilesHorizGap),
                    vertTilesMarginBottom - (8 * imgWidth) - (3 * vertTilesVertGap));
                m_document.Add(m_images[2]);
            }

            // Tile 17-18 L
            for (int i = 0; i < 2; i++)
            {
                m_images[2].SetAbsolutePosition(
                    vertTilesMarginLeft + (i * imgHeight) + (i * vertTilesHorizGap),
                    vertTilesMarginBottom - (10 * imgWidth) - (4 * vertTilesVertGap));
                m_document.Add(m_images[2]);
            }
        }

        public void Slot4()
        {
            // Tile 4 R
            m_images[3].SetAbsolutePosition(
                vertTilesMarginLeft + (3 * imgHeight) + (3 * vertTilesHorizGap),
                vertTilesMarginBottom - imgWidth);
            m_document.Add(m_images[3]);

            // Tile 10 R
            m_images[3].SetAbsolutePosition(
                vertTilesMarginLeft + imgHeight + vertTilesHorizGap,
                vertTilesMarginBottom - (5 * imgWidth) - (2 * vertTilesVertGap));
            m_document.Add(m_images[3]);

            // Tile 15 R
            m_images[3].SetAbsolutePosition(
                vertTilesMarginLeft + (2 * imgHeight) + (2 * vertTilesHorizGap),
                vertTilesMarginBottom - (7 * imgWidth) - (3 * vertTilesVertGap));
            m_document.Add(m_images[3]);

            // Tile 19 R
            m_images[3].SetAbsolutePosition(
                vertTilesMarginLeft + (2 * imgHeight) + (2 * vertTilesHorizGap),
                vertTilesMarginBottom - (9 * imgWidth) - (4 * vertTilesVertGap));
            m_document.Add(m_images[3]);

            // Tile 19-20 L
            for (int i = 2; i < 4; i++)
            {
                m_images[3].SetAbsolutePosition(
                    vertTilesMarginLeft + (i * imgHeight) + (i * vertTilesHorizGap),
                    vertTilesMarginBottom - (10 * imgWidth) - (4 * vertTilesVertGap));
                m_document.Add(m_images[3]);
            }

            // Rotate image before horizontal drawing
            m_images[3].RotationDegrees = 0;
            m_images[3].Rotate();

            // Tile 21-22 L
            for (int i = 1; i <= 2; i++)
            {
                m_images[3].SetAbsolutePosition(
                    horizTilesMarginLeft,
                    horizTilesMarginBottom - (i * imgHeight) - ((i - 1) * horizTilesVertGap));
                m_document.Add(m_images[3]);
            }
        }

        public void Slot5()
        {
            // Tile 5 R
            m_images[4].SetAbsolutePosition(
                vertTilesMarginLeft,
                vertTilesMarginBottom - (3 * imgWidth) - vertTilesVertGap);
            m_document.Add(m_images[4]);

            // Tile 11 R
            m_images[4].SetAbsolutePosition(
                vertTilesMarginLeft + (2 * imgHeight) + (2 * vertTilesHorizGap),
                vertTilesMarginBottom - (5 * imgWidth) - (2 * vertTilesVertGap));
            m_document.Add(m_images[4]);

            // Tile 16 R
            m_images[4].SetAbsolutePosition(
                vertTilesMarginLeft + (3 * imgHeight) + (3 * vertTilesHorizGap),
                vertTilesMarginBottom - (7 * imgWidth) - (3 * vertTilesVertGap));
            m_document.Add(m_images[4]);

            // Tile 20 R
            m_images[4].SetAbsolutePosition(
                vertTilesMarginLeft + (3 * imgHeight) + (3 * vertTilesHorizGap),
                vertTilesMarginBottom - (9 * imgWidth) - (4 * vertTilesVertGap));
            m_document.Add(m_images[4]);

            // Rotate image before horizontal drawing
            m_images[4].RotationDegrees = 0;
            m_images[4].Rotate();

            // Tile 23 R
            m_images[4].SetAbsolutePosition(
                    horizTilesMarginLeft + imgWidth,
                    horizTilesMarginBottom - (3 * imgHeight) - (2 * horizTilesVertGap));
            m_document.Add(m_images[4]);
            
            // Tile 23-25 L
            for (int i = 3; i <= 5; i++)
            {
                m_images[4].SetAbsolutePosition(
                    horizTilesMarginLeft,
                    horizTilesMarginBottom - (i * imgHeight) - ((i - 1) * horizTilesVertGap));
                m_document.Add(m_images[4]);
            }
        }

        public void Slot6()
        {
            // Tile 6 R
            m_images[5].SetAbsolutePosition(
                vertTilesMarginLeft + imgHeight + vertTilesHorizGap,
                vertTilesMarginBottom - (3 * imgWidth) - vertTilesVertGap);
            m_document.Add(m_images[5]);

            // Tile 12 R
            m_images[5].SetAbsolutePosition(
                vertTilesMarginLeft + (3 * imgHeight) + (3 * vertTilesHorizGap),
                vertTilesMarginBottom - (5 * imgWidth) - (2 * vertTilesVertGap));
            m_document.Add(m_images[5]);

            // Tile 17 R
            m_images[5].SetAbsolutePosition(
                vertTilesMarginLeft,
                vertTilesMarginBottom - (9 * imgWidth) - (4 * vertTilesVertGap));
            m_document.Add(m_images[5]);

            // Rotate image before horizontal drawing
            m_images[5].RotationDegrees = 0;
            m_images[5].Rotate();

            // Tile 21 R
            m_images[5].SetAbsolutePosition(
                    horizTilesMarginLeft + imgWidth,
                    horizTilesMarginBottom - imgHeight);
            m_document.Add(m_images[5]);

            // Tile 24 R
            m_images[5].SetAbsolutePosition(
                    horizTilesMarginLeft + imgWidth,
                    horizTilesMarginBottom - (4 * imgHeight) - (3 * horizTilesVertGap));
            m_document.Add(m_images[5]);

            // Tile 26 R
            m_images[5].SetAbsolutePosition(
                    horizTilesMarginLeft + imgWidth,
                    horizTilesMarginBottom - (6 * imgHeight) - (5 * horizTilesVertGap));
            m_document.Add(m_images[5]);

            // Tile 26-27 L
            for (int i = 6; i <= 7; i++)
            {
                m_images[5].SetAbsolutePosition(
                    horizTilesMarginLeft,
                    horizTilesMarginBottom - (i * imgHeight) - ((i - 1) * horizTilesVertGap));
                m_document.Add(m_images[5]);
            }
        }

        public void Slot7()
        {
            // Tile 7 R
            m_images[6].SetAbsolutePosition(
                vertTilesMarginLeft + (2 * imgHeight) + (2 * vertTilesHorizGap),
                vertTilesMarginBottom - (3 * imgWidth) - vertTilesVertGap);
            m_document.Add(m_images[6]);

            // Tile 13 R
            m_images[6].SetAbsolutePosition(
                vertTilesMarginLeft,
                vertTilesMarginBottom - (7 * imgWidth) - (3 * vertTilesVertGap));
            m_document.Add(m_images[6]);

            // Tile 18 R
            m_images[6].SetAbsolutePosition(
                vertTilesMarginLeft + imgHeight + vertTilesHorizGap,
                vertTilesMarginBottom - (9 * imgWidth) - (4 * vertTilesVertGap));
            m_document.Add(m_images[6]);

            // Rotate image before horizontal drawing
            m_images[6].RotationDegrees = 0;
            m_images[6].Rotate();

            // Tile 22 R
            m_images[6].SetAbsolutePosition(
                    horizTilesMarginLeft + imgWidth,
                    horizTilesMarginBottom - (2 * imgHeight) - horizTilesVertGap);            
            m_document.Add(m_images[6]);

            // Tile 25 R
            m_images[6].SetAbsolutePosition(
                    horizTilesMarginLeft + imgWidth,
                    horizTilesMarginBottom - (5 * imgHeight) - (4 * horizTilesVertGap));
            m_document.Add(m_images[6]);

            // Tile 27-28 R
            for (int i = 7; i <= 8; i++)
            {
                m_images[6].SetAbsolutePosition(
                    horizTilesMarginLeft + imgWidth,
                    horizTilesMarginBottom - (i * imgHeight) - ((i - 1) * horizTilesVertGap));
                m_document.Add(m_images[6]);
            }

            // Tile 28 L
            m_images[6].SetAbsolutePosition(
                    horizTilesMarginLeft,
                    horizTilesMarginBottom - (8 * imgHeight) - (7 * horizTilesVertGap));
            m_document.Add(m_images[6]);
        }
        #endregion
    }
}
