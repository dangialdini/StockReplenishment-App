using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockReplenishment.Enumerations;

namespace StockReplenishment.Models.Models {
    public class Error {
        public ErrorIcon Icon { set; get; }
        public string FieldName { set; get; } = "";
        public string Message { set; get; } = "";
        public int Id { set; get; } = 0;    // This is only used by tests
        public string URL { set; get; } = "";
        public Object Data { set; get; } = null;

        public bool IsError { get { return Icon == ErrorIcon.Error && !string.IsNullOrEmpty(Message); } }
        public bool IsInfo { get { return Icon == ErrorIcon.Information && !string.IsNullOrEmpty(Message); } }
        public bool IsEmpty { get { return Icon == ErrorIcon.None; } }

        public void SetError(string message, string fieldName = "", string p1 = null, string p2 = null, string p3 = null, string p4 = null) {
            setError(ErrorIcon.Error, message, fieldName, p1, p2, p3, p4);
        }

        public void SetInfo(string message, string fieldName = "", string p1 = null, string p2 = null, string p3 = null, string p4 = null) {
            setError(ErrorIcon.Information, message, fieldName, p1, p2, p3, p4);
        }

        public void SetRecordError(string tableName, int rowId) {
            setError(ErrorIcon.Error, "Error: Failed to find record #%2 in table '%1'!", "", tableName, rowId.ToString());
        }

        public void SetError(Exception ex, string fieldName = "") {
            string msg = "Error: " + ex.Message;
            if (ex.InnerException != null) {
                if (ex.InnerException.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.InnerException.Message)) msg += "||" + ex.InnerException.InnerException.Message;
                if (!string.IsNullOrEmpty(ex.InnerException.Message)) msg += "||" + ex.InnerException.Message;
            }
            setError(ErrorIcon.Error, msg, fieldName);
        }

        private void setError(ErrorIcon icon, string message, string fieldName = "", string p1 = null, string p2 = null, string p3 = null, string p4 = null) {
            string msg = message;
            if (p1 != null) msg = msg.Replace("%1", p1);
            if (p2 != null) msg = msg.Replace("%2", p2);
            if (p3 != null) msg = msg.Replace("%3", p3);
            if (p4 != null) msg = msg.Replace("%4", p4);

            Icon = icon;
            FieldName = fieldName;
            Message = msg.Replace("\r\n", "||").Replace("\\", "&#92;");
        }
    }
}
