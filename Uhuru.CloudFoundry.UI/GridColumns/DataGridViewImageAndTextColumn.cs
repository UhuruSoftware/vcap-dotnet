using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

namespace Uhuru.CloudFoundry.UI
{
    
        public class DataGridViewImageAndTextColumn : DataGridViewImageColumn
        {
            public DataGridViewImageAndTextColumn()
            {
                CellTemplate = new DataGridViewImageAndTextCell();
            }
        }


        class DataGridViewImageAndTextCell : DataGridViewImageCell
        {

            private const int HORIZONTAL_OFFSET = 5;
            
            // Used to make custom cell consistent with a DataGridViewImageCell
            static Image emptyImage;
            static DataGridViewImageAndTextCell()
            {
                emptyImage = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            }
            
            
            public DataGridViewImageAndTextCell()
            {
                this.ValueType = typeof(List<KeyValuePair<Image, string>>);
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
                Brush backColorBrush = new SolidBrush(cellStyle.BackColor);
                Brush foreColorBrush = new SolidBrush(cellStyle.ForeColor);

                // draws the cell grid
                base.Paint(g, clipBounds, cellBounds,
                    rowIndex, cellState, value, formattedValue, errorText,
                    cellStyle, advancedBorderStyle, (paintParts & ~DataGridViewPaintParts.ContentForeground));

                List<KeyValuePair<Image, string>> values = this.Value as List<KeyValuePair<Image, string>>;
                if (values == null) return;

                // draw the image
                float top = cellBounds.Y + (cellBounds.Height - values.Select(k => k.Key.Height).Sum()) / 2;
                foreach (KeyValuePair<Image, string> kvp in values)
                {
                    Image image = kvp.Key;
                    if (image != null)
                    {
                        Bitmap b = new Bitmap(image);
                        b.MakeTransparent(Color.White);

                        g.DrawImage(b, cellBounds.X + HORIZONTAL_OFFSET, top, image.Width, image.Height);
                    }

                    //draw the text
                    SizeF size = g.MeasureString(kvp.Value, cellStyle.Font);
                    g.DrawString(kvp.Value, cellStyle.Font, foreColorBrush, cellBounds.X + HORIZONTAL_OFFSET + (image == null ? 0 : image.Width), top);

                    top += (image == null ? size.Height : image.Height);
                }
            }

        }
    }

