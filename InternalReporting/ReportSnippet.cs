namespace SCFM
{

    /// <summary>Model of the report that SCFM tests issue.</summary>
    /// <remarks>SCFM tests should not send information directly to the user! This is a task for the GUI.</remarks>
    public class ReportSnippet
    {

        #region PROPERTIES: list of items of the report of the test outcomes.

        /// <summary>Test identification.</summary>
        /// <value>Auto-implemented property.</value>
        /// <remarks>See <see cref="SCFM.SCFMtests"/></remarks>
        public SCFMtests Test { get; set; }

        /// <summary>Test score.</summary>
        /// <value>Auto-implemented property.</value>
        /// <remarks>See <see cref="SCFM.InfoLevel"/>.</remarks>
        public InfoLevel Level { get; set; }

        /// <summary>Test message.</summary>
        /// <value>Auto-implemented property.</value>
        /// <remarks>Write short string messages!</remarks>
        public string Message { get; set; }

        /// <summary>Object (source) that issued the report.</summary>
        /// <value>Auto-implemented property.</value>
        public object Source { get; set; }

        /// <summary>Current value of the assessed parameter.</summary>
        /// <value>Auto-implemented property.</value>
        public object CurrentValue { get; set; }

        /// <summary>Units of the assessed parameter.</summary>
        /// <value>Auto-implemented property.</value>
        /// <remarks>If the parameter has no units, this property must remain uninitalized (<c>null</c>).</remarks>
        public string ValueUnits { get; set; }

        /// <summary>Reference value of the assessed parameter.</summary>
        /// <value>Auto-implemented property.</value>
        /// <remarks>If reference value is a meaningless concept for this test, this property must remain uninitalized (<c>null</c>).</remarks>
        public object ReferenceValue { get; set; }

        /// <summary>Tolerance value of the assessed parameter. If a two-fold/range tolerance concept applies, it is the "soft" tolerance.</summary>
        /// <value>Auto-implemented property.</value>
        /// <remarks>
        /// If tolerance value is a meaningless concept for this test, this property must remain uninitalized (<c>null</c>).
        /// If a single tolerance value applies for this test, <see cref="ToleranceSoft"/> and <see cref="ToleranceHard"/> properties must be equal.
        /// </remarks>
        public object ToleranceSoft { get; set; }

        /// <summary>Tolerance value of the assessed parameter. If a two-fold/range tolerance concept applies, it is the "hard" tolerance.</summary>
        /// <value>Auto-implemented property.</value>
        /// <remarks>
        /// If tolerance value is a meaningless concept for this test, this property must remain uninitalized (<c>null</c>).
        /// If a single tolerance value applies for this test, <see cref="ToleranceSoft"/> and <see cref="ToleranceHard"/> properties must be equal.
        /// </remarks>
        public object ToleranceHard { get; set; }

        /// <summary>Units of the tolerance of the assessed parameter.</summary>
        /// <value>Auto-implemented property.</value>
        /// <remarks>
        /// If the tolerance has no units, this property must remain uninitalized (<c>null</c>).
        /// Tolerance units are usually the same than parameter units, but in particular situations they may differ (e.g., tolerance expressed as a percentage around a reference value).
        /// </remarks>
        public string ToleranceUnits { get; set; }

        /// <summary>Comparison mode between the current value and the reference/tolerance value(s).</summary>
        /// <value>Auto-implemented property.</value>
        /// <remarks>See <see cref="SCFM.ComparisonMode"/>.</remarks>
        public ComparisonMode Comparison { get; set; }

        #endregion

        #region CONSTRUCTORS

        /// <summary>Default parameterless constructor.</summary>
        /// <remarks>It is recommended when the object properties are not known a priori.</remarks>
        public ReportSnippet() { }

        /// <summary>Constructor with the most reduced set of parameters. No reference nor tolerances values are provided.</summary>
        /// <param name="test">Test identification. Type: <see cref="SCFM.SCFMtests"/>.</param>
        /// <param name="level">Test score. Type: <see cref="SCFM.InfoLevel"/>.</param>
        /// <param name="msg">Test message. Type: <see cref="System.String"/>.</param>
        /// <param name="s">Object (source) that issued the current report. Type: <see cref="System.Object"/>.</param>
        /// <param name="current">Current value of the assessed parameter. Type: <see cref="System.Object"/>.</param>
        /// <param name="unit">Units of the assessed parameter. Type: <see cref="System.String"/>.</param>
        /// <remarks>It is recommended when the reporting event does not support reference or tolerance concepts.</remarks>
        public ReportSnippet(SCFMtests test, InfoLevel level, string msg, object s, object current, string unit)
        {
            this.Test = test;
            this.Level = level;
            this.Message = msg;
            this.Source = s;
            this.CurrentValue = current;
            this.ValueUnits = unit;
        }

        /// <summary>Constructor with the most reduced set of parameters, plus a reference value along with the comparison mode (tolerance concept does not apply).</summary>
        /// <param name="test">Test identification. Type: <see cref="SCFM.SCFMtests"/>.</param>
        /// <param name="level">Test score. Type: <see cref="SCFM.InfoLevel"/>.</param>
        /// <param name="msg">Test message. Type: <see cref="System.String"/>.</param>
        /// <param name="s">Object (source) that issued the current report. Type: <see cref="System.Object"/>.</param>
        /// <param name="current">Current value of the assessed parameter. Type: <see cref="System.Object"/>.</param>
        /// <param name="reference">Reference value of the assessed parameter. Type: <see cref="System.Object"/>.</param>
        /// <param name="unit">Units of the assessed parameter. Type: <see cref="System.String"/>.</param>
        /// <param name="comparison">Comparison mode between the current value and the reference value. Type: <see cref="SCFM.ComparisonMode"/>.</param>
        /// <remarks>It is recommended when the reporting event only supports reference concept.</remarks>
        public ReportSnippet(SCFMtests test, InfoLevel level, string msg, object s, object current, object reference, string unit, ComparisonMode comparison)
        {
            this.Test = test;
            this.Level = level;
            this.Message = msg;
            this.Source = s;
            this.CurrentValue = current;
            this.ValueUnits = unit;
            this.ReferenceValue = reference;
            this.Comparison = comparison;
        }

        /// <summary>Constructor with the most reduced set of parameters, plus a tolerance(s) value(s) along with the comparison mode (reference concept does not apply).</summary>
        /// <param name="test">Test identification. Type: <see cref="SCFM.SCFMtests"/>.</param>
        /// <param name="level">Test score. Type: <see cref="SCFM.InfoLevel"/>.</param>
        /// <param name="msg">Test message. Type: <see cref="System.String"/>.</param>
        /// <param name="s">Object (source) that issued the current report. Type: <see cref="System.Object"/>.</param>
        /// <param name="current">Current value of the assessed parameter. Type: <see cref="System.Object"/>.</param>
        /// <param name="unit">Units of the assessed parameter. Type: <see cref="System.String"/>.</param>
        /// <param name="tolSoft">Tolerance value of the assessed parameter. If a two-fold/range tolerance concept applies, it is the "soft" tolerance. Type: <see cref="System.Object"/>.</param>
        /// <param name="tolHard">Tolerance value of the assessed parameter. If a two-fold/range tolerance concept applies, it is the "hard" tolerance. Type: <see cref="System.Object"/>.</param>
        /// <param name="tolUnit">Tolerance units. May or may not differ from parameter units. Type: <see cref="System.String"/>.</param>
        /// <param name="comparison">Comparison mode between the current value and the tolerance value. Type: <see cref="SCFM.ComparisonMode"/>.</param>
        /// <remarks>It is recommended when the reporting event only supports tolerance concept.</remarks>
        public ReportSnippet(SCFMtests test, InfoLevel level, string msg, object s, object current, string unit, object tolSoft, object tolHard, string tolUnit, ComparisonMode comparison)
        {
            this.Test = test;
            this.Level = level;
            this.Message = msg;
            this.Source = s;
            this.CurrentValue = current;
            this.ValueUnits = unit;
            this.ToleranceSoft = tolSoft;
            this.ToleranceHard = tolHard;
            this.ToleranceUnits = tolUnit;
            this.Comparison = comparison;
        }

        /// <summary>Constructor that initializes all properties, when both reference and tolerance concepts apply.</summary>
        /// <param name="test">Test identification. Type: <see cref="SCFM.SCFMtests"/>.</param>
        /// <param name="level">Test score. Type: <see cref="SCFM.InfoLevel"/>.</param>
        /// <param name="msg">Test message. Type: <see cref="System.String"/>.</param>
        /// <param name="s">Object (source) that issued the current report. Type: <see cref="System.Object"/>.</param>
        /// <param name="current">Current value of the assessed parameter. Type: <see cref="System.Object"/>.</param>
        /// <param name="reference">Reference value of the assessed parameter. Type: <see cref="System.Object"/>.</param>
        /// <param name="unit">Units of the assessed parameter. Type <see cref="System.String"/>.</param>
        /// <param name="tolSoft">Tolerance value of the assessed parameter. If a two-fold/range tolerance concept applies, it is the "soft" tolerance. Type: <see cref="System.Object"/>.</param>
        /// <param name="tolHard">Tolerance value of the assessed parameter. If a two-fold/range tolerance concept applies, it is the "hard" tolerance. Type: <see cref="System.Object"/>.</param>
        /// <param name="tolUnit">Tolerance units. May or may not differ from parameter units. Type: <see cref="System.String"/>.</param>
        /// <param name="comparison">Comparison mode between the current value and the tolerance around the reference value. Type: <see cref="SCFM.ComparisonMode"/>.</param>
        /// <remarks>It is recommended when the reporting event supports both reference and tolerance concepts.</remarks>
        public ReportSnippet(SCFMtests test, InfoLevel level, string msg, object s, object current, object reference, string unit, object tolSoft, object tolHard, string tolUnit, ComparisonMode comparison)
        {
            this.Test = test;
            this.Level = level;
            this.Message = msg;
            this.Source = s;
            this.CurrentValue = current;
            this.ReferenceValue = reference;
            this.ValueUnits = unit;
            this.ToleranceSoft = tolSoft;
            this.ToleranceHard = tolHard;
            this.ToleranceUnits = tolUnit;
            this.Comparison = comparison;
        }

        #endregion

        #region OVERRIDING .NET BASE CLASS LIBRARY METHODS

        /// <summary>Written version of the report issued by the current test.</summary>
        /// <returns>The written version of each property of <c>this</c> object (one line per property). Type: <see cref="System.String"/>.</returns>
        /// <remarks>Overrided version of the method <see cref="System.Object.ToString"/> of the .NET base class library.</remarks>
        public override string ToString()
        {
            return
                "SCFMtests: " + this.Test.ToString() +
                "\nInfoLevel: " + this.Level.ToString() +
                "\nMessage: " + this.Message +
                "\nSource: " + this.Source.ToString() +
                "\nCurrent value: " + this.CurrentValue.ToString() +
                "\nValue units: " + this.ValueUnits.ToString() +
                "\nReference value: " + this.ReferenceValue.ToString() +
                "\nTolerance (\"soft\"): " + this.ToleranceSoft.ToString() +
                "\nTolerance (\"hard\"): " + this.ToleranceHard.ToString() +
                "\nTolerance units: " + this.ToleranceUnits +
                "\nComparison mode: " + this.Comparison.ToString();
        }

        #endregion

    }

}