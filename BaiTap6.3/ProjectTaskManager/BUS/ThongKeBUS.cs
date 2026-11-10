using System.Collections.Generic;
using ProjectTaskManager.DAL;
using ProjectTaskManager.DTO;

namespace ProjectTaskManager.BUS
{
    public class ThongKeBUS
    {
        private readonly CongViecDAL cvDal = new CongViecDAL();
        private readonly ThongKeDAL tkDal = new ThongKeDAL();

        public List<CongViecDTO> GetCongViecTreHan(int? maDuAn) => cvDal.GetOverdue(maDuAn);
        public List<ThongKeThanhVienDTO> GetTyLeTheoThanhVien(int maDuAn) => tkDal.GetTyLeTheoThanhVien(maDuAn);
        public decimal GetTyLeDuAn(int maDuAn) => tkDal.GetTyLeDuAn(maDuAn);
    }
}
