using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookmarkaa.Helpers.Controls
{
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Media;

    public class DropIndicatorAdorner : Adorner
    {
        private readonly bool _isAbove;

        public DropIndicatorAdorner(UIElement adornedElement, bool isAbove)
            : base(adornedElement)
        {
            _isAbove = isAbove;
            IsHitTestVisible = false;
        }

        protected override void OnRender(DrawingContext dc)
        {
            var pen = new Pen(Brushes.Red, 1); // DodgerBlue
            pen.Freeze();

            double y = _isAbove ? 0 : AdornedElement.RenderSize.Height;
            dc.DrawLine(pen, new Point(0, y), new Point(AdornedElement.RenderSize.Width, y));
        }
    }

}
