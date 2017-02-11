using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace SCFM
{

    /// <summary>List of possible results of a test within the SCFM project.</summary>
    public enum InfoLevel
    {

        /// <summary>Undefined/uninitialized state.</summary>
        undefined = 0,

        /// <summary>The test issues a "pass/success/green" score.</summary>
        GREEN = 1,

        /// <summary>The test issues a "warning/yellow" score.</summary>
        YELLOW = 2,

        /// <summary>The test issues a "fail/error/red" score.</summary>
        RED = 3

    }

}