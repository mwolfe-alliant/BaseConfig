// AUTO-GENERATED -- do not edit by hand.
// Re-run Generate Handles in Alliant UI to regenerate from the current database state.
// Source:  c_entity_terminology_locale (UDKey slot -> base name mapping).
//
// Three-tier naming:
//   EngineCol.UDKey2        - internal engine slot name
//   BaseCol.ActivityType    - shared library name (this file)
//   CustCol.Account         - customer-specific alias

using VelocityProto;

namespace VelocityProto.Handles
{
    public static class BaseCol
    {
        // -- UDKey config slots ----------------------------------------------------
        /// <summary>Catalog (UDKey1).</summary>
        public static readonly UDKey1Col Catalog = EngineCol.UDKey1;
        /// <summary>Activity Type (UDKey2).</summary>
        public static readonly UDKey2Col ActivityType = EngineCol.UDKey2;
        /// <summary>Transaction Type (UDKey3).</summary>
        public static readonly UDKey3Col TransType = EngineCol.UDKey3;
        /// <summary>Channel (UDKey4).</summary>
        public static readonly UDKey4Col Channel = EngineCol.UDKey4;
        /// <summary>Territory (UDKey5).</summary>
        public static readonly UDKey5Col Territory = EngineCol.UDKey5;
        /// <summary>Media (UDKey6).</summary>
        public static readonly UDKey6Col Media = EngineCol.UDKey6;
        /// <summary>Language (UDKey7).</summary>
        public static readonly UDKey7Col Language = EngineCol.UDKey7;
        /// <summary>Bundle (UDKey8).</summary>
        public static readonly UDKey8Col Bundle = EngineCol.UDKey8;
        /// <summary>Rights Type (UDKey9).</summary>
        public static readonly UDKey9Col RightsType = EngineCol.UDKey9;
        /// <summary>Format (UDKey10).</summary>
        public static readonly UDKey10Col Format = EngineCol.UDKey10;
        /// <summary>Customer (UDKey11).</summary>
        public static readonly UDKey11Col Customer = EngineCol.UDKey11;
        /// <summary>Provider (UDKey12).</summary>
        public static readonly UDKey12Col Provider = EngineCol.UDKey12;
        /// <summary>Payment Method Approval Type (UDKey13).</summary>
        public static readonly UDKey13Col PaymentMethodApprovalType = EngineCol.UDKey13;
        /// <summary>Tier (UDKey14).</summary>
        public static readonly UDKey14Col Tier = EngineCol.UDKey14;
        /// <summary>Recoupment Group (UDKey15).</summary>
        public static readonly UDKey15Col RecoupmentGroup = EngineCol.UDKey15;
        /// <summary>Royalty Method (UDKey16).</summary>
        public static readonly UDKey16Col RoyaltyMethod = EngineCol.UDKey16;
        /// <summary>Source Currency (UDKey17).</summary>
        public static readonly UDKey17Col SourceCurrency = EngineCol.UDKey17;
        /// <summary>Calculation Currency (UDKey18).</summary>
        public static readonly UDKey18Col CalculationCurrency = EngineCol.UDKey18;
        /// <summary>Division (UDKey19).</summary>
        public static readonly UDKey19Col Division = EngineCol.UDKey19;
        /// <summary>GL Code (UDKey20).</summary>
        public static readonly UDKey20Col GLCode = EngineCol.UDKey20;

        // -- Volume / amount columns (fixed engine concepts) ----------------------
        /// <summary>Primary amount column.</summary>
        public static readonly DecimalCol Amount  = new DecimalCol("Amount");
        /// <summary>Primary units column.</summary>
        public static readonly DecimalCol Units   = new DecimalCol("Units");
        /// <summary>Secondary amount column (Scratch1 / "Amount2").</summary>
        public static readonly DecimalCol Amount2 = EngineCol.Amount2;
        /// <summary>Secondary units column (Scratch2 / "Units2").</summary>
        public static readonly DecimalCol Units2  = EngineCol.Units2;
    }
}
