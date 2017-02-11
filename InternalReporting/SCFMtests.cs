using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace SCFM
{

    /// <summary>List of all tests within SCFM project.</summary>
    /// <remarks>This Enum is used by <see cref="SCFM.ReportSnippet"/> to identify the underlying test of the report.</remarks>
    public enum SCFMtests
    {

        /// <summary>Dosimetric assessment of constraint of an OAR.</summary>
        constraintCheck

    }

}