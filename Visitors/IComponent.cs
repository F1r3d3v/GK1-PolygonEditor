﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GK1_PolygonEditor
{
    internal interface IComponent
    {
        void Accept(IVisitor visitor);
    }
}
