using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Origins
{
    public static class Healthbar
    {
        public static void Draw(Graphics G, int X, int Y, int height, int width, double value, double maxValue, Color backColor, Color foreColor)
        {
            if (value > maxValue)
                value = maxValue;
            if (value < 0)
                value = 0;

            G.DrawRectangle(new Pen(backColor, 0.5f), new Rectangle(X, Y, width, height));
            G.FillRectangle(new SolidBrush(foreColor), new Rectangle(X, Y, (int)((width * value) / maxValue), height));
        }
    }
}
