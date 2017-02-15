namespace SCFM
{

    /// <summary>List of all possible comparison operators between actual values and reference/tolerance values.</summary>
    /// <remarks>This Enum is used by <see cref="SCFM.ReportSnippet"/> to establish the underlying relation between actual value and reference/tolerance concepts.</remarks>
    public enum ComparisonMode
    {

        /// <summary>Equality between actual value and reference value.</summary>
        /// <remarks>It is an appropriate operator when the concept of tolerance does not apply, and the comparison is performed against a single reference value.</remarks>
        equal,

        /// <summary>Inequality between actual value and reference value.</summary>
        /// <remarks>It is an appropriate operator when the concept of tolerance does not apply, and the comparison is performed against a single reference value..</remarks>
        notEqual,

        /// <summary>Actual value must be greater or equal than a given reference/tolerance value.</summary>
        /// <remarks>
        /// It is an appropriate operator when the concept of tolerance does not apply, and the comparison is performed against a single reference value.
        /// Alternatively, it is an appropriate operator when a two-fold tolerance concept apply (e.g. the comparison is performed against a lower and an upper threshold) and, therefore, the reference concept is meaningless.
        /// </remarks>
        greaterEqual,

        /// <summary>Actual value must be greater than a given reference/tolerance value.</summary>
        /// <remarks>
        /// It is an appropriate operator when the concept of tolerance does not apply, and the comparison is performed against a single reference value.
        /// Alternatively, it is an appropriate operator when a two-fold tolerance concept apply (e.g. the comparison is performed against a lower and an upper threshold) and, therefore, the reference concept is meaningless.
        /// </remarks>
        greaterThan,

        /// <summary>Actual value must be lesser or equal than a given reference/tolerance value.</summary>
        /// <remarks>
        /// It is an appropriate operator when the concept of tolerance does not apply, and the comparison is performed against a single reference value.
        /// Alternatively, it is an appropriate operator when a two-fold tolerance concept apply (e.g. the comparison is performed against a lower and an upper threshold) and, therefore, the reference concept is meaningless.
        /// </remarks>
        lessEqual,

        /// <summary>Actual value must be lesser than a given reference/tolerance value.</summary>
        /// <remarks>
        /// It is an appropriate operator when the concept of tolerance does not apply, and the comparison is performed against a single reference value.
        /// Alternatively, it is an appropriate operator when a two-fold tolerance concept apply (e.g. the comparison is performed against a lower and an upper threshold) and, therefore, the reference concept is meaningless.
        /// </remarks>
        lessThan,

        /// <summary>Actual value must lie within a range of values (closed interval).</summary>
        /// <remarks>
        /// It is an approprate operator when the concept of reference does not apply, and the comparison is performed against two fixed thresholds (or tolerances).
        /// Alternatively, it is an appropriate operator when a symmertric range (defined by a single tolerance) around a single refence value applies.
        /// </remarks>
        betweenEqual,

        /// <summary>Actual value must lie within a range of values (open interval).</summary>
        /// <remarks>
        /// It is an approprate operator when the concept of reference does not apply, and the comparison is performed against two fixed thresholds (or tolerances).
        /// Alternatively, it is an appropriate operator when a symmertric range (defined by a single tolerance) around a single refence value applies.
        /// </remarks>
        betweenThan,

        /// <summary>Actual value must lie outside a range of values (closed interval).</summary>
        /// <remarks>
        /// It is an approprate operator when the concept of reference does not apply, and the comparison is performed against two fixed thresholds (or tolerances).
        /// Alternatively, it is an appropriate operator when a symmertric range (defined by a single tolerance) around a single refence value applies.
        /// </remarks>
        notBetweenEqual,

        /// <summary>Actual value must lie within a range of values (open interval).</summary>
        /// <remarks>
        /// It is an approprate operator when the concept of reference does not apply, and the comparison is performed against two fixed thresholds (or tolerances).
        /// Alternatively, it is an appropriate operator when a symmertric range (defined by a single tolerance) around a single refence value applies.
        /// </remarks>
        notBetweenThan

    }

}