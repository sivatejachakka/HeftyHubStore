using HeftyHub.DataAccess.Data;
using HeftyHub.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeftyHub.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        public ICategoryRepository _CategoryRepository { get; private set; }
        public IProductRepository _ProductRepository { get; private set; }
        public IProductImageRepository _ProductImageRepository { get; private set; }
        public ICompanyRepository _CompanyRepository { get; private set; }
        public IShoppingCartRepository _ShoppingCartRepository { get; private set; }
        public IApplicationUserRepository _ApplicationUserRepository { get; private set; }
        public IOrderHeaderRepository _OrderHeaderRepository { get; private set; }
        public IOrderDetailRepository _OrderDetailRepository { get; private set; }

        private readonly ApplicationDbContext _db;

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            _CategoryRepository = new CategoryRepository(_db);
            _ProductRepository = new ProductRepository(_db);
            _ProductImageRepository = new ProductImageRepository(_db);
            _CompanyRepository = new CompanyRepository(_db);
            _ShoppingCartRepository = new ShoppingCartRepository(_db);
            _ApplicationUserRepository = new ApplicationUserRepository(_db);
            _OrderHeaderRepository = new OrderHeaderRepository(_db);
            _OrderDetailRepository = new OrderDetailRepository(_db);
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
