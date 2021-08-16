#region Using
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
#endregion

namespace Subli_art_Domino_Cropper
{
    #region SLOT
    struct Slot
    {
        public PictureBox pb;
        public RectangleF sourceRect;
        public Color rectColor;
        public Bitmap bmp;
        public bool approved;
        public Slot(PictureBox pb, RectangleF sourceRect, Color rectColor, Bitmap bmp, bool approved)
        {
            this.pb = pb;
            this.sourceRect = sourceRect;
            this.rectColor = rectColor;
            this.bmp = bmp;
            this.approved = approved;
        }

        public int Width
        {
            get { return pb.Width; }
        }

        public int Height
        {
            get { return pb.Height; }
        }

        public Size Size
        {
            get { return pb.Size; }
        }

        public Point Location
        {
            get { return pb.Location; }
        }

        public Color Color
        {
            get { return rectColor; }
            set { rectColor = value; }
        }

        public bool Approved
        {
            get { return approved; }
            set { approved = value; }
        }

        public void Invalidate()
        {
            pb.Invalidate();
        }
    } 
    #endregion

    public class SlotsPanelClass
    {
        #region Members
        const short SLOT_COUNT = 7;
        int m_gap, m_slotInd;
        CropperForm m_cropperForm;
        Panel m_slotsPanel;
        System.Drawing.Rectangle m_destRect;
        System.Drawing.Font m_font;
        Slot[] m_slots;
        DominoPDFClass m_PDFDocument;
        #endregion

        #region Initialize
        public SlotsPanelClass(CropperForm cropperForm)
        {
            float slotWidth;


            m_slotInd = 0;
            m_cropperForm = cropperForm;

            // Initialize the slots panel
            m_slotsPanel = new Panel();
            m_slotsPanel.BackColor = Color.FromArgb(255, 78, 78, 78);
            m_slotsPanel.Width = m_cropperForm.ClientSize.Width - m_cropperForm.m_imagesLibrary.Width;

            // Set the slots gap
            m_gap = (int)(m_slotsPanel.Width * 0.005f);

            // Set the slot width
            slotWidth = (m_slotsPanel.Width - ((SLOT_COUNT + 2) * m_gap)) / (float)(SLOT_COUNT + 1);
            
            m_slotsPanel.Height = (int)(slotWidth / (float)m_cropperForm.SlotRatio) + (2 * m_gap);
            m_slotsPanel.Location = new Point(
                m_cropperForm.m_imagesLibrary.Width, 
                m_cropperForm.ClientSize.Height - m_slotsPanel.Height);
            m_slotsPanel.Paint += new PaintEventHandler(slotsPanelPaint);

            // Initialize the slots
            m_slots = new Slot[SLOT_COUNT];
            for (int i = 0; i < m_slots.Length; i++)
            {
                m_slots[i] = new Slot();
                m_slots[i].pb = new PictureBox();
                m_slots[i].pb.Size = new Size((int)slotWidth, m_slotsPanel.Height - (2 * m_gap));
                m_slots[i].pb.Location = new Point((int)(((i + 1) * m_gap) + (i * slotWidth)), m_gap);
                m_slots[i].pb.Cursor = Cursors.Hand;
                m_slots[i].pb.BackgroundImage = Properties.Resources.Slot;
                m_slots[i].pb.BackgroundImageLayout = ImageLayout.Stretch;
                m_slots[i].pb.Click += new EventHandler(slotClick);
                m_slots[i].pb.Paint += new PaintEventHandler(slotPaint);

                // Set the defualt crop tool color
                m_slots[i].Color = Color.FromArgb(100, 255, 0, 0);

                // Set the slot to not approved
                m_slots[i].Approved = false;

                // Add the slot picture box
                m_slotsPanel.Controls.Add(m_slots[i].pb);
            }

            m_destRect = new System.Drawing.Rectangle(0, 0, m_slots[0].Width, m_slots[0].Height);
            m_font = new Font("Arial", m_slots[0].Height * 0.08f);

            // Initialize the 'create pdf' button
            Button createPDF = new Button();
            createPDF.Size = m_slots[0].Size;
            createPDF.Location = new Point(m_slots.Last().Location.X + createPDF.Width + m_gap, m_gap);
            createPDF.BackColor = Color.FromArgb(255, 220, 208, 192);
            createPDF.Text = m_cropperForm.m_langDictionary["Create PDF"];
            createPDF.Font = m_font;
            createPDF.FlatStyle = FlatStyle.Flat;
            createPDF.FlatAppearance.BorderSize = 0;
            createPDF.Cursor = Cursors.Hand;
            createPDF.Click += new EventHandler(createPDF_Click);
            m_slotsPanel.Controls.Add(createPDF);
            m_cropperForm.Controls.Add(m_slotsPanel);

            // Initialize the pdf generator
            m_PDFDocument = new DominoPDFClass(m_cropperForm);
        } 
        #endregion

        #region Events
        public void createPDF_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < m_slots.Length; i++)
            {
                if (m_slots[i].bmp == null)
                {
                    MessageBox.Show(m_cropperForm.m_langDictionary["All slots must be full"], m_cropperForm.m_langDictionary["Create PDF"], MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }

            for (int i = 0; i < m_slots.Length; i++)
            {
                if (!m_slots[i].Approved)
                {
                    MessageBox.Show(m_cropperForm.m_langDictionary["All slots must be approved"], m_cropperForm.m_langDictionary["Create PDF"], MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "PDF (*.pdf)|*.pdf";
            saveDialog.FileName = "DominoPDF";

            DialogResult result = saveDialog.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                // Genreate the domino pdf
                m_PDFDocument.StartCreatingPDF(saveDialog.FileName);                
            }

            return;
        }

        private void slotsPanelPaint(object sender, PaintEventArgs e)
        {
            int borderWidth = (int)(m_gap * 0.5f);
            e.Graphics.DrawRectangle(new Pen(new SolidBrush(Color.FromArgb(255, 220, 208, 192)), borderWidth),
                new System.Drawing.Rectangle(m_slots[m_slotInd].Location.X - 1, m_slots[m_slotInd].Location.Y - 1,
                    m_slots[m_slotInd].Width + 1, m_slots[m_slotInd].Height + 1));
        }

        private void slotClick(object sender, EventArgs e)
        {
            m_slotInd = m_slotsPanel.Controls.IndexOf(((PictureBox)sender));
            m_slotsPanel.Invalidate();
            m_cropperForm.m_cropPanel.DisplayImage(m_slots[m_slotInd].bmp, m_slots[m_slotInd].sourceRect);
        }

        private void slotPaint(object sender, PaintEventArgs e)
        {
            int currSlot = m_slotsPanel.Controls.IndexOf(((PictureBox)sender));
            if (m_slots[currSlot].bmp == null)
            {
                string str = m_cropperForm.m_langDictionary["Slot"] + " " + (currSlot + 1);
                SizeF stringSize = e.Graphics.MeasureString(str, m_font);
                e.Graphics.DrawString(
                    str, m_font, new SolidBrush(Color.FromArgb(255, 220, 208, 192)), new PointF(
                        (m_slots[0].Width - stringSize.Width) / 2.0f,
                        (m_slots[0].Height - stringSize.Height) / 2.0f));
                return;
            }

            // Draw the cropped image
            e.Graphics.DrawImage(
                m_slots[currSlot].bmp,
                m_destRect,
                m_slots[currSlot].sourceRect,
                GraphicsUnit.Pixel);

            if (m_slots[currSlot].Approved)
            {
                e.Graphics.FillRectangle(
                    new SolidBrush(Color.FromArgb(100, 0, 0, 0)), 
                    new Rectangle(0, 0, m_slots[currSlot].Width, m_slots[currSlot].Height));

                Bitmap vBmp = new Bitmap(
                    Properties.Resources.V,
                    (int)(m_slots[currSlot].Width * 0.5f), 
                    (int)(m_slots[currSlot].Width * 0.5f));

                e.Graphics.DrawImage(vBmp, (m_slots[currSlot].Width - vBmp.Width) / 2.0f, (m_slots[currSlot].Height - vBmp.Height) / 2.0f);
            }

            // If control is pressed while clicking on
            // images from the side bar, the slot's
            // focus moves to the next slot
            if (m_cropperForm.m_imagesLibrary.m_ctrl)
            {
                m_cropperForm.m_imagesLibrary.m_ctrl = false;

                // If ctrl+shift are pressed while clicking on
                // images from the side bar, the slot's
                // focus moves to the previous slot
                if (m_cropperForm.m_imagesLibrary.m_shift)
                {
                    m_cropperForm.m_imagesLibrary.m_shift = false;
                    m_slotInd--;
                    if (m_slotInd < 0)
                        m_slotInd = m_slots.Length - 1;
                }
                else
                {
                    m_slotInd++;
                    if (m_slotInd > m_slots.Length - 1)
                        m_slotInd = 0;
                }

                m_slotsPanel.Invalidate();
                m_cropperForm.m_cropPanel.DisplayImage(m_slots[m_slotInd].bmp, m_slots[m_slotInd].sourceRect);
            }
        }
        #endregion

        #region Methods
        public void CropToSlot(Bitmap bmp, RectangleF sourceRect)
        {
            m_slots[m_slotInd].bmp = bmp;
            m_slots[m_slotInd].sourceRect = sourceRect;
            m_slots[m_slotInd].Invalidate();
        }

        public void CropToSlot(Bitmap bmp, RectangleF sourceRect, int slotIndex)
        {
            m_slots[slotIndex].bmp = bmp;
            m_slots[slotIndex].sourceRect = sourceRect;
            m_slots[slotIndex].Invalidate();            
        }

        public void DeleteSlot()
        {
            // Set the bmp to null
            m_slots[m_slotInd].bmp = null;

            // Set the default crop tool color
            m_slots[m_slotInd].Color = Color.FromArgb(100, 255, 0, 0);
            m_slots[m_slotInd].Invalidate();
        }

        public void SetSlotColor(Color newColor)
        {
            m_slots[m_slotInd].Color = newColor;
        }

        public Color GetSlotColor()
        {
            return m_slots[m_slotInd].Color;
        }

        public void DisapproveSlot()
        {
            // Set the slot to disapproved
            m_slots[m_slotInd].Approved = false;
        }

        public void ApproveSlot()
        {
            // If the slot is not approved
            if (!m_slots[m_slotInd].Approved)
            {
                // Set the slot to approved
                m_slots[m_slotInd].Approved = true;

                // Invalidate the slot
                m_slots[m_slotInd].Invalidate();
            }

            // Move to the next slot
            NextSlot();
        }

        public void NextSlot()
        {
            m_slotInd++;
            if (m_slotInd > m_slots.Length - 1)
                m_slotInd = 0;
            m_slotsPanel.Invalidate();
            m_cropperForm.m_cropPanel.DisplayImage(m_slots[m_slotInd].bmp, m_slots[m_slotInd].sourceRect);
        }

        public void PrevSlot()
        {
            m_slotInd--;
            if (m_slotInd < 0)
                m_slotInd = m_slots.Length - 1;
            m_slotsPanel.Invalidate();
            m_cropperForm.m_cropPanel.DisplayImage(m_slots[m_slotInd].bmp, m_slots[m_slotInd].sourceRect);
        }

        public void ImportFromFile(RectangleF[] sourceRectangles, Color[] rectanglesColors, Bitmap[] bitmaps, int slotIndex, bool[] approvedSlots)
        {
            for (int i = 0; i < m_slots.Length; i++)
            {
                m_slots[i].sourceRect = sourceRectangles[i];
                m_slots[i].rectColor = rectanglesColors[i];
                m_slots[i].bmp = bitmaps[i];
                m_slots[i].Approved = approvedSlots[i];
                m_slots[i].Invalidate();
            }

            if (AllApproved)
                m_cropperForm.m_cropPanel.ShowArrow();
            else
                m_cropperForm.m_cropPanel.HideArrow();

            if (slotIndex >= 0 && slotIndex < m_slots.Length)
                m_slotInd = slotIndex;
            m_slotsPanel.Invalidate();
        }

        public void Reset()
        {
            for (int i = 0; i < m_slots.Length; i++)
            {
                m_slots[i].pb.BackgroundImage = Properties.Resources.Slot;
                m_slots[i].bmp = null;
                m_slots[i].rectColor = Color.FromArgb(100, 255, 0, 0);
                m_slots[i].approved = false;
            }

            m_slotInd = 0;
            m_slotsPanel.Invalidate();
        }
        #endregion
        
        #region Properties
        public int Height
        {
            get { return m_slotsPanel.Height; }
        }

        public int Width
        {
            get { return m_slotsPanel.Width; }
        }

        public int SlotWidth
        {
            get { return m_slots[0].Width; }
        }

        public int SlotGap
        {
            get { return m_gap; }
        }

        public short SlotCount
        {
            get { return SLOT_COUNT; }
        }

        public RectangleF[] SourceRectangles
        {
            get 
            {
                RectangleF[] sourceRects = new RectangleF[m_slots.Length];
                for (int i = 0; i < m_slots.Length; i++)
                    sourceRects[i] = m_slots[i].sourceRect;
                return sourceRects; 
            }
        }

        public Bitmap[] Bitmaps
        {
            get
            {
                Bitmap[] bitmaps = new Bitmap[m_slots.Length];
                for (int i = 0; i < m_slots.Length; i++)
                    bitmaps[i] = m_slots[i].bmp;
                return bitmaps;
            }
        }

        public Color[] RectanglesColors
        {
            get
            {
                Color[] rectangleColors = new Color[m_slots.Length];
                for (int i = 0; i < m_slots.Length; i++)
                    rectangleColors[i] = m_slots[i].Color;
                return rectangleColors;
            }
        }

        public bool[] ApprovedSlots
        {
            get
            {
                bool[] approvedSlots = new bool[m_slots.Length];
                for (int i = 0; i < m_slots.Length; i++)
                    approvedSlots[i] = m_slots[i].Approved;
                return approvedSlots;
            }
        }

        public int SlotIndex
        {
            get { return m_slotInd; }
        }

        public bool AllEmpty
        {
            get 
            {
                for (int i = 0; i < m_slots.Length; i++)
                    if (m_slots[i].bmp != null)
                        return false;

                return true;
            }
        }

        public bool AllApproved
        {
            get
            {
                for (int i = 0; i < m_slots.Length; i++)
                    if (!m_slots[i].Approved)
                        return false;

                return true;
            }
        }

        public bool IsSlotEmpty(int slotInd)
        {
            if (m_slots[slotInd].bmp == null)
                return true;

            return false;
        }
        #endregion
    }
}
