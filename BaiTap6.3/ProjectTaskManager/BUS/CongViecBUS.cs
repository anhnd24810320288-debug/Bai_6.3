using System;
using System.Collections.Generic;
using ProjectTaskManager.DAL;
using ProjectTaskManager.DTO;

namespace ProjectTaskManager.BUS
{
    public class CongViecBUS
    {
        private readonly CongViecDAL dal = new CongViecDAL();

        public List<CongViecDTO> GetByFilter(int? maDuAn, string trangThai) => dal.GetByFilter(maDuAn, trangThai);

        private static void Validate(CongViecDTO t)
        {
            if (t.MaDuAn <= 0) throw new ArgumentException("Vui lòng chọn dự án cho công việc.");
            if (string.IsNullOrWhiteSpace(t.TenTask)) throw new ArgumentException("Tên công việc không được để trống.");
            if (t.TenTask.Length > 200) throw new ArgumentException("Tên công việc tối đa 200 ký tự.");
            if (Array.IndexOf(TrangThaiTask.All, t.TrangThai) < 0) throw new ArgumentException("Trạng thái không hợp lệ.");
            if (Array.IndexOf(TrangThaiTask.DoUuTienAll, t.DoUuTien) < 0) throw new ArgumentException("Độ ưu tiên không hợp lệ.");
        }

        public int Them(CongViecDTO t) { Validate(t); return dal.Insert(t); }

        public void Sua(CongViecDTO t)
        {
            if (t.MaTask <= 0) throw new ArgumentException("Chưa chọn công việc cần sửa.");
            Validate(t); dal.Update(t);
        }

        public void Xoa(int maTask)
        {
            if (maTask <= 0) throw new ArgumentException("Chưa chọn công việc cần xóa.");
            dal.Delete(maTask);
        }

        public void ChuyenTrangThai(int maTask, string trangThaiMoi)
        {
            if (Array.IndexOf(TrangThaiTask.All, trangThaiMoi) < 0)
                throw new ArgumentException("Trạng thái không hợp lệ.");
            dal.UpdateTrangThai(maTask, trangThaiMoi);
        }

        /// <summary>Trạng thái kế tiếp/lùi theo thứ tự To Do → In Progress → Review → Done; null nếu hết.</summary>
        public static string TrangThaiKe(string hienTai, int buoc)
        {
            int i = Array.IndexOf(TrangThaiTask.All, hienTai) + buoc;
            return (i < 0 || i >= TrangThaiTask.All.Length) ? null : TrangThaiTask.All[i];
        }
    }
}
