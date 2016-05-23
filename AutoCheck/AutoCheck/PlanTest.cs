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
}
