using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using ProjectTaskManager.DTO;

namespace ProjectTaskManager.DAL
{
    public class ThanhVienDAL
    {
        public List<ThanhVienDTO> GetAll()
        {
            var list = new List<ThanhVienDTO>();
            const string sql = "SELECT MaThanhVien, HoTen, Email, ChuyenMon FROM ThanhVien ORDER BY HoTen";
            using (var conn = DbHelper.GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new ThanhVienDTO
                        {
                            MaThanhVien = r.GetInt32(0),
                            HoTen = r.GetString(1),
                            Email = r.IsDBNull(2) ? null : r.GetString(2),
                            ChuyenMon = r.IsDBNull(3) ? null : r.GetString(3)
                        });
                    }
                }
            }
            return list;
        }

        public int Insert(ThanhVienDTO t)
            => DbHelper.ExecuteNonQuery(
                "INSERT INTO ThanhVien (HoTen, Email, ChuyenMon) VALUES (@Ten, @Email, @CM)",
                DbHelper.P("@Ten", t.HoTen), DbHelper.P("@Email", t.Email), DbHelper.P("@CM", t.ChuyenMon));

        public int Update(ThanhVienDTO t)
            => DbHelper.ExecuteNonQuery(
                "UPDATE ThanhVien SET HoTen=@Ten, Email=@Email, ChuyenMon=@CM WHERE MaThanhVien=@Ma",
                DbHelper.P("@Ten", t.HoTen), DbHelper.P("@Email", t.Email),
                DbHelper.P("@CM", t.ChuyenMon), DbHelper.P("@Ma", t.MaThanhVien));

        public int Delete(int ma)
        {
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        using (var c1 = new SqlCommand("DELETE FROM PhanCong WHERE MaThanhVien=@Ma", conn, trans))
                        { c1.Parameters.AddWithValue("@Ma", ma); c1.ExecuteNonQuery(); }
                        int n;
                        using (var c2 = new SqlCommand("DELETE FROM ThanhVien WHERE MaThanhVien=@Ma", conn, trans))
                        { c2.Parameters.AddWithValue("@Ma", ma); n = c2.ExecuteNonQuery(); }
                        trans.Commit();
                        return n;
                    }
                    catch { trans.Rollback(); throw; }
                }
            }
        }
    }
}
