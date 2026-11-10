using System;
using System.Collections.Generic;
using ProjectTaskManager.DAL;
using ProjectTaskManager.DTO;

namespace ProjectTaskManager.BUS
{
    public class DuAnBUS
    {
        private readonly DuAnDAL dal = new DuAnDAL();

        public List<DuAnDTO> GetAll() => dal.GetAll();

        private static void Validate(DuAnDTO d)
        {
            if (string.IsNullOrWhiteSpace(d.TenDuAn))
                throw new ArgumentException("Tên dự án không được để trống.");
            if (d.TenDuAn.Length > 150)
                throw new ArgumentException("Tên dự án tối đa 150 ký tự.");
            if (d.NgayBatDau.HasValue && d.NgayKetThuc.HasValue && d.NgayKetThuc < d.NgayBatDau)
                throw new ArgumentException("Ngày kết thúc phải sau hoặc bằng ngày bắt đầu.");
        }

        public void Them(DuAnDTO d) { Validate(d); dal.Insert(d); }
        public void Sua(DuAnDTO d)
        {
            if (d.MaDuAn <= 0) throw new ArgumentException("Chưa chọn dự án cần sửa.");
            Validate(d); dal.Update(d);
        }
        public void Xoa(int ma)
        {
            if (ma <= 0) throw new ArgumentException("Chưa chọn dự án cần xóa.");
            dal.Delete(ma);
        }
    }
}
