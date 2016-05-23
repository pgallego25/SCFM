using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.IO;

namespace VMS.TPS
{
    /* Bateria de tests aplicats sobre els camps (de setup o de tractament) individuals. */

    internal static class BeamTests
    {

        // Invocar els tests des d'aquí. Crear els tests dins d'aquesta classe amb signatura "private static void newTest(Beam myBeam)"
        internal static void BeamTestsSet(Beam myBeam)
        {
            DoseRateTest(myBeam);
            if (!myBeam.IsSetupField)
                minMUTest(myBeam);
            EnergyTest(myBeam);
            JawTest(myBeam);
            if (!myBeam.IsSetupField && Script.myTechnique != TechniqueType.Electrons)
                MLCTest(myBeam);
            IDTest(myBeam);
            CouchTest(myBeam);
        }

        /* Verifica que el DoseRate de qualsevol camp sigui correcte (o adient). */
        private static void DoseRateTest(Beam myBeam)
        {
            int myDoseRate = myBeam.DoseRate;
            if (myBeam.IsSetupField)
            {
                if (myDoseRate != TolAndMess.DR_image)
                    Script.myErrors.Add(string.Format(TolAndMess.errorDR_image, myBeam.Id, myDoseRate, TolAndMess.DR_image));
            }
            else
            {
                switch (Script.myTechnique)
                {
                    case TechniqueType.Standard:
                    case TechniqueType.IMRT:
                    case TechniqueType.Electrons:
                        if (myDoseRate != TolAndMess.DR_low)
                            Script.myWarnings.Add(string.Format(TolAndMess.warningDR_treat, myBeam.Id, myDoseRate, TolAndMess.DR_low));
                        break;
                    case TechniqueType.Palliative:
                    case TechniqueType.SBRT:
                        if (myDoseRate != TolAndMess.DR_high)
                            Script.myWarnings.Add(string.Format(TolAndMess.warningDR_treat, myBeam.Id, myDoseRate, TolAndMess.DR_high));
                        break;
                }
            }
        }

        /* Verifica que el nombre d'UM dels camps de tractament superi el mínim permès. */
        private static void minMUTest(Beam myBeam)
        {
            double myUM = myBeam.Meterset.Value;
            if (myBeam.Wedges.OfType<EnhancedDynamicWedge>().Count() != 0)
            {
                if (myUM < TolAndMess.MU_EDWThreshold)
                    Script.myErrors.Add(string.Format(TolAndMess.errorMU_EDW, myBeam.Id, myUM, TolAndMess.MU_EDWThreshold));
            }
            else
            {
                if (myUM < TolAndMess.MU_generalThreshold)
                    Script.myErrors.Add(string.Format(TolAndMess.errorMU_general, myBeam.Id, myUM, TolAndMess.MU_generalThreshold));
            }
        }

        /* Verifica que l'energia de qualsevol camp sigui correcte (o adient). */
        private static void EnergyTest(Beam myBeam)
        {
            string myEnergy = myBeam.EnergyModeDisplayName;
            if (myBeam.IsSetupField)
            {
                if (myEnergy != TolAndMess.E_image)
                    Script.myErrors.Add(string.Format(TolAndMess.errorE_image, myBeam.Id, myEnergy, TolAndMess.E_image));
            }
            else
            {
                switch (Script.myTechnique)
                {
                    case TechniqueType.IMRT:
                        if (myEnergy != TolAndMess.E_IMRT)
                            Script.myWarnings.Add(string.Format(TolAndMess.warningE_IMRT, myBeam.Id, myEnergy, TolAndMess.E_IMRT));
                        break;
                    case TechniqueType.SBRT:
                        if (myEnergy != TolAndMess.E_SBRT)
                            Script.myWarnings.Add(string.Format(TolAndMess.warningE_SBRT, myBeam.Id, myEnergy, TolAndMess.E_SBRT));
                        break;
                }
            }
        }

        /* a) Verifica que les mandíbules de qualsevol camp no sobrepassin les seves posicions límit (limitació física o recomanable).
         * b) Verifica que els camps de tractament que potencialment estan dissenyats amb tècnica d'hemicamps tinguin una de les mandíbules exactament sobre l'eix central del feix. */
        private static void JawTest(Beam myBeam)
        {
            double firstX1 = myBeam.ControlPoints.First().JawPositions.X1;
            double firstX2 = myBeam.ControlPoints.First().JawPositions.X2;
            double firstY1 = myBeam.ControlPoints.First().JawPositions.Y1;
            double firstY2 = myBeam.ControlPoints.First().JawPositions.Y2;
            double lastX1 = myBeam.ControlPoints.Last().JawPositions.X1;
            double lastX2 = myBeam.ControlPoints.Last().JawPositions.X2;
            double lastY1 = myBeam.ControlPoints.Last().JawPositions.Y1;
            double lastY2 = myBeam.ControlPoints.Last().JawPositions.Y2;
            double firstX = firstX2 - firstX1;
            double firstY = firstY2 - firstY1;
            double lastX = lastX2 - lastX1;
            double lastY = lastY2 - lastY1;
            double gantry = myBeam.ControlPoints.First().GantryAngle;
            double J_wedgeLimit;
            if (myBeam.IsSetupField)
            {
                if (myBeam.Id == "CBCT")
                {
                    if (firstX1 != -TolAndMess.J_CBCT ||
                        lastX2 != TolAndMess.J_CBCT ||
                        firstY1 != -TolAndMess.J_CBCT ||
                        lastY2 != TolAndMess.J_CBCT)
                    {
                        Script.myErrors.Add(string.Format(TolAndMess.errorJ_CBCT, myBeam.Id, TolAndMess.J_CBCT / 10));
                    }
                }
                else
                {
                    if (firstX1 < -TolAndMess.J_setupXLimit ||
                        lastX2 > TolAndMess.J_setupXLimit ||
                        firstY1 < -TolAndMess.J_setupYLimit ||
                        lastY2 > TolAndMess.J_setupYLimit)
                    {
                        Script.myErrors.Add(string.Format(TolAndMess.errorJ_setup, myBeam.Id, TolAndMess.J_setupXLimit / 10, TolAndMess.J_setupYLimit / 10));
                    }
                    if (gantry == 0.0)
                    {
                        if (firstX1 > TolAndMess.J_overTreatXLimit ||
                            lastX2 < -TolAndMess.J_overTreatXLimit ||
                            firstY1 > TolAndMess.J_overTreatYLimit ||
                            lastY2 < -TolAndMess.J_overTreatYLimit)
                        {
                            Script.myErrors.Add(string.Format(TolAndMess.errorJ_overSetup, myBeam.Id, TolAndMess.J_overTreatXLimit / 10, TolAndMess.J_overTreatYLimit / 10, TolAndMess.J_overkVLimit / 10));
                        }
                        if (firstX1 == TolAndMess.J_overTreatXLimit ||
                            lastX2 == -TolAndMess.J_overTreatXLimit ||
                            firstY1 == TolAndMess.J_overTreatYLimit ||
                            lastY2 == -TolAndMess.J_overTreatYLimit)
                        {
                            Script.myWarnings.Add(string.Format(TolAndMess.warningJ_overSetup, myBeam.Id, TolAndMess.J_overTreatXLimit / 10, TolAndMess.J_overTreatYLimit / 10, TolAndMess.J_overkVLimit / 10));
                        }
                    }
                    else
                    {
                        if (firstX1 > TolAndMess.J_overkVLimit ||
                            lastX2 < -TolAndMess.J_overkVLimit ||
                            firstY1 > TolAndMess.J_overkVLimit ||
                            lastY2 < -TolAndMess.J_overkVLimit)
                        {
                            Script.myErrors.Add(string.Format(TolAndMess.errorJ_overSetup, myBeam.Id, TolAndMess.J_overTreatXLimit / 10, TolAndMess.J_overTreatYLimit / 10, TolAndMess.J_overkVLimit / 10));
                        }
                        if (firstX1 == TolAndMess.J_overkVLimit ||
                            lastX2 == -TolAndMess.J_overkVLimit ||
                            firstY1 == TolAndMess.J_overkVLimit ||
                            lastY2 == -TolAndMess.J_overkVLimit)
                        {
                            Script.myWarnings.Add(string.Format(TolAndMess.warningJ_overSetup, myBeam.Id, TolAndMess.J_overTreatXLimit / 10, TolAndMess.J_overTreatYLimit / 10, TolAndMess.J_overkVLimit / 10));
                        }
                    }
                }
            }
            else
            {
                foreach (Wedge myWedge in myBeam.Wedges)
                {
                    if (myWedge is EnhancedDynamicWedge)
                    {
                        if ((myWedge.Direction == 0 && lastY2 > TolAndMess.J_EDWYLimit) ||
                            (myWedge.Direction == 180 && firstY1 < -TolAndMess.J_EDWYLimit))
                        {
                            Script.myErrors.Add(string.Format(TolAndMess.errorJ_EDW, myBeam.Id, myWedge.WedgeAngle, TolAndMess.J_EDWYLimit / 10));
                        }
                        if ((myWedge.Direction == 0 && lastY2 == TolAndMess.J_EDWYLimit) ||
                            (myWedge.Direction == 180 && firstY1 == -TolAndMess.J_EDWYLimit))
                        {
                            Script.myWarnings.Add(string.Format(TolAndMess.warningJ_EDW, myBeam.Id, myWedge.WedgeAngle, TolAndMess.J_EDWYLimit / 10));
                        }
                    }
                    else
                    {
                        if (myWedge.WedgeAngle == 60)
                            J_wedgeLimit = TolAndMess.J_wedgeLimit_60;
                        else
                            J_wedgeLimit = TolAndMess.J_wedgeLimit_153045;
                        if (((myWedge.Direction == 0 || myWedge.Direction == 180) && (firstY1 < -J_wedgeLimit || lastY2 > J_wedgeLimit)) ||
                            ((myWedge.Direction == 90 || myWedge.Direction == 270) && (firstX1 < -J_wedgeLimit || lastX2 > J_wedgeLimit)))
                        {
                            Script.myErrors.Add(string.Format(TolAndMess.errorJ_wedge, myBeam.Id, myWedge.WedgeAngle, J_wedgeLimit / 10));
                        }
                        if (((myWedge.Direction == 0 || myWedge.Direction == 180) && (firstY1 == -J_wedgeLimit || lastY2 == J_wedgeLimit)) ||
                            ((myWedge.Direction == 90 || myWedge.Direction == 270) && (firstX1 == -J_wedgeLimit || lastX2 == J_wedgeLimit)))
                        {
                            Script.myWarnings.Add(string.Format(TolAndMess.warningJ_wedge, myBeam.Id, myWedge.WedgeAngle, J_wedgeLimit / 10));
                        }
                    }
                }
                switch (Script.myTechnique) // [Veure nota @ TolAndMess]
                {
                    case TechniqueType.IMRT:
                        if (lastX1 > TolAndMess.J_overTreatXLimit ||
                            firstX2 < -TolAndMess.J_overTreatXLimit ||
                            lastY1 > TolAndMess.J_overTreatYLimit ||
                            firstY2 < -TolAndMess.J_overTreatYLimit)
                        {
                            Script.myErrors.Add(string.Format(TolAndMess.errorJ_overTreat, myBeam.Id, TolAndMess.J_overTreatXLimit / 10, TolAndMess.J_overTreatYLimit / 10));
                        }
                        if (lastY1 == TolAndMess.J_overTreatYLimit ||
                            firstY2 == -TolAndMess.J_overTreatYLimit)
                        {
                            Script.myWarnings.Add(string.Format(TolAndMess.warningJ_overTreat, myBeam.Id, TolAndMess.J_overTreatXLimit / 10, TolAndMess.J_overTreatYLimit / 10));
                        }
                        break;
                    case TechniqueType.Standard:
                    case TechniqueType.Palliative:
                    case TechniqueType.SBRT:
                        if (lastX1 > TolAndMess.J_overTreatXLimit ||
                            firstX2 < -TolAndMess.J_overTreatXLimit ||
                            lastY1 > TolAndMess.J_overTreatYLimit ||
                            firstY2 < -TolAndMess.J_overTreatYLimit)
                        {
                            Script.myErrors.Add(string.Format(TolAndMess.errorJ_overTreat, myBeam.Id, TolAndMess.J_overTreatXLimit / 10, TolAndMess.J_overTreatYLimit / 10));
                        }
                        if (lastX1 == TolAndMess.J_overTreatXLimit ||
                            firstX2 == -TolAndMess.J_overTreatXLimit ||
                            lastY1 == TolAndMess.J_overTreatYLimit ||
                            firstY2 == -TolAndMess.J_overTreatYLimit)
                        {
                            Script.myWarnings.Add(string.Format(TolAndMess.warningJ_overTreat, myBeam.Id, TolAndMess.J_overTreatXLimit / 10, TolAndMess.J_overTreatYLimit / 10));
                        }
                        break;
                }
                if (firstX1 < -TolAndMess.J_openLimit ||
                    lastX2 > TolAndMess.J_openLimit ||
                    firstY1 < -TolAndMess.J_openLimit ||
                    lastY2 > TolAndMess.J_openLimit)
                {
                    Script.myErrors.Add(string.Format(TolAndMess.errorJ_open, myBeam.Id, TolAndMess.J_openLimit / 10));
                }
                if (firstX1 == -TolAndMess.J_openLimit ||
                    lastX2 == TolAndMess.J_openLimit ||
                    firstY1 == -TolAndMess.J_openLimit ||
                    lastY2 == TolAndMess.J_openLimit)
                {
                    Script.myWarnings.Add(string.Format(TolAndMess.warningJ_open, myBeam.Id, TolAndMess.J_openLimit / 10));
                }
                if (firstX < TolAndMess.J_smallThreshold ||
                    firstY < TolAndMess.J_smallThreshold ||
                    lastX < TolAndMess.J_smallThreshold ||
                    lastY < TolAndMess.J_smallThreshold)
                {
                    Script.myWarnings.Add(string.Format(TolAndMess.warningJ_small, myBeam.Id, TolAndMess.J_smallThreshold / 10));
                }
                if ((firstX1 != 0 && Math.Abs(firstX1) < TolAndMess.J_hemiSens) ||
                    (lastX2 != 0 && Math.Abs(lastX2) < TolAndMess.J_hemiSens) ||
                    (firstY1 != 0 && Math.Abs(firstY1) < TolAndMess.J_hemiSens) ||
                    (lastY2 != 0 && Math.Abs(lastY2) < TolAndMess.J_hemiSens))
                {
                    Script.myWarnings.Add(string.Format(TolAndMess.warningJ_hemi, myBeam.Id, TolAndMess.J_hemiSens / 10));
                }
            }
        }

        /* Verifica que la configuració de l'MLC dels camps de tractament (no s'aplica per camps d'electrons) sigui correcte en relació a les mandíbules del camp. */
        private static void MLCTest(Beam myBeam)
        {

            double firstX1 = myBeam.ControlPoints.First().JawPositions.X1;
            double lastX2 = myBeam.ControlPoints.First().JawPositions.X2;
            double firstY1 = myBeam.ControlPoints.First().JawPositions.Y1;
            double lastY2 = myBeam.ControlPoints.First().JawPositions.Y2;
            float[,] firstMLC = myBeam.ControlPoints.First().LeafPositions;
            float[,] lastMLC = myBeam.ControlPoints.Last().LeafPositions;
            int firstLeaf;
            int lastLeaf;
            double minFirstMLC = 200.0;
            double maxLastMLC = -200.0;

            if (200.0 + firstY1 < 100.0)
                firstLeaf = 1 + (int)Math.Floor((200.0 + firstY1) / 10.0);
            else if (200.0 + firstY1 < 300.0)
                firstLeaf = 11 + (int)Math.Floor((100.0 + firstY1) / 5.0);
            else
                firstLeaf = 51 + (int)Math.Floor((-100.0 + firstY1) / 10.0);
            if (200.0 + lastY2 < 100.0)
                lastLeaf = (int)Math.Ceiling((200.0 + lastY2) / 10.0);
            else if (200.0 + lastY2 < 300.0)
                lastLeaf = 10 + (int)Math.Ceiling((100.0 + lastY2) / 5.0);
            else
                lastLeaf = 50 + (int)Math.Ceiling((-100.0 + lastY2) / 10.0);

            for (int countingLeafs = 1; countingLeafs < firstLeaf; countingLeafs++)
            {
                if (firstMLC[0, countingLeafs - 1] != firstMLC[1, countingLeafs - 1])
                    Script.myErrors.Add(string.Format(TolAndMess.errorMLC_out, myBeam.Id, countingLeafs));
            }
            for (int countingLeafs = lastLeaf + 1; countingLeafs <= 60; countingLeafs++)
            {
                if (firstMLC[0, countingLeafs - 1] != firstMLC[1, countingLeafs - 1])
                    Script.myErrors.Add(string.Format(TolAndMess.errorMLC_out, myBeam.Id, countingLeafs));
            }

            switch (Script.myTechnique)
            {
                case TechniqueType.Standard:
                case TechniqueType.Palliative:
                case TechniqueType.SBRT:
                    for (int countingLeafs = firstLeaf; countingLeafs <= lastLeaf; countingLeafs++)
                    {
                        if (firstMLC[0, countingLeafs - 1] == firstMLC[1, countingLeafs - 1] &&
                            (firstMLC[0, countingLeafs - 1] < firstX1 || firstMLC[1, countingLeafs - 1] > lastX2))
                        {
                            continue;
                        }
                        if (firstMLC[0, countingLeafs - 1] == firstMLC[1, countingLeafs - 1] &&
                            firstMLC[0, countingLeafs - 1] > firstX1 &&
                            firstMLC[1, countingLeafs - 1] < lastX2)
                        {
                            Script.myErrors.Add(string.Format(TolAndMess.errorMLC_in, myBeam.Id, countingLeafs));
                            continue;
                        }
                        if (firstMLC[1, countingLeafs - 1] - firstMLC[0, countingLeafs - 1] < TolAndMess.MLC_gapSens &&
                            firstMLC[0, countingLeafs - 1] > firstX1 &&
                            firstMLC[1, countingLeafs - 1] < lastX2)
                        {
                            Script.myWarnings.Add(string.Format(TolAndMess.warningMLC_gap, myBeam.Id, countingLeafs, TolAndMess.MLC_gapSens));
                        }
                        if (firstMLC[0, countingLeafs - 1] < minFirstMLC)
                            minFirstMLC = firstMLC[0, countingLeafs - 1];
                        if (lastMLC[1, countingLeafs - 1] > maxLastMLC)
                            maxLastMLC = lastMLC[1, countingLeafs - 1];
                    }
                    if (lastX2 - firstX1 <= TolAndMess.J_smallThreshold)
                    {
                        if (firstX1 - minFirstMLC > TolAndMess.MLC_jawGap3DSens ||
                            maxLastMLC - lastX2 > TolAndMess.MLC_jawGap3DSens)
                        {
                            Script.myErrors.Add(string.Format(TolAndMess.errorMLC_edge, myBeam.Id, TolAndMess.MLC_jawGap3DSens));
                        }
                    }
                    else
                    {
                        if (Math.Abs(minFirstMLC - firstX1) > TolAndMess.MLC_jawGap3DSens ||
                            Math.Abs(maxLastMLC - lastX2) > TolAndMess.MLC_jawGap3DSens)
                        {
                            Script.myErrors.Add(string.Format(TolAndMess.errorMLC_edge, myBeam.Id, TolAndMess.MLC_jawGap3DSens));
                        }
                    }
                    break;
                case TechniqueType.IMRT: // [Veure nota @ TolAndMess]
                    for (int countingLeafs = firstLeaf; countingLeafs <= lastLeaf; countingLeafs++)
                    {
                        if (lastMLC[1, countingLeafs - 1] > maxLastMLC)
                            maxLastMLC = lastMLC[1, countingLeafs - 1];
                    }
                    if (maxLastMLC - lastX2 > TolAndMess.MLC_jawGap3DSens)
                        Script.myErrors.Add(string.Format(TolAndMess.errorMLC_edge, myBeam.Id, TolAndMess.MLC_jawGap3DSens));
                    if (lastX2 - maxLastMLC > TolAndMess.MLC_jawGapIMRTSens)
                        Script.myErrors.Add(string.Format(TolAndMess.errorMLC_edge, myBeam.Id, TolAndMess.MLC_jawGapIMRTSens));
                    break;
            }
        }

        /* Verifica que les DRRs estiguin creades i que els noms i les IDs, tant de les DRR com dels camps, siguin correctes. */
        private static void IDTest(Beam myBeam)
        {

            if (myBeam.ReferenceImage == null)
                Script.myErrors.Add(string.Format(TolAndMess.errorID_DRR, myBeam.Id));
            else if (!myBeam.ReferenceImage.Id.StartsWith(myBeam.Id, StringComparison.OrdinalIgnoreCase))
                Script.myWarnings.Add(string.Format(TolAndMess.warningID_DRRID, myBeam.Id));

            if (!myBeam.Name.StartsWith(myBeam.Id, StringComparison.OrdinalIgnoreCase))
                Script.myWarnings.Add(string.Format(TolAndMess.warningID_nameID, myBeam.Id));
            double myCouch = 360.0 - myBeam.ControlPoints.FirstOrDefault().PatientSupportAngle;
            double myGantry = myBeam.ControlPoints.FirstOrDefault().GantryAngle;
            if (myBeam.IsSetupField)
            {
                if (myGantry == 0.0)
                {
                    if (!(myBeam.Id.StartsWith("SETUP 0º", StringComparison.OrdinalIgnoreCase) ||
                          myBeam.Id.StartsWith("SETUP0º", StringComparison.OrdinalIgnoreCase) ||
                          myBeam.Id.StartsWith("FLUORO 0º", StringComparison.OrdinalIgnoreCase) ||
                          myBeam.Id.StartsWith("FLUORO0º", StringComparison.OrdinalIgnoreCase) ||
                          myBeam.Id.StartsWith("CBCT", StringComparison.OrdinalIgnoreCase)))
                    {
                        Script.myWarnings.Add(string.Format(TolAndMess.warningID_setup, myBeam.Id));
                    }
                }
                else if (myGantry == 270.0)
                {
                    if (!(myBeam.Id.StartsWith("SETUP 270º", StringComparison.OrdinalIgnoreCase) ||
                          myBeam.Id.StartsWith("SETUP270º", StringComparison.OrdinalIgnoreCase) ||
                          myBeam.Id.StartsWith("FLUORO 270º", StringComparison.OrdinalIgnoreCase) ||
                          myBeam.Id.StartsWith("FLUORO270º", StringComparison.OrdinalIgnoreCase)))
                    {
                        Script.myWarnings.Add(string.Format(TolAndMess.warningID_setup, myBeam.Id));
                    }
                }
                else
                {
                    if (!(myBeam.Id.StartsWith("FLUORO", StringComparison.OrdinalIgnoreCase)))
                    {
                        Script.myWarnings.Add(string.Format(TolAndMess.warningID_setup, myBeam.Id));
                    }
                }
            }
            else
            {
                if (myCouch == 360.0)
                {
                    if (!(myBeam.Id.StartsWith(myGantry.ToString() + "º", StringComparison.Ordinal) ||
                          myBeam.Id.StartsWith(myGantry.ToString() + "Eº", StringComparison.Ordinal) ||
                          myBeam.Id.StartsWith(Math.Truncate(myGantry).ToString() + "º", StringComparison.Ordinal) ||
                          myBeam.Id.StartsWith(Math.Truncate(myGantry).ToString() + "Eº", StringComparison.Ordinal) ||
                          myBeam.Id.StartsWith(Math.Ceiling(myGantry).ToString() + "º", StringComparison.Ordinal) ||
                          myBeam.Id.StartsWith(Math.Ceiling(myGantry).ToString() + "Eº", StringComparison.Ordinal)))
                    {
                        Script.myWarnings.Add(string.Format(TolAndMess.warningID_field, myBeam.Id));
                    }
                }
                else
                {
                    if (!(myBeam.Id.StartsWith(myGantry.ToString() + "º", StringComparison.Ordinal) ||
                          myBeam.Id.StartsWith(myGantry.ToString() + "Eº", StringComparison.Ordinal) ||
                          myBeam.Id.StartsWith(Math.Ceiling(myGantry).ToString() + "º", StringComparison.Ordinal) ||
                          myBeam.Id.StartsWith(Math.Ceiling(myGantry).ToString() + "Eº", StringComparison.Ordinal) ||
                          myBeam.Id.StartsWith(Math.Truncate(myGantry).ToString() + "º", StringComparison.Ordinal) ||
                          myBeam.Id.StartsWith(Math.Truncate(myGantry).ToString() + "Eº", StringComparison.Ordinal)) ||
                        !(myBeam.Id.Contains("º-" + myCouch.ToString() + "º") ||
                          myBeam.Id.Contains("º-" + Math.Ceiling(myCouch).ToString() + "º") ||
                          myBeam.Id.Contains("º-" + Math.Truncate(myCouch).ToString() + "º")))
                    {
                        Script.myWarnings.Add(string.Format(TolAndMess.warningID_field, myBeam.Id));
                    }
                }
            }

        }

        /* Verifica si els girs de couch són en el sentit adequat i si són necessaris. */
        private static void CouchTest(Beam myBeam)
        {
            double myCouch = 360.0 - myBeam.ControlPoints.FirstOrDefault().PatientSupportAngle;
            if (myBeam.IsSetupField)
            {
                if (myCouch != 360.0)
                    Script.myErrors.Add(string.Format(TolAndMess.errorCH_setup, myBeam.Id));
            }
            else
            {
                if (myCouch != 360.0 &&
                    (myCouch < TolAndMess.CH_couchThreshold || myCouch > 360.0 - TolAndMess.CH_couchThreshold))
                {
                    Script.myWarnings.Add(string.Format(TolAndMess.warningCH_couch, myBeam.Id, TolAndMess.CH_couchThreshold));
                }
                if (myCouch > TolAndMess.CH_couchClinacLimit &&
                    myCouch < 180.0 &&
                    myBeam.TreatmentUnit.Id != "Clinac2_2100CD")
                {
                    Script.myErrors.Add(string.Format(TolAndMess.errorCH_couchClinac, myBeam.Id));
                }
                if (myCouch < 360.0 - TolAndMess.CH_couchClinacLimit &&
                    myCouch > 180.0 &&
                    myBeam.TreatmentUnit.Id == "Clinac2_2100CD")
                {
                    Script.myErrors.Add(string.Format(TolAndMess.errorCH_couchClinac, myBeam.Id));
                }
            }
        }

    }
}
