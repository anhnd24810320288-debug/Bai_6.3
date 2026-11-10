using System;
using System.Collections.Generic;
using ProjectTaskManager.DAL;

namespace ProjectTaskManager.BUS
{
    public class PhanCongBUS
    {
        private readonly PhanCongDAL dal = new PhanCongDAL();

        public List<int> GetThanhVienDuocGiao(int maTask) => dal.GetMaThanhVienByTask(maTask);

        public void GiaoViec(int maTask, List<int> dsThanhVien)
        {
            if (maTask <= 0) throw new ArgumentException("Chưa chọn công việc để giao.");
            dal.GiaoViec(maTask, dsThanhVien ?? new List<int>());
        }
    }
}
