﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class StockReplenishmentEntities : DbContext
    {
        public StockReplenishmentEntities()
            : base("name=StockReplenishmentEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<ErplyTemp> ErplyTemps { get; set; }
        public virtual DbSet<FileTransferQueue> FileTransferQueues { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ProductRange> ProductRanges { get; set; }
        public virtual DbSet<ProductsSold> ProductsSolds { get; set; }
        public virtual DbSet<Store> Stores { get; set; }
        public virtual DbSet<StoreStock> StoreStocks { get; set; }
        public virtual DbSet<Setting> Settings { get; set; }
        public virtual DbSet<Range> Ranges { get; set; }
        public virtual DbSet<Log> Logs { get; set; }
    }
}
