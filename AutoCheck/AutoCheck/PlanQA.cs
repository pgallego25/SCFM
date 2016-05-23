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

    public class Script
    {

        public Script() { }

        internal static List<string> myErrors = new List<string>();
        internal static List<string> myWarnings = new List<string>();
        internal static TechniqueType myTechnique;

        public void Execute(ScriptContext context/*, Window window*/)
        {
            if (context.ExternalPlansInScope.Count() == 0 || context.Image == null)
            {
                MessageBox.Show(TolAndMess.errorS_context, TolAndMess.S_contextHeader, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                foreach (ExternalPlanSetup myPlan in context.Course.ExternalPlanSetups)
                {
                    if (!myPlan.IsDoseValid)
                        MessageBox.Show(TolAndMess.errorS_dose, string.Format(TolAndMess.S_errorsHeader, myPlan.Id), MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                    {
                        myTechnique = TechniqueChoice(myPlan);
                        foreach (Beam myBeam in myPlan.Beams)
                            BeamTests.BeamTestsSet(myBeam);
                        PlanTests.PlanTestsSet(myPlan);
                        if (myErrors.Count() == 0 && myWarnings.Count() == 0)
                            MessageBox.Show(string.Format(TolAndMess.errorS_empty, myPlan.Id), string.Format(TolAndMess.S_emptyHeader, myPlan.Id), MessageBoxButton.OK, MessageBoxImage.Information);
                        else
                        {
                            if (myErrors.Count() != 0)
                            {
                                MessageBox.Show(string.Join("\n", myErrors), string.Format(TolAndMess.S_errorsHeader, myPlan.Id), MessageBoxButton.OK, MessageBoxImage.Error);
                                myErrors.Clear();
                            }
                            if (myWarnings.Count() != 0)
                            {
                                MessageBox.Show(string.Join("\n", myWarnings), string.Format(TolAndMess.S_warningsHeader, myPlan.Id), MessageBoxButton.OK, MessageBoxImage.Warning);
                                myWarnings.Clear();
                            }
                        }
                    }
                }
                CourseTests.CourseTestsSet(context.Course);
                if (myErrors.Count() == 0 && myWarnings.Count() == 0)
                    MessageBox.Show(string.Format(TolAndMess.errorS_empty, context.Course.Id), string.Format(TolAndMess.S_emptyHeader, context.Course.Id), MessageBoxButton.OK, MessageBoxImage.Information);
                else
                {
                    if (myErrors.Count() != 0)
                        MessageBox.Show(string.Join("\n", myErrors), string.Format(TolAndMess.S_errorsHeader, context.Course.Id), MessageBoxButton.OK, MessageBoxImage.Error);
                    if (myWarnings.Count() != 0)
                        MessageBox.Show(string.Join("\n", myWarnings), string.Format(TolAndMess.S_warningsHeader, context.Course.Id), MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        /* Escull el tipus de tècnica utilitzada en el pla de tractament. */
        private static TechniqueType TechniqueChoice(ExternalPlanSetup myPlan)
        {
            double myDosePerFraction = myPlan.UniqueFractionation.PrescribedDosePerFraction.Dose;
            if (myPlan.UniqueFractionation.PrescribedDosePerFraction.Unit == DoseValue.DoseUnit.cGy)
                myDosePerFraction /= 100.0;
            MLCPlanType myMLC = myPlan.Beams.Where(myBeam => !(myBeam.IsSetupField)).First().MLCPlanType;
            TechniqueType techniqueQuery = TechniqueType.Standard;
            switch (myMLC)
            {
                case MLCPlanType.DoseDynamic:
                    techniqueQuery = TechniqueType.IMRT;
                    break;
                case MLCPlanType.Static:
                    if (myDosePerFraction >= TolAndMess.TC_SBRTDosePerFractionThreshold && myPlan.Beams.Count() > TolAndMess.TC_SBRTNumberOfBeamsThreshold)
                        techniqueQuery = TechniqueType.SBRT;
                    else if (myDosePerFraction >= TolAndMess.TC_palliativeDosePerFractionThreshold)
                        techniqueQuery = TechniqueType.Palliative;
                    break;
                default:
                    techniqueQuery = TechniqueType.Electrons;
                    break;
            }
            return techniqueQuery;
        }

    }

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

    /* Bateria de tests aplicats sobre plans de tractament individuals. */
    internal static class PlanTests
    {

        // Invocar els tests des d'aquí. Crear els tests dins d'aquesta classe amb signatura "private static void newTest(ExternalPlanSetup myPlan)"
        internal static void PlanTestsSet(ExternalPlanSetup myPlan)
        {
            SetupFieldsTest(myPlan);
            IsocenterTest(myPlan);
            ClinacTest(myPlan);
            AlgorithmTest(myPlan);
            if (Script.myTechnique == TechniqueType.IMRT)
                IMRTTest(myPlan);
            if (Script.myTechnique != TechniqueType.Electrons)
                ClockTest(myPlan);
        }

        /* Verifica que els camps de setup de qualsevol pla de tractament estiguin ben configurats, en nombre i angulació. */
        private static void SetupFieldsTest(ExternalPlanSetup myPlan)
        {

            IEnumerable<Beam> mySetupFields = myPlan.Beams.Where(myBeam => myBeam.IsSetupField);
            IEnumerable<Beam> setupFieldsAt0 = mySetupFields.Where(myBeam => myBeam.ControlPoints.First().GantryAngle == 0.0);
            IEnumerable<Beam> setupFieldsAt270 = mySetupFields.Where(myBeam => myBeam.ControlPoints.First().GantryAngle == 270.0);
            IEnumerable<Beam> setupFieldsFluoro = mySetupFields.Where(myBeam => myBeam.ControlPoints.First().GantryAngle != 0.0 &&
                                                                                myBeam.ControlPoints.First().GantryAngle != 270.0);
            
            foreach (Beam myBeam in mySetupFields)
            {
                if (myBeam.ControlPoints.First().CollimatorAngle != 0)
                    Script.myErrors.Add(string.Format(TolAndMess.errorSF_collimator, myBeam.Id));
            }
            
            switch (Script.myTechnique)
            {
                case TechniqueType.Electrons:
                    if (setupFieldsAt0.Count() != 1 ||
                        setupFieldsAt270.Count() > 1 ||
                        setupFieldsFluoro.Count() != 0)
                    {
                        Script.myErrors.Add(TolAndMess.errorSF_electrons);
                    }
                    break;
                case TechniqueType.Standard:
                case TechniqueType.Palliative:
                case TechniqueType.SBRT:
                case TechniqueType.IMRT:
                    if (setupFieldsAt270.Count() != 1)
                    {
                        Script.myErrors.Add(TolAndMess.errorSF_270);
                    }
                    switch (myPlan.Beams.First().TreatmentUnit.Id)
                    {
                        case "Clinac1_2100CD":
                            if (setupFieldsAt0.Count() != 1 ||
                                setupFieldsAt0.First().Id.StartsWith("CBCT", StringComparison.OrdinalIgnoreCase))
                            {
                                Script.myErrors.Add(TolAndMess.errorSF_0);
                            }
                            if (setupFieldsFluoro.Count() != 0)
                            {
                                Script.myErrors.Add(string.Format(TolAndMess.errorSF_fluoro, setupFieldsFluoro.Count()));
                            }
                            break;
                        case "Clinac2_2100CD":
                            if (setupFieldsAt0.Count() != 2 ||
                                !((setupFieldsAt0.ElementAt(0).Id.StartsWith("CBCT", StringComparison.OrdinalIgnoreCase)) ^ (setupFieldsAt0.ElementAt(1).Id.StartsWith("CBCT", StringComparison.OrdinalIgnoreCase))))
                            {
                                Script.myErrors.Add(TolAndMess.errorSF_0);
                            }
                            if (setupFieldsFluoro.Count() > 1)
                            {
                                Script.myErrors.Add(string.Format(TolAndMess.errorSF_fluoro, setupFieldsFluoro.Count()));
                            }
                            else if (setupFieldsFluoro.Count() == 1)
                            {
                                IEnumerable<Beam> myTreatmentFields = myPlan.Beams.Where(myBeam => !myBeam.IsSetupField);
                                double meanGantry = 0.0;
                                foreach (Beam myBeam in myTreatmentFields)
                                {
                                    double myGantry = myBeam.ControlPoints.First().GantryAngle;
                                    if (myGantry <= 180.0)
                                    {
                                        meanGantry += myGantry / (double)myTreatmentFields.Count();
                                    }
                                    else
                                    {
                                        meanGantry += (myGantry - 360.0) / (double)myTreatmentFields.Count();
                                    }
                                }
                                double fluoroGantry = setupFieldsFluoro.First().ControlPoints.First().GantryAngle;
                                if (meanGantry > 0.0 &&
                                    (fluoroGantry < TolAndMess.SF_fluoroLeftMin || fluoroGantry > TolAndMess.SF_fluoroLeftMax))
                                {
                                    Script.myWarnings.Add(string.Format(TolAndMess.warningSF_fluoro, fluoroGantry, TolAndMess.SF_fluoroRightMin, TolAndMess.SF_fluoroRightMax, TolAndMess.SF_fluoroLeftMin, TolAndMess.SF_fluoroLeftMax));
                                }
                                if (meanGantry < 0.0 &&
                                    (fluoroGantry < TolAndMess.SF_fluoroRightMin || fluoroGantry > TolAndMess.SF_fluoroRightMax))
                                {
                                    Script.myWarnings.Add(string.Format(TolAndMess.warningSF_fluoro, fluoroGantry, TolAndMess.SF_fluoroRightMin, TolAndMess.SF_fluoroRightMax, TolAndMess.SF_fluoroLeftMin, TolAndMess.SF_fluoroLeftMax));
                                }
                            }
                            break;
                        case "Clinac3_2100CD":
                            if (setupFieldsAt0.Count() != 2 ||
                                !((setupFieldsAt0.ElementAt(0).Id.StartsWith("CBCT", StringComparison.OrdinalIgnoreCase)) ^ (setupFieldsAt0.ElementAt(1).Id.StartsWith("CBCT", StringComparison.OrdinalIgnoreCase))))
                            {
                                Script.myErrors.Add(TolAndMess.errorSF_0);
                            }
                            if (setupFieldsFluoro.Count() != 0)
                            {
                                Script.myErrors.Add(string.Format(TolAndMess.errorSF_fluoro, setupFieldsFluoro.Count()));
                            }
                            break;
                    }
                    break;
            }

        }

        /* a) Verifica que la posició de l'isocentre de qualsevol pla de tractament sigui única i adequada.
         * b) Verifica que la tècnica de tractament sigui única i correcte. */
        private static void IsocenterTest(ExternalPlanSetup myPlan)
        {

            VVector myIsocenter = myPlan.StructureSet.Image.DicomToUser(myPlan.Beams.FirstOrDefault().IsocenterPosition, myPlan);
            foreach (Beam myBeam in myPlan.Beams)
            {
                VVector myIso = myPlan.StructureSet.Image.DicomToUser(myBeam.IsocenterPosition, myPlan);
                if (Math.Abs(myIso.x - myIsocenter.x) > TolAndMess.Iso_precisionSens ||
                    Math.Abs(myIso.y - myIsocenter.y) > TolAndMess.Iso_precisionSens ||
                    Math.Abs(myIso.z - myIsocenter.z) > TolAndMess.Iso_precisionSens)
                {
                    Script.myErrors.Add(TolAndMess.errorIso_singleIso);
                    break;
                }
            }

            bool numericPrecision = (Math.Abs(myIsocenter.x - Math.Round(myIsocenter.x)) > TolAndMess.Iso_precisionSens && myIsocenter.x != Math.Truncate(myIsocenter.x)) ||
                                    (Math.Abs(myIsocenter.y - Math.Round(myIsocenter.y)) > TolAndMess.Iso_precisionSens && myIsocenter.y != Math.Truncate(myIsocenter.y)) ||
                                    (Math.Abs(myIsocenter.z - Math.Round(myIsocenter.z)) > TolAndMess.Iso_precisionSens && myIsocenter.z != Math.Truncate(myIsocenter.z));
            bool coordRounding = (Math.Abs(myIsocenter.x) < TolAndMess.Iso_shiftLimit - TolAndMess.Iso_precisionSens && Math.Abs(myIsocenter.x) > TolAndMess.Iso_precisionSens) ||
                                 (Math.Abs(myIsocenter.y) < TolAndMess.Iso_shiftLimit - TolAndMess.Iso_precisionSens && Math.Abs(myIsocenter.y) > TolAndMess.Iso_precisionSens) ||
                                 (Math.Abs(myIsocenter.z) < TolAndMess.Iso_shiftLimit - TolAndMess.Iso_precisionSens && Math.Abs(myIsocenter.z) > TolAndMess.Iso_precisionSens);
            if (numericPrecision || coordRounding)
            {
                Script.myWarnings.Add(string.Format(TolAndMess.warningIso_rounding, TolAndMess.Iso_shiftLimit / 10.0));
            }
                        
            SetupTechnique mySetup = myPlan.Beams.First().SetupTechnique;
            foreach (Beam myBeam in myPlan.Beams)
            {
                if (myBeam.SetupTechnique != mySetup)
                {
                    Script.myErrors.Add(TolAndMess.errorIso_singleTech);
                    break;
                }
            }

            switch (Script.myTechnique)
            {
                case TechniqueType.Electrons:
                    if (mySetup != SetupTechnique.FixedSSD)
                        Script.myErrors.Add(string.Format(TolAndMess.errorIso_Technique, mySetup));
                    break;
                case TechniqueType.Standard:
                case TechniqueType.Palliative:
                case TechniqueType.SBRT:
                case TechniqueType.IMRT:
                    if (mySetup != SetupTechnique.Isocentric)
                        Script.myErrors.Add(string.Format(TolAndMess.errorIso_Technique, mySetup));
                    break;
            }

            IEnumerable<Beam> myTreatmentFields = myPlan.Beams.Where(myBeam => !myBeam.IsSetupField);
            IEnumerable<Beam> myNoHemiXFields = myTreatmentFields.Where(myBeam => Math.Abs(myBeam.ControlPoints.First().JawPositions.X1) > TolAndMess.Iso_precisionSens &&
                                                                                  Math.Abs(myBeam.ControlPoints.Last().JawPositions.X2) > TolAndMess.Iso_precisionSens);
            IEnumerable<Beam> myNoHemiYFields = myTreatmentFields.Where(myBeam => Math.Abs(myBeam.ControlPoints.First().JawPositions.Y1) > TolAndMess.Iso_precisionSens &&
                                                                                  Math.Abs(myBeam.ControlPoints.Last().JawPositions.Y2) > TolAndMess.Iso_precisionSens);
            IEnumerable<Beam> myAsymXFields =
                from myBeam in myNoHemiXFields
                where myBeam.ControlPoints.First().JawPositions.X1 * myBeam.ControlPoints.Last().JawPositions.X2 > 0.0 ||
                      (myBeam.ControlPoints.First().JawPositions.X1 * myBeam.ControlPoints.Last().JawPositions.X2 < 0.0 &&
                       (Math.Abs(myBeam.ControlPoints.First().JawPositions.X1) / myBeam.ControlPoints.Last().JawPositions.X2 > TolAndMess.Iso_asymThreshold ||
                       myBeam.ControlPoints.Last().JawPositions.X2 / Math.Abs(myBeam.ControlPoints.First().JawPositions.X1) > TolAndMess.Iso_asymThreshold))
                select myBeam;
            IEnumerable<Beam> myAsymYFields =
                from myBeam in myNoHemiYFields
                where myBeam.ControlPoints.First().JawPositions.Y1 * myBeam.ControlPoints.Last().JawPositions.Y2 > 0.0 ||
                      (myBeam.ControlPoints.First().JawPositions.Y1 * myBeam.ControlPoints.Last().JawPositions.Y2 < 0.0 &&
                       (Math.Abs(myBeam.ControlPoints.First().JawPositions.Y1) / myBeam.ControlPoints.Last().JawPositions.Y2 > TolAndMess.Iso_asymThreshold ||
                       myBeam.ControlPoints.Last().JawPositions.Y2 / Math.Abs(myBeam.ControlPoints.First().JawPositions.Y1) > TolAndMess.Iso_asymThreshold))
                select myBeam;
            double asymXFields = (double)myAsymXFields.Count() / (double)myTreatmentFields.Count();
            double asymYFields = (double)myAsymYFields.Count() / (double)myTreatmentFields.Count();
            if (asymXFields > TolAndMess.Iso_ratioAsymThreshold ||
                asymYFields > TolAndMess.Iso_ratioAsymThreshold)
            {
                Script.myWarnings.Add(string.Format(TolAndMess.warningIso_asymetric, asymXFields * 100, asymYFields * 100, TolAndMess.Iso_ratioAsymThreshold * 100, TolAndMess.Iso_asymThreshold));
            }

        }

        /* Verifica que la selecció d'unitat de tractament sigui única i adequada per a qualsevol pla de tractament. */
        private static void ClinacTest(ExternalPlanSetup myPlan)
        {

            string myClinac = myPlan.Beams.FirstOrDefault().TreatmentUnit.Id;
            foreach (Beam myBeam in myPlan.Beams)
            {
                if (myBeam.TreatmentUnit.Id != myClinac)
                {
                    Script.myErrors.Add(TolAndMess.errorCl_multiple);
                    break;
                }
            }

            switch (Script.myTechnique)
            {
                case TechniqueType.IMRT:
                    if (myClinac == TolAndMess.Cl_IMRT)
                        Script.myErrors.Add(TolAndMess.errorCl_IMRT);
                    break;
                case TechniqueType.SBRT:
                    if (myClinac != TolAndMess.Cl_SBRT)
                        Script.myErrors.Add(TolAndMess.errorCl_SBRT);
                    break;
            }

        }

        /* Verifica que la configuració dels algorismes de càlcul utilitzats per a qualsevol pla de tractament sigui correcte. */
        private static void AlgorithmTest(ExternalPlanSetup myPlan)
        {
            Dictionary<string, string> myCalculationOptions = new Dictionary<string, string>();
            switch (Script.myTechnique)
            {
                case TechniqueType.Standard:
                case TechniqueType.Palliative:
                case TechniqueType.IMRT:
                case TechniqueType.SBRT:
                    myCalculationOptions = myPlan.PhotonCalculationOptions;
                    if (myPlan.PhotonCalculationModel != TolAndMess.Alg_photonVer ||
                        myCalculationOptions["CalculationGridSizeInCM"] != TolAndMess.A_photonRes ||
                        myCalculationOptions["FieldNormalizationType"] != TolAndMess.A_photonNorm ||
                        myCalculationOptions["HeterogeneityCorrection"] != TolAndMess.A_photonHeter)
                    {
                        Script.myErrors.Add(string.Format(TolAndMess.errorAlg_photonConfig, TolAndMess.Alg_photonVer, TolAndMess.A_photonRes, TolAndMess.A_photonNorm, TolAndMess.A_photonHeter));
                    }
                    break;
                case TechniqueType.Electrons:
                    myCalculationOptions = myPlan.ElectronCalculationOptions;
                    if (myPlan.ElectronCalculationModel != TolAndMess.Alg_elecVer ||
                        myCalculationOptions["Accuracy"] != TolAndMess.A_elecAcc ||
                        myCalculationOptions["NumberOfParticleHistories"] != TolAndMess.A_elecPart ||
                        myCalculationOptions["SmoothingLevel"] != TolAndMess.A_elecSmoothLevel ||
                        myCalculationOptions["SmoothingMethod"] != TolAndMess.A_elecSmoothMethod)
                    {
                        Script.myErrors.Add(string.Format(TolAndMess.errorAlg_elecConfig, TolAndMess.Alg_elecVer, TolAndMess.A_elecAcc, TolAndMess.A_elecPart, TolAndMess.A_elecSmoothLevel, TolAndMess.A_elecSmoothMethod));
                    }
                    string myEnergy = myPlan.Beams.Where(myBeam => !myBeam.IsSetupField).FirstOrDefault().EnergyModeDisplayName;
                    switch (myEnergy)
                    {
                        case "6E":
                        case "9E":
                        case "12E":
                            if (myCalculationOptions["CalculationGridSizeInCM"] != TolAndMess.Alg_elecRes_6912)
                                Script.myErrors.Add(string.Format(TolAndMess.errorAlg_elecRes, myEnergy, TolAndMess.Alg_elecRes_6912));
                            break;
                        case "16E":
                            if (myCalculationOptions["CalculationGridSizeInCM"] != TolAndMess.A_elecRes_16)
                                Script.myErrors.Add(string.Format(TolAndMess.errorAlg_elecRes, myEnergy, TolAndMess.A_elecRes_16));
                            break;
                        case "20E":
                            if (myCalculationOptions["CalculationGridSizeInCM"] != TolAndMess.A_elecRes_20)
                                Script.myErrors.Add(string.Format(TolAndMess.errorAlg_elecRes, myEnergy, TolAndMess.A_elecRes_20));
                            break;
                    }
                    break;
            }
        }

        /* a) Verifica que el nombre d'UM sigui raonable.
         * b) Verifica que els paràmetres d'optimizació siguin els estàndard: Normal Tissue Objectives (NTO), suavitzat de fluències, resolució de les estructures i objectius. */
        private static void IMRTTest(ExternalPlanSetup myPlan)
        {

            double totalMU = 0.0;
            IEnumerable<Beam> myTreatmentFields = myPlan.Beams.Where(myBeam => !myBeam.IsSetupField);
            foreach (Beam myBeam in myTreatmentFields)
                totalMU += myBeam.Meterset.Value;
            double meanMU = totalMU / (double)myTreatmentFields.Count();
            if (meanMU > TolAndMess.IMRT_meanMULimit)
                Script.myWarnings.Add(string.Format(TolAndMess.warningIMRT_meanMU, totalMU, meanMU, TolAndMess.IMRT_meanMULimit));

            IEnumerable<OptimizationNormalTissueParameter> myNTOParams =
                from param in myPlan.OptimizationSetup.Parameters
                where param is OptimizationNormalTissueParameter
                select (OptimizationNormalTissueParameter)param;
            if (myNTOParams.Count() == 0)
                Script.myErrors.Add(string.Format(TolAndMess.errorIMRT_NTO, TolAndMess.IMRT_NTOPriority, TolAndMess.IMRT_NTODistanceLimit / 10.0, TolAndMess.IMRT_NTOStartDose, TolAndMess.IMRT_NTOEndDoseLimit, TolAndMess.IMRT_NTOFallOffMin, TolAndMess.IMRT_NTOFallOffMax));
            else
            {
                OptimizationNormalTissueParameter myNTOParam = myNTOParams.FirstOrDefault();
                if (myNTOParam.DistanceFromTargetBorderInMM <= 0.0 ||
                    myNTOParam.DistanceFromTargetBorderInMM > TolAndMess.IMRT_NTODistanceLimit ||
                    myNTOParam.StartDosePercentage != TolAndMess.IMRT_NTOStartDose ||
                    myNTOParam.EndDosePercentage > TolAndMess.IMRT_NTOEndDoseLimit ||
                    myNTOParam.FallOff < TolAndMess.IMRT_NTOFallOffMin ||
                    myNTOParam.FallOff > TolAndMess.IMRT_NTOFallOffMax ||
                    myNTOParam.Priority != TolAndMess.IMRT_NTOPriority)
                {
                    Script.myErrors.Add(string.Format(TolAndMess.errorIMRT_NTO, TolAndMess.IMRT_NTOPriority, TolAndMess.IMRT_NTODistanceLimit / 10.0, TolAndMess.IMRT_NTOStartDose, TolAndMess.IMRT_NTOEndDoseLimit, TolAndMess.IMRT_NTOFallOffMin, TolAndMess.IMRT_NTOFallOffMax));
                }
            }

            IEnumerable<OptimizationIMRTBeamParameter> myIMRTParams =
                from param in myPlan.OptimizationSetup.Parameters
                where param is OptimizationIMRTBeamParameter
                select (OptimizationIMRTBeamParameter)param;
            foreach (OptimizationIMRTBeamParameter myIMRTParam in myIMRTParams)
            {
                if (myIMRTParam.SmoothX < TolAndMess.IMRT_smoothMin ||
                    myIMRTParam.SmoothY < TolAndMess.IMRT_smoothMin ||
                    myIMRTParam.SmoothX > TolAndMess.IMRT_smoothMax ||
                    myIMRTParam.SmoothY > TolAndMess.IMRT_smoothMax)
                {
                    Script.myWarnings.Add(string.Format(TolAndMess.warningIMRT_smooth, TolAndMess.IMRT_smoothMin, TolAndMess.IMRT_smoothMax));
                }
            }
            
            IEnumerable<OptimizationPointCloudParameter> myPointCloudParams =
                from param in myPlan.OptimizationSetup.Parameters
                where param is OptimizationPointCloudParameter
                select (OptimizationPointCloudParameter)param;
            List<string> listStructureRes = new List<string>();
            List<string> listPTVRes = new List<string>();
            foreach (OptimizationPointCloudParameter myOptParam in myPointCloudParams)
            {
                double myStructVol_mm3 = myOptParam.Structure.Volume * 1000.0;
                double myVoxelVol_mm3 = myOptParam.PointResolutionInMM * myOptParam.PointResolutionInMM * myOptParam.PointResolutionInMM;
                if (myStructVol_mm3 > TolAndMess.IMRT_resVolumeThreshold)
                {
                    if (myStructVol_mm3 / myVoxelVol_mm3 / TolAndMess.IMRT_resVolumeVsVoxelCorr < TolAndMess.IMRT_resPointsThreshold) // [Veure nota @ TolAndMess]
                        listStructureRes.Add(myOptParam.Structure.Id);
                    if (myOptParam.Structure.DicomType == "EXTERNAL" ||
                        myOptParam.Structure.Id.StartsWith("BODY", StringComparison.OrdinalIgnoreCase) ||
                        myOptParam.Structure.Id.StartsWith("CUERPO", StringComparison.OrdinalIgnoreCase)) // DicomType: veure DICOM RT Standard
                    {
                        if (myOptParam.PointResolutionInMM != TolAndMess.IMRT_resBody)
                            Script.myErrors.Add(string.Format(TolAndMess.errorIMRT_resBody, TolAndMess.IMRT_resBody));
                    }
                    else if (myOptParam.Structure.DicomType == "PTV" ||
                             myOptParam.Structure.Id.StartsWith("PTV", StringComparison.OrdinalIgnoreCase) ||
                             myOptParam.Structure.Id.StartsWith("_PTV", StringComparison.OrdinalIgnoreCase) ||
                             myOptParam.Structure.Id.StartsWith("-PTV", StringComparison.OrdinalIgnoreCase) ||
                             myOptParam.Structure.Id.StartsWith("INT", StringComparison.OrdinalIgnoreCase) ||
                             myOptParam.Structure.Id.StartsWith("_INT", StringComparison.OrdinalIgnoreCase) ||
                             myOptParam.Structure.Id.StartsWith("-INT", StringComparison.OrdinalIgnoreCase)) // DicomType: veure DICOM RT Standard
                    {
                        if (myOptParam.PointResolutionInMM > TolAndMess.IMRT_resPTVLimit)
                            listPTVRes.Add(myOptParam.Structure.Id);
                    }
                }
            }
            if (listPTVRes.Count() != 0)
                Script.myErrors.Add(string.Format(TolAndMess.errorIMRT_resPTV, string.Join("\n  - ", listPTVRes), TolAndMess.IMRT_resPTVLimit));
            if (listStructureRes.Count() != 0)
                Script.myErrors.Add(string.Format(TolAndMess.errorIMRT_res, string.Join("\n  - ", listStructureRes), TolAndMess.IMRT_resPointsThreshold, TolAndMess.IMRT_resVolumeThreshold / 1000.0));

            IEnumerable<OptimizationPointObjective> myPointObjs =
                from obj in myPlan.OptimizationSetup.Objectives
                where obj is OptimizationPointObjective
                select (OptimizationPointObjective)obj;
            var myPointObjsByStructure =
                from obj in myPointObjs
                group obj by obj.StructureId into structGroup
                select structGroup;
            List<string> listPTVPriority = new List<string>();
            List<string> listPTVObjNumber = new List<string>();
            List<string> listOARPriority = new List<string>();
            List<string> listOARObjNumber = new List<string>();
            foreach (var structGroup in myPointObjsByStructure)
            {
                var myLowerObj = structGroup.Where(point => point.Operator == OptimizationObjectiveOperator.Lower)
                                            .OrderBy(point => point.Dose.Dose);
                var myUpperObj = structGroup.Where(point => point.Operator == OptimizationObjectiveOperator.Upper)
                                            .OrderBy(point => point.Dose.Dose);
                if (structGroup.FirstOrDefault().Structure.DicomType == "EXTERNAL" ||
                    structGroup.Key.StartsWith("BODY", StringComparison.OrdinalIgnoreCase) ||
                    structGroup.Key.StartsWith("CUERPO", StringComparison.OrdinalIgnoreCase)) // DicomType: veure DICOM RT Standard
                {
                    Script.myErrors.Add(TolAndMess.errorIMRT_objBody);
                }
                else if (structGroup.FirstOrDefault().Structure.DicomType == "PTV" ||
                    structGroup.Key.StartsWith("PTV", StringComparison.OrdinalIgnoreCase) ||
                    structGroup.Key.StartsWith("_PTV", StringComparison.OrdinalIgnoreCase) ||
                    structGroup.Key.StartsWith("-PTV", StringComparison.OrdinalIgnoreCase) ||
                    structGroup.Key.StartsWith("INT", StringComparison.OrdinalIgnoreCase) ||
                    structGroup.Key.StartsWith("_INT", StringComparison.OrdinalIgnoreCase) ||
                    structGroup.Key.StartsWith("-INT", StringComparison.OrdinalIgnoreCase)) // DicomType: veure DICOM RT Standard
                {
                    foreach (OptimizationPointObjective myObj in structGroup)
                    {
                        if (myObj.Priority < TolAndMess.IMRT_objPTVPriorityMin ||
                            myObj.Priority > TolAndMess.IMRT_objPTVPriorityMax)
                        {
                            listPTVPriority.Add(structGroup.Key);
                            break;
                        }
                    }
                    if (myLowerObj.Count() != 1 ||
                        myUpperObj.Count() == 0 ||
                        myUpperObj.Count() > 2)
                    {
                        listPTVObjNumber.Add(structGroup.Key);
                    }
                    else
                    {
                        if (myLowerObj.FirstOrDefault().Volume < TolAndMess.IMRT_objPTVLowerVolumeThreshold)
                            Script.myWarnings.Add(string.Format(TolAndMess.warningIMRT_objPTVLower, structGroup.Key, TolAndMess.IMRT_objPTVLowerVolumeThreshold));
                        if (myUpperObj.ElementAt(0).Dose.Dose <= myLowerObj.FirstOrDefault().Dose.Dose ||
                            myUpperObj.ElementAt(0).Dose.Dose - myLowerObj.FirstOrDefault().Dose.Dose > TolAndMess.IMRT_objPTVUpperLowerDiffLimit)
                        {
                            Script.myWarnings.Add(string.Format(TolAndMess.warningIMRT_objPTVUpperDose, structGroup.Key, TolAndMess.IMRT_objPTVUpperLowerDiffLimit));
                        }
                        if (myUpperObj.Count() == 1)
                        {
                            if (myUpperObj.ElementAt(0).Volume > TolAndMess.IMRT_objPTVUpperVolumeLimitHard)
                                Script.myWarnings.Add(string.Format(TolAndMess.warningIMRT_objPTVUpperVolume, structGroup.Key, TolAndMess.IMRT_objPTVUpperVolumeLimitHard, TolAndMess.IMRT_objPTVUpperVolumeLimitSoft));
                        }
                        else
                        {
                            if (myUpperObj.ElementAt(0).Volume > TolAndMess.IMRT_objPTVUpperVolumeLimitSoft)
                                Script.myWarnings.Add(string.Format(TolAndMess.warningIMRT_objPTVUpperVolume, structGroup.Key, TolAndMess.IMRT_objPTVUpperVolumeLimitHard, TolAndMess.IMRT_objPTVUpperVolumeLimitSoft));
                            if (myUpperObj.ElementAt(1).Volume > TolAndMess.IMRT_objPTVUpperVolumeLimitHard ||
                                myUpperObj.ElementAt(1).Volume >= myUpperObj.ElementAt(0).Volume)
                            {
                                Script.myWarnings.Add(string.Format(TolAndMess.warningIMRT_objPTVUpperVolume, structGroup.Key, TolAndMess.IMRT_objPTVUpperVolumeLimitHard, TolAndMess.IMRT_objPTVUpperVolumeLimitSoft));
                            }
                        }
                    }
                }
                else
                {
                    foreach (OptimizationPointObjective myObj in structGroup)
                    {
                        if (myObj.Priority > TolAndMess.IMRT_objOARPriorityMax)
                        {
                            listOARPriority.Add(structGroup.Key);
                            break;
                        }
                    }
                    if (myLowerObj.Count() != 0 ||
                        myUpperObj.Count() == 0 ||
                        myUpperObj.Count() > 2)
                    {
                        listOARObjNumber.Add(structGroup.Key);
                    }
                    else
                    {
                        if (myUpperObj.ElementAt(0).Volume > TolAndMess.IMRT_OARUpperVolumeLimit)
                            Script.myWarnings.Add(string.Format(TolAndMess.warningIMRT_objOARUpperVolume, structGroup.Key, TolAndMess.IMRT_OARUpperVolumeLimit));
                        if (myUpperObj.Count() == 2)
                        {
                            if (myUpperObj.ElementAt(1).Volume > TolAndMess.IMRT_OARUpperVolumeLimit ||
                                myUpperObj.ElementAt(1).Volume >= myUpperObj.ElementAt(0).Volume)
                            {
                                Script.myWarnings.Add(string.Format(TolAndMess.warningIMRT_objOARUpperVolume, structGroup.Key, TolAndMess.IMRT_OARUpperVolumeLimit));
                            }
                        }
                    }
                }
            }
            if (listPTVPriority.Count() != 0)
                Script.myWarnings.Add(string.Format(TolAndMess.warningIMRT_objPTVPriority, string.Join("\n  - ", listPTVPriority), TolAndMess.IMRT_objPTVPriorityMin, TolAndMess.IMRT_objPTVPriorityMax));
            if (listPTVObjNumber.Count() != 0)
                Script.myWarnings.Add(string.Format(TolAndMess.warningIMRT_objPTVNumber, string.Join("\n  - ", listPTVObjNumber)));
            if (listOARPriority.Count() != 0)
                Script.myWarnings.Add(string.Format(TolAndMess.warningIMRT_objOARPriority, string.Join("\n  - ", listOARPriority), TolAndMess.IMRT_objOARPriorityMax));
            if (listOARObjNumber.Count() != 0)
                Script.myWarnings.Add(string.Format(TolAndMess.warningIMRT_objOARNumber, string.Join("\n  - ", listOARObjNumber)));

        }

        /* a) Verifica que el sentit de gir del gantry dels camps entre 175º i 185º sigui l'adequat (extended o no).
         * b1) IMRT: verifica que el gir de col·limador estigui optimitzat per minimitzar el recorregut de les làmines i evitar divisions de camp.
         * b2) Resta: verifica que els girs de col·limador entre camps adjacents siguin moderats (només si el camp no té falques). */
        private static void ClockTest(ExternalPlanSetup myPlan)
        {

            IEnumerable<Beam> electiveBeams = myPlan.Beams.Where(myBeam => !myBeam.IsSetupField &&
                                                                           myBeam.ControlPoints.FirstOrDefault().PatientSupportAngle == 0.0);

            IEnumerable<Beam> regularGantry = electiveBeams.Where(myBeam => myBeam.ControlPoints.FirstOrDefault().GantryAngle < 175.0 ||
                                                                            myBeam.ControlPoints.FirstOrDefault().GantryAngle > 185.0);
            IEnumerable<Beam> extendedGantry = electiveBeams.Where(myBeam => myBeam.ControlPoints.FirstOrDefault().GantryAngle >= 175.0 &&
                                                                             myBeam.ControlPoints.FirstOrDefault().GantryAngle <= 185.0);
            if (extendedGantry.Count() != 0 && regularGantry.Count() != 0)
            {
                double meanGantry = 0.0;
                foreach (Beam myBeam in regularGantry)
                {
                    double myGantry = myBeam.ControlPoints.FirstOrDefault().GantryAngle;
                    if (myGantry <= 180.0)
                        meanGantry += myGantry / (double)regularGantry.Count();
                    else
                        meanGantry += (myGantry - 360.0) / (double)regularGantry.Count();
                }
                if (meanGantry > TolAndMess.CK_meanGantryRange)
                {
                    foreach (Beam myBeam in extendedGantry)
                    {
                        double myGantry = myBeam.ControlPoints.FirstOrDefault().GantryAngle;
                        if (myGantry > 180.0)
                        {
                            if (!myBeam.Id.Contains("Eº"))
                                Script.myErrors.Add(string.Format(TolAndMess.errorCK_extended, myBeam.Id));
                        }
                        else
                        {
                            if (myBeam.Id.Contains("Eº"))
                                Script.myErrors.Add(string.Format(TolAndMess.errorCK_extended, myBeam.Id));
                        }
                    }
                }
                else if (meanGantry < -TolAndMess.CK_meanGantryRange)
                {
                    foreach (Beam myBeam in extendedGantry)
                    {
                        double myGantry = myBeam.ControlPoints.FirstOrDefault().GantryAngle;
                        if (myGantry <= 180.0)
                        {
                            if (!myBeam.Id.Contains("Eº"))
                                Script.myErrors.Add(string.Format(TolAndMess.errorCK_extended, myBeam.Id));
                        }
                        else
                        {
                            if (myBeam.Id.Contains("Eº"))
                                Script.myErrors.Add(string.Format(TolAndMess.errorCK_extended, myBeam.Id));
                        }
                    }
                }
            }

            switch (Script.myTechnique)
            {
                case TechniqueType.IMRT: // [Veure nota @ TolAndMes]
                    foreach (Beam myBeam in myPlan.Beams.Where(b => !b.IsSetupField))
                    {
                        double X = myBeam.ControlPoints.LastOrDefault().JawPositions.X2 - myBeam.ControlPoints.FirstOrDefault().JawPositions.X1;
                        double Y = myBeam.ControlPoints.LastOrDefault().JawPositions.Y2 - myBeam.ControlPoints.FirstOrDefault().JawPositions.Y1;
                        if (X > TolAndMess.CK_IMRTXLimit && Y < TolAndMess.CK_IMRTXLimit)
                            Script.myErrors.Add(string.Format(TolAndMess.errorCK_IMRTX, myBeam.Id, TolAndMess.CK_IMRTXLimit / 10.0));
                        else if (X - Y > TolAndMess.CK_IMRTJawDiffThreshold)
                            Script.myWarnings.Add(string.Format(TolAndMess.warningCK_IMRTJawDiff, myBeam.Id, TolAndMess.CK_IMRTJawDiffThreshold / 10.0));
                    }
                    break;
                case TechniqueType.Standard:
                case TechniqueType.Palliative:
                case TechniqueType.SBRT:
                    IEnumerable<Beam> gantry0To180 = electiveBeams.Where(myBeam => myBeam.ControlPoints.FirstOrDefault().GantryAngle <= 180.0 &&
                                                                                   !myBeam.Id.Contains("Eº"))
                                                                  .OrderBy(myBeam => myBeam.ControlPoints.FirstOrDefault().GantryAngle);
                    IEnumerable<Beam> gantry180To360 = electiveBeams.Where(myBeam => myBeam.ControlPoints.FirstOrDefault().GantryAngle > 180.0 &&
                                                                                     !myBeam.Id.Contains("Eº"))
                                                                    .OrderBy(myBeam => myBeam.ControlPoints.FirstOrDefault().GantryAngle);
                    IEnumerable<Beam> gantry175To180extended = electiveBeams.Where(myBeam => myBeam.ControlPoints.FirstOrDefault().GantryAngle <= 180.0 &&
                                                                                             myBeam.Id.Contains("Eº"))
                                                                            .OrderBy(myBeam => myBeam.ControlPoints.FirstOrDefault().GantryAngle);
                    IEnumerable<Beam> gantry180To185extended = electiveBeams.Where(myBeam => myBeam.ControlPoints.FirstOrDefault().GantryAngle > 180.0 &&
                                                                                             myBeam.Id.Contains("Eº"))
                                                                            .OrderBy(myBeam => myBeam.ControlPoints.FirstOrDefault().GantryAngle);
                    IEnumerable<Beam> clockwiseBeams = gantry175To180extended.Concat(gantry180To360.Concat(gantry0To180.Concat(gantry180To185extended)));
                    if (clockwiseBeams.Count() > 2)
                    {
                        if (clockwiseBeams.FirstOrDefault().Wedges.Count() == 0)
                        {
                            double myColl = clockwiseBeams.FirstOrDefault().ControlPoints.FirstOrDefault().CollimatorAngle;
                            double myPostColl = clockwiseBeams.ElementAt(1).ControlPoints.FirstOrDefault().CollimatorAngle;
                            if (myColl > 180.0)
                                myColl = myColl - 360.0;
                            if (myPostColl > 180.0)
                                myPostColl = myPostColl - 360.0;
                            double collimatorExcursion = Math.Abs(myPostColl - myColl);
                            if (collimatorExcursion >= TolAndMess.CK_partialExcursionThreshold / 2.0)
                            {
                                double myGantry = clockwiseBeams.FirstOrDefault().ControlPoints.FirstOrDefault().GantryAngle;
                                double myPostGantry = clockwiseBeams.ElementAt(1).ControlPoints.FirstOrDefault().GantryAngle;
                                if (myGantry > 180.0)
                                    myGantry = myGantry - 360.0;
                                if (myPostGantry > 180.0)
                                    myPostGantry = myPostGantry - 360.0;
                                double gantryExcursion = myPostGantry - myGantry;
                                double myTimeDelayRatio = 1.0;
                                if (gantryExcursion > TolAndMess.CK_presicionSens)
                                    myTimeDelayRatio = collimatorExcursion / gantryExcursion;
                                if (collimatorExcursion > TolAndMess.CK_partialExcursionLimit / 2.0 &&
                                    myTimeDelayRatio > TolAndMess.CK_colVsGantryVelRatio)
                                {
                                    Script.myWarnings.Add(string.Format(TolAndMess.warningCK_colPartialExcursion, clockwiseBeams.FirstOrDefault().Id, TolAndMess.CK_partialExcursionLimit / 2.0));
                                }
                            }
                        }
                        for (int i = 1; i < clockwiseBeams.Count() - 1; i++)
                        {
                            if (clockwiseBeams.ElementAt(i).Wedges.Count() == 0)
                            {
                                double myPreColl = clockwiseBeams.ElementAt(i - 1).ControlPoints.FirstOrDefault().CollimatorAngle;
                                double myColl = clockwiseBeams.ElementAt(i).ControlPoints.FirstOrDefault().CollimatorAngle;
                                double myPostColl = clockwiseBeams.ElementAt(i + 1).ControlPoints.FirstOrDefault().CollimatorAngle;
                                if (myPreColl > 180.0)
                                    myPreColl = myPreColl - 360.0;
                                if (myColl > 180.0)
                                    myColl = myColl - 360.0;
                                if (myPostColl > 180.0)
                                    myPostColl = myPostColl - 360.0;
                                double collimatorExcursion = Math.Abs(myColl - myPreColl) + Math.Abs(myPostColl - myColl);
                                double myPreGantry = clockwiseBeams.ElementAt(i - 1).ControlPoints.FirstOrDefault().GantryAngle;
                                double myPostGantry = clockwiseBeams.ElementAt(i + 1).ControlPoints.FirstOrDefault().GantryAngle;
                                if (myPreGantry > 180.0)
                                    myPreGantry = myPreGantry - 360.0;
                                if (myPostGantry > 180.0)
                                    myPostGantry = myPostGantry - 360.0;
                                double gantryExcursion = myPostGantry - myPreGantry;
                                double myTimeDelayRatio = 1.0;
                                if (gantryExcursion > TolAndMess.CK_presicionSens)
                                    myTimeDelayRatio = collimatorExcursion / gantryExcursion;
                                if (collimatorExcursion > TolAndMess.CK_partialExcursionLimit &&
                                    myTimeDelayRatio > TolAndMess.CK_colVsGantryVelRatio &&
                                    (myColl - myPreColl) * (myPostColl - myColl) < -TolAndMess.CK_presicionSens)
                                {
                                    Script.myWarnings.Add(string.Format(TolAndMess.warningCK_colPartialExcursion, clockwiseBeams.ElementAt(i).Id, TolAndMess.CK_partialExcursionLimit));
                                }
                            }
                        }
                        if (clockwiseBeams.LastOrDefault().Wedges.Count() == 0)
                        {
                            double myPreColl = clockwiseBeams.ElementAt(clockwiseBeams.Count() - 2).ControlPoints.FirstOrDefault().CollimatorAngle;
                            double myColl = clockwiseBeams.LastOrDefault().ControlPoints.FirstOrDefault().CollimatorAngle;
                            if (myPreColl > 180.0)
                                myPreColl = myPreColl - 360.0;
                            if (myColl > 180.0)
                                myColl = myColl - 360.0;
                            double collimatorExcursion = Math.Abs(myColl - myPreColl);
                            if (collimatorExcursion >= TolAndMess.CK_partialExcursionThreshold / 2.0)
                            {
                                double myPreGantry = clockwiseBeams.ElementAt(clockwiseBeams.Count() - 2).ControlPoints.FirstOrDefault().GantryAngle;
                                double myGantry = clockwiseBeams.LastOrDefault().ControlPoints.FirstOrDefault().GantryAngle;
                                if (myPreGantry > 180.0)
                                    myPreGantry = myPreGantry - 360.0;
                                if (myGantry > 180.0)
                                    myGantry = myGantry - 360.0;
                                double gantryExcursion = myGantry - myPreGantry;
                                double myTimeDelayRatio = 1.0;
                                if (gantryExcursion > TolAndMess.CK_presicionSens)
                                    myTimeDelayRatio = collimatorExcursion / gantryExcursion;
                                if (collimatorExcursion > TolAndMess.CK_partialExcursionLimit / 2.0 && myTimeDelayRatio > TolAndMess.CK_colVsGantryVelRatio)
                                    Script.myWarnings.Add(string.Format(TolAndMess.warningCK_colPartialExcursion, clockwiseBeams.LastOrDefault().Id, TolAndMess.CK_partialExcursionLimit / 2.0));
                            }
                        }
                        double myMaxColl = clockwiseBeams.FirstOrDefault().ControlPoints.FirstOrDefault().CollimatorAngle;
                        if (myMaxColl > 180.0)
                            myMaxColl = myMaxColl - 360.0;
                        double myMinColl = myMaxColl;
                        foreach (Beam myBeam in clockwiseBeams)
                        {
                            double myColl = myBeam.ControlPoints.FirstOrDefault().CollimatorAngle;
                            if (myColl > 180.0)
                                myColl = myColl - 360.0;
                            if (myColl > myMaxColl)
                                myMaxColl = myColl;
                            if (myColl < myMinColl)
                                myMinColl = myColl;
                        }
                        double maxCollDiff = myMaxColl - myMinColl;
                        if (maxCollDiff > TolAndMess.CK_colMaxExcursionLimit)
                            Script.myWarnings.Add(string.Format(TolAndMess.warningCK_colTotalExcursion, TolAndMess.CK_colMaxExcursionLimit));
                    }
                    break;

            }

        }

    }

    /* Bateria de tests aplicats sobre el curs actiu al context. */
    internal static class CourseTests
    {

        // Invocar els tests des d'aquí. Crear els tests dins d'aquesta classe amb signatura "private static void newTest(Course myCourse/*, Image myCT*/)"
        internal static void CourseTestsSet(Course myCourse)
        {
            CourseConsistencyTest(myCourse);
        }

        /* Verifica la consistència entre els diferents plans de tractament del mateix curs de tractament. */
        private static void CourseConsistencyTest(Course myCourse)
        {

            IEnumerable<ExternalPlanSetup> myPlans = myCourse.ExternalPlanSetups;
            string Clinac = myPlans.First().Beams.First().TreatmentUnit.Id;
            foreach (ExternalPlanSetup myPlan in myPlans)
            {
                string myClinac = myPlan.Beams.First().TreatmentUnit.Id;
                if (myClinac != Clinac)
                {
                    Script.myErrors.Add(TolAndMess.errorCC_clinac);
                    break;
                }
            }

            for (int i = 0; i < myPlans.Count(); i++)
            {
                bool stop = false;
                Image CT = myPlans.ElementAt(i).StructureSet.Image;
                VVector Isocenter = CT.DicomToUser(myPlans.ElementAt(i).Beams.FirstOrDefault().IsocenterPosition, myPlans.ElementAt(i));
                for (int j = i + 1; j < myPlans.Count(); j++)
                {
                    Image myCT = myPlans.ElementAt(j).StructureSet.Image;
                    if (myCT.Series.Study.Id == CT.Series.Study.Id &&
                        myCT.Series.Id == CT.Series.Id &&
                        myCT.Id == CT.Id)
                    {
                        VVector myIso = myCT.DicomToUser(myPlans.ElementAt(j).Beams.First().IsocenterPosition, myPlans.ElementAt(j));
                        double isoDist = VVector.Distance(myIso, Isocenter);
                        bool isoEqual = Math.Abs(myIso.x - Isocenter.x) < TolAndMess.CC_precisionSens &&
                                        Math.Abs(myIso.y - Isocenter.y) < TolAndMess.CC_precisionSens &&
                                        Math.Abs(myIso.z - Isocenter.z) < TolAndMess.CC_precisionSens;
                        if (!isoEqual && isoDist < TolAndMess.CC_isoShiftLimit)
                        {
                            Script.myWarnings.Add(string.Format(TolAndMess.warningCC_isocenter, TolAndMess.CC_isoShiftLimit / 10.0));
                            stop = true;
                            break;
                        }
                    }
                }
                if (stop)
                    break;
            }

        }

    }

    // Enumeració que classifica el pla de tractament en funció de la tècnica utilitzada.
    internal enum TechniqueType
    {
        Electrons,
        Standard,
        Palliative,
        SBRT,
        IMRT
    }

    // Classe que agrupa les toleràncies i els missatges d'error/alerta de l'Script.
    internal static class TolAndMess
    {

        internal const string S_contextHeader = "PlanQA diu:";
        internal const string errorS_context = "No s'han trobat imatges i/o plans de tractament actius. Carregar:\n> Pacient\n   > Curs\n      > TC + Pla (no pla suma).";
        internal const string S_emptyHeader = "Anàlisi sobre: {0}";
        internal const string errorS_empty = "PlanQA no ha trobat errors ni alertes en el {0}.";
        internal const string S_errorsHeader = "Llista d'errors en: {0}";
        internal const string S_warningsHeader = "Llista d'alertes en: {0}";
        internal const string errorS_dose = "El pla no té un càlcul de dosi vàlid i s'ometrà l'anàlisi.";

        internal const double TC_palliativeDosePerFractionThreshold = 3.0;
        internal const double TC_SBRTDosePerFractionThreshold = 7.5;
        internal const int TC_SBRTNumberOfBeamsThreshold = 10;

        internal const int DR_image = 100;
        internal const string errorDR_image = "Camp {0}: DoseRate incorrecte.\n(DoseRate: {1} -> {2} UM/min.)\n";
        internal const int DR_low = 300, DR_high = 600;
        internal const string warningDR_treat = "Camp {0}: DoseRate inusual per a aquest fraccionament.\n(DoseRate: {1} -> {2} UM/min?)\n";

        internal const double MU_generalThreshold = 6.5;
        internal const string errorMU_general = "Camp {0}: UM insuficients.\n(UM = {1:F3} < {2:F0} UM.)\n";
        internal const double MU_EDWThreshold = 21.5;
        internal const string errorMU_EDW = "Camp {0}: UM insuficients per a una EDW.\n(UM = {1:F3} < {2:F0} UM.)\n";

        internal const string E_image = "6X";
        internal const string errorE_image = "Camp {0}: Energia incorrecte per a un camp de setup.\n(Energia: {1} -> {2}.)\n";
        internal const string E_IMRT = "6X";
        internal const string warningE_IMRT = "Camp {0}: Energia inadequada per a un tractament d'IMRT.\n(Energia: {1} -> {2}?)\n";
        internal const string E_SBRT = "6X";
        internal const string warningE_SBRT = "Camp {0}: Energia inadequada per a un tractament d'SBRT.\n(Energia: {1} -> {2}?)\n";

        internal const double J_CBCT = 75.0;
        internal const string errorJ_CBCT = "Camp {0}: Mides de camp incorrectes.\n(X1 = X2 = Y1 = Y2 = {1} cm.)\n";
        internal const double J_setupXLimit = 125.0, J_setupYLimit = 100.0;
        internal const string errorJ_setup = "Camp {0}: S'han superat les mides de camp recomanades.\n(X1, X2 <= {1} cm; Y1, Y2 <= {2} cm.)\n";
        internal const double J_overkVLimit = 35.0;
        internal const string errorJ_overSetup = "Camp {0}: S'ha superat l'overtravel permès.\n(MV: X1, X2 > {1} cm; Y1, Y2 > {2} cm.\nkV: X1, X2, Y1, Y2 > {3} cm.)\n";
        internal const string warningJ_overSetup = "Camp {0}: L'overtravel està just al límit permès.\n(MV: X1, X2 = {1} cm? Y1, Y2 = {2} cm?\nkV: X1, X2, Y1, Y2 = {3} cm?)\n";
        internal const double J_EDWYLimit = 100.0;
        internal const string errorJ_EDW = "Camp {0}: S'ha superat l'overtravel permès de la EDW{1}º.\n(Y1(OUT)/Y2(IN) > {2} cm.)\n";
        internal const string warningJ_EDW = "Camp {0}: L'overtravel de la EDW{1}º està just al límit permès.\n(Y1(OUT)/Y2(IN) = {2} cm?)\n";
        internal const double J_wedgeLimit_153045 = 100.0, J_wedgeLimit_60 = 75.0;
        internal const string errorJ_wedge = "Camp {0}: S'han superat les mides de camp permeses de la W{1}º.\n(X1,X2(LEFT/RIGHT), Y1,Y2(IN/OUT) > {2} cm.)\n";
        internal const string warningJ_wedge = "Camp {0}: Les mides de camp de la W{1}º estan just al límit permès.\n(X1,X2(LEFT/RIGHT), Y1,Y2(IN/OUT) = {2} cm?)\n";
        internal const double J_openLimit = 200.0;
        internal const string errorJ_open = "Camp {0}: S'han superat les mides de camp permeses.\n(X1, X2, Y1, Y2 > {1} cm.)\n";
        internal const string warningJ_open = "Camp {0}: Les mides de camp estan just al límit permès.\n(X1, X2, Y1, Y2 = {1} cm?)\n";
        internal const double J_overTreatXLimit = 20.0, J_overTreatYLimit = 100.0;
        internal const string errorJ_overTreat = "Camp {0}: S'ha superat l'overtravel permès.\n(X1, X2 > {1} cm; Y1, Y2 > {2} cm.)\n";
        internal const string warningJ_overTreat = "Camp {0}: L'overtravel està just al límit permès.\n(X1, X2 = {1} cm? Y1, Y2 = {2} cm?)\n";
        internal const double J_smallThreshold = 35.0;
        internal const string warningJ_small = "Camp {0}: Les mídes de camp no arriben al mínim recomanat.\n(X, Y < {1} cm?)\n";
        internal const double J_hemiSens = 5.0;
        internal const string warningJ_hemi = "Camp {0}: Alguna mandíbula coincideix pràcticament amb l'eix central del feix (< {1} cm).\n(Hemicamp: X1, X2, Y1, Y2 = 0.0 cm?)\n";

        internal const string errorMLC_out = "Camp {0}: El parell de làmines nº{1} no està correctament tancat.\n(Gap = 0.0 mm.)\n";
        internal const string errorMLC_in = "Camp {0}: El parell de làmines nº{1} està tancat dins del camp de radiació.\n(Retirar el parell de làmines darrere de les mandíbules X.)\n";
        internal const double MLC_gapSens = 5.0;
        internal const string warningMLC_gap = "Camp {0}: El parell de làmines nº{1} està pràcticament tancat dins del camp de radiació (< {2} mm).\n(Tancar el parell de làmines i retirar-lo darrere de les mandíbules X?)\n";
        internal const double MLC_jawGap3DSens = 2.0, MLC_jawGapIMRTSens = 3.0;
        internal const string errorMLC_edge = "Camp {0}: Falta d'ajust entre l'MLC i les mandíbules X (<{1} mm).\n(Ajust = 0.0 mm.)\n";

        internal const string errorID_DRR = "Camp {0}: DRR inexistent.\n(Tots els camps han de tenir una imatge (DRR) de referència.)\n";
        internal const string warningID_DRRID = "Camp {0}: Incoherència de la ID de la DRR.\n(ID: IDcamp-DRR.)\n";
        internal const string warningID_nameID = "Camp {0}: Incoherència entre nom i ID.\n(ID = nom.)\n";
        internal const string warningID_setup = "Camp {0}: ID no habitual.\n(ID: SETUP 0º / SETUP 270º / CBCT / FLUORO?)\n";
        internal const string warningID_field = "Camp {0}: ID no habitual.\n(ID sense couch: gantryº...\nID amb couch: gantryº-couchº...)\n";

        internal const string errorCH_setup = "Camp {0}: couch incorrecte per un camp de setup.\n(Couch = 0º.)\n";
        internal const double CH_couchThreshold = 5.0;
        internal const string warningCH_couch = "Camp {0}: angle de couch molt petit (<{1}º).\n(Couch innecessari?)\n";
        internal const double CH_couchClinacLimit = 45.0;
        internal const string errorCH_couchClinac = "Camp {0}: sentit de couch inapropiat per a aquest Clinac.\n(Girar la taula en sentit invers?)\n";

        internal const string errorSF_collimator = "Camp {0}: Gir de col·limador incorrecte.\n(Col·limador = 0.0º.)\n";
        internal const string errorSF_electrons = "Configuració dels camps de setup incorrecte.\n(Electrons: només 1 camp de setup amb gantry = 0º i 0/1 camps de setup amb gantry = 270º.)\n";
        internal const string errorSF_270 = "Configuració del camps de setup a 270º incorrecte.\n(General: 1 camp.)\n";
        internal const string errorSF_0 = "Configuració dels camps de setup a 0º incorrecte\n(Clinac1: 1 camp (no CBCT).\nClinac2 i 3: 2 camps (un d'ells CBCT).)\n";
        internal const string errorSF_fluoro = "S'han trobat {0} camps de setup fora de les posicions naturals (0º i 270º).\n(Clinac1 i 3: sense camps de fluoroscòpia.\nClinac2: si n'hi ha, 1 camp de fluoroscòpia.)\n";
        internal const double SF_fluoroLeftMin = 280.0, SF_fluoroLeftMax = 350.0, SF_fluoroRightMin = 190.0, SF_fluoroRightMax = 260.0;
        internal const string warningSF_fluoro = "Configuració del camp de fluoroscòpia incorrecte (gantry = {0}º)?\n(Mama dreta: {1}º < gantry < {2}º. Mama esquerre: {3}º < gantry < {4}º.)\n";

        internal const double Iso_precisionSens = 0.05, Iso_shiftLimit = 5.0;
        internal const string errorIso_singleIso = "S'han trobat camps amb diferents isocentres.\n(Isocentre únic per pla.)\n";
        internal const string warningIso_rounding = "Arrodoniment de coordenades d'isocentre inadequat.\n(Arrodonir els decimals fins a 0.1 cm?\nCoordenades = 0.0 si són <{0} cm?)\n";
        internal const string errorIso_singleTech = "S'han trobat camps amb diferents tècniques de tractament.\n(Tècnica única per pla (isocèntrica/isomètrica/etc.).)\n";
        internal const string errorIso_Technique = "Tècnica de tractament incorrecte.\n(Fotons: tècnica isocèntrica.\nElectrons: tècnica isomètrica.)\n";
        internal const double Iso_asymThreshold = 2.0, Iso_ratioAsymThreshold = 0.3;
        internal const string warningIso_asymetric = "S'han detectat un {0:F1}% / {1:F1}% de les mandíbules X / Y dels camps de tractament (>{2}%) significativament asimètriques (<>{3}:1).\n(La posició de l'isocentre està correctament centrada?)\n";

        internal const string errorCl_multiple = "Multiplicitat d'unitats de tractament.\n(Tots els camps han d'estar definits al mateix Clinac.)\n";
        internal const string Cl_IMRT = "Clinac1_2100CD";
        internal const string errorCl_IMRT = "Selecció inadequada de la unitat de tractament per a una tècnica d'IMRT.\n(IMRT: Clinac2 o Clinac3.)\n";
        internal const string Cl_SBRT = "Clinac2_2100CD";
        internal const string errorCl_SBRT = "Selecció inadequada de la unitat de tractament per a una tècnica d'SBRT.\n(SBRT: Clinac2.)\n";

        internal const string Alg_photonVer = "AAA_13535", A_photonRes = "0.25", A_photonNorm = "100% to isocenter", A_photonHeter = "ON";
        internal const string errorAlg_photonConfig = "Configuració de l'algorisme de fotons incorrecte.\n(Algorisme: {0}.\nCalculationGridSizeInCM: {1}.\nFieldNormalizationType: {2}.\nHeterogeneityCorrection: {3}.)\n";
        internal const string Alg_elecVer = "eMC_13535", A_elecAcc = "1", A_elecPart = "0", A_elecSmoothLevel = "Low", A_elecSmoothMethod = "3-D_Gaussian";
        internal const string errorAlg_elecConfig = "Configuració de l'algorisme d'electrons incorrecte.\n(Algorisme: {0}.\nAccuracy: {1}.\nNumberOfParticleHistories: {2}.\nSmoothingLevel: {3}.\nSmoothingMethod: {4}.)\n";
        internal const string Alg_elecRes_6912 = "0.15", A_elecRes_16 = "0.20", A_elecRes_20 = "0.25";
        internal const string errorAlg_elecRes = "Resolució del càlcul d'electrons incorrecte.\n({0}: CalculationGridSizeInCM = {1}.)\n";

        internal const double IMRT_meanMULimit = 200.0;
        internal const string warningIMRT_meanMU = "Total = {0} UM, Mitjana = {1:F1} UM/camp.\n(Suavitzar més les fluències?\nRecomanable < {2} UM/camp.)\n";
        internal const double IMRT_NTODistanceLimit = 10.0, IMRT_NTOStartDose = 100.0, IMRT_NTOEndDoseLimit = 10.0, IMRT_NTOFallOffMin = 0.05, IMRT_NTOFallOffMax = 0.2, IMRT_NTOPriority = 150.0;
        internal const string errorIMRT_NTO = "Paràmetres NTO inexistents o no habituals.\n(Prioridad = {0}.\nDistancia desde borde objetivo <= {1} cm.\nDosis inicial = {2}%.\nDosis final <= {3}%.\n{4} <= Reducción <= {5}.)\n";
        internal const double IMRT_smoothMin = 150.0, IMRT_smoothMax = 200.0;
        internal const string warningIMRT_smooth = "Suavitzats de fluències no habituals.\n({0} <= XLiso, YLiso <= {1}.)\n";
        internal const double IMRT_resPointsThreshold = 2000.0, IMRT_resVolumeVsVoxelCorr = 1.11, IMRT_resVolumeThreshold = 2220.0;
        internal const string errorIMRT_res = "Resolució de les següents estructures incorrecte:\n  - {0}\n(>{1} punts si >{2} cm3.)\n";
        internal const double IMRT_resBody = 10.0;
        internal const string errorIMRT_resBody = "Resolució del BODY incorrecte.\n(Resolució BODY = {0} mm.)\n";
        internal const double IMRT_resPTVLimit = 2.5;
        internal const string errorIMRT_resPTV = "Resolució dels següents PTVs incorrecte:\n  - {0}\n(Resolució PTV <= {1} mm.)\n";
        internal const string errorIMRT_objBody = "S'han trobat objectius d'optimització al BODY.\n(BODY només ha de tenir objectius NTO.)\n";
        internal const double IMRT_objPTVPriorityMin = 250.0, IMRT_objPTVPriorityMax = 350.0;
        internal const string warningIMRT_objPTVPriority = "Prioritats no habituals en els següents PTVs:\n  - {0}\n({1} <= prioritat <= {2}.)\n";
        internal const string warningIMRT_objPTVNumber = "Nombre no habitual d'objectius d'optimització en els següents PTVs:\n  - {0}\n(Els PTVs s'optimitzen amb 1 objectiu inferior i 1 ó 2 objectius superiors.)\n";
        internal const double IMRT_objPTVLowerVolumeThreshold = 99.0;
        internal const string warningIMRT_objPTVLower = "Volum no habitual de l'objectiu inferior del PTV {0}.\n(V >= {1}%.)\n";
        internal const double IMRT_objPTVUpperLowerDiffLimit = 1.0;
        internal const string warningIMRT_objPTVUpperDose = "Dosi dels objectius d'optimització superiors (Dsup) incoherent amb la dosi de l'objectiu d'optimització inferior (Dinf) del PTV {0}.\n(Dinf <= Dsup (si n'hi ha dos, el menor) <= Dinf + {1} Gy.)\n";
        internal const double IMRT_objPTVUpperVolumeLimitHard = 1.0, IMRT_objPTVUpperVolumeLimitSoft = 30.0;
        internal const string warningIMRT_objPTVUpperVolume = "Volum no habitual dels objectius superiors del PTV {0}.\n(V < {1}%. Si n'hi ha dos, V < {2}% pel menor, i Volum del major < Volum del menor.)\n"; 
        internal const double IMRT_objOARPriorityMax = 250;
        internal const string warningIMRT_objOARPriority = "Prioritats no habituals en les següents estructures:\n  - {0}\n(prioritat <= {1}.)\n";
        internal const string warningIMRT_objOARNumber = "Nombre no habitual d'objectius d'optimització en les següents estructures:\n  - {0}\n(Els OAR/estructures auxiliars s'optimitzen sense objectius inferiors i 1 ó 2 objectius superiors.)\n";
        internal const double IMRT_OARUpperVolumeLimit = 50;
        internal const string warningIMRT_objOARUpperVolume = "Volum no habitual dels objectius superiors de l'estructura {0}.\n(V < {1}%. Si n'hi ha dos, Volum del major < Volum del menor.)\n";

        internal const double CK_meanGantryRange = 10.0;
        internal const string errorCK_extended = "Camp {0}: Sentit de gir del gantry (extended) incorrecte.\n(ID: gantryEº...)\n";
        internal const double CK_IMRTXLimit = 140.0, CK_IMRTJawDiffThreshold = 20.0;
        internal const string errorCK_IMRTX = "Camp {0}: Divisió de camps innecessària (X > {1} cm).\n(Canviar X <-> Y o optimitzar el gir de col·limador per evitar la divisió.)\n";
        internal const string warningCK_IMRTJawDiff = "Camp {0}: Gir de col·limador no optimitzat (X - Y > {1} cm).\n(Optimitzar el gir de col·limador per minimitzar el recorregut de les làmines?)\n";
        internal const double CK_presicionSens = 0.05, CK_colVsGantryVelRatio = 7.0 / 12.0;
        internal const double CK_partialExcursionThreshold = 60.0, CK_partialExcursionLimit = 120.0;
        internal const string warningCK_colPartialExcursion = "Camp {0}: Gir de col·limador d'anada-i-tornada >{1}º.\n(Optimitzar el gir col·limador entre els camps adjacents?)\n";
        internal const double CK_colMaxExcursionLimit = 120.0;
        internal const string warningCK_colTotalExcursion = "Rang de gir del col·limador excessiu.\n(Gir: max - min < {0}º?)\n";

        internal const string errorCC_clinac = "Multiplicitat d'unitats de tractament.\n(Tots els plans han d'estar calculats al mateix Clinac.)\n";
        internal const double CC_isoShiftLimit = 15.0, CC_precisionSens = 0.05;
        internal const string warningCC_isocenter = "S'han trobat plans amb isocentres que disten <{0} cm.\n(Aquests plans poden compartir isocentre?)\n";
        
    }

}