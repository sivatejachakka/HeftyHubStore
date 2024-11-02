using HeftyHub.DataAccess.Data;
using HeftyHub.DataAccess.Repository.IRepository;
using HeftyHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HeftyHub.DataAccess.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private readonly ApplicationDbContext _db;

        public ShoppingCartRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ShoppingCart cart)
        {
            _db.tblShoppingCart.Update(cart);
        }
    }
}
