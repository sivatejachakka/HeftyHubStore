using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeftyHub.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICategoryRepository _CategoryRepository { get; }
        IProductRepository _ProductRepository { get; }
        IProductImageRepository _ProductImageRepository { get; }
        ICompanyRepository _CompanyRepository { get; }
        IShoppingCartRepository _ShoppingCartRepository { get; }
        IApplicationUserRepository _ApplicationUserRepository { get; }
        IOrderDetailRepository _OrderDetailRepository { get; }
        IOrderHeaderRepository _OrderHeaderRepository { get; }

        void Save();
    }
}
