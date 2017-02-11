using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;


namespace SCFM
{

    /// <summary>Unit tests for type <see cref="SCFM.ReportSnippet"/>.</summary>
    /// <remarks>
    /// These tests are not based on any testing framework: they are constructed by using regular classes and methods.
    /// Static class since it only contains a set of static methods.
    /// </remarks>
    public static class ReportSnippet_Test
    {

        /// <summary>Method that checks the instantiation and all properties of an object of type <see cref="SCFM.ReportSnippet"/> when appropriate arguments are provided.</summary>
        /// <param name="context">The context provided by Eclipse. Type: <see cref="VMS.TPS.Common.Model.API.ScriptContext"/>.</param>
        /// <returns>The result of the test in terms of a single boolean variable, resulting from a logical AND of all individual checks (<c>true</c> = pass, <c>false</c> = fail). Type: <see cref="System.Boolean"/>.</returns>
        public static bool Instantiation_and_Querying(ScriptContext context)
        {

            // Arrange
            SCFMtests _testSCFM = SCFMtests.constraintCheck;
            InfoLevel _testInfo = InfoLevel.GREEN;
            string _testMessage = "Test message";
            object _testSource = new Object();
            double
                _testCurrent = 35.0,            
                _testRefefence = 38.0,
                _testTolSoft = 2.0,
                _testTolHard = 5.0;
            string
                _testUnit = "u",
                _testTolUnit = "t";
            ComparisonMode _testComparison = ComparisonMode.greaterThan;
            
            // Act
            ReportSnippet
                r1 = new ReportSnippet(),
                r2 = new ReportSnippet(_testSCFM, _testInfo, _testMessage, _testSource, _testCurrent, _testUnit),
                r3 = new ReportSnippet(_testSCFM, _testInfo, _testMessage, _testSource, _testCurrent, _testRefefence, _testUnit, _testComparison),
                r4 = new ReportSnippet(_testSCFM, _testInfo, _testMessage, _testSource, _testCurrent, _testUnit, _testTolSoft, _testTolHard, _testTolUnit, _testComparison),
                r5 = new ReportSnippet(_testSCFM, _testInfo, _testMessage, _testSource, _testCurrent, _testRefefence, _testUnit, _testTolSoft, _testTolHard, _testTolUnit, _testComparison);
            
            // Assert
            throw new NotImplementedException();

        }

    }

}
