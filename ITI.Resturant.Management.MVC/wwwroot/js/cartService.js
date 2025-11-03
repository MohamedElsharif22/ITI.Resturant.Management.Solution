// Cart service for managing cart state and syncing with server
const cartService = {
    // Local cache of cart data
    _cartData: null,

    // Get cart data from server, with local caching
    async getCart() {
        if (this._cartData) return this._cartData;
        
        try {
            const response = await fetch('/Cart/GetCart');
            if (!response.ok) throw new Error('Failed to fetch cart');
            
            this._cartData = await response.json();
            return this._cartData;
        } catch (error) {
            console.error('Error fetching cart:', error);
            // Fallback to empty cart
            return { items: [] };
        }
    },

    // Add item to cart
    async addToCart(item) {
        try {
            const response = await fetch('/Cart/AddToCart', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(item)
            });

            if (!response.ok) throw new Error('Failed to add item to cart');
            
            this._cartData = await response.json();
            this.updateUI();
            
            // Show success toast
            this.showAddedToast(item.name, item.quantity || 1);
        } catch (error) {
            console.error('Error adding to cart:', error);
            alert('Failed to add item to cart. Please try again.');
        }
    },

    // Update item quantity
    async updateQuantity(menuItemId, quantity) {
        try {
            const response = await fetch('/Cart/UpdateQuantity', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ menuItemId, quantity })
            });

            if (!response.ok) throw new Error('Failed to update cart');
            
            this._cartData = await response.json();
            this.updateUI();
        } catch (error) {
            console.error('Error updating cart:', error);
            alert('Failed to update cart. Please try again.');
        }
    },

    // Clear the entire cart
    async clearCart() {
        if (!confirm('Are you sure you want to clear your cart?')) return;
        
        try {
            const response = await fetch('/Cart/Clear', {
                method: 'POST'
            });

            if (!response.ok) throw new Error('Failed to clear cart');
            
            this._cartData = { items: [] };
            this.updateUI();
        } catch (error) {
            console.error('Error clearing cart:', error);
            alert('Failed to clear cart. Please try again.');
        }
    },

    // Convenience method for adding by details
    async addToCartByDetails(menuItemId, name, price, imageUrl, category, qty = 1) {
        await this.addToCart({
            menuItemId,
            name,
            price,
            imageUrl,
            category,
            quantity: qty
        });
    },

    // Update the cart UI
    async updateUI() {
        const cart = await this.getCart();
        const items = cart.items || [];
        
        // Update badge
        const total = items.reduce((sum, item) => sum + (item.quantity || 0), 0);
        const badgeEl = document.getElementById('cartCountBadge');
        if (badgeEl) badgeEl.textContent = total;

        // Update cart items
        const cartItemsEl = document.getElementById('cartItems');
        const cartTotalEl = document.getElementById('cartTotal');
        
        if (!cartItemsEl || !cartTotalEl) return;

        if (!items.length) {
            cartItemsEl.innerHTML = '<p class="text-muted">Your cart is empty.</p>';
            cartTotalEl.textContent = '$0.00';
            return;
        }

        const cartTotal = items.reduce((sum, item) => sum + (item.price * item.quantity), 0);
        cartTotalEl.textContent = cartTotal.toLocaleString(undefined, {style: 'currency', currency: 'USD'});

        cartItemsEl.innerHTML = items.map(item => `
            <div class="cart-item mb-3">
                <div class="d-flex align-items-center">
                    ${item.imageUrl ? `<img src="${item.imageUrl}" alt="${item.name}" class="cart-item-img me-2" style="width:50px;height:50px;object-fit:cover;">` : ''}
                    <div class="flex-grow-1">
                        <h6 class="mb-0">${item.name}</h6>
                        <small class="text-muted">${item.category}</small>
                    </div>
                    <div class="text-end ms-3">
                        <div class="cart-item-price">${(item.price * item.quantity).toLocaleString(undefined, {style: 'currency', currency: 'USD'})}</div>
                        <div class="cart-item-quantity">
                            <button class="btn btn-sm btn-outline-secondary" onclick="cartService.updateQuantity(${item.menuItemId}, ${item.quantity - 1})">-</button>
                            <span class="mx-2">${item.quantity}</span>
                            <button class="btn btn-sm btn-outline-secondary" onclick="cartService.updateQuantity(${item.menuItemId}, ${item.quantity + 1})">+</button>
                        </div>
                    </div>
                </div>
            </div>
        `).join('');
    },

    // Show a toast notification
    showAddedToast(name, qty) {
        const t = document.createElement('div');
        t.className = 'toast align-items-center text-bg-success border-0 show';
        t.style.position = 'fixed';
        t.style.right = '20px';
        t.style.bottom = '20px';
        t.style.zIndex = 2000;
        t.innerHTML = `<div class="d-flex"><div class="toast-body">Added ${qty} × ${name} to cart</div><button type="button" class="btn-close btn-close-white me-2 m-auto" onclick="this.closest('.toast').remove()"></button></div>`;
        document.body.appendChild(t);
        setTimeout(() => t.remove(), 2500);
    }
};

// Initialize cart UI when page loads
document.addEventListener('DOMContentLoaded', () => cartService.updateUI());