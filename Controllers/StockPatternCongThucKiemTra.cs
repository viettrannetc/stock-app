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
                    var dk1 = ThỏaĐiềuKiệnLọc(CongThuc.CTKVJVC, histories, phienHumNay);
                    //var dk2 = ThỏaĐiềuKiệnLọc(CongThuc.CT1B, histories, phienHumNay);
                    //var dk3 = ThỏaĐiềuKiệnLọc(CongThuc.CT1C, histories, phienHumNay);
                    //var dk4 = ThỏaĐiềuKiệnLọc((LocCoPhieuFilterRequest)CongThuc.CT3, histories, phienHumNay);

                    lstBan.Add(new Tuple<string, bool>("CTKVJVC", dk1));
                    //lstBan.Add(new Tuple<string, bool>("CT1B", dk2));
                    //lstBan.Add(new Tuple<string, bool>("CT1C", dk3));
                    //lstBan.Add(new Tuple<string, bool>("CT3", dk4));

                    if (!lstBan.Any(t => t.Item2)) continue;

                    var giaMuaGoiY = phienHumNay.C;
                    var phienNgayMai = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date > phienHumNay.Date);
                    if (phienNgayMai != null)
                    {
                        var hasThisPrice = phienNgayMai.L <= giaMuaGoiY && phienNgayMai.H >= giaMuaGoiY;
                        if (!hasThisPrice)
                        {
                            result1.Add($"{symbol._sc_} - Nhắc mua {phienHumNay.Date.ToShortDateString()} tại giá {giaMuaGoiY} - Nhưng thực tế giá thấp nhất ở {phienNgayMai.L} - cao hơn {Math.Round(phienNgayMai.L / giaMuaGoiY, 2) - 1}%");
                            giaMuaGoiY = 0;
                            continue;
                        }
                    }

                    TimThoiGianBanTheoT(symbol._sc_, result1, ref dung, ref sai, histories, phienHumNay, giaMuaGoiY, lstBan, 10);
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
                    //var dk6 = ThỏaĐiềuKiệnLọc(CongThuc.CT2B, histories, phienHumNay);
                    var dk7 = ThỏaĐiềuKiệnLọc(CongThuc.CT2C, histories, phienHumNay);
                    //var dk8 = ThỏaĐiềuKiệnLọc(CongThuc.CT2D, histories, phienHumNay);
                    //var dk9 = ThỏaĐiềuKiệnLọc(CongThuc.CT2E, histories, phienHumNay);
                    //var dk10 = ThỏaĐiềuKiệnLọc(CongThuc.CT2F, histories, phienHumNay);


                    //lstBan.Add(new Tuple<string, bool>("CT2A", dk5));
                    //lstBan.Add(new Tuple<string, bool>("CT2B", dk6));
                    lstBan.Add(new Tuple<string, bool>("CT2C", dk7));
                    //lstBan.Add(new Tuple<string, bool>("CT2D", dk8));
                    //lstBan.Add(new Tuple<string, bool>("CT2E", dk9));
                    //lstBan.Add(new Tuple<string, bool>("CT2F", dk10));

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

        public async Task<List<Tuple<string, decimal, List<string>>>> CongThuc3(string code, DateTime ngay, DateTime ngayCuoi, int ma20vol, int MANhanh, int MACham)
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

            var historiesStockCodeByHour = await _context.HistoryHour
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

                var historiesByHour = historiesStockCodeByHour
                    .Where(ss => ss.StockSymbol == symbol._sc_)
                    .OrderBy(h => h.Date)
                    .ToList();


                var ngayBatDau = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date >= ngayCuoi);
                if (ngayBatDau == null) return;

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
                if (startedI == 0) return;

                for (int i = startedI; i <= stoppedI; i++)
                {
                    var phienHumNay = histories[i];
                    var phienHumWa = histories[i - 1];
                    var lstBan = new List<Tuple<string, bool>>();

                    var thoaDK = ThỏaĐiềuKiệnLọc(CongThuc.CT0A2, histories, phienHumNay);                           //TODO: CT ở đây
                    if (!thoaDK) continue;

                    lstBan.Add(new Tuple<string, bool>("CT0A2", thoaDK));

                    //CT13B
                    var giaMuaGoiY = phienHumNay.C * 6.5M;
                    var phienNgayMai = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date > phienHumNay.Date);
                    if (phienNgayMai != null)
                    {
                        giaMuaGoiY = phienNgayMai.O;
                        //var hasThisPrice = phienNgayMai.L <= giaMuaGoiY;// && phienNgayMai.H >= giaMuaGoiY;
                        //if (!hasThisPrice)
                        //{
                        //    result1.Add($"{symbol._sc_} - Nhắc mua {phienHumNay.Date.ToShortDateString()} tại giá {giaMuaGoiY} - Nhưng thực tế giá thấp nhất ở {phienNgayMai.L} - cao hơn {Math.Round(phienNgayMai.L / giaMuaGoiY, 2) - 1}%");
                        //    giaMuaGoiY = 0;
                        //    continue;
                        //}
                    }

                    //TimThoiGianBan(symbol._sc_, result1, ref dung, ref sai, histories, phienHumNay, giaMua.Item3.Value, lstBan);
                    TimThoiGianBanTheoT(symbol._sc_, result1, ref dung, ref sai, histories, phienHumNay, giaMuaGoiY, lstBan, 10);
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

        public async Task<List<Tuple<string, decimal, List<string>>>> CongThuc4(string code, DateTime ngay, DateTime ngayCuoi, int ma20vol, int MANhanh, int MACham)
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

            var historiesStockCodeByHour = await _context.HistoryHour
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

                var historiesByHour = historiesStockCodeByHour
                    .Where(ss => ss.StockSymbol == symbol._sc_)
                    .OrderBy(h => h.Date)
                    .ToList();


                var ngayBatDau = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date >= ngayCuoi);
                if (ngayBatDau == null) return;

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

                for (int i = startedI; i <= stoppedI; i++)
                {
                    var phienHumNay = histories[i];
                    var phienHumWa = histories[i - 1];
                    var lstBan = new List<Tuple<string, bool>>();

                    var thoaDK = ThỏaĐiềuKiệnLọc(CongThuc.CTNT2, histories, phienHumNay);                           //TODO: CT ở đây
                    if (!thoaDK) continue;

                    lstBan.Add(new Tuple<string, bool>("CTNT2", thoaDK));



                    //var ngayMua = histories.Where(h => h.Date > phienHumNay.Date).OrderBy(h => h.Date).FirstOrDefault();
                    //var giaMua = ngayMua == null
                    //   ? phienHumNay.C
                    //   : ngayMua.O;

                    ////CTNT1
                    //var giaMuaGoiY = histories.LaCayVuotMA20(phienHumNay)
                    //    ? phienHumNay.BandsMid
                    //    : phienHumNay.BandsMid * 1.01M;
                    //var phienNgayMai = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date > phienHumNay.Date);
                    //if (phienNgayMai != null)
                    //{
                    //    var hasThisPrice = phienNgayMai.L <= giaMuaGoiY && phienNgayMai.H >= giaMuaGoiY;
                    //    if (!hasThisPrice)
                    //    {
                    //        giaMuaGoiY = 0;                            continue;
                    //    }

                    //    var dataByHour = historiesByHour.OrderBy(h => h.Date).Where(h => h.Date > phienHumNay.Date.AddDays(1)).Take(5).ToList();
                    //    if (dataByHour[3].C > dataByHour[0].O && dataByHour[4].C > dataByHour[0].O)
                    //    { }
                    //    else
                    //    {
                    //        giaMuaGoiY = 0;                            continue;
                    //    }
                    //}

                    //CTNT2
                    var giaMuaGoiY = phienHumNay.BandsMid;
                    var phienNgayMai = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date > phienHumNay.Date);
                    if (phienNgayMai != null)
                    {
                        var hasThisPrice = phienNgayMai.L <= giaMuaGoiY && phienNgayMai.H >= giaMuaGoiY;
                        if (!hasThisPrice)
                        {
                            result1.Add($"{symbol._sc_} - Nhắc mua {phienHumNay.Date.ToShortDateString()} tại giá {giaMuaGoiY} - Nhưng thực tế giá thấp nhất ở {phienNgayMai.L} - cao hơn {Math.Round(phienNgayMai.L / giaMuaGoiY, 2) - 1}%");
                            giaMuaGoiY = 0;
                            continue;
                        }
                    }

                    ////CTNT4
                    //var phienBungNo = histories.TimPhienBungNoTrongNPhienTruoc(phienHumNay);
                    //var giaMuaGoiY = ((phienBungNo.L + phienBungNo.H) / 2) * 1.02M;
                    //var phienNgayMai = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date > phienHumNay.Date);
                    //if (phienNgayMai != null)
                    //{
                    //    var hasThisPrice = phienNgayMai.L <= giaMuaGoiY && phienNgayMai.H >= giaMuaGoiY;
                    //    if (!hasThisPrice)
                    //    {
                    //        result1.Add($"{symbol._sc_} - Phiên Bùng Nổ {phienBungNo.Date.ToShortDateString()} - Nhắc mua {phienHumNay.Date.ToShortDateString()} tại giá {giaMuaGoiY} - Nhưng thực tế giá thấp nhất ở {phienNgayMai.L} - cao hơn {Math.Round(phienNgayMai.L / giaMuaGoiY, 2) - 1}%");
                    //        giaMuaGoiY = 0;
                    //        continue;
                    //    }
                    //}

                    ////CT2D && CT2E
                    //var giaMuaGoiY = phienHumNay.L == phienHumNay.H && phienHumNay.V < phienHumNay.VOL(histories, -20) / 5
                    //    ? phienHumNay.L * 0.93M
                    //    : phienHumNay.L * 0.94M;
                    //var phienNgayMai = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date > phienHumNay.Date);
                    //if (phienNgayMai != null)
                    //{
                    //    var hasThisPrice = phienNgayMai.L <= giaMuaGoiY;// && phienNgayMai.H >= giaMuaGoiY;
                    //    if (!hasThisPrice)
                    //    {
                    //        result1.Add($"{symbol._sc_} - Nhắc mua {phienHumNay.Date.ToShortDateString()} tại giá {giaMuaGoiY} - Nhưng thực tế giá thấp nhất ở {phienNgayMai.L} - cao hơn {Math.Round(phienNgayMai.L / giaMuaGoiY, 2) - 1}%");
                    //        giaMuaGoiY = 0;
                    //        continue;
                    //    }
                    //}

                    ////CT13B
                    //var giaMuaGoiY = phienHumNay.L == phienHumNay.H && phienHumNay.V < phienHumNay.VOL(histories, -20) / 5
                    //    ? phienHumNay.L * 0.93M
                    //    : phienHumNay.L * 0.94M;
                    //var phienNgayMai = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date > phienHumNay.Date);
                    //if (phienNgayMai != null)
                    //{
                    //    var hasThisPrice = phienNgayMai.L <= giaMuaGoiY;// && phienNgayMai.H >= giaMuaGoiY;
                    //    if (!hasThisPrice)
                    //    {
                    //        result1.Add($"{symbol._sc_} - Nhắc mua {phienHumNay.Date.ToShortDateString()} tại giá {giaMuaGoiY} - Nhưng thực tế giá thấp nhất ở {phienNgayMai.L} - cao hơn {Math.Round(phienNgayMai.L / giaMuaGoiY, 2) - 1}%");
                    //        giaMuaGoiY = 0;
                    //        continue;
                    //    }
                    //}

                    ////CT13B
                    //var giaMuaGoiY = phienHumNay.C;
                    //var phienNgayMai = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date > phienHumNay.Date);
                    //if (phienNgayMai != null)
                    //{
                    //    var hasThisPrice = phienNgayMai.L <= giaMuaGoiY;// && phienNgayMai.H >= giaMuaGoiY;
                    //    if (!hasThisPrice)
                    //    {
                    //        result1.Add($"{symbol._sc_} - Nhắc mua {phienHumNay.Date.ToShortDateString()} tại giá {giaMuaGoiY} - Nhưng thực tế giá thấp nhất ở {phienNgayMai.L} - cao hơn {Math.Round(phienNgayMai.L / giaMuaGoiY, 2) - 1}%");
                    //        giaMuaGoiY = 0;
                    //        continue;
                    //    }
                    //}

                    //var giaMua = TimGiaMuaMongMuon(histories, phienHumNay, lstBan);

                    //if (!giaMua.Item3.HasValue)
                    //{
                    //    var stringCTMua = string.Join(",", lstBan.Where(ct => ct.Item2).Select(ct => ct.Item1).ToList());
                    //    //explanation.Add($"{symbol._sc_} - {stringCTMua} - Mua: {phienHumNay.Date.ToShortDateString()} từ {giaMua.Item1} tới {giaMua.Item2} - Chưa đủ dữ liệu T3 để tính toán giá bán.");
                    //    continue;
                    //}

                    //if (!giaMua.Item3.HasValue)
                    //{
                    //    var stringCTMua = string.Join(",", lstBan.Where(ct => ct.Item2).Select(ct => ct.Item1).ToList());
                    //    result1.Add($"{symbol._sc_} - {stringCTMua} - Mua phiên sau (hum nay {phienHumNay.Date.ToShortDateString()}) tại giá {giaMua.Item1} tới giá {giaMua.Item2} - Chưa đủ dữ liệu T3 để tính toán giá bán.");
                    //    continue;
                    //}

                    //if (giaMua.Item3.HasValue && giaMua.Item3.Value < 0)
                    //{
                    //    var ngayMua = histories.Where(h => h.Date > phienHumNay.Date).OrderBy(h => h.Date).First();
                    //    var stringCTMua = string.Join(",", lstBan.Where(ct => ct.Item2).Select(ct => ct.Item1).ToList());
                    //    result1.Add($"{symbol._sc_} - {stringCTMua} - Mua phiên sau (hum nay {phienHumNay.Date.ToShortDateString()}) tại giá {giaMua.Item1} tới giá {giaMua.Item2} - Nhưng thực tế giá thấp nhất ở {ngayMua.L}");
                    //    continue;
                    //}


                    //TimThoiGianBan(symbol._sc_, result1, ref dung, ref sai, histories, phienHumNay, giaMua.Item3.Value, lstBan);
                    TimThoiGianBanTheoT(symbol._sc_, result1, ref dung, ref sai, histories, phienHumNay, giaMuaGoiY, lstBan, 10);
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
        /// CT đúc kết từ trải nghiệm bản thân 2022
        /// </summary>
        /// <returns></returns>
        public async Task<List<Tuple<string, decimal, List<string>>>> CongThuc5(string code, DateTime ngay, DateTime ngayCuoi)
        {
            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");
            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > ConstantData.minMA20VolDaily).ToListAsync()
                : await _context.StockSymbol.Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > ConstantData.minMA20VolDaily && splitStringCode.Contains(s._sc_)).ToListAsync();
            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var historiesStockCode = await _context.History
                .Where(ss => stockCodes.Contains(ss.StockSymbol)
                    && ss.Date <= ngay.AddDays(10)
                    && ss.Date >= ngayCuoi.AddDays(-100))
                .OrderByDescending(ss => ss.Date)
                .ToListAsync();

            var historiesStockCodeByHour = await _context.HistoryHour
                .Where(ss => stockCodes.Contains(ss.StockSymbol)
                    && ss.Date <= ngay.AddDays(10)
                    && ss.Date >= ngayCuoi.AddDays(-100))
                .OrderByDescending(ss => ss.Date)
                .ToListAsync();

            var tup = new List<Tuple<string, decimal, List<string>>>();


            var allTradingHistory = new Dictionary<string, List<Tuple<DateTime, string>>>();
            foreach (var symbol in symbols)
            {
                decimal tienVon = 100000000;
                decimal tienDangMua = 0;
                var trend = EnumTrend.MuaBatDay;
                var tradingHistory = new List<Tuple<DateTime, string>>();

                var result1 = new List<string>();
                decimal dung = 0;
                decimal sai = 0;

                var histories = historiesStockCode
                    .Where(ss => ss.StockSymbol == symbol._sc_)
                    .OrderBy(h => h.Date)
                    .ToList();

                var historiesByHour = historiesStockCodeByHour
                    .Where(ss => ss.StockSymbol == symbol._sc_)
                    .OrderBy(h => h.Date)
                    .ToList();


                var ngayBatDau = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date >= ngayCuoi);
                if (ngayBatDau == null) continue;

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

                for (int i = startedI; i <= stoppedI; i++)
                {
                    var chuaMuaCP = tienDangMua == 0;
                    var pattern1 = true;
                    var pattern2 = true;
                    var pattern3 = true;

                    var phienHumNay = histories[i];
                    var phienHumWa = histories[i - 1];
                    var phienNgayMai = i < stoppedI ? histories[i + 1] : histories[i];

                    //var noteToday = new StringBuilder();
                    //if (chuaMuaCP == true)// (tienDangMua == 0 && ) //canh mua
                    //{
                    //    if (phienHumNay.XacNhanRSIPKDLanThu(3)) //pattern 1
                    //    {
                    //        if (phienHumNay.ID == phienNgayMai.ID)
                    //            noteToday.Append("Mua giá ATO ngày mai");
                    //        trend = EnumTrend.MuaBatDay;

                    //        var soCPDuRaSau100 = Math.Round((tienVon / phienNgayMai.O) % 100, 2);
                    //        var soCPSeMua = (tienVon / phienNgayMai.O) - soCPDuRaSau100;
                    //        tienDangMua = soCPSeMua * phienNgayMai.O;
                    //        tienVon = tienVon - tienDangMua;
                    //    }
                    //    else if (phienHumNay.XacNhanDangTichLuyTotTrenMA20CungVoiVNINDEXTrong(3)) //pattern 2
                    //    {
                    //        trend = EnumTrend.MuaTrongSideway;
                    //        var soCPDuRaSau100 = Math.Round((tienVon / phienNgayMai.O) % 100, 2);
                    //        var soCPSeMua = (tienVon / phienNgayMai.O) - soCPDuRaSau100;
                    //        tienDangMua = soCPSeMua * phienNgayMai.O;
                    //        tienVon = tienVon - tienDangMua;
                    //    }
                    //    else if (phienHumNay.XacNhanDauHieuSongHoiLanThuTrendGiam()) //pattern 2
                    //    {
                    //        trend = EnumTrend.MuaTrongSongHoi;
                    //        var soCPDuRaSau100 = Math.Round((tienVon / phienNgayMai.O) % 100, 2);
                    //        var soCPSeMua = (tienVon / phienNgayMai.O) - soCPDuRaSau100;
                    //        tienDangMua = soCPSeMua * phienNgayMai.O;
                    //        tienVon = tienVon - tienDangMua;
                    //    }
                    //    else
                    //    {
                    //        noteToday.Append("Không đủ DK mua");
                    //    }
                    //}
                    //else //canh bán
                    //{
                    //    if (phienHumNay.Lost(5))
                    //    {

                    //    }
                    //    else if (phienHumNay.XacNhanMACDPKADinhChartDLanThu(2))
                    //    {

                    //    }
                    //    else if (phienHumNay.XacNhanMACDPKADinhChartHLanThu(3))
                    //    {

                    //    }
                    //    else if (phienHumNay.XacNhanRSIPKADinhChartDLanThu(2))
                    //    {

                    //    }
                    //    else if (phienHumNay.XacNhanRSIPKADinhChartHLanThu(3))
                    //    {

                    //    }
                    //    else if (trend == EnumTrend.MuaTrongSongHoi && phienHumNay.LucMuaSuyYeu())
                    //    {

                    //    }
                    //    else
                    //    {
                    //        noteToday.Append("Không đủ DK bán");
                    //    }
                    //}

                    //tradingHistory.Add(new Tuple<DateTime, string>(phienHumNay.Date, noteToday.ToString()));







                }

                if (result1.Any())
                {
                    //tong = dung + sai;
                    dung = dung - sai;
                    var winRate = dung;// tong == 0 ? 0 : Math.Round(dung / tong, 2);
                    tup.Add(new Tuple<string, decimal, List<string>>(symbol._sc_, winRate, result1));
                }
            }

            tup = tup.OrderByDescending(t => t.Item2).ToList();

            return tup;
        }

    }
}