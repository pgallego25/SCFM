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
    internal static class TolAndMess
    {
        // Classe que agrupa les toleràncies i els missatges d'error/alerta de l'Script.

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
