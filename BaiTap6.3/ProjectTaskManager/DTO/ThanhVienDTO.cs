namespace ProjectTaskManager.DTO
{
    public class ThanhVienDTO
    {
        public int MaThanhVien { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string ChuyenMon { get; set; }

        public override string ToString() => HoTen;
    }
}
