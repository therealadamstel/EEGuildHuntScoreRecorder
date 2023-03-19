using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEGuildHuntTool
{
    class RectEqualityComparer : IEqualityComparer<Rectangle>
    {
        public bool Equals(Rectangle x, Rectangle y)
        {
            return x.Width == y.Width
                && x.Height == y.Height
                && x.Top == y.Top
                && x.Left == y.Left;
        }

        public int GetHashCode([DisallowNull] Rectangle obj)
        {
            return obj.GetHashCode();
        }
    }
}
