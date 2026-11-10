using System;

namespace ProjectTaskManager.DTO
{
    public class DuAnDTO
    {
        public int MaDuAn { get; set; }
        public string TenDuAn { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public string TrangThai { get; set; }

        public override string ToString() => TenDuAn;
    }

    public static class TrangThaiDuAn
    {
        public static readonly string[] All = { "Đang chạy", "Hoàn thành", "Tạm dừng" };
    }
}
