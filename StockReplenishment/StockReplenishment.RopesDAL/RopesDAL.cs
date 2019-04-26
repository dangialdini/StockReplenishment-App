using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockReplenishment.Models.Models;

namespace StockReplenishment.RopesDAL {
    public partial class RopesDAL {

        #region Protected members

        protected string connStr = "";

        private OleDbConnection conn = null;
        private bool isOpen { get; set; } = false;

        #endregion

        #region Construction

        public RopesDAL() {
            var accessFile = ConfigurationManager.AppSettings["RopesDatabase"].ToString();
            connStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + accessFile;
        }

        ~RopesDAL() {
            Close();
        }

        #endregion

        #region Database opening and closing

        public void Open() {
            if (!isOpen) {
                conn = new OleDbConnection(connStr);
                conn.Open();
                isOpen = true;
            }
        }

        public void Close() {
            if (isOpen && conn != null) {
                try {
                    conn.Close();
                } catch { }
            }
            conn = null;
            isOpen = false;
        }

        #endregion

        #region Query execution

        private Error executeSQL(string sql) {
            var error = new Error();
            try {
                Open();
                using (OleDbCommand cmd = new OleDbCommand(sql, conn)) {
                    cmd.ExecuteNonQuery();
                }
            } catch (Exception e) {
                error.SetError(e);
            }
            return error;
        }

        private OleDbDataReader executeQuery(string sql) {
            Open();
            OleDbCommand cmd = new OleDbCommand(sql, conn);
            return cmd.ExecuteReader();
        }

        private OleDbDataReader findRecord(string tableName, int rowId) {
            return executeQuery($"SELECT * FROM {tableName} WHERE Id={rowId}");
        }

        private OleDbDataReader findRecords(string tableName, string where) {
            if (!string.IsNullOrEmpty(where)) {
                return executeQuery($"SELECT * FROM {tableName} WHERE {where}");
            } else {
                return executeQuery($"SELECT * FROM {tableName}");
            }
        }

        #endregion
    }
}
