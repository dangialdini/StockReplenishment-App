//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace StockReplenishment.SqlDAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class ProductsSold
    {
        public int Id { get; set; }
        public int StoreId { get; set; }
        public int UnixDateStamp { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public Nullable<int> Minimum { get; set; }
        public int Qty { get; set; }
        public bool DFO { get; set; }
    
        public virtual Store Store { get; set; }
    }
}
