﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeftyHub.Utility
{
    public static class Constants
    {
        public const string ROLE_USER_CUSTOMER = "Customer";
        public const string ROLE_USER_COMPANY = "Company";
        public const string ROLE_USER_ADMIN = "Admin";
        public const string ROLE_USER_EMPLOYEE = "Employee";

        public const string STATUS_PENDING = "Pending";
        public const string STATUS_APPROVED = "Approved";
        public const string STATUS_INPROCESS= "Processing";
        public const string STATUS_SHIPPED = "Shipped";
        public const string STATUS_CANCELLED = "Cancelled";
        public const string STATUS_REFUNDED = "Refunded";

        public const string PAYMENT_STATUS_PENDING = "Pending";
        public const string PAYMENT_STATUS_APPROVED = "Approved";
        public const string PAYMENT_STATUS_DELAYED_PAYMENT= "ApprovedForDelayedPayment";
        public const string PAYMENT_STATUS_REJECTED = "Rejected";

        public const string SESSION_CART = "SessionShoppingCart";
    }
}
