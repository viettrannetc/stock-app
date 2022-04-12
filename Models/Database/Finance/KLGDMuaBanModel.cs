namespace DotNetCoreSqlDb.Models.Database.Finance
{
    public class KLGDMuaBanDataItemModel
    {
    }
    public class KLGDMuaBanDataModel
    {
        //public List<KLGDMuaBanDataItemModel> data { get; set; }//TODO:
        /// <summary>
        /// Mua chủ động
        /// </summary>
        public decimal total_buy { get; set; }
        /// <summary>
        /// Bán chủ động
        /// </summary>
        public decimal total_sell { get; set; }
        /// <summary>
        /// Chưa xác định
        /// </summary>
        public decimal total_unknow { get; set; }
        /// <summary>
        /// Tổng
        /// </summary>
        public decimal total_vol { get; set; }
    }

    public class KLGDMuaBanModel
    {
        public KLGDMuaBanDataModel data { get; set; }
        /// <summary>
        /// 200
        /// </summary>
        public string status { get; set; } //": ,

    }
}
