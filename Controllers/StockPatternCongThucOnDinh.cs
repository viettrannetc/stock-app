using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DotNetCoreSqlDb.Models;
using DotNetCoreSqlDb.Common;
using System.Text;
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
        private DateTime fireAntDataForMinuteFromDate = new DateTime(2023, 1, 17);
        private DateTime fireAntDataForHourFromDate = new DateTime(2022, 10, 17);
        private string fireAntCookies = "_gid=GA1.2.180306469.1674532778; _gat=1; _ga_ZJ4G3SW582=GS1.1.1674539348.87.0.1674539348.0.0.0; _ga=GA1.2.805353658.1649312480; FireAnt.Authentication.v3=5uCwF4NDvTmz-zZ-KbRmLjo7DlqALfKvltmim0hOl9MK0jM1IAN42oCKY_98Iusdz_5nHeOYl_mTndaFWEG1HiH5ClY-b5N1B4J8VsUWnQkJ2FyS8WNYUmZ3LUGLbzSBxgD2kFafFhXjsMWrFz6I4VifOAHB6DacXTG55NO0Q8RIqvLa3l7m4Y7xJFJYWW_Ft6meXpOBNbIMCfSrLDytm8Mn9KrPfj9lEnxYYoDE0h3J0mKmAFcI-5mlIR2szM0X6a64y4TQ2jkqnML8txhJmqB-xDJn4z-F7TAH4oLANF9wJw4-H-G-jX4R0GhrSHC5lMPwKDpRmT3kEYlPLYyBHloeTmUtuvYt0JrQd2pitJXBsh4uF3ImkwpcKyJ9hv39iZ9zrKKLMu3kh_acHZhBPn7Osq1F8MCwLFw0o3XnayS-jWZknBbHG4Oxv2QBEsxr68aza3jWZu6O3Q2zTtMzYJhDYTBMiACzC_GHU1y6hPKGpfmGD_PiCjbNlK1fiAGPMu2ojN9K5kpz-BMfXViTHzm7Fn9ZkYEy6psBaSVc_g9S0XSXV1NbvwfDlY3Krkz786pIM-ns7C-MpHLUk0UyWrvKZvChzVHHRwQ5ufk5F9aNcI9lCL5LRqsCtwe6jTK_UNoKbOc82qBOLd1Qk2kPnzXABCXLt44iSYpYzF5c57bxUsB0g39XRNEWUF2pGbfMp4boiBfE9aTH9h3elqBSw4o_dVbzupAs1nS0qg4Wx2jbOyHj5UAHpGKqUtkNRMADYXLBSlf24d4NK9Bj2AvfOMNljrXjYR8M13Zd0jqqDYbwVXo0C6XnmZG8icCubP-b6KY8psOtdBsl9UNrT_g7lfXN2r1--KDxJamT-dYYd0S9ap4XN8nNDVNTnOSEVD5boR-onHwNpajV33W09TfPpqxZr8jBYItdrHBq_0oGQeNlpDX0MUgclhJT8HlaYtEsVsZmx_6iPl1FSnaaae_TPQTJHVM7DwS5Mw5g7PhBkcTw-fYmZjwU7q8SiQ5VSXlDHE51gEGrPtFe7W15TZ_LWEsjGL842EOUan67lu3zP-HKPvR_jw4i-EOhzRf-tJkG9cUSTnirptsRD_P85kgnJx_ds_gjQtQuPuoQjveSu3iot6bLWN9tAcetZZp6ikrcIq9CltuRFR6la_EdNmiMkjdUiu79FVlGZ2Af6I6_W7FJxWu4Jd52rp8m4c4_l6YFKvJ8VVZPkyR-i8pH24DDoQC63eCaX7owI7Zxm9t_JxdtDheGT0vNnVr0n4AWkh2AbszAfCk0eRwdMGDVJ8PxrmGAAXsPo0Y0rtvPlwzfhsJVPV2a1f3wbyrQVBuHgZ_inOyQLQREWx4W0xSWUtpzynILpbx2Ilc21eorJSffF-VFR-Uxajarx1DmlHfGI7r1oFcsDcBEQ0V29QOf8TkLdprr12hToqrerq2yyPPUeLlbhaVTl8R0SzarQtwd5YmU2mUPvwAbuUjnfRK7g_2raYYgNXpbK3CK4jGboQYI8w62oVo2Htuf6TZ3khrs1haFaVh6KC4xDu0GGgfcwIgrmzeNOmAAx__IHRlcY0QDrBs9cemyx4WDOuTc0hFzLjDT_A27u37pxUbAIJor8UNkqBpraTfJR-BTSa9oKw0hIkq13HnyQrNM96vV2jffHOCQkteb7oHIbU2-hyIMTHFiblU3pxJ9StsY8xwrZDEzZp58YA8eX4KL06quWkO0Yx9I241HC7GVbLF06Ibtimzg2uE5QaJUbVIGQr6FfCPY93xnQ2-WCNbZ7xp6xewUstgM_miGRpe3AqN1nWx6pNg5TRDS2c3yCYTzSeYaQC2ZWdaSn7d_4rd0JrQrWqBR698ysX0i2QC1sPedWsacMr4mo3S1VzIPrIpHaEfS9gAzPKxrzBEKrhvHxk8llbqrePUH9F2zX7ro1LpZcjYzwHM5qhCwAzqev-qM4YKy15mL5U6nTwqIthiDGxk1jgQgNJeF6Rw8RTf7jdXmOkJ9eUbap3NIwTRdw6MoZHqvuGZ6PjBVs4z8o-bw8WwRGU8E-E9ELNrogdtMF985cimasB3LubKBCHXA6JQCD63CEbXUn6rt958URo9xShU09-lBkMlaCs7H9BuK4qXOQLRQyxuVWrZSPGQx3bgrEvPykYIdFyK37Kqgu2Elicun09iMDLZ2EaSQKj-uSsfz2xcLFXJaujEx2JfHkysNsrTPQTu9pySkt8UE_HKKnvpi0S2XfkFL3tL-MA0HN8xA1S07eXPrjNxdWaVyktwmNY0S7K_yJJuM3MSRbD5Vt0B5uLEZL_SHcGAwfW2Ae4FmFLcCZt_Hj5T2Tgcnxfd7XJqCWk7_9vwNuAMylg7eWC-oaOD4J_1Hqybl4kBvb95XrdFEKsNpnsqIMeR-HLswQd059cmfSlTSQsbeZCxc1ATRV42I5quU6xAd5PqCaD7AE0i-5R5tzzEbTN7iQNdedbpIF5OpkqRDPkBe4QOe2vGAFrmp1fQyzePjpf25FnQMOqO3OWN6V3-xzVCoYTT-kf-SwuXyioe-k3gFWD2TQVuRj25dTPHA_IaX0rBNAfqSll_GtuCX3CewlSy5RgResIflLLK0yeHYR8gP-fokjIH_C--O4AxVjgct6YdgixXL_Eu7jx3eHB20-Y_0eEYQ7TIC_2BsdxPfCKz0cPkLLbIx_Vjw-zvMl9BkhodXPI5K1M5VCJOWRSmjt2hmFVDS-cBIRV_NHtcsTN3wud3N4FDi5HiwmxWH_XD58VDiopkji3-XFq8TVx_XIuFnyz8OXNu8dGVVjE-XlPaxW4G-R1BZ1UiQ778ZRULDN1DNxQAmXhid3zoJVFEmx85wYJvzhJF7PQC97aGt0vGXWVLAqPQlW7PClFw9AqG8AyakSroNOBHKJySZKL_xLah4GK1Whi8o_5r4qTCkTIfoB1A3XwEIPr7QKUi-ZJ7YVhx0ILVxTfe2ANVm0cQ7h2Q; ASP.NET_SessionId=2j0yr34x30talkyewlt5h0yd";

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
            if (ngay == DateTime.MinValue) ngay = DateTime.Now;
            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");
            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.Where(s => !ConstantData.BadCodes.Contains(s._sc_) && s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > ConstantData.minMA20VolDaily).ToListAsync()
                : await _context.StockSymbol.Where(s => !ConstantData.BadCodes.Contains(s._sc_) && s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > ConstantData.minMA20VolDaily && splitStringCode.Contains(s._sc_)).ToListAsync();
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
                if (histories.IndexOf(ngayBatDau) <= 0) return;

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
                    var buyingDate = histories[i];
                    //Giả định ngày trước đó là đáy
                    var dayGiaDinh = histories[i - 1];
                    //hum nay RSI tăng so với hum wa, thì hum wa mới là đáy, còn ko thì mai RSI vẫn có thể giảm tiếp, ko ai bik

                    var propertyValueOfDayGiaDinh = (decimal)dayGiaDinh.GetPropValue(propertyName);
                    var propertyValueOfSuggestedDate = (decimal)buyingDate.GetPropValue(propertyName);
                    if (propertyValueOfDayGiaDinh == 0
                        || propertyValueOfSuggestedDate <= propertyValueOfDayGiaDinh)
                    //|| buyingDate.NenBot < dayGiaDinh.NenBot) -- chưa rõ tại sao có dk này
                    {
                        continue;
                    }

                    //Kiem tra đáy giả định: trong vòng 14 phiên trước không có cây nào trước đó thấp hơn nó
                    var nhungNgaySoSanhVoiDayGiaDinh = histories.OrderByDescending(h => h.Date).Where(h => h.Date < dayGiaDinh.Date).Take(SoPhienKT).ToList();
                    if (nhungNgaySoSanhVoiDayGiaDinh.Count < SoPhienKT) continue;

                    for (int j = KC2D; j < nhungNgaySoSanhVoiDayGiaDinh.Count - 1; j++)
                    {
                        var ngàyĐếmNgược = nhungNgaySoSanhVoiDayGiaDinh[j];

                        var propertyValueOfngàyĐếmNgược = (decimal)ngàyĐếmNgược.GetPropValue(propertyName);

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

        public async Task<List<string>> BoLocCoPhieu(string code, DateTime ngay)
        {
            var ngayBatDauKiemTraTiLeDungSai = new DateTime(2022, 1, 1);
            var boloc = new LocCoPhieuRequest(code, ngay, ConstantData.minMA20VolDaily);

            CongThuc.allCongThuc.Clear();
            CongThuc.allCongThuc.AddRange(new List<LocCoPhieuFilterRequest>() {
                CongThuc.CTNT1, CongThuc.CTNT2, CongThuc.CTNT4,
                CongThuc.CT0A2,
                CongThuc.CT2D, CongThuc.CT2E, CongThuc.CT2F,
                CongThuc.CT3TenKanVsKijun
            });

            var selectedCongthuc = CongThuc.allCongThuc.Where(ct => ct.Confirmed).ToList();
            boloc.Filters.AddRange(selectedCongthuc);

            var splitStringCode = string.IsNullOrWhiteSpace(boloc.Code) ? new string[0] : boloc.Code.Split(",");

            //TODO: validation
            if (!boloc.Filters.Any()) return new List<string>() { "Bộ lọc cổ phiếu trống rỗng" };

            var predicate = PredicateBuilder.New<StockSymbol>();
            predicate.And(s => !ConstantData.BadCodes.Contains(s._sc_) && s._sc_.Length == 3 && s.BiChanGiaoDich == false);

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
                    : predicate.And(s => s.MA20Vol > boloc.VolToiThieu.Value)
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
                    : predicate.And(s => splitStringCode.Contains(s._sc_) && s.MA20Vol > boloc.VolToiThieu.Value);

            var symbols = await _context.StockSymbol.Where(predicate)
                .OrderBy(s => s._in_)
                .ThenBy(s => s._sc_)
                .ToListAsync();
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

            foreach (var filter in boloc.Filters)
            {
                result1.Add(filter.Note);
            }

            foreach (var coPhieu in symbols)
            {
                foreach (var filter in boloc.Filters)
                {
                    var histories = historiesStockCode
                        .Where(ss => ss.StockSymbol == coPhieu._sc_)
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
                            result1.Add($"{coPhieu._sc_} - {filter.Name}  - {coPhieu._in_} - Nhắc mua từ {giaMua.Item1} tới {giaMua.Item2}");
                            continue;
                        }

                        if (giaMua.Item3.HasValue && giaMua.Item3.Value < 0)
                        {
                            var ngayMua = histories.Where(h => h.Date > phienKiemTra.Date).OrderBy(h => h.Date).First();
                            result1.Add($"{coPhieu} - {filter.Name} - Mua phiên sau (hum nay {phienKiemTra.Date.ToShortDateString()}) tại giá {giaMua.Item1} tới giá {giaMua.Item2} - Nhưng thực tế giá thấp nhất ở {ngayMua.L}");
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

            if (result && filter.RSIAmTheoNgay.HasValue && filter.RSIAmTheoNgay.Value)
            {
                result = histories.RSIAmTheoNgay(phienKiemTra);
            }

            if (result && filter.CTNT1)
            {
                result = histories.CTNT1(phienKiemTra);
            }

            if (result && filter.CTNT2)
            {
                result = histories.CTNT2(phienKiemTra);
            }

            if (result && filter.PhienBungNo)
            {
                result = histories.CoXuatHienPhienBungNoTrongNPhienTruoc(phienKiemTra);
            }

            if (result && filter.BienDoBandsHep)
            {
                result = histories.BienDoBands10PhanTram(phienKiemTra);
            }

            if (result && filter.BienDoBandsHep)
            {
                result = histories.NenGiamSatHoacNgoaiBandsBot(phienKiemTra);
            }

            if (result && filter.DangTrendTang)
            {
                result = histories.DangTrendTang(phienKiemTra);
            }

            if (result && filter.DangCoGame)
            {
                result = histories.DangCoGame(phienKiemTra);
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

        private void TimThoiGianBanTheoT(string code, List<string> result1, ref decimal dung, ref decimal sai,
            List<History> histories,
            History phienHumNay,
            decimal giáĐặtMua,
            List<Tuple<string, bool>> ctMua,
            int TBan)
        {
            var kiVongLoiToiThieuDeBan = 1.03M;
            var lstNgayCoTheBan = histories.Where(h => h.Date > phienHumNay.Date).OrderBy(h => h.Date).Skip(3).Take(TBan - 3).ToList();
            var stringCTMua = string.Join(",", ctMua.Where(ct => ct.Item2).Select(ct => ct.Item1).ToList());
            if (!lstNgayCoTheBan.Any())
            {
                //Dữ liệu chưa có đủ cho T3 - phải chờ
                result1.Add($"{code} - {stringCTMua} - Mua: {phienHumNay.Date.ToShortDateString()} tại giá {giáĐặtMua} - Chưa đủ dữ liệu T3");
            }
            else
            {
                var note = new StringBuilder();
                if (phienHumNay.DangTrongMay(histories) || phienHumNay.DangNamDuoiMayFlat(histories))
                {
                    note.Append(" - Đang gặp mây xấu.");
                }
                if (phienHumNay.CoXuatHienMACDCatXuongSignalTrongXPhienGanNhat(histories, 3))
                {
                    note.Append(" - MACD đã cắt xuống Signal.");
                }

                var cóThểBán = false;
                var sentence = new StringBuilder();
                for (int j = 0; j < lstNgayCoTheBan.Count; j++)
                {
                    var ngayBanGiaDinh = lstNgayCoTheBan[j];

                    /*
                     * Hôm nay là ngày có thể bán, phải cbi kịch bản để bán hoặc giữ tiếp
                     */

                    if (ngayBanGiaDinh.C > giáĐặtMua * kiVongLoiToiThieuDeBan)
                    {
                        var giaBan = ngayBanGiaDinh.C;
                        cóThểBán = true;
                        sentence.Append("Lời");
                        dung++;

                        result1.Add($"{code} - {sentence} - Nhắc mua: {phienHumNay.Date.ToShortDateString()} ở {giáĐặtMua} - Bán - {ngayBanGiaDinh.Date.ToShortDateString()} ở {giaBan} {note}");
                        break;
                    }
                }

                if (!cóThểBán)
                {
                    //result1.Add($"{code} - {stringCTMua} - Nhắc mua: {phienHumNay.Date.ToShortDateString()} tại giá {giáĐặtMua} - Chưa tìm được điểm bán");
                    sai++;
                    sentence.Append("Lỗ");
                    result1.Add($"{code} - {sentence} - Nhắc mua: {phienHumNay.Date.ToShortDateString()} ở {giáĐặtMua} {note}");
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

        public DataTable ToDataTable(List<Tuple<string, string, decimal, string>> dlist)
        {
            var json = JsonConvert.SerializeObject(dlist);
            DataTable dataTable = (DataTable)JsonConvert.DeserializeObject(json, (typeof(DataTable)));
            return dataTable;
        }


        public async Task<List<Tuple<string, decimal, decimal, List<Tuple<decimal, decimal, string>>>>> TimVolMin(string code, int t = 10, decimal tileWin = 0.9M, int ma20vol = 800000)
        {
            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");
            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.Where(s => !ConstantData.BadCodes.Contains(s._sc_) && s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > ma20vol).ToListAsync()
                : await _context.StockSymbol.Where(s => !ConstantData.BadCodes.Contains(s._sc_) && s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > ma20vol && splitStringCode.Contains(s._sc_)).ToListAsync();

            var stockCodes = symbols.Select(s => s._sc_).ToList();
            var ngayCuoi = new DateTime(2022, 1, 1);
            DateTime ngay = new DateTime(2022, 12, 31);

            var historiesStockCode = await _context.History
                .Where(ss => stockCodes.Contains(ss.StockSymbol) && ss.Date >= ngayCuoi.AddDays(-20))
                .OrderByDescending(ss => ss.Date)
                .ToListAsync();

            //Mã - max win rate - vol - [vol - win rate - dates]
            var totalTup = new List<Tuple<string, decimal, decimal, List<Tuple<decimal, decimal, string>>>>();
            var result1 = new List<string>();
            Parallel.ForEach(symbols, (Action<StockSymbol>)(symbol =>
            {
                var histories = historiesStockCode
                                .Where(ss => ss.StockSymbol == symbol._sc_)
                                .OrderBy(s => s.Date)
                                .ToList();

                var checkingHistories = histories.Where(h => h.DojiCoRauHoacTangGia()).ToList();
                var volcaoNhat = checkingHistories.Sum(h => h.V) / 2;
                var volthapNhat = checkingHistories.Where(h => h.V > 0).OrderBy(h => h.V).First().V;

                var volRange = checkingHistories.Where(h => h.V <= volcaoNhat && h.V >= volthapNhat).OrderByDescending(h => h.V).ToList();
                var tupDetails = new List<Tuple<decimal, decimal, string>>();
                foreach (var item in volRange)
                {
                    var tup = new List<Tuple<string, decimal, List<string>>>();
                    var result1 = new List<string>();
                    decimal tong = 0;
                    decimal dung = 0;
                    decimal sai = 0;

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

                        var dk1 = phienHumNay.V <= item.V;
                        var dk2 = phienHumNay.DojiCoRauHoacTangGia();
                        if (!dk1 || !dk2) continue;

                        var lstBan = new List<Tuple<string, bool>>();
                        lstBan.Add(new Tuple<string, bool>("CTKVJVC", dk1 && dk2)); //không wan trọng

                        var giaMuaGoiY = phienHumNay.C;
                        var phienNgayMai = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date > phienHumNay.Date);
                        if (phienNgayMai != null)
                        {
                            var hasThisPrice = phienNgayMai.L <= giaMuaGoiY && phienNgayMai.H >= giaMuaGoiY;
                            if (!hasThisPrice)
                            {
                                giaMuaGoiY = phienNgayMai.O;
                            }
                        }

                        TimThoiGianBanTheoT(symbol._sc_, result1, ref dung, ref sai, histories, phienHumNay, giaMuaGoiY, lstBan, t);
                    }

                    if (result1.Any())
                    {
                        tong = dung + sai;
                        var winRate = tong == 0 ? 0 : Math.Round(dung / tong, 2);
                        if (winRate >= tileWin)
                        {
                            tupDetails.Add(new Tuple<decimal, decimal, string>(item.V, winRate, string.Join(",", result1)));
                        }

                        if (winRate == 1) break;
                    }
                }

                if (tupDetails.Any())
                {
                    var maxWinRate = tupDetails.OrderByDescending(t => t.Item2).First();
                    totalTup.Add(new Tuple<string, decimal, decimal, List<Tuple<decimal, decimal, string>>>(symbol._sc_, maxWinRate.Item2, maxWinRate.Item1, tupDetails));
                }
            }));

            totalTup = totalTup.OrderByDescending(t => t.Item2).ThenByDescending(t => t.Item3).ThenBy(t => t.Item1).ToList();

            var json = JsonConvert.SerializeObject(totalTup);
            DataTable dataTable = (DataTable)JsonConvert.DeserializeObject(json, (typeof(DataTable)));

            var folder = ConstantPath.Path;
            var g = DateTime.Now.ToString("yyyy-MM-dd HH-mm");
            var name = $@"{folder}{g}.xlsx";
            dataTable.WriteToExcel(name);

            return totalTup;
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
            var ngayMua = histories.Where(h => h.Date > phienHumNay.Date).OrderBy(h => h.Date).FirstOrDefault();

            for (int i = 0; i < lstConditions.Count; i++)
            {
                var item = lstConditions[i];
                var giaMongDoi = 0M;
                var giaCaoNhat = 0M;
                switch (item.Item1)
                {
                    case "CT3TenKanVsKijun":
                        giaMongDoi = phienHumNay.C;
                        giaCaoNhat = phienHumNay.C;
                        break;
                    case "CT1A":
                    case "CT1A1":
                    case "CT1A2":
                    case "CT1A3A":
                    case "CT1A3B":
                    case "CT1A4":
                    case "CT1A5":
                    case "CTKH":
                    case "CTNT2A":
                    case "CTNT2B":
                    case "CTRSI1":
                        giaMongDoi = phienHumNay.NenBot + (phienHumNay.NenTop - phienHumNay.NenBot) / 2;
                        giaCaoNhat = phienHumNay.NenTop + (phienHumNay.NenTop - phienHumNay.NenBot) / 5;
                        break;
                    case "CTNT2":
                        giaMongDoi = Math.Min(phienHumNay.BandsMid + 1.01M, (phienHumNay.H + phienHumNay.L) / 2);
                        giaCaoNhat = Math.Max(phienHumNay.BandsMid + 1.01M, (phienHumNay.H + phienHumNay.L) / 2);
                        break;
                    case "CTNT3":
                        giaMongDoi = phienHumNay.BandsMid;
                        giaCaoNhat = phienHumNay.BandsMid * 1.02M;
                        break;
                    case "CT1B":
                    case "CT1B2":
                        giaMongDoi = phienHumNay.O + (phienHumNay.NenTop - phienHumNay.NenBot) / 2;
                        giaCaoNhat = phienHumNay.C * 1.02M;
                        break;
                    case "CT1B3":
                    case "CT0A":
                        /* SAI: MBG 19/1/22
                         * 
                         * Giá mua cho CT này nên đợi tín hiệu từ phiên ngày hum sau - nếu ko bị đạp từ lúc 9h thì sẽ tiến hành mua, còn nếu thấy đạp mạnh thì bỏ
                         * VD: IDJ, MHC 13/4/22
                         * DIG - nến đỏ phiên 9 và 10h - 18/4/22
                         * ROS - 8/6/22
                         * BII - 1/4/22 - phiên 10h ngày 4/4/22 mới xác nhận nến đỏ
                         * MBG - 13/4/22- phien 13h ngày 14/4/22 mới xác nhận nến đỏ
                         * 
                         * Chú ý: 
                         *  - nếu thị trường xuất hiện nhiều mã tạo nến tăng đảo chiều sau nhiều phiên, thì hãy nhìn VNINDEX - nếu VNINDEX có MACD cắt xuống SIGNAL trong 3 phiên gần nhất thì hiện tại chúng ta sẽ phủ định cây nến đảo chiều này
                         *      Ví dụ: 13/04/22
                         */
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
                    case "CTKVJVC":
                        giaMongDoi = ngayMua != null ? ngayMua.O : phienHumNay.C * 0.98M;
                        //phienHumNay.O + (phienHumNay.NenTop - phienHumNay.NenBot) / 2;
                        giaCaoNhat = ngayMua != null ? ngayMua.O : phienHumNay.C;// phienHumNay.C + (phienHumNay.NenTop - phienHumNay.NenBot) / 5;
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
                    //giaMongDoi = phienHumNay.NenBot - (phienHumNay.NenTop - phienHumNay.NenBot) / 2;
                    //giaCaoNhat = phienHumNay.NenBot + (phienHumNay.NenTop - phienHumNay.NenBot) / 5;
                    //break;
                    case "CT2E":
                    case "CT2F":
                    case "CT2G":
                        giaMongDoi = phienHumNay.L * 0.93M;
                        giaCaoNhat = phienHumNay.L * 0.94M;
                        break;
                    case "CTNT1":
                        var cayVuotMA20 = histories.LaCayVuotMA20(phienHumNay);
                        if (cayVuotMA20)
                        {
                            giaMongDoi = phienHumNay.BandsMid;
                            giaCaoNhat = phienHumNay.BandsMid * 1.01M;
                        }
                        break;
                    case "CTNT4":
                        var phienBungNo = histories.TimPhienBungNoTrongNPhienTruoc(phienHumNay);
                        if (phienBungNo != null)
                        {
                            giaMongDoi = ((phienBungNo.L + phienBungNo.H) / 2);
                            giaCaoNhat = ((phienBungNo.L + phienBungNo.H) / 2) * 1.02M;
                        }
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


            var cogiaMua = false;
            if (ngayMua != null)
            {
                giaTonTai = giaMongDoiCuoiCung;
                while (giaTonTai <= giaCaoNhatCuoiCung)
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
                ? await _context.StockSymbol.Where(s => !ConstantData.BadCodes.Contains(s._sc_) && s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > ma20vol).ToListAsync()
                : await _context.StockSymbol.Where(s => !ConstantData.BadCodes.Contains(s._sc_) && s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > ma20vol && splitStringCode.Contains(s._sc_)).ToListAsync();
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

        public IEnumerable<Quote> MACDConvertHistory(List<HistoryHour> histories)
        {
            var qoutes = new List<Quote>();
            foreach (var h in histories)
            {
                qoutes.Add(new Quote
                {
                    Date = h.Date,
                    Close = h.C,
                    High = h.H,
                    Low = h.L,
                    Open = h.O,
                    Volume = h.V,
                });
            }

            return qoutes;
        }

        /// <summary>
        /// Khuyến nghị chạy 2p 1 lần
        /// 1 - Giá vượt MA 20 chart H
        /// 2 - Giá vòng về test Fibo 38.2
        /// 3 - Giá vòng về test Fibo 38.2 chart D
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task<List<string>> MonitoringEveryMinutes()
        {
            var restService = new RestServiceHelper();
            //string code = string.Empty;

            string DangMua = "BSR,HCM";
            string DauKhi = "PVD,PVS,";
            string LuongThuc = "HAG,DBC,PAN,TAR,";
            string NganHang = "BID,HDB,MBB,";
            string ChungKhoan = "HCM,SSI,VND,APS,CTS,";
            string BDS = "LDG,IDJ,CEO,DIG,";
            string DauTuCong = "HBC,HUT,VCG,VPG,";
            string VanChuyen = "PVT,GMD,VOS,";
            string ThuySan = "ANV,IDI,";
            string BanLe = "MSN,FRT,DGW";
            string PhanBon = "DGC,DPM,DCM,";
            string Thep = "HPG,NKG,HSG";

            StringBuilder strCode = new StringBuilder();
            strCode.Append(DangMua);
            strCode.Append(DauKhi);
            strCode.Append(LuongThuc);
            strCode.Append(NganHang);
            strCode.Append(ChungKhoan);
            strCode.Append(BDS);
            strCode.Append(DauTuCong);
            strCode.Append(VanChuyen);
            strCode.Append(ThuySan);
            strCode.Append(BanLe);
            strCode.Append(PhanBon);
            strCode.Append(Thep);

            //strCode.Append("APH,DBC,HCM,PAS,PVD");

            var code = strCode.ToString();

            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new List<string>() : code.Split(",").OrderBy(t => t).Distinct().ToList();
            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.Where(s => !ConstantData.BadCodes.Contains(s._sc_) && s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > ConstantData.minMA20VolDaily).OrderByDescending(s => s._sc_).ToListAsync()
                : await _context.StockSymbol.Where(s => !ConstantData.BadCodes.Contains(s._sc_) && s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > ConstantData.minMA20VolDaily && splitStringCode.Contains(s._sc_)).OrderByDescending(s => s._sc_).ToListAsync();
            var selectedCodes = symbols.Select(s => s._sc_).Distinct().ToList();
            var historiesHourly = new List<HistoryHour>();
            var historiesMinutes = new List<HistoryHour>();
            var result = new List<string>();
            var service = new Service();

            await service.GetVHoursFireAnt(historiesHourly, selectedCodes, fireAntDataForHourFromDate, fireAntCookies);
            var codes = await service.GetDataMinutesFireAnt(selectedCodes, fireAntDataForMinuteFromDate, fireAntCookies);

            result.AddRange(codes);

            //foreach (var symbol in splitStringCode)
            //{
            //    //Update indicators - Done
            //    var historiesInPeriodOfTime = historiesHourly
            //        .Where(ss => ss.StockSymbol == symbol)
            //        .OrderBy(h => h.Date)
            //        .ToList();

            //    if (!historiesInPeriodOfTime.Any()) continue;

            //    var qoutes = MACDConvertHistory(historiesInPeriodOfTime);

            //    //var ichimoku = qoutes.GetIchimoku();
            //    //var macd = qoutes.GetMacd();
            //    //var rsis = qoutes.GetRsi();
            //    var bands = qoutes.GetBollingerBands();
            //    //var ma5 = qoutes.GetSma(5);


            //    var test = new List<HistoryHour>();
            //    for (int i = 0; i < historiesInPeriodOfTime.Count; i++)
            //    {
            //        try
            //        {
            //            //if (historiesInPeriodOfTime[i].HadAllIndicators()) continue;

            //            //var sameDateMA5 = ma5.Where(r => r.Date == historiesInPeriodOfTime[i].Date).FirstOrDefault();
            //            //if (sameDateMA5 != null && !historiesInPeriodOfTime[i].HadMA5())
            //            //{
            //            //    historiesInPeriodOfTime[i].GiaMA05 = sameDateMA5.Sma.HasValue ? (decimal)sameDateMA5.Sma.Value : 0;
            //            //}

            //            //var sameDateIchi = ichimoku.Where(r => r.Date == historiesInPeriodOfTime[i].Date).FirstOrDefault();
            //            //if (sameDateIchi != null && !historiesInPeriodOfTime[i].HadIchimoku())
            //            //{
            //            //    historiesInPeriodOfTime[i].IchimokuCloudBot = sameDateIchi.SenkouSpanB.HasValue ? (decimal)sameDateIchi.SenkouSpanB.Value : 0;
            //            //    historiesInPeriodOfTime[i].IchimokuCloudTop = sameDateIchi.SenkouSpanA.HasValue ? (decimal)sameDateIchi.SenkouSpanA.Value : 0;
            //            //    historiesInPeriodOfTime[i].IchimokuTenKan = sameDateIchi.TenkanSen.HasValue ? (decimal)sameDateIchi.TenkanSen.Value : 0;
            //            //    historiesInPeriodOfTime[i].IchimokuKijun = sameDateIchi.KijunSen.HasValue ? (decimal)sameDateIchi.KijunSen.Value : 0;
            //            //}

            //            historiesInPeriodOfTime[i].NenBot = historiesInPeriodOfTime[i].TangGia() ? historiesInPeriodOfTime[i].O : historiesInPeriodOfTime[i].C;
            //            historiesInPeriodOfTime[i].NenTop = historiesInPeriodOfTime[i].TangGia() ? historiesInPeriodOfTime[i].C : historiesInPeriodOfTime[i].O;

            //            //var sameDateRSI = rsis.Where(r => r.Date == historiesInPeriodOfTime[i].Date).FirstOrDefault();
            //            //if (sameDateRSI != null && !historiesInPeriodOfTime[i].HadRsi())
            //            //{
            //            //    historiesInPeriodOfTime[i].RSI = sameDateRSI.Rsi.HasValue ? (decimal)sameDateRSI.Rsi.Value : 0;
            //            //}

            //            var sameDateBands = bands.Where(r => r.Date == historiesInPeriodOfTime[i].Date).FirstOrDefault();
            //            if (sameDateBands != null && !historiesInPeriodOfTime[i].HadBands())
            //            {
            //                historiesInPeriodOfTime[i].BandsTop = sameDateBands.UpperBand.HasValue ? (decimal)sameDateBands.UpperBand.Value : 0;
            //                historiesInPeriodOfTime[i].BandsBot = sameDateBands.LowerBand.HasValue ? (decimal)sameDateBands.LowerBand.Value : 0;
            //                historiesInPeriodOfTime[i].BandsMid = sameDateBands.Sma.HasValue ? (decimal)sameDateBands.Sma.Value : 0;
            //            }

            //            //var sameDateMacd = macd.Where(r => r.Date == historiesInPeriodOfTime[i].Date).FirstOrDefault();
            //            //if (sameDateMacd != null && !historiesInPeriodOfTime[i].HadMACD())
            //            //{
            //            //    historiesInPeriodOfTime[i].MACD = sameDateMacd.Macd.HasValue ? (decimal)sameDateMacd.Macd.Value : 0;
            //            //    historiesInPeriodOfTime[i].MACDSignal = sameDateMacd.Signal.HasValue ? (decimal)sameDateMacd.Signal.Value : 0;
            //            //    historiesInPeriodOfTime[i].MACDMomentum = sameDateMacd.Histogram.HasValue ? (decimal)sameDateMacd.Histogram.Value : 0;
            //            //}
            //        }
            //        catch (Exception ex)
            //        {
            //            continue;
            //        }
            //    }
            //}

            //var thelast30Sessions = DateTime.Now.AddDays(-100); //need 100 days to make sure we can take at least 60 last sessions
            //var historyDaily = await _context.History.Where(h => splitStringCode.Contains(h.StockSymbol) && h.Date >= thelast30Sessions).OrderByDescending(h => h.Date).ToListAsync();
            //var now = DateTime.Now.ToString("MM-dd HH:mm");
            //var tupData = new List<Tuple<string, string>>();
            //Parallel.ForEach(splitStringCode, (Action<string>)(symbol =>
            //{
            //    var histories = historiesHourly
            //                    .Where(ss => ss.StockSymbol == symbol)
            //                    .OrderByDescending(h => h.Date)
            //                    .ToList();

            //    if (histories.Count > 100)
            //    {
            //        var phienHumNay = histories.First();
            //        var volMa20CuaPhienHumNay = phienHumNay.VOL(histories, -20);
            //        var hasDongTien = historyDaily.CoDongTienVo(historyDaily[0])
            //            ? " --- CP co dong tien"
            //            : "";
            //        //Xet cho trường hợp hiện tại, giá vượt MA 20 chart H, theo dõi V để vô
            //        if (histories.LaCayVuotMA201(phienHumNay))
            //            //result.Add($"{symbol} - ({now}) Giá / MA20 ------ {phienHumNay.C.ToString("N2")}/{phienHumNay.BandsMid.ToString("N2")} -------- {phienHumNay.V}/{volMa20CuaPhienHumNay} - Theo doi Vol {hasDongTien}");
            //            tupData.Add(new Tuple<string, string>(symbol, $"{symbol} - ({now}) Giá / MA20 ------ {phienHumNay.C.ToString("N2")}/{phienHumNay.BandsMid.ToString("N2")} -------- {phienHumNay.V}/{volMa20CuaPhienHumNay} - Theo doi Vol {hasDongTien}"));
            //        else
            //        {
            //            //Xet cho truong hop trong qua khứ đã vượt MA 20
            //            if (volMa20CuaPhienHumNay > ConstantData.minMA20VolHourly)
            //            {
            //                var cayvuotMA20 = new HistoryHour();
            //                for (int j = 0; j < 12; j++)
            //                {
            //                    var phienHMinus = histories[0 + j];
            //                    var vuot = histories.LaCayVuotMA20(phienHMinus);
            //                    if (vuot)
            //                    {
            //                        cayvuotMA20 = phienHMinus;
            //                        break;
            //                    }
            //                }

            //                if (cayvuotMA20.V > 0
            //                    && phienHumNay.V > 0                        //Nếu ko có cây vượt thì thôi (V = 0)
            //                    && phienHumNay.C >= phienHumNay.BandsMid)   //Nếu cây hiện tại nằm dưới MA 20 hoặc không có vol thì cũng thôi
            //                {
            //                    //nếu cây hum nay là cây vượt hoặc ngay sau cây vượt thì vol phải cao hơn MA 20
            //                    if (cayvuotMA20.Date == phienHumNay.Date && phienHumNay.V > volMa20CuaPhienHumNay)
            //                    {
            //                        //result.Add($"{symbol} - ({now}) Giá / MA20 ------ {phienHumNay.C.ToString("N2")}/{phienHumNay.BandsMid.ToString("N2")} -------- {phienHumNay.V}/{volMa20CuaPhienHumNay} {hasDongTien}");
            //                        tupData.Add(new Tuple<string, string>(symbol, $"{symbol} - ({now}) Giá / MA20 ------ {phienHumNay.C.ToString("N2")}/{phienHumNay.BandsMid.ToString("N2")} -------- {phienHumNay.V}/{volMa20CuaPhienHumNay} {hasDongTien}"));
            //                    }

            //                    //nếu cây vượt ở quá khứ, thì theo dõi vol và giá vòng về MA 20
            //                    if (cayvuotMA20.Date < phienHumNay.Date)
            //                    {
            //                        //result.Add($"{symbol} - ({now}) Giá / MA20 ------ {phienHumNay.C.ToString("N2")}/{phienHumNay.BandsMid.ToString("N2")} -------- {phienHumNay.V}/{volMa20CuaPhienHumNay} - Vuot MA20 {cayvuotMA20.Date.AddHours(7).ToString("MM-dd:HH:mm")} {hasDongTien}");
            //                        tupData.Add(new Tuple<string, string>(symbol, $"{symbol} - ({now}) Giá / MA20 ------ {phienHumNay.C.ToString("N2")}/{phienHumNay.BandsMid.ToString("N2")} -------- {phienHumNay.V}/{volMa20CuaPhienHumNay} - Vuot MA20 {cayvuotMA20.Date.AddHours(7).ToString("MM-dd:HH:mm")} {hasDongTien}"));
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}));

            //tupData = tupData.OrderBy(t => t.Item1).ToList();
            //result.AddRange(tupData.Select(t => t.Item2).ToList());
            return result;
        }

        /// <summary>
        /// Khuyến nghị chạy lúc 14:20
        /// Lấy dữ liệu trong vòng 5 ngày trước
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> MonitoringMinVol()
        {
            var result = new List<string>();
            var files = Directory.GetFiles(ConstantPath.Path);

            var filename = files.Where(f => f.Split(ConstantPath.Path)[1].StartsWith("2022-")).OrderByDescending(f => f).FirstOrDefault();
            if (string.IsNullOrEmpty(filename)) return null;

            var folder = ConstantPath.Path;
            //var g = DateTime.Now.ToString("yyyy-MM-dd HH-mm");
            //var name = $@"{filename}.xlsx";
            //string pathExcel = $"{ConstantPath.Path}{filename}";
            var data = filename.ReadFromExcel();

            //Code - win rate - Vol
            var tupData = new List<Tuple<string, decimal, decimal>>();
            var dataResult = new List<Tuple<string, decimal, DateTime, string>>();
            foreach (DataRow item in data.Rows)
            {
                tupData.Add(new Tuple<string, decimal, decimal>(item[0].ToString(), decimal.Parse(item[1].ToString()), decimal.Parse(item[2].ToString())));
            }
            var symbolsText = tupData.Select(s => s.Item1).ToList();
            var restService = new RestServiceHelper();
            var fireantData = new List<HistoryHour>();
            var service = new Service();
            var startCalculateFrom = new DateTime(2022, 8, 14); //DateTime.Now;
            await service.GetVHoursFireAnt(fireantData, symbolsText, startCalculateFrom, fireAntCookies);
            var histories = await _context.History.Where(h => symbolsText.Contains(h.StockSymbol) && h.Date >= startCalculateFrom).ToListAsync();
            foreach (var tupItem in tupData)
            {
                var fireantItems = fireantData.Where(t => t.StockSymbol == tupItem.Item1).OrderByDescending(f => f.Date).ToList();

                if (!fireantItems.Any()) continue;

                var fireantItemsGroupedByDay = fireantItems.GroupBy(g => g.Date.Date).Select(d => new { key = d.Key, values = d.ToList() }).ToList();
                foreach (var item in fireantItemsGroupedByDay.OrderBy(f => f.key))
                {
                    var fireantItem = item.values.OrderBy(v => v.Date).ToList();

                    var phienMoCua = fireantItem.First();
                    var phienDongCua = fireantItem.Last();
                    var laPhienTangGia = phienMoCua.O <= phienDongCua.C && fireantItem.Any(f => f.H > phienMoCua.O);

                    if (laPhienTangGia && fireantItem.Sum(t => t.V) > 0 && fireantItem.Sum(t => t.V) < tupItem.Item3)
                    {
                        var giaGoiY = item.key.WithoutHours() < DateTime.Now.WithoutHours()
                            ? histories.First(h => h.StockSymbol == tupItem.Item1 && h.Date == item.key.WithoutHours()).C
                            : phienDongCua.C;
                        var stringText = $"{tupItem.Item1}-Rate: {tupItem.Item2}-{item.key.ToShortDateString()}- {fireantItem.Sum(t => t.V)}/{tupItem.Item3} (Current/Expected) - Giá goi y: {giaGoiY.ToString("N2")}";
                        dataResult.Add(new Tuple<string, decimal, DateTime, string>(tupItem.Item1, tupItem.Item2, item.key, stringText));
                    }
                }
            }

            dataResult = dataResult.OrderByDescending(d => d.Item3).ThenBy(d => d.Item1).ToList();
            result = dataResult.Select(d => d.Item4).ToList();
            return result;
        }

        /// <summary>
        /// Khuyến nghị chạy 1h 1 lần
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task<List<string>> MonitoringHourly()
        {
            var restService = new RestServiceHelper();
            string code = string.Empty;//OIL,PSH,TSC,NBC,AGM,BSR,DRC,MBS,STB,LTG,NKG,PAN,PAS,TAR,IDJ,
            //string code = "TGG";

            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new List<string>() : code.Split(",").ToList();
            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.Where(s => !ConstantData.BadCodes.Contains(s._sc_) && s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > 300000).OrderByDescending(s => s._sc_).ToListAsync()
                : await _context.StockSymbol.Where(s => !ConstantData.BadCodes.Contains(s._sc_) && s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > 300000 && splitStringCode.Contains(s._sc_)).OrderByDescending(s => s._sc_).ToListAsync();
            var selectedCode = symbols.Select(s => s._sc_).Distinct().ToList();
            var historiesHourly = new List<HistoryHour>();
            var result = new List<string>();
            var service = new Service();

            await service.GetVHoursFireAnt(historiesHourly, selectedCode, fireAntDataForHourFromDate, fireAntCookies);

            foreach (var symbol in selectedCode)
            {
                //Update indicators - Done
                var historiesInPeriodOfTime = historiesHourly
                    .Where(ss => ss.StockSymbol == symbol)
                    .OrderBy(h => h.Date)
                    .ToList();

                if (!historiesInPeriodOfTime.Any()) continue;

                var qoutes = MACDConvertHistory(historiesInPeriodOfTime);

                //var ichimoku = qoutes.GetIchimoku();
                var macd = qoutes.GetMacd();
                var rsis = qoutes.GetRsi();
                var bands = qoutes.GetBollingerBands();
                //var ma5 = qoutes.GetSma(5);

                for (int i = 0; i < historiesInPeriodOfTime.Count; i++)
                {
                    try
                    {
                        //if (historiesInPeriodOfTime[i].HadAllIndicators()) continue;

                        //var sameDateMA5 = ma5.Where(r => r.Date == historiesInPeriodOfTime[i].Date).FirstOrDefault();
                        //if (sameDateMA5 != null && !historiesInPeriodOfTime[i].HadMA5())
                        //{
                        //    historiesInPeriodOfTime[i].GiaMA05 = sameDateMA5.Sma.HasValue ? (decimal)sameDateMA5.Sma.Value : 0;
                        //}

                        //var sameDateIchi = ichimoku.Where(r => r.Date == historiesInPeriodOfTime[i].Date).FirstOrDefault();
                        //if (sameDateIchi != null && !historiesInPeriodOfTime[i].HadIchimoku())
                        //{
                        //    historiesInPeriodOfTime[i].IchimokuCloudBot = sameDateIchi.SenkouSpanB.HasValue ? (decimal)sameDateIchi.SenkouSpanB.Value : 0;
                        //    historiesInPeriodOfTime[i].IchimokuCloudTop = sameDateIchi.SenkouSpanA.HasValue ? (decimal)sameDateIchi.SenkouSpanA.Value : 0;
                        //    historiesInPeriodOfTime[i].IchimokuTenKan = sameDateIchi.TenkanSen.HasValue ? (decimal)sameDateIchi.TenkanSen.Value : 0;
                        //    historiesInPeriodOfTime[i].IchimokuKijun = sameDateIchi.KijunSen.HasValue ? (decimal)sameDateIchi.KijunSen.Value : 0;
                        //}

                        historiesInPeriodOfTime[i].NenBot = historiesInPeriodOfTime[i].TangGia() ? historiesInPeriodOfTime[i].O : historiesInPeriodOfTime[i].C;
                        historiesInPeriodOfTime[i].NenTop = historiesInPeriodOfTime[i].TangGia() ? historiesInPeriodOfTime[i].C : historiesInPeriodOfTime[i].O;

                        var sameDateRSI = rsis.Where(r => r.Date == historiesInPeriodOfTime[i].Date).FirstOrDefault();
                        if (sameDateRSI != null && !historiesInPeriodOfTime[i].HadRsi())
                        {
                            historiesInPeriodOfTime[i].RSI = sameDateRSI.Rsi.HasValue ? (decimal)sameDateRSI.Rsi.Value : 0;
                        }

                        var sameDateBands = bands.Where(r => r.Date == historiesInPeriodOfTime[i].Date).FirstOrDefault();
                        if (sameDateBands != null && !historiesInPeriodOfTime[i].HadBands())
                        {
                            historiesInPeriodOfTime[i].BandsTop = sameDateBands.UpperBand.HasValue ? (decimal)sameDateBands.UpperBand.Value : 0;
                            historiesInPeriodOfTime[i].BandsBot = sameDateBands.LowerBand.HasValue ? (decimal)sameDateBands.LowerBand.Value : 0;
                            historiesInPeriodOfTime[i].BandsMid = sameDateBands.Sma.HasValue ? (decimal)sameDateBands.Sma.Value : 0;
                        }

                        var sameDateMacd = macd.Where(r => r.Date == historiesInPeriodOfTime[i].Date).FirstOrDefault();
                        if (sameDateMacd != null && !historiesInPeriodOfTime[i].HadMACD())
                        {
                            historiesInPeriodOfTime[i].MACD = sameDateMacd.Macd.HasValue ? (decimal)sameDateMacd.Macd.Value : 0;
                            historiesInPeriodOfTime[i].MACDSignal = sameDateMacd.Signal.HasValue ? (decimal)sameDateMacd.Signal.Value : 0;
                            historiesInPeriodOfTime[i].MACDMomentum = sameDateMacd.Histogram.HasValue ? (decimal)sameDateMacd.Histogram.Value : 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
            }

            //return historiesHourly;

            var rsiPk = await PhanKyDuongChartH(historiesHourly, 2, 60, "RSI", 1.01M, 1.01M);
            result.AddRange(rsiPk);
            var macdPk = await PhanKyDuongChartH(historiesHourly, 2, 60, "MACD", 1.01M, 1.01M);
            result.AddRange(macdPk);

            return result;
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
        public async Task<List<string>> PhanKyDuongChartH(List<HistoryHour> historiesStockCode,
            int KC2D, int SoPhienKT,
            string propertyName,
            decimal CL2G, decimal CL2D)
        {
            var stockCodes = historiesStockCode.Select(s => s.StockSymbol).Distinct().OrderBy(d => d).ToList();
            var dis = new List<Tuple<DateTime, DateTime, string>>();
            var result1 = new List<string>();

            foreach (var symbol in stockCodes)
            {
                var histories = historiesStockCode
                                .Where(ss => ss.StockSymbol == symbol)
                                .OrderByDescending(h => h.Date)
                                .ToList();

                if (histories.Count <= 100) continue;

                if (histories[0].RSI <= histories[1].RSI) //rsi đang giảm
                {
                    continue; //ko cần tìm trong quá khứ làm gì cả, đang giảm rùi
                }

                for (int i = 0; i < 12; i++) //chỉ tìm trong 3 phiên trước, nếu có xuất hiện đáy 2 thì mới báo, còn lâu hơn rùi thì thui
                {
                    var buyingDate = histories[i];
                    //Giả định ngày trước đó là đáy
                    var dayGiaDinh = histories[i + 1];

                    //hum nay RSI tăng so với hum wa, thì hum wa mới là đáy, còn ko thì mai RSI vẫn có thể giảm tiếp, ko ai bik
                    var propertyValueOfDayGiaDinh = (decimal)dayGiaDinh.GetPropValue(propertyName);
                    var propertyValueOfSuggestedDate = (decimal)buyingDate.GetPropValue(propertyName);

                    if (propertyValueOfDayGiaDinh == 0
                        || propertyValueOfSuggestedDate <= propertyValueOfDayGiaDinh)
                    {
                        continue;
                    }

                    //Kiem tra đáy giả định: trong vòng 14 phiên trước không có cây nào trước đó thấp hơn nó
                    var nhungNgaySoSanhVoiDayGiaDinh = histories.OrderByDescending(h => h.Date).Where(h => h.Date < dayGiaDinh.Date).Take(SoPhienKT).ToList();
                    if (nhungNgaySoSanhVoiDayGiaDinh.Count < SoPhienKT) continue;

                    bool? hasPhanKy = false;
                    for (int j = KC2D; j < nhungNgaySoSanhVoiDayGiaDinh.Count - 1; j++)
                    {
                        var ngàyĐếmNgược = nhungNgaySoSanhVoiDayGiaDinh[j];

                        var propertyValueOfngàyĐếmNgược = (decimal)ngàyĐếmNgược.GetPropValue(propertyName);

                        hasPhanKy = histories.HasPhanKyDuong(dayGiaDinh, ngàyĐếmNgược, propertyName, SoPhienKT, CL2G, CL2D);

                        if (hasPhanKy != null)
                        {
                            dis.Add(new Tuple<DateTime, DateTime, string>(dayGiaDinh.Date, ngàyĐếmNgược.Date, $"{symbol} - {propertyName} - Xác nhận: {buyingDate.Date.ToShortDateString()} - Đáy 2: {dayGiaDinh.Date.ToShortDateString()} {propertyValueOfDayGiaDinh} - Giá {dayGiaDinh.NenBot} - Đáy 1: {ngàyĐếmNgược.Date.ToShortDateString()} {propertyValueOfngàyĐếmNgược} - Giá {ngàyĐếmNgược.NenBot}"));
                            break;
                        }
                    }

                    if (hasPhanKy.HasValue) break;
                }
            }

            dis = dis.OrderByDescending(d => d.Item1).ThenBy(d => d.Item2).ThenBy(d => d.Item3).ToList();
            result1 = dis.Select(d => d.Item3).ToList();

            return result1;
        }
    }
}