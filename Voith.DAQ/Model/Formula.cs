using System;
using SqlSugar;

namespace Voith.DAQ.Model
{
    class Formula
    {
        [SugarColumn(IsNullable = false, IsPrimaryKey = true, IsIdentity = true)]
        public short Id { get; set; }

        [SugarColumn(IsNullable = false, Length = 10)]
        public string FormulaNum { get; set; }

        [SugarColumn(IsNullable = false, Length = 20)]
        public string StationName { get; set; }

        [SugarColumn(IsNullable = false)]
        public short WorkStep { get; set; }


        [SugarColumn(IsNullable = false)]
        public short OperationTypeId { get; set; }


        [SugarColumn(IsNullable = true, Length = 100)]
        public string OperationTypeContent { get; set; }


        [SugarColumn(IsNullable = true, Length = 200)]
        public string ActionDescription { get; set; }

        [SugarColumn(IsNullable = true)]
        public short Parameter1 { get; set; }

        [SugarColumn(IsNullable = true)]
        public short Parameter2 { get; set; }

        [SugarColumn(IsNullable = true)]
        public short Parameter3 { get; set; }

        [SugarColumn(IsNullable = true)]
        public short Parameter4 { get; set; }

        [SugarColumn(IsNullable = true)]
        public short Parameter5 { get; set; }

        [SugarColumn(IsNullable = true)]
        public short Parameter6 { get; set; }

        [SugarColumn(IsNullable = true)]
        public short Parameter7 { get; set; }

        [SugarColumn(IsNullable = true)]
        public short Parameter8 { get; set; }

        [SugarColumn(IsNullable = true)]
        public short Parameter9 { get; set; }

        [SugarColumn(IsNullable = true)]
        public short Parameter10 { get; set; }

        [SugarColumn(IsNullable = true)]
        public short Parameter11 { get; set; }

        [SugarColumn(IsNullable = true)]
        public short Parameter12 { get; set; }

        [SugarColumn(IsNullable = true)]
        public short Parameter13 { get; set; }

        [SugarColumn(IsNullable = true)]
        public short Parameter14 { get; set; }

        [SugarColumn(IsNullable = true)]
        public short Parameter15 { get; set; }

        [SugarColumn(IsNullable = true)]
        public short Parameter16 { get; set; }

        [SugarColumn(IsNullable = true)]
        public short Parameter17 { get; set; }

        [SugarColumn(IsNullable = true)]
        public short Parameter18 { get; set; }

        [SugarColumn(IsNullable = true)]
        public short Parameter19 { get; set; }

        [SugarColumn(IsNullable = true)]
        public short Parameter20 { get; set; }

        [SugarColumn(IsNullable = true)]
        public short Parameter21 { get; set; }

        [SugarColumn(IsNullable = true)]
        public short Parameter22 { get; set; }

        [SugarColumn(IsNullable = true)]
        public short Parameter23 { get; set; }

        [SugarColumn(IsNullable = true)]
        public short Parameter24 { get; set; }

        [SugarColumn(IsNullable = true)]
        public short Parameter25 { get; set; }

        [SugarColumn(IsNullable = false, Length = 20)]
        public string FeatureCode1 { get; set; }

        [SugarColumn(IsNullable = false, Length = 20)]
        public string FeatureCode2 { get; set; }

        [SugarColumn(IsNullable = false, Length = 20)]
        public string FeatureCode3 { get; set; }

        [SugarColumn(IsNullable = false, Length = 20)]
        public string FeatureCode4 { get; set; }

        [SugarColumn(IsNullable = false, Length = 20)]
        public string FeatureCode5 { get; set; }

        [SugarColumn(IsNullable = false, Length = 20)]
        public string FeatureCode6 { get; set; }

        [SugarColumn(IsNullable = false, Length = 20)]
        public string FeatureCode7 { get; set; }

        [SugarColumn(IsNullable = false, Length = 20)]
        public string FeatureCode8 { get; set; }

        [SugarColumn(IsNullable = true, DefaultValue = "GetDate()")]
        public DateTime UpdateTime { get; set; } = DateTime.Now;
    }
}
