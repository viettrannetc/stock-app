using DotNetCoreSqlDb.Models.Business;
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
    public class LearningModel
    {
        public LearningModel()
        {
            Data = new List<LearningDataPatternResponseModel>();
        }
        public List<LearningDataPatternResponseModel> Data { get; set; }
    }

    //public class LearningDataModel
    //{
    //    public string Combination { get; set; }

    //    public bool Result { get; set; }
    //}

    public class LearningDataConditionModel
    {
        public LearningDataConditionModel()
        {
            Condition = new Dictionary<EnumExcelColumnModel, bool>();
        }
        public Dictionary<EnumExcelColumnModel, bool> Condition { get; set; }
    }
}
