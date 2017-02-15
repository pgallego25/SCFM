namespace SCFM
{

    /// <summary>List of structure types.</summary>
    public enum StructureType
    {

        /// <summary>Arbitrary structure that can hold any structure apart from OARs and PTVs: body, region of dose, optimization structure, etc.</summary>
        structure = 0,

        /// <summary>OAR structure.</summary>
        OAR = 1,

        /// <summary>PTV structure.</summary>
        PTV = 2
        
    }

}
