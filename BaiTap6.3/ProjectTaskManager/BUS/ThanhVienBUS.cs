using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ProjectTaskManager.DAL;
using ProjectTaskManager.DTO;

namespace ProjectTaskManager.BUS
{
    public class ThanhVienBUS
    {
        private readonly ThanhVienDAL dal = new ThanhVienDAL();

        public List<ThanhVienDTO> GetAll() => dal.GetAll();

        private static void Validate(ThanhVienDTO t)
        {
            if (string.IsNullOrWhiteSpace(t.HoTen))
                throw new ArgumentException("Họ tên không được để trống.");
            if (!string.IsNullOrWhiteSpace(t.Email) &&
                !Regex.IsMatch(t.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ArgumentException("Email không hợp lệ.");
            if (string.IsNullOrWhiteSpace(t.Email)) t.Email = null; // tránh trùng chuỗi rỗng với UNIQUE
        }

        public void Them(ThanhVienDTO t) { Validate(t); dal.Insert(t); }
        public void Sua(ThanhVienDTO t)
        {
            if (t.MaThanhVien <= 0) throw new ArgumentException("Chưa chọn thành viên cần sửa.");
            Validate(t); dal.Update(t);
        }
        public void Xoa(int ma)
        {
            if (ma <= 0) throw new ArgumentException("Chưa chọn thành viên cần xóa.");
            dal.Delete(ma);
        }
    }
}
