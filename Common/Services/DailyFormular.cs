using DotNetCoreSqlDb.Models;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCoreSqlDb.Common.Services
{
    public class DailyFormular : IFormular
    {
        public List<History> histories;
        public History ngayKiemTra;

        private int SoNgayQuayTroLaiDeKiemTraDay2MACD = 30;
        private int SoNgayQuayTroLaiDeKiemTraDayMACDPhanKi = 14;
        private decimal CL2G = 1;
        private decimal CL2D = 1;
        private int KC2D = 3;
        private string MACDProperty = "MACD";
        private string RSIProperty = "RSI";

        public DailyFormular()
        {

        }

        /// <summary>
        /// Giá đang giảm >= 4 phiên (Nến đỏ hoặc nến bot hum nay nhỏ hơn nến bot hum wa)
        /// MACD bẻ ngang hoặc tăng (nhưng nến buộc phải là nến đỏ hoặc nến doji, spin có nến bot cao nhất chỉ cách nến bot hum wa = nến bot hum wa + 1/5 thân nến)
        /// Với mẫu này thì canh mua ở gần ATC để biết giá đóng cửa chính xác, mua thăm dò 20%
        /// 
        /// IDI (7/7/22), HPG (16/6/22)
        /// Giá rơi tự do, điểm mua sẽ xuất hiện khi MACD bé ngang + đây là giá thấp nhất trong vòng 6 tháng qua, hoặc là RSI thấp nhất trong vòng 6 tháng qua - sẽ có nhịp hồi kĩ thuật
        /// MSN 17/5/22
        /// </summary>
        public string ChuanBiBatDaoRoi()
        {
            var soNgayKiemTra = histories.OrderByDescending(h => h.Date).Where(h => h.Date <= ngayKiemTra.Date).Take(SoNgayQuayTroLaiDeKiemTraDay2MACD).ToList();
            var soNgayGiaDangGiam = 0;
            var macdDangBeNgang = false;
            for (int i = 0; i < soNgayKiemTra.Count - 1; i++)
            {
                if (!soNgayKiemTra[i].TangGia()
                    || (soNgayKiemTra[i].L < soNgayKiemTra[i + 1].L && soNgayKiemTra[i].H < soNgayKiemTra[i + 1].H))
                    soNgayGiaDangGiam++;
            }

            if (soNgayKiemTra[0].MACD - soNgayKiemTra[0 + 1].MACD > soNgayKiemTra[0 + 1].MACD - soNgayKiemTra[0 + 2].MACD
                && soNgayKiemTra[0].MACD - soNgayKiemTra[0 + 1].MACD > soNgayKiemTra[0 + 2].MACD - soNgayKiemTra[0 + 3].MACD)
                macdDangBeNgang = true;

            var rsiDuoi35 = ngayKiemTra.RSI <= 35;

            var historyInTheLast30Session = histories.OrderByDescending(h => h.Date).Where(h => h.Date <= ngayKiemTra.Date).Take(30).ToList();

            //Đây có phải RSI thấp nhất trong 1.5 tháng vừa qua chưa
            var top3RSI = historyInTheLast30Session.OrderByDescending(h => h.RSI).Take(3).ToList();
            var rsiThapNhatTrong30PhienChua = top3RSI.Any(t => t.RSI == ngayKiemTra.RSI);

            //Đây có phải giá thấp nhất trong 1.5 tháng vừa qua chưa
            var top3Gia = historyInTheLast30Session.OrderByDescending(h => h.NenBot).Take(3).ToList();
            var giaThapNhatTrong30PhienChua = top3Gia.Any(t => t.NenBot == ngayKiemTra.NenBot);

            if (soNgayGiaDangGiam >= 3 && rsiThapNhatTrong30PhienChua && giaThapNhatTrong30PhienChua)
                return "Canh trong phiên mai - nếu nến đỏ liên tục hoặc nến xanh thân nhỏ thì mua giá ở ATC cho rẻ. Nếu nến xanh thân dày thì mua sớm giá tốt sau 11h. Tuyệt đối không mua khi giá mở cửa xuất hiện cao hơn hoặc bằng nến top ngày hum wa";

            if (soNgayGiaDangGiam >= 3 && macdDangBeNgang && rsiDuoi35)
                return "MACD đi ngang, nên kết hợp w đi ngang giá, nhưng khá an tâm khi mua ở điểm này, có thể giá sẽ cao 1 chút. Canh mua giá ở giữa thân nến ngày hum nay tới giá H của ngày hum nay. Tuyệt đối không mua khi giá mở cửa tạo gap";

            return string.Empty;
        }

        public int MACD2DayTuNPhienTruoc()
        {
            var soNgayKiemTra = histories.OrderByDescending(h => h.Date).Where(h => h.Date < ngayKiemTra.Date).Take(SoNgayQuayTroLaiDeKiemTraDay2MACD).ToList();
            for (int i = 0; i < soNgayKiemTra.Count; i++)
            {
                var có2Đáy = histories.CoTao2DayChua(soNgayKiemTra[i], "MACD");
                if (có2Đáy)
                    return i + 1;// 0 is yesterday - we return 1 means đáy 2 đã có từ 1 ngày trước
            }

            return 0;
        }

        public int MACDPhanKyDuongTuNPhienTruoc()
        {
            var soNgayKiemTra = histories.OrderByDescending(h => h.Date).Where(h => h.Date < ngayKiemTra.Date).Take(SoNgayQuayTroLaiDeKiemTraDay2MACD).ToList();
            for (int i = 0; i < soNgayKiemTra.Count; i++)
            {
                var buyingDate = histories[i]; //Or ngayKiemTra is also true

                //Giả định ngày trước đó là đáy
                var dayGiaDinh = histories[i - 1];

                //hum nay RSI tăng so với hum wa, thì hum wa mới là đáy, còn ko thì mai RSI vẫn có thể giảm tiếp, ko ai bik
                var propertyValueOfDayGiaDinh = (decimal)dayGiaDinh.GetPropValue(MACDProperty);
                var propertyValueOfSuggestedDate = (decimal)buyingDate.GetPropValue(MACDProperty);
                if (propertyValueOfDayGiaDinh == 0
                    || propertyValueOfSuggestedDate <= propertyValueOfDayGiaDinh
                    || buyingDate.NenBot < dayGiaDinh.NenBot)
                {
                    continue;
                }

                //Kiem tra đáy giả định: trong vòng 14 phiên trước không có cây nào trước đó thấp hơn nó
                var nhungNgaySoSanhVoiDayGiaDinh = histories.OrderByDescending(h => h.Date).Where(h => h.Date < dayGiaDinh.Date).Take(SoNgayQuayTroLaiDeKiemTraDayMACDPhanKi).ToList();
                if (nhungNgaySoSanhVoiDayGiaDinh.Count < SoNgayQuayTroLaiDeKiemTraDayMACDPhanKi) continue;

                for (int j = KC2D; j < nhungNgaySoSanhVoiDayGiaDinh.Count - 1; j++)
                {
                    var ngàyĐếmNgược = nhungNgaySoSanhVoiDayGiaDinh[j];

                    var hasPhanKyDuong = histories.HasPhanKyDuong(dayGiaDinh, ngàyĐếmNgược, MACDProperty, SoNgayQuayTroLaiDeKiemTraDayMACDPhanKi, CL2G, CL2D);

                    if (hasPhanKyDuong != null && hasPhanKyDuong.HasValue && hasPhanKyDuong.Value) //ngày đếm cũng là ngày có đáy
                        return i + 1;// 0 is yesterday - we return 1 means đáy 2 đã có từ 1 ngày trước
                }
            }

            return 0;
        }

        public int RSIPhanKyDuongTuNPhienTruoc()
        {
            var soNgayKiemTra = histories.OrderByDescending(h => h.Date).Where(h => h.Date < ngayKiemTra.Date).Take(SoNgayQuayTroLaiDeKiemTraDay2MACD).ToList();
            for (int i = 0; i < soNgayKiemTra.Count; i++)
            {
                var buyingDate = histories[i]; //Or ngayKiemTra is also true

                //Giả định ngày trước đó là đáy
                var dayGiaDinh = histories[i - 1];

                //hum nay RSI tăng so với hum wa, thì hum wa mới là đáy, còn ko thì mai RSI vẫn có thể giảm tiếp, ko ai bik
                var propertyValueOfDayGiaDinh = (decimal)dayGiaDinh.GetPropValue(RSIProperty);
                var propertyValueOfSuggestedDate = (decimal)buyingDate.GetPropValue(RSIProperty);
                if (propertyValueOfDayGiaDinh == 0
                    || propertyValueOfSuggestedDate <= propertyValueOfDayGiaDinh
                    || buyingDate.NenBot < dayGiaDinh.NenBot)
                {
                    continue;
                }

                //Kiem tra đáy giả định: trong vòng 14 phiên trước không có cây nào trước đó thấp hơn nó
                var nhungNgaySoSanhVoiDayGiaDinh = histories.OrderByDescending(h => h.Date).Where(h => h.Date < dayGiaDinh.Date).Take(SoNgayQuayTroLaiDeKiemTraDayMACDPhanKi).ToList();
                if (nhungNgaySoSanhVoiDayGiaDinh.Count < SoNgayQuayTroLaiDeKiemTraDayMACDPhanKi) continue;

                for (int j = KC2D; j < nhungNgaySoSanhVoiDayGiaDinh.Count - 1; j++)
                {
                    var ngàyĐếmNgược = nhungNgaySoSanhVoiDayGiaDinh[j];

                    var hasPhanKyDuong = histories.HasPhanKyDuong(dayGiaDinh, ngàyĐếmNgược, RSIProperty, SoNgayQuayTroLaiDeKiemTraDayMACDPhanKi, CL2G, CL2D);

                    if (hasPhanKyDuong != null && hasPhanKyDuong.HasValue && hasPhanKyDuong.Value) //ngày đếm cũng là ngày có đáy
                        return i + 1;// 0 is yesterday - we return 1 means đáy 2 đã có từ 1 ngày trước
                }
            }

            return 0;
        }

        public int TichLuyDuoiMA20()
        {
            var soNgayKiemTra = histories.OrderByDescending(h => h.Date).Where(h => h.Date <= ngayKiemTra.Date).Take(SoNgayQuayTroLaiDeKiemTraDay2MACD).ToList();
            var soNgayGiaDiNgang = 0;
            for (int i = 0; i < soNgayKiemTra.Count; i++)
            {
                var phienKiemTra = soNgayKiemTra[i];
                var giaDangDiNgang = histories.PropertyDiNgangLienTucTrongNPhien(phienKiemTra, "NenTop", 1, Models.Business.Patterns.LocCoPhieu.SoSanhEnum.Bang, 1.02M);

                if (giaDangDiNgang) soNgayGiaDiNgang++;
                else break;
            }

            return soNgayGiaDiNgang;
        }

        public int TichLuyTrenMA20()
        {
            var soNgayKiemTra = histories.OrderByDescending(h => h.Date).Where(h => h.Date <= ngayKiemTra.Date).Take(SoNgayQuayTroLaiDeKiemTraDay2MACD).ToList();
            var soNgayGiaDiNgang = 0;
            for (int i = 0; i < soNgayKiemTra.Count; i++)
            {
                var phienKiemTra = soNgayKiemTra[i];
                var giaDangDiNgang = histories.PropertyDiNgangLienTucTrongNPhien(phienKiemTra, "NenTop", 1, Models.Business.Patterns.LocCoPhieu.SoSanhEnum.Bang, 1.02M);

                if (giaDangDiNgang && phienKiemTra.C > phienKiemTra.BandsMid) soNgayGiaDiNgang++;
                else break;
            }

            return soNgayGiaDiNgang;
        }

        public bool TinHieuDaoChieu()
        {
            var phienHumwa = histories.OrderByDescending(h => h.Date).First(h => h.Date < ngayKiemTra.Date);
            return ngayKiemTra.IsNenDaoChieuTang(phienHumwa, 0.85M) || ngayKiemTra.IsNenDaoChieuTang(phienHumwa, 0.75M);
        }
    }
}
