using System.Linq;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;


namespace SCFM
{

    /// <summary>Unit tests for type <see cref="SCFM.Constraint_Dmax"/>.</summary>
    /// <remarks>
    /// These tests are not based on any testing framework: they are constructed by using regular classes and methods.
    /// Static class since it only contains a set of static methods.
    /// </remarks>
    public static class Constraint_Dmax_Test
    {

        /// <summary>Method that checks the instantiation through all available constructors and the usage of all properties of an object of type <see cref="SCFM.Constraint_Dmax"/> when appropriate arguments are provided (exceptions are not tested).</summary>
        /// <param name="context">The context provided by Eclipse. Type: <see cref="VMS.TPS.Common.Model.API.ScriptContext"/>.</param>
        /// <returns>The result of the test in terms of a single boolean variable, resulting from a logical AND of all individual checks (<c>true</c> = pass, <c>false</c> = fail). Type: <see cref="System.Boolean"/>.</returns>
        public static bool Instantiation_and_Querying(ScriptContext context)
        {

            // Arrange
            double _testDose = 25.5;
            DoseValue.DoseUnit
                _testDoseUnitAbs = DoseValue.DoseUnit.Gy,
                _testDoseUnitRel = DoseValue.DoseUnit.Percent;
            DoseValue
                D_ABS = new DoseValue(_testDose, _testDoseUnitAbs),
                D_REL = new DoseValue(_testDose, _testDoseUnitRel);
            Structure _testStructure = context.StructureSet.Structures.FirstOrDefault();
            StructureType
                _testOARtype = StructureType.OAR,
                _testPTVtype = StructureType.PTV,
                _testStructureType = StructureType.structure;
            ComparisonMode
                _testComparerLT = ComparisonMode.lessThan,
                _testComparerLE = ComparisonMode.lessEqual,
                _testComparerGT = ComparisonMode.greaterThan,
                _testComparerGE = ComparisonMode.greaterEqual;
            string
                _testDoseUnitAsStringAbs = "Gy",
                _testDoseUnitAsStringRel = "%",
                _testLabel = "Dmax",
                _testComparerSymbolLT = "<",
                _testComparerSymbolLE = "<=",
                _testComparerSymbolGT = ">",
                _testComparerSymbolGE = ">=";

            // Act
            Constraint_Dmax
                c1 = new Constraint_Dmax(D_ABS, _testOARtype, _testComparerLT),
                c2 = new Constraint_Dmax(D_REL, _testOARtype, _testComparerLE),
                c3 = new Constraint_Dmax(D_ABS, _testPTVtype, _testComparerGT),
                c4 = new Constraint_Dmax(D_REL, _testStructureType, _testComparerGE),
                c5 = new Constraint_Dmax(D_ABS, _testStructure, _testOARtype, _testComparerLT),
                c6 = new Constraint_Dmax(D_REL, _testStructure, _testOARtype, _testComparerLE),
                c7 = new Constraint_Dmax(D_ABS, _testStructure, _testPTVtype, _testComparerGT),
                c8 = new Constraint_Dmax(D_REL, _testStructure, _testStructureType, _testComparerGE);

            // Assert
            bool dose =
                c1.Dose.Dose == _testDose && c1.Dose.Unit == _testDoseUnitAbs &&
                c2.Dose.Dose == _testDose && c2.Dose.Unit == _testDoseUnitRel &&
                c3.Dose.Dose == _testDose && c3.Dose.Unit == _testDoseUnitAbs &&
                c4.Dose.Dose == _testDose && c4.Dose.Unit == _testDoseUnitRel &&
                c5.Dose.Dose == _testDose && c5.Dose.Unit == _testDoseUnitAbs &&
                c6.Dose.Dose == _testDose && c6.Dose.Unit == _testDoseUnitRel &&
                c7.Dose.Dose == _testDose && c7.Dose.Unit == _testDoseUnitAbs &&
                c8.Dose.Dose == _testDose && c8.Dose.Unit == _testDoseUnitRel;
            bool volume =
                c1.Volume == null && c1.VolumeUnits == null &&
                c2.Volume == null && c2.VolumeUnits == null &&
                c3.Volume == null && c3.VolumeUnits == null &&
                c4.Volume == null && c4.VolumeUnits == null &&
                c5.Volume == null && c5.VolumeUnits == null &&
                c6.Volume == null && c6.VolumeUnits == null &&
                c7.Volume == null && c7.VolumeUnits == null &&
                c8.Volume == null && c8.VolumeUnits == null;
            bool label =
                c1.Label == _testLabel &&
                c2.Label == _testLabel &&
                c3.Label == _testLabel &&
                c4.Label == _testLabel &&
                c5.Label == _testLabel &&
                c6.Label == _testLabel &&
                c7.Label == _testLabel &&
                c8.Label == _testLabel;
            bool index =
                c1.Index == null && c1.IndexUnits == null &&
                c2.Index == null && c2.IndexUnits == null &&
                c3.Index == null && c3.IndexUnits == null &&
                c4.Index == null && c4.IndexUnits == null &&
                c5.Index == null && c5.IndexUnits == null &&
                c6.Index == null && c6.IndexUnits == null &&
                c7.Index == null && c7.IndexUnits == null &&
                c8.Index == null && c8.IndexUnits == null;
            bool threshold =
                c1.Threshold == _testDose && c1.ThresholdUnits == _testDoseUnitAsStringAbs &&
                c2.Threshold == _testDose && c2.ThresholdUnits == _testDoseUnitAsStringRel &&
                c3.Threshold == _testDose && c3.ThresholdUnits == _testDoseUnitAsStringAbs &&
                c4.Threshold == _testDose && c4.ThresholdUnits == _testDoseUnitAsStringRel &&
                c5.Threshold == _testDose && c5.ThresholdUnits == _testDoseUnitAsStringAbs &&
                c6.Threshold == _testDose && c6.ThresholdUnits == _testDoseUnitAsStringRel &&
                c7.Threshold == _testDose && c7.ThresholdUnits == _testDoseUnitAsStringAbs &&
                c8.Threshold == _testDose && c8.ThresholdUnits == _testDoseUnitAsStringRel;            
            bool structure =
                c1.Structure == null && c1.StructureType == _testOARtype &&
                c2.Structure == null && c2.StructureType == _testOARtype &&
                c3.Structure == null && c3.StructureType == _testPTVtype &&
                c4.Structure == null && c4.StructureType == _testStructureType &&
                c5.Structure == _testStructure && c5.StructureType == _testOARtype &&
                c6.Structure == _testStructure && c6.StructureType == _testOARtype &&
                c7.Structure == _testStructure && c7.StructureType == _testPTVtype &&
                c8.Structure == _testStructure && c8.StructureType == _testStructureType;
            bool comparer =
                c1.ComparisonMode == _testComparerLT && c1.ComparisonSymbol == _testComparerSymbolLT &&
                c2.ComparisonMode == _testComparerLE && c2.ComparisonSymbol == _testComparerSymbolLE &&
                c3.ComparisonMode == _testComparerGT && c3.ComparisonSymbol == _testComparerSymbolGT &&
                c4.ComparisonMode == _testComparerGE && c4.ComparisonSymbol == _testComparerSymbolGE &&
                c5.ComparisonMode == _testComparerLT && c5.ComparisonSymbol == _testComparerSymbolLT &&
                c6.ComparisonMode == _testComparerLE && c6.ComparisonSymbol == _testComparerSymbolLE &&
                c7.ComparisonMode == _testComparerGT && c7.ComparisonSymbol == _testComparerSymbolGT &&
                c8.ComparisonMode == _testComparerGE && c8.ComparisonSymbol == _testComparerSymbolGE;
            return dose && volume && label && index && threshold && structure && comparer;
            
        }

        /// <summary>Method that checks built-in parameter-checking of the properties of objects of type <see cref="SCFM.Constraint_Dmax"/> (which prevent the creation of exotic objects by passing incorrect parameters to the constructor).</summary>
        /// <param name="context">The context provided by Eclipse. Type: <see cref="VMS.TPS.Common.Model.API.ScriptContext"/>.</param>
        /// <returns>The result of the test in terms of a single boolean variable, resulting from a logical AND of all individual checks (<c>true</c> = pass, <c>false</c> = fail). Type: <see cref="System.Boolean"/>.</returns>
        //public static bool ExoticArguments(ScriptContext context)
        //{
            
        //    // Arrange
        //    double
        //        _testPositiveDose = 25.5,
        //        _testZeroDose = 0.0,
        //        _testNegativeDose = -25.5;
        //    DoseValue.DoseUnit
        //        _testDoseUnitAbsGY = DoseValue.DoseUnit.Gy,
        //        _testDoseUnitAbsCGY = DoseValue.DoseUnit.cGy,
        //        _testDoseUnitRel = DoseValue.DoseUnit.Percent,
        //        _testDoseUnitUndef = DoseValue.DoseUnit.Unknown;
        //    DoseValue
        //        D_ZERO_ABS = new DoseValue(_testZeroDose, _testDoseUnitAbsGY),
        //        D_ZERO_REL = new DoseValue(_testZeroDose, _testDoseUnitRel),
        //        D_NEG_ABS = new DoseValue(_testNegativeDose, _testDoseUnitAbsGY),
        //        D_NEG_REL = new DoseValue(_testNegativeDose, _testDoseUnitRel),
        //        D_POS_CGY = new DoseValue(_testPositiveDose, _testDoseUnitAbsCGY),
        //        D_UNDEF = DoseValue.UndefinedDose();
        //    Structure _testOAR = context.StructureSet.Structures.FirstOrDefault();
        //    StructureType _testOARtype = StructureType.OAR;
        //    string
        //        _testParameterDose = "value.Dose (D)",
        //        _testParameterUnit = "value.Unit ([D])",
        //        _testMessage = "SCFM exception";
        //    bool exotic = true;

        //    try // Act
        //    {                
        //        Constraint_Dmax c1 = new Constraint_Dmax(D_ZERO_ABS);
        //    }
        //    catch (ArgumentOutOfRangeException e) // Assert
        //    {                
        //        exotic &= ((double)e.ActualValue == _testZeroDose && e.ParamName == _testParameterDose && e.Message.StartsWith(_testMessage));
        //    }
        //    try // Act
        //    {
        //        Constraint_Dmax c2 = new Constraint_Dmax(D_ZERO_ABS, _testOAR, _testOARtype);
        //    }
        //    catch (ArgumentOutOfRangeException e) // Assert
        //    {
        //        exotic &= ((double)e.ActualValue == _testZeroDose && e.ParamName == _testParameterDose && e.Message.StartsWith(_testMessage));
        //    }
        //    try // Act
        //    {
        //        Constraint_Dmax c3 = new Constraint_Dmax(D_ZERO_REL);
        //    }
        //    catch (ArgumentOutOfRangeException e) // Assert
        //    {
        //        exotic &= ((double)e.ActualValue == _testZeroDose && e.ParamName == _testParameterDose && e.Message.StartsWith(_testMessage));
        //    }
        //    try // Act
        //    {
        //        Constraint_Dmax c4 = new Constraint_Dmax(D_ZERO_REL, _testOAR, _testOARtype);
        //    }
        //    catch (ArgumentOutOfRangeException e) // Assert
        //    {
        //        exotic &= ((double)e.ActualValue == _testZeroDose && e.ParamName == _testParameterDose && e.Message.StartsWith(_testMessage));
        //    }
        //    try // Act
        //    {
        //        Constraint_Dmax c5 = new Constraint_Dmax(D_NEG_ABS);
        //    }
        //    catch (ArgumentOutOfRangeException e) // Assert
        //    {
        //        exotic &= ((double)e.ActualValue == _testNegativeDose && e.ParamName == _testParameterDose && e.Message.StartsWith(_testMessage));
        //    }
        //    try // Act
        //    {
        //        Constraint_Dmax c6 = new Constraint_Dmax(D_NEG_ABS, _testOAR, _testOARtype);
        //    }
        //    catch (ArgumentOutOfRangeException e) // Assert
        //    {
        //        exotic &= ((double)e.ActualValue == _testNegativeDose && e.ParamName == _testParameterDose && e.Message.StartsWith(_testMessage));
        //    }
        //    try // Act
        //    {
        //        Constraint_Dmax c7 = new Constraint_Dmax(D_NEG_REL);
        //    }
        //    catch (ArgumentOutOfRangeException e) // Assert
        //    {
        //        exotic &= ((double)e.ActualValue == _testNegativeDose && e.ParamName == _testParameterDose && e.Message.StartsWith(_testMessage));
        //    }
        //    try // Act
        //    {
        //        Constraint_Dmax c8 = new Constraint_Dmax(D_NEG_REL, _testOAR, _testOARtype);
        //    }
        //    catch (ArgumentOutOfRangeException e) // Assert
        //    {
        //        exotic &= ((double)e.ActualValue == _testNegativeDose && e.ParamName == _testParameterDose && e.Message.StartsWith(_testMessage));
        //    }
        //    try // Act
        //    {
        //        Constraint_Dmax c9 = new Constraint_Dmax(D_POS_CGY);
        //    }
        //    catch (ArgumentOutOfRangeException e) // Assert
        //    {
        //        exotic &= ((DoseValue.DoseUnit)e.ActualValue == _testDoseUnitAbsCGY && e.ParamName == _testParameterUnit && e.Message.StartsWith(_testMessage));
        //    }
        //    try // Act
        //    {
        //        Constraint_Dmax c10 = new Constraint_Dmax(D_POS_CGY, _testOAR, _testOARtype);
        //    }
        //    catch (ArgumentOutOfRangeException e) // Assert
        //    {
        //        exotic &= ((DoseValue.DoseUnit)e.ActualValue == _testDoseUnitAbsCGY && e.ParamName == _testParameterUnit && e.Message.StartsWith(_testMessage));
        //    }
        //    try // Act
        //    {
        //        Constraint_Dmax c11 = new Constraint_Dmax(D_UNDEF);
        //    }
        //    catch (ArgumentOutOfRangeException e) // Assert
        //    {
        //        exotic &= ((DoseValue.DoseUnit)e.ActualValue == _testDoseUnitUndef && e.ParamName == _testParameterUnit && e.Message.StartsWith(_testMessage));
        //    }
        //    try // Act
        //    {
        //        Constraint_Dmax c12 = new Constraint_Dmax(D_UNDEF, _testOAR, _testOARtype);
        //    }
        //    catch (ArgumentOutOfRangeException e) // Assert
        //    {
        //        exotic &= ((DoseValue.DoseUnit)e.ActualValue == _testDoseUnitUndef && e.ParamName == _testParameterUnit && e.Message.StartsWith(_testMessage));
        //    }
        //    return exotic;
        //}

        /// <summary>Method that checks the string representation of an object of type <see cref="SCFM.Constraint_Dmax"/>.</summary>
        /// <param name="context">The context provided by Eclipse. Type: <see cref="VMS.TPS.Common.Model.API.ScriptContext"/>.</param>
        /// <returns>The result of the test in terms of a single boolean variable, resulting from a logical AND of all individual checks (<c>true</c> = pass, <c>false</c> = fail). Type: <see cref="System.Boolean"/>.</returns>
        public static bool StringRepresentation(ScriptContext context)
        {

            // Arrange
            double _testDose = 25.5;
            DoseValue.DoseUnit
                _testDoseUnitAbs = DoseValue.DoseUnit.Gy,
                _testDoseUnitRel = DoseValue.DoseUnit.Percent;
            DoseValue
                D_ABS = new DoseValue(_testDose, _testDoseUnitAbs),
                D_REL = new DoseValue(_testDose, _testDoseUnitRel);
            Structure _testStructure = context.StructureSet.Structures.FirstOrDefault();
            StructureType _testStructureType = StructureType.OAR;
            ComparisonMode
                _testComparerLT = ComparisonMode.lessThan,
                _testComparerLE = ComparisonMode.lessEqual,
                _testComparerGT = ComparisonMode.greaterThan,
                _testComparerGE = ComparisonMode.greaterEqual;
            string
                _testString1 = "Dmax<25.5Gy",
                _testString2 = "Dmax<=25.5%",
                _testString3 = "Dmax>25.5Gy",
                _testString4 = "Dmax>=25.5%",
                _testString5startsWith = "Dmax(",
                _testString5endsWith = ")<25.5Gy",
                _testString6startsWith = "Dmax(",
                _testString6endsWith = ")<=25.5%",
                _testString7startsWith = "Dmax(",
                _testString7endsWith = ")>25.5Gy",
                _testString8startsWith = "Dmax(",
                _testString8endsWith = ")>=25.5%";

            // Act
            Constraint_Dmax
                c1 = new Constraint_Dmax(D_ABS, _testStructureType, _testComparerLT),
                c2 = new Constraint_Dmax(D_REL, _testStructureType, _testComparerLE),
                c3 = new Constraint_Dmax(D_ABS, _testStructureType, _testComparerGT),
                c4 = new Constraint_Dmax(D_REL, _testStructureType, _testComparerGE),
                c5 = new Constraint_Dmax(D_ABS, _testStructure, _testStructureType, _testComparerLT),
                c6 = new Constraint_Dmax(D_REL, _testStructure, _testStructureType, _testComparerLE),
                c7 = new Constraint_Dmax(D_ABS, _testStructure, _testStructureType, _testComparerGT),
                c8 = new Constraint_Dmax(D_REL, _testStructure, _testStructureType, _testComparerGE);

            // Assert
            return
                c1.ToString() == _testString1 &&
                c2.ToString() == _testString2 &&
                c3.ToString() == _testString3 &&
                c4.ToString() == _testString4 &&
                c5.ToString().StartsWith(_testString5startsWith) && c5.ToString().EndsWith(_testString5endsWith) &&
                c6.ToString().StartsWith(_testString6startsWith) && c6.ToString().EndsWith(_testString6endsWith) &&
                c7.ToString().StartsWith(_testString7startsWith) && c7.ToString().EndsWith(_testString7endsWith) &&
                c8.ToString().StartsWith(_testString8startsWith) && c8.ToString().EndsWith(_testString8endsWith);

        }
        
    }

}

// TODO: under construction...