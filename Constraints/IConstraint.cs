using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GK1_PolygonEditor
{
    internal interface IConstraint
    {
        ConstraintType ConstraintType { get; }
        bool IsPreserved();
        Image? Icon { get; }
        string? Text { get; }
    }
}
