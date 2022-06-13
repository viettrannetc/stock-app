using System;
using System.ComponentModel.DataAnnotations;

namespace DotNetCoreSqlDb.Models
{
    public class StockSymbol
    {
        public int ID { get; set; }
        /// <summary>
        /// : "A32"
        /// </summary>
        public string _sc_ { get; set; }

        /// <summary>
        /// : 32600.0,
        /// </summary>
        public decimal _bp_ { get; set; }

        /// <summary>
        /// 37400.0,
        /// </summary>
        public decimal _clp_ { get; set; }
        /// <summary>
        /// 27800.0,
        /// </summary>
        public decimal _fp_ { get; set; }
        /// <summary>
        /// 32600.0,
        /// </summary>
        public decimal _op_ { get; set; }
        /// <summary>
        /// 32600.0,
        /// </summary>
        public decimal _cp_ { get; set; }
        /// <summary>
        /// 32600.0,
        /// </summary>
        public decimal _lp_ { get; set; }
        /// <summary>
        /// 0.0,
        /// </summary>
        public decimal change { get; set; }
        /// <summary>
        /// 0.00,
        /// </summary>
        public decimal _pc_ { get; set; }
        /// <summary>
        /// 200.0,
        /// </summary>
        public decimal _tvol_ { get; set; }
        /// <summary>
        /// 6520000.0,
        /// </summary>
        public decimal _tval_ { get; set; }
        /// <summary>
        /// 221.680000,
        /// </summary>
        public decimal _vhtt_ { get; set; }

        /// <summary>
        /// "Sản xuất",
        /// </summary>
        public string _in_ { get; set; }
        /// <summary>
        /// "Sản xuất các sản phẩm da và liên quan",
        /// </summary>
        public string _sin_ { get; set; }
        /// <summary>
        /// 3
        /// </summary>
        public int catID { get; set; }
        /// <summary>
        /// "CTCP 32",
        /// </summary>
        public string stockName { get; set; }
        /// <summary>
        /// 1700.0
        /// </summary>
        public decimal _diviend_ { get; set; }

        public bool BiChanGiaoDich { get; set; }
        public decimal MA20Vol { get; set; }

    }
}

