// AUTO-GENERATED -- do not edit by hand.
// Re-run Generate Handles in Velocity Studio to regenerate from the current database state.
// Source:  c_datatype + c_datatype_enumerated_value  (16 types)

namespace Velocity.Handles
{
    // User Defined DataType (UDDT) enumerated-value handles.
    // The engine writes the enumerated_sid as a string into the row's UDF column,
    // so these constants are the raw values that compare against DS.GetUDFValue / column reads.
    public static class UDDT
    {
        /// <summary>Yes and No (datatype_sid=100059)</summary>
        public static class YesAndNo
        {
            /// <summary>Yes (enumerated_sid=404)</summary>
            public const string Yes = "404";
            /// <summary>No (enumerated_sid=405)</summary>
            public const string No = "405";
        }

        /// <summary>Deal Type (datatype_sid=100060)</summary>
        public static class DealType
        {
            /// <summary>Accrual (enumerated_sid=406)</summary>
            public const string Accrual = "406";
            /// <summary>Statement (enumerated_sid=407)</summary>
            public const string Statement = "407";
            /// <summary>Revenue Allocation (enumerated_sid=410)</summary>
            public const string Revenue_Allocation = "410";
            /// <summary>Participation - Accrual (enumerated_sid=438)</summary>
            public const string Participation_Accrual = "438";
            /// <summary>Participation - Statement (enumerated_sid=439)</summary>
            public const string Participation_Statement = "439";
        }

        /// <summary>Explosion Method (datatype_sid=100062)</summary>
        public static class ExplosionMethod
        {
            /// <summary>Percentage Based (enumerated_sid=408)</summary>
            public const string Percentage_Based = "408";
            /// <summary>Unit Based (enumerated_sid=409)</summary>
            public const string Unit_Based = "409";
        }

        /// <summary>Payment on Hold Reason (datatype_sid=100064)</summary>
        public static class PaymentOnHoldReason
        {
            /// <summary>Reason 1 (enumerated_sid=414)</summary>
            public const string Reason_1 = "414";
            /// <summary>Reason 2 (enumerated_sid=415)</summary>
            public const string Reason_2 = "415";
            /// <summary>Reason 3 (enumerated_sid=416)</summary>
            public const string Reason_3 = "416";
        }

        /// <summary>Royalty Method Type (datatype_sid=100066)</summary>
        public static class RoyaltyMethodType
        {
            /// <summary>Standard (enumerated_sid=417)</summary>
            public const string Standard = "417";
            /// <summary>Greater Of (enumerated_sid=418)</summary>
            public const string Greater_Of = "418";
        }

        /// <summary>GL Account (datatype_sid=100067)</summary>
        public static class GLAccount
        {
            /// <summary>Roy Expense (enumerated_sid=419)</summary>
            public const string Roy_Expense = "419";
            /// <summary>Accrual (enumerated_sid=420)</summary>
            public const string Accrual = "420";
            /// <summary>Prepaid (enumerated_sid=421)</summary>
            public const string Prepaid = "421";
            /// <summary>Shortfall Expense (enumerated_sid=422)</summary>
            public const string Shortfall_Expense = "422";
            /// <summary>654789 (enumerated_sid=423)</summary>
            public const string _654789 = "423";
            /// <summary>789654 (enumerated_sid=424)</summary>
            public const string _789654 = "424";
            /// <summary>897456 (enumerated_sid=425)</summary>
            public const string _897456 = "425";
            /// <summary>987456 (enumerated_sid=426)</summary>
            public const string _987456 = "426";
        }

        /// <summary>Bonus Payment Type (datatype_sid=100069)</summary>
        public static class BonusPaymentType
        {
            /// <summary>No Bonus (enumerated_sid=430)</summary>
            public const string No_Bonus = "430";
            /// <summary>Units (enumerated_sid=431)</summary>
            public const string Units = "431";
            /// <summary>Amount (enumerated_sid=432)</summary>
            public const string Amount = "432";
        }

        /// <summary>MAGR_Royalty Method (datatype_sid=100070)</summary>
        public static class MAGR_RoyaltyMethod
        {
            /// <summary>Percent of Amount (enumerated_sid=433)</summary>
            public const string Percent_of_Amount = "433";
            /// <summary>Per Unit (enumerated_sid=434)</summary>
            public const string Per_Unit = "434";
            /// <summary>Percent of List Price (enumerated_sid=435)</summary>
            public const string Percent_of_List_Price = "435";
            /// <summary>Greater of (enumerated_sid=436)</summary>
            public const string Greater_of = "436";
            /// <summary>Lesser of (enumerated_sid=437)</summary>
            public const string Lesser_of = "437";
        }

        /// <summary>MAGR_Imputed License Fee Type (datatype_sid=100071)</summary>
        public static class MAGR_ImputedLicenseFeeType
        {
            /// <summary>Fixed Amount (enumerated_sid=440)</summary>
            public const string Fixed_Amount = "440";
            /// <summary>Percent of Budget (enumerated_sid=441)</summary>
            public const string Percent_of_Budget = "441";
            /// <summary>Percent of Cost of Production (enumerated_sid=442)</summary>
            public const string Percent_of_Cost_of_Production = "442";
        }

        /// <summary>MAGR_Imputed License Fee Trigger (datatype_sid=100072)</summary>
        public static class MAGR_ImputedLicenseFeeTrigger
        {
            /// <summary>Final Delivery Date (enumerated_sid=444)</summary>
            public const string Final_Delivery_Date = "444";
            /// <summary>Preparation of Photography (enumerated_sid=445)</summary>
            public const string Preparation_of_Photography = "445";
            /// <summary>Beginning of Photography (enumerated_sid=446)</summary>
            public const string Beginning_of_Photography = "446";
        }

        /// <summary>MAGR_3rd Party Expense Share Choice (datatype_sid=100073)</summary>
        public static class MAGR_3rdPartyExpenseShareChoice
        {
            /// <summary>Total Participant Share (enumerated_sid=447)</summary>
            public const string Total_Participant_Share = "447";
            /// <summary>Modified Adjusted Gross Receipts (enumerated_sid=448)</summary>
            public const string Modified_Adjusted_Gross_Receipts = "448";
            /// <summary>NONE (enumerated_sid=452)</summary>
            public const string NONE = "452";
        }

        /// <summary>MAGR_3rd Party COP Share Choice (datatype_sid=100075)</summary>
        public static class MAGR_3rdPartyCOPShareChoice
        {
            /// <summary>Total Participant Share (enumerated_sid=453)</summary>
            public const string Total_Participant_Share = "453";
            /// <summary>Modified Adjusted Gross Receipts (enumerated_sid=454)</summary>
            public const string Modified_Adjusted_Gross_Receipts = "454";
            /// <summary>NONE (enumerated_sid=455)</summary>
            public const string NONE = "455";
        }

        /// <summary>MAGR_3rd Party (MAGR) Share Choice (datatype_sid=100076)</summary>
        public static class MAGR_3rdPartyMAGRShareChoice
        {
            /// <summary>NONE (enumerated_sid=456)</summary>
            public const string NONE = "456";
            /// <summary>Total Participant Share (enumerated_sid=457)</summary>
            public const string Total_Participant_Share = "457";
            /// <summary>Modified Adjusted Gross Receipts (enumerated_sid=458)</summary>
            public const string Modified_Adjusted_Gross_Receipts = "458";
        }

        /// <summary>MAGR_3rd Party (Participant Share) Share Choice (datatype_sid=100078)</summary>
        public static class MAGR_3rdPartyParticipantShareShareChoice
        {
            /// <summary>Total Participant Share (enumerated_sid=463)</summary>
            public const string Total_Participant_Share = "463";
            /// <summary>NONE (enumerated_sid=464)</summary>
            public const string NONE = "464";
            /// <summary>Modified Adjusted Gross Receipts (enumerated_sid=465)</summary>
            public const string Modified_Adjusted_Gross_Receipts = "465";
        }

        /// <summary>MAGR_3rd Party Deduction Cap Type (datatype_sid=100079)</summary>
        public static class MAGR_3rdPartyDeductionCapType
        {
            /// <summary>No Floor (enumerated_sid=466)</summary>
            public const string No_Floor = "466";
            /// <summary>Hard Floor (enumerated_sid=467)</summary>
            public const string Hard_Floor = "467";
            /// <summary>Hard and Soft Floor (enumerated_sid=468)</summary>
            public const string Hard_and_Soft_Floor = "468";
        }

        /// <summary>MAGR_Interest Balance Type (datatype_sid=100080)</summary>
        public static class MAGR_InterestBalanceType
        {
            /// <summary>No Interest (enumerated_sid=469)</summary>
            public const string No_Interest = "469";
            /// <summary>Beginning of Month (enumerated_sid=470)</summary>
            public const string Beginning_of_Month = "470";
            /// <summary>Monthly Average (enumerated_sid=471)</summary>
            public const string Monthly_Average = "471";
            /// <summary>End of Month (enumerated_sid=472)</summary>
            public const string End_of_Month = "472";
        }
    }
}
