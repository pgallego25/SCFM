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
                        myTechnique = TechniqueType.TechniqueChoice(myPlan);
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

   
   

}