using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace RLib.DB
{
    public class DbConnMsSql : DbConn
    {
        private System.Data.SqlClient.SqlConnection conn = null;
        private SqlTransaction trans = null;
        public DbConnMsSql(string dbconnstr)
        {
            base._dbtype = DbType.SQLSERVER;
            conn = new SqlConnection(dbconnstr);
            base._conn = conn;
        }
        public override System.Data.DataSet SqlToDataSet(string sql, List<ProcedureParameter> paras)
        {
            return WatchLog.Loger.TimeWatchSql(sql, paras, () =>
             {
                 SqlDataAdapter sqlad = new SqlDataAdapter();
                 sqlad.SelectCommand = new SqlCommand(sql, conn);
                 sqlad.SelectCommand.Transaction = trans;
                 if (paras != null)
                 {
                     foreach (var a in paras)
                     {
                         sqlad.SelectCommand.Parameters.Add(ParameterTransform(a));
                     }
                 }
                 DataSet ds = new DataSet();
                 sqlad.Fill(ds);
                 return ds;
             });
        }

        public override System.Data.DataTable SqlToDataTable(string sql, List<ProcedureParameter> paras)
        {
            return WatchLog.Loger.TimeWatchSql(sql, paras, () =>
            {
                SqlDataAdapter sqlad = new SqlDataAdapter();
                sqlad.SelectCommand = new SqlCommand(sql, conn);
                sqlad.SelectCommand.Transaction = trans;
                if (paras != null)
                {
                    foreach (var a in paras)
                    {
                        sqlad.SelectCommand.Parameters.Add(ParameterTransform(a));
                    }
                }
                DataTable tb = new DataTable();
                sqlad.UpdateBatchSize = 1000;
                sqlad.Fill(tb);
                return tb;
            });
        }


        public override int ExecuteSql(string sql, Array paras, int minutes)
        {
            List<ProcedureParameter> _paras = new List<ProcedureParameter>();
            foreach (var a in paras)
            {
                _paras.Add(a as ProcedureParameter);
            }
            return WatchLog.Loger.TimeWatchSql(sql, _paras, () =>
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Transaction = trans;
                if (paras != null)
                {
                    cmd.Parameters.AddRange(paras);
                }
                if (minutes > 0)
                    cmd.CommandTimeout = minutes * 60;
                int r = cmd.ExecuteNonQuery();
                return r;
            });
        }

        public override int ExecuteSql(string sql, List<ProcedureParameter> paras, int minutes)
        {
            return WatchLog.Loger.TimeWatchSql(sql, paras, () =>
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Transaction = trans;
                if (paras != null)
                {
                    foreach (var a in paras)
                    {
                        cmd.Parameters.Add(ParameterTransform(a));
                    }
                }
                if (minutes > 0)
                    cmd.CommandTimeout = minutes * 60;
                int r = cmd.ExecuteNonQuery();
                return r;
            });
        }

        public override object ExecuteScalar(string sql, List<ProcedureParameter> paras, int minutes)
        {
            return WatchLog.Loger.TimeWatchSql(sql, paras, () =>
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Transaction = trans;
                if (minutes > 0)
                {
                    cmd.CommandTimeout = minutes * 60;
                }
                if (paras != null)
                {
                    foreach (var a in paras)
                    {
                        cmd.Parameters.Add(ParameterTransform(a));
                    }
                }
                object r = cmd.ExecuteScalar();
                return r;
            });
        }

        /// <summary>
        /// 批量插入
        /// </summary>
        /// <param name="tb"></param>
        /// <param name="destTable"></param>
        /// <param name="tableMap">如果为空 取tb列名对应</param>
        public override void BuckCopy(DataTable tb, string destTable, Dictionary<string, string> tableMap)
        {
            SqlBulkCopy bcp = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, trans);
            if (string.IsNullOrWhiteSpace(destTable))
            {
                destTable = tb.TableName;
            }
            bcp.DestinationTableName = destTable;
            if (tableMap != null)
            {
                foreach (var a in tableMap)
                {
                    bcp.ColumnMappings.Add(a.Key, a.Value);
                }
            }
            else
            {
                foreach (DataColumn dc in tb.Columns)
                {
                    bcp.ColumnMappings.Add(dc.ColumnName, dc.ColumnName);
                }
            }
            bcp.WriteToServer(tb);
        }

        public override void BeginTransaction()
        {
            trans = conn.BeginTransaction();
        }

        public override void Commit()
        {
            trans.Commit();
        }

        public override void Rollback()
        {
            trans.Rollback();
        }

        public override int GetIdentity()
        {
            object ob = ExecuteScalar("select @@identity", null);
            return Convert.ToInt32(ob);
        }

        public override DateTime GetServerDate()
        {
            object ob = ExecuteScalar("select getdate()", null);
            return Convert.ToDateTime(ob);
        }


        /// <summary>参数类型转化</summary>
        /// <param name="Par"></param>
        /// <returns></returns>
        private SqlParameter ParameterTransform(ProcedureParameter Par)
        {
            if (Par.ParType == ProcParType.Default)
                return new SqlParameter(Par.Name, Par.Value);
            SqlParameter p = new SqlParameter();
            p.ParameterName = Par.Name;
            switch (Par.ParType)
            {
                case ProcParType.Int16:
                    p.SqlDbType = SqlDbType.SmallInt;
                    break;
                case ProcParType.Int32:
                    p.SqlDbType = SqlDbType.Int;
                    break;
                case ProcParType.Int64:
                    p.SqlDbType = SqlDbType.BigInt;
                    break;
                case ProcParType.Single:
                    p.SqlDbType = SqlDbType.Real;
                    break;
                case ProcParType.Double:
                    p.SqlDbType = SqlDbType.Float;
                    break;
                case ProcParType.Decimal:
                    p.SqlDbType = SqlDbType.Decimal;
                    break;
                case ProcParType.Char:
                    p.SqlDbType = SqlDbType.Char;
                    break;
                case ProcParType.VarChar:
                    p.SqlDbType = SqlDbType.VarChar;
                    break;
                case ProcParType.NVarchar:
                    p.SqlDbType = SqlDbType.NVarChar;
                    break;
                case ProcParType.Image:
                    p.SqlDbType = SqlDbType.Binary;
                    break;
                case ProcParType.DateTime:
                    p.SqlDbType = SqlDbType.DateTime;
                    break;
                default:
                    throw new Exception("未知类型ProcParType：" + Par.ParType.ToString());
            }
            p.Size = Par.Size;
            p.Direction = Par.Direction;
            switch (Par.Direction)
            {
                case ParameterDirection.Input:
                case ParameterDirection.InputOutput:
                    if (Par.Value == null)
                    {
                        p.Value = DBNull.Value;
                    }
                    else
                    {
                        p.Value = Par.Value;
                    }
                    break;
            }
            return p;
        }


    }
}
