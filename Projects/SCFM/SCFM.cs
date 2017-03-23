using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using SCFM;


namespace VMS.TPS
{

    /// <summary>Bridge between ESAPI and SCFM project.</summary>
    /// <remarks>The types, GUI and performance of the project must be enclosed within <c>SCFM</c> namespace. This class only links ESAPI with SCFM project.</remarks>
    public class Script
    {

        /// <summary>Default 0-parameter constructor.</summary>
        public Script() { }

        /// <summary>Entry point of the script.</summary>
        /// <param name="context">The context provided by Eclipse. Type: <see cref="VMS.TPS.Common.Model.API.ScriptContext"/>.</param>
        public void Execute(ScriptContext context /*, System.Windows.Window window*/)
        {
            IntegrationTests.Constraint_Dmax(context);
        }

    }

    /// <summary>Static class that contains calls to all test projects of the solution.</summary>
    public static class IntegrationTests
    {

        /// <summary>Static method providing access test outcomes for type <see cref="SCFM.Constraint_Dmax"/>.</summary>
        /// <param name="context">The context provided by Eclipse. Type: <see cref="VMS.TPS.Common.Model.API.ScriptContext"/>.</param>
        public static void Constraint_Dmax(ScriptContext context)
        {
            string result =
                "TYPE: SCFM.Constraint_Dmax" +
                "\n - Instantiation and Querying:\t" + Constraint_Dmax_Test.Instantiation_and_Querying(context).ToString() +
                /*"\n - Exotic Arguments:\t" + Constraint_Dmax_Test.ExoticArguments(context).ToString() +*/
                "\n - String Representation:\t" + Constraint_Dmax_Test.StringRepresentation(context).ToString();
            MessageBox.Show(result, "SCFM test results");
        }

    }

}

// TODO: for testing purposes only.