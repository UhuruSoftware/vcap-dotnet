using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using CloudFoundry.Net;

namespace Uhuru.CloudFoundry.UI
{
    public class DataGridViewUriListColumn : DataGridViewImageColumn
    {
        public DataGridViewUriListColumn()
        {
            CellTemplate = new DataGridViewUriListCell();
        }
    }
    
    class DataGridViewUriListCell : DataGridViewImageCell
    {
        private const int MIN_VERTICAL_OFFSET = 0; //20;
        
        // Used to make custom cell consistent with a DataGridViewImageCell
        static Image emptyImage;
        
        static DataGridViewUriListCell()
        {
            emptyImage = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        }
        
        public DataGridViewUriListCell()
        {
            this.ValueType = typeof(string[]);
        }

        // Method required to make the cell consistent with the default Image Cell. 
        // The default Image Cell assumes an Image as a value
        protected override object GetFormattedValue(object value,
                            int rowIndex, ref DataGridViewCellStyle cellStyle,
                            TypeConverter valueTypeConverter,
                            TypeConverter formattedValueTypeConverter,
                            DataGridViewDataErrorContexts context)
        {
            return emptyImage;
        }

        private string GetItemAtCoordinates(int x, int y)
        {
            string[] content = this.Value as string[];
            if (content == null) return string.Empty;

            DataGridView dataGrid = this.DataGridView;
            if (dataGrid == null) return string.Empty;

            //recompute measurements so that we'll know which item was clicked
            float topOffset = Math.Max(0, (this.Size.Height - GetContentHeight(content, this.InheritedStyle.Font)) / 2);

            //get the index of the clicked uri
            float approximateIndex = ((y - topOffset) / this.InheritedStyle.Font.Height);
            int index = (int)Math.Floor(approximateIndex);
            if (approximateIndex < 0 || index >= content.Length) //the click was outside the text
                return string.Empty;

            //and now we need to check whether the click was on the actual string or beside it
            Graphics g = dataGrid.CreateGraphics();
            SizeF stringSize = g.MeasureString(content[index], this.InheritedStyle.Font);
            if (x > stringSize.Width) //the click was outside the string
                return string.Empty;

            return content[index];
        }

        protected override void OnMouseClick(DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                string uriUnderMouse = GetItemAtCoordinates(e.X, e.Y);
                if (uriUnderMouse.Length > 0)
                    Utils.OpenLink(uriUnderMouse);
              
            }
            
            base.OnMouseClick(e);
        }

        public int GetContentHeight(string[] urls, Font font)
        {
            //returns the sum of the heights of the strings written with the given font & underlined
            Font underlined = new Font(font.FontFamily, font.Size, FontStyle.Underline);
            Graphics e = Graphics.FromImage(new Bitmap(1,1)); 
            float sum = (urls.Select(u => e.MeasureString(u, underlined).Height)).Sum();

            return (int)Math.Ceiling(sum) + MIN_VERTICAL_OFFSET;
        }


        protected override void Paint(System.Drawing.Graphics g, System.Drawing.Rectangle clipBounds, System.Drawing.Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            Brush backColorBrush = new SolidBrush(cellStyle.BackColor);
            Brush foreColorBrush = new SolidBrush(cellStyle.ForeColor);

            // draws the cell grid
            base.Paint(g, clipBounds, cellBounds,
             rowIndex, cellState, value, formattedValue, errorText,
             cellStyle, advancedBorderStyle, (paintParts & ~DataGridViewPaintParts.ContentForeground));

            string[] uris = this.Value as string[];
            if (uris == null || uris.Length == 0) return;
            
            //draw the text
            Font font = new Font(cellStyle.Font.FontFamily, cellStyle.Font.Size, FontStyle.Underline);
            float top = cellBounds.Y + Math.Max(0, (cellBounds.Height - GetContentHeight(uris, cellStyle.Font)) / 2);
            foreach (string uri in uris)
            {
                //SizeF size = g.MeasureString(uri, cellStyle.Font);
                g.DrawString(uri, font, foreColorBrush, cellBounds.X, top);
                top += cellStyle.Font.Height;
            }
        }

        protected override void OnMouseLeave(int rowIndex)
        {
            base.OnMouseLeave(rowIndex);
            if (this.DataGridView == null)
            {
                return;
            }
            this.DataGridView.Cursor = Cursors.Default;
        }

        protected override void OnMouseMove(DataGridViewCellMouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (this.DataGridView == null) return;

            string itemUnderMouse = GetItemAtCoordinates(e.X, e.Y);
            this.DataGridView.Cursor = itemUnderMouse.Length == 0 ? Cursors.Default : Cursors.Hand;
        }
    }
}
