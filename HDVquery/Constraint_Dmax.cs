using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;


namespace SCFM
{

    /// <summary>Model of a constraint of an OAR/PTV/other of type "maximum dose" (Dmax).</summary>
    /// <remarks>Sealed class. Constraints of type "Dmax" do not need further specialization.</remarks>
    public sealed class Constraint_Dmax : Constraint
    {

        #region OVERRIDING "DOSE/VOLUME" PROPERTIES

        /// <summary>Property that holds the volume of the constraint.</summary>
        /// <value><see cref="System.Nullable"/> of type <see cref="System.Double"/> <c>base.Volume</c></value>
        /// <remarks>
        /// Overrided version of the base class property: <see cref="Volume"/> is <c>null</c> by definition.
        /// Protected setter to force initialization through the constructor.
        /// </remarks>
        /// <exception cref="System.ArgumentOutOfRangeException"><c>value</c> is not <c>null</c>.</exception>
        public override double? Volume
        {
            get { return base.Volume; }
            protected set
            {
                if (value == null)
                    base.Volume = value;
                else
                    throw new ArgumentOutOfRangeException("value (V)", value, "SCFM exception:\nRange(Dmax): V = null.");
            }
        }

        /// <summary>Property that holds the units of the volume of the constraint.</summary>
        /// <value><see cref="System.Nullable"/> of type <see cref="VMS.TPS.Common.Model.Types.VolumePresentation"/> <c>base.VolumeUnits</c>.</value>
        /// <remarks>
        /// Overrided version of the base class property: <see cref="VolumeUnits"/> is <c>null</c> by definition.
        /// Protected setter to force initialization through the constructor.
        /// </remarks>
        /// <exception cref="System.ArgumentOutOfRangeException"><c>value</c> is not <c>null</c>.</exception>
        public override VolumePresentation? VolumeUnits
        {
            get { return base.VolumeUnits; }
            protected set
            {
                if (value == null)
                    base.VolumeUnits = value;
                else
                    throw new ArgumentOutOfRangeException("value ([V])", value, "SCFM exception:\nRange(Dmax): [V] = null.");
            }
        }

        #endregion

        #region OVERRIDING "QUERY VALUES" PROPERTIES

        /// <summary>Property that holds the text label that identifies the constraint.</summary>
        /// <value><see cref="System.String"/> <see cref="Label"/>.</value>
        /// <remarks>Read-only property because label is determined by the type: "Dmax" for this type.</remarks>
        public override string Label
        {
            get { return "Dmax"; }
        }

        /// <summary>Property that holds the index of the constraint (queried value).</summary>
        /// <value><see cref="System.Nullable"/> of type <see cref="System.Double"/> <see cref="Index"/>.</value>
        /// <remarks>Read-only property because index is determined by the type: <c>null</c> for this type.</remarks>
        public override double? Index
        {
            get { return null; }
        }

        /// <summary>Property that holds the units of the index of the constraint.</summary>
        /// <value><see cref="System.String"/> <see cref="IndexUnits"/>.</value>
        /// <remarks>Read-only property because index units are determined by the type: <c>null</c> for this type.</remarks>
        public override string IndexUnits
        {
            get { return null; }
        }

        /// <summary>Property that holds the threshold of the constraint (constraint value).</summary>
        /// <value><see cref="System.Double"/> <c>base.Dose.Dose</c>.</value>
        /// <remarks>Read-only property because threshold is determined by the type: <c>Dose.Dose</c> for this type.</remarks>
        public override double Threshold
        {
            get { return base.Dose.Dose; }
        }

        /// <summary>Property that holds the units of the threshold of the constraint.</summary>
        /// <value><see cref="System.String"/> <c>base.Dose.UnitAsString</c>.</value>
        /// <remarks>Read-only property because threshold units are determined by the type: <c>Dose.UnitAsString</c> for this type.</remarks>
        public override string ThresholdUnits
        {
            get { return base.Dose.UnitAsString; }
        }

        #endregion

        #region CONSTRUCTORS

        /// <summary>This constructor initializes all properties except the structure associated to the constraint.</summary>
        /// <param name="dose">The dose associated to the constraint. Type: <see cref="VMS.TPS.Common.Model.Types.DoseValue"/>.</param>
        /// <param name="structureType">The type of the underlying structure (OAR/PTV/other). Type: <see cref="SCFM.StructureType"/>.</param>
        /// <param name="comparisonMode">The comparison mode between actual value and the constraint value. Type: <see cref="ComparisonMode"/>.</param>
        /// <remarks>
        /// <see cref="Volume"/> and <see cref="VolumeUnits"/> are <c>null</c> by definition.
        /// Properties are initialized in an appropriate order to allow meaningful argument checking.
        /// </remarks>
        public Constraint_Dmax(DoseValue dose, StructureType structureType, ComparisonMode comparisonMode)
        {
            this.Dose = dose;
            this.VolumeUnits = null;
            this.Volume = null;
            this.Structure = null;
            this.StructureType = structureType;
            this.ComparisonMode = comparisonMode;
        }

        /// <summary>This constructor initializes all properties.</summary>
        /// <param name="dose">The dose associated to the constraint. Type: <see cref="VMS.TPS.Common.Model.Types.DoseValue"/>.</param>
        /// <param name="structure">The structure associated to the constraint. Type: <see cref="VMS.TPS.Common.Model.API.Structure"/>.</param>
        /// <param name="structureType">The type of the underlying structure (OAR/PTV/other). Type: <see cref="SCFM.StructureType"/>.</param>
        /// <param name="comparisonMode">The comparison mode between actual value and the constraint value. Type: <see cref="ComparisonMode"/>.</param>
        /// <remarks>
        /// <see cref="Volume"/> and <see cref="VolumeUnits"/> are <c>null</c> by definition.
        /// Properties are initialized in an appropriate order to allow meaningful argument checking.
        /// </remarks>
        public Constraint_Dmax(DoseValue dose, Structure structure, StructureType structureType, ComparisonMode comparisonMode)
        {
            this.Dose = dose;
            this.VolumeUnits = null;
            this.Volume = null;
            this.Structure = structure;
            this.StructureType = structureType;
            this.ComparisonMode = comparisonMode;
        }

        #endregion

        #region OVERRIDING PROTECTED METHODS: INTERNAL STUFF

        /// <summary>This method retrieves the maximum dose (Dmax) of the HDV of the current OAR/PTV/other for the current treatment plan (to compare against <c>this</c> constraint).</summary>
        /// <param name="myPlan">The current plan. Type: <see cref="VMS.TPS.Common.Model.API.PlanningItem"/>.</param>
        /// <param name="cumulativePrescriptionDose">(Optional parameter: only needed if <paramref name="myPlan"/> is of type <see cref="VMS.TPS.Common.Model.API.PlanSum"/> and a relative dose assessment is required). The maximum cumulative prescribed dose (in Gy) resulting from the sum of individual prescription doses.</param>
        /// <returns>The Dmax of the current OAR/PTV/other for the current treatment plan (to compare against <c>this</c> constraint). Dmax has the same representation (absolute/relative) and units than <c>this</c> constraint. Type: <see cref="System.Double"/>.</returns>
        /// <remarks>
        /// Overrided version of the base class method to return Dmax.
        /// This method accepts either plans of type <see cref="VMS.TPS.Common.Model.API.PlanSetup"/> or <see cref="VMS.TPS.Common.Model.API.PlanSum"/>. In the latter case, the optional parameter <paramref name="cumulativePrescriptionDose"/> must be provided if dose assessment is performed in relative units (%), in order to calculate percentage with respect that maximum cumulative dose.
        /// The optional parameter is not an issue for client code because the accessibility level of this method is protected. See the caller method (<see cref="SCFM.Constraint.GetReport"/>) for more information.
        /// </remarks>
        protected override double getCurrentValue(PlanningItem myPlan, double cumulativePrescriptionDose = 0.0)
        {
            if (myPlan is PlanSum && this.Dose.Unit == DoseValue.DoseUnit.Percent)
                return myPlan.GetDVHCumulativeData(this.Structure,
                    DoseValuePresentation.Absolute,
                    VolumePresentation.AbsoluteCm3,
                    0.001).MaxDose.Dose * 100.0 / cumulativePrescriptionDose;
            else
                return myPlan.GetDVHCumulativeData(this.Structure,
                    this.Dose.Unit == DoseValue.DoseUnit.Percent ? DoseValuePresentation.Relative : DoseValuePresentation.Absolute,
                    VolumePresentation.AbsoluteCm3,
                    0.001).MaxDose.Dose;
        }

        #endregion

    }

}