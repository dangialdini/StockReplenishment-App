using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockReplenishment.SqlDAL {
    public partial class StockReplenishmentEntities {

        #region testing

        DbContextTransaction transaction = null;

        public bool IsTesting { get { return transaction != null; } }

        public void StartTest() {
            transaction = Database.BeginTransaction();
            // To view table contents of uncommitted transactions, use SSMS:
            //  set transaction isolation level read uncommitted;
        }

        public void EndTest() {
            transaction.Rollback();
            transaction.Dispose();
        }
        #endregion

        #region db Transaction

        private DbContextTransaction trans = null;

        public void BeginTransaction() {
            trans = Database.BeginTransaction();
        }

        public void CommitTransaction() {
            if(trans != null) trans.Commit();
            trans = null;
        }

        public void RollbackTransaction() {
            trans.Rollback();
        }

        #endregion

        #region ErplyTemp table

        public void InsertOrUpdateErplyTemp(ErplyTemp et) {
            if (et.Id == 0) ErplyTemps.Add(et);
            SaveChanges();
        }

        public void CleanErplyTempTable() {
            ErplyTemps.RemoveRange(ErplyTemps);
            SaveChanges();
        }

        #endregion

        #region FileTransferQueue table

        public FileTransferQueue AddFileTransferQueue(string fileName) {
            var queueFile = new FileTransferQueue {
                CreatedDateTime = DateTimeOffset.Now,
                FileName = fileName
            };
            FileTransferQueues.Add(queueFile);
            SaveChanges();
            return queueFile;
        }

        public IQueryable<FileTransferQueue> FindTransferQueues() {
            return FileTransferQueues.OrderBy(ftq => ftq.CreatedDateTime);
        }

        public void DeleteFileTransferQueue(FileTransferQueue file) {
            FileTransferQueues.Remove(file);
            SaveChanges();
        }

        #endregion

        #region Log table

        public void CleanLog(DateTimeOffset dt) {
            Logs.RemoveRange(Logs.Where(l => l.LogDateTime < dt));
            SaveChanges();
        }

        public void InsertOrUpdateLog(Log log) {
            if (log.Id == 0) Logs.Add(log);
            SaveChanges();
        }

        #endregion

        #region Product table

        public IQueryable<Product> FindProducts() {
            return Products.OrderBy(p => p.Id).Include(i => i.ProductRanges);
        }

        public Product FindProduct(int id) {
            return Products.Where(p => p.Id == id)
                           .SingleOrDefault();
        }

        public Product FindProduct(string productCode) {
            return Products.Where(p => p.ProductCode == productCode)
                           .FirstOrDefault();
        }

        public void InsertOrUpdateProduct(Product prod, bool bAdd) {
            if (bAdd) Products.Add(prod);
            SaveChanges();
        }

        #endregion

        #region ProductRange table

        public ProductRange FindProductRange(int id) {
            return ProductRanges.Where(pr => pr.Id == id)
                                .SingleOrDefault();
        }

        public void InsertOrUpdateProductRange(ProductRange pr) {
            ProductRanges.Add(pr);
            SaveChanges();
        }

        public void DeleteProductRange(int id) {
            ProductRanges.RemoveRange(ProductRanges.Where(pr => pr.Id == id));
        }

        #endregion

        #region ProductsSold table

        public void InsertOrUpdateProductSold(ProductsSold ps) {
            if (ps.Id == 0) ProductsSolds.Add(ps);
            SaveChanges();
        }
        public IQueryable<ProductsSold> FindProductsSold(int unixDate) {
            return ProductsSolds.Where(ps => ps.UnixDateStamp >= unixDate)
                                .OrderBy(ps => ps.StoreId);
        }

        public ProductsSold RefreshEntity(ProductsSold ps) {
            Entry(ps).State = EntityState.Detached;
            return ProductsSolds.Where(psold => psold.Id == ps.Id)
                                .SingleOrDefault();
        }

        #endregion

        #region Settings table

        public int GetLastOrderExtractTime() {
            var settings = Settings.FirstOrDefault();
            if (settings != null && settings.LastErplyOrderExtractTimeUnix != null) {
                return settings.LastErplyOrderExtractTimeUnix.Value;
            } else {
                return (int)DateTimeOffset.Now.ToUnixTimeSeconds();
            }
        }

        public void UpdateLastOrderExtractTime(int lastRun) {
            var settings = Settings.FirstOrDefault();
            if (settings == null) settings = new Setting();

            settings.LastErplyOrderExtractTime = DateTimeOffset.FromUnixTimeSeconds(lastRun);
            settings.LastErplyOrderExtractTimeUnix = lastRun;

            if (settings.Id == 0) Settings.Add(settings);
            SaveChanges();
        }

        public int GetLastOrderCreationTime() {
            DateTimeOffset dt;
            var settings = Settings.FirstOrDefault();
            if (settings != null && settings.LastOrderCreationTime != null) {
                dt = settings.LastOrderCreationTime.Value;
            } else {
                dt = DateTimeOffset.Now;
            }

            return (int)dt.ToUnixTimeSeconds();
        }

        public void UpdateLastOrderCreationTime(DateTimeOffset lastRun) {
            var settings = Settings.FirstOrDefault();
            if (settings == null) settings = new Setting();

            settings.LastOrderCreationTime = lastRun;
            settings.LastOrderCreationTimeUnix = (int)lastRun.ToUnixTimeSeconds();

            if (settings.Id == 0) Settings.Add(settings);
            SaveChanges();
        }

        public int GetNextOrderNumber() {
            int nextOrderNo = 1;
            var settings = Settings.FirstOrDefault();
            if (settings == null) {
                settings = new Setting { Id = 0 };
            } else {
                nextOrderNo = settings.NextOrderNo ?? 1;
            }
            settings.NextOrderNo = nextOrderNo + 1;
            if (settings.Id == 0) Settings.Add(settings);
            SaveChanges();

            return nextOrderNo;
        }

        #endregion

        #region Range table

        public IQueryable<Range> FindRanges() {
            return Ranges.OrderBy(pr => pr.Id);
        }

        public Range FindRange(int id) {
            return Ranges.Where(pr => pr.Id == id)
                                .FirstOrDefault();
        }

        public void InsertOrUpdateRange(Range pr) {
            var temp = FindRange(pr.Id);
            if (temp == null) Ranges.Add(pr);
            SaveChanges();
        }

        public void DeleteRange(int id) {
            Ranges.RemoveRange(Ranges.Where(r => r.Id == id));
            SaveChanges();
        }

        #endregion

        #region Store table

        public IQueryable<Store> FindStores(bool activeOnly = false) {
            return Stores.Where(s => activeOnly == false || s.IsActive == true)
                         .OrderBy(s => s.Priority);
        }

        public Store FindStore(int id) {
            return Stores.Where(s => s.Id == id)
                         .FirstOrDefault();
        }

        public Store FindStore(string storeName) {
            return Stores.Where(s => s.Name == storeName)
                         .FirstOrDefault();
        }

        public void InsertOrUpdateStore(Store store) {
            var temp = FindStore(store.Id);
            if (temp == null) Stores.Add(store);
            SaveChanges();
        }

        #endregion

        #region StoreStock table

        public StoreStock FindStoreStock(int storeId, int productId) {
            return StoreStocks.Where(ss => ss.StoreId == storeId &&
                                           ss.ProductId == productId)
                              .FirstOrDefault();
        }

        public void InsertOrUpdateStoreStock(StoreStock storeStock) {
            if (storeStock.Id == 0) StoreStocks.Add(storeStock);
            SaveChanges();
        }

        #endregion
    }
}
