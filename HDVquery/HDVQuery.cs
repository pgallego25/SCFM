using System;
using System.Collections.Generic;
using System.Linq;
using VMS.TPS.Common.Model.API;


namespace SCFM
{

    /// <summary>Static class that holds static methods to provide convenience tools for HDV querying.</summary>
    /// <remarks>It is highly recommended to perform all HDVQueries within the SCFM project by using these tools.</remarks>
    public static class HDVQuery
    {

        /// <summary>The full list of constraints that apply to the current patient/plan.</summary>
        /// <remarks>Every constraint in the list must have all its properties initialized with valid values. Otherwise, functionality is not guaranteed.</remarks>
        private static List<Constraint> myConstraints = new List<Constraint>();
        
        /// <summary>Method to add a constraint to <see cref="myConstraints"/>.</summary>
        /// <param name="c">A constraint. Type: <see cref="SCFM.Constraint"/>.</param>
        /// <remarks>
        /// Constrains must have a structure assigned. Otherwise, an exception is raised to prevent poor usage.
        /// If the constraint already exists in <see cref="myConstraints"/>, the method do not perform any action.
        /// </remarks>
        /// <exception cref="System.ArgumentException">The structure of the constraint (c) passed as an argument is lacking.</exception>
        public static void AddConstraint(Constraint c)
        {
            if (c.Structure == null)
                throw new ArgumentException("SCFM exception:\nThe structure of the constraint (c) passed as an argument is lacking.", "c");
            else
            {
                if (!myConstraints.Contains(c))
                    myConstraints.Add(c);
            }
        }
        
        /// <summary>Method to remove a constraint from <see cref="myConstraints"/>.</summary>
        /// <param name="c">A constraint. Type: <see cref="SCFM.Constraint"/>.</param>
        /// <remarks>If no such constraint is found, the method do not perform any action.</remarks>
        public static void RemoveConstraint(Constraint c)
        {
            if (myConstraints.Contains(c))
                myConstraints.Remove(c);
        }
        
        /// <summary>Provides an HDV assessment in a given treatment plan, by assessing each constraint of the given structure (OAR).</summary>
        /// <param name="plan">The current treatment plan. Type: <see cref="VMS.TPS.Common.Model.API.PlanningItem"/>.</param>
        /// <param name="oar">The current structure (OAR). Type: <see cref="VMS.TPS.Common.Model.API.Structure"/>.</param>
        /// <param name="cumulativePrescriptionDose">(Optional parameter: only needed if <paramref name="plan"/> is of type <see cref="VMS.TPS.Common.Model.API.PlanSum"/> and a relative dose assessment is required). The maximum cumulative prescribed dose resulting from the sum of individual prescription doses.</param>
        /// <returns>A list of individual reports (one for each constraint) of the HDV assessment of the current structure in the given treatment plan. Type: <see cref="System.Collections.Generic.List{T}"/> of type <see cref="SCFM.ReportSnippet"/>.</returns>
        /// <exception cref="System.ArgumentException">Invalid structure (oar) and/or no constraints assigned.</exception>
        public static List<ReportSnippet> QuerySingleOAR(PlanningItem plan, Structure oar, double cumulativePrescriptionDose = 0.0)
        {
            if (oar == null || !myConstraints.Any(c => c.Structure == oar))
                throw new ArgumentException("SCFM exception:\nInvalid sturcure (oar) and/or no constraints assigned.", "oar");
            else
            {
                IEnumerable<Constraint> constraintsQuery = myConstraints.Where(c => c.Structure == oar);
                if (constraintsQuery.Any(c => c.StructureType != StructureType.OAR))
                    throw new ArgumentException("SCFM exception:\nOne or more constraints assigned to the structure (oar) passed as an argument do not behave as OAR constraints.", "oar");
                else
                {
                    List<Constraint> constraintsList = constraintsQuery.ToList();
                    List<ReportSnippet> myReport = new List<ReportSnippet>();
                    foreach (Constraint c in constraintsList)
                        myReport.Add(c.GetReport(plan, constraintsList, cumulativePrescriptionDose));
                    return myReport;
                }
            }
        }
        

        public static List<ReportSnippet> QueryAllOAR(PlanningItem plan, IEnumerable<Structure> structureSet, double cumulativePrescriptionDose = 0.0)
        {
            throw new NotImplementedException();
            // seleccionar aquelles estructures de structureSet que responguin a oar. Això ho faig a partir d'aquelles que estan presents a myConstraints i que tenen un comportament d'oar.
            // un cop trobades, iterar per totes elles i fer reportsnippets.
        }

        public static List<ReportSnippet> QuerySinglePTV(PlanningItem plan, Structure ptv, double cumulativePrescriptionDose = 0.0)
        {
            throw new NotImplementedException();
            // fer una implementacio similar a QuerySingleOAR.
        }

        public static List<ReportSnippet> QueryAllPTV(PlanningItem plan, double cumulativePrescriptionDose = 0.0)
        {
            throw new NotImplementedException();
            // seleccionar aquelles estructures de structureSet que responguin a ptv. Això ho faig a partir d'aquelles que estan presents a myConstraints i que tenen un comportament de ptv.
            // un cop trobades, iterar per totes elles i fer reportsnippets.
        }

    }

}

// TODO: implmentar try-catch blocks al voltant de les parts de codi que criden a codi que pot generar excepcions:
// QuerySingleOAR().
// No aplico controls d'integritat sobre PlanningItem perquè no crec que sigui el lloc. Cal posar-los "a baix de tot" (a Constraint)? O "a dalt de tot" (codi client que utilitza aquestes llibreries)? Crec que "més amunt"...