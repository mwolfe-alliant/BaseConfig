// Hand-maintained — base library column handle mapping.
// Sits between EngineCol (internal UDKeyN names) and CustCol (customer-specific names).
// These names are meaningful across customers sharing the same library configuration.
// Not database-backed — update this file when the slot-to-concept mapping changes.
//
// Three-tier naming:
//   EngineCol.UDKey2        — internal engine slot name
//   BaseCol.ActivityType    — shared library name (this file)
//   CustCol.Account         — customer-specific alias (Customer\Generated\CustCol.cs)

using VelocityProto;

namespace VelocityProto.Handles
{
    // Additional Comment
    // Base library column handle mapping.
    // Usage:  new MathOperation(BaseCol.ActivityType, MathOp.SETTO, Account.NegEarnBal)
    //         rs.GetData(BaseCol.Currency, CompareOp.EQ, Currency.USD)
    public static class BaseCol
    {
        // ── UDKey config slots ───────────────────────────────────────────────
        /// <summary>The Product dimension (UDKey1).</summary>
        public static readonly UDKey1Col Product = EngineCol.UDKey1;
        /// <summary>The activity or account type dimension (UDKey2).</summary>
        public static readonly UDKey2Col  ActivityType = EngineCol.UDKey2;
        /// <summary>Transaction type (current, ITD, statement, etc.) (UDKey3).</summary>
        public static readonly UDKey3Col  TransType    = EngineCol.UDKey3;
        /// <summary>UDKey4 — not yet mapped to a base concept.</summary>
        public static readonly UDKey4Col  UDKey4       = EngineCol.UDKey4;
        /// <summary>Sales territory or geographic region (UDKey5).</summary>
        public static readonly UDKey5Col  Territory    = EngineCol.UDKey5;
        /// <summary>Product type or content category (UDKey7).</summary>
        public static readonly UDKey7Col  ProductType  = EngineCol.UDKey7;
        /// <summary>UDKey8 — not yet mapped to a base concept.</summary>
        public static readonly UDKey8Col  UDKey8       = EngineCol.UDKey8;
        /// <summary>Brand or label (UDKey9).</summary>
        public static readonly UDKey9Col  Brand        = EngineCol.UDKey9;
        /// <summary>Transaction currency (UDKey10).</summary>
        public static readonly UDKey10Col Currency     = EngineCol.UDKey10;
        /// <summary>Royalty tier or rate tier (UDKey11).</summary>
        public static readonly UDKey11Col Tier         = EngineCol.UDKey11;
        /// <summary>UDKey12 — not yet mapped to a base concept.</summary>
        public static readonly UDKey12Col UDKey12      = EngineCol.UDKey12;
        /// <summary>UDKey13 — not yet mapped to a base concept.</summary>
        public static readonly UDKey13Col UDKey13      = EngineCol.UDKey13;
        /// <summary>UDKey14 — not yet mapped to a base concept.</summary>
        public static readonly UDKey14Col UDKey14      = EngineCol.UDKey14;
        /// <summary>Exclusivity type (exclusive, non-exclusive, etc.) (UDKey16).</summary>
        public static readonly UDKey16Col Exclusivity  = EngineCol.UDKey16;
        /// <summary>UDKey18 — not yet mapped to a base concept.</summary>
        public static readonly UDKey18Col UDKey18      = EngineCol.UDKey18;
        /// <summary>UDKey19 — not yet mapped to a base concept.</summary>
        public static readonly UDKey19Col UDKey19      = EngineCol.UDKey19;
        /// <summary>UDKey20 — not yet mapped to a base concept.</summary>
        public static readonly UDKey20Col UDKey20      = EngineCol.UDKey20;
    }
}
