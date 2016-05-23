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
}
