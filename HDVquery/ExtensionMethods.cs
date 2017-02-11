using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace SCFM
{

    /// <summary>Static class containing extension methods of ESAPI types.</summary>
    public static class ExtensionMethods
    {

        /// <summary>Method that returns the dose at a given volume of the HDV of a given structure for a given a treatment plan..</summary>
        /// <param name="myPlan">Type that this methods extends. Type: <see cref="VMS.TPS.Common.Model.API.PlanningItem"/>.</param>
        /// <param name="structure">Structure (typically, an OAR) that is being assessed. Type: <see cref="VMS.TPS.Common.Model.API.Structure"/>.</param>
        /// <param name="volume">Volume at which the dose is requested. Type: <see cref="System.Double"/>.</param>
        /// <param name="volumePresentation">Volume units. Type: <see cref="VMS.TPS.Common.Model.Types.VolumePresentation"/>.</param>
        /// <param name="dosePresentation">Dose units. Type: <see cref="VMS.TPS.Common.Model.Types.DoseValuePresentation"/>.</param>
        /// <returns>The dose at the given volume for the given structure and treatment plan. Type: <see cref="VMS.TPS.Common.Model.Types.DoseValue"/>.</returns>
        /// <remarks>
        /// Extension method for the type <see cref="VMS.TPS.Common.Model.API.PlanningItem"/>.
        /// This method leverages the homonymous method of the type <see cref="VMS.TPS.Common.Model.API.PlanSetup"/> to its parent type <see cref="VMS.TPS.Common.Model.API.PlanningItem"/>. Therefore, this method becomes also available for derived types, like <see cref="VMS.TPS.Common.Model.API.PlanSum"/>.
        /// If <paramref name="myPlan"/> is already of type <see cref="VMS.TPS.Common.Model.API.PlanSetup"/>, this method calls the ESAPI method. Alternatively, if <paramref name="myPlan"/> is of type <see cref="VMS.TPS.Common.Model.API.PlanSum"/>, this implementation applies.
        /// WARNING: The type <see cref="VMS.TPS.Common.Model.API.PlanSum"/>, does not support relative dose values.
        /// </remarks>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="dosePresentation"/> retrieves a relative doses, which are not supported for <see cref="VMS.TPS.Common.Model.API.PlanSum"/> type.</exception>
        public static DoseValue GetDoseAtVolume(this PlanningItem myPlan, Structure structure, double volume, VolumePresentation volumePresentation, DoseValuePresentation dosePresentation)
        {
            if (myPlan is PlanSetup)
                return ((PlanSetup)myPlan).GetDoseAtVolume(structure, volume, volumePresentation, dosePresentation);
            else // PlanSum
            {
                if (dosePresentation == DoseValuePresentation.Absolute)
                {
                    DoseValue myDose = DoseValue.UndefinedDose();
                    DVHPoint[] myDVHpoints = myPlan.GetDVHCumulativeData(structure, dosePresentation, volumePresentation, 0.001).CurveData;
                    foreach (DVHPoint p in myDVHpoints)
                    {
                        if (p.Volume < volume)
                        {
                            myDose = p.DoseValue;
                            break;
                        }
                    }
                    return myDose;
                }
                else // dosePresentation == DoseValuePresentation.Relative
                    throw new ArgumentOutOfRangeException("dosePresentation ([D])", dosePresentation, "SCFM exception:\nPlan sum only supports absolute doses in Gy.");
            }
        }

        /// <summary>Method that returns the volume at a given dose of the HDV of a given structure for a given treatment plan.</summary>
        /// <param name="myPlan">Type that this methods extends. Type: <see cref="VMS.TPS.Common.Model.API.PlanningItem"/>.</param>
        /// <param name="structure">Structure (typically, an OAR) that is being assessed. Type: <see cref="VMS.TPS.Common.Model.API.Structure"/>.</param>
        /// <param name="dose">Dose at which the volume is requested. Type: <see cref="VMS.TPS.Common.Model.Types.DoseValue"/>.</param>
        /// <param name="volumePresentation">Volume units. Type: <see cref="VMS.TPS.Common.Model.Types.VolumePresentation"/>.</param>
        /// <returns>The volume at the given dose for the given struture and treatment plan. Type: <see cref="System.Double"/>.</returns>
        /// <remarks>
        /// Extension method for the type <see cref="VMS.TPS.Common.Model.API.PlanningItem"/>.
        /// This method leverages the homonymous method of the type <see cref="VMS.TPS.Common.Model.API.PlanSetup"/> to its parent type <see cref="VMS.TPS.Common.Model.API.PlanningItem"/>. Therefore, this method becomes also available for derived types, like <see cref="VMS.TPS.Common.Model.API.PlanSum"/>.
        /// If <paramref name="myPlan"/> is already of type <see cref="VMS.TPS.Common.Model.API.PlanSetup"/>, this method calls the ESAPI method. Alternatively, if <paramref name="myPlan"/> is of type <see cref="VMS.TPS.Common.Model.API.PlanSum"/>, this implementation applies.
        /// WARNING: The type <see cref="VMS.TPS.Common.Model.API.PlanSum"/>, does not support relative dose values.
        /// </remarks>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="dose"/> retrieves relative doses (or exotic units as "cGy" or "unknown"), which are not supported for <see cref="VMS.TPS.Common.Model.API.PlanSum"/> type.</exception>
        public static double GetVolumeAtDose(this PlanningItem myPlan, Structure structure, DoseValue dose, VolumePresentation volumePresentation)
        {
            if (myPlan is PlanSetup)
                return ((PlanSetup)myPlan).GetVolumeAtDose(structure, dose, volumePresentation);
            else
            {
                if (dose.Unit == DoseValue.DoseUnit.Gy)
                {
                    double myVolume = 0.0;
                    DVHPoint[] myDVHpoints = myPlan.GetDVHCumulativeData(structure, DoseValuePresentation.Absolute, volumePresentation, 0.001).CurveData;
                    foreach (DVHPoint p in myDVHpoints)
                    {
                        if (p.DoseValue.Dose > dose.Dose)
                        {
                            myVolume = p.Volume;
                            break;
                        }
                    }
                    return myVolume;
                }
                else // dose.Unit == DoseValue.DoseUnit.Percent (or "cGy" or "unknown")
                    throw new ArgumentOutOfRangeException("dose.Unit ([D])", dose, "SCFM exception:\nPlan sum only supports absolute doses in Gy.");
            }
        }

    }

}