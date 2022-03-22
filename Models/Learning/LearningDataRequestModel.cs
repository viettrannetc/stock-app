using System.Collections.Generic;

namespace DotNetCoreSqlDb.Models.Learning
{
    /// <summary>    
    /// Data : [
    ///     {
    ///         Combination: "A, B, A-B",
    ///         Result: true
    ///     },
    ///     {
    ///         Combination: "A, B",
    ///         Result: true
    ///     },
    /// ]
    /// </summary>
    public class LearningDataRequestModel
    {
        public LearningDataRequestModel()
        {
            Columns = new List<string>();
        }
        public string FileName { get; set; }
        public List<string> Columns { get; set; }
        public int MinCombination { get; set; }
        public string Condition { get; set; }
        /// <summary>
        /// It should be Q or AB
        /// </summary>
        public string MeasureColumn { get; set; }

        /// <summary>
        /// The succeed percent of the chosen parttern: 50, 80 - not 0.5 or 0.8
        /// </summary>
        public int ExpectedPercentage { get; set; }

        public int minMatchedPattern { get; set; }


    }


    public class LearningDataResponseModel
    {
        public LearningDataResponseModel()
        {
            Pattern = new List<LearningDataPatternResponseModel>();
            PatternWithCodes = new List<LearningDataPatternWithCodeResponseModel>();
        }
        public List<LearningDataPatternResponseModel> Pattern { get; set; }
        public List<LearningDataPatternWithCodeResponseModel> PatternWithCodes { get; set; }
        
    }

    public class LearningDataPatternResponseModel
    {
        public string Pattern { get; set; }
        public decimal Tong { get; set; }
        public decimal Tile { get; set; }
    }

    public class LearningDataPatternWithCodeResponseModel
    {
        public LearningDataPatternWithCodeResponseModel()
        {
            Details = new List<LearningDataPatternWithCodeDetailResponseModel>();
        }
        public string Pattern { get; set; }
        public List<LearningDataPatternWithCodeDetailResponseModel> Details { get; set; }
    }

    public class LearningDataPatternWithCodeDetailResponseModel
    {
        public string Ma { get; set; }
        public decimal Tong { get; set; }
        public decimal Tile { get; set; }
    }

}
