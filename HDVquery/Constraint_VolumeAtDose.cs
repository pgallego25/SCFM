using System;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;


namespace SCFM
{

    /// <summary>Model of a constraint of an OAR/PTV/other of type "volume at dose" (V_D).</summary>
    /// <remarks>Sealed class. Constraints of type "V_D" do not need further specialization.</remarks>
    public sealed class Constraint_VolumeAtDose : Constraint
    {

        #region OVERRIDING "DOSE/VOLUME" PROPERTIES

        /// <summary>Property that holds the volume of the constraint.</summary>
        /// <value><see cref="System.Nullable"/> of type <see cref="System.Double"/> <c>base.Volume</c></value>
        /// <remarks>
        /// Overrided version of the base class property: not <c>null</c> <see cref="Volume"/> values are required and, if relative, <see cref="Volume"/> can not exceed 100%.
        /// Protected setter to force initialization through the constructor.
        /// </remarks>
        /// <exception cref="System.ArgumentNullException"><c>value</c> is <c>null</c>.</exception>"
        /// <exception cref="System.ArgumentOutOfRangeException"><c>value</c> (when relative) exceeds 100%.</exception>
        /// <exception cref="System.ArgumentNullException"><see cref="VolumeUnits"/> is <c>null</c>.</exception>"
        public override double? Volume
        {
            get { return base.Volume; }
            protected set
            {
                if (value == null)
                    throw new ArgumentNullException("value (V)", "SCFM exception:\nVolume is not provided.");
                else
                {
                    switch (this.VolumeUnits)
                    {
                        case VolumePresentation.AbsoluteCm3:
                            base.Volume = value;
                            break;
                        case VolumePresentation.Relative:
                            if (value <= 100.0)
                                base.Volume = value;
                            else
                                throw new ArgumentOutOfRangeException("value (V)", value, "SCFM exception:\nRange(V_D): V <= 100%.");
                            break;
                        default: // null value
                            throw new ArgumentNullException("VolumeUnits ([V])", "SCFM exception:\nVolume checking is not possible because volume units are not provided.");
                    }
                }
            }
        }

        /// <summary>Property that holds the units of the volume of the constraint.</summary>
        /// <value><see cref="System.Nullable"/> of type <see cref="VMS.TPS.Common.Model.Types.VolumePresentation"/> <c>base.VolumeUnits</c>.</value>
        /// <remarks>
        /// Overrided version of the base class property: <see cref="VolumeUnits"/> is not <c>null</c> by definition.
        /// Protected setter to force initialization through the constructor.
        /// </remarks>
        /// <exception cref="System.ArgumentNullException"><c>value</c> is <c>null</c>.</exception>
        public override VolumePresentation? VolumeUnits
        {
            get { return base.VolumeUnits; }
            protected set
            {
                if (value == null)
                    throw new ArgumentNullException("value ([V])", "SCFM exception:\nVolume units are not provided.");
                else
                    base.VolumeUnits = value;
            }
        }

        #endregion

        #region OVERRIDING "QUERY VALUES" PROPERTIES

        /// <summary>Property that holds the text label that identifies the constraint.</summary>
        /// <value><see cref="System.String"/> <see cref="Label"/>.</value>
        /// <remarks>Read-only property because such label is determined by the type: "V" for this type.</remarks>
        public override string Label
        {
            get { return "V"; }
        }

        /// <summary>Property that holds the index of the constraint (queried value).</summary>
        /// <value><see cref="System.Nullable"/> of type <see cref="System.Double"/> <c>base.Dose.Dose</c>.</value>
        /// <remarks>Read-only property because index is determined by the type: <c>Dose.Dose</c> for this type.</remarks>
        public override double? Index
        {
            get { return base.Dose.Dose; }
        }

        /// <summary>Property that holds the units of the index of the constraint.</summary>
        /// <value><see cref="System.String"/> <c>base.Dose.UnitAsString</c>.</value>
        /// <remarks>Read-only property because index units are determined by the type: <c>Dose.UnitAsString</c> for this type.</remarks>
        public override string IndexUnits
        {
            get { return base.Dose.UnitAsString; }
        }

        /// <summary>Property that holds the threshold of the constraint (constraint value).</summary>
        /// <value><see cref="System.Double"/> <c>(double)base.Volume</c>.</value>
        /// <remarks>Read-only property because threshold is determined by the type: <see cref="Volume"/> for this type.</remarks>
        public override double Threshold
        {
            get { return (double)base.Volume; }
        }

        /// <summary>Property that holds the units of the threshold of the constraint.</summary>
        /// <value><see cref="System.String"/> <see cref="ThresholdUnits"/>.</value>
        /// <remarks>Read-only property because threshold units are determined by the type: either "cc" or "%" depending upon the <see cref="VolumeUnits"/> property.</remarks>
        public override string ThresholdUnits
        {
            get
            {
                if (base.VolumeUnits == VolumePresentation.AbsoluteCm3)
                    return "cc";
                else
                    return "%";
            }
        }

        #endregion

        #region CONSTRUCTORS

        /// <summary>This constructor initializes all properties except the structure associated to the constraint.</summary>
        /// <param name="dose">The dose associated to the constraint. Type: <see cref="VMS.TPS.Common.Model.Types.DoseValue"/>.</param>
        /// <param name="volume">The volume associated to the constraint. Type: <see cref="System.Double"/>.</param>
        /// <param name="volumeUnits">The units of the volume. Type: <see cref="VMS.TPS.Common.Model.Types.VolumePresentation"/>.</param>
        /// <param name="structureType">The type of the underlying structure (OAR/PTV/other). Type: <see cref="SCFM.StructureType"/>.</param>
        /// <param name="comparisonMode">The comparison mode between actual value and the constraint value. Type: <see cref="ComparisonMode"/>.</param>
        /// <remarks>Properties are initialized in an appropriate order to allow meaningful argument checking.</remarks>
        public Constraint_VolumeAtDose(DoseValue dose, double volume, VolumePresentation volumeUnits, StructureType structureType, ComparisonMode comparisonMode)
        {
            this.Dose = dose;
            this.VolumeUnits = volumeUnits;
            this.Volume = volume;
            this.Structure = null;
            this.StructureType = structureType;
            this.ComparisonMode = comparisonMode;
        }

        /// <summary>This constructor initializes all properties.</summary>
        /// <param name="dose">The dose associated to the constraint. Type: <see cref="VMS.TPS.Common.Model.Types.DoseValue"/>.</param>
        /// <param name="volume">The volume associated to the constraint. Type: <see cref="System.Double"/>.</param>
        /// <param name="volumeUnits">The units of the volume. Type: <see cref="VMS.TPS.Common.Model.Types.VolumePresentation"/>.</param>
        /// <param name="structure">The structure associated to the constraint. Type: <see cref="VMS.TPS.Common.Model.API.Structure"/>.</param>
        /// <param name="structureType">The type of the underlying structure (OAR/PTV/other). Type: <see cref="SCFM.StructureType"/>.</param>
        /// <param name="comparisonMode">The comparison mode between actual value and the constraint value. Type: <see cref="ComparisonMode"/>.</param>
        /// <remarks>Properties are initialized in an appropriate order to allow meaningful argument checking.</remarks>
        public Constraint_VolumeAtDose(DoseValue dose, double volume, VolumePresentation volumeUnits, Structure structure, StructureType structureType, ComparisonMode comparisonMode)
        {
            this.Dose = dose;
            this.VolumeUnits = volumeUnits;
            this.Volume = volume;
            this.Structure = structure;
            this.StructureType = structureType;
            this.ComparisonMode = comparisonMode;
        }

        #endregion

        #region OVERRIDING PROTECTED METHODS: INTERNAL STUFF

        /// <summary>This method retrieves the volume at dose (V_D) of the HDV of the current OAR/PTV/other for the current treatment plan (to compare against <c>this</c> constraint).</summary>
        /// <param name="myPlan">The current plan. Type: <see cref="VMS.TPS.Common.Model.API.PlanningItem"/>.</param>
        /// <param name="cumulativePrescriptionDose">(Optional parameter: only needed if <paramref name="myPlan"/> is of type <see cref="VMS.TPS.Common.Model.API.PlanSum"/> and a relative dose assessment is required). The maximum cumulative prescribed dose (in Gy) resulting from the sum of individual prescription doses.</param>
        /// <returns>The V_D of the current OAR/PTV/other for the current treatment plan (to compare against <c>this</c> constraint). V_D has the same representation (absolute/relative) and units than <c>this</c> constraint, and it is evaluated at the same point (<see cref="Index"/>). Type: <see cref="System.Double"/>.</returns>
        /// <remarks>
        /// Overrided version of the base class method to return V_D.
        /// This method accepts either plans of type <see cref="VMS.TPS.Common.Model.API.PlanSetup"/> or <see cref="VMS.TPS.Common.Model.API.PlanSum"/>. In the latter case, the optional parameter <paramref name="cumulativePrescriptionDose"/> must be provided if dose assessment is performed in relative units (%), in order to calculate percentage with respect that maximum cumulative dose.
        /// The optional parameter is not an issue for client code because the accessibility level of this method is protected. See the caller method (<see cref="SCFM.Constraint.GetReport"/>) for more information.
        /// </remarks>
        protected override double getCurrentValue(PlanningItem myPlan, double cumulativePrescriptionDose = 0.0)
        {
            if (myPlan is PlanSum && this.Dose.Unit == DoseValue.DoseUnit.Percent)
                return myPlan.GetVolumeAtDose(this.Structure,
                    new DoseValue(this.Dose.Dose * cumulativePrescriptionDose / 100.0, DoseValue.DoseUnit.Gy),
                    (VolumePresentation)this.VolumeUnits);
            else
                return myPlan.GetVolumeAtDose(this.Structure,
                    this.Dose,
                    (VolumePresentation)this.VolumeUnits);
        }

        #endregion

    }

}