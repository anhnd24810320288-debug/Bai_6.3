using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using ProjectTaskManager.DTO;

namespace ProjectTaskManager.DAL
{
    public class ThongKeDAL
    {
        /// <summary>% hoàn thành của từng thành viên trong 1 dự án (GROUP BY).</summary>
        public List<ThongKeThanhVienDTO> GetTyLeTheoThanhVien(int maDuAn)
        {
            const string sql = @"
SELECT tv.MaThanhVien, tv.HoTen,
       COUNT(*) AS TongTask,
       SUM(CASE WHEN cv.TrangThai = N'Done' THEN 1 ELSE 0 END) AS TaskHoanThanh,
       CAST(100.0 * SUM(CASE WHEN cv.TrangThai = N'Done' THEN 1 ELSE 0 END) / COUNT(*) AS DECIMAL(5,2)) AS TyLe
FROM PhanCong pc
JOIN CongViec cv ON cv.MaTask = pc.MaTask
JOIN ThanhVien tv ON tv.MaThanhVien = pc.MaThanhVien
WHERE cv.MaDuAn = @MaDuAn
GROUP BY tv.MaThanhVien, tv.HoTen
ORDER BY TyLe DESC, tv.HoTen";
            var list = new List<ThongKeThanhVienDTO>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@MaDuAn", maDuAn);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new ThongKeThanhVienDTO
                        {
                            MaThanhVien = r.GetInt32(0),
                            HoTen = r.GetString(1),
                            TongTask = r.GetInt32(2),
                            TaskHoanThanh = r.GetInt32(3),
                            TyLeHoanThanh = r.GetDecimal(4)
                        });
                    }
                }
            }
            return list;
        }

        /// <summary>% hoàn thành toàn dự án (ExecuteScalar).</summary>
        public decimal GetTyLeDuAn(int maDuAn)
        {
            const string sql = @"
SELECT ISNULL(CAST(100.0 * SUM(CASE WHEN TrangThai = N'Done' THEN 1 ELSE 0 END)
                   / NULLIF(COUNT(*), 0) AS DECIMAL(5,2)), 0)
FROM CongViec WHERE MaDuAn = @MaDuAn";
            return Convert.ToDecimal(DbHelper.ExecuteScalar(sql, new SqlParameter("@MaDuAn", maDuAn)));
        }
    }
}
