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
    }


    public class LearningDataResponseModel
    {
        public LearningDataResponseModel()
        {
            Pattern = new List<LearningDataPatternResponseModel>();
        }
        public List<LearningDataPatternResponseModel> Pattern { get; set; }
    }

    public class LearningDataPatternResponseModel
    {
        public string Pattern { get; set; }
        public decimal Tong { get; set; }
        public decimal Tile { get; set; }
    }

}
