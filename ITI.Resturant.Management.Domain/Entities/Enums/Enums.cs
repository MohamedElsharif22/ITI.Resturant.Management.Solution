using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.Domain.Entities.Enums
{
    public enum OrderType
    {
        DineIn,
        TakeOut,
        Delivery,
        Online
    }

    public enum OrderStatus
    {
        Pending,
        Confirmed,
        Preparing,
        Ready,
        Delivered,
        Cancelled
    }

    public enum CustomerDiscountType
    {
        None,
        Student,
        Senior
    }
}
