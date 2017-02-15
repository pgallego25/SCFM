namespace SCFM
{

    /// <summary>List of possible results of a test within the SCFM project.</summary>
    public enum InfoLevel
    {

        /// <summary>Undefined/uninitialized state.</summary>
        undefined = 0,

        /// <summary>The test issues a "pass/success/green" score.</summary>
        GREEN = 1,

        /// <summary>The test issues a "warning/yellow" score.</summary>
        YELLOW = 2,

        /// <summary>The test issues a "fail/error/red" score.</summary>
        RED = 3

    }

}