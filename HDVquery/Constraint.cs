using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;


namespace SCFM
{

    /// <summary>Model of a constraint of an OAR/PTV/other.</summary>
    /// <remarks>
    /// Abstract class: actual constraints are modeled by of one of the following derived classes:
    /// <list type="bullet">
    /// <item><see cref="Constraint_Dmax"/>: contraint of type "maximum dose" (Dmax).</item>
    /// <item><see cref="Constraint_Dmean"/>: constraint of type "mean dose" (Dmean).</item>
    /// <item><see cref="Constraint_DoseAtVolume"/>: constraint of type "dose at volume" (D_V).</item>
    /// <item><see cref="Constraint_VolumeAtDose"/>: constraint of type "volume at dose" (V_D).</item>
    /// </list>
    /// </remarks>
    public abstract class Constraint
    {

        # region BACKING-FIELDS

        private DoseValue _dose;
        private double? _volume;
        private ComparisonMode _comparer;

        #endregion

        #region GENERAL PROPERTIES

        /// <summary>Property that holds the underlying structure of the constraint.</summary>
        /// <value><see cref="VMS.TPS.Common.Model.API.Structure"/> (auto-implemented property).</value>
        /// <remarks>This property may not be initialized when instantiating a particular constraint, but it is mandatory for HDV queries.</remarks>
        public Structure Structure { get; set; }

        /// <summary>Property that holds the type of the underlying structure (OAR/PTV/other).</summary>
        /// <value><see cref="SCFM.StructureType"/> (auto-implemented property).</value>
        /// <remarks>Protected setter to force initialization through the constructor of the derived classes.</remarks>
        public StructureType StructureType { get; protected set; }

        /// <summary>Property that holds the comparison mode between the actual value and the constraint value.</summary>
        /// <value><see cref="SCFM.ComparisonMode"/> <see cref="_comparer"/>.</value>
        /// <remarks>
        /// Only equal, greater equal, greater than, less equal or less than comparison modes are supported.
        /// Protected setter to force initialization through the constructor of the derived classes.
        /// </remarks>
        /// <exception cref="System.ArgumentOutOfRangeException">Unsupported comparison mode.</exception>
        public ComparisonMode ComparisonMode
        {
            get { return _comparer; }
            protected set
            {
                if (value == SCFM.ComparisonMode.equal ||
                    value == SCFM.ComparisonMode.greaterEqual ||
                    value == SCFM.ComparisonMode.greaterThan ||
                    value == SCFM.ComparisonMode.lessEqual ||
                    value == SCFM.ComparisonMode.lessThan)
                    _comparer = value;
                else
                    throw new ArgumentOutOfRangeException("value (comparisonMode)", value, "SCFM exception:\nUnsupported comparison mode.");
            }
        }

        /// <summary>Property that holds the symbol of the comparison mode of the constraint.</summary>
        /// <value><see cref="System.String"/> <see cref="ComparisonSymbol"/>.</value>
        /// <remarks>Read-only property because the symbol is determined by <see cref="ComparisonMode"/>.</remarks>
        public string ComparisonSymbol
        {
            get
            {
                string symbol = "";
                switch (ComparisonMode)
                {
                    case ComparisonMode.equal:
                        symbol = "=";
                        break;
                    case ComparisonMode.greaterEqual:
                        symbol = ">=";
                        break;
                    case ComparisonMode.greaterThan:
                        symbol = ">";
                        break;
                    case ComparisonMode.lessEqual:
                        symbol = "<=";
                        break;
                    case ComparisonMode.lessThan:
                        symbol = "<";
                        break;
                }
                return symbol;
            }
        }

        #endregion

        #region "DOSE/VOLUME" PROPERTIES

        /// <summary>Property that holds the dose of the constraint.</summary>
        /// <value><see cref="VMS.TPS.Common.Model.Types.DoseValue"/> <see cref="_dose"/></value>
        /// <remarks>
        /// Only postivie values are allowed, either relative (in %) or absolute (in Gy). Other units (cGy or Unknown) are not allowed.
        /// Protected setter to force initialization through the constructor of the derived classes.
        /// </remarks>
        /// <exception cref="System.ArgumentOutOfRangeException"><c>value.Dose</c> is not positive.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><c>value.Unit</c> is not "Gy" nor "Percent".</exception>
        public DoseValue Dose
        {
            get { return _dose; }
            protected set
            {
                if (value.Unit == DoseValue.DoseUnit.Gy || value.Unit == DoseValue.DoseUnit.Percent)
                {
                    if (value.Dose > 0.0)
                        _dose = value;
                    else
                        throw new ArgumentOutOfRangeException("value.Dose (D)", value.Dose, "SCFM exception:\nRange: D > 0.");
                }
                else
                    throw new ArgumentOutOfRangeException("value.Unit ([D])", value.Unit, "SCFM exception:\nRange: [D] = Gy or [D] = Percent");
            }
        }

        /// <summary>Property that holds the volume of the constraint.</summary>
        /// <value><see cref="System.Nullable"/> of type <see cref="System.Double"/> <see cref="_volume"/></value>
        /// <remarks>
        /// Only positive values are allowed (or <c>null</c>). Virtual property allows derived classes to implement further input parameter checkings.
        /// Protected setter to force initialization through the constructor of the derived classes.
        /// </remarks>
        /// <exception cref="System.ArgumentOutOfRangeException"><c>value</c> is not positive (or <c>null</c>).</exception>
        public virtual double? Volume
        {
            get { return _volume; }
            protected set
            {
                if (value > 0.0 || value == null)
                    _volume = value;
                else
                    throw new ArgumentOutOfRangeException("value (V)", value, "SCFM exception:\nRange: V > 0.");
            }
        }

        /// <summary>Property that holds the units of the volume of the constraint.</summary>
        /// <value><see cref="System.Nullable"/> of type <see cref="VMS.TPS.Common.Model.Types.VolumePresentation"/> (auto-implemented property).</value>
        /// <remarks>
        /// Virtual property allows derived classes to implement further input parameter checkings.
        /// Protected setter to force initialization through the constructor of the derived classes.
        /// </remarks>
        public virtual VolumePresentation? VolumeUnits { get; protected set; }

        #endregion

        #region ABSTRACT "QUERY VALUES" PROPERTIES

        /// <summary>Property that holds the text label that identifies the constraint.</summary>
        /// <value><see cref="System.String"/> <see cref="Label"/>.</value>
        /// <remarks>Read-only property because label depends only on the derived type.</remarks>
        public abstract string Label { get; }

        /// <summary>Property that holds the index of the constraint (queried value).</summary>
        /// <value><see cref="System.Nullable"/> of type <see cref="System.Double"/> <see cref="Index"/>.</value>
        /// <remarks>Read-only property because index, either dose or volume, depends only on the derived type.</remarks>
        public abstract double? Index { get; }

        /// <summary>Property that holds the units of the index of the constraint.</summary>
        /// <value><see cref="System.String"/> <see cref="IndexUnits"/>.</value>
        /// <remarks>Read-only property because index units, either dose or volume, depend only on the derived the type.</remarks>
        public abstract string IndexUnits { get; }

        /// <summary>Property that holds the threshold of the constraint (constraint value).</summary>
        /// <value><see cref="System.Double"/> <see cref="Threshold"/>.</value>
        /// <remarks>Read-only property because threshold, either dose or volume, depends only on the derived type.</remarks>
        public abstract double Threshold { get; }

        /// <summary>Property that holds the units of the threshold of the constraint.</summary>
        /// <value><see cref="System.String"/> <see cref="ThresholdUnits"/>.</value>
        /// <remarks>Read-only property because threshold units, either dose or volume, depend only on the derived type.</remarks>
        public abstract string ThresholdUnits { get; }

        #endregion

        #region PUBLIC METHODS: DOSIMETRIC REPORTING

        /// <summary>Method that assesses if the current structure in the current treatment plan fulfills the provided clinical constraints.</summary>
        /// <param name="myPlan">The current plan. Type: <see cref="VMS.TPS.Common.Model.API.PlanningItem"/>.</param>
        /// <param name="myConstraints">(Optional parameter: only needed if more than one constraint (apart from <c>this</c>) applies to the current OAR/PTV/other) The full list of constraints (including <c>this</c>) of the current OAR/PTV/other. Generic type: <see cref="System.Collections.Generic.List{T}"/> of type <see cref="SCFM.Constraint"/>.</param>
        /// <param name="cumulativePrescriptionDose">(Optional parameter: only needed if <paramref name="myPlan"/> is of type <see cref="VMS.TPS.Common.Model.API.PlanSum"/> and a relative dose assessment is required). The maximum cumulative prescribed dose resulting from the sum of individual prescription doses.</param>
        /// <returns>A report of the assessment. Type: <see cref="SCFM.ReportSnippet"/>.</returns>
        /// <remarks>
        /// This method accepts either plans of type <see cref="VMS.TPS.Common.Model.API.PlanSetup"/> or <see cref="VMS.TPS.Common.Model.API.PlanSum"/>. In the latter case, the optional parameter <paramref name="cumulativePrescriptionDose"/> must be provided if dose assessment is performed in relative units (%), in order to calculate percentage with respect that maximum cumulative dose.
        /// If the optional parameter <paramref name="myConstraints"/> is not provided, this method assesses <c>this</c> constraint only. If it is provided, the assessment is two-fold: both with respect to the least and the most restrictive constraints among the constraints of the list with coincident type, <c>Dose.Unit</c>, <see cref="VolumeUnits"/> and <see cref="Index"/> than <c>this</c>.
        /// </remarks>
        /// <exception cref="System.ArgumentException">The optional parameter <paramref name="cumulativePrescriptionDose"/> was expected but it was not provided (relative dose (%) assessment in a plan sum), or it was not expected but it was provided (in the remaining situations).</exception>
        /// <exception cref="System.InvalidOperationException">The constraint has no structure defined.</exception>
        public ReportSnippet GetReport(PlanningItem myPlan, List<Constraint> myConstraints = null, double cumulativePrescriptionDose = 0.0)
        {
            if (this.Structure == null)
                throw new InvalidOperationException("SCFM exception:\nThe constraint has no structure defined.");
            if (myPlan is PlanSum && this.Dose.Unit == DoseValue.DoseUnit.Percent)
            {
                if (cumulativePrescriptionDose == 0.0)
                    throw new ArgumentException("SCFM exception:\nA maximum cumulative prescription dose (in Gy) is required to assess a relative dose (%) in a plan sum.",
                                                "cumulativePrescriptionDose");
            }
            else
            {
                if (cumulativePrescriptionDose != 0.0)
                    throw new ArgumentException("SCFM exception:\nA maximum cumulative prescription dose was unnecessary provided.",
                                                "cumulativePrescriptionDose");
            }
            double[] thresholds;
            if (myConstraints == null || !getDuplicateThresholds(myConstraints, out thresholds))
                return assessConstraint(getCurrentValue(myPlan, cumulativePrescriptionDose));
            else
                return assessConstraint(getCurrentValue(myPlan, cumulativePrescriptionDose), thresholds[0], thresholds[1]);
        }

        #endregion

        #region PROTECTED METHODS: ENABLE POLYMORPHISM FOR DOSIMETRIC REPORTING

        /// <summary>This method retrieves the HDV value of the current OAR/PTV/other for the current treatment plan (to compare against <c>this</c> constraint).</summary>
        /// <param name="myPlan">The current plan. Type: <see cref="VMS.TPS.Common.Model.API.PlanningItem"/>.</param>
        /// <param name="cumulativePrescriptionDose">(Optional parameter: only needed if <paramref name="myPlan"/> is of type <see cref="VMS.TPS.Common.Model.API.PlanSum"/> and a relative dose assessment is required). The maximum cumulative prescribed dose resulting from the sum of individual prescription doses.</param>
        /// <returns>The HDV value of the current OAR/PTV/other for the current treatment plan (to compare against <c>this</c> constraint). The HDV is evaluated as <c>this</c> constraint: maintaining dose/volume representation (absolute/relative) and units. Type: <see cref="System.Double"/>.</returns>
        /// <remarks>
        /// Abstract method to force a specific implementation in each derived class.
        /// This method accepts either plans of type <see cref="VMS.TPS.Common.Model.API.PlanSetup"/> or <see cref="VMS.TPS.Common.Model.API.PlanSum"/>. In the latter case, the optional parameter <paramref name="cumulativePrescriptionDose"/> must be provided if dose assessment is performed in relative units (%), in order to calculate percentage with respect that maximum cumulative dose.
        /// The optional parameter is not an issue for client code because the accessibility level of this method is protected. See the caller method (<see cref="SCFM.Constraint.GetReport"/>) for more information.
        /// </remarks>
        protected abstract double getCurrentValue(PlanningItem myPlan, double cumulativePrescriptionDose = 0.0);

        #endregion

        #region PRIVATE METHODS: INTERNAL STUFF

        /// <summary>This method searches for redundant constraints to <c>this</c> among a list of constraints.</summary>
        /// <param name="myConstraints">The full list of constraints (including <c>this</c>). Generic type: <see cref="System.Collections.Generic.List{T}"/> of type <see cref="SCFM.Constraint"/>.</param>
        /// <param name="myThresholds">(<c>out</c> parameter) Minimum and maximum thresholds among redunant constraints to <c>this</c>. In case of no redundant constraints, minimum and maximum values are equal to <see cref="Threshold"/>. Type: <see cref="System.Array"/> of <see cref="System.Double"/>.</param>
        /// <returns><c>true</c> if at least one redundant constraint is found or <c>false</c> otherwise. Type: <see cref="System.Boolean"/>.</returns>
        /// <remarks>
        /// A constraint is considered redundant to <c>this</c> if, and only if, the following parameters are the same:
        /// <list type="bullet">
        /// <item><see cref="Structure"/></item>
        /// <item>Type.</item>
        /// <item><c>Dose.Unit</c></item>
        /// <item><see cref="VolumeUnits"/></item>
        /// <item><see cref="Index"/></item>
        /// <item><see cref="ComparisonMode"/></item>
        /// </list>
        /// it has coincident type, <c>Dose.Unit</c>, <see cref="VolumeUnits"/> and <see cref="Index"/>.
        /// Minimum or maximum values (or both, if no redundant constraints are found) may coincide with <see cref="Threshold"/>.
        /// </remarks>
        private bool getDuplicateThresholds(List<Constraint> myConstraints, out double[] myThresholds)
        {
            IEnumerable<Constraint> duplicates = from c in myConstraints
                                                 where
                                                 c.Structure == this.Structure &&
                                                 c.GetType() == this.GetType() &&
                                                 c.Dose.Unit == this.Dose.Unit &&
                                                 c.VolumeUnits == this.VolumeUnits &&
                                                 c.Index == this.Index &&
                                                 c.ComparisonMode == this.ComparisonMode
                                                 select c;
            myThresholds = new double[] { duplicates.Min(c => c.Threshold), duplicates.Max(c => c.Threshold) };
            return myThresholds[0] != myThresholds[1];
        }

        /// <summary>Method that assess if the current structure in the current treatment plan fulfills <c>this</c> constraint.</summary>
        /// <param name="currentValue">Current HDV value for the current structure for the current treatment plan. Type: <see cref="System.Double"/>.</param>
        /// <returns>A report of the assessment. Type: <see cref="SCFM.ReportSnippet"/>.</returns>
        /// <remarks>
        /// <paramref name="currentValue"/> units (either absolute or relative) must match to <see cref="ThresholdUnits"/>. WARNING: since <paramref name="currentValue"/> units are not traceable, coherent units must be provided.
        /// The object of type <see cref="SCFM.ReportSnippet"/> is instantiated by using the constructor which takes into acount the concept of two-fold tolerances. Both tolerance levels are ascribed to <see cref="Threshold"/>.
        /// </remarks>
        private ReportSnippet assessConstraint(double currentValue)
        {
            string criteria = "";
            InfoLevel level = InfoLevel.undefined;
            switch (this.ComparisonMode)
            {
                case ComparisonMode.greaterThan:
                    if (currentValue > this.Threshold)
                    {
                        criteria = "PASS";
                        level = InfoLevel.GREEN;
                    }
                    else
                    {
                        criteria = "FAIL";
                        level = InfoLevel.RED;
                    }
                    break;
                case ComparisonMode.greaterEqual:
                    if (currentValue >= this.Threshold)
                    {
                        criteria = "PASS";
                        level = InfoLevel.GREEN;
                    }
                    else
                    {
                        criteria = "FAIL";
                        level = InfoLevel.RED;
                    }
                    break;
                case ComparisonMode.lessThan:
                    if (currentValue < this.Threshold)
                    {
                        criteria = "PASS";
                        level = InfoLevel.GREEN;
                    }
                    else
                    {
                        criteria = "FAIL";
                        level = InfoLevel.RED;
                    }
                    break;
                case ComparisonMode.equal:
                    if (currentValue == this.Threshold)
                    {
                        criteria = "PASS";
                        level = InfoLevel.GREEN;
                    }
                    else
                    {
                        criteria = "FAIL";
                        level = InfoLevel.RED;
                    }
                    break;
            }
            string msg = string.Format("{0}{1}{2} = {3:F1}{4}\t{5} ({6}{7}{8})",
                this.Label, this.Index, this.IndexUnits,
                currentValue, this.ThresholdUnits,
                criteria, this.ComparisonSymbol,
                this.Threshold, this.ThresholdUnits);
            return new ReportSnippet(SCFMtests.constraintCheck, level, msg, this,
                currentValue, this.ThresholdUnits,
                this.Threshold, this.Threshold, this.ThresholdUnits, this.ComparisonMode);
        }

        /// <summary>Method that assess if the current structure in the current treatment plan fulfills the lowermost and/or uppermost provided thresholds.</summary>
        /// <param name="currentValue">Current HDV value for the current structure for the current treatment plan. Type: <see cref="System.Double"/>.</param>
        /// <param name="lowerThreshold">Lowermost threshold. It may be equal, or not, to <see cref="Threshold"/>. Type: <see cref="System.Double"/>.</param>
        /// <param name="upperThreshold">Uppermost threshold. It may be equal, or not, to <see cref="Threshold"/>. Type: <see cref="System.Double"/>.</param>
        /// <returns>A report of the assessment if <c>this</c> corresponds to the lowermost threshold, or <c>null</c> otherwise (see remarks). Type: <see cref="SCFM.ReportSnippet"/>.</returns>
        /// <remarks>
        /// <paramref name="currentValue"/> units (either absolute or relative) must match to <see cref="ThresholdUnits"/>. WARNING: since <paramref name="currentValue"/> units are not traceable, coherent units must be provided.
        /// Typically, every constraint of a list of constrains issues an individual report. In case of redundant constraints, and in order to avoid redundant reports, only the lowermost representative of the redundant thresholds isses a report.
        /// The object of type <see cref="SCFM.ReportSnippet"/> is instantiated by using the constructor which takes into account the concept of two-fold tolerances. "hard"/"soft" tolerance levels are ascribed to <paramref name="lowerThreshold"/>/<paramref name="upperThreshold"/> depending upong <see cref="ComparisonMode"/>.
        /// </remarks>
        private ReportSnippet assessConstraint(double currentValue, double lowerThreshold, double upperThreshold)
        {
            if (this.Threshold != lowerThreshold)
                return null;
            string criteria = "";
            InfoLevel level = InfoLevel.undefined;
            double softConstraint = 0.0, hardConstraint = 0.0;
            switch (this.ComparisonMode)
            {
                case ComparisonMode.greaterThan:
                    softConstraint = upperThreshold;
                    hardConstraint = lowerThreshold;
                    if (currentValue > softConstraint)
                    {
                        criteria = "PASS";
                        level = InfoLevel.GREEN;
                    }
                    else if (currentValue > hardConstraint)
                    {
                        criteria = "WARNING";
                        level = InfoLevel.YELLOW;
                    }
                    else
                    {
                        criteria = "FAIL";
                        level = InfoLevel.RED;
                    }
                    break;
                case ComparisonMode.greaterEqual:
                    softConstraint = upperThreshold;
                    hardConstraint = lowerThreshold;
                    if (currentValue >= softConstraint)
                    {
                        criteria = "PASS";
                        level = InfoLevel.GREEN;
                    }
                    else if (currentValue >= hardConstraint)
                    {
                        criteria = "WARNING";
                        level = InfoLevel.YELLOW;
                    }
                    else
                    {
                        criteria = "FAIL";
                        level = InfoLevel.RED;
                    }
                    break;
                case ComparisonMode.lessThan:
                    softConstraint = lowerThreshold;
                    hardConstraint = upperThreshold;
                    if (currentValue < softConstraint)
                    {
                        criteria = "PASS";
                        level = InfoLevel.GREEN;
                    }
                    else if (currentValue < hardConstraint)
                    {
                        criteria = "WARNING";
                        level = InfoLevel.YELLOW;
                    }
                    else
                    {
                        criteria = "FAIL";
                        level = InfoLevel.RED;
                    }
                    break;
                case ComparisonMode.lessEqual:
                    softConstraint = lowerThreshold;
                    hardConstraint = upperThreshold;
                    if (currentValue <= softConstraint)
                    {
                        criteria = "PASS";
                        level = InfoLevel.GREEN;
                    }
                    else if (currentValue <= hardConstraint)
                    {
                        criteria = "WARNING";
                        level = InfoLevel.YELLOW;
                    }
                    else
                    {
                        criteria = "FAIL";
                        level = InfoLevel.RED;
                    }
                    break;
            }
            string msg = string.Format("{0}{1}{2} = {3:F1}{4}\t{5} ({6}{7}-{8}{9})",
                this.Label, this.Index, this.IndexUnits,
                currentValue, this.ThresholdUnits,
                criteria, this.ComparisonSymbol,
                lowerThreshold, upperThreshold, this.ThresholdUnits);
            return new ReportSnippet(SCFMtests.constraintCheck, level, msg, this,
                currentValue, this.ThresholdUnits,
                softConstraint, hardConstraint, this.ThresholdUnits, this.ComparisonMode);
        }

        #endregion

        #region OVERRIDING .NET BASE CLASS LIBRARY METHODS

        /// <summary>Written representation of the constraint in compact notation.</summary>
        /// <returns>The string sequence: "[Label][Index][IndexUnits](Structure.Id)[ComparisonSymbol][Threshold][ThresholdUnits]". Type: <see cref="System.String"/>.</returns>
        /// <remarks>Overrided version of the method <see cref="System.Object.ToString"/> of the .NET base class library.</remarks>
        public override string ToString()
        {
            string structureID = "";
            if (this.Structure != null)
                structureID = "(" + this.Structure.Id + ")";
            return
                this.Label +
                this.Index +
                this.IndexUnits +
                structureID +
                this.ComparisonSymbol +
                this.Threshold +
                this.ThresholdUnits;
        }

        #endregion

    }

}