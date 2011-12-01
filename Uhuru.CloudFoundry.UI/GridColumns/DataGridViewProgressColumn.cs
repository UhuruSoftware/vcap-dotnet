using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;

namespace Uhuru.CloudFoundry.UI
{
    
        public class DataGridViewProgressColumn : DataGridViewImageColumn
        {
            public DataGridViewProgressColumn()
            {
                CellTemplate = new DataGridViewProgressCell();
            }
        }
    
    
        class DataGridViewProgressCell : DataGridViewImageCell
        {
            private float maxValue = 100;

            public float MaxValue
            {
                get { return maxValue; }
                set { maxValue = value; }
            }
            
            // Used to make custom cell consistent with a DataGridViewImageCell
            static Image emptyImage;
            static DataGridViewProgressCell()
            {
                emptyImage = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            }
            public DataGridViewProgressCell()
            {
                this.ValueType = typeof(int);
            }

            // Method required to make the Progress Cell consistent with the default Image Cell. 
            // The default Image Cell assumes an Image as a value, although the value of the Progress Cell is an int.
            protected override object GetFormattedValue(object value,
                                int rowIndex, ref DataGridViewCellStyle cellStyle,
                                TypeConverter valueTypeConverter,
                                TypeConverter formattedValueTypeConverter,
                                DataGridViewDataErrorContexts context)
            {
                return emptyImage;
            }

            protected override void Paint(System.Drawing.Graphics g, System.Drawing.Rectangle clipBounds, System.Drawing.Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
            {
                int progressVal = Convert.ToInt32(value ?? 0);
                float percentage = maxValue == 0? 0 : ((float)progressVal / maxValue);
                Brush backColorBrush = new SolidBrush(cellStyle.BackColor);
                Brush foreColorBrush = new SolidBrush(cellStyle.ForeColor);
                // Draws the cell grid
                base.Paint(g, clipBounds, cellBounds,
                 rowIndex, cellState, value, formattedValue, errorText,
                 cellStyle, advancedBorderStyle, (paintParts & ~DataGridViewPaintParts.ContentForeground));
                if (percentage >= 0.0)
                {
                    // Draw the progress bar and the text
                    g.FillRectangle(new SolidBrush(Color.FromArgb(163, 189, 242)), cellBounds.X + 2, cellBounds.Y + 2, Convert.ToInt32((percentage * cellBounds.Width - 4)), cellBounds.Height - 4);

                    string stringToDraw = string.Format("{0}/{1}", progressVal, maxValue);
                    SizeF size = g.MeasureString(stringToDraw, cellStyle.Font);

                    g.DrawString(stringToDraw, cellStyle.Font, foreColorBrush, cellBounds.X + (cellBounds.Width - size.Width) / 2, cellBounds.Y + (cellBounds.Height - size.Height) / 2);
                }
                else
                {
                    // draw the text
                    if (this.DataGridView.CurrentRow.Index == rowIndex)
                        g.DrawString(progressVal.ToString() + "%", cellStyle.Font, new SolidBrush(cellStyle.SelectionForeColor), cellBounds.X + 6, cellBounds.Y + 2);
                    else
                        g.DrawString(progressVal.ToString() + "%", cellStyle.Font, foreColorBrush, cellBounds.X + 6, cellBounds.Y + 2);
                }
            }
        }
    }

