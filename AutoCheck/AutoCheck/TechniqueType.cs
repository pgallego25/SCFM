using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.IO;

namespace VMS.TPS
{
        // Enumeració que classifica el pla de tractament en funció de la tècnica utilitzada.

        internal enum TechniqueType
        {
            Electrons,
            Standard,
            Palliative,
            SBRT,
            IMRT
        }



}