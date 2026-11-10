using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace ProjectTaskManager.DAL
{
    public class PhanCongDAL
    {
        public List<int> GetMaThanhVienByTask(int maTask)
        {
            var list = new List<int>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = new SqlCommand("SELECT MaThanhVien FROM PhanCong WHERE MaTask=@Ma", conn))
            {
                cmd.Parameters.AddWithValue("@Ma", maTask);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(r.GetInt32(0));
            }
            return list;
        }

        /// <summary>Giao việc cho nhiều thành viên: xóa phân công cũ rồi thêm mới (1 transaction).</summary>
        public void GiaoViec(int maTask, List<int> dsThanhVien)
        {
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        using (var del = new SqlCommand("DELETE FROM PhanCong WHERE MaTask=@T", conn, trans))
                        { del.Parameters.AddWithValue("@T", maTask); del.ExecuteNonQuery(); }

                        foreach (int tv in dsThanhVien)
                        {
                            using (var ins = new SqlCommand(
                                "INSERT INTO PhanCong (MaTask, MaThanhVien) VALUES (@T, @TV)", conn, trans))
                            {
                                ins.Parameters.AddWithValue("@T", maTask);
                                ins.Parameters.AddWithValue("@TV", tv);
                                ins.ExecuteNonQuery();
                            }
                        }
                        trans.Commit();
                    }
                    catch { trans.Rollback(); throw; }
                }
            }
        }
    }
}
