using System;
using System.Collections.Generic;
using System.Linq;

namespace ITI.Resturant.Management.Domain.Entities.Cart_
{
    public class Cart
    {
        private readonly List<CartItem> _items = new();

        public string Id { get; private set; }
        public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();
        public decimal Total => _items.Sum(x => x.Quantity * x.Price);

        public Cart(string id)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }

        public void AddItem(int menuItemId, string name, decimal price, string imageUrl, string category, int quantity = 1)
        {
            var existingItem = _items.FirstOrDefault(x => x.MenuItemId == menuItemId);
            if (existingItem != null)
            {
                existingItem.UpdateQuantity(existingItem.Quantity + quantity);
            }
            else
            {
                _items.Add(new CartItem(menuItemId, name, price, imageUrl, category, quantity));
            }
        }

        public void UpdateItemQuantity(int menuItemId, int quantity)
        {
            var item = _items.FirstOrDefault(x => x.MenuItemId == menuItemId);
            if (item == null) return;

            if (quantity <= 0)
            {
                _items.Remove(item);
            }
            else
            {
                item.UpdateQuantity(quantity);
            }
        }

        public void Clear()
        {
            _items.Clear();
        }
    }

    public class CartItem
    {
        public int MenuItemId { get; private set; }
        public string Name { get; private set; }
        public decimal Price { get; private set; }
        public string ImageUrl { get; private set; }
        public string Category { get; private set; }
        public int Quantity { get; private set; }

        public CartItem(int menuItemId, string name, decimal price, string imageUrl, string category, int quantity = 1)
        {
            MenuItemId = menuItemId;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Price = price;
            ImageUrl = imageUrl;
            Category = category;
            UpdateQuantity(quantity);
        }

        internal void UpdateQuantity(int quantity)
        {
            if (quantity <= 0) throw new ArgumentException("Quantity must be positive", nameof(quantity));
            Quantity = quantity;
        }
    }
}