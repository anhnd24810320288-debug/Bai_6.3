using System;

namespace ProjectTaskManager.DTO
{
    public class CongViecDTO
    {
        public int MaTask { get; set; }
        public int MaDuAn { get; set; }
        public string TenTask { get; set; }
        public string MoTa { get; set; }
        public DateTime? HanNop { get; set; }
        public string DoUuTien { get; set; }
        public string TrangThai { get; set; }

        // Thuộc tính hiển thị (JOIN)
        public string TenDuAn { get; set; }
        public string NguoiThucHien { get; set; }

        public override string ToString()
            => $"#{MaTask} {TenTask} [{DoUuTien}]" + (string.IsNullOrEmpty(NguoiThucHien) ? "" : $" - {NguoiThucHien}");
    }

    public static class TrangThaiTask
    {
        public static readonly string[] All = { "To Do", "In Progress", "Review", "Done" };
        public static readonly string[] DoUuTienAll = { "Cao", "Trung bình", "Thấp" };
    }
}
