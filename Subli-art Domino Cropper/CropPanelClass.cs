#region Using
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
#endregion

namespace Subli_art_Domino_Cropper
{
    enum Direction { N, NE, E, SE, S, SW, W, NW, CENTER }
    enum CropAction { DRAG, SCALE, CREATE, NONE }
    public class CropPanelClass
    {
        #region Members
        int m_scaleRadius;
        float m_slotRatio;
        CropperForm m_cropperForm;
        Panel m_cropPanel;
        PictureBox
            m_cropPb,
            m_arrowPb;
        Button
            m_deleteBut,
            m_CWButt, m_CCWButt,
            m_colorBut,
            m_approveCrop;
        PointF
            m_prevMouse,
            m_newLocation,
            m_arrowPos;
        RectangleF m_cropRect;
        CropAction m_cropAction;
        Direction m_scaleDir;
        Bitmap
            m_currImage,
            m_logo,
            m_arrowBmp;
        Size m_logoSize;
        Timer m_arrowTimer;
        #endregion

        #region Initialize
        public CropPanelClass(CropperForm cropperForm)
        {
            m_cropperForm = cropperForm;
            m_slotRatio = m_cropperForm.SlotRatio;

            // Initialize the crop panel
            m_cropPanel = new Panel();
            m_cropPanel.Width = m_cropperForm.ClientSize.Width - m_cropperForm.m_imagesLibrary.Width;
            m_cropPanel.Height = m_cropperForm.ClientSize.Height - m_cropperForm.m_menuStrip.Height - m_cropperForm.m_slotsPanel.Height;
            m_cropPanel.Location = new Point(m_cropperForm.m_imagesLibrary.Width, m_cropperForm.m_menuStrip.Height);
            m_cropPanel.BackgroundImageLayout = ImageLayout.Center;
            m_cropPanel.BackgroundImageChanged += new EventHandler(m_cropPanel_BackgroundImageChanged);
            m_cropPanel.Paint += new PaintEventHandler(m_cropPanel_Paint);
            m_cropperForm.Controls.Add(m_cropPanel);
            
            // Create the arrow bitmap
            m_arrowBmp = new Bitmap(
                Properties.Resources.Arrow,
                m_cropperForm.m_slotsPanel.SlotWidth, m_cropperForm.m_slotsPanel.SlotWidth);

            // Initialize the arrow position
            m_arrowPos = new Point(0, 0);

            // Create the arrow picture box
            m_arrowPb = new PictureBox();
            m_arrowPb.Size = new Size(
                m_arrowBmp.Width,
                (int)(m_arrowBmp.Width * 1.2f));
            m_arrowPb.Location = new Point(
                ((m_cropperForm.m_slotsPanel.SlotCount + 1) * m_cropperForm.m_slotsPanel.SlotGap) + (m_cropperForm.m_slotsPanel.SlotCount * m_cropperForm.m_slotsPanel.SlotWidth),
                m_cropPanel.Height - m_arrowPb.Height);
            m_arrowPb.Image = m_arrowBmp;
            m_arrowPb.Tag = true; // Means the arrow goes down at first
            m_arrowPb.BackColor = Color.Transparent;
            m_arrowPb.Visible = false;
            m_cropPanel.Controls.Add(m_arrowPb);

            // Create the delete button
            m_deleteBut = new Button();
            m_deleteBut.BackgroundImageLayout = ImageLayout.Stretch;
            m_deleteBut.BackColor = Color.Black;
            m_deleteBut.BackgroundImage = Properties.Resources.X;
            m_deleteBut.Size = new Size((int)(m_cropPanel.Width * 0.03f), (int)(m_cropPanel.Width * 0.03f));
            m_deleteBut.Cursor = Cursors.Hand;
            m_deleteBut.Visible = false;
            m_deleteBut.Click += new EventHandler(m_deleteBut_Click);
            m_cropPanel.Controls.Add(m_deleteBut);

            // Create the color palette button
            m_CCWButt = new Button(); ;
            m_CCWButt.Name = "CCW";
            m_CCWButt.BackgroundImageLayout = ImageLayout.Stretch;
            m_CCWButt.BackColor = Color.Black;
            m_CCWButt.BackgroundImage = Properties.Resources.CW;
            m_CCWButt.BackgroundImage.RotateFlip(RotateFlipType.RotateNoneFlipX);
            m_CCWButt.Size = new Size((int)(m_cropPanel.Width * 0.03f), (int)(m_cropPanel.Width * 0.03f));
            m_CCWButt.Location = new Point(m_deleteBut.Width, 0);
            m_CCWButt.Cursor = Cursors.Hand;
            m_CCWButt.Visible = false;
            m_CCWButt.Click += new EventHandler(rotateButt_Click);
            m_cropPanel.Controls.Add(m_CCWButt);

            // Create the color palette button
            m_CWButt = new Button(); ;
            m_CWButt.Name = "CW";
            m_CWButt.BackgroundImageLayout = ImageLayout.Stretch;
            m_CWButt.BackColor = Color.Black;
            m_CWButt.BackgroundImage = Properties.Resources.CW;
            m_CWButt.Size = new Size((int)(m_cropPanel.Width * 0.03f), (int)(m_cropPanel.Width * 0.03f));
            m_CWButt.Location = new Point(2 * m_deleteBut.Width, 0);
            m_CWButt.Cursor = Cursors.Hand;
            m_CWButt.Visible = false;
            m_CWButt.Click += new EventHandler(rotateButt_Click);
            m_cropPanel.Controls.Add(m_CWButt);

            // Create the color dialog button
            m_colorBut = new Button();
            m_colorBut.BackColor = Color.Red;
            m_colorBut.Size = new Size((int)(m_cropPanel.Width * 0.03f), (int)(m_cropPanel.Width * 0.03f));
            m_colorBut.Location = new Point(3 * m_deleteBut.Width, 0);
            m_colorBut.Cursor = Cursors.Hand;
            m_colorBut.Visible = false;
            m_colorBut.Click += new EventHandler(m_colorBut_Click);
            m_cropPanel.Controls.Add(m_colorBut);

            // Create the color aprove crop button
            m_approveCrop = new Button();
            m_approveCrop.BackgroundImageLayout = ImageLayout.Stretch;
            m_approveCrop.BackColor = Color.Black;
            m_approveCrop.BackgroundImage = Properties.Resources.V;
            m_approveCrop.Size = new Size((int)(m_cropPanel.Width * 0.03f), (int)(m_cropPanel.Width * 0.03f));
            m_approveCrop.Location = new Point(4 * m_deleteBut.Width, 0);
            m_approveCrop.Cursor = Cursors.Hand;
            m_approveCrop.Visible = false;
            m_approveCrop.Click += new EventHandler(m_approveCrop_Click);
            m_cropPanel.Controls.Add(m_approveCrop);

            // Crop rectangle variables
            m_cropAction = CropAction.NONE;
            m_newLocation = new Point();
            m_prevMouse = new Point();
            m_scaleRadius = 6;
            m_scaleDir = Direction.CENTER;

            // Crop rectangle
            m_cropRect = new Rectangle();

            // Main picture box
            m_cropPb = new PictureBox();
            m_cropPb.Visible = false;
            m_cropPb.MouseMove += new MouseEventHandler(m_cropPb_MouseMove);
            m_cropPb.MouseDown += new MouseEventHandler(m_cropPb_MouseDown);
            m_cropPb.MouseUp += new MouseEventHandler(m_cropPb_MouseUp);
            m_cropPb.Paint += new PaintEventHandler(m_cropPb_Paint);
            m_cropPanel.Controls.Add(m_cropPb);

            // Initialize the logo bitmap
            Image logoImage = Properties.Resources.Subli_art_logo;
            float logoWidth = m_cropPanel.Width * 0.3f;
            float logoHeight = logoWidth / (logoImage.Width / (float)logoImage.Height);
            m_logoSize = new Size((int)logoWidth, (int)logoHeight);
            m_logo = new Bitmap(logoImage, m_logoSize);
            
            // Initialize the arrow timer
            m_arrowTimer = new Timer();
            m_arrowTimer.Interval = 1;
            m_arrowTimer.Tick += new EventHandler(m_arrowTimer_Tick);
        }
        #endregion

        #region Events
        private void m_cropPanel_Paint(object sender, PaintEventArgs e)
        {
            LinearGradientBrush brush = new LinearGradientBrush(
                new PointF(m_cropPanel.Width / 2.0f, 0),
                new PointF(m_cropPanel.Width / 2.0f, m_cropPanel.Height),
                Color.FromArgb(255, 98, 112, 173), Color.FromArgb(255, 45, 51, 78));

            if (m_currImage == null)
            {
                // Draw the crop panel background
                e.Graphics.FillRectangle(brush, new Rectangle(0, 0, m_cropPanel.Width, m_cropPanel.Height));
                
                // Draw the logo
                e.Graphics.DrawImage(m_logo, new PointF(
                    (m_cropPanel.Width - m_logoSize.Width) / 2.0f,
                    (m_cropPanel.Height - m_logoSize.Height) / 2.0f));
            }
            else
            {
                // If the image width-height ratio is bigger than the
                // crop panel width-height ratio
                if ((m_currImage.Width / (float)m_currImage.Height) > m_cropPanel.Width / (float)m_cropPanel.Height)
                {
                    e.Graphics.FillRectangle(brush, new RectangleF(0, 0, m_cropPanel.Width, (m_cropPanel.Height - m_currImage.Height) / 2.0f));
                    e.Graphics.FillRectangle(brush, new RectangleF(0, (m_cropPanel.Height + m_currImage.Height) / 2.0f, m_cropPanel.Width, (m_cropPanel.Height - m_currImage.Height) / 2.0f));
                }
                else
                {
                    e.Graphics.FillRectangle(brush, new RectangleF(0, 0, (m_cropPanel.Width - m_currImage.Width) / 2.0f, m_cropPanel.Height));
                    e.Graphics.FillRectangle(brush, new RectangleF((m_cropPanel.Width + m_currImage.Width) / 2.0f, 0, (m_cropPanel.Width - m_currImage.Width) / 2.0f, m_cropPanel.Height));
                }
            }
        }

        private void m_cropPanel_BackgroundImageChanged(object sender, EventArgs e)
        {
            // If no background image
            if (m_cropPanel.BackgroundImage == null)
            {
                m_deleteBut.Visible = false;
                m_CWButt.Visible = false;
                m_CCWButt.Visible = false;
                m_colorBut.Visible = false;
                m_approveCrop.Visible = false;
            }
        }

        private void m_cropPb_MouseDown(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
                InCropRect(e.X, e.Y, "down");
        }

        private void m_cropPb_MouseMove(object sender, MouseEventArgs e)
        {
            // If the mouse is in the crop picture box area
            if (e.X >= 0 && e.Y >= 0 && e.X < m_cropPb.Width && e.Y < m_cropPb.Height)
            {
                if (m_cropAction == CropAction.CREATE)
                    CreateCrop(e.X, e.Y);

                if (m_cropAction != CropAction.SCALE)
                   InCropRect(e.X, e.Y, "move");
                
                if(m_cropAction == CropAction.DRAG)
                    DragCrop(e.X, e.Y);
                else if (m_cropAction == CropAction.SCALE)
                    ScaleCrop(e.X, e.Y);
            }
        }

        private void m_cropPb_MouseUp(object sender, MouseEventArgs e)
        {
            m_cropAction = CropAction.NONE;
        }

        private void m_cropPb_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(m_cropperForm.m_slotsPanel.GetSlotColor()), m_cropRect);
            
            // Show crop in slot
            m_cropperForm.m_slotsPanel.CropToSlot(m_currImage, m_cropRect);
        }

        private void m_deleteBut_Click(object sender, EventArgs e)
        {
            DeleteImage();
        }

        private void m_colorBut_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            if (cd.ShowDialog() == DialogResult.OK)
            {
                Color newColor = Color.FromArgb(100, cd.Color.R, cd.Color.G, cd.Color.B);
                m_cropperForm.m_slotsPanel.SetSlotColor(newColor);
                m_colorBut.BackColor = Color.FromArgb(255, newColor.R, newColor.G, newColor.B);
                m_cropPb.Invalidate();
            }
        }

        private void m_approveCrop_Click(object sender, EventArgs e)
        {
            ApproveCrop();
        }

        private void rotateButt_Click(object sender, EventArgs e)
        {
            // Disapprove the slot
            m_cropperForm.m_slotsPanel.DisapproveSlot();
            HideArrow();

            if (((Button)sender).Name.Equals("CCW"))
                m_cropPanel.BackgroundImage.RotateFlip(RotateFlipType.Rotate90FlipXY);
            else
                m_cropPanel.BackgroundImage.RotateFlip(RotateFlipType.Rotate90FlipNone);

            ResizeCropArea(m_cropPanel.BackgroundImage);
        }

        private void m_arrowTimer_Tick(object sender, EventArgs e)
        {
            /*
            if (m_arrow.Image != null)
                // dispose old image (you might consider reusing it rather than making a new one each frame)
                m_arrow.Image.Dispose();
             */

            // The box tht contains the image <--- Play around with this more
            Image img = new Bitmap(m_arrowPb.Width, m_arrowPb.Height);

            // Setting the img Image to the pictureBox class?
            m_arrowPb.Image = img;

            // The arrow goes down
            if ((bool)m_arrowPb.Tag)
            {
                m_arrowPos.Y++;
                if (m_arrowPos.Y > m_arrowPb.Height - m_arrowBmp.Height)
                {
                    m_arrowPos.Y = m_arrowPb.Height - m_arrowBmp.Height;
                    m_arrowPb.Tag = false;
                }
            }
            // The arrow goes up
            else
            {
                m_arrowPos.Y--;
                if (m_arrowPos.Y < 0)
                {
                    m_arrowPos.Y = 0;
                    m_arrowPb.Tag = true;
                }
            }

            // G represents a drawing surface
            Graphics g = Graphics.FromImage(m_arrowPb.Image);

            // if the image needs to be behind the path, draw it beforehand
            g.DrawImage(m_arrowBmp, m_arrowPos);

            // Create a StringFormat object with the each line of text, and the block 
            // of text centered on the page.
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;


            //g.FillRectangle(new SolidBrush(Color.FromArgb(100, Color.Green)),
            //    new Rectangle((int)m_arrowPos.X, (int)m_arrowPos.Y, m_arrowPb.Width, (int)(m_arrowPb.Height * 0.3f)));

            // Draw the text and the surrounding rectangle.
            g.DrawString(
                m_cropperForm.m_langDictionary["Arrow Text"],
                new Font("Arial", m_arrowPb.Width * 0.06f), 
                Brushes.White,
                new Rectangle((int)m_arrowPos.X, (int)m_arrowPos.Y, m_arrowPb.Width, (int)(m_arrowPb.Height * 0.3f)),
                stringFormat);

            // prevent possible memory leaks
            g.Dispose();

            m_arrowPb.Refresh();
        }
        #endregion

        #region Functions
        private void InCropRect(int currX, int currY, string state)
        {
            // If the mouse is in crop rectangle area
            if (
                (currX > m_cropRect.X - m_scaleRadius && currX < m_cropRect.X + m_cropRect.Width + m_scaleRadius) &&
                (currY > m_cropRect.Y - m_scaleRadius && currY < m_cropRect.Y + m_cropRect.Height + m_scaleRadius))
            {
                // Get the place of the mouse on the crop rectangle
                m_scaleDir = GetDirection(currX, currY);

                // If the mouse is down
                if (state.Equals("down"))
                {
                    m_prevMouse.X = currX;
                    m_prevMouse.Y = currY;

                    if (m_scaleDir == Direction.CENTER)
                        m_cropAction = CropAction.DRAG;
                    else
                        m_cropAction = CropAction.SCALE;
                }
                else
                {
                    if (m_scaleDir == Direction.N || m_scaleDir == Direction.S)
                        m_cropPb.Cursor = Cursors.SizeNS;
                    else if (m_scaleDir == Direction.W || m_scaleDir == Direction.E)
                        m_cropPb.Cursor = Cursors.SizeWE;
                    else if (m_scaleDir == Direction.NW || m_scaleDir == Direction.SE)
                        m_cropPb.Cursor = Cursors.SizeNWSE;
                    else if (m_scaleDir == Direction.NE || m_scaleDir == Direction.SW)
                        m_cropPb.Cursor = Cursors.SizeNESW;
                    else
                        m_cropPb.Cursor = Cursors.SizeAll;
                }
            }
            else
            {
                // If the mouse is down
                if (state.Equals("down"))
                {
                    m_prevMouse.X = currX;
                    m_prevMouse.Y = currY;

                    m_cropAction = CropAction.CREATE;
                }
                else
                    m_cropAction = CropAction.NONE;

                m_cropPb.Cursor = Cursors.Cross;
            }
        }

        public void DeleteImage()
        {
            m_deleteBut.Visible = false;
            m_CWButt.Visible = false;
            m_CCWButt.Visible = false;
            m_colorBut.Visible = false;
            m_cropPb.Visible = false;
            m_approveCrop.Visible = false;
            m_currImage = null;
            m_cropPanel.BackgroundImage = null;
            m_cropperForm.m_slotsPanel.DeleteSlot();
            m_cropperForm.m_slotsPanel.DisapproveSlot();
            HideArrow();
        }

        private void DragCrop(int currX, int currY)
        {
            // Disapprove the slot
            m_cropperForm.m_slotsPanel.DisapproveSlot();
            HideArrow();

            // Calculateas the new location
            m_newLocation.X = m_cropRect.X + (currX - m_prevMouse.X);
            m_newLocation.Y = m_cropRect.Y + (currY - m_prevMouse.Y);

            // RIGHT border
            if (m_newLocation.X + m_cropRect.Width > m_cropPb.Width)
                m_newLocation.X = m_cropPb.Width - m_cropRect.Width;

            // BOTTOM border
            if (m_newLocation.Y + m_cropRect.Height > m_cropPb.Height)
                m_newLocation.Y = m_cropPb.Height - m_cropRect.Height;

            // LEFT border
            if (m_newLocation.X < 0)
                m_newLocation.X = 0;

            // TOP border
            if (m_newLocation.Y < 0)
                m_newLocation.Y = 0;

            // Updates the crop rectangle location
            m_cropRect.Location = m_newLocation;

            // Re-paint the crop region
            m_cropPb.Invalidate();

            // Store the previous mouse coordinates
            m_prevMouse.X = currX;
            m_prevMouse.Y = currY;
        }

        private void ScaleCrop(int currX, int currY)
        {
            // Disapprove the slot
            m_cropperForm.m_slotsPanel.DisapproveSlot();
            HideArrow();

            ScaleCorners(currX, currY);

            ScaleSides(currX, currY);
            
            // Re-paint the crop region
            m_cropPb.Invalidate();

            // Store the previous mouse coordinates
            m_prevMouse.X = currX;
            m_prevMouse.Y = currY;
        }

        private void ScaleSides(int currX, int currY)
        {
            float
                newX = m_cropRect.X,
                newY = m_cropRect.Y,
                newWidth = m_cropRect.Width,
                newHeight = m_cropRect.Height;

            // EAST
            if (m_scaleDir == Direction.E)
            {
                newWidth = m_cropRect.Width + (currX - m_prevMouse.X);

                // MINIMUM crop rectangle size
                if (newWidth < m_cropPanel.Width * 0.01f)
                    newWidth = m_cropPanel.Width * 0.01f;

                newHeight = newWidth / m_slotRatio;
                float pbRatio = (m_cropRect.Y + (m_cropRect.Height / 2.0f)) / m_cropPb.Height;
                newY = (m_cropPb.Height * pbRatio) - (newHeight * 0.5f);
                
                // BOTTOM border
                if (newY + newHeight > m_cropPb.Height)
                {
                    newY = m_cropRect.Y - (m_cropPb.Height - (m_cropRect.Y + m_cropRect.Height));
                    newHeight = m_cropPb.Height - newY;
                    newWidth = newHeight * m_slotRatio;
                }

                // TOP border
                if (newY < 0)
                {
                    newY = 0;
                    newHeight = m_cropRect.Height + (2 * m_cropRect.Y);
                    newWidth = newHeight * m_slotRatio;
                }
            }
            // WEST
            else if (m_scaleDir == Direction.W)
            {
                newWidth = m_cropRect.Width + (m_prevMouse.X - currX);

                // MINIMUM crop rectangle size
                if (newWidth < m_cropPanel.Width * 0.01f)
                    newWidth = m_cropPanel.Width * 0.01f;

                newHeight = newWidth / m_slotRatio;
                float pbRatio = (m_cropRect.Y + (m_cropRect.Height / 2.0f)) / m_cropPb.Height;
                newY = (m_cropPb.Height * pbRatio) - (newHeight * 0.5f);

                // BOTTOM border
                if (newY + newHeight > m_cropPb.Height)
                {
                    newY = m_cropRect.Y - (m_cropPb.Height - (m_cropRect.Y + m_cropRect.Height));
                    newHeight = m_cropPb.Height - newY;
                    newWidth = newHeight * m_slotRatio;
                }

                // TOP border
                if (newY < 0)
                {
                    newY = 0;
                    newHeight = m_cropRect.Height + (2 * m_cropRect.Y);
                    newWidth = newHeight * m_slotRatio;
                }

                newX = m_cropRect.X - (newWidth - m_cropRect.Width);
            }
            // SOUTH
            else if (m_scaleDir == Direction.S)
            {
                newHeight = m_cropRect.Height + (currY - m_prevMouse.Y);
                newWidth = newHeight * m_slotRatio;
                                
                // MINIMUM crop rectangle size
                if (newWidth < m_cropPanel.Width * 0.01f)
                {
                    newWidth = m_cropPanel.Width * 0.01f;
                    newHeight = newWidth / m_slotRatio;
                }

                float pbRatio = (m_cropRect.X + (m_cropRect.Width / 2.0f)) / m_cropPb.Width;
                newX = (m_cropPb.Width * pbRatio) - (newWidth* 0.5f);

                // RIGHT border
                if (newX + newWidth > m_cropPb.Width)
                {
                    newX = m_cropRect.X - (m_cropPb.Width - (m_cropRect.X + m_cropRect.Width));
                    newWidth = m_cropPb.Width - newX;
                    newHeight = newWidth / m_slotRatio;
                }

                // LEFT border
                if (newX < 0)
                {
                    newX = 0;
                    newWidth = m_cropRect.Width + (2 * m_cropRect.X);
                    newHeight = newWidth / m_slotRatio;
                }
            }
            // NORTH
            else if (m_scaleDir == Direction.N)
            {
                newHeight = m_cropRect.Height + (m_prevMouse.Y - currY);
                newWidth = newHeight * m_slotRatio;

                // MINIMUM crop rectangle size
                if (newWidth < m_cropPanel.Width * 0.01f)
                {
                    newWidth = m_cropPanel.Width * 0.01f;
                    newHeight = newWidth / m_slotRatio;
                }

                float pbRatio = (m_cropRect.X + (m_cropRect.Width / 2.0f)) / m_cropPb.Width;
                newX = (m_cropPb.Width * pbRatio) - (newWidth * 0.5f);

                // BOTTOM border
                if (newX + newWidth > m_cropPb.Width)
                {
                    newX = m_cropRect.X - (m_cropPb.Width - (m_cropRect.X + m_cropRect.Width));
                    newWidth = m_cropPb.Width - newX;
                    newHeight = newWidth / m_slotRatio;
                }

                // TOP border
                if (newX < 0)
                {
                    newX = 0;
                    newWidth = m_cropRect.Width + (2 * m_cropRect.X);
                    newHeight = newWidth / m_slotRatio;
                }

                newY = m_cropRect.Y - (newHeight - m_cropRect.Height);
            }

            m_cropRect.X = newX;
            m_cropRect.Y = newY;
            m_cropRect.Width = newWidth;
            m_cropRect.Height = newHeight;
        }

        private void ScaleCorners(int currX, int currY)
        {
            float
                newX = m_cropRect.X,
                newY = m_cropRect.Y,
                newWidth = m_cropRect.Width,
                newHeight = m_cropRect.Height;


            // SOUTH EAST
            if (m_scaleDir == Direction.SE)
            {
                // Calculate axis differences:
                // Y difference
                float yDiff = (currY - m_prevMouse.Y);

                // What the X difference should be
                float xDiff = yDiff * m_slotRatio;

                // What the actually X difference is
                xDiff = xDiff - (currX - m_prevMouse.X);

                // Final Y difference
                yDiff = yDiff - (xDiff * (1 - (m_slotRatio / 2.0f)));

                // Calculate new size
                newHeight = m_cropRect.Height + yDiff;
                newWidth = newHeight * m_slotRatio;

                // MINIMUM crop rectangle width
                if (newWidth < m_cropPanel.Width * 0.01f)
                    newWidth = m_cropPanel.Width * 0.01f;

                // Calculate height
                newHeight = newWidth / m_slotRatio;

                // RIGHT border
                if (m_cropRect.X + newWidth > m_cropPb.Width)
                {
                    newWidth = m_cropPb.Width - m_cropRect.X; 
                    newHeight = newWidth / m_slotRatio;
                }

                // BOTTOM border
                if (m_cropRect.Y + newHeight > m_cropPb.Height)
                {
                    newHeight = m_cropPb.Height - m_cropRect.Y;
                    newWidth = newHeight * m_slotRatio;
                }
            }
            // SOUTH WEST
            else if (m_scaleDir == Direction.SW)
            {
                // Calculate axis differences:
                // Y difference
                float yDiff = (currY - m_prevMouse.Y);

                // What the X difference should be
                float xDiff = yDiff * m_slotRatio;

                // What the actually X difference is
                xDiff = xDiff - (m_prevMouse.X - currX);

                // Final Y difference
                yDiff = yDiff - (xDiff * (1 - (m_slotRatio / 2.0f)));

                // Calculate new size
                newHeight = m_cropRect.Height + yDiff;
                newWidth = newHeight * m_slotRatio;

                // MINIMUM crop rectangle size
                if (newWidth < m_cropPanel.Width * 0.01f)
                    newWidth = m_cropPanel.Width * 0.01f;

                newHeight = newWidth / m_slotRatio;
                newX = m_cropRect.X - (newWidth - m_cropRect.Width);

                // LEFT border
                if (newX < 0)
                {
                    newX = 0;
                    newWidth = m_cropRect.X + m_cropRect.Width;
                    newHeight = newWidth / m_slotRatio;
                }

                // BOTTOM border
                if (m_cropRect.Y + newHeight > m_cropPb.Height)
                {
                    newHeight = m_cropPb.Height - m_cropRect.Y;
                    newWidth = newHeight * m_slotRatio;
                    newX = m_cropRect.X - (newWidth - m_cropRect.Width);
                }
            }
            // NORTH EAST
            else if (m_scaleDir == Direction.NE)
            {
                // Calculate axis differences:
                // Y difference
                float yDiff = (m_prevMouse.Y - currY);

                // What the X difference should be
                float xDiff = yDiff * m_slotRatio;

                // What the actually X difference is
                xDiff = xDiff - (currX - m_prevMouse.X);

                // Final Y difference
                yDiff = yDiff - (xDiff * (1 - (m_slotRatio / 2.0f)));

                // Calculate new size
                newHeight = m_cropRect.Height + yDiff;
                newWidth = newHeight * m_slotRatio;

                // MINIMUM crop rectangle size
                if (newWidth < m_cropPanel.Width * 0.01f)
                    newWidth = m_cropPanel.Width * 0.01f;

                newHeight = newWidth / m_slotRatio;
                newY = m_cropRect.Y - (newHeight - m_cropRect.Height);

                // TOP border
                if (newY < 0)
                {
                    newY = 0;
                    newHeight = m_cropRect.Y + m_cropRect.Height;
                    newWidth = newHeight * m_slotRatio;
                }
                
                // RIGHT border
                if (m_cropRect.X + newWidth > m_cropPb.Width)
                {
                    newWidth = m_cropPb.Width - m_cropRect.X;
                    newHeight = newWidth / m_slotRatio;
                    newY = m_cropRect.Y - (newHeight - m_cropRect.Height);
                }
            }
            // NORTH WEST
            else if (m_scaleDir == Direction.NW)
            {
                // Calculate axis differences:
                // Y difference
                float yDiff = (m_prevMouse.Y - currY);

                // What the X difference should be
                float xDiff = yDiff * m_slotRatio;

                // What the actually X difference is
                xDiff = xDiff - (m_prevMouse.X - currX);

                // Final Y difference
                yDiff = yDiff - (xDiff * (1 - (m_slotRatio / 2.0f)));

                // Calculate new size
                newHeight = m_cropRect.Height + yDiff;
                newWidth = newHeight * m_slotRatio;

                // MINIMUM crop rectangle size
                if (newWidth < m_cropPanel.Width * 0.01f)
                    newWidth = m_cropPanel.Width * 0.01f;

                newHeight = newWidth / m_slotRatio;
                newY = m_cropRect.Y - (newHeight - m_cropRect.Height);
                newX = m_cropRect.X - (newWidth - m_cropRect.Width);

                // TOP border
                if (newY < 0)
                {
                    newY = 0;
                    newHeight = m_cropRect.Y + m_cropRect.Height;
                    newWidth = newHeight * m_slotRatio;
                    newX = m_cropRect.X - (newWidth - m_cropRect.Width);
                }

                // LEFT border
                if (newX < 0)
                {
                    newX = 0;
                    newWidth = m_cropRect.X + m_cropRect.Width;
                    newHeight = newWidth / m_slotRatio;
                    newY = m_cropRect.Y - (newHeight - m_cropRect.Height);
                }
            }
            

            m_cropRect.Y = newY;
            m_cropRect.X = newX;
            m_cropRect.Width = newWidth;
            m_cropRect.Height = newHeight;
        }

        public void CreateCrop(int currX, int currY)
        {
            // Disapprove the slot
            m_cropperForm.m_slotsPanel.DisapproveSlot();
            HideArrow();

            // EAST
            if (currX >= m_prevMouse.X)
            {
                // SOUTH
                if(currY >= m_prevMouse.Y)
                    m_scaleDir = Direction.SE;
                // NORTH
                else
                    m_scaleDir = Direction.NE;
            }
            // WEST
            else
            {
                // SOUTH
                if (currY >= m_prevMouse.Y)
                    m_scaleDir = Direction.SW;
                // NORTH
                else
                    m_scaleDir = Direction.NW;
            }

            m_cropRect.X = m_prevMouse.X;
            m_cropRect.Y = m_prevMouse.Y;
            m_cropRect.Width = currX - m_prevMouse.X;
            m_cropRect.Height = currY - m_prevMouse.Y;
            m_cropAction = CropAction.SCALE;
        }

        private Direction GetDirection(int currX, int currY)
        {
            // NORTH WEST
            if (
                (currX > m_cropRect.X - m_scaleRadius && currX < m_cropRect.X + m_scaleRadius) &&
                (currY > m_cropRect.Y - m_scaleRadius && currY < m_cropRect.Y + m_scaleRadius))
            {
                return Direction.NW;
            }
            // SOUTH EAST
            else if (
                (currY > m_cropRect.Y + m_cropRect.Height - m_scaleRadius && currY < m_cropRect.Y + m_cropRect.Height + m_scaleRadius) &&
                (currX > m_cropRect.X + m_cropRect.Width - m_scaleRadius && currX < m_cropRect.X + m_cropRect.Width + m_scaleRadius))
            {
                return Direction.SE;
            }
            // SOUTH WEST
            else if (
                (currX > m_cropRect.X - m_scaleRadius && currX < m_cropRect.X + m_scaleRadius) &&
                (currY > m_cropRect.Y + m_cropRect.Height - m_scaleRadius && currY < m_cropRect.Y + m_cropRect.Height + m_scaleRadius))
            {
                return Direction.SW;
            }
            // NORTH EAST
            else if ((currY > m_cropRect.Y - m_scaleRadius && currY < m_cropRect.Y + m_scaleRadius) &&
                (currX > m_cropRect.X + m_cropRect.Width - m_scaleRadius && currX < m_cropRect.X + m_cropRect.Width + m_scaleRadius))
            {
                return Direction.NE;
            }

            // WEST border
            else if (currX > m_cropRect.X - m_scaleRadius && currX < m_cropRect.X + m_scaleRadius)
            {
                return Direction.W;
            }
            // EAST border
            else if (currX > m_cropRect.X + m_cropRect.Width - m_scaleRadius && currX < m_cropRect.X + m_cropRect.Width + m_scaleRadius)
            {
                return Direction.E;
            }

            // NORTH border
            else if (currY > m_cropRect.Y - m_scaleRadius && currY < m_cropRect.Y + m_scaleRadius)
            {
                return Direction.N;
            }
            // SOUTH border
            else if (currY > m_cropRect.Y + m_cropRect.Height - m_scaleRadius && currY < m_cropRect.Y + m_cropRect.Height + m_scaleRadius)
            {
                return Direction.S;
            }

            return Direction.CENTER;
        }

        public void DisplayImage(string imagePath)
        {
            ResizeCropArea(Image.FromFile(imagePath));

            // Disapprove the slot
            m_cropperForm.m_slotsPanel.DisapproveSlot();
            HideArrow();

            // Shows the X button at the top left corner
            m_deleteBut.Visible = true;
            m_CWButt.Visible = true;
            m_CCWButt.Visible = true;
            m_colorBut.Visible = true;
            m_approveCrop.Visible = true;
            m_cropperForm.m_slotsPanel.SetSlotColor(Color.FromArgb(100, 255, 0, 0));

            // Set the color dialog button back color
            Color slotColor = m_cropperForm.m_slotsPanel.GetSlotColor();
            m_colorBut.BackColor = Color.FromArgb(255, slotColor.R, slotColor.G, slotColor.B);
        }

        public void DisplayImage(Bitmap bmp, RectangleF cropRect)
        {
            if (bmp == null)
            {
                m_deleteBut.Visible = false;
                m_CWButt.Visible = false;
                m_CCWButt.Visible = false;
                m_cropPb.Visible = false;
                m_currImage = null;
                m_cropPanel.BackgroundImage = bmp;
                return;
            }
            
            // If the image width-height ratio is bigger than the
            // crop panel width-height ratio
            if ((bmp.Width / (float)bmp.Height) > m_cropPanel.Width / (float)m_cropPanel.Height)
            {
                m_cropPb.Width = m_cropPanel.Width;
                m_cropPb.Height = (int)(bmp.Height * (m_cropPb.Width / (float)bmp.Width));
            }
            else
            {
                m_cropPb.Height = m_cropPanel.Height;
                m_cropPb.Width = (int)(bmp.Width * (m_cropPb.Height / (float)bmp.Height));
            }

            // Save the current image
            m_currImage = bmp;
            
            // Set the transparent picturebox location
            m_cropPb.Location = new Point(
                (int)((m_cropPanel.Width - m_cropPb.Width) / 2.0f),
                (int)((m_cropPanel.Height - m_cropPb.Height) / 2.0f));
            m_cropPb.Visible = true;
            m_cropPb.BackColor = Color.Transparent;

            // Set the crop panel background image
            m_cropPanel.BackgroundImage = m_currImage;
            
            // Set the crop rectangle size and location
            m_cropRect = cropRect;

            // Shows the control buttons at the top left corner
            m_deleteBut.Visible = true;
            m_CWButt.Visible = true;
            m_CCWButt.Visible = true;
            m_colorBut.Visible = true;
            m_approveCrop.Visible = true;

            // Set the color dialog button back color
            Color slotColor = m_cropperForm.m_slotsPanel.GetSlotColor();
            m_colorBut.BackColor = Color.FromArgb(255, slotColor.R, slotColor.G, slotColor.B);
        }

        private void ResizeCropArea(Image img)
        {
            // If the image width-height ratio is bigger than the
            // crop panel width-height ratio
            if ((img.Width / (float)img.Height) > m_cropPanel.Width / (float)m_cropPanel.Height)
            {
                // Set the crop picture box size
                m_cropPb.Width = m_cropPanel.Width;
                m_cropPb.Height = (int)(img.Height * (m_cropPb.Width / (float)img.Width));
                
                // Set the crop rect size
                m_cropRect.Height = m_cropPb.Height * 0.4f;
                m_cropRect.Width = m_cropRect.Height * m_slotRatio;
            }
            else
            {
                // Set the crop picture box size
                m_cropPb.Height = m_cropPanel.Height;
                m_cropPb.Width = (int)(img.Width * (m_cropPb.Height / (float)img.Height));

                // Set the crop rect size
                m_cropRect.Width = m_cropPb.Width * 0.4f;
                m_cropRect.Height = m_cropRect.Width / m_slotRatio;
            }

            // Initialize the bitmap
            m_currImage = new Bitmap(img, m_cropPb.Size);

            // Set the transparent picturebox location
            m_cropPb.Location = new Point(
                (int)((m_cropPanel.Width - m_cropPb.Width) / 2.0f),
                (int)((m_cropPanel.Height - m_cropPb.Height) / 2.0f));
            m_cropPb.BackColor = Color.Transparent;
            m_cropPb.Visible = true;

            // Set the crop panel background image
            m_cropPanel.BackgroundImage = m_currImage;

            // Set the crop rectangle size and location
            m_cropRect.X = (int)((m_cropPb.Width - m_cropRect.Width) / 2.0f);
            m_cropRect.Y = (int)((m_cropPb.Height - m_cropRect.Height) / 2.0f);
        }

        public void ApproveCrop()
        {
            // Approve the slot
            m_cropperForm.m_slotsPanel.ApproveSlot();

            // If all slots are approved
            if (m_cropperForm.m_slotsPanel.AllApproved)
                // Show arrow
                ShowArrow();                           
        }

        public void ShowArrow()
        {
            if (m_arrowPb.Visible == false)
            {
                // Show the arrow picture box
                m_arrowPb.Visible = true;

                // Start the timer
                m_arrowTimer.Start();
            }
        }

        public void HideArrow()
        {
            if (m_arrowPb.Visible == true)
            {
                // Hide the arrow picture box
                m_arrowPb.Visible = false;

                // Initialize the arrow position
                m_arrowPos = new Point(0, 0);

                // Stop the arrow timer
                m_arrowTimer.Stop();
            }
        }
        #endregion

        #region Properties
        public int Height
        {
            get { return m_cropPanel.Height; }
        }
        #endregion
    }
}
