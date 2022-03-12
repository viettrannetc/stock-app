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
            Data = new List<LearningDataModel>();
        }
        public List<LearningDataModel> Data { get; set; }

        /// <summary>
        /// Pattern: 
        ///     + X-Y-Z
        ///     + Times
        ///     + Success
        /// </summary>
        public List<LearningDataModel> Data2 { get; set; }
    }

    public class LearningDataModel
    {
        public string Combination { get; set; }
        public bool Result { get; set; }
    }


}
