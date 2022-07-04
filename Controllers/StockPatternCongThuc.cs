using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DotNetCoreSqlDb.Models;
using DotNetCoreSqlDb.Models.Business;
using DotNetCoreSqlDb.Common;
using System.Collections.Concurrent;
using DotNetCoreSqlDb.Models.Business.Report;
using DotNetCoreSqlDb.Models.Business.Report.Implementation;
using System.Text;
using DotNetCoreSqlDb.Models.Prediction;
using Newtonsoft.Json;
using Skender.Stock.Indicators;
using DotNetCoreSqlDb.Models.Learning.RealData;
using DotNetCoreSqlDb.Models.Business.Patterns.LocCoPhieu;
using LinqKit;
using System.Data;

namespace DotNetCoreSqlDb.Controllers
{
    public partial class StockPatternController : Controller
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="ngay"></param>
        /// <param name="ngayCuoi"></param>
        /// <param name="KC2D"></param>
        /// <param name="SoPhienKT"></param>
        /// <param name="CL2D">Chênh lệch 2 đáy: Đáy RSI 1 * 0.98 > Đáy RSI 2 (đáy mới thấp hơn đáy cũ) - đáy 2 phải thấp hơn đáy 1 ít nhất 2%</param>
        /// <returns></returns>
        public async Task<List<string>> RSITestV1(string code, DateTime ngay, DateTime ngayCuoi, int KC2D, int SoPhienKT, decimal CL2D)
        {
            int rsi = 14;

            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");
            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > 100000).ToListAsync()
                : await _context.StockSymbol.Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > 100000 && splitStringCode.Contains(s._sc_)).ToListAsync();
            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var historiesStockCode = await _context.History
                .Where(ss => stockCodes.Contains(ss.StockSymbol)
                    && ss.Date <= ngay.AddDays(10)
                    && ss.Date >= ngayCuoi.AddDays(-(SoPhienKT * 3)))
                .OrderByDescending(ss => ss.Date)
                .ToListAsync();

            var result1 = new List<string>();

            Parallel.ForEach(symbols, symbol =>
            {
                decimal tong = 0;
                decimal dung = 0;
                decimal sai = 0;

                var histories = historiesStockCode
                                    .Where(ss => ss.StockSymbol == symbol._sc_)
                                    .OrderBy(h => h.Date)
                                    .ToList();
                var ngayBatDau = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date >= ngayCuoi);
                for (int i = histories.IndexOf(ngayBatDau); i < histories.Count; i++)
                {
                    ngayBatDau = histories[i];
                    if (ngayBatDau != null && ngayBatDau.HadAllIndicators())
                    {
                        break;
                    }
                }

                var ngayDungLai = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date >= ngay);
                if (ngayDungLai == null) //đang nhập ngày trong tương lai -> chuyển về hiện tại
                {
                    ngayDungLai = histories.OrderByDescending(h => h.Date).First();
                }
                var startedI = histories.IndexOf(ngayBatDau);
                var stoppedI = histories.IndexOf(ngayDungLai);

                for (int i = startedI; i < stoppedI; i++)
                {
                    var buyingDate = histories[i];
                    //Giả định ngày trước đó là đáy
                    var dayGiaDinh = histories[i - 1];
                    //hum nay RSI tăng so với hum wa, thì hum wa mới là đáy, còn ko thì mai RSI vẫn có thể giảm tiếp, ko ai bik
                    if (dayGiaDinh.RSI == 0
                        || buyingDate.RSI <= dayGiaDinh.RSI
                        || buyingDate.NenBot < dayGiaDinh.NenBot)
                    {
                        continue;
                    }

                    //Kiem tra đáy giả định: trong vòng 14 phiên trước không có cây nào trước đó thấp hơn nó
                    var nhungNgaySoSanhVoiDayGiaDinh = histories.OrderByDescending(h => h.Date).Where(h => h.Date < dayGiaDinh.Date).Take(SoPhienKT).ToList();
                    if (nhungNgaySoSanhVoiDayGiaDinh.Count < SoPhienKT) continue;

                    var ngayCoRSIThapNhatDeSoSanhVoiDayGiaDinh = nhungNgaySoSanhVoiDayGiaDinh.OrderBy(h => h.RSI).First();
                    var indexOfDayGiaDinh = histories.IndexOf(dayGiaDinh);
                    var indexOfngayCoRSIThapNhatDeSoSanhVoiDayGiaDinh = histories.IndexOf(ngayCoRSIThapNhatDeSoSanhVoiDayGiaDinh);
                    if (indexOfDayGiaDinh - indexOfngayCoRSIThapNhatDeSoSanhVoiDayGiaDinh <= KC2D) continue; //đáy RSI thấp nhất không dc nằm trong 3 ngày tính từ ngày của đáy giả định trở về trước

                    for (int j = KC2D; j < nhungNgaySoSanhVoiDayGiaDinh.Count - 1; j++)
                    {
                        var ngàyĐếmNgược = nhungNgaySoSanhVoiDayGiaDinh[j];

                        var hasPhanKyDuong = histories.HasPhanKyDuong(dayGiaDinh, ngàyĐếmNgược, "RSI", SoPhienKT, CL2D);
                        if (hasPhanKyDuong != null && hasPhanKyDuong.HasValue && hasPhanKyDuong.Value)
                        {
                            var tileChinhXac = 0;
                            var tPlus = histories.Where(h => h.Date >= buyingDate.Date)
                                .OrderBy(h => h.Date)
                                .Skip(3)
                                .Take(3)
                                .ToList();

                            if (tPlus.Any(t => t.C > buyingDate.O * 1.01M))    //Mình đặt mua ở giá mở cửa ngày hum sau luôn
                            {
                                dung++;
                                result1.Add($"{symbol._sc_} - Đúng - T3-5 lời - Điểm xét: {buyingDate.Date.ToShortDateString()} - Đáy RSI: {dayGiaDinh.Date.ToShortDateString()} RSI {dayGiaDinh.RSI.ToString("N2")} - Giá {dayGiaDinh.NenBot.ToString("N2")} - Điểm tín hiệu: {ngàyĐếmNgược.Date.ToShortDateString()} RSI {ngàyĐếmNgược.RSI.ToString("N2")} - Giá {ngàyĐếmNgược.NenBot.ToString("N2")}");
                            }
                            else
                            {
                                sai++;
                                result1.Add($"{symbol._sc_} - Sai  - T3-5 lỗ  - Điểm xét: {buyingDate.Date.ToShortDateString()} - Đáy RSI: {dayGiaDinh.Date.ToShortDateString()} RSI {dayGiaDinh.RSI.ToString("N2")} - Giá {dayGiaDinh.NenBot.ToString("N2")} - Điểm tín hiệu: {ngàyĐếmNgược.Date.ToShortDateString()} RSI {ngàyĐếmNgược.RSI.ToString("N2")} - Giá {ngàyĐếmNgược.NenBot.ToString("N2")}");
                            }

                        }
                    }
                }
            });

            //tong = dung + sai;
            //var tile = Math.Round(dung / tong, 2);
            //result1.Add($"Tỉ lệ: {tile}");
            return result1;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="ngay"></param>
        /// <param name="ngayCuoi"></param>
        /// <param name="KC2D"></param>
        /// <param name="SoPhienKT"></param>
        /// <param name="propertyName"></param>
        /// <param name="CL2G">Chệnh lệch 2 giá                 ở đáy 1 và đáy 2 - 0.95 -> đáy 1 >= đáy 2 * 0.95</param>
        /// <param name="CL2D">Chệnh lệch 2 giá trị MACD/RSI    ở đáy 1 và đáy 2 - 0.10 -> đáy 2 >= đáy 1 tăng 10%</param>
        /// <returns></returns>
        public async Task<List<string>> PhanKyDuong(string code, DateTime ngay, DateTime ngayCuoi, int KC2D, int SoPhienKT, string propertyName, decimal CL2G, decimal CL2D)
        {
            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");
            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > 100000).ToListAsync()
                : await _context.StockSymbol.Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > 100000 && splitStringCode.Contains(s._sc_)).ToListAsync();
            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var historiesStockCode = await _context.History
                .Where(ss => stockCodes.Contains(ss.StockSymbol)
                    && ss.Date <= ngay.AddDays(10)
                    && ss.Date >= ngayCuoi.AddDays(-(SoPhienKT * 3)))
                .OrderByDescending(ss => ss.Date)
                .ToListAsync();

            var result1 = new List<string>();

            Parallel.ForEach(symbols, symbol =>
            {
                decimal tong = 0;
                decimal dung = 0;
                decimal sai = 0;

                var histories = historiesStockCode
                                    .Where(ss => ss.StockSymbol == symbol._sc_)
                                    .OrderBy(h => h.Date)
                                    .ToList();
                var ngayBatDau = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date >= ngayCuoi);
                for (int i = histories.IndexOf(ngayBatDau); i < histories.Count; i++)
                {
                    ngayBatDau = histories[i];
                    if (ngayBatDau != null && ngayBatDau.HadAllIndicators())
                    {
                        break;
                    }
                }

                var ngayDungLai = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date >= ngay);
                if (ngayDungLai == null) //đang nhập ngày trong tương lai -> chuyển về hiện tại
                {
                    ngayDungLai = histories.OrderByDescending(h => h.Date).First();
                }
                var startedI = histories.IndexOf(ngayBatDau);
                var stoppedI = histories.IndexOf(ngayDungLai);

                for (int i = startedI; i < stoppedI; i++)
                {
                    var buyingDate = histories[i];
                    //Giả định ngày trước đó là đáy
                    var dayGiaDinh = histories[i - 1];
                    //hum nay RSI tăng so với hum wa, thì hum wa mới là đáy, còn ko thì mai RSI vẫn có thể giảm tiếp, ko ai bik

                    var propertyValueOfDayGiaDinh = (decimal)dayGiaDinh.GetPropValue(propertyName);
                    var propertyValueOfSuggestedDate = (decimal)buyingDate.GetPropValue(propertyName);
                    if (propertyValueOfDayGiaDinh == 0
                        || propertyValueOfSuggestedDate <= propertyValueOfDayGiaDinh
                        || buyingDate.NenBot < dayGiaDinh.NenBot)
                    {
                        continue;
                    }

                    //Kiem tra đáy giả định: trong vòng 14 phiên trước không có cây nào trước đó thấp hơn nó
                    var nhungNgaySoSanhVoiDayGiaDinh = histories.OrderByDescending(h => h.Date).Where(h => h.Date < dayGiaDinh.Date).Take(SoPhienKT).ToList();
                    if (nhungNgaySoSanhVoiDayGiaDinh.Count < SoPhienKT) continue;

                    //var ngayCoRSIThapNhatDeSoSanhVoiDayGiaDinh = nhungNgaySoSanhVoiDayGiaDinh.OrderBy(h => h.RSI).First();
                    //var propertyInfo = typeof(History).GetProperty(propertyName);
                    //var ngayCoRSIThapNhatDeSoSanhVoiDayGiaDinh = nhungNgaySoSanhVoiDayGiaDinh.OrderBy(x => propertyInfo.GetValue(x, null)).First();

                    //var indexOfDayGiaDinh = histories.IndexOf(dayGiaDinh);
                    //var indexOfngayCoRSIThapNhatDeSoSanhVoiDayGiaDinh = histories.IndexOf(ngayCoRSIThapNhatDeSoSanhVoiDayGiaDinh);
                    //if (indexOfDayGiaDinh - indexOfngayCoRSIThapNhatDeSoSanhVoiDayGiaDinh <= KC2D) continue; //đáy RSI thấp nhất không dc nằm trong 3 ngày tính từ ngày của đáy giả định trở về trước

                    for (int j = KC2D; j < nhungNgaySoSanhVoiDayGiaDinh.Count - 1; j++)
                    {
                        var ngàyĐếmNgược = nhungNgaySoSanhVoiDayGiaDinh[j];
                        //var ngàyTrướcĐáy1 = nhungNgaySoSanhVoiDayGiaDinh[j + 1];    //+1 tại đang đếm ngược
                        //var ngàySauĐáy1 = nhungNgaySoSanhVoiDayGiaDinh[j - 1];      //-1 tại đang đếm ngược

                        //var propertyValueOfngàySauĐáy1 = (decimal)ngàySauĐáy1.GetPropValue(propertyName);
                        //var propertyValueOfngàyTrướcĐáy1 = (decimal)ngàyTrướcĐáy1.GetPropValue(propertyName);
                        var propertyValueOfngàyĐếmNgược = (decimal)ngàyĐếmNgược.GetPropValue(propertyName);

                        //if (propertyValueOfngàyTrướcĐáy1 <= propertyValueOfngàyĐếmNgược) continue;

                        //if (propertyValueOfngàySauĐáy1 <= propertyValueOfngàyĐếmNgược) continue;

                        var hasPhanKyDuong = histories.HasPhanKyDuong(dayGiaDinh, ngàyĐếmNgược, propertyName, SoPhienKT, CL2G, CL2D);

                        if (hasPhanKyDuong != null && hasPhanKyDuong.HasValue && hasPhanKyDuong.Value) //ngày đếm cũng là ngày có đáy
                        {
                            var tileChinhXac = 0;
                            var tPlus = histories.Where(h => h.Date >= buyingDate.Date)
                                .OrderBy(h => h.Date)
                                .Skip(3)
                                .Take(3)
                                .ToList();

                            if (tPlus.Any(t => t.C > buyingDate.O * 1.01M))    //Mình đặt mua ở giá mở cửa ngày hum sau luôn
                            {
                                dung++;
                                result1.Add($"{symbol._sc_} - Đúng - Điểm xét: {buyingDate.Date.ToShortDateString()} - Đáy {propertyName}: {dayGiaDinh.Date.ToShortDateString()} {propertyName} {propertyValueOfDayGiaDinh} - Giá {dayGiaDinh.NenBot} - Điểm tín hiệu: {ngàyĐếmNgược.Date.ToShortDateString()} {propertyName} {propertyValueOfngàyĐếmNgược} - Giá {ngàyĐếmNgược.NenBot}");
                            }
                            else
                            {
                                sai++;
                                result1.Add($"{symbol._sc_} - Sai  - Điểm xét: {buyingDate.Date.ToShortDateString()} - Đáy {propertyName}: {dayGiaDinh.Date.ToShortDateString()} {propertyName} {propertyValueOfDayGiaDinh} - Giá {dayGiaDinh.NenBot} - Điểm tín hiệu: {ngàyĐếmNgược.Date.ToShortDateString()} {propertyName} {propertyValueOfngàyĐếmNgược} - Giá {ngàyĐếmNgược.NenBot}");
                            }

                        }
                    }
                }
            });

            //tong = dung + sai;
            //var tile = Math.Round(dung / tong, 2);
            //result1.Add($"Tỉ lệ: {tile}");
            return result1;
        }


        public async Task<List<Tuple<string, decimal, List<string>>>> RSITestRSI(string code, DateTime ngay, DateTime ngayCuoi, int KC2D, int SoPhienKT, int ma20vol)
        {
            var result = new PatternDetailsResponseModel();
            int rsi = 14;
            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");
            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.Where(s => s.BiChanGiaoDich == false && s.MA20Vol > ma20vol).ToListAsync()
                : await _context.StockSymbol.Where(s => s.BiChanGiaoDich == false && s.MA20Vol > ma20vol && splitStringCode.Contains(s._sc_)).ToListAsync();
            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var historiesStockCode = await _context.History
                .Where(ss => stockCodes.Contains(ss.StockSymbol)
                    && ss.Date <= ngay.AddDays(10) //calculate T
                    && ss.Date >= ngayCuoi.AddDays(-30)) //caculate SRI
                .OrderByDescending(ss => ss.Date)
                .ToListAsync();

            var tup = new List<Tuple<string, decimal, List<string>>>();

            Parallel.ForEach(symbols, symbol =>
            {
                var result1 = new List<string>();
                decimal tong = 0;
                decimal dung = 0;
                decimal sai = 0;

                var historiesInPeriodOfTime = historiesStockCode
                    .Where(ss => ss.StockSymbol == symbol._sc_)
                    .OrderBy(h => h.Date)
                    .ToList();

                var histories = historiesInPeriodOfTime
                    .Where(ss => ss.Date <= ngay && ss.Date >= ngayCuoi)
                    .OrderByDescending(h => h.Date)
                    .ToList();

                var patternOnsymbol = new PatternBySymbolResponseModel();
                patternOnsymbol.StockCode = symbol._sc_;

                var history = histories.FirstOrDefault(h => h.Date == ngay);
                if (history == null) return;

                var yesterday = histories.FirstOrDefault(h => h.Date < ngay);
                if (yesterday == null) return;

                for (int i = 0; i < histories.Count - 1; i++)
                {
                    var buyingDate = histories[i];

                    //Giả định ngày trước đó là đáy
                    var dayGiaDinh = histories[i + 1];

                    //trước đây đáy phải là 1 cây giảm:
                    if (i + 1 + 1 <= histories.Count - 1)
                    {
                        var cayTruocCayDayGiaDinh = histories[i + 1 + 1];
                        if (cayTruocCayDayGiaDinh.TangGia()) continue;
                    }

                    //Kiem tra đáy giả định: trong vòng 14 phiên trước không có cây nào trước đó thấp hơn nó
                    var rsi14Period = histories.Where(h => h.Date < dayGiaDinh.Date).Take(SoPhienKT).ToList();
                    if (rsi14Period.Count < SoPhienKT) continue;

                    var nhungNgaySoSanhVoiDayGiaDinh = rsi14Period.OrderByDescending(h => h.Date).Skip(KC2D).ToList();

                    var ngaySoSanhVoiDayGiaDinh = nhungNgaySoSanhVoiDayGiaDinh.Where(h => h.NenBot == nhungNgaySoSanhVoiDayGiaDinh.Min(f => f.NenBot)).OrderBy(h => h.RSI).First();

                    var isDayGiaDinhDung = ngaySoSanhVoiDayGiaDinh.NenBot > dayGiaDinh.NenBot;
                    if (isDayGiaDinhDung == false) continue;

                    var isRSIIncreasing = ngaySoSanhVoiDayGiaDinh.RSI < dayGiaDinh.RSI;

                    //TODO: giữa 2 điểm so sánh, ko có 1 điểm nào xen ngang vô cả
                    var middlePoints = histories.Where(h => h.Date > ngaySoSanhVoiDayGiaDinh.Date && h.Date < dayGiaDinh.Date).ToList();
                    if (middlePoints.Count < 2) continue; //ở giữa ít nhất 2 điểm

                    var trendLineRSI = new Line();
                    trendLineRSI.x1 = 0;  //x là trục tung - trục đối xứng
                    trendLineRSI.y1 = ngaySoSanhVoiDayGiaDinh.RSI;   //
                    trendLineRSI.x2 = (decimal)((dayGiaDinh.Date - ngaySoSanhVoiDayGiaDinh.Date).TotalDays);
                    trendLineRSI.y2 = dayGiaDinh.RSI;
                    var crossLine = new Line();
                    crossLine.x1 = (decimal)((middlePoints.OrderByDescending(h => h.RSI).First().Date - ngaySoSanhVoiDayGiaDinh.Date).TotalDays);
                    crossLine.y1 = middlePoints.OrderByDescending(h => h.RSI).First().RSI;
                    crossLine.x2 = (decimal)((middlePoints.OrderByDescending(h => h.RSI).Last().Date - ngaySoSanhVoiDayGiaDinh.Date).TotalDays);
                    crossLine.y2 = middlePoints.OrderByDescending(h => h.RSI).Last().RSI;

                    //var trendLine = new Line();
                    //trendLine.x1 = 0;  //x là trục tung - trục đối xứng
                    //trendLine.y1 = ngaySoSanhVoiDayGiaDinh.NenBot;   //
                    //trendLine.x2 = (decimal)((dayGiaDinh.Date - ngaySoSanhVoiDayGiaDinh.Date).TotalDays);
                    //trendLine.y2 = dayGiaDinh.NenBot;

                    //var crossLine = new Line();
                    //var point1 = middlePoints.OrderByDescending(h => h.NenBot).First();
                    //var point2 = middlePoints.OrderByDescending(h => h.NenBot).Last();
                    //crossLine.x1 = (decimal)((point1.Date - ngaySoSanhVoiDayGiaDinh.Date).TotalDays);
                    //crossLine.y1 = point1.NenBot;
                    //crossLine.x2 = (decimal)((point2.Date - ngaySoSanhVoiDayGiaDinh.Date).TotalDays);
                    //crossLine.y2 = point2.NenBot;

                    var point = trendLineRSI.FindIntersection(crossLine);

                    if (isRSIIncreasing && point == null)
                    {
                        var tileChinhXac = 0;
                        var tPlus = historiesInPeriodOfTime.Where(h => h.Date >= buyingDate.Date)
                            .OrderBy(h => h.Date)
                            .Skip(3)
                            .Take(3)
                            .ToList();

                        if (tPlus.Any(t => t.C > buyingDate.O * 1.01M))    //Mình đặt mua ở giá mở cửa ngày hum sau luôn
                        {
                            dung++;
                            result1.Add($"{symbol._sc_} - Đúng T3-5 - Điểm nhắc để ngày mai mua: {dayGiaDinh.Date.ToShortDateString()} RSI {dayGiaDinh.RSI.ToString("N2")} - Giá {dayGiaDinh.NenBot.ToString("N2")} - Điểm so sánh: {ngaySoSanhVoiDayGiaDinh.Date.ToShortDateString()} RSI {ngaySoSanhVoiDayGiaDinh.RSI.ToString("N2")} - Giá {ngaySoSanhVoiDayGiaDinh.NenBot.ToString("N2")}");
                        }
                        else
                        {
                            sai++;
                            result1.Add($"{symbol._sc_} - Sai  T3-5 - Điểm nhắc để ngày mai mua: {dayGiaDinh.Date.ToShortDateString()} RSI {dayGiaDinh.RSI.ToString("N2")} - Giá {dayGiaDinh.NenBot.ToString("N2")} - Điểm so sánh: {ngaySoSanhVoiDayGiaDinh.Date.ToShortDateString()} RSI {ngaySoSanhVoiDayGiaDinh.RSI.ToString("N2")} - Giá {ngaySoSanhVoiDayGiaDinh.NenBot.ToString("N2")}");
                        }
                    }
                }

                tong = dung + sai;
                var tile = tong == 0 ? 0 : Math.Round(dung / tong, 2);
                //result1.Add($"Tỉ lệ: {tile}");
                tup.Add(new Tuple<string, decimal, List<string>>(symbol._sc_, tile, result1));
            });

            return tup;
        }

        public async Task<List<Tuple<string, decimal, List<string>>>> RSITestMANhanhCatCham(string code, DateTime ngay, DateTime ngayCuoi, int ma20vol, int MANhanh, int MACham, decimal percentProfit)
        {
            var result = new PatternDetailsResponseModel();
            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");
            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.Where(s => s.MA20Vol > ma20vol).ToListAsync()
                : await _context.StockSymbol.Where(s => s.MA20Vol > ma20vol && splitStringCode.Contains(s._sc_)).ToListAsync();
            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var historiesStockCode = await _context.History
                .Where(ss => stockCodes.Contains(ss.StockSymbol)
                    && ss.Date <= ngay.AddDays(10) //calculate T
                    && ss.Date >= ngayCuoi.AddDays(-MACham * 2)) //caculate SRI
                .OrderByDescending(ss => ss.Date)
                .ToListAsync();

            var tup = new List<Tuple<string, decimal, List<string>>>();

            Parallel.ForEach(symbols, symbol =>
            {
                var result1 = new List<string>();
                decimal tong = 0;
                decimal dung = 0;
                decimal sai = 0;

                var NhậtKýMuaBán = new List<Tuple<string, DateTime, bool, decimal>>();

                var historiesInPeriodOfTime = historiesStockCode
                    .Where(ss => ss.StockSymbol == symbol._sc_)
                    .OrderBy(h => h.Date)
                    .ToList();
                if (historiesInPeriodOfTime.Count < 100) return;


                var ngayCuoiCuaMa = historiesInPeriodOfTime[0].Date.AddDays(30) > ngayCuoi
                    ? historiesInPeriodOfTime[0].Date.AddDays(30)
                    : ngayCuoi;

                var histories = historiesInPeriodOfTime
                    .Where(ss => ss.Date <= ngay && ss.Date >= ngayCuoiCuaMa)
                    .OrderBy(h => h.Date)
                    .ToList();

                var patternOnsymbol = new PatternBySymbolResponseModel();
                patternOnsymbol.StockCode = symbol._sc_;

                var history = histories.FirstOrDefault(h => h.Date == ngay);
                if (history == null) return;

                for (int i = 0; i < histories.Count; i++)
                {
                    var phienHumNay = histories[i];
                    var phienHumWa = historiesInPeriodOfTime.Where(h => h.Date < phienHumNay.Date).OrderByDescending(h => h.Date).First();
                    var phienHumKia = historiesInPeriodOfTime.Where(h => h.Date < phienHumWa.Date).OrderByDescending(h => h.Date).First();

                    var phienHumNayMa20 = phienHumNay.MA(historiesInPeriodOfTime, -MACham);
                    var phienHumNayMa05 = phienHumNay.MA(historiesInPeriodOfTime, -MANhanh);
                    var phienHumWaMa05 = phienHumWa.MA(historiesInPeriodOfTime, -MANhanh);
                    var phienHumWaMa20 = phienHumWa.MA(historiesInPeriodOfTime, -MACham);


                    //tín hiệu mua
                    var Ma05DuoiMa20 = phienHumNayMa05 < phienHumNayMa20;
                    var MA05HuongLen = phienHumWaMa05 < phienHumNayMa05;
                    var nenTangGia = phienHumNay.TangGia();
                    var nenTangLenChamMa20 = phienHumNay.NenTop >= phienHumNayMa20 && phienHumNay.NenBot < phienHumNayMa20;             //Giá trong phiên MA 05 tăng lên chạm MA 20
                    var râunếnTangLenChamMa20 = phienHumNay.H >= phienHumNayMa20 && phienHumNay.NenBot < phienHumNayMa20;               //Giá trong phiên MA 05 tăng lên chạm MA 20
                    var duongMa05CatLenTrenMa20 = phienHumWaMa05 < phienHumNayMa20 && phienHumNayMa05 > phienHumNayMa20;     //MA 05 cắt lên trên MA 20
                    var nenNamDuoiMA20 = phienHumNay.NenBot < phienHumNayMa20;                                                            //Giá nằm dưới MA 20
                    var thânNếnKhôngVượtQuáBandTren = phienHumNay.NenTop < phienHumNay.BandsTop;


                    /*TODO: cảnh báo mua nếu giá mở cửa tăng chạm bands trên. Ví dụ 9/4/2021 KBC, 17/1/2022 VNM - tăng gần chạm bands mà ko thấy dấu hiệu mở bands rộng ra, mA 20 cũng ko hướng lên - sideway
                     * 10-2-22 VNM: bands ko mở rộng, bands tren hướng xuống, bands dưới đi ngang, các giá trước loanh quanh MA 20, ko có dấu hiệu phá bỏ sideway
                     * var khángCựĐỉnh = phienKiemTra.KhángCựĐỉnh(historiesInPeriodOfTime);
                     * var khángCựBands = phienKiemTra.KhángCựBands(historiesInPeriodOfTime);
                     * var khángCựFibonacci = phienKiemTra.KhángCựFibonacci(historiesInPeriodOfTime);
                     * var khángCựIchimoku   = phienKiemTra.KhángCựIchimoku(historiesInPeriodOfTime);
                     * 
                     * Ví dụ: 
                     *  KBC - 22/3/22 (MA + bands) -> nhưng có thể xét vì giá đã tăng gần tới viền mây dưới ichimoku rồi nên ko mua, hoặc giá từ MA 5 đi lên (hổ trợ lên kháng cự) ở MA 20, và bands bóp nên giá chỉ quay về MA20
                     *  KBC - 03/3/20 (MA + bands) -> MA, Bands đi ngang, có thể mua để lợi T+ => thất bại -> có thể xét tới MACD trong trường hợp này:
                     *                              + MACD dưới 0, đỏ cắt lên xanh
                     *                              + Đã mua ở ngày 26/2 rồi, MACD chưa vượt 0 lên dương mạnh thì cũng ko cần mua thêm
                     *                              + => bands hẹp, bands ko thay đổi, ma 20 k thay đổi, thân nến ở giữa bands => ko mua, vì rất dễ sảy T3
                     *                                      
                     *  KBC - 10/3/20 -> 17/3/20 - Nếu bất chấp nến ngoài bolinger bands dưới để mua, thì hãy cân nhắc KBC trong những ngày này -> nên kết hợp MACD (macd giảm, momentum âm mạnh) 
                     *                              + => CThuc A
                     *                                                     
                     *  KBC - 27/03/20 tới 31/03/2020 -> Nến rút chân xanh 3 cây liên tục, bands dưới vòng cung lên, band trên đi xuống => bands bóp => biên độ cực rộng => giá sẽ qua về MA 20
                     *                                + 3 cây nến xanh bám ở MA 5 liên tục, rút chân lên => mua vô ở cây sau được, giá mua vô từ trung bình râu nến dưới của 2 cây trước (do tín hiệu tăng) lên tới MA 5, ko cần mua đuổi
                     *                                + Theo dõi sau đó, vì nếu band tăng, MA 20 tăng, thì MA 20 sẽ là hỗ trợ cho nhịp hồi này, khi nào bán?                                          
                     *                                      + RSI rớt way về quanh 80 thì xả từ từ
                     *                                      + Giá dưới MA 5 2 phiên thì xả thêm 1 đoạn
                     *                                      + MA 5 cắt xuống MA 20 thì xả hết
                     *                              + => CThuc A
                     *                                  
                     *  KBC - 7/07/20 tới 22/07/2020 -> từ 26/6/20 tới 6/7/20 -> giao dịch quanh kháng cự là MA 5, và hổ trợ là bands dưới
                     *                                  + ngày 7/7/20 -> giá vượt kháng cự (MA 05), MACD xanh bẻ ngang lúc này kháng cự mới sẽ là MA 20, MA 5 sẽ là hỗ trợ
                     *                                  + có thể ra nhanh đoạn này khi T3 về (13/7/20) vì giá vượt kháng cự, nhưng lại lạ nến đỏ => ko qua dc, dễ dội về hỗ trợ => ko cần giữ lâu
                     *                              + => CThuc A
                     *  KBC - 10/8/20 (MA + bands) -> Nếu mua ngày 4/8/20 (trước đó 4 ngày) vì phân kì tăng RSI, thì mình có thể tránh trường hợp này
                     *                                + 31/7/20: nến doji tăng ngoài bands dưới
                     *                                + 03/8/20: nến tăng xác nhận doji trước là đáy -> cuối ngày ngồi coi - RSI tăng, Giá giảm -> tín hiệu đảo chiều -> nên mua vô
                     *                                + 04/8/20: mua vô ở giá đóng cửa của phiên trước (03/8/20)
                     *                                + Giá tăng liên tục trong những phiên sau, nhưng vol < ma 20, thân nến nhỏ => giao dịch ít, lưỡng lự, ko nên tham gia lâu dài trong tình huống này
                     *                                + Nếu lỡ nhịp mua ngày 04/8/20 rồi thì thôi
                     *                              + => CThuc A
                     *  KBC - 12/11/20 tới 17/11/2020 -> Nếu đã mua ngày 11/11/2020, thì nên theo dõi thêm MACD để tránh bán hàng mà lỡ nhịp tăng
                     *                                + MACD cắt ngày 11/11/20, tạo tín hiệu đảo chiều, kết hợp với những yếu tố đi kèm,
                     *                                + MACD tăng dần lên 0, momentum tăng dần theo, chờ vượt 0 là nổ
                     *                              + => CThuc B
                     *  KBC - 21/5/21 -> Lưu ý đặt sẵn giá mua ở giá sàn những ngày này nếu 2 nến trước đã tạo râu bám vô MA 05, nếu ko có râu bám vô MA 5 thì thôi
                     *                                + nếu có râu nên bám vô, thì đặt sẵn giá mua = giá từ giá thấp nhất của cây nến thứ 2 có râu
                     *                                  
                     *                                  
                     *  KBC - 12/7/21 - 26/7/21  -> giống đợt 31/7/20 tới 4/8/20
                     *                              + Ngày 12/7 1 nến con quay dài xuất hiện dưới bands bot => xác nhận đáy, cùng đáy với ngày 21/5/21 => hỗ trợ mạnh vùng thân nến đỏ trải xuống râu nến này, có thể vô tiền 1 ít tầm giá này
                     *                              + Sau đó giá bật lại MA 5
                     *                              + tới ngày 19/7/20 - 1 cây giảm mạnh, nhưng giá cũng chỉ loanh quanh vùng hỗ trợ này, vol trong nhịp này giảm => hết sức đạp, cũng chả ai muốn bán
                     *                              + RSI trong ngày 19/7, giá đóng cửa xuống giá thấp hơn ngày 12/7, nhưng RSI đang cao hơn => tín hiệu đảo chiều 
                     *                              + - nhưng cần theo dõi thêm 1 phiên, nếu phiên ngày mai xanh thì ok, xác nhận phân kỳ tăng => Có thể mua vô dc
                     *                              
                     *  Bands và giá rớt (A - Done)
                     *      + Nếu giá rớt liên tục giữa bands và MA 5, nếu xuất hiện 1 cây nến có thân rớt ra khỏi bands dưới, có râu trên dài > 2 lần thân nến, thì bắt đầu để ý vô lệnh mua
                     *      + (A1) Nếu nến rớt ngoài bands này là nến xanh => đặt mua ở giá quanh thân nến
                     *      + (A2) Nếu nến rớt ngoài bands này là nến đỏ   => tiếp tục chờ 1 cây nến sau cây đỏ này, nếu vẫn là nến đỏ thì bỏ, nếu là nến xanh thì đặt mua cây tiếp theo
                     *          + đặt mua ở giá trung bình giữa giá mở cửa của cây nến đỏ ngoài bands và giá MA 5 ngày hum nay
                     *          
                     *      + Ví dụ: KBC: 10/3/20 -> 17/3/20                                                    03/8/20                 3/11/20                                     12/7/21 - 26/7/21                   
                     *                  - RSI dương   (cây nến hiện tại hoặc 1 trong 3 cây trước là dc)         RSI dương               RSI dương                                   Cây nến 13/7/21 ko tăng
                     *                  - MACD momentum tăng->0                                                 Tăng                    Tăng                                        Tăng rất nhẹ (~2%)
                     *                  - MACD tăng                                                             Tăng                    Giảm nhẹ hơn trước (-5 -> -41 -> -50)       Giảm
                     *                  - nến tăng                                                              Tăng                    Tăng                                        Tăng
                     *                  - giá bật từ bands về MA 5                                              OK                      OK
                     *                  - 13 nến trước (100% bám bands dưới, thân nến dưới MA 5                 7 (100%) dưới MA 5      4/5 cây giảm (80%) ko chạm MA 5             4/6 nến giảm liên tục ko chạm MA 5
                     *                  - MA 5 bẻ ngang -> giảm nhẹ hơn 2 phiên trước:                          MA 5 tăng               14330 (-190) -> 14140 (-170)
                     *                      + T(-1) - T(0) < T(-3) - T(-2) && T(-2) - T(-1)                                             -> 13970 (-60) -> 13910
                     *                  - Khoảng cách từ MA 5 tới MA 20 >= 15%                                  > 15%                   4% (bỏ)                                     12%
                     *                      + vì giá sẽ về MA 20, nên canh tí còn ăn
                     *                      + mục tiêu là 10% trong những đợt hồi này, nên mua quanh +-3%       +-3%
                     *                      + Khoảng cách càng lớn thì đặt giá mua càng cao, tối đa 3%
                     *                      + Cân nhắc đặt ATO cho dễ tính => đặt giá C như bth
                     *      
                    */
                    var bandsTrenHumNay = phienHumNay.BandsTop;
                    var bandsDuoiHumNay = phienHumNay.BandsBot;
                    var bandsTrenHumWa = phienHumWa.BandsTop;
                    var bandsDuoiHumWa = phienHumWa.BandsBot;

                    var bắtĐầuMuaDoNếnTăngA1 = phienHumNay.NếnĐảoChiềuTăngMạnhA1();
                    var bắtĐầuMuaDoNếnTăngA2 = phienHumNay.NếnĐảoChiềuTăngMạnhA2(phienHumWa);

                    var bandsTrênĐangGiảm = bandsTrenHumNay < bandsTrenHumWa;
                    var bandsMởRộng = bandsTrenHumNay > bandsTrenHumWa && bandsDuoiHumNay > bandsDuoiHumWa;
                    var bandsĐangBópLại = bandsTrenHumNay < bandsTrenHumWa && bandsDuoiHumNay > bandsDuoiHumWa;
                    var ma20ĐangGiảm = phienHumNayMa20 < phienHumWaMa20;


                    var bandsKhôngĐổi = bandsTrenHumNay == bandsTrenHumWa && bandsDuoiHumNay == bandsDuoiHumWa;
                    var ma20KhôngĐổi = phienHumNayMa20 == phienHumWaMa20;
                    var giaOGiuaBands = phienHumNay.NenBot * 0.93M < phienHumNay.BandsBot && phienHumNay.NenTop * 1.07M > phienHumNay.BandsTop;

                    var muaTheoMA = thânNếnKhôngVượtQuáBandTren && nenTangGia && ((duongMa05CatLenTrenMa20 && nenNamDuoiMA20)
                                                    || (MA05HuongLen && (nenTangLenChamMa20 || râunếnTangLenChamMa20) && Ma05DuoiMa20));
                    var nếnTụtMạnhNgoàiBandDưới = phienHumNay.BandsBot > phienHumNay.NenBot + ((phienHumNay.NenTop - phienHumNay.NenBot) / 2);

                    var momenTumTốt = (phienHumKia.MACDMomentum.IsDifferenceInRank(phienHumWa.MACDMomentum, 0.01M) || phienHumWa.MACDMomentum > phienHumKia.MACDMomentum) && phienHumNay.MACDMomentum > phienHumWa.MACDMomentum * 0.96M;
                    var momenTumTăngTốt = phienHumWa.MACDMomentum > phienHumKia.MACDMomentum * 0.96M && phienHumNay.MACDMomentum > phienHumWa.MACDMomentum * 0.96M;

                    var nếnBậtMạnhLênTừBandsDướiVềMA05HoặcTrongBands =
                        (phienHumWa.NenTop < phienHumWa.BandsBot
                            || (phienHumWa.NenBot.IsDifferenceInRank(phienHumWa.BandsBot, 0.02M) && phienHumWa.NenTop < phienHumWaMa05))
                        && (phienHumNay.NenTop >= phienHumNayMa05 || phienHumNay.NenBot >= phienHumNay.BandsBot);

                    var trongXuHướngGiảmMạnh = phienHumNay.TỉLệNếnCựcYếu(histories) >= 0.5M;
                    var trongXuHướngGiảm = phienHumNay.TỉLệNếnYếu(histories) >= 0.5M;

                    var ma05ĐangBẻNgang = phienHumNay.MAChuyểnDần(histories, false, -5, 3);
                    var khôngNênBánT3 = phienHumNay.MACDMomentum > phienHumWa.MACDMomentum && phienHumNay.MACD > phienHumWa.MACD && phienHumNay.MACDMomentum > -100;

                    var rsiDuong = phienHumNay.RSIDương(histories);
                    var tínHiệuMuaTrongSóngHồiMạnh =
                                                   momenTumTăngTốt
                                                && phienHumNay.TangGia()
                                                && (nếnBậtMạnhLênTừBandsDướiVềMA05HoặcTrongBands || ma05ĐangBẻNgang)
                                                && (trongXuHướngGiảmMạnh)
                                                && phienHumNayMa20 / phienHumNayMa05 > 1.1M;

                    var tínHiệuMuaTrongSóngHồiTrungBình =
                                                   momenTumTốt
                                                && (phienHumNay.TangGia() || phienHumNay.Doji())
                                                && (nếnBậtMạnhLênTừBandsDướiVềMA05HoặcTrongBands || ma05ĐangBẻNgang)
                                                && (trongXuHướngGiảmMạnh || trongXuHướngGiảm)
                                                && phienHumNayMa20 / phienHumNayMa05 > 1.1M;

                    var tinhieuMuaManh = tínHiệuMuaTrongSóngHồiMạnh ? 10 : 0;
                    var tinhieuMuaTrungBinh = tínHiệuMuaTrongSóngHồiTrungBình ? 5 : 0;
                    var tinHieuMuaTrungBinh1 = muaTheoMA ? 5 : 0;
                    var tinHieuMuaTrungBinh2 = nếnTụtMạnhNgoàiBandDưới && ma05ĐangBẻNgang ? 5 : 0;
                    var tinHieuMuaYếu1 = bandsMởRộng ? 5 : 0;

                    var tinHieuGiảmMua1 = bandsTrênĐangGiảm && ma20ĐangGiảm ? -5 : 0;// && !nếnGiảmVượtMạnhNgoàiBandDưới ? -5 : 0;
                    //var tinHieuGiảmMua2 = bandsKhôngĐổi && ma20KhôngĐổi && giaOGiuaBands ? -5 : 0;// && !nếnGiảmVượtMạnhNgoàiBandDưới ? -5 : 0;
                    var tinHieuGiảmMua2 = bandsKhôngĐổi && ma20KhôngĐổi ? -5 : 0;// && !nếnGiảmVượtMạnhNgoàiBandDưới ? -5 : 0;
                    var tinHieuGiảmMua3 = giaOGiuaBands ? -5 : 0;// && !nếnGiảmVượtMạnhNgoàiBandDưới ? -5 : 0;
                    var tinHieuGiảmMua4 = !phienHumNay.SoSánhGiá(1) || !phienHumWa.SoSánhGiá(1) ? -5 : 0;

                    if (tinhieuMuaManh
                        + tinhieuMuaTrungBinh + tinHieuMuaTrungBinh1 + tinHieuMuaTrungBinh2
                        + tinHieuGiảmMua1 + tinHieuGiảmMua2 + tinHieuGiảmMua3 + tinHieuGiảmMua4 <= 0) continue;

                    var ngayMua = historiesInPeriodOfTime.Where(h => h.Date > phienHumNay.Date).OrderBy(h => h.Date).FirstOrDefault();
                    if (ngayMua == null) ngayMua = new History() { Date = phienHumNay.Date.AddDays(1) };
                    var giáĐặtMua = nếnTụtMạnhNgoàiBandDưới
                        ? (phienHumNay.BandsBot + phienHumNay.NenBot) / 2
                        : phienHumNay.C;

                    //if (giáĐặtMua >= ngayMua.L && giáĐặtMua <= ngayMua.H)       //Giá hợp lệ
                    //{
                    var giữT = khôngNênBánT3 ? 6 : 3;
                    var tPlus = historiesInPeriodOfTime.Where(h => h.Date >= ngayMua.Date)
                        .OrderBy(h => h.Date)
                        .Skip(3)
                        .Take(giữT)
                        .ToList();

                    if (tPlus.Count < 3) //hiện tại
                    {
                        result1.Add($"{symbol._sc_} - Hiện tại điểm nhắc mua: {phienHumNay.Date.ToShortDateString()} ở giá {giáĐặtMua.ToString("N2")}");
                    }
                    else
                    {
                        if (tPlus.Any(t => t.C > ngayMua.O * (1M + percentProfit) || t.O > ngayMua.O * (1M + percentProfit)))    //Mình đặt mua ở giá mở cửa ngày hum sau luôn
                        {
                            dung++;
                            result1.Add($"{symbol._sc_} - Đúng T3-5 - Điểm nhắc mua: {phienHumNay.Date.ToShortDateString()} ở giá {giáĐặtMua.ToString("N2")}");
                        }
                        else
                        {
                            sai++;
                            result1.Add($"{symbol._sc_} - Sai  T3-5 - Điểm nhắc mua: {phienHumNay.Date.ToShortDateString()} ở giá {giáĐặtMua.ToString("N2")}");
                        }
                    }

                    //if (bandsĐangGiảm && ma20ĐangGiảm && !nếnGiảmVượtMạnhNgoàiBandDưới)
                    //{
                    //    if (tPlus.Any(t => t.C > ngayMua.O * 1.01M || t.O > ngayMua.O * 1.01M))    //Mình đặt mua ở giá mở cửa ngày hum sau luôn
                    //    {
                    //        sai++;
                    //        result1.Add($"{symbol._sc_} - Sai  - Band xấu - Điểm nhắc để ngày mai mua: {phienKiemTra.Date.ToShortDateString()} ở giá {giáĐặtMua.ToString("N2")}");
                    //    }
                    //    else
                    //    {
                    //        dung++;
                    //        result1.Add($"{symbol._sc_} - Đúng - Band xấu - Điểm nhắc để ngày mai mua: {phienKiemTra.Date.ToShortDateString()} ở giá {giáĐặtMua.ToString("N2")}");
                    //    }
                    //}

                    //if (!bandsĐangGiảm || !ma20ĐangGiảm || nếnGiảmVượtMạnhNgoàiBandDưới)
                    //{
                    //    if (tPlus.Any(t => t.C > ngayMua.O * 1.01M || t.O > ngayMua.O * 1.01M))    //Mình đặt mua ở giá mở cửa ngày hum sau luôn
                    //    {
                    //        dung++;
                    //        result1.Add($"{symbol._sc_} - Đúng T3-5 - Điểm nhắc để ngày mai mua: {phienKiemTra.Date.ToShortDateString()} ở giá {giáĐặtMua.ToString("N2")}");
                    //    }
                    //    else
                    //    {
                    //        sai++;
                    //        result1.Add($"{symbol._sc_} - Sai  T3-5 - Điểm nhắc để ngày mai mua: {phienKiemTra.Date.ToShortDateString()} ở giá {giáĐặtMua.ToString("N2")}");
                    //    }
                    //}
                }
                //else
                //{
                //    result1.Add($"{symbol._sc_} - Không có giá {giáĐặtMua.ToString("N2")} ở ngày mai mua: {phienKiemTra.Date.ToShortDateString()}");
                //}


                ////tín hiệu bán
                //if ((phienTruocPhienKiemTraMa05 > phienKiemTraMa05              //MA 05 đang hướng lên
                //        && phienKiemTra.NenBot <= phienKiemTraMa20)             //Giá MA 05 chạm MA 20
                //    || (phienTruocPhienKiemTraMa05 >= phienKiemTraMa20 && phienKiemTraMa05 <= phienKiemTraMa20))  //MA 05 cắt xuống dưới MA 20
                //{
                //    var ngayBán = historiesInPeriodOfTime.Where(h => h.Date > phienKiemTra.Date).OrderBy(h => h.Date).First();
                //    var tileChinhXac = 0;
                //    var tPlus = historiesInPeriodOfTime.Where(h => h.Date >= ngayBán.Date)
                //        .OrderBy(h => h.Date)
                //        .Skip(3)
                //        .Take(3)
                //        .ToList();

                //    if (tPlus.All(t => t.C > ngayBán.O || t.O > ngayBán.O))    //Mình đặt mua ở giá mở cửa ngày hum sau luôn
                //    {
                //        dung++;
                //        result1.Add($"{symbol._sc_} - Đúng T3-5 - Điểm nhắc để ngày mai bán: {phienKiemTra.Date.ToShortDateString()}");
                //    }
                //    else
                //    {
                //        sai++;
                //        result1.Add($"{symbol._sc_} - Sai  T3-5 - Điểm nhắc để ngày mai bán: {phienKiemTra.Date.ToShortDateString()} - Bán: {ngayBán.Date.ToShortDateString()} giá {ngayBán.O}");
                //    }
                //}
                //}

                tong = dung + sai;
                var tile = tong == 0 ? 0 : Math.Round(dung / tong, 2);
                //result1.Add($"Tỉ lệ: {tile}");
                tup.Add(new Tuple<string, decimal, List<string>>(symbol._sc_, tile, result1));
            });

            tup = tup.OrderByDescending(t => t.Item2).ToList();

            return tup;
        }

        /*
         * Trend giảm
         *              MACD 
         *                              - Tìm 3 đỉnh giảm dần, nối lại tạo trend line giảm
         *                              - Giữa 3 đỉnh sẽ có 2 đáy
         *                              - ở 2 đáy xét động lượng của MACD -> nếu động lượng cao dần thì đó là tín hiệu đảo chiều
         *                              - Giờ chỉ còn chờ đỉnh 4 - sẽ là điểm breakout
         *                              - Kết hợp thêm kháng cự và hỗ trợ 
         *                              
         *                              Có thể kết hợp thêm
         *                               - MACD đường xanh cắt lên đỏ
         *                               - Bolinger bands
         *                               - Ichimoku
         *                               - RSI
         *                               
         *                              Example:
         *                                  Good:
         *                                      AAA: 04/04/2022 - 19/05/2022
         *                              
         *                              Hướng dẫn
         *                                                          https://www.youtube.com/watch?v=fmtBWx9eMHc
         *                                                          confirm pyll back/trendline  (MUST)
         *                                                          confirm macf histogram       (MUST)
         *                                                          confirm support/resistance   (NO)
         *                                                          confirm ending volume        (NO)
         *                                                          confirm entry breakout point (MUST)
         *              
         *              RSI Trendline
         *                              - Tìm đáy trong quá khứ, nối lại tạo trend line A
         *                              - Ở 2 đáy, nối 2 điểm RSI tạo thành trendline B
         *                              - Nếu A đi xuống hoặc ngang, và B đi lên, đây là tín hiệu đảo chiều, cbi tăng
         *                              - Kết hợp thêm kháng cự và hỗ trợ 
         *
         *                              Có thể kết hợp thêm
         *                               - MACD đường xanh cắt lên đỏ
         *                               - Bolinger bands
         *                               - Ichimoku
         *
         *                              BAD examples:
         *                                  - AAA: 20/8/2019 -> 02/10/2019
         *                                      - Sửa: Nếu kết hợp Breakout thì ăn (Breakout: 23/10/2019, vậy là mua ở cây 24/10/2019 - điểm nối từ 14/08/2019, cắt qua 16/9/2019, nó sẽ kéo tới 22/10/2019, sau cây này là cây breakout)
         *                                              - Sau cây breakout, giá chạm MA 20 là bật lên, MA 20 trở thành hỗ trợ - chờ ra hàng
         *                                              - Kết hợp bolinger bands để tìm điểm bán phù hợp trong sideway (top của bolinger bands)
         *                                              - Ngày 14/11/2019 - MA 20 bị phá vỡ, MACD xanh cắt xuống MACD đỏ, momentum MACD âm, RSI dốc xướng dưới 40 => tín hiệu bán mạnh
         *                                         
         *                                         
         * 
         
         */

        /*
         * Nhìn vô TA
         *  - Xu hướng (mục đích làm gì?)
         *      + Xu hướng tăng: MACD 26 từ âm vượt wa dương
         *      + Xu hướng giảm: MACD 26 từ âm vượt wa dương
         *      + Xu hướng sideway:
         *          
         *      + Chart Tuần: 
         *          + 
         *      + Chart Ngày:
         *  - Vẽ trend line:
         *      + 
         *  - RSI
         *  - MACD
         *  - Bolinger Bands
         *      + upway  : bands trên và dưới ko chênh lệnh quá so với trung bình của bands trên/dưới tính từ 3 phiên trước > 3%
         *      + sideway: bands trên và dưới ko chênh lệnh quá so với trung bình của bands trên/dưới tính từ 3 phiên trước > -3% < 3%
         *      + down   : bands trên và dưới ko chênh lệnh quá so với trung bình của bands trên/dưới tính từ 3 phiên trước < 3%
         *  - Ichimoku
         *  - MA 20
         *  - MA 50
         *  - MA 200
         *  
         *  - Mẫu sideway
         *      + Bán đỉnh bolinger bands
         *      + Mua đáy bolinger bands
         *      + Nếu sideway trên MA 20 - thì MA 20 sẽ là hỗ trợ nhẹ, bot bands là hỗ trợ mạnh, top bands là kháng cự -> mua ở hỗ trợ nhẹ, bán ở gần kháng cự * 0.8
         *      + Nếu sideway dưới hoặc ngang MA 20 - không làm gì cả
         *              + Định nghĩa sideway 
         *                  + trên  MA 20: > 80% các nến trong lần kiểm tra đều có giá dưới cùng của nến (O OR C) cao  hơn MA 20 3-5%
         *                  + ngang MA 20: > 80% các nến trong lần kiểm tra đều có thân nến đề lên MA 20
         *                  + dưới  MA 20: > 80% các nến trong lần kiểm tra đều có giá trên cùng của nến (O OR C) thấp hơn MA 20 3-5% 
         
         */

        public async Task<List<Tuple<string, decimal, List<string>>>> DoTimCongThuc(string code, DateTime ngay, DateTime ngayCuoi, int ma20vol, int MANhanh, int MACham, decimal percentProfit)
        {
            var result = new PatternDetailsResponseModel();
            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");
            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.Where(s => s.MA20Vol > ma20vol).ToListAsync()
                : await _context.StockSymbol.Where(s => s.MA20Vol > ma20vol && splitStringCode.Contains(s._sc_)).ToListAsync();
            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var historiesStockCode = await _context.History
                .Where(ss => stockCodes.Contains(ss.StockSymbol)
                    && ss.Date <= ngay.AddDays(10) //calculate T
                    && ss.Date >= ngayCuoi.AddDays(-MACham * 2)) //caculate SRI
                .OrderByDescending(ss => ss.Date)
                .ToListAsync();

            var tup = new List<Tuple<string, decimal, List<string>>>();

            Parallel.ForEach(symbols, symbol =>
            {
                var result1 = new List<string>();
                decimal tong = 0;
                decimal dung = 0;
                decimal sai = 0;

                var NhậtKýMuaBán = new List<Tuple<string, DateTime, bool, decimal>>();

                var historiesInPeriodOfTime = historiesStockCode
                    .Where(ss => ss.StockSymbol == symbol._sc_)
                    .OrderBy(h => h.Date)
                    .ToList();
                if (historiesInPeriodOfTime.Count < 100) return;


                var ngayCuoiCuaMa = historiesInPeriodOfTime[0].Date.AddDays(30) > ngayCuoi
                    ? historiesInPeriodOfTime[0].Date.AddDays(30)
                    : ngayCuoi;

                var histories = historiesInPeriodOfTime
                    .Where(ss => ss.Date <= ngay && ss.Date >= ngayCuoiCuaMa)
                    .OrderBy(h => h.Date)
                    .ToList();

                var patternOnsymbol = new PatternBySymbolResponseModel();
                patternOnsymbol.StockCode = symbol._sc_;

                var history = histories.FirstOrDefault(h => h.Date == ngay);
                if (history == null) return;

                for (int i = 0; i < histories.Count; i++)
                {
                    var phienHumNay = histories[i];
                    var phienHumWa = historiesInPeriodOfTime.Where(h => h.Date < phienHumNay.Date).OrderByDescending(h => h.Date).First();
                    var phienHumKia = historiesInPeriodOfTime.Where(h => h.Date < phienHumWa.Date).OrderByDescending(h => h.Date).First();

                    var phienHumNayMa20 = phienHumNay.MA(historiesInPeriodOfTime, -MACham);
                    var phienHumNayMa05 = phienHumNay.MA(historiesInPeriodOfTime, -MANhanh);
                    var phienHumWaMa05 = phienHumWa.MA(historiesInPeriodOfTime, -MANhanh);
                    var phienHumWaMa20 = phienHumWa.MA(historiesInPeriodOfTime, -MACham);

                    /* 
                     * Xác định trend
                     *      + Tăng
                     *          + Kết thúc đà tăng
                     *      + Giảm
                     *          + Kết thúc đà giảm
                     *      + Sideway 
                     *          + Phân phối
                     *          + Tích lũy
                     *          
                     * bands dưới   giảm dần đều trong >= 1 phiên gần nhất >=1%                                     tín hiệu chuyển wa trend giảm nhẹ
                     *                                 >= 2 phiên gần nhất >=3%                                     tín hiệu chuyển wa trend giảm trung bình
                     *                                 >= 3 phiên gần nhất >=5%                                     tín hiệu chuyển wa trend giảm bền vững
                     *                                 >= 4 phiên gần nhất >=7%                                     tín hiệu chuyển wa trend giảm mạnh
                     *              có khoảng cách tới MA 20 tăng dần trong >= 1 phiên gần đây                      tín hiệu chuyển wa trend giảm nhẹ
                     *                                                      >= 2 phiên gần đây                      tín hiệu chuyển wa trend giảm trung bình
                     *                                                      >= 3 phiên gần đây                      tín hiệu chuyển wa trend giảm bền vững
                     *                                                      >= 4 phiên gần đây                      tín hiệu chuyển wa trend giảm mạnh
                     *              có khoảng cách tới MA 20 giảm dần trong >= 1 phiên gần đây                      tín hiệu kết thúc trend giảm tạo đáy way đầu, hoặc giá tiệm cận hỗ trợ nhẹ
                     *                                                      >= 2 phiên gần đây                      tín hiệu kết thúc trend giảm tạo đáy way đầu, hoặc giá tiệm cận hỗ trợ trung bình
                     *                                                      >= 3 phiên gần đây                      tín hiệu kết thúc trend giảm tạo đáy way đầu, hoặc giá tiệm cận hỗ trợ bền vững
                     *                                                      >= 4 phiên gần đây                      tín hiệu kết thúc trend giảm tạo đáy way đầu, hoặc giá tiệm cận hỗ trợ mạnh
                     *              tăng dần đều trong >= 1 phiên gần nhất >=1%                                     tín hiệu kết thúc trend giảm tạo đáy way đầu, hoặc giá tiệm cận hỗ trợ nhẹ
                     *                                 >= 2 phiên gần nhất >=3%                                     tín hiệu kết thúc trend giảm tạo đáy way đầu, hoặc giá tiệm cận hỗ trợ trung bình
                     *                                 >= 3 phiên gần nhất >=5%                                     tín hiệu kết thúc trend giảm tạo đáy way đầu, hoặc giá tiệm cận hỗ trợ bền vững
                     *                                 >= 4 phiên gần nhất >=7%                                     tín hiệu kết thúc trend giảm tạo đáy way đầu, hoặc giá tiệm cận hỗ trợ mạnh
                     *              trong vòng >= 1 phiên, giá trị ko chênh lệch quá 2% so với ngày quá khứ         tín hiệu chuyển wa sideway nhẹ
                     *              trong vòng >= 3 phiên, giá trị ko chênh lệch quá 2% so với ngày quá khứ         tín hiệu chuyển wa sideway trung bình
                     *              trong vòng >= 5 phiên, giá trị ko chênh lệch quá 2% so với ngày quá khứ         tín hiệu chuyển wa sideway mạnh
                     *              trong vòng >= 7 phiên, giá trị ko chênh lệch quá 2% so với ngày quá khứ         tín hiệu chuyển wa sideway cực mạnh
                     *              
                     * bands trên   tăng dần đều trong >= 1 phiên gần nhất                      + giá >= MA 20      tín hiệu chuyển wa trend tăng nhẹ
                     *                                 >= 2 phiên gần nhất                      + giá >= MA 20      tín hiệu chuyển wa trend tăng trung bình
                     *                                 >= 3 phiên gần nhất                      + giá >= MA 20      tín hiệu chuyển wa trend tăng bền vững
                     *                                 >= 4 phiên gần nhất                      + giá >= MA 20      tín hiệu chuyển wa trend tăng mạnh
                     *              có khoảng cách tới MA 20 tăng dần trong >= 1 phiên gần đây  + giá >= MA 20      tín hiệu chuyển wa trend tăng nhẹ
                     *                                                      >= 2 phiên gần đây  + giá >= MA 20      tín hiệu chuyển wa trend tăng trung bình
                     *                                                      >= 3 phiên gần đây  + giá >= MA 20      tín hiệu chuyển wa trend tăng bền vững
                     *                                                      >= 4 phiên gần đây  + giá >= MA 20      tín hiệu chuyển wa trend tăng mạnh
                     *              có khoảng cách tới MA 20 giảm dần trong >= 1 phiên gần đây  + giá >= MA 20      tín hiệu kết thúc trend tăng tạo đỉnh way đầu, hoặc giá tiệm cận kháng cự nhẹ
                     *                                                      >= 2 phiên gần đây  + giá >= MA 20      tín hiệu kết thúc trend tăng tạo đỉnh way đầu, hoặc giá tiệm cận kháng cự trung bình
                     *                                                      >= 3 phiên gần đây  + giá >= MA 20      tín hiệu kết thúc trend tăng tạo đỉnh way đầu, hoặc giá tiệm cận kháng cự bền vững
                     *                                                      >= 4 phiên gần đây  + giá >= MA 20      tín hiệu kết thúc trend tăng tạo đỉnh way đầu, hoặc giá tiệm cận kháng cự mạnh
                     *              tăng dần đều trong >= 1 phiên gần nhất >=1%                                     tín hiệu kết thúc trend tăng tạo đỉnh way đầu, hoặc giá tiệm cận kháng cự nhẹ
                     *                                 >= 2 phiên gần nhất >=3%                                     tín hiệu kết thúc trend tăng tạo đỉnh way đầu, hoặc giá tiệm cận kháng cự trung bình
                     *                                 >= 3 phiên gần nhất >=5%                                     tín hiệu kết thúc trend tăng tạo đỉnh way đầu, hoặc giá tiệm cận kháng cự bền vững
                     *                                 >= 4 phiên gần nhất >=7%                                     tín hiệu kết thúc trend tăng tạo đỉnh way đầu, hoặc giá tiệm cận kháng cự mạnh
                     *              trong vòng >= 1 phiên, giá trị ko chênh lệch quá 2% so với ngày quá khứ         tín hiệu chuyển wa sideway nhẹ
                     *              trong vòng >= 3 phiên, giá trị ko chênh lệch quá 2% so với ngày quá khứ         tín hiệu chuyển wa sideway trung bình
                     *              trong vòng >= 5 phiên, giá trị ko chênh lệch quá 2% so với ngày quá khứ         tín hiệu chuyển wa sideway mạnh
                     *              trong vòng >= 7 phiên, giá trị ko chênh lệch quá 2% so với ngày quá khứ         tín hiệu chuyển wa sideway cực mạnh
                     *
                     * 
                     * MA 5         cắt lên MA 20                                                                   tín hiệu chuyển wa trend tăng trung bình
                     *              cắt xuống MA 20                                                                 tín hiệu chuyển wa trend giảm trung bình
                     *              giảm dần đều trong >= 1 phiên gần nhất  >=1%                                    tín hiệu chuyển wa trend giảm nhẹ
                     *                                 >= 2 phiên gần nhất  >=3%                                    tín hiệu chuyển wa trend giảm trung bình
                     *                                 >= 3 phiên gần nhất  >=5%                                    tín hiệu chuyển wa trend giảm bền vững
                     *                                 >= 4 phiên gần nhất  >=7%                                    tín hiệu chuyển wa trend giảm mạnh
                     *              ở trên MA 20 và có khoảng cách với MA 20 tăng dần trong  >= 1 phiên gần đây     tín hiệu chuyển wa trend tăng nhẹ
                     *                                                                       >= 2 phiên gần đây     tín hiệu chuyển wa trend tăng trung bình
                     *                                                                       >= 3 phiên gần đây     tín hiệu chuyển wa trend tăng bền vững
                     *                                                                       >= 4 phiên gần đây     tín hiệu chuyển wa trend tăng mạnh
                     *                                                                       
                     *                                                 MA 20 giảm dần trong  >= 1 phiên gần đây     tín hiệu chuyển wa trend giảm nhẹ
                     *                                                                       >= 2 phiên gần đây     tín hiệu chuyển wa trend giảm trung bình
                     *                                                                       >= 3 phiên gần đây     tín hiệu chuyển wa trend giảm bền vững
                     *                                                                       >= 4 phiên gần đây     tín hiệu chuyển wa trend giảm mạnh
                     *                                                                       
                     *              ở dưới MA 20 và có khoảng cách với MA 20 giảm dần trong  >= 1 phiên gần đây     tín hiệu chuyển wa trend tăng nhẹ
                     *                                                                       >= 2 phiên gần đây     tín hiệu chuyển wa trend tăng trung bình
                     *                                                                       >= 3 phiên gần đây     tín hiệu chuyển wa trend tăng bền vững
                     *                                                                       >= 4 phiên gần đây     tín hiệu chuyển wa trend tăng mạnh
                     *                                                                       
                     *                                                 MA 20 tăng dần trong  >= 1 phiên gần đây     tín hiệu chuyển wa trend giảm nhẹ               
                     *                                                                       >= 2 phiên gNhần đây     tín hiệu chuyển wa trend giảm trung bình                      
                     *                                                                       >= 3 phiên gần đây     tín hiệu chuyển wa trend giảm bền vững                    
                     *                                                                       >= 4 phiên gần đây     tín hiệu chuyển wa trend giảm mạnh                
                     *                                                                       
                     *              tăng dần đều trong >= 1 phiên gần nhất  >=1%                                    tín hiệu chuyển wa trend tăng nhẹ
                     *                                 >= 2 phiên gần nhất  >=3%                                    tín hiệu chuyển wa trend tăng trung bình
                     *                                 >= 3 phiên gần nhất  >=5%                                    tín hiệu chuyển wa trend tăng bền vững
                     *                                 >= 4 phiên gần nhất  >=7%                                    tín hiệu chuyển wa trend tăng mạnh
                     * 
                     * MACD         hướng lên                       trên 0                                          tín hiệu chuyển wa trend tăng trung bình
                     * MACD         hướng lên                       dưới 0                                          tín hiệu chuyển wa trend tăng nhẹ
                     * MACD         cắt lên signal                                                                  tín hiệu chuyển wa trend tăng trung bình
                     * MACD         hướng xuống                     dưới 0                                          tín hiệu chuyển wa trend giảm trung bình
                     * MACD         hướng xuống                     trên 0                                          tín hiệu chuyển wa trend giảm nhẹ
                     * MACD         cắt xuống signal                                                                tín hiệu chuyển wa trend giảm trung bình
                     * Momentum     giảm dần                        trên 0                                          tín hiệu chuyển wa trend giảm nhẹ
                     * Momentum     giảm dần                        dưới 0                                          tín hiệu chuyển wa trend giảm trung bình
                     * Momentum     tăng dần                        trên 0                                          tín hiệu chuyển wa trend tăng trung bình
                     * Momentum     tăng dần                        dưới 0                                          tín hiệu chuyển wa trend tăng nhẹ
                     * 
                     * 
                     * 
                     * Ichi         Tenkan cắt lên Kijun            trên mây,   chikou & Price trên mây             tín hiệu chuyển wa trend tăng mạnh
                     *                                              trong mây   chikou & Price trên mây             tín hiệu chuyển wa trend tăng trung bình
                     *                                              dưới mây                                        N/A
                     *              Tenkan xuống Kijun              trên mây                                        N/A
                     *                                              trong mây   chikou & Price trong mây            tín hiệu chuyển wa trend giảm trung bình
                     *                                              dưới mây    chikou & Price dưới mây             tín hiệu chuyển wa trend giảm mạnh
                     *              Span A cắt lên trên Span B                                                      tín hiệu chuyển wa trend tăng nhẹ
                     *              Span A cắt dưới Span B                                                          tín hiệu chuyển wa trend giảm nhẹ
                     *              
                     * Giá          nến top chạm bands top                                                          tín hiệu chuyển wa trend giảm nhẹ
                     *              nến bot chạm bands dưới                                                         tín hiệu chuyển wa trend tăng nhẹ
                     *              nến bot vượt ra khỏi bands top                                                  tín hiệu chuyển wa trend giảm trung bình
                     *              nến top vượt ra khỏi bands top                                                  tín hiệu chuyển wa trend giảm nhẹ
                     *              nến top vượt ra khỏi bands bot                                                  tín hiệu chuyển wa trend tăng trung bình
                     *              nến xanh                                                                        tín hiệu chuyển wa trend tăng nhẹ
                     *              nến đỏ                                                                          tín hiệu chuyển wa trend giảm nhẹ
                     *              thân nến xanh dài               từ dưới MA 20 vượt lên gần bands top            tín hiệu chuyển wa trend giảm trung binh
                     *              bật lên chạm MA 05                                                              tín hiệu chuyển wa trend tăng trung bình
                     *              tụt xuống chạm MA 20                                                            tín hiệu chuyển wa trend giảm trung bình
                     * 
                     * Vol          > MA 20                         giá tăng                                        tín hiệu chuyển wa trend tăng nhẹ
                     * Vol          > MA 20                         giá giảm                                        tín hiệu chuyển wa trend giảm nhẹ
                     * Vol          < MA 20                         giá tăng                                        tín hiệu chuyển wa trend tăng nhẹ
                     * Vol          < MA 20                         giá giảm                                        tín hiệu chuyển wa trend giảm nhẹ
                     *              
                     * 
                     * 
                     */

                    //tính hiệu quả

                    //tín hiệu mua
                    var Ma05DuoiMa20 = phienHumNayMa05 < phienHumNayMa20;
                    var MA05HuongLen = phienHumWaMa05 < phienHumNayMa05;
                    var nenTangGia = phienHumNay.TangGia();
                    var nenTangLenChamMa20 = phienHumNay.NenTop >= phienHumNayMa20 && phienHumNay.NenBot < phienHumNayMa20;             //Giá trong phiên MA 05 tăng lên chạm MA 20
                    var râunếnTangLenChamMa20 = phienHumNay.H >= phienHumNayMa20 && phienHumNay.NenBot < phienHumNayMa20;               //Giá trong phiên MA 05 tăng lên chạm MA 20
                    var duongMa05CatLenTrenMa20 = phienHumWaMa05 < phienHumNayMa20 && phienHumNayMa05 > phienHumNayMa20;     //MA 05 cắt lên trên MA 20
                    var nenNamDuoiMA20 = phienHumNay.NenBot < phienHumNayMa20;                                                            //Giá nằm dưới MA 20
                    var thânNếnKhôngVượtQuáBandTren = phienHumNay.NenTop < phienHumNay.BandsTop;


                    /*TODO: cảnh báo mua nếu giá mở cửa tăng chạm bands trên. Ví dụ 9/4/2021 KBC, 17/1/2022 VNM - tăng gần chạm bands mà ko thấy dấu hiệu mở bands rộng ra, mA 20 cũng ko hướng lên - sideway
                     * 10-2-22 VNM: bands ko mở rộng, bands tren hướng xuống, bands dưới đi ngang, các giá trước loanh quanh MA 20, ko có dấu hiệu phá bỏ sideway
                     * var khángCựĐỉnh = phienKiemTra.KhángCựĐỉnh(historiesInPeriodOfTime);
                     * var khángCựBands = phienKiemTra.KhángCựBands(historiesInPeriodOfTime);
                     * var khángCựFibonacci = phienKiemTra.KhángCựFibonacci(historiesInPeriodOfTime);
                     * var khángCựIchimoku   = phienKiemTra.KhángCựIchimoku(historiesInPeriodOfTime);
                     * 
                     * Ví dụ: 
                     *  KBC - 22/3/22 (MA + bands) -> nhưng có thể xét vì giá đã tăng gần tới viền mây dưới ichimoku rồi nên ko mua, hoặc giá từ MA 5 đi lên (hổ trợ lên kháng cự) ở MA 20, và bands bóp nên giá chỉ quay về MA20
                     *  KBC - 03/3/20 (MA + bands) -> MA, Bands đi ngang, có thể mua để lợi T+ => thất bại -> có thể xét tới MACD trong trường hợp này:
                     *                              + MACD dưới 0, đỏ cắt lên xanh
                     *                              + Đã mua ở ngày 26/2 rồi, MACD chưa vượt 0 lên dương mạnh thì cũng ko cần mua thêm
                     *                              + => bands hẹp, bands ko thay đổi, ma 20 k thay đổi, thân nến ở giữa bands => ko mua, vì rất dễ sảy T3
                     *                                      
                     *  KBC - 10/3/20 -> 17/3/20 - Nếu bất chấp nến ngoài bolinger bands dưới để mua, thì hãy cân nhắc KBC trong những ngày này -> nên kết hợp MACD (macd giảm, momentum âm mạnh) 
                     *                              + => CThuc A
                     *                                                     
                     *  KBC - 27/03/20 tới 31/03/2020 -> Nến rút chân xanh 3 cây liên tục, bands dưới vòng cung lên, band trên đi xuống => bands bóp => biên độ cực rộng => giá sẽ qua về MA 20
                     *                                + 3 cây nến xanh bám ở MA 5 liên tục, rút chân lên => mua vô ở cây sau được, giá mua vô từ trung bình râu nến dưới của 2 cây trước (do tín hiệu tăng) lên tới MA 5, ko cần mua đuổi
                     *                                + Theo dõi sau đó, vì nếu band tăng, MA 20 tăng, thì MA 20 sẽ là hỗ trợ cho nhịp hồi này, khi nào bán?                                          
                     *                                      + RSI rớt way về quanh 80 thì xả từ từ
                     *                                      + Giá dưới MA 5 2 phiên thì xả thêm 1 đoạn
                     *                                      + MA 5 cắt xuống MA 20 thì xả hết
                     *                              + => CThuc A
                     *                                  
                     *  KBC - 7/07/20 tới 22/07/2020 -> từ 26/6/20 tới 6/7/20 -> giao dịch quanh kháng cự là MA 5, và hổ trợ là bands dưới
                     *                                  + ngày 7/7/20 -> giá vượt kháng cự (MA 05), MACD xanh bẻ ngang lúc này kháng cự mới sẽ là MA 20, MA 5 sẽ là hỗ trợ
                     *                                  + có thể ra nhanh đoạn này khi T3 về (13/7/20) vì giá vượt kháng cự, nhưng lại lạ nến đỏ => ko qua dc, dễ dội về hỗ trợ => ko cần giữ lâu
                     *                              + => CThuc A
                     *  KBC - 10/8/20 (MA + bands) -> Nếu mua ngày 4/8/20 (trước đó 4 ngày) vì phân kì tăng RSI, thì mình có thể tránh trường hợp này
                     *                                + 31/7/20: nến doji tăng ngoài bands dưới
                     *                                + 03/8/20: nến tăng xác nhận doji trước là đáy -> cuối ngày ngồi coi - RSI tăng, Giá giảm -> tín hiệu đảo chiều -> nên mua vô
                     *                                + 04/8/20: mua vô ở giá đóng cửa của phiên trước (03/8/20)
                     *                                + Giá tăng liên tục trong những phiên sau, nhưng vol < ma 20, thân nến nhỏ => giao dịch ít, lưỡng lự, ko nên tham gia lâu dài trong tình huống này
                     *                                + Nếu lỡ nhịp mua ngày 04/8/20 rồi thì thôi
                     *                              + => CThuc A
                     *  KBC - 12/11/20 tới 17/11/2020 -> Nếu đã mua ngày 11/11/2020, thì nên theo dõi thêm MACD để tránh bán hàng mà lỡ nhịp tăng
                     *                                + MACD cắt ngày 11/11/20, tạo tín hiệu đảo chiều, kết hợp với những yếu tố đi kèm,
                     *                                + MACD tăng dần lên 0, momentum tăng dần theo, chờ vượt 0 là nổ
                     *                              + => CThuc B
                     *  KBC - 21/5/21 -> Lưu ý đặt sẵn giá mua ở giá sàn những ngày này nếu 2 nến trước đã tạo râu bám vô MA 05, nếu ko có râu bám vô MA 5 thì thôi
                     *                                + nếu có râu nên bám vô, thì đặt sẵn giá mua = giá từ giá thấp nhất của cây nến thứ 2 có râu
                     *                                  
                     *                                  
                     *  KBC - 12/7/21 - 26/7/21  -> giống đợt 31/7/20 tới 4/8/20
                     *                              + Ngày 12/7 1 nến con quay dài xuất hiện dưới bands bot => xác nhận đáy, cùng đáy với ngày 21/5/21 => hỗ trợ mạnh vùng thân nến đỏ trải xuống râu nến này, có thể vô tiền 1 ít tầm giá này
                     *                              + Sau đó giá bật lại MA 5
                     *                              + tới ngày 19/7/20 - 1 cây giảm mạnh, nhưng giá cũng chỉ loanh quanh vùng hỗ trợ này, vol trong nhịp này giảm => hết sức đạp, cũng chả ai muốn bán
                     *                              + RSI trong ngày 19/7, giá đóng cửa xuống giá thấp hơn ngày 12/7, nhưng RSI đang cao hơn => tín hiệu đảo chiều 
                     *                              + - nhưng cần theo dõi thêm 1 phiên, nếu phiên ngày mai xanh thì ok, xác nhận phân kỳ tăng => Có thể mua vô dc
                     *                              
                     *  Bands và giá rớt (A - Done)
                     *      + Nếu giá rớt liên tục giữa bands và MA 5, nếu xuất hiện 1 cây nến có thân rớt ra khỏi bands dưới, có râu trên dài > 2 lần thân nến, thì bắt đầu để ý vô lệnh mua
                     *      + (A1) Nếu nến rớt ngoài bands này là nến xanh => đặt mua ở giá quanh thân nến
                     *      + (A2) Nếu nến rớt ngoài bands này là nến đỏ   => tiếp tục chờ 1 cây nến sau cây đỏ này, nếu vẫn là nến đỏ thì bỏ, nếu là nến xanh thì đặt mua cây tiếp theo
                     *          + đặt mua ở giá trung bình giữa giá mở cửa của cây nến đỏ ngoài bands và giá MA 5 ngày hum nay
                     *          
                     *      + Ví dụ: KBC: 10/3/20 -> 17/3/20                                                    03/8/20                 3/11/20                                     12/7/21 - 26/7/21                   
                     *                  - RSI dương   (cây nến hiện tại hoặc 1 trong 3 cây trước là dc)         RSI dương               RSI dương                                   Cây nến 13/7/21 ko tăng
                     *                  - MACD momentum tăng->0                                                 Tăng                    Tăng                                        Tăng rất nhẹ (~2%)
                     *                  - MACD tăng                                                             Tăng                    Giảm nhẹ hơn trước (-5 -> -41 -> -50)       Giảm
                     *                  - nến tăng                                                              Tăng                    Tăng                                        Tăng
                     *                  - giá bật từ bands về MA 5                                              OK                      OK
                     *                  - 13 nến trước (100% bám bands dưới, thân nến dưới MA 5                 7 (100%) dưới MA 5      4/5 cây giảm (80%) ko chạm MA 5             4/6 nến giảm liên tục ko chạm MA 5
                     *                  - MA 5 bẻ ngang -> giảm nhẹ hơn 2 phiên trước:                          MA 5 tăng               14330 (-190) -> 14140 (-170)
                     *                      + T(-1) - T(0) < T(-3) - T(-2) && T(-2) - T(-1)                                             -> 13970 (-60) -> 13910
                     *                  - Khoảng cách từ MA 5 tới MA 20 >= 15%                                  > 15%                   4% (bỏ)                                     12%
                     *                      + vì giá sẽ về MA 20, nên canh tí còn ăn
                     *                      + mục tiêu là 10% trong những đợt hồi này, nên mua quanh +-3%       +-3%
                     *                      + Khoảng cách càng lớn thì đặt giá mua càng cao, tối đa 3%
                     *                      + Cân nhắc đặt ATO cho dễ tính => đặt giá C như bth
                     *      
                    */
                    var bandsTrenHumNay = phienHumNay.BandsTop;
                    var bandsDuoiHumNay = phienHumNay.BandsBot;
                    var bandsTrenHumWa = phienHumWa.BandsTop;
                    var bandsDuoiHumWa = phienHumWa.BandsBot;

                    var bắtĐầuMuaDoNếnTăngA1 = phienHumNay.NếnĐảoChiềuTăngMạnhA1();
                    var bắtĐầuMuaDoNếnTăngA2 = phienHumNay.NếnĐảoChiềuTăngMạnhA2(phienHumWa);

                    var bandsTrênĐangGiảm = bandsTrenHumNay < bandsTrenHumWa;
                    var bandsMởRộng = bandsTrenHumNay > bandsTrenHumWa && bandsDuoiHumNay > bandsDuoiHumWa;
                    var bandsĐangBópLại = bandsTrenHumNay < bandsTrenHumWa && bandsDuoiHumNay > bandsDuoiHumWa;
                    var ma20ĐangGiảm = phienHumNayMa20 < phienHumWaMa20;


                    var bandsKhôngĐổi = bandsTrenHumNay == bandsTrenHumWa && bandsDuoiHumNay == bandsDuoiHumWa;
                    var ma20KhôngĐổi = phienHumNayMa20 == phienHumWaMa20;
                    var giaOGiuaBands = phienHumNay.NenBot * 0.93M < phienHumNay.BandsBot && phienHumNay.NenTop * 1.07M > phienHumNay.BandsTop;

                    var muaTheoMA = thânNếnKhôngVượtQuáBandTren && nenTangGia && ((duongMa05CatLenTrenMa20 && nenNamDuoiMA20)
                                                    || (MA05HuongLen && (nenTangLenChamMa20 || râunếnTangLenChamMa20) && Ma05DuoiMa20));
                    var nếnTụtMạnhNgoàiBandDưới = phienHumNay.BandsBot > phienHumNay.NenBot + ((phienHumNay.NenTop - phienHumNay.NenBot) / 2);

                    var momenTumTốt = (phienHumKia.MACDMomentum.IsDifferenceInRank(phienHumWa.MACDMomentum, 0.01M) || phienHumWa.MACDMomentum > phienHumKia.MACDMomentum) && phienHumNay.MACDMomentum > phienHumWa.MACDMomentum * 0.96M;
                    var momenTumTăngTốt = phienHumWa.MACDMomentum > phienHumKia.MACDMomentum * 0.96M && phienHumNay.MACDMomentum > phienHumWa.MACDMomentum * 0.96M;

                    var nếnBậtMạnhLênTừBandsDướiVềMA05HoặcTrongBands =
                        (phienHumWa.NenTop < phienHumWa.BandsBot
                            || (phienHumWa.NenBot.IsDifferenceInRank(phienHumWa.BandsBot, 0.02M) && phienHumWa.NenTop < phienHumWaMa05))
                        && (phienHumNay.NenTop >= phienHumNayMa05 || phienHumNay.NenBot >= phienHumNay.BandsBot);

                    var trongXuHướngGiảmMạnh = phienHumNay.TỉLệNếnCựcYếu(histories) >= 0.5M;
                    var trongXuHướngGiảm = phienHumNay.TỉLệNếnYếu(histories) >= 0.5M;

                    var ma05ĐangBẻNgang = phienHumNay.MAChuyểnDần(histories, false, -5, 3);
                    var khôngNênBánT3 = phienHumNay.MACDMomentum > phienHumWa.MACDMomentum && phienHumNay.MACD > phienHumWa.MACD && phienHumNay.MACDMomentum > -100;

                    var rsiDuong = phienHumNay.RSIDương(histories);
                    var tínHiệuMuaTrongSóngHồiMạnh =
                                                   momenTumTăngTốt
                                                && phienHumNay.TangGia()
                                                && (nếnBậtMạnhLênTừBandsDướiVềMA05HoặcTrongBands || ma05ĐangBẻNgang)
                                                && (trongXuHướngGiảmMạnh)
                                                && phienHumNayMa20 / phienHumNayMa05 > 1.1M;

                    var tínHiệuMuaTrongSóngHồiTrungBình =
                                                   momenTumTốt
                                                && (phienHumNay.TangGia() || phienHumNay.Doji())
                                                && (nếnBậtMạnhLênTừBandsDướiVềMA05HoặcTrongBands || ma05ĐangBẻNgang)
                                                && (trongXuHướngGiảmMạnh || trongXuHướngGiảm)
                                                && phienHumNayMa20 / phienHumNayMa05 > 1.1M;

                    var tinhieuMuaManh = tínHiệuMuaTrongSóngHồiMạnh ? 10 : 0;
                    var tinhieuMuaTrungBinh = tínHiệuMuaTrongSóngHồiTrungBình ? 5 : 0;
                    var tinHieuMuaTrungBinh1 = muaTheoMA ? 5 : 0;
                    var tinHieuMuaTrungBinh2 = nếnTụtMạnhNgoàiBandDưới && ma05ĐangBẻNgang ? 5 : 0;
                    var tinHieuMuaYếu1 = bandsMởRộng ? 5 : 0;

                    var tinHieuGiảmMua1 = bandsTrênĐangGiảm && ma20ĐangGiảm ? -5 : 0;// && !nếnGiảmVượtMạnhNgoàiBandDưới ? -5 : 0;
                    //var tinHieuGiảmMua2 = bandsKhôngĐổi && ma20KhôngĐổi && giaOGiuaBands ? -5 : 0;// && !nếnGiảmVượtMạnhNgoàiBandDưới ? -5 : 0;
                    var tinHieuGiảmMua2 = bandsKhôngĐổi && ma20KhôngĐổi ? -5 : 0;// && !nếnGiảmVượtMạnhNgoàiBandDưới ? -5 : 0;
                    var tinHieuGiảmMua3 = giaOGiuaBands ? -5 : 0;// && !nếnGiảmVượtMạnhNgoàiBandDưới ? -5 : 0;
                    var tinHieuGiảmMua4 = !phienHumNay.SoSánhGiá(1) || !phienHumWa.SoSánhGiá(1) ? -5 : 0;

                    if (tinhieuMuaManh
                        + tinhieuMuaTrungBinh + tinHieuMuaTrungBinh1 + tinHieuMuaTrungBinh2
                        + tinHieuGiảmMua1 + tinHieuGiảmMua2 + tinHieuGiảmMua3 + tinHieuGiảmMua4 <= 0) continue;

                    var ngayMua = historiesInPeriodOfTime.Where(h => h.Date > phienHumNay.Date).OrderBy(h => h.Date).FirstOrDefault();
                    if (ngayMua == null) ngayMua = new History() { Date = phienHumNay.Date.AddDays(1) };
                    var giáĐặtMua = nếnTụtMạnhNgoàiBandDưới
                        ? (phienHumNay.BandsBot + phienHumNay.NenBot) / 2
                        : phienHumNay.C;

                    //if (giáĐặtMua >= ngayMua.L && giáĐặtMua <= ngayMua.H)       //Giá hợp lệ
                    //{
                    var giữT = khôngNênBánT3 ? 6 : 3;
                    var tPlus = historiesInPeriodOfTime.Where(h => h.Date >= ngayMua.Date)
                        .OrderBy(h => h.Date)
                        .Skip(3)
                        .Take(giữT)
                        .ToList();

                    if (tPlus.Count < 3) //hiện tại
                    {
                        result1.Add($"{symbol._sc_} - Hiện tại điểm nhắc mua: {phienHumNay.Date.ToShortDateString()} ở giá {giáĐặtMua.ToString("N2")}");
                    }
                    else
                    {
                        if (tPlus.Any(t => t.C > ngayMua.O * (1M + percentProfit) || t.O > ngayMua.O * (1M + percentProfit)))    //Mình đặt mua ở giá mở cửa ngày hum sau luôn
                        {
                            dung++;
                            result1.Add($"{symbol._sc_} - Đúng T3-5 - Điểm nhắc mua: {phienHumNay.Date.ToShortDateString()} ở giá {giáĐặtMua.ToString("N2")}");
                        }
                        else
                        {
                            sai++;
                            result1.Add($"{symbol._sc_} - Sai  T3-5 - Điểm nhắc mua: {phienHumNay.Date.ToShortDateString()} ở giá {giáĐặtMua.ToString("N2")}");
                        }
                    }

                    //if (bandsĐangGiảm && ma20ĐangGiảm && !nếnGiảmVượtMạnhNgoàiBandDưới)
                    //{
                    //    if (tPlus.Any(t => t.C > ngayMua.O * 1.01M || t.O > ngayMua.O * 1.01M))    //Mình đặt mua ở giá mở cửa ngày hum sau luôn
                    //    {
                    //        sai++;
                    //        result1.Add($"{symbol._sc_} - Sai  - Band xấu - Điểm nhắc để ngày mai mua: {phienKiemTra.Date.ToShortDateString()} ở giá {giáĐặtMua.ToString("N2")}");
                    //    }
                    //    else
                    //    {
                    //        dung++;
                    //        result1.Add($"{symbol._sc_} - Đúng - Band xấu - Điểm nhắc để ngày mai mua: {phienKiemTra.Date.ToShortDateString()} ở giá {giáĐặtMua.ToString("N2")}");
                    //    }
                    //}

                    //if (!bandsĐangGiảm || !ma20ĐangGiảm || nếnGiảmVượtMạnhNgoàiBandDưới)
                    //{
                    //    if (tPlus.Any(t => t.C > ngayMua.O * 1.01M || t.O > ngayMua.O * 1.01M))    //Mình đặt mua ở giá mở cửa ngày hum sau luôn
                    //    {
                    //        dung++;
                    //        result1.Add($"{symbol._sc_} - Đúng T3-5 - Điểm nhắc để ngày mai mua: {phienKiemTra.Date.ToShortDateString()} ở giá {giáĐặtMua.ToString("N2")}");
                    //    }
                    //    else
                    //    {
                    //        sai++;
                    //        result1.Add($"{symbol._sc_} - Sai  T3-5 - Điểm nhắc để ngày mai mua: {phienKiemTra.Date.ToShortDateString()} ở giá {giáĐặtMua.ToString("N2")}");
                    //    }
                    //}
                }
                //else
                //{
                //    result1.Add($"{symbol._sc_} - Không có giá {giáĐặtMua.ToString("N2")} ở ngày mai mua: {phienKiemTra.Date.ToShortDateString()}");
                //}


                ////tín hiệu bán
                //if ((phienTruocPhienKiemTraMa05 > phienKiemTraMa05              //MA 05 đang hướng lên
                //        && phienKiemTra.NenBot <= phienKiemTraMa20)             //Giá MA 05 chạm MA 20
                //    || (phienTruocPhienKiemTraMa05 >= phienKiemTraMa20 && phienKiemTraMa05 <= phienKiemTraMa20))  //MA 05 cắt xuống dưới MA 20
                //{
                //    var ngayBán = historiesInPeriodOfTime.Where(h => h.Date > phienKiemTra.Date).OrderBy(h => h.Date).First();
                //    var tileChinhXac = 0;
                //    var tPlus = historiesInPeriodOfTime.Where(h => h.Date >= ngayBán.Date)
                //        .OrderBy(h => h.Date)
                //        .Skip(3)
                //        .Take(3)
                //        .ToList();

                //    if (tPlus.All(t => t.C > ngayBán.O || t.O > ngayBán.O))    //Mình đặt mua ở giá mở cửa ngày hum sau luôn
                //    {
                //        dung++;
                //        result1.Add($"{symbol._sc_} - Đúng T3-5 - Điểm nhắc để ngày mai bán: {phienKiemTra.Date.ToShortDateString()}");
                //    }
                //    else
                //    {
                //        sai++;
                //        result1.Add($"{symbol._sc_} - Sai  T3-5 - Điểm nhắc để ngày mai bán: {phienKiemTra.Date.ToShortDateString()} - Bán: {ngayBán.Date.ToShortDateString()} giá {ngayBán.O}");
                //    }
                //}
                //}

                tong = dung + sai;
                var tile = tong == 0 ? 0 : Math.Round(dung / tong, 2);
                //result1.Add($"Tỉ lệ: {tile}");
                tup.Add(new Tuple<string, decimal, List<string>>(symbol._sc_, tile, result1));
            });

            tup = tup.OrderByDescending(t => t.Item2).ToList();

            return tup;
        }

        public async Task<List<string>> ToiUuLoiNhuan(string code, DateTime toiNgay, DateTime tuNgay)
        {
            var ma20vol = 100000;
            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");
            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.Where(s => s.MA20Vol > ma20vol).ToListAsync()
                : await _context.StockSymbol.Where(s => s.MA20Vol > ma20vol && splitStringCode.Contains(s._sc_)).ToListAsync();
            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var historiesStockCode = await _context.History
                .Where(ss => stockCodes.Contains(ss.StockSymbol)
                    && ss.Date <= toiNgay.AddDays(10) //calculate T
                    && ss.Date >= tuNgay.AddDays(-150)) //caculate SRI
                .OrderByDescending(ss => ss.Date)
                .ToListAsync();


            //var histories = await _context.History
            //    .Where(ss => ss.StockSymbol == code
            //        && ss.Date <= ngay.AddDays(10) //calculate T
            //        && ss.Date >= ngayCuoi.AddDays(-50)) //caculate SRI
            //    .OrderBy(ss => ss.Date)
            //    .ToListAsync();

            var result1 = new List<string>();
            var NhậtKýMuaBán = new List<LearningRealDataModel>();

            for (int s = 0; s < symbols.Count; s++)
            {
                var byCodeList = historiesStockCode.Where(ss => ss.StockSymbol == symbols[s]._sc_).ToList();
                var pastList = byCodeList.Where(ss => ss.Date <= tuNgay.AddDays(-100)).OrderByDescending(h => h.Date).ToList();
                var firstDate = pastList.FirstOrDefault();
                var ngayBatDauCuaCoPhieu = firstDate != null ? tuNgay : historiesStockCode.Where(ss => ss.StockSymbol == symbols[s]._sc_).OrderBy(h => h.Date).Skip(100).FirstOrDefault()?.Date;
                if (!ngayBatDauCuaCoPhieu.HasValue) continue;
                var histories = historiesStockCode
                    .Where(ss => ss.StockSymbol == symbols[s]._sc_ && ss.Date >= ngayBatDauCuaCoPhieu.Value.AddDays(-50))
                    .OrderBy(ss => ss.Date)
                    .ToList();

                decimal root = 1M;
                var hasMoney = true;
                var ngayMuaToiUu = new History();
                var ngayMuaT3 = new History();

                var ngayBatDau = histories.First(h => h.Date >= ngayBatDauCuaCoPhieu);
                var ngayDungLai = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date >= toiNgay);
                if (ngayDungLai == null) //đang nhập ngày trong tương lai -> chuyển về hiện tại
                {
                    ngayDungLai = histories.OrderByDescending(h => h.Date).First();
                }
                var startedI = histories.IndexOf(ngayBatDau);
                var stoppedI = histories.IndexOf(ngayDungLai);
                for (int i = startedI; i <= stoppedI; i++)
                {
                    try
                    {
                        var phienHumKia = histories[i - 2];
                        var phienHumWa = histories[i - 1];
                        var phienHumNay = histories[i];
                        if (phienHumNay.Date < ngayMuaT3.Date) continue;
                        if (hasMoney && phienHumNay.VOL(histories, -20) < ma20vol) continue;

                        var phienT1 = histories[i + 1];
                        var phienT2 = histories[i + 2];
                        var phienT3 = histories[i + 3];
                        var phienT4 = histories[i + 4];

                        if (hasMoney)
                        {
                            if (phienT3.C <= phienHumNay.C || (phienT1.C < phienHumNay.C && phienT1.C <= phienT4.C))
                            {
                                continue;
                            }
                            else
                            {
                                ngayMuaToiUu = phienHumNay;
                                ngayMuaT3 = phienT3;
                                hasMoney = false;
                                result1.Add($"{phienHumNay.StockSymbol}-{phienHumNay.Date.ToShortDateString()} - MUA - {phienHumNay.C} - Vốn {root}");
                                NhậtKýMuaBán.Add(new LearningRealDataModel(histories, phienHumNay, phienHumWa, phienHumKia, true, 0, root));
                            }
                        }
                        else
                        {
                            var nextPhiens = new List<History>() { phienT1, phienT2, phienT3, phienT4 };
                            if (nextPhiens.All(p => p.C <= phienHumNay.C))
                            {
                                hasMoney = true;
                                var lời = ((Math.Round((decimal)phienHumNay.C / (decimal)(ngayMuaToiUu.C), 2)) - 1) * 100;
                                root = root + (Math.Round((decimal)root * (decimal)(lời) / 100, 2));
                                result1.Add($"{phienHumNay.StockSymbol}-{phienHumNay.Date.ToShortDateString()} - BÁN - {phienHumNay.C} Lời {lời}% - Vốn {root}");

                                //update lời cho lần trước
                                NhậtKýMuaBán.Last().Loi = lời;
                                NhậtKýMuaBán.Add(new LearningRealDataModel(histories, phienHumNay, phienHumWa, phienHumKia, false, lời, root));
                                ngayMuaToiUu = new History();
                                ngayMuaT3 = new History();
                                continue;
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        result1.Add($"{symbols[s]._sc_}-{histories[i].Date.ToShortDateString()} - Lỗi {ex.ToString()}");
                        continue;
                    }
                }
            }

            /*
             * Tìm kịch bản cho những ngày trước khi mua/bán    (Done)
             * Tìm kịch bản cho những ngày mua/bán              (thêm cột cho những ngày mua/bán)
             * 
             * [Vòng lặp]
             *  -   duyệt lại kịch bản, chọn thông số phù hợp
             *  -   chạy lại kịch bản dựa trên thông số phù hợp với điểm mua và bán
             *  
             * Chạy cho tất cả các mã
             * [Vòng lặp]
             *  -   duyệt lại kịch bản, chọn thông số phù hợp
             *  -   chạy lại kịch bản dựa trên thông số phù hợp với điểm mua và bán
             */

            var folder = ConstantPath.Path;
            var g = Guid.NewGuid();
            var name = $@"{folder}{g}.xlsx";
            NhậtKýMuaBán.ToDataTable().WriteToExcel(name);

            return result1;
        }

        public async Task<List<string>> KiemTraTileDungSaiTheoPattern(string code, DateTime tuNgay, DateTime toiNgay, LocCoPhieuFilterRequest filter)
        {
            var histories = await _context.History
                .Where(ss => ss.StockSymbol == code
                    && ss.Date <= toiNgay.AddDays(10)
                    && ss.Date >= tuNgay.AddDays(-30))
                .OrderBy(ss => ss.Date)
                .ToListAsync();

            var result1 = new List<string>();
            var ngayBatDau = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date >= tuNgay);
            for (int i = histories.IndexOf(ngayBatDau); i < histories.Count; i++)
            {
                ngayBatDau = histories[i];
                if (ngayBatDau != null && ngayBatDau.HadAllIndicators())
                {
                    break;
                }
            }

            var ngayMuaToiUu = new History();
            var ngayMuaT3 = new History();
            var tup = new List<Tuple<string, decimal, List<string>>>();
            decimal dung = 0;
            decimal sai = 0;
            var ngayDungLai = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date >= toiNgay);
            if (ngayDungLai == null) //đang nhập ngày trong tương lai -> chuyển về hiện tại
            {
                ngayDungLai = histories.OrderByDescending(h => h.Date).First();
            }
            var startedI = histories.IndexOf(ngayBatDau);
            var stoppedI = histories.IndexOf(ngayDungLai);
            for (int i = startedI; i <= stoppedI; i++)
            {
                var phienHumNay = histories[i];
                var phienHumWa = histories[i - 1];
                var lstBan = new List<Tuple<string, bool>>();

                var thoaDK = ThỏaĐiềuKiệnLọc(filter, histories, phienHumNay);
                if (!thoaDK) continue;

                lstBan.Add(new Tuple<string, bool>(filter.Name, thoaDK));

                var giaMua = TimGiaMuaMongMuon(histories, phienHumNay, lstBan);

                if (!giaMua.Item3.HasValue)
                {
                    var stringCTMua = string.Join(",", lstBan.Where(ct => ct.Item2).Select(ct => ct.Item1).ToList());
                    result1.Add($"{code} - {stringCTMua} - Mua phiên sau ngày {phienHumNay.Date.ToShortDateString()} tại giá {giaMua.Item1} tới giá {giaMua.Item2} - Chưa đủ dữ liệu T3 để tính toán giá bán.");
                    continue;
                }

                if (giaMua.Item3.HasValue && giaMua.Item3.Value < 0)
                {
                    var ngayMua = histories.Where(h => h.Date > phienHumNay.Date).OrderBy(h => h.Date).First();
                    var stringCTMua = string.Join(",", lstBan.Where(ct => ct.Item2).Select(ct => ct.Item1).ToList());
                    result1.Add($"{code} - {stringCTMua} - Mua phiên sau ngày {phienHumNay.Date.ToShortDateString()} tại giá {giaMua.Item1} tới giá {giaMua.Item2} - Nhưng thực tế giá thấp nhất ở {ngayMua.L}");
                    continue;
                }

                TimThoiGianBan(code, result1, ref dung, ref sai, histories, phienHumNay, giaMua.Item3.Value, lstBan);
            }

            if (result1.Any())
            {
                //tong = dung + sai;
                dung = dung - sai;
                var winRate = dung;// tong == 0 ? 0 : Math.Round(dung / tong, 2);
                tup.Add(new Tuple<string, decimal, List<string>>(code, winRate, result1));
            }


            //var tile = dung + sai > 0 ? Math.Round(dung / (dung + sai), 2) : 0;
            //result1.Insert(0, $"{code} - Tỉ lệ: {tile}");

            return result1;
        }

        public async Task<List<string>> BoLocCoPhieu(string code, DateTime ngay)
        {
            var ngayBatDauKiemTraTiLeDungSai = new DateTime(2022, 1, 1);
            var boloc = new LocCoPhieuRequest(code, ngay);

            CongThuc.allCongThuc.Clear();
            CongThuc.allCongThuc.AddRange(new List<LocCoPhieuFilterRequest>() {
                CongThuc.CT1A, CongThuc.CT1B, CongThuc.CT1C, CongThuc.CT3, CongThuc.CT1B2, CongThuc.CT1B3,
                CongThuc.CT2B,CongThuc.CT2C, CongThuc.CT2D,CongThuc.CT2E, CongThuc.CT2F
            });

            boloc.Filters.AddRange(CongThuc.allCongThuc);

            var ma20vol = 100000;
            var splitStringCode = string.IsNullOrWhiteSpace(boloc.Code) ? new string[0] : boloc.Code.Split(",");

            //TODO: validation
            if (!boloc.Filters.Any()) return new List<string>() { "Bộ lọc cổ phiếu trống rỗng" };

            var predicate = PredicateBuilder.New<StockSymbol>();
            predicate.And(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false);

            predicate = string.IsNullOrWhiteSpace(boloc.Code)
                ? boloc.VolToiThieu != null
                    ? boloc.VolToiThieu.Ope == SoSanhEnum.LonHon
                            ? predicate.And(s => s.MA20Vol > boloc.VolToiThieu.Value)
                            : boloc.VolToiThieu.Ope == SoSanhEnum.LonHonHoacBang
                                ? predicate.And(s => s.MA20Vol >= boloc.VolToiThieu.Value)
                                : boloc.VolToiThieu.Ope == SoSanhEnum.Bang
                                    ? predicate.And(s => s.MA20Vol == boloc.VolToiThieu.Value)
                                    : boloc.VolToiThieu.Ope == SoSanhEnum.NhoHonHoacBang
                                        ? predicate.And(s => s.MA20Vol <= boloc.VolToiThieu.Value)
                                        : predicate.And(s => s.MA20Vol < boloc.VolToiThieu.Value)
                    : predicate.And(s => s.MA20Vol > ma20vol)
                : boloc.VolToiThieu != null
                    ? boloc.VolToiThieu.Ope == SoSanhEnum.LonHon
                        ? predicate.And(s => splitStringCode.Contains(s._sc_) && s.MA20Vol > boloc.VolToiThieu.Value)
                        : boloc.VolToiThieu.Ope == SoSanhEnum.LonHonHoacBang
                            ? predicate.And(s => splitStringCode.Contains(s._sc_) && s.MA20Vol >= boloc.VolToiThieu.Value)
                            : boloc.VolToiThieu.Ope == SoSanhEnum.Bang
                                ? predicate.And(s => splitStringCode.Contains(s._sc_) && s.MA20Vol == boloc.VolToiThieu.Value)
                                : boloc.VolToiThieu.Ope == SoSanhEnum.NhoHonHoacBang
                                    ? predicate.And(s => splitStringCode.Contains(s._sc_) && s.MA20Vol <= boloc.VolToiThieu.Value)
                                    : predicate.And(s => splitStringCode.Contains(s._sc_) && s.MA20Vol < boloc.VolToiThieu.Value)
                    : predicate.And(s => splitStringCode.Contains(s._sc_) && s.MA20Vol > ma20vol);

            var symbols = await _context.StockSymbol.Where(predicate).ToListAsync();
            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var tuNgay = boloc.Ngay.AddDays(-200);
            var today = boloc.Ngay;


            var historiesStockCode = await _context.History
                .Where(ss => stockCodes.Contains(ss.StockSymbol)
                    && ss.Date <= today
                    && ss.Date >= tuNgay.AddDays(-20))
                .OrderByDescending(ss => ss.Date)
                .ToListAsync();

            var result1 = new List<string>();
            var NhậtKýMuaBán = new List<LearningRealDataModel>();

            //foreach (var filter in boloc.Filters)
            //{
            //    for (int s = 0; s < symbols.Count; s++)
            //{
            //    decimal dung = 0;
            //    decimal sai = 0;
            //    var symbol = symbols[s];
            //    var histories = historiesStockCode.Where(ss => ss.StockSymbol == symbol._sc_).ToList();
            //    var phienHumNay = histories.Where(h => h.Date <= today).First();
            //    var phienHumwa = histories.Where(h => h.Date < phienHumNay.Date).First();

            //    var lstBan = new List<Tuple<string, bool>>();

            //    foreach (var item in CongThuc.allCongThuc)
            //    {
            //        var dk = ThỏaĐiềuKiệnLọc(item, histories, phienHumNay);
            //        lstBan.Add(new Tuple<string, bool>(item.Name, dk));
            //    }

            //    if (!lstBan.Any(t => t.Item2)) continue;

            //    var giaMua = XacDinhGiaBan(histories, phienHumNay, lstBan);

            //    if (!giaMua.Item3.HasValue)
            //    {
            //        var stringCTMua = string.Join(",", lstBan.Where(ct => ct.Item2).Select(ct => ct.Item1).ToList());
            //        result1.Add($"{symbol._sc_} - {stringCTMua} - Mua phiên sau (hum nay {phienHumNay.Date.ToShortDateString()}) tại giá {giaMua.Item1} tới giá {giaMua.Item2} - Chưa đủ dữ liệu T3 để tính toán giá bán.");
            //        continue;
            //    }

            //    if (giaMua.Item3.HasValue && giaMua.Item3.Value < 0)
            //    {
            //        var ngayMua = histories.Where(h => h.Date > phienHumNay.Date).OrderBy(h => h.Date).First();
            //        var stringCTMua = string.Join(",", lstBan.Where(ct => ct.Item2).Select(ct => ct.Item1).ToList());
            //        result1.Add($"{symbol._sc_} - {stringCTMua} - Mua phiên sau (hum nay {phienHumNay.Date.ToShortDateString()}) tại giá {giaMua.Item1} tới giá {giaMua.Item2} - Nhưng thực tế giá thấp nhất ở {ngayMua.L}");
            //        continue;
            //    }

            //    XácDinhDiemBan(symbol, result1, ref dung, ref sai, histories, phienHumNay, giaMua.Item3.Value, lstBan);

            foreach (var item in stockCodes)
            {
                foreach (var filter in boloc.Filters)
                {
                    var histories = historiesStockCode
                        .Where(ss => ss.StockSymbol == item)
                        .ToList();

                    var firstDate = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date >= tuNgay);
                    if (firstDate == null || !firstDate.HadAllIndicators()) continue;

                    var phienKiemTra = histories.Where(h => h.Date <= today).First();
                    var phienHumwa = histories.Where(h => h.Date < phienKiemTra.Date).First();
                    if (histories.PropertySoSanhDuLieu(phienKiemTra.C, boloc.GiaToiThieu.Value, boloc.GiaToiThieu.Ope) == false) continue;

                    var result = ThỏaĐiềuKiệnLọc(filter, histories, phienKiemTra);
                    if (result)
                    {
                        /*
                         * Chạy về quá khứ kiểm tra dữ liệu đối với cùng pattern CÙNG MÃ        -> tỉ lệ đúng sai khi KN giá mua T3 / T5 / T7
                         *      Example: 80% bán ở T3 có lời, 20% còn bán ở T5 có lời
                         * Chạy về quá khứ kiểm tra dữ liệu đối với cùng pattern TẤT CẢ MÃ KHÁC -> tỉ lệ đúng sai khi KN giá mua
                         *     "ACL - 27-05-2022 - Giá 26.700,00",
                         */
                        if (boloc.ShowHistory)
                        {
                            var duLieuQuaKhu = await KiemTraTileDungSaiTheoPattern(phienKiemTra.StockSymbol, ngayBatDauKiemTraTiLeDungSai, phienKiemTra.Date, filter);
                            result1.AddRange(duLieuQuaKhu);
                            continue;
                        }

                        var giaMua = TimGiaMuaMongMuon(histories, phienKiemTra, new List<Tuple<string, bool>>() { new Tuple<string, bool>(filter.Name, false) });
                        if (!giaMua.Item3.HasValue)
                        {
                            result1.Add($"{item} - {filter.Name} - Mua phiên sau (hum nay {phienKiemTra.Date.ToShortDateString()}) tại giá {giaMua.Item1} tới giá {giaMua.Item2} - Chưa đủ dữ liệu T3 để tính toán giá bán.");
                            continue;
                        }

                        if (giaMua.Item3.HasValue && giaMua.Item3.Value < 0)
                        {
                            var ngayMua = histories.Where(h => h.Date > phienKiemTra.Date).OrderBy(h => h.Date).First();
                            result1.Add($"{item} - {filter.Name} - Mua phiên sau (hum nay {phienKiemTra.Date.ToShortDateString()}) tại giá {giaMua.Item1} tới giá {giaMua.Item2} - Nhưng thực tế giá thấp nhất ở {ngayMua.L}");
                            continue;
                        }
                    }
                }
            }

            //var folder = ConstantPath.Path;
            //var g = Guid.NewGuid();
            //var name = $@"{folder}{g}.xlsx";
            //NhậtKýMuaBán.ToDataTable().WriteToExcel(name);

            return result1;
        }

        private static bool ThỏaĐiềuKiệnLọc(LocCoPhieuFilterRequest filter, List<History> histories, History phienKiemTra, decimal giáMua = 0)
        {
            var phienHumwa = histories.OrderByDescending(h => h.Date).First(h => h.Date < phienKiemTra.Date);
            var phienHumKia = histories.OrderByDescending(h => h.Date).First(h => h.Date < phienHumwa.Date);

            var result = true;

            foreach (var compareItem in filter.Properties)
            {
                result = compareItem.PropertyChecking(histories, phienKiemTra);
                if (!result) break;
            }

            if (result && filter.NenBaoPhuDaoChieuTrungBinh != null)
            {
                if (filter.NenBaoPhuDaoChieuTrungBinh.Value && !phienKiemTra.IsNenDaoChieuTang(phienHumwa, 0.85M))
                    result = false;
                if (!filter.NenBaoPhuDaoChieuTrungBinh.Value && !phienKiemTra.IsNenDaoChieuGiam(phienHumwa))
                    result = false;
            }

            if (result && filter.NenBaoPhuDaoChieuManh != null)
            {
                if (filter.NenBaoPhuDaoChieuManh.Value && !phienKiemTra.IsNenDaoChieuTang(phienHumwa, 0.75M))
                    result = false;
                if (!filter.NenBaoPhuDaoChieuManh.Value && !phienKiemTra.IsNenDaoChieuGiam(phienHumwa, 0.75M))
                    result = false;
            }

            if (result && filter.RSIHumWa != null)
                result = histories.PropertyMongMuon(phienHumwa, "RSI", filter.RSIHumWa);

            if (result && filter.VolSoVoiVolMA20 != null)
                result = histories.PropertySoSanhDuLieu(phienKiemTra.V, phienKiemTra.VOL(histories, -20), filter.VolSoVoiVolMA20.Ope);
            if (result && filter.VolLonHonMA20LienTucTrongNPhien != null)
                result = histories.VolTrenMA20LienTucTrongNPhien(phienKiemTra, filter.VolLonHonMA20LienTucTrongNPhien.Value);
            if (result && filter.VolNhoHonMA20LienTucTrongNPhien != null)
                result = histories.VolDuoiMA20LienTucTrongNPhien(phienKiemTra, filter.VolNhoHonMA20LienTucTrongNPhien.Value);

            if (result && filter.GiaSoVoiDinhTrongVong40Ngay != null)
            {
                var time40NgayTruoc = histories.OrderByDescending(h => h.Date).Where(h => h.Date < phienKiemTra.Date).Take(40).ToList();
                var dinh40NgayTruoc = time40NgayTruoc.OrderByDescending(h => h.C).First();
                result = histories.PropertySoSanhDuLieu(phienKiemTra.C, dinh40NgayTruoc.C * filter.GiaSoVoiDinhTrongVong40Ngay.Value, filter.GiaSoVoiDinhTrongVong40Ngay.Ope);
            }

            if (result && filter.CachDayThapNhatCua40NgayTrongVongXNgay.HasValue)
            {
                var time40NgayTruoc = histories.OrderByDescending(h => h.Date).Where(h => h.Date < phienKiemTra.Date).Take(40).ToList();
                var day40NgayTruoc = time40NgayTruoc.OrderBy(h => h.C).First();
                var indexOfToday = histories.IndexOf(phienKiemTra);
                var indexOfDay = histories.IndexOf(day40NgayTruoc);

                result = indexOfToday - indexOfDay <= filter.CachDayThapNhatCua40NgayTrongVongXNgay.Value;
            }

            if (result && filter.ChieuDaiThanNenSoVoiRau != null)
            {
                result = histories.PropertySoSanhDuLieu(phienKiemTra.H - phienKiemTra.L, Math.Abs(phienKiemTra.C - phienKiemTra.O), filter.ChieuDaiThanNenSoVoiRau);
            }

            if (result && filter.KiVong != null && giáMua > 0)
            {
                var kiVongCanSoSanhData = giáMua;
                var dữLiệuKìVọng = kiVongCanSoSanhData * filter.KiVong.Result;

                var dữLiệuCủaKìVọngCóTồnTạiKhông = (decimal)phienKiemTra.L <= dữLiệuKìVọng && (decimal)phienKiemTra.H >= dữLiệuKìVọng;

                result = dữLiệuCủaKìVọngCóTồnTạiKhông;
            }

            if (result && filter.BatDay1.HasValue && filter.BatDay1.Value)
            {
                result = histories.BatDayDcChua(phienKiemTra);
            }

            if (result && filter.KiemTraGiaVsMACD.HasValue && filter.KiemTraGiaVsMACD.Value)
            {
                result = histories.TuongQuanGiuaGiaVaMACD(phienKiemTra);
            }

            if (result && filter.KiemTraTangManhTuDay.HasValue && filter.KiemTraTangManhTuDay.Value)
            {
                result = histories.KiemTraTangManhTuDay(phienKiemTra);
            }

            if (result && filter.FullMargin.HasValue && filter.FullMargin.Value)
            {
                result = histories.FullMargin(phienKiemTra);
            }

            return result;
        }

        private void TimThoiGianBan(string code, List<string> result1, ref decimal dung, ref decimal sai,
            List<History> histories,
            History phienHumNay,
            decimal giáĐặtMua,
            List<Tuple<string, bool>> ctMua)
        {
            var lstNgayCoTheBan = histories.Where(h => h.Date > phienHumNay.Date).OrderBy(h => h.Date).Skip(3).ToList();
            var stringCTMua = string.Join(",", ctMua.Where(ct => ct.Item2).Select(ct => ct.Item1).ToList());
            if (!lstNgayCoTheBan.Any())
            {
                //Dữ liệu chưa có đủ cho T3 - phải chờ
                result1.Add($"{code} - {stringCTMua} - Mua: {phienHumNay.Date.ToShortDateString()} tại giá {giáĐặtMua} - Chưa đủ dữ liệu T3");
            }
            else
            {
                var cóThểBán = false;
                for (int j = 0; j < lstNgayCoTheBan.Count; j++)
                {
                    var ngayBanGiaDinh = lstNgayCoTheBan[j];

                    /*
                     * Hôm nay là ngày có thể bán, phải cbi kịch bản để bán hoặc giữ tiếp
                     */

                    var lstBan = new List<Tuple<string, bool, bool>>();
                    var ban1 = ThỏaĐiềuKiệnLọc(CongThuc.CTB1A, histories, ngayBanGiaDinh, giáĐặtMua);
                    var ban2 = ThỏaĐiềuKiệnLọc(CongThuc.CTB1B, histories, ngayBanGiaDinh);
                    var ban31 = ThỏaĐiềuKiệnLọc(CongThuc.CTB1C1, histories, ngayBanGiaDinh);
                    var ban32 = ThỏaĐiềuKiệnLọc(CongThuc.CTB1C2, histories, ngayBanGiaDinh);
                    var ban4 = ThỏaĐiềuKiệnLọc(CongThuc.CTB1D, histories, ngayBanGiaDinh);
                    var ban5 = ThỏaĐiềuKiệnLọc(CongThuc.CTB1E, histories, ngayBanGiaDinh, giáĐặtMua);

                    lstBan.Add(new Tuple<string, bool, bool>("CTB1A", ban1, true));
                    lstBan.Add(new Tuple<string, bool, bool>("CTB1B", ban2, true));
                    lstBan.Add(new Tuple<string, bool, bool>("CTB1C1", ban31, false));
                    lstBan.Add(new Tuple<string, bool, bool>("CTB1C2", ban32, false));
                    lstBan.Add(new Tuple<string, bool, bool>("CTB1D", ban4, true));
                    lstBan.Add(new Tuple<string, bool, bool>("CTB1E", ban5, true));

                    if (lstBan.Any(t => t.Item2))
                    {
                        var giaBan = ban1
                            ? giáĐặtMua * CongThuc.CTB1A.KiVong.Result
                                : ban5
                                ? giáĐặtMua * CongThuc.CTB1E.KiVong.Result
                            : ngayBanGiaDinh.C;

                        var chenhLechGia = (decimal)giaBan / (decimal)giáĐặtMua;

                        var kiVongLoiToiThieuDeBan = 1.03M;
                        var agreeToSell = lstBan.Where(ct => ct.Item2).ToList();
                        if (!agreeToSell.Any(ct => ct.Item3) && chenhLechGia > 1 && chenhLechGia < kiVongLoiToiThieuDeBan) continue; //chưa đủ kì vọng, chờ tiếp
                        if (!agreeToSell.Any(ct => ct.Item3) && chenhLechGia < 1 && chenhLechGia > CongThuc.CTB1E.KiVong.Result) continue;    //chưa đủ kì vọng, chưa tới mức cắt lỗ, gồng tiếp

                        cóThểBán = true;
                        var ctBan = string.Join(",", lstBan.Where(ct => ct.Item2).Select(ct => ct.Item1).ToList());
                        var phanTramChenhLech = chenhLechGia > 1
                            ? (chenhLechGia - 1) * 100
                            : (1 - chenhLechGia) * 100;

                        var sentence = string.Empty;
                        if (chenhLechGia > 1)
                        {
                            sentence = "Lời";
                            dung += phanTramChenhLech;
                        }
                        else
                        {
                            sai += phanTramChenhLech;
                            sentence = "Lỗ";
                        }

                        result1.Add($"{code} - {stringCTMua} - {sentence} {phanTramChenhLech.ToString("N2")}% - Nhắc mua: {phienHumNay.Date.ToShortDateString()} ở {giáĐặtMua} - Bán - {ctBan} - {ngayBanGiaDinh.Date.ToShortDateString()} ở {giaBan}");
                        break;
                    }
                }

                if (!cóThểBán)
                {
                    result1.Add($"{code} - {stringCTMua} - Nhắc mua: {phienHumNay.Date.ToShortDateString()} tại giá {giáĐặtMua} - Chưa tìm được điểm bán");
                }
            }

        }

        private bool? BanTCongCoLai(string code, List<string> result1, List<History> histories,
            History phienHumNay,
            decimal giáĐặtMua,
            List<Tuple<string, bool>> ctMua, int TCong)
        {
            var lstNgayCoTheBan = histories.Where(h => h.Date > phienHumNay.Date).OrderBy(h => h.Date).Skip(3).ToList();
            var stringCTMua = string.Join(",", ctMua.Where(ct => ct.Item2).Select(ct => ct.Item1).ToList());
            if (!lstNgayCoTheBan.Any())
            {
                //Dữ liệu chưa có đủ cho T3 - phải chờ
                result1.Add($"{code} - {stringCTMua} - Mua: {phienHumNay.Date.ToShortDateString()} tại giá {giáĐặtMua} - Chưa đủ dữ liệu T3");
                return null;
            }
            else
            {
                var lstNgaySeBan = lstNgayCoTheBan.Take(TCong).ToList();

                var sentence = string.Empty;
                var ngayBanGiaDinh = lstNgaySeBan.FirstOrDefault(h => h.C > giáĐặtMua * 1.01M);
                if (ngayBanGiaDinh != null)
                {
                    sentence = "Lời";
                    result1.Add($"{code} - {stringCTMua} - {sentence} - Nhắc mua: {phienHumNay.Date.ToShortDateString()} ở {giáĐặtMua}");
                    return true;
                }
                else
                {
                    sentence = "Lỗ";
                    result1.Add($"{code} - {stringCTMua} - {sentence} - Nhắc mua: {phienHumNay.Date.ToShortDateString()} ở {giáĐặtMua}");
                    return false;
                }
            }
        }

        public async Task<List<Tuple<string, decimal, List<string>>>> CongThuc1(string code, DateTime ngay, DateTime ngayCuoi, int ma20vol, int MANhanh, int MACham, decimal percentProfit)
        {
            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");
            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > ma20vol).ToListAsync()
                : await _context.StockSymbol.Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > ma20vol && splitStringCode.Contains(s._sc_)).ToListAsync();
            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var historiesStockCode = await _context.History
                .Where(ss => stockCodes.Contains(ss.StockSymbol)
                    && ss.Date <= ngay.AddDays(10)
                    && ss.Date >= ngayCuoi.AddDays(-100))
                .OrderByDescending(ss => ss.Date)
                .ToListAsync();

            var tup = new List<Tuple<string, decimal, List<string>>>();

            Parallel.ForEach(symbols, (Action<StockSymbol>)(symbol =>
            {
                var result1 = new List<string>();
                decimal tong = 0;
                decimal dung = 0;
                decimal sai = 0;

                var histories = historiesStockCode
                                    .Where(ss => ss.StockSymbol == symbol._sc_)
                                    .OrderBy(h => h.Date)
                                    .ToList();
                var ngayBatDau = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date >= ngayCuoi);
                for (int i = histories.IndexOf(ngayBatDau); i < histories.Count; i++)
                {
                    ngayBatDau = histories[i];
                    if (ngayBatDau != null && ngayBatDau.HadAllIndicators())
                    {
                        break;
                    }
                }

                var ngayDungLai = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date >= ngay);
                if (ngayDungLai == null) //đang nhập ngày trong tương lai -> chuyển về hiện tại
                {
                    ngayDungLai = histories.OrderByDescending(h => h.Date).First();
                }
                var startedI = histories.IndexOf(ngayBatDau);
                var stoppedI = histories.IndexOf(ngayDungLai);

                for (int i = startedI; i < stoppedI; i++)
                {
                    var phienHumNay = histories[i];
                    var phienHumWa = histories.Where(h => h.Date < phienHumNay.Date).OrderByDescending(h => h.Date).First();
                    var phienHumKia = histories.Where(h => h.Date < phienHumWa.Date).OrderByDescending(h => h.Date).First();

                    //var dk1 = ThỏaĐiềuKiệnLọc(CT1A, histories, phienHumNay);
                    //var dk2 = ThỏaĐiềuKiệnLọc(CT1B, histories, phienHumNay);
                    //var dk3 = ThỏaĐiềuKiệnLọc(CT1C, histories, phienHumNay);

                    var lstBan = new List<Tuple<string, bool>>();
                    var dk1 = ThỏaĐiềuKiệnLọc(CongThuc.CT1A, histories, phienHumNay);
                    var dk2 = ThỏaĐiềuKiệnLọc(CongThuc.CT1B, histories, phienHumNay);
                    var dk3 = ThỏaĐiềuKiệnLọc(CongThuc.CT1C, histories, phienHumNay);
                    var dk4 = ThỏaĐiềuKiệnLọc((LocCoPhieuFilterRequest)CongThuc.CT3, histories, phienHumNay);

                    lstBan.Add(new Tuple<string, bool>("CT1A", dk1));
                    lstBan.Add(new Tuple<string, bool>("CT1B", dk2));
                    lstBan.Add(new Tuple<string, bool>("CT1C", dk3));
                    lstBan.Add(new Tuple<string, bool>("CT3", dk4));

                    if (!lstBan.Any(t => t.Item2)) continue;

                    var giaMua = TimGiaMuaMongMuon(histories, phienHumNay, lstBan);

                    if (!giaMua.Item3.HasValue)
                    {
                        var stringCTMua = string.Join(",", lstBan.Where(ct => ct.Item2).Select(ct => ct.Item1).ToList());
                        result1.Add($"{symbol._sc_} - {stringCTMua} - Mua: {phienHumNay.Date.ToShortDateString()} tại giá {giaMua.Item1} tới giá {giaMua.Item2} - Chưa đủ dữ liệu T3 để tính toán giá bán.");
                        continue;
                    }

                    if (giaMua.Item3.HasValue && giaMua.Item3.Value < 0)
                    {
                        var ngayMua = histories.Where(h => h.Date > phienHumNay.Date).OrderBy(h => h.Date).First();
                        var stringCTMua = string.Join(",", lstBan.Where(ct => ct.Item2).Select(ct => ct.Item1).ToList());
                        result1.Add($"{symbol._sc_} - {stringCTMua} - Mua: {phienHumNay.Date.ToShortDateString()} tại giá {giaMua.Item1} tới giá {giaMua.Item2} - Nhưng thực tế giá thấp nhất ở {ngayMua.L}");
                        continue;
                    }

                    TimThoiGianBan(symbol._sc_, result1, ref dung, ref sai, histories, phienHumNay, giaMua.Item3.Value, lstBan);
                }

                if (result1.Any())
                {
                    tong = dung + sai;
                    var winRate = tong == 0 ? 0 : Math.Round(dung / tong, 2);
                    tup.Add(new Tuple<string, decimal, List<string>>(symbol._sc_, winRate, result1));
                }
            }));

            tup = tup.OrderByDescending(t => t.Item2).ToList();

            return tup;
        }

        public async Task<List<Tuple<string, decimal, List<string>>>> CongThuc2(string code, DateTime ngay, DateTime ngayCuoi, int ma20vol, int MANhanh, int MACham, decimal percentProfit)
        {
            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");
            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > ma20vol).ToListAsync()
                : await _context.StockSymbol.Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > ma20vol && splitStringCode.Contains(s._sc_)).ToListAsync();
            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var historiesStockCode = await _context.History
                .Where(ss => stockCodes.Contains(ss.StockSymbol)
                    && ss.Date <= ngay.AddDays(10)
                    && ss.Date >= ngayCuoi.AddDays(-100))
                .OrderByDescending(ss => ss.Date)
                .ToListAsync();

            var tup = new List<Tuple<string, decimal, List<string>>>();

            Parallel.ForEach(symbols, symbol =>
            {
                var result1 = new List<string>();
                decimal dung = 0;
                decimal sai = 0;

                var histories = historiesStockCode
                    .Where(ss => ss.StockSymbol == symbol._sc_)
                    .OrderBy(h => h.Date)
                    .ToList();
                var ngayBatDau = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date >= ngayCuoi);
                for (int i = histories.IndexOf(ngayBatDau); i < histories.Count; i++)
                {
                    ngayBatDau = histories[i];
                    if (ngayBatDau != null && ngayBatDau.HadAllIndicators())
                    {
                        break;
                    }
                }

                var ngayDungLai = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date >= ngay);
                if (ngayDungLai == null) //đang nhập ngày trong tương lai -> chuyển về hiện tại
                {
                    ngayDungLai = histories.OrderByDescending(h => h.Date).First();
                }
                var startedI = histories.IndexOf(ngayBatDau);
                var stoppedI = histories.IndexOf(ngayDungLai);

                for (int i = startedI; i < stoppedI; i++)
                {
                    var phienHumNay = histories[i];
                    var phienHumWa = histories[i - 1];

                    //var thoaDK = ThỏaĐiềuKiệnLọc(CT2A, histories, phienHumNay)
                    //          || ThỏaĐiềuKiệnLọc(CT2B, histories, phienHumNay)
                    //          || ThỏaĐiềuKiệnLọc(CT2C, histories, phienHumNay);
                    //if (!thoaDK) continue;

                    var lstBan = new List<Tuple<string, bool>>();
                    //var dk5 = ThỏaĐiềuKiệnLọc(CongThuc.CT2A, histories, phienHumNay);
                    var dk6 = ThỏaĐiềuKiệnLọc(CongThuc.CT2B, histories, phienHumNay);
                    var dk7 = ThỏaĐiềuKiệnLọc(CongThuc.CT2C, histories, phienHumNay);
                    var dk8 = ThỏaĐiềuKiệnLọc(CongThuc.CT2D, histories, phienHumNay);
                    var dk9 = ThỏaĐiềuKiệnLọc(CongThuc.CT2E, histories, phienHumNay);
                    var dk10 = ThỏaĐiềuKiệnLọc(CongThuc.CT2F, histories, phienHumNay);


                    //lstBan.Add(new Tuple<string, bool>("CT2A", dk5));
                    lstBan.Add(new Tuple<string, bool>("CT2B", dk6));
                    lstBan.Add(new Tuple<string, bool>("CT2C", dk7));
                    lstBan.Add(new Tuple<string, bool>("CT2D", dk8));
                    lstBan.Add(new Tuple<string, bool>("CT2E", dk9));
                    lstBan.Add(new Tuple<string, bool>("CT2F", dk10));

                    if (!lstBan.Any(t => t.Item2)) continue;

                    var giaMua = TimGiaMuaMongMuon(histories, phienHumNay, lstBan);

                    if (!giaMua.Item3.HasValue)
                    {
                        var stringCTMua = string.Join(",", lstBan.Where(ct => ct.Item2).Select(ct => ct.Item1).ToList());
                        result1.Add($"{symbol._sc_} - {stringCTMua} - Mua: {phienHumNay.Date.ToShortDateString()} tại giá {giaMua.Item1} tới giá {giaMua.Item2} - Chưa đủ dữ liệu T3 để tính toán giá bán.");
                        continue;
                    }

                    if (giaMua.Item3.HasValue && giaMua.Item3.Value < 0)
                    {
                        var ngayMua = histories.Where(h => h.Date > phienHumNay.Date).OrderBy(h => h.Date).First();
                        var stringCTMua = string.Join(",", lstBan.Where(ct => ct.Item2).Select(ct => ct.Item1).ToList());
                        result1.Add($"{symbol._sc_} - {stringCTMua} - Mua: {phienHumNay.Date.ToShortDateString()} tại giá {giaMua.Item1} tới giá {giaMua.Item2} - Nhưng thực tế giá thấp nhất ở {ngayMua.L}");
                        continue;
                    }

                    TimThoiGianBan(symbol._sc_, result1, ref dung, ref sai, histories, phienHumNay, giaMua.Item3.Value, lstBan);
                }

                if (result1.Any())
                {
                    var tile = (dung + sai) == 0 ? 0 : Math.Round(dung / (dung + sai), 2);
                    tup.Add(new Tuple<string, decimal, List<string>>(symbol._sc_, tile, result1));
                }
            });

            tup = tup.OrderByDescending(t => t.Item2).ToList();

            return tup;
        }

        public async Task<List<Tuple<string, decimal, List<string>>>> CongThuc3(string code, DateTime ngay, DateTime ngayCuoi, int ma20vol, int MANhanh, int MACham, decimal percentProfit)
        {
            CongThuc.allCongThuc.Clear();
            CongThuc.allCongThuc.AddRange(new List<LocCoPhieuFilterRequest>() {
                       CongThuc.CT1A, CongThuc.CT1B, CongThuc.CT1C, CongThuc.CT3, CongThuc.CT1B2, CongThuc.CT1B3, CongThuc.CT2B,CongThuc.CT2C, CongThuc.CT2D,CongThuc.CT2E, CongThuc.CT2F}
            );

            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");
            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > ma20vol).ToListAsync()
                : await _context.StockSymbol.Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > ma20vol && splitStringCode.Contains(s._sc_)).ToListAsync();
            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var historiesStockCode = await _context.History
                .Where(ss => stockCodes.Contains(ss.StockSymbol)
                    && ss.Date <= ngay.AddDays(10)
                    && ss.Date >= ngayCuoi.AddDays(-100))
                .OrderByDescending(ss => ss.Date)
                .ToListAsync();

            var tup = new List<Tuple<string, decimal, List<string>>>();

            Parallel.ForEach(symbols, symbol =>
            {
                var result1 = new List<string>();
                decimal tong = 0;
                decimal dung = 0;
                decimal sai = 0;

                var histories = historiesStockCode
                                    .Where(ss => ss.StockSymbol == symbol._sc_)
                                    .OrderBy(h => h.Date)
                                    .ToList();
                var ngayBatDau = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date >= ngayCuoi);
                for (int i = histories.IndexOf(ngayBatDau); i < histories.Count; i++)
                {
                    ngayBatDau = histories[i];
                    if (ngayBatDau != null && ngayBatDau.HadAllIndicators())
                    {
                        break;
                    }
                }

                var ngayDungLai = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date >= ngay);
                if (ngayDungLai == null) //đang nhập ngày trong tương lai -> chuyển về hiện tại
                {
                    ngayDungLai = histories.OrderByDescending(h => h.Date).First();
                }

                var startedI = histories.IndexOf(ngayBatDau);
                var stoppedI = histories.IndexOf(ngayDungLai);

                for (int i = startedI; i < stoppedI; i++)
                {
                    var phienHumNay = histories[i];
                    var phienHumWa = histories.Where(h => h.Date < phienHumNay.Date).OrderByDescending(h => h.Date).First();

                    var lstBan = new List<Tuple<string, bool>>();



                    foreach (var congThuc in CongThuc.allCongThuc)
                    {
                        var dk = ThỏaĐiềuKiệnLọc(congThuc, histories, phienHumNay);
                        lstBan.Add(new Tuple<string, bool>(congThuc.Name, dk));
                    }

                    if (!lstBan.Any(t => t.Item2)) continue;

                    var giaMua = TimGiaMuaMongMuon(histories, phienHumNay, lstBan);

                    if (!giaMua.Item3.HasValue)
                    {
                        var stringCTMua = string.Join(",", lstBan.Where(ct => ct.Item2).Select(ct => ct.Item1).ToList());
                        result1.Add($"{symbol._sc_} - {stringCTMua} - Mua: {phienHumNay.Date.ToShortDateString()} tại giá {giaMua.Item1} tới giá {giaMua.Item2} - Chưa đủ dữ liệu T3 để tính toán giá bán.");
                        continue;
                    }

                    if (giaMua.Item3.HasValue && giaMua.Item3.Value < 0)
                    {
                        var ngayMua = histories.Where(h => h.Date > phienHumNay.Date).OrderBy(h => h.Date).First();
                        var stringCTMua = string.Join(",", lstBan.Where(ct => ct.Item2).Select(ct => ct.Item1).ToList());
                        result1.Add($"{symbol._sc_} - {stringCTMua} - Mua: {phienHumNay.Date.ToShortDateString()} tại giá {giaMua.Item1} tới giá {giaMua.Item2} - Nhưng thực tế giá thấp nhất ở {ngayMua.L}");
                        continue;
                    }

                    TimThoiGianBan(symbol._sc_, result1, ref dung, ref sai, histories, phienHumNay, giaMua.Item3.Value, lstBan);
                }

                if (result1.Any())
                {
                    //tong = dung + sai;
                    dung = dung - sai;
                    var winRate = dung;// tong == 0 ? 0 : Math.Round(dung / tong, 2);
                    tup.Add(new Tuple<string, decimal, List<string>>(symbol._sc_, winRate, result1));
                }
            });

            tup = tup.OrderByDescending(t => t.Item2).ToList();

            return tup;
        }

        /// <summary>
        /// Return min/max và thực tế (thực tế có thể null vì ví dụ ngày mai thì chưa có data)
        /// </summary>
        /// <param name="histories"></param>
        /// <param name="phienHumNay"></param>
        /// <param name="lstConditions"></param>
        /// <returns></returns>
        private Tuple<decimal, decimal, decimal?> TimGiaMuaMongMuon(List<History> histories, History phienHumNay, List<Tuple<string, bool>> lstConditions)
        {
            var giaMongDoiCuoiCung = 0M;
            var giaCaoNhatCuoiCung = 0M;
            decimal? giaTonTai = null;

            for (int i = 0; i < lstConditions.Count; i++)
            {
                var item = lstConditions[i];
                var giaMongDoi = 0M;
                var giaCaoNhat = 0M;
                switch (item.Item1)
                {
                    case "CT1A":
                    case "CT1A1":
                    case "CT1A2":
                    case "CT1A3A":
                    case "CT1A3B":
                    case "CT1A4":
                    case "CTKH":
                    case "CTNT1":
                        giaMongDoi = phienHumNay.O + (phienHumNay.NenTop - phienHumNay.NenBot) / 2;
                        giaCaoNhat = phienHumNay.C + (phienHumNay.NenTop - phienHumNay.NenBot) / 5;
                        break;
                    case "CT1B":
                    case "CT1B2":
                        giaMongDoi = phienHumNay.O + (phienHumNay.NenTop - phienHumNay.NenBot) / 2;
                        giaCaoNhat = phienHumNay.C * 1.02M;
                        break;
                    case "CT1B3":
                        giaMongDoi = phienHumNay.O + (phienHumNay.NenTop - phienHumNay.NenBot) / 2;
                        giaCaoNhat = phienHumNay.C + (phienHumNay.NenTop - phienHumNay.NenBot) / 5;
                        break;
                    case "CT1C":
                        giaMongDoi = phienHumNay.GiaMA05;
                        giaCaoNhat = phienHumNay.BandsMid;
                        break;
                    case "CT3":
                        giaMongDoi = phienHumNay.O + (phienHumNay.NenTop - phienHumNay.NenBot) / 2;
                        giaCaoNhat = phienHumNay.C + (phienHumNay.NenTop - phienHumNay.NenBot) / 5;
                        break;


                    case "CT2B":
                        giaMongDoi = phienHumNay.NenBot * 0.93M;
                        giaCaoNhat = phienHumNay.NenBot + (phienHumNay.NenTop - phienHumNay.NenBot) / 5;
                        break;
                    case "CT2C":
                        giaMongDoi = phienHumNay.NenBot + (phienHumNay.NenTop - phienHumNay.NenBot) / 2;
                        giaCaoNhat = phienHumNay.C + (phienHumNay.NenTop - phienHumNay.NenBot) / 5;
                        break;
                    case "CT2D":
                        giaMongDoi = phienHumNay.NenBot - (phienHumNay.NenTop - phienHumNay.NenBot) / 2;
                        giaCaoNhat = phienHumNay.NenBot + (phienHumNay.NenTop - phienHumNay.NenBot) / 5;
                        break;
                    case "CT2E":
                    case "CT2F":
                        giaMongDoi = phienHumNay.NenBot * 0.93M;
                        giaCaoNhat = phienHumNay.NenBot - (phienHumNay.NenTop - phienHumNay.NenBot) / 2;
                        break;
                    default:
                        throw new Exception($"There is no Giá Mong Đợi cho {item.Item1}");
                }

                if (i == 0)
                {
                    giaMongDoiCuoiCung = giaMongDoi;
                    giaCaoNhatCuoiCung = giaCaoNhat;
                }
                else
                {
                    if (giaMongDoi < giaMongDoiCuoiCung) giaMongDoiCuoiCung = giaMongDoi;
                    if (giaCaoNhat > giaCaoNhatCuoiCung) giaCaoNhatCuoiCung = giaCaoNhat;
                }
            }

            var ngayMua = histories.Where(h => h.Date > phienHumNay.Date).OrderBy(h => h.Date).FirstOrDefault();
            var cogiaMua = false;
            if (ngayMua != null)
            {
                giaTonTai = giaMongDoiCuoiCung;
                while (giaTonTai < giaCaoNhatCuoiCung)
                {
                    giaTonTai = giaTonTai + 0.01M;
                    if (ngayMua.L <= giaTonTai && ngayMua.H >= giaTonTai)
                    {
                        cogiaMua = true;
                        break;
                    }
                }
            }

            if (ngayMua != null && !cogiaMua)
                return new Tuple<decimal, decimal, decimal?>(giaMongDoiCuoiCung, giaCaoNhatCuoiCung, -1);
            else
                return new Tuple<decimal, decimal, decimal?>(giaMongDoiCuoiCung, giaCaoNhatCuoiCung, giaTonTai);
        }

        public async Task<List<Tuple<string, decimal, List<string>>>> CongThuc4(string code, DateTime ngay, DateTime ngayCuoi, int ma20vol, int MANhanh, int MACham, decimal percentProfit)
        {
            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");
            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > ma20vol).ToListAsync()
                : await _context.StockSymbol.Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > ma20vol && splitStringCode.Contains(s._sc_)).ToListAsync();
            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var historiesStockCode = await _context.History
                .Where(ss => stockCodes.Contains(ss.StockSymbol)
                    && ss.Date <= ngay.AddDays(10)
                    && ss.Date >= ngayCuoi.AddDays(-100))
                .OrderByDescending(ss => ss.Date)
                .ToListAsync();

            var tup = new List<Tuple<string, decimal, List<string>>>();

            Parallel.ForEach(symbols, symbol =>
            {
                var result1 = new List<string>();
                decimal dung = 0;
                decimal sai = 0;

                var histories = historiesStockCode
                    .Where(ss => ss.StockSymbol == symbol._sc_)
                    .OrderBy(h => h.Date)
                    .ToList();
                var ngayBatDau = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date >= ngayCuoi);
                for (int i = histories.IndexOf(ngayBatDau); i < histories.Count; i++)
                {
                    ngayBatDau = histories[i];
                    if (ngayBatDau != null && ngayBatDau.HadAllIndicators())
                    {
                        break;
                    }
                }

                var ngayDungLai = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date >= ngay);
                if (ngayDungLai == null) //đang nhập ngày trong tương lai -> chuyển về hiện tại
                {
                    ngayDungLai = histories.OrderByDescending(h => h.Date).First();
                }
                var startedI = histories.IndexOf(ngayBatDau);
                var stoppedI = histories.IndexOf(ngayDungLai);

                for (int i = startedI; i < stoppedI; i++)
                {
                    var phienHumNay = histories[i];
                    var phienHumWa = histories[i - 1];
                    var lstBan = new List<Tuple<string, bool>>();

                    var thoaDK = ThỏaĐiềuKiệnLọc(CongThuc.CTNT3, histories, phienHumNay);
                    if (!thoaDK) continue;

                    lstBan.Add(new Tuple<string, bool>("CTNT3", thoaDK));

                    var giaMua = TimGiaMuaMongMuon(histories, phienHumNay, lstBan);

                    if (!giaMua.Item3.HasValue)
                    {
                        var stringCTMua = string.Join(",", lstBan.Where(ct => ct.Item2).Select(ct => ct.Item1).ToList());
                        result1.Add($"{symbol._sc_} - {stringCTMua} - Mua phiên sau (hum nay {phienHumNay.Date.ToShortDateString()}) tại giá {giaMua.Item1} tới giá {giaMua.Item2} - Chưa đủ dữ liệu T3 để tính toán giá bán.");
                        continue;
                    }

                    if (giaMua.Item3.HasValue && giaMua.Item3.Value < 0)
                    {
                        var ngayMua = histories.Where(h => h.Date > phienHumNay.Date).OrderBy(h => h.Date).First();
                        var stringCTMua = string.Join(",", lstBan.Where(ct => ct.Item2).Select(ct => ct.Item1).ToList());
                        result1.Add($"{symbol._sc_} - {stringCTMua} - Mua phiên sau (hum nay {phienHumNay.Date.ToShortDateString()}) tại giá {giaMua.Item1} tới giá {giaMua.Item2} - Nhưng thực tế giá thấp nhất ở {ngayMua.L}");
                        continue;
                    }

                    TimThoiGianBan(symbol._sc_, result1, ref dung, ref sai, histories, phienHumNay, giaMua.Item3.Value, lstBan);
                }

                if (result1.Any())
                {
                    //tong = dung + sai;
                    dung = dung - sai;
                    var winRate = dung;// tong == 0 ? 0 : Math.Round(dung / tong, 2);
                    tup.Add(new Tuple<string, decimal, List<string>>(symbol._sc_, winRate, result1));
                }
            });

            tup = tup.OrderByDescending(t => t.Item2).ToList();

            return tup;
        }

        public async Task<List<Tuple<string, string, decimal, string>>> TimCongThucPhuHopVsCoPhieu(string code, DateTime ngay, DateTime ngayCuoiCungOQuaKhu, int ma20vol, int MANhanh, int MACham, decimal percentProfit)
        {
            CongThuc.allCongThuc.Clear();
            //CongThuc.allCongThuc.AddRange(new List<LocCoPhieuFilterRequest>() {
            //    CongThuc.CT1A, CongThuc.CT1B, CongThuc.CT1C, CongThuc.CT1B2, CongThuc.CT1B3,
            //    CongThuc.CT1A1, CongThuc.CT1A2, CongThuc.CT1A3A, CongThuc.CT1A3B, CongThuc.CT1A4,
            //    CongThuc.CT2B,CongThuc.CT2C, CongThuc.CT2D,CongThuc.CT2E, CongThuc.CT2F,
            //    CongThuc.CT3,
            //});

            var ct1KetHop = new LocCoPhieuFilterRequest("CTKH");
            ct1KetHop.PropertiesSoSanh.AddRange(CongThuc.CT1A.PropertiesSoSanh);
            ct1KetHop.PropertiesSoSanh.AddRange(CongThuc.CT1A1.PropertiesSoSanh);
            ct1KetHop.PropertiesSoSanh.AddRange(CongThuc.CT1A2.PropertiesSoSanh);
            //ct1KetHop.PropertiesSoSanh.AddRange(CongThuc.CT1A3A.PropertiesSoSanh);//bỏ 2 thằng này lời 1111, lỗ 863
            //ct1KetHop.PropertiesSoSanh.AddRange(CongThuc.CT1A3B.PropertiesSoSanh);//có 2 thằng này lời 254, lỗ 243
            ct1KetHop.PropertiesSoSanh.AddRange(CongThuc.CT1A4.PropertiesSoSanh);
            ct1KetHop.PropertiesSoSanh.AddRange(CongThuc.CT1A5.PropertiesSoSanh);
            //ct1KetHop.KiemTraTangManhTuDay = true;                                  //có thằng này vô 0 ra kq luôn
            ct1KetHop.KiemTraGiaVsMACD = true;

            //CongThuc.allCongThuc.AddRange(new List<LocCoPhieuFilterRequest>() {
            //    CongThuc.CT1A,
            //    CongThuc.CT1A1, CongThuc.CT1A2, CongThuc.CT1A3A, CongThuc.CT1A3B, CongThuc.CT1A4
            //});

            CongThuc.allCongThuc.AddRange(new List<LocCoPhieuFilterRequest>() {
                ct1KetHop
            });

            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");
            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > ma20vol).ToListAsync()
                : await _context.StockSymbol.Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > ma20vol && splitStringCode.Contains(s._sc_)).ToListAsync();
            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var historiesStockCode = await _context.History
                .Where(ss => stockCodes.Contains(ss.StockSymbol)
                    && ss.Date <= ngay.AddDays(10)
                    && ss.Date >= ngayCuoiCungOQuaKhu.AddDays(-100))
                .OrderByDescending(ss => ss.Date)
                .ToListAsync();

            var tup = new List<Tuple<string, string, decimal, string>>();

            Parallel.ForEach(symbols, symbol =>
            {
                var histories = historiesStockCode.Where(ss => ss.StockSymbol == symbol._sc_).OrderBy(h => h.Date).ToList();
                if (!histories.Any()) return;

                var ngayBatDauXetCongThuc = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date >= ngayCuoiCungOQuaKhu);
                for (int i = histories.IndexOf(ngayBatDauXetCongThuc); i < histories.Count; i++)
                {
                    ngayBatDauXetCongThuc = histories[i];
                    if (ngayBatDauXetCongThuc != null && ngayBatDauXetCongThuc.HadAllIndicators())
                    {
                        break;
                    }
                }

                var ngayDungLai = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date >= ngay);
                if (ngayDungLai == null) //đang nhập ngày trong tương lai -> dời về hiện tại vì tương lai chưa có dữ liệu
                {
                    ngayDungLai = histories.OrderByDescending(h => h.Date).First();
                }

                var startedI = histories.IndexOf(ngayBatDauXetCongThuc);
                var stoppedI = histories.IndexOf(ngayDungLai);

                foreach (var congThuc in CongThuc.allCongThuc)
                {
                    var explanation = new List<string>();
                    decimal dung = 0;
                    decimal sai = 0;

                    for (int i = startedI; i < stoppedI; i++)
                    {
                        var phienHumNay = histories[i];
                        var phienHumWa = histories.Where(h => h.Date < phienHumNay.Date).OrderByDescending(h => h.Date).First();

                        var lstBan = new List<Tuple<string, bool>>();

                        var dk = ThỏaĐiềuKiệnLọc(congThuc, histories, phienHumNay);
                        lstBan.Add(new Tuple<string, bool>(congThuc.Name, dk));


                        if (!lstBan.Any(t => t.Item2)) continue;

                        var giaMua = TimGiaMuaMongMuon(histories, phienHumNay, lstBan);

                        if (!giaMua.Item3.HasValue)
                        {
                            var stringCTMua = string.Join(",", lstBan.Where(ct => ct.Item2).Select(ct => ct.Item1).ToList());
                            explanation.Add($"{symbol._sc_} - {stringCTMua} - Mua: {phienHumNay.Date.ToShortDateString()} từ {giaMua.Item1} tới {giaMua.Item2} - Chưa đủ dữ liệu T3 để tính toán giá bán.");
                            continue;
                        }

                        if (giaMua.Item3.HasValue && giaMua.Item3.Value < 0)
                        {
                            var ngayMua = histories.Where(h => h.Date > phienHumNay.Date).OrderBy(h => h.Date).First();
                            var stringCTMua = string.Join(",", lstBan.Where(ct => ct.Item2).Select(ct => ct.Item1).ToList());

                            var dung1 = BanTCongCoLai(symbol._sc_, explanation, histories, phienHumNay, ngayMua.O, lstBan, 2);
                            if (!dung1.HasValue) continue;

                            var text = dung1.Value ? "Lời" : "Lỗ";
                            explanation.Add($"{symbol._sc_} - {stringCTMua} - Mua: {phienHumNay.Date.ToShortDateString()} từ {giaMua.Item1} tới {giaMua.Item2} - Nhưng thực tế giá thấp nhất ở {ngayMua.L} - Mua ATO {text}");
                            continue;
                        }

                        //TimThoiGianBan(symbol._sc_, explanation, ref dung, ref sai, histories, phienHumNay, giaMua.Item3.Value, lstBan);
                        var ctDung = BanTCongCoLai(symbol._sc_, explanation, histories, phienHumNay, giaMua.Item3.Value, lstBan, 2);

                        if (!ctDung.HasValue) continue;

                        if (ctDung.Value) dung++;
                        else sai++;
                    }

                    if (explanation.Any())
                    {
                        var tong = dung + sai;
                        var winRate = tong > 0 ? Math.Round(dung / tong, 2) : 0;
                        tup.Add(new Tuple<string, string, decimal, string>(symbol._sc_, congThuc.Name, winRate, string.Join(Environment.NewLine, explanation)));
                    }
                }
            });

            tup = tup.OrderByDescending(t => t.Item2).ToList();

            //var folder = ConstantPath.Path;
            //var g = Guid.NewGuid();
            //var name = $@"{folder}{g}.xlsx";
            //ToDataTable(tup).WriteToExcel(name);

            return tup;
        }


        public DataTable ToDataTable(List<Tuple<string, string, decimal, string>> dlist)
        {
            var json = JsonConvert.SerializeObject(dlist);
            DataTable dataTable = (DataTable)JsonConvert.DeserializeObject(json, (typeof(DataTable)));
            return dataTable;
        }


        public async Task<List<string>> CongThucLungTung()
        {
            var symbols = await _context.StockSymbol
                .Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > 500000)
                //.Where(h => h._sc_ == "VKC")
                .ToListAsync();
            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var historiesStockCode = await _context.History
                .Where(ss => stockCodes.Contains(ss.StockSymbol) && ss.Date >= new DateTime(2022, 1, 1))
                .OrderByDescending(ss => ss.Date)
                .ToListAsync();
            /* 1 CP 
             * 2 Nhom Nganh 
             * 3 Gia
             * 4 So sánh với Mây
             * 5 So sánh với MA 20
             * 6 So sánh với đáy T6
             * 7 manh/yeu
             */

            var tup = new List<Tuple<string, string, decimal, decimal, decimal, decimal, bool>>();
            var result1 = new List<string>();
            Parallel.ForEach(symbols, (Action<StockSymbol>)(symbol =>
            {
                var histories = historiesStockCode
                                    .Where(ss => ss.StockSymbol == symbol._sc_)
                                    .OrderBy(h => h.Date)
                                    .ToList();

                var dayT5 = histories.Where(h => h.Date >= new DateTime(2022, 5, 1) && h.Date <= new DateTime(2022, 6, 1)).OrderBy(h => h.L).First();
                var cpCoThungDayT5TrongT6 = histories.Any(h => h.Date >= new DateTime(2022, 6, 1) && h.NenBot <= dayT5.L);

                if (cpCoThungDayT5TrongT6)
                {
                    var dayT6 = histories.Where(h => h.Date >= new DateTime(2022, 6, 1) && h.NenBot <= dayT5.L).OrderBy(h => h.L).First().L;
                    var today = histories.OrderByDescending(h => h.Date).First();

                    var khangMay = today.IchimokuBot > today.NenTop
                        ? ((today.IchimokuBot / today.NenTop) - 1) * -1  //Đang dưới mây bot
                        : (today.NenTop / today.IchimokuBot) - 1;        //Đang trên mây bot

                    var ma20 = today.BandsMid / today.NenTop > 0
                        ? ((today.BandsMid / today.NenTop) - 1) * -1     //Đang dưới MA20
                        : (today.NenTop / today.IchimokuBot) - 1;        //Đang trên MA20

                    var soDayT6 = (today.L / dayT6) - 1;        //Đang trên MA20

                    var dinhT5 = histories.Where(h => h.Date >= dayT5.Date && h.Date <= new DateTime(2022, 6, 1)).OrderByDescending(h => h.H).First();
                    var tileChenhLenhTrongT5 = dinhT5.H / dayT5.L;
                    
                    if (tileChenhLenhTrongT5 >= 1.2M)
                        tup.Add(new Tuple<string, string, decimal, decimal, decimal, decimal, bool>(
                            symbol._sc_, symbol._in_, today.C, khangMay * 100, ma20 * 100, soDayT6 * 100, today.V > today.VOL(histories, -20)
                        ));

                }
            }));

            tup = tup
                .OrderBy(h => h.Item2)
                .ThenBy(h => h.Item7)
                .ThenBy(h => h.Item6)
                .ThenBy(h => h.Item5)                
                .ThenBy(h => h.Item4)
                .ThenBy(h => h.Item3).ThenBy(h => h.Item1).ToList();

            foreach (var item in tup)
            {
                var textMa20 = item.Item5 > 0 ? $"Trên MA 20 {Math.Abs(item.Item5).ToString("N2")}%" : $"Dưới MA 20 {Math.Abs(item.Item5).ToString("N2")}%";
                var textDay = item.Item6 == 1
                    ? $"Đang ở đáy T6"
                    : item.Item6 > 0 ? $"Trên Đáy {Math.Abs(item.Item6).ToString("N2")}%" : $"Dưới Đáy {Math.Abs(item.Item6).ToString("N2")}%";
                var textMay = item.Item4 > 0 ? $"Trên Mây {Math.Abs(item.Item4).ToString("N2")}%" : $"Dưới Mây {Math.Abs(item.Item4).ToString("N2")}%";

                //var textManhYeu = item.Item7 ? $"Trong T5 Mạnh từ đáy" : $"Trong T5 Yếu từ đáy";
                var volTo = item.Item7 ? $"Vol TO" : $"Vol Bé";

                result1.Add($"{item.Item2} - {volTo} - {textDay} - {textMa20} - {textMay} - {item.Item3} - {item.Item1}");
            }

            return result1;
        }

    }
}

/*
 * 
 
 */

/*
 * CEO:
 *  + 20/1/22:  vol tăng lớn >= vol của 3 cây giảm liên tiếp trước đó * 0.95
 *              + giá tăng, thân nến xanh dày
 *              + giá bot ~ giá sàn * 0,2%
 *              + RSI hướng lên
 *          ==> Mua vô, trong vòng t5 bán nếu lời >= 15%, hoặc lỗ >= 5%
 *  
 *  1 - rsi phân kì âm ?
 *  2 - MACD hướng lên trong N phiên
 *  3 - MACD hướng xuống trong N phiên
 */